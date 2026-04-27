using UnityEngine;

public enum EdgeType {
    Critical, 
    Secondary, 
    Inactive
}

[System.Serializable]
public class EdgeStyle
{
    public Color color = Color.white;
    public float width = 0.1f;
}

public class Edge : MonoBehaviour
{
    private Room A;
    private Room B;

    [SerializeField] private EdgeType type = EdgeType.Critical;
    [SerializeField] private float lineWidth = 0.1f;

    private LineRenderer lr;

    public void Init(Room a, Room b, EdgeType edgeType = EdgeType.Critical)
    {
        A = a;
        B = b;
        type = edgeType;

        lr = gameObject.AddComponent<LineRenderer>();

        // Basic setup
        lr.positionCount = 2;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.widthMultiplier = lineWidth;

        // Make sure it's visible in 2D
        lr.useWorldSpace = true;
        lr.sortingOrder = 10;

        ApplyStyle();
    }

    void Update()
    {
        if (A == null || B == null || lr == null) return;

        DrawEdge();
    }

    public void DrawEdge()
    {
        lr.SetPosition(0, A.GetRoomCenter());
        lr.SetPosition(1, B.GetRoomCenter());
    }

    void ApplyStyle()
    {
        Color color;

        switch (type)
        {
            case EdgeType.Critical:
                color = Color.green;
                break;

            case EdgeType.Secondary:
                color = Color.yellow;
                break;

            case EdgeType.Inactive:
                color = Color.gray;
                break;

            default:
                color = Color.white;
                break;
        }

        lr.startColor = color;
        lr.endColor = color;

        // Optional: different widths per type
        switch (type)
        {
            case EdgeType.Critical:
                lr.widthMultiplier = lineWidth * 1.5f;
                break;

            case EdgeType.Secondary:
                lr.widthMultiplier = lineWidth;
                break;

            case EdgeType.Inactive:
                lr.widthMultiplier = lineWidth * 0.5f;
                break;
        }
    }

    public Room GetOtherEdge(Room current)
    {
        if (current.Id == A.Id) return B;
        if (current.Id == B.Id) return A;
        return null;
    }
}