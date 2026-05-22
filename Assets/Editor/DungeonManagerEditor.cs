using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonManager))]
public class DungeonManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // draws your serialized fields normally

        DungeonManager manager = (DungeonManager)target;
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField(
            "Dungeon Statistics",
            EditorStyles.boldLabel
        );
        EditorGUILayout.LabelField(
            "Shortcut Count",
            manager.ShortcutCount().ToString()
        );
        
        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Dungeon"))
        {
            Undo.RecordObject(manager, "Generate Dungeon"); // makes it ctrl+Z-able
            manager.ResetMap();
            manager.Generate();
            manager.ExportAsciiMap();
            manager.RenderMap();
            manager.ModifyMap();
            EditorUtility.SetDirty(manager); // marks scene as changed
        }
        
        if (GUILayout.Button("Clear"))
        {
            Undo.RecordObject(manager, "Clear"); // makes it ctrl+Z-able
            manager.ResetMap();
            EditorUtility.SetDirty(manager); // marks scene as changed
        }
        
        if (GUILayout.Button("Modify Map"))
        {
            Undo.RecordObject(manager, "Clear"); // makes it ctrl+Z-able
            manager.ModifyMap();
            EditorUtility.SetDirty(manager); // marks scene as changed
        }
    }
}