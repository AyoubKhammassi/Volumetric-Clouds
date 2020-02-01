using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EditorModeHandler))]
public class EditorModeHandlerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorModeHandler handler = (EditorModeHandler)target;

        if (GUILayout.Button("Add Renderer"))
            handler.AddRendererToEditorCamera();

        if (GUILayout.Button("Remove Renderer"))
            handler.RemoveRendererFromEditorCamera();

    }
}
