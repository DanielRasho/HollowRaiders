using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Randomness
{
    public static List<int> PickRandomIndexes(int listSize, int count)
    {
        List<int> indexes = Enumerable.Range(0, listSize).ToList();

        for (int i = 0; i < indexes.Count; i++)
        {
            int swapIndex = Random.Range(i, indexes.Count);
            (indexes[i], indexes[swapIndex]) = (indexes[swapIndex], indexes[i]);
        }

        return indexes.Take(count).ToList();
    }   
}
