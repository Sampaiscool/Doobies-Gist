using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(Enum), true)]
public class SearchableEnumDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Enum)
        {
            EditorGUI.LabelField(position, label.text, "Use with Enums only.");
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        Rect fieldRect = EditorGUI.PrefixLabel(position, label);
        string currentName = property.enumDisplayNames[property.enumValueIndex];

        if (EditorGUI.DropdownButton(fieldRect, new GUIContent(currentName), FocusType.Keyboard))
        {
            SearchablePopup.Show(fieldRect, property);
        }

        EditorGUI.EndProperty();
    }
}

public class SearchablePopup : PopupWindowContent
{
    private class EnumItem
    {
        public string Name;
        public int Index;
    }

    private readonly List<EnumItem> allItems;
    private readonly SerializedProperty property;
    private readonly Action<int> onSelect;

    private string searchTerm = "";
    private Vector2 scrollPos;
    private int selectedIndex = 0;
    private bool typingMode = true;

    public static void Show(Rect rect, SerializedProperty property)
    {
        var names = property.enumDisplayNames.Select((n, i) => new EnumItem { Name = n, Index = i })
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        PopupWindow.Show(rect, new SearchablePopup(property, names));
    }

    private SearchablePopup(SerializedProperty prop, List<EnumItem> items)
    {
        property = prop;
        allItems = items;
        selectedIndex = allItems.FindIndex(e => e.Index == prop.enumValueIndex);
        onSelect = (i) =>
        {
            property.enumValueIndex = i;
            property.serializedObject.ApplyModifiedProperties();
        };
    }

    public override Vector2 GetWindowSize() => new Vector2(250, 300);

    public override void OnGUI(Rect rect)
    {
        GUILayout.Space(4);

        GUI.SetNextControlName("SearchField");
        searchTerm = EditorGUILayout.TextField(searchTerm, EditorStyles.toolbarSearchField);

        if (typingMode && Event.current.type == EventType.Repaint)
            GUI.FocusControl("SearchField");

        var filtered = allItems
            .Where(x => string.IsNullOrEmpty(searchTerm) || x.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
            .ToList();

        if (filtered.Count == 0)
            selectedIndex = -1;
        else
            selectedIndex = Mathf.Clamp(selectedIndex, 0, filtered.Count - 1);

        HandleKeyboard(filtered);

        GUILayout.Space(4);
        scrollPos = GUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < filtered.Count; i++)
        {
            var item = filtered[i];
            bool isSelected = i == selectedIndex;
            GUIStyle style = new GUIStyle(isSelected ? EditorStyles.boldLabel : EditorStyles.label);

            Rect buttonRect = GUILayoutUtility.GetRect(new GUIContent(item.Name), style);

            if (Event.current.type == EventType.Repaint && isSelected)
                EditorGUI.DrawRect(buttonRect, new Color(0.3f, 0.5f, 0.85f, 0.25f));

            if (GUI.Button(buttonRect, item.Name, style))
            {
                onSelect?.Invoke(item.Index);
                editorWindow.Close();
            }
        }

        GUILayout.EndScrollView();
    }

    private void HandleKeyboard(List<EnumItem> filtered)
    {
        Event e = Event.current;
        if (e.type != EventType.KeyDown) return;

        // Typing characters -> stay in typing mode
        if (char.IsLetterOrDigit((char)e.character) || e.keyCode == KeyCode.Backspace || e.keyCode == KeyCode.Space)
        {
            typingMode = true;
            selectedIndex = 0;
            editorWindow.Repaint();
            return;
        }

        // Arrow keys -> switch to navigation mode
        if (e.keyCode == KeyCode.UpArrow || e.keyCode == KeyCode.DownArrow ||
            e.keyCode == KeyCode.LeftArrow || e.keyCode == KeyCode.RightArrow)
        {
            typingMode = false;
            e.Use();

            switch (e.keyCode)
            {
                case KeyCode.DownArrow: selectedIndex = Mathf.Min(selectedIndex + 1, filtered.Count - 1); break;
                case KeyCode.UpArrow: selectedIndex = Mathf.Max(selectedIndex - 1, 0); break;
                case KeyCode.LeftArrow: selectedIndex = 0; break;
                case KeyCode.RightArrow: selectedIndex = filtered.Count - 1; break;
            }

            editorWindow.Repaint();
            return;
        }

        // Enter always selects highlighted item
        if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
        {
            if (selectedIndex >= 0 && selectedIndex < filtered.Count)
                onSelect(filtered[selectedIndex].Index);

            editorWindow.Close();
            e.Use();
            return;
        }

        // Escape closes popup
        if (e.keyCode == KeyCode.Escape)
        {
            editorWindow.Close();
            e.Use();
        }
    }
}
