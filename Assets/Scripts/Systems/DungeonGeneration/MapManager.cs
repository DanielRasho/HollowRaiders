using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    [Header("Camera & Textures")]
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private RenderTexture mapRenderTexture;
    [SerializeField] private RawImage minimapDisplay;
    [SerializeField] private int padding = 0;

    [Header("Player Dot")]
    [SerializeField] private Transform player;
    [SerializeField] private RectTransform playerDot;

    [Header("Fog of War")]
    [SerializeField] private Texture2D brushTexture;     // soft brush PNG, Read/Write enabled
    [SerializeField] private int brushSize = 64;         // pixel radius — expose so you can tune

    [Header("Zoom & Pan")]
    [SerializeField] private float zoomSpeed = 0.5f;
    [SerializeField] private float panSpeed = 0.3f;
    [SerializeField][Range(1f, 8f)] private float minZoom = 1f;
    [SerializeField][Range(1f, 8f)] private float maxZoom = 6f;

    [Header("UI")]
    [SerializeField] private GameObject UICanvas;

    // Internal state
    private int mapWidth;
    private int mapHeight;
    private Bounds mapBounds;

    private float currentZoom = 1f;
    private Vector2 panOffset = Vector2.zero;
    private bool mapOpen = false;

    // Fog — created at runtime, no inspector asset needed
    private Texture2D fogTexture;
    private Color[] fogPixels;

    // ─── Lifecycle ───────────────────────────────────────────

    void Start()
    {
        LevelManager.OnStartMinimapGeneration += GenerateMap;
        LevelManager.OnShowMap += ShowMap;

        Input_Manager.Instance.Actions.Map.Close.performed    += CloseMap;
        Input_Manager.Instance.Actions.Map.ZoomIn.performed   += ZoomIn;
        Input_Manager.Instance.Actions.Map.ZoomOut.performed  += ZoomOut;
        Input_Manager.Instance.Actions.Map.Pan.performed      += OnPan;
    }

    void OnDestroy()
    {
        LevelManager.OnStartMinimapGeneration -= GenerateMap;
        LevelManager.OnShowMap -= ShowMap;

        Input_Manager.Instance.Actions.Map.Close.performed    -= CloseMap;
        Input_Manager.Instance.Actions.Map.ZoomIn.performed   -= ZoomIn;
        Input_Manager.Instance.Actions.Map.ZoomOut.performed  -= ZoomOut;
        Input_Manager.Instance.Actions.Map.Pan.performed      -= OnPan;

        if (fogTexture != null)
            Destroy(fogTexture);
    }

    // ─── Setup ───────────────────────────────────────────────

    void GenerateMap(Vector2 mapCenter, int width, int height)
    {

        Vector3 position = new Vector3(
            mapCenter.x,
            mapCenter.y,
            -10f
        );

        mapBounds = new Bounds(position, new Vector3(width, height, 0f));

        FrameMap();
        CreateFogTexture();
        SnapshotMap();
    }

    public void FrameMap()
    {
        float aspect = (float)minimapCamera.pixelWidth / minimapCamera.pixelHeight;

        minimapCamera.transform.position = new Vector3(
            mapBounds.center.x,
            mapBounds.center.y,
            mapBounds.center.z
        );

        float sizeFromHeight =  mapBounds.size.y * 0.5f + padding;
        float sizeFromWidth  = mapBounds.size.x / aspect * 0.5f + padding;
        minimapCamera.orthographicSize = Mathf.Max(sizeFromHeight, sizeFromWidth);
    }

    void CreateFogTexture()
    {
        // Match the render texture resolution so UVs stay aligned
        int w = mapRenderTexture.width;
        int h = mapRenderTexture.height;

        fogTexture = new Texture2D(w, h, TextureFormat.R8, false);
        fogTexture.filterMode = FilterMode.Bilinear;
        fogTexture.wrapMode   = TextureWrapMode.Clamp;

        // Start fully black — nothing revealed
        fogPixels = new Color[w * h];
        for (int i = 0; i < fogPixels.Length; i++)
            fogPixels[i] = Color.black;

        fogTexture.SetPixels(fogPixels);
        fogTexture.Apply();
    }

    void SnapshotMap()
    {
        minimapCamera.targetTexture = mapRenderTexture;
        minimapCamera.Render();
        minimapCamera.targetTexture = null;
        minimapCamera.enabled = false;
        
        mapRenderTexture.wrapMode = TextureWrapMode.Clamp;  // ← add this

        minimapDisplay.material.SetTexture("_MapTex", mapRenderTexture);
        minimapDisplay.material.SetTexture("_FogTex", fogTexture);
    }

    // ─── World → UV ──────────────────────────────────────────

    Vector2 WorldToUV(Vector3 worldPos)
    {
        return new Vector2(
            Mathf.InverseLerp(mapBounds.min.x, mapBounds.max.x, worldPos.x),
            Mathf.InverseLerp(mapBounds.min.y, mapBounds.max.y, worldPos.y)
        );
    }

    Vector2 UVToMinimapLocal(Vector2 uv)
    {
        Rect r      = minimapDisplay.rectTransform.rect;
        Rect uvRect = minimapDisplay.uvRect;

        float nx = (uv.x - uvRect.x) / uvRect.width;
        float ny = (uv.y - uvRect.y) / uvRect.height;

        return new Vector2(
            Mathf.Lerp(r.xMin, r.xMax, nx),
            Mathf.Lerp(r.yMin, r.yMax, ny)
        );
    }

    // ─── Update ──────────────────────────────────────────────

    void Update()
    {
        if (player == null || fogTexture == null) return;

        PaintFog();
        UpdateMinimapView();
        UpdatePlayerDot();
    }

    // ─── Fog painting (CPU) ───────────────────────────────────

    void PaintFog()
    {
        Vector2 playerUV = WorldToUV(player.position);

        int cx   = Mathf.RoundToInt(playerUV.x * fogTexture.width);
        int cy   = Mathf.RoundToInt(playerUV.y * fogTexture.height);
        int half = brushSize / 2;

        bool dirty = false;

        for (int py = -half; py < half; py++)
        {
            for (int px = -half; px < half; px++)
            {
                int tx = cx + px;
                int ty = cy + py;

                if (tx < 0 || tx >= fogTexture.width  ||
                    ty < 0 || ty >= fogTexture.height) continue;

                // Remap brush pixel offset to 0..1 to sample the brush texture
                float bx = (float)(px + half) / brushSize;
                float by = (float)(py + half) / brushSize;
                float brushAlpha = brushTexture.GetPixelBilinear(bx, by).a;

                int   idx     = ty * fogTexture.width + tx;
                float current = fogPixels[idx].r;
                float next    = Mathf.Max(current, brushAlpha);

                if (next > current)
                {
                    fogPixels[idx] = new Color(next, next, next, 1f);
                    dirty = true;
                }
            }
        }

        if (dirty)
        {
            fogTexture.SetPixels(fogPixels);
            fogTexture.Apply();
        }
    }

    // ─── Minimap view ─────────────────────────────────────────

    void UpdateMinimapView()
    {
        Vector2 playerUV = WorldToUV(player.position);
        float   half     = 0.5f / currentZoom;

        Vector2 center = mapOpen
            ? playerUV + panOffset
            : playerUV;

        minimapDisplay.uvRect = new Rect(
            center.x - half,
            center.y - half,
            1f / currentZoom,
            1f / currentZoom
        );
    }

    void UpdatePlayerDot()
    {
        Vector2 playerUV      = WorldToUV(player.position);
        playerDot.anchoredPosition = UVToMinimapLocal(playerUV);
    }

    // ─── Controls ────────────────────────────────────────────

    public void ShowMap()
    {
        mapOpen   = true;
        panOffset = Vector2.zero;
        UICanvas.SetActive(true);
    }

    public void CloseMap(InputAction.CallbackContext ctx)
    {
        mapOpen   = false;
        panOffset = Vector2.zero;
        UICanvas.SetActive(false);
        Input_Manager.Instance.SwitchToMap(InputMap.PLAYER);
        CursorManager.Instance.SetCursor(CursorManager.CursorType.Pointer);
    }

    // Called by the Input System scroll action (Vector2, y = scroll delta)
    public void ZoomIn(InputAction.CallbackContext ctx)
    {
        currentZoom = Mathf.Clamp(currentZoom + zoomSpeed, minZoom, maxZoom);
    }

    public void ZoomOut(InputAction.CallbackContext ctx)
    {
        currentZoom = Mathf.Clamp(currentZoom - zoomSpeed, minZoom, maxZoom);
    }

    // Pan action — bind to Mouse/Delta in your Input Actions asset
    private void OnPan(InputAction.CallbackContext ctx)
    {
        Pan(ctx.ReadValue<Vector2>());
    }

    public void Pan(Vector2 delta)
    {
        if (!mapOpen) return;

        float uvPerPixel = (1f / currentZoom) / minimapDisplay.rectTransform.rect.width;
        // Negate: dragging right moves the view left (like dragging a map)
        panOffset += delta * uvPerPixel * panSpeed;
    }
