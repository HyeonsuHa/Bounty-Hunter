using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TilePlacer3D : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private MapGrid3D grid;
    [SerializeField] private Camera rayCamera;
    [SerializeField] private LayerMask groundMask;

    [Header("Tile")]
    [SerializeField] private TileDefinition selectedTile;

    [Header("Grid Settings")]
    [SerializeField] private int currentY = 0;

    [Header("Ghost (single)")]
    [SerializeField] private bool showGhost = true;
    [SerializeField] private Material ghostMaterial;

    [Header("Drag Area")]
    [SerializeField] private float holdToDragSeconds = 0.18f; // 이 시간 이상 누르면 드래그 모드
    [SerializeField] private bool requireAllCellsFree = false; // true면 하나라도 막히면 전체 취소, false면 가능한 곳만 설치

    private GameObject _ghost; // 단일 고스트

    // 드래그(배치)
    private bool _holdingPlace;
    private bool _dragPlace;
    private float _placeHoldTimer;
    private Vector3Int _placeStartCell;
    private Vector3Int _placeCurrentCell;

    // 드래그(삭제)
    private bool _holdingDelete;
    private bool _dragDelete;
    private float _deleteHoldTimer;
    private Vector3Int _deleteStartCell;
    private Vector3Int _deleteCurrentCell;

    // 영역 고스트 풀
    private readonly List<GameObject> _areaGhostPool = new();

    private void Awake()
    {
        if (!grid) grid = FindFirstObjectByType<MapGrid3D>();
        if (!rayCamera) rayCamera = Camera.main;
    }

    private void OnEnable()
    {
        ModeManager.Instance.OnModeChanged += OnModeChanged;
        OnModeChanged(ModeManager.Instance.CurrentMode);
    }

    private void OnDisable()
    {
        if (ModeManager.Instance != null)
            ModeManager.Instance.OnModeChanged -= OnModeChanged;
    }

    private void OnModeChanged(GameMode mode)
    {
        bool isEdit = (mode == GameMode.Edit);

        if (!isEdit)
        {
            HideAllGhosts();
            ResetDragStates();
        }
        else
        {
            EnsureSingleGhost();
            if (_ghost) _ghost.SetActive(showGhost);
        }
    }

    private void Update()
    {
        if (ModeManager.Instance.CurrentMode != GameMode.Edit) return;
        if (!grid || !rayCamera) return;

        HandleHeightInput();

        if (!TryGetXZCell(out var cellXZ))
        {
            HideAllGhosts();
            ResetDragStates();
            return;
        }

        Vector3Int hoverCell = new Vector3Int(cellXZ.x, currentY, cellXZ.z);
        var input = ModeManager.Instance.Input.EditMode;

        // --- Place (좌클릭) ---
        HandlePlaceDrag(input, hoverCell);

        // --- Delete (우클릭) ---
        HandleDeleteDrag(input, hoverCell);

        // 아무 드래그도 아닐 때는 기존 단일 고스트 표시
        if (!_holdingPlace && !_holdingDelete && !_dragPlace && !_dragDelete)
        {
            HideAreaGhosts();
            UpdateSingleGhost(hoverCell);
        }
    }

    private void HandlePlaceDrag(InputSystem_Actions.EditModeActions input, Vector3Int hoverCell)
    {
        // 큰 타일은 드래그 배치 비활성(규칙 복잡)
        bool canArea = selectedTile && selectedTile.size == Vector3Int.one;

        if (input.Place.WasPressedThisFrame())
        {
            _holdingPlace = true;
            _dragPlace = false;
            _placeHoldTimer = 0f;
            _placeStartCell = hoverCell;
            _placeCurrentCell = hoverCell;
        }

        if (_holdingPlace && input.Place.IsPressed())
        {
            _placeHoldTimer += Time.deltaTime;

            if (canArea && !_dragPlace && _placeHoldTimer >= holdToDragSeconds)
                _dragPlace = true;

            if (_dragPlace)
            {
                _placeCurrentCell = hoverCell;
                HideSingleGhost();
                UpdateAreaGhostRect(_placeStartCell, _placeCurrentCell);
            }
            else
            {
                // 아직 드래그 전: 단일 고스트
                HideAreaGhosts();
                UpdateSingleGhost(hoverCell);
            }
        }

        if (_holdingPlace && input.Place.WasReleasedThisFrame())
        {
            if (_dragPlace)
            {
                // 사각형 설치
                PlaceRect(_placeStartCell, _placeCurrentCell);
            }
            else
            {
                // 단일 설치
                TryPlace(hoverCell);
            }

            _holdingPlace = false;
            _dragPlace = false;
            _placeHoldTimer = 0f;

            HideAreaGhosts();
        }
    }

    private void HandleDeleteDrag(InputSystem_Actions.EditModeActions input, Vector3Int hoverCell)
    {
        if (input.Delete.WasPressedThisFrame())
        {
            _holdingDelete = true;
            _dragDelete = false;
            _deleteHoldTimer = 0f;
            _deleteStartCell = hoverCell;
            _deleteCurrentCell = hoverCell;
        }

        if (_holdingDelete && input.Delete.IsPressed())
        {
            _deleteHoldTimer += Time.deltaTime;

            if (!_dragDelete && _deleteHoldTimer >= holdToDragSeconds)
                _dragDelete = true;

            if (_dragDelete)
            {
                _deleteCurrentCell = hoverCell;
                HideSingleGhost();
                UpdateAreaGhostRect(_deleteStartCell, _deleteCurrentCell);
            }
        }

        if (_holdingDelete && input.Delete.WasReleasedThisFrame())
        {
            if (_dragDelete)
            {
                DeleteRect(_deleteStartCell, _deleteCurrentCell);
            }
            else
            {
                grid.RemoveAt(hoverCell);
            }

            _holdingDelete = false;
            _dragDelete = false;
            _deleteHoldTimer = 0f;

            HideAreaGhosts();
        }
    }

    private void HandleHeightInput()
    {
        var input = ModeManager.Instance.Input.EditMode;

        if (input.RaiseY.WasPressedThisFrame())
            currentY++;

        if (input.LowerY.WasPressedThisFrame())
            currentY = Mathf.Max(0, currentY - 1);
    }

    private bool TryGetXZCell(out Vector3Int cellXZ)
    {
        cellXZ = default;

        Vector2 mouse = Mouse.current.position.ReadValue();
        Ray ray = rayCamera.ScreenPointToRay(mouse);

        if (!Physics.Raycast(ray, out var hit, 500f, groundMask))
            return false;

        Vector3Int c = grid.WorldToCell(hit.point);
        cellXZ = new Vector3Int(c.x, 0, c.z);
        return true;
    }

    private void TryPlace(Vector3Int coord)
    {
        if (!selectedTile || !selectedTile.prefab) return;
        if (!grid.CanPlace(selectedTile, coord)) return;

        grid.Place(selectedTile, coord);
    }

    // -----------------------
    // Rect place/delete
    // -----------------------
    private void PlaceRect(Vector3Int a, Vector3Int b)
    {
        if (!selectedTile || !selectedTile.prefab) return;
        if (selectedTile.size != Vector3Int.one)
        {
            // 큰 타일은 안전하게 단일만
            TryPlace(b);
            return;
        }

        var cells = GetRectCellsXZ(a, b, currentY);

        if (requireAllCellsFree)
        {
            foreach (var c in cells)
                if (!grid.CanPlace(selectedTile, c)) return; // 전체 취소
        }

        foreach (var c in cells)
        {
            if (grid.CanPlace(selectedTile, c))
                grid.Place(selectedTile, c);
        }
    }

    private void DeleteRect(Vector3Int a, Vector3Int b)
    {
        var cells = GetRectCellsXZ(a, b, currentY);

        // 중복 Destroy 방지 위해: 시도는 그냥 좌표마다 해도 되지만
        // inst가 여러 셀을 점유할 수 있어서 결과적으로 같은 타일을 여러번 remove하려 할 수 있음.
        // MapGrid3D.RemoveAt이 이미 방어해주니 여기서는 단순 반복으로 충분.
        foreach (var c in cells)
            grid.RemoveAt(c);
    }

    private static List<Vector3Int> GetRectCellsXZ(Vector3Int a, Vector3Int b, int y)
    {
        int minX = Mathf.Min(a.x, b.x);
        int maxX = Mathf.Max(a.x, b.x);
        int minZ = Mathf.Min(a.z, b.z);
        int maxZ = Mathf.Max(a.z, b.z);

        var list = new List<Vector3Int>((maxX - minX + 1) * (maxZ - minZ + 1));
        for (int x = minX; x <= maxX; x++)
            for (int z = minZ; z <= maxZ; z++)
                list.Add(new Vector3Int(x, y, z));

        return list;
    }

    // -----------------------
    // Ghosts
    // -----------------------
    private void EnsureSingleGhost()
    {
        if (!showGhost || _ghost != null || !selectedTile || !selectedTile.prefab) return;

        _ghost = Instantiate(selectedTile.prefab);
        _ghost.name = "[Ghost] " + selectedTile.name;

        foreach (var col in _ghost.GetComponentsInChildren<Collider>())
            col.enabled = false;

        ApplyGhostMaterial(_ghost);
    }

    private void UpdateSingleGhost(Vector3Int coord)
    {
        if (!_ghost)
        {
            EnsureSingleGhost();
            if (!_ghost) return;
        }

        _ghost.SetActive(showGhost);
        _ghost.transform.position = grid.CellToWorldCenter(coord);

        bool canPlace = selectedTile && grid.CanPlace(selectedTile, coord);
        _ghost.transform.position += Vector3.up * (canPlace ? 0f : 0.2f);
    }

    private void UpdateAreaGhostRect(Vector3Int a, Vector3Int b)
    {
        if (!showGhost) { HideAreaGhosts(); return; }
        if (!selectedTile || !selectedTile.prefab) { HideAreaGhosts(); return; }

        // 드래그 고스트는 1칸 타일만 지원
        if (selectedTile.size != Vector3Int.one)
        {
            HideAreaGhosts();
            return;
        }

        var cells = GetRectCellsXZ(a, b, currentY);
        EnsureAreaGhostPool(cells.Count);

        for (int i = 0; i < _areaGhostPool.Count; i++)
        {
            bool active = i < cells.Count;
            var g = _areaGhostPool[i];
            if (!g) continue;

            g.SetActive(active);
            if (!active) continue;

            Vector3Int c = cells[i];
            g.transform.position = grid.CellToWorldCenter(c);

            bool can = grid.CanPlace(selectedTile, c);
            g.transform.position += Vector3.up * (can ? 0f : 0.2f);
        }
    }

    private void EnsureAreaGhostPool(int need)
    {
        while (_areaGhostPool.Count < need)
        {
            var g = Instantiate(selectedTile.prefab);
            g.name = "[AreaGhost] " + selectedTile.name;

            foreach (var col in g.GetComponentsInChildren<Collider>())
                col.enabled = false;

            ApplyGhostMaterial(g);
            _areaGhostPool.Add(g);
        }
    }

    private void ApplyGhostMaterial(GameObject go)
    {
        if (ghostMaterial == null) return;

        foreach (var r in go.GetComponentsInChildren<Renderer>())
            r.sharedMaterial = ghostMaterial;
    }

    private void HideSingleGhost()
    {
        if (_ghost) _ghost.SetActive(false);
    }

    private void HideAreaGhosts()
    {
        for (int i = 0; i < _areaGhostPool.Count; i++)
            if (_areaGhostPool[i]) _areaGhostPool[i].SetActive(false);
    }

    private void HideAllGhosts()
    {
        HideSingleGhost();
        HideAreaGhosts();
    }

    private void ResetDragStates()
    {
        _holdingPlace = _dragPlace = false;
        _placeHoldTimer = 0f;

        _holdingDelete = _dragDelete = false;
        _deleteHoldTimer = 0f;
    }

    public void SetSelectedTile(TileDefinition def)
    {
        selectedTile = def;

        if (_ghost)
        {
            Destroy(_ghost);
            _ghost = null;
        }

        // 영역 고스트 풀도 타일 프리팹이 바뀌면 싹 갈아끼우는 게 안전
        for (int i = 0; i < _areaGhostPool.Count; i++)
            if (_areaGhostPool[i]) Destroy(_areaGhostPool[i]);
        _areaGhostPool.Clear();

        EnsureSingleGhost();
    }

    public int CurrentY => currentY;
}
