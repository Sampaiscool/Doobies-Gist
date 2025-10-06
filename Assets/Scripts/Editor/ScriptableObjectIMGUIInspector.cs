using UnityEngine;
using UnityEditor;

// This tells Unity this editor is for all ScriptableObjects
[CustomEditor(typeof(ScriptableObject), true)]
[CanEditMultipleObjects]
public class ScriptableObjectIMGUIInspector : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector using IMGUI
        DrawDefaultInspector();
    }
}
