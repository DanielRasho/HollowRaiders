using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomView))]
public class RoomViewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // draws your serialized fields normally

        RoomView room = (RoomView)target;

        if (GUILayout.Button("Populate"))
        {
            Undo.RecordObject(room, "Populate Room"); // makes it ctrl+Z-able
            room.Populate();
            EditorUtility.SetDirty(room); // marks scene as changed
        }
        
        if (GUILayout.Button("Clear"))
        {
            Undo.RecordObject(room, "Clear Room"); // makes it ctrl+Z-able
            room.Clear();
            EditorUtility.SetDirty(room); // marks scene as changed
        }
        
        if (GUILayout.Button("Place Forniture"))
        {
            Undo.RecordObject(room, "Place Forniture"); // makes it ctrl+Z-able
            room.PlaceForniture();
            EditorUtility.SetDirty(room); // marks scene as changed
        }
        
        if (GUILayout.Button("Place Enemies"))
        {
            Undo.RecordObject(room, "Place Enemies"); // makes it ctrl+Z-able
            room.FillWithEnemies();
            EditorUtility.SetDirty(room); // marks scene as changed
        }
    }
}
