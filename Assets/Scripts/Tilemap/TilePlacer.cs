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
    [SerializeField] private float holdToDragSeconds = 0.18f;
    [SerializeField] private bool requireAllCellsFree = false;



    private GameObject _ghost;

    private bool _holdingPlace;
    private bool _dragPlace;
    private float _placeHoldTimer;
    private Vector3Int _placeStartCell;
    private Vector3Int _placeCurrentCell;

    private bool _holdingDelete;
    private bool _dragDelete;
    private float _deleteHoldTimer;
    private Vector3Int _deleteStartCell;
    private Vector3Int _deleteCurrentCell;

    private readonly List<GameObject> _areaGhostPool = new();

    private int _rotIndex;
    private Quaternion CurrentRotation => Quaternion.Euler(0f, _rotIndex * 90f, 0f);

    private void Awake()
    {
        if (!grid) grid = FindFirstObjectByType<MapGrid3D>();
        if (!rayCamera) rayCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (ModeManager.Instance != null)
        {
            ModeManager.Instance.OnModeChanged += OnModeChanged;
            OnModeChanged(ModeManager.Instance.CurrentMode);
        }
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
        if (ModeManager.Instance == null) return;
        if (ModeManager.Instance.CurrentMode != GameMode.Edit) return;
        if (!grid || !rayCamera) return;

        HandleHeightInput();

        var input = ModeManager.Instance.Input.EditMode;

        HandleRotateInput(input); 

        bool hasCell = TryGetXZCell(out var cellXZ);
        Vector3Int hoverCell = hasCell
            ? new Vector3Int(cellXZ.x, currentY, cellXZ.z)
            : default;

        if (!hasCell && !_holdingPlace && !_holdingDelete && !_dragPlace && !_dragDelete)
        {
            HideAllGhosts();
            ResetDragStates();
            return;
        }

        if (_holdingPlace || _dragPlace)
            HandlePlaceDrag(input, hasCell, hoverCell);

        if (_holdingDelete || _dragDelete)
            HandleDeleteDrag(input, hasCell, hoverCell);

        if (!_holdingPlace && !_holdingDelete && !_dragPlace && !_dragDelete)
        {
            if (!hasCell)
            {
                HideAllGhosts();
                ResetDragStates();
                return;
            }

            HandlePlaceDrag(input, hasCell, hoverCell);
            HandleDeleteDrag(input, hasCell, hoverCell);

            if (!_holdingPlace && !_holdingDelete && !_dragPlace && !_dragDelete)
            {
                HideAreaGhosts();
                UpdateSingleGhost(hoverCell);
            }
        }
    }

    private void HandleRotateInput(InputSystem_Actions.EditModeActions input)
    {
        if (input.RotateLeft.WasPressedThisFrame())
            _rotIndex = (_rotIndex + 3) % 4;

        if (input.RotateRight.WasPressedThisFrame())
            _rotIndex = (_rotIndex + 1) % 4;

        if (_ghost)
            _ghost.transform.rotation = CurrentRotation;

        for (int i = 0; i < _areaGhostPool.Count; i++)
            if (_areaGhostPool[i])
                _areaGhostPool[i].transform.rotation = CurrentRotation;
    }
    private void HandlePlaceDrag(InputSystem_Actions.EditModeActions input, bool hasCell, Vector3Int hoverCell)
    {
        bool canArea = selectedTile && selectedTile.size == Vector3Int.one;

        if (input.Place.WasPressedThisFrame())
        {
            if (!hasCell) return;

            _holdingPlace = true;
            _dragPlace = false;
            _placeHoldTimer = 0f;
            _placeStartCell = hoverCell;
            _placeCurrentCell = hoverCell;
        }

        if (_holdingPlace && input.Place.IsPressed())
        {
            _placeHoldTimer += Time.deltaTime;

            if (hasCell)
                _placeCurrentCell = hoverCell;

            if (canArea && !_dragPlace && _placeHoldTimer >= holdToDragSeconds)
                _dragPlace = true;

            if (_dragPlace)
            {
                HideSingleGhost();
                UpdateAreaGhostRect(_placeStartCell, _placeCurrentCell);
            }
            else
            {
                HideAreaGhosts();
                if (hasCell) UpdateSingleGhost(_placeCurrentCell);
            }
        }

        if (_holdingPlace && input.Place.WasReleasedThisFrame())
        {
            if (_dragPlace)
            {
                PlaceRect(_placeStartCell, _placeCurrentCell);
            }
            else
            {
                TryPlace(_placeCurrentCell);
            }

            _holdingPlace = false;
            _dragPlace = false;
            _placeHoldTimer = 0f;

            HideAreaGhosts();
        }
    }

    private void HandleDeleteDrag(InputSystem_Actions.EditModeActions input, bool hasCell, Vector3Int hoverCell)
    {
        if (input.Delete.WasPressedThisFrame())
        {
            if (!hasCell) return;

            _holdingDelete = true;
            _dragDelete = false;
            _deleteHoldTimer = 0f;
            _deleteStartCell = hoverCell;
            _deleteCurrentCell = hoverCell;
        }

        if (_holdingDelete && input.Delete.IsPressed())
        {
            _deleteHoldTimer += Time.deltaTime;

            if (hasCell)
                _deleteCurrentCell = hoverCell;

            if (!_dragDelete && _deleteHoldTimer >= holdToDragSeconds)
                _dragDelete = true;

            if (_dragDelete)
            {
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
                RemoveAtWithPlayerCheck(_deleteCurrentCell);
            }

            _holdingDelete = false;
            _dragDelete = false;
            _deleteHoldTimer = 0f;

            HideAreaGhosts();
        }
    }

    private void HandleHeightInput()
    {
        if (ModeManager.Instance == null) return;
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

        if (selectedTile.isPlayerTile)
        {
            if (PlayerPlacementManager.Instance != null &&
                !PlayerPlacementManager.Instance.CanPlacePlayer())
                return;
        }

        var inst = grid.Place(selectedTile, coord, CurrentRotation);

        if (selectedTile.isPlayerTile && inst != null && PlayerPlacementManager.Instance != null)
        {
            PlayerPlacementManager.Instance.RegisterPlayer(inst);
        }
    }

    private void PlaceRect(Vector3Int a, Vector3Int b)
    {
        if (!selectedTile || !selectedTile.prefab) return;

        if (selectedTile.size != Vector3Int.one)
        {
            TryPlace(b);
            return;
        }

        var cells = GetRectCellsXZ(a, b, currentY);

        if (requireAllCellsFree)
        {
            foreach (var c in cells)
                if (!grid.CanPlace(selectedTile, c)) return;
        }

        foreach (var c in cells)
        {
            if (grid.CanPlace(selectedTile, c))
                TryPlace(c);
        }
    }

    private void DeleteRect(Vector3Int a, Vector3Int b)
    {
        var cells = GetRectCellsXZ(a, b, currentY);

        foreach (var c in cells)
            RemoveAtWithPlayerCheck(c);
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
    private void RemoveAtWithPlayerCheck(Vector3Int coord)
    {
        if (grid == null) return;

        if (!grid.TryGetTile(coord, out var inst) || inst == null)
            return;

        bool wasPlayer = inst.definition != null && inst.definition.isPlayerTile;

        bool removed = grid.RemoveAt(coord);
        if (!removed) return;

        if (wasPlayer && PlayerPlacementManager.Instance != null)
            PlayerPlacementManager.Instance.UnregisterPlayer();
    }
    private void EnsureSingleGhost()
    {
        if (_ghost != null) return;
        if (!selectedTile || !selectedTile.prefab) return;

        _ghost = Instantiate(selectedTile.prefab);
        _ghost.transform.rotation = CurrentRotation;
        _ghost.name = "[Ghost] " + selectedTile.name;

        foreach (var col in _ghost.GetComponentsInChildren<Collider>())
            col.enabled = false;

        ApplyGhostMaterial(_ghost);
        _ghost.SetActive(showGhost);
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
        _ghost.transform.rotation = CurrentRotation;
    }

    private void UpdateAreaGhostRect(Vector3Int a, Vector3Int b)
    {
        if (!showGhost) { HideAreaGhosts(); return; }
        if (!selectedTile || !selectedTile.prefab) { HideAreaGhosts(); return; }
        if (selectedTile.size != Vector3Int.one) { HideAreaGhosts(); return; }

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
            g.transform.rotation = CurrentRotation;
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

        ResetDragStates();
        HideAllGhosts();

        if (_ghost)
        {
            Destroy(_ghost);
            _ghost = null;
        }

        for (int i = 0; i < _areaGhostPool.Count; i++)
            if (_areaGhostPool[i]) Destroy(_areaGhostPool[i]);
        _areaGhostPool.Clear();

        EnsureSingleGhost();
    }

    public int CurrentY => currentY;
}
