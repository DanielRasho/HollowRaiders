using UnityEngine;
using UnityEngine.Serialization;

public enum EdgeType_Legacy {
    Critical, 
    Secondary, 
    Inactive
}

[System.Serializable]
public class EdgeStyle_Legacy
{
    public Color color = Color.white;
    public float width = 0.1f;
}

public class Edge_Legacy : MonoBehaviour
{
    private Room_Legacy A;
    private Room_Legacy B;

    [FormerlySerializedAs("type")] [SerializeField] private EdgeType_Legacy typeLegacy = EdgeType_Legacy.Critical;
    [SerializeField] private float lineWidth = 0.1f;

    private LineRenderer lr;

    public void Init(Room_Legacy a, Room_Legacy b, EdgeType_Legacy edgeTypeLegacy = EdgeType_Legacy.Critical)
    {
        A = a;
        B = b;
        typeLegacy = edgeTypeLegacy;

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

        switch (typeLegacy)
        {
            case EdgeType_Legacy.Critical:
                color = Color.green;
                break;

            case EdgeType_Legacy.Secondary:
                color = Color.yellow;
                break;

            case EdgeType_Legacy.Inactive:
                color = Color.gray;
                break;

            default:
                color = Color.white;
                break;
        }

        lr.startColor = color;
        lr.endColor = color;

        // Optional: different widths per type
        switch (typeLegacy)
        {
            case EdgeType_Legacy.Critical:
                lr.widthMultiplier = lineWidth * 1.5f;
                break;

            case EdgeType_Legacy.Secondary:
                lr.widthMultiplier = lineWidth;
                break;

            case EdgeType_Legacy.Inactive:
                lr.widthMultiplier = lineWidth * 0.5f;
                break;
        }
    }

    public Room_Legacy GetOtherEdge(Room_Legacy current)
    {
        if (current.Id == A.Id) return B;
        if (current.Id == B.Id) return A;
        return null;
    }
}