using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class EnumEditorWindow : EditorWindow
{
    private string enumFilePath = "Assets/Scripts/Data/UpgradeNames.cs";
    private string enumName = "UpgradeNames";
    private string searchTerm = "";
    private bool autoSort = false;

    private List<string> enumValues = new List<string>();
    private Vector2 scrollPos;

    [MenuItem("Tools/Enum Editor")]
    public static void OpenWindow()
    {
        GetWindow<EnumEditorWindow>("Enum Editor");
    }

    private void OnEnable()
    {
        LoadEnumFile();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label($"Editing Enum: {enumName}", EditorStyles.boldLabel);

        enumFilePath = EditorGUILayout.TextField("Enum File Path", enumFilePath);
        enumName = EditorGUILayout.TextField("Enum Name", enumName);

        if (GUILayout.Button("Reload Enum File"))
            LoadEnumFile();

        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        searchTerm = EditorGUILayout.TextField("Search", searchTerm);
        autoSort = EditorGUILayout.ToggleLeft("Auto Sort", autoSort, GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        for (int i = 0; i < enumValues.Count; i++)
        {
            string value = enumValues[i];
            if (!string.IsNullOrEmpty(searchTerm) && !value.ToLower().Contains(searchTerm.ToLower()))
                continue;

            EditorGUILayout.BeginHorizontal();
            enumValues[i] = EditorGUILayout.TextField(value);
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                enumValues.RemoveAt(i);
                SaveEnumFile();
                return;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);
        if (GUILayout.Button("Add New Value"))
        {
            enumValues.Add("NewValue");
            SaveEnumFile();
        }

        if (GUILayout.Button("Save Enum File"))
        {
            SaveEnumFile();
        }
    }

    private void LoadEnumFile()
    {
        enumValues.Clear();
        if (!File.Exists(enumFilePath))
        {
            Debug.LogWarning("Enum file not found: " + enumFilePath);
            return;
        }

        string[] lines = File.ReadAllLines(enumFilePath);
        bool insideEnum = false;

        foreach (var line in lines)
        {
            if (line.Contains($"enum {enumName}"))
            {
                insideEnum = true;
                continue;
            }

            if (insideEnum)
            {
                if (line.Contains("}"))
                    break;

                string trimmed = line.Trim().TrimEnd(',').Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    enumValues.Add(trimmed);
            }
        }
    }

    private void SaveEnumFile()
    {
        if (autoSort)
            enumValues = enumValues.OrderBy(v => v).ToList();

        List<string> lines = new List<string>
        {
            $"public enum {enumName}",
            "{"
        };

        foreach (var val in enumValues)
        {
            lines.Add($"    {val},");
        }

        lines.Add("}");

        File.WriteAllLines(enumFilePath, lines);
        AssetDatabase.Refresh();
        Debug.Log($"Saved and refreshed enum: {enumName}");
    }
}
