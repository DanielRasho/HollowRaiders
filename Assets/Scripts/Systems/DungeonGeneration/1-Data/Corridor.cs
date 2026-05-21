using UnityEngine;

public enum CorridorType
{
    VERTICAL = 1,
    HORIZONTAL = 2
}

public class Corridor
{
    public string Id;

    [SerializeField] public CorridorType Type;

    public Room A;
    public Room B;

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
    }

    private string BuildId()
    {
        return $"f{A.Id().Id()}-{B.Id().Id()}";
    }

    public void Activate(bool newStatus)
    {
        Active = newStatus;
    }

    public void DefineType(Room a, Room b)
    {
        if (a.Coords.x == b.Coords.x)
        {
            this.Type = CorridorType.HORIZONTAL;
        } else
        {
            this.Type = CorridorType.VERTICAL;
        }
    }
}
