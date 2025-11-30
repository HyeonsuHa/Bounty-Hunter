using UnityEngine;

public class GridFloorGenerator : MonoBehaviour
{
    [Header("Tile Settings")]
    public GameObject tilePrefab;   // FloorTile 프리팹
    public int sizeX = 10;          // X방향 칸 수
    public int sizeZ = 10;          // Z방향 칸 수
    public float cellSize = 1f;     // 칸 간격 (타일 크기)

    [Header("Generate Options")]
    public bool generateOnStart = false;

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateGrid();
        }
    }

    // 에디터에서 우클릭 메뉴로도 실행하기 쉽게
    [ContextMenu("Generate Grid")]
    public void GenerateGrid()
    {
        if (tilePrefab == null)
        {
            Debug.LogWarning("GridFloorGenerator: tilePrefab is not assigned.");
            return;
        }

        // 기존 자식들 제거 (다시 생성할 때 깔끔하게)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // 그리드 가운데가 (0,0,0)이 되도록 오프셋 계산
        float offsetX = -(sizeX * cellSize) * 0.5f + cellSize * 0.5f;
        float offsetZ = -(sizeZ * cellSize) * 0.5f + cellSize * 0.5f;

        for (int x = 0; x < sizeX; x++)
        {
            for (int z = 0; z < sizeZ; z++)
            {
                Vector3 pos = new Vector3(
                    offsetX + x * cellSize,
                    0f,
                    offsetZ + z * cellSize
                );

                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                tile.name = $"Tile_{x}_{z}";
            }
        }

        Debug.Log($"Generated grid {sizeX} x {sizeZ}");
    }
}
