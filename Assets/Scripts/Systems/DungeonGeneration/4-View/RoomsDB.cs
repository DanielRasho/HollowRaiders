using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomsDB", menuName = "Scriptable Objects/Rooms Database" )]
public class RoomsDB : ScriptableObject
{
    public List<RoomView> entries = new();

    private Dictionary<
        (int width, int height, RoomType type),
        List<RoomView>
    > index = new();

    private void OnEnable()
    {
        BuildIndex();
    }

    private void OnValidate()
    {
        BuildIndex();
    }

    private void BuildIndex()
    {
        index.Clear();

        foreach (RoomView room in entries)
        {
            var key = (
                room.width,
                room.height,
                room.type
            );

            if (!index.TryGetValue(key, out var list))
            {
                list = new List<RoomView>();

                index[key] = list;
            }

            list.Add(room);
        }
    }

    public RoomView FindMatch(
        int width,
        int height,
        RoomType roomType = RoomType.ANY)
    {
        List<RoomView> matches = new();

        // Exact type matches
        var exactKey = (
            width,
            height,
            roomType
        );

        if (index.TryGetValue(exactKey, out var exact))
        {
            matches.AddRange(exact);
        }

        // Generic ANY matches
        var anyKey = (
            width,
            height,
            RoomType.ANY
        );

        if (index.TryGetValue(anyKey, out var any))
        {
            matches.AddRange(any);
        }

        if (matches.Count == 0)
        {
            Debug.LogWarning(
                $"No room found for {width}x{height} type {roomType}"
            );

            return null;
        }

        return matches[
            Random.Range(0, matches.Count)
        ];
    }
}