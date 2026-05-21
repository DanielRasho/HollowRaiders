using UnityEngine;

public class CorridorView : MonoBehaviour
{
    [Header("Room size")]
    public int width  = 1;
    public int height = 1;

    [Header("Metadata")]
    public CorridorType type;
    
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.9f, 0.4f, 0.85f);
        DrawGizmoRect(transform.position, width, height);
    }

    private void DrawGizmoRect(Vector3 origin, float w, float h)
    {
        Vector3 tl = origin;
        Vector3 tr = origin + new Vector3(w, 0);
        Vector3 br = origin + new Vector3(w, h);
        Vector3 bl = origin + new Vector3(0, h);

        Gizmos.DrawLine(tl, tr);
        Gizmos.DrawLine(tr, br);
        Gizmos.DrawLine(br, bl);
        Gizmos.DrawLine(bl, tl);
    }
}
