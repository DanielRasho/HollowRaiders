using NUnit.Framework.Constraints;
using UnityEngine;

public enum CorridorType
{
    VERTICAL = 1,
    HORIZONTAL = 2
}

public class Corridor
{
    public string Id;

    public CorridorType Type;

    public Room A;
    public Room B;
    
    public bool isFromShortcut = false;
    public bool Active = true;

    public Corridor(Room a, Room b)
    {
        if (a.Id().LessThan(b.Id()))
        {
            A = a;
            B = b;
        }
        else
        {
            A = b;
            B = a;
        }

        Id = BuildId();

        Type = Mathf.Abs(A.Coords.x - B.Coords.x) > 0
            ? CorridorType.HORIZONTAL
            : CorridorType.VERTICAL;
        
        DefineType(a, b);
    }

    private string BuildId()
    {
        return $"{A.Id().Id()}-{B.Id().Id()}";
    }

    public void Activate(bool newStatus)
    {
        Active = newStatus;
    }

    private void DefineType(Room a, Room b)
    {
        if (a.Coords.x == b.Coords.x)
        {
            Type = CorridorType.VERTICAL;
        } else
        {
            Type = CorridorType.HORIZONTAL;
        }
    }

    public Room GetBottomMost()
    {
        return A.Coords.y < B.Coords.y ? A : B;
    }

    public Room GetLeftMost()
    {
        return A.Coords.x < B.Coords.x ? A : B;
    }
    public override bool Equals(object obj)
    {
        if (obj is not Corridor other)
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
