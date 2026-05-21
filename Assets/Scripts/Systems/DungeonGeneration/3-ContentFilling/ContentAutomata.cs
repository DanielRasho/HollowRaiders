using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Automata", menuName = "Scriptable Objects/Map Content Automata" )]
public class ContentAutomata : ScriptableObject
{
    [SerializeField]
    private int seed = 12345;

    [SerializeField]
    private RoomType startState;

    [SerializeField]
    private List<AutomataState> states =
        new List<AutomataState>();

    private System.Random rng;

    private Dictionary<RoomType, AutomataState> lookup;

    public void Initialize()
    {
        rng = new System.Random(seed);

        lookup =
            new Dictionary<RoomType, AutomataState>();

        foreach (var state in states)
        {
            lookup[state.type] = state;
        }
    }

    public AutomataState GetState(RoomType type)
    {
        if (lookup == null)
            Initialize();

        return lookup[type];
    }

    public AutomataState GetNext(RoomType currentType)
    {
        if (lookup == null)
            Initialize();

        AutomataState current =
            lookup[currentType];

        if (current.connections.Count == 0)
            return current;

        float total = 0f;

        foreach (var c in current.connections)
        {
            total += c.prob;
        }

        float roll =
            (float)(rng.NextDouble() * total);

        float accum = 0f;

        foreach (var c in current.connections)
        {
            accum += c.prob;

            if (roll <= accum)
            {
                return lookup[c.to];
            }
        }

        return current;
    }

    public AutomataState GetStartState()
    {
        return GetState(startState);
    }
}

[Serializable]
public class AutomataState
{
    public RoomType type;

    public List<AutomataConnection> connections =
        new List<AutomataConnection>();
}

[Serializable]
public class AutomataConnection
{
    public RoomType to;

    [Range(0f, 1f)]
    public float prob = 1f;
}