using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomsDB", menuName = "Scriptable Objects/Rooms Database")]
public class RoomsDB : ScriptableObject
{
    public List<RoomView> entries = new();

    // Finds a definition matching size AND room type.
    // A definition tagged RoomType.Any always matches regardless of the requested type.
    // Returns null and logs a warning if nothing fits.
    public RoomView FindMatch(int width, int height, RoomType roomType = RoomType.ANY)
    {
        foreach (var def in entries)
        {
            if (def.width  != width)  continue;
            if (def.height != height) continue;

            if (def.type == RoomType.ANY || def.type == roomType)
                return def;
        }

        Debug.LogWarning(
            $"[FurnitureLibrary] No furniture found for size {width}x{height} " +
            $"in room type '{roomType}'. Leaving placeholder empty.");
        return null;
    }
}
