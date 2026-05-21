using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CorridorDB", menuName = "Scriptable Objects/Corridor Database")]
public class CorridorsDB : ScriptableObject
{
    public List<CorridorView> entries = new();

    // Finds a definition matching size AND room type.
    // A definition tagged RoomType.Any always matches regardless of the requested type.
    // Returns null and logs a warning if nothing fits.
    public CorridorView FindMatch(int width, int height, CorridorType type)
    {
        foreach (var def in entries)
        {
            if (def.width  != width)  continue;
            if (def.height != height) continue;

            if (def.type == type)
                return def;
        }

        Debug.LogWarning(
            $"[FurnitureLibrary] No furniture found for size {width}x{height} " +
            $"in corridor type '{type}'. Leaving placeholder empty.");
        return null;
    }
}
