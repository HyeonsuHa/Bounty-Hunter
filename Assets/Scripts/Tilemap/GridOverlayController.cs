using UnityEngine;

public class GridOverlayController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private MapGrid3D grid;
    [SerializeField] private Renderer targetRenderer;

    [Header("Optional: Sync Y from TilePlacer3D")]
    [SerializeField] private TilePlacer3D tilePlacer;   // 넣으면 currentY 자동 동기화
    [SerializeField] private bool followTilePlacerY = true;

    [Header("Which Y level to show (if not following placer)")]
    [SerializeField] private int currentY = 0;

    [Header("Lift to avoid z-fighting")]
    [SerializeField] private float yEpsilon = 0.01f;

    private MaterialPropertyBlock _mpb;

    private static readonly int OriginID = Shader.PropertyToID("_Origin");
    private static readonly int CellSizeID = Shader.PropertyToID("_CellSize");

    // change detection
    private Vector3 _lastOrigin;
    private Vector3 _lastCell;
    private int _lastY;
    private float _lastEps;

    private void Awake()
    {
        if (!grid) grid = FindFirstObjectByType<MapGrid3D>();
        if (!targetRenderer) targetRenderer = GetComponent<Renderer>();
        if (!tilePlacer) tilePlacer = FindFirstObjectByType<TilePlacer3D>();

        _mpb = new MaterialPropertyBlock();
        CacheLast(); // 초기 캐시
    }

    private bool _isEditMode;

    private void OnEnable()
    {
        if (ModeManager.Instance != null)
        {
            ModeManager.Instance.OnModeChanged += HandleMode;
            HandleMode(ModeManager.Instance.CurrentMode);
        }
    }

    private void OnDisable()
    {
        if (ModeManager.Instance != null)
            ModeManager.Instance.OnModeChanged -= HandleMode;
    }

    private void HandleMode(GameMode mode)
    {
        _isEditMode = (mode == GameMode.Edit);

        if (targetRenderer)
            targetRenderer.enabled = _isEditMode;
    }

    private void LateUpdate()
    {
        if (!_isEditMode) return;
        if (!grid || !targetRenderer) return;

        if (followTilePlacerY && tilePlacer != null)
            currentY = tilePlacer.CurrentY;

        Vector3 origin = grid.Origin;
        Vector3 cell = grid.CellSize;

        float y = origin.y + currentY * cell.y + yEpsilon;
        var p = transform.position;
        transform.position = new Vector3(p.x, y, p.z);

        targetRenderer.GetPropertyBlock(_mpb);
        _mpb.SetVector(OriginID, origin);
        _mpb.SetVector(CellSizeID, cell);
        targetRenderer.SetPropertyBlock(_mpb);
    }


    private void Apply()
    {
        Vector3 origin = grid.Origin;
        Vector3 cell = grid.CellSize;

        float y = origin.y + currentY * cell.y + yEpsilon;
        var p = transform.position;
        transform.position = new Vector3(p.x, y, p.z);

        targetRenderer.GetPropertyBlock(_mpb);
        _mpb.SetVector(OriginID, origin);
        _mpb.SetVector(CellSizeID, cell);
        targetRenderer.SetPropertyBlock(_mpb);
    }

    private bool HasChanged()
    {
        if (_lastY != currentY) return true;
        if (!Mathf.Approximately(_lastEps, yEpsilon)) return true;
        if (_lastOrigin != grid.Origin) return true;
        if (_lastCell != grid.CellSize) return true;
        return false;
    }

    private void CacheLast()
    {
        _lastOrigin = grid ? grid.Origin : Vector3.zero;
        _lastCell = grid ? grid.CellSize : Vector3.one;
        _lastY = currentY;
        _lastEps = yEpsilon;
    }

    private void ForceRefresh()
    {
        CacheLast();
        _lastY = int.MinValue; // 강제로 변경 감지
    }

    public void SetCurrentY(int y)
    {
        currentY = y;
        ForceRefresh();
    }
}
