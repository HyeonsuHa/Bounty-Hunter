using System.Collections.Generic;
using UnityEngine;

public class MapGrid3D : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private Vector3 origin = Vector3.zero;
    [SerializeField] private Vector3 cellSize = Vector3.one; // (1,1,1) 권장

    public Vector3 Origin => origin;
    public Vector3 CellSize => cellSize;


    private readonly Dictionary<Vector3Int, TileInstance> _tiles = new();

    public Vector3 CellToWorld(Vector3Int c)
    {
        return origin + Vector3.Scale((Vector3)c, cellSize);
    }

    public Vector3Int WorldToCell(Vector3 w)
    {
        Vector3 local = w - origin;
        return new Vector3Int(
            Mathf.FloorToInt(local.x / cellSize.x),
            Mathf.FloorToInt(local.y / cellSize.y),
            Mathf.FloorToInt(local.z / cellSize.z)
        );
    }

    public bool HasTile(Vector3Int c) => _tiles.ContainsKey(c);

    public bool TryGetTile(Vector3Int c, out TileInstance t) => _tiles.TryGetValue(c, out t);

    public bool CanPlace(TileDefinition def, Vector3Int baseCoord)
    {
        Vector3Int size = Vector3Int.Max(def.size, Vector3Int.one);
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                for (int z = 0; z < size.z; z++)
                {
                    var c = baseCoord + new Vector3Int(x, y, z);
                    if (_tiles.ContainsKey(c)) return false;
                }
        return true;
    }

    public TileInstance Place(TileDefinition def, Vector3Int baseCoord, Transform parent = null)
    {
        if (!CanPlace(def, baseCoord)) return null;

        Vector3 worldPos = CellToWorldCenter(baseCoord);
        var go = Instantiate(def.prefab, worldPos, Quaternion.identity, parent ? parent : transform);

        var inst = go.GetComponent<TileInstance>();
        if (!inst) inst = go.AddComponent<TileInstance>();
        inst.definition = def;
        inst.coord = baseCoord;

        Vector3Int size = Vector3Int.Max(def.size, Vector3Int.one);
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                for (int z = 0; z < size.z; z++)
                {
                    _tiles[baseCoord + new Vector3Int(x, y, z)] = inst;
                }

        return inst;
    }

    public bool RemoveAt(Vector3Int coord)
    {
        if (!_tiles.TryGetValue(coord, out var inst) || !inst) return false;

        // 같은 inst가 점유한 모든 좌표를 제거
        var keysToRemove = new List<Vector3Int>();
        foreach (var kv in _tiles)
            if (kv.Value == inst) keysToRemove.Add(kv.Key);

        foreach (var k in keysToRemove) _tiles.Remove(k);
        Destroy(inst.gameObject);
        return true;
    }

    public Vector3 CellToWorldCenter(Vector3Int cell)
    {
        Vector3 p = CellToWorld(cell);
        return p + new Vector3(cellSize.x * 0.5f, 0f, cellSize.z * 0.5f);
    }
}