#if UNITY_EDITOR

    [ContextMenu("Debug/Save Fog Texture")]
    void SaveFogTexture()
    {
        if (fogTexture == null) { Debug.LogWarning("Fog texture not created yet — press Play first."); return; }
        byte[] bytes = fogTexture.EncodeToPNG();
        string path  = "Assets/Debug_FogTexture.png";
        System.IO.File.WriteAllBytes(path, bytes);
        AssetDatabase.Refresh();
        Debug.Log($"Fog texture saved to {path}");
    }

    [ContextMenu("Debug/Save Map RenderTexture")]
    void SaveMapTexture()
    {
        if (mapRenderTexture == null) { Debug.LogWarning("Map texture not created yet."); return; }

        // RenderTexture needs to be read back via Texture2D
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = mapRenderTexture;
        Texture2D readback = new Texture2D(mapRenderTexture.width, mapRenderTexture.height, TextureFormat.RGBA32, false);
        readback.ReadPixels(new Rect(0, 0, mapRenderTexture.width, mapRenderTexture.height), 0, 0);
        readback.Apply();
        RenderTexture.active = prev;

        byte[] bytes = readback.EncodeToPNG();
        DestroyImmediate(readback);
        string path = "Assets/Debug_MapTexture.png";
        System.IO.File.WriteAllBytes(path, bytes);
        AssetDatabase.Refresh();
        Debug.Log($"Map texture saved to {path}");
    }
#endif
}