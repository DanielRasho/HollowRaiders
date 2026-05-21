using UnityEngine;

[CreateAssetMenu(fileName = "MapConfig", menuName = "Scriptable Objects/Map Generation Config")]
public class MapConfig : ScriptableObject
{
    [Header("Tetris Texture")]
    public int width = 10;
    public int height = 10;

    public int minShapeSize = 2;
    public int maxShapeSize = 5;

    public int maxGrowthAttempts = 20;

    [Header("Dungeon Generation")]
    public int numCycles = 4;
    public int maxLandmarks = 5;
    
    [Header("Dungeon Modification")]
    public int max_active_shorcuts = 3;
    
    [Header("Blocks")]
    public Vector2Int RoomSize = Vector2Int.zero;
    public Vector2Int VerticalCorridorSize = Vector2Int.zero; // X for lenght and Y for thickniess
    public Vector2Int HorizontalCorridorSize = Vector2Int.zero; // X for lenght and Y for thickniess
}