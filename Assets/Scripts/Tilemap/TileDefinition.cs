using UnityEngine;

[CreateAssetMenu(menuName = "UserMap/Tile Definition")]
public class TileDefinition : ScriptableObject
{
    public string id;
    public GameObject prefab;

    public Vector3Int size = Vector3Int.one; // 1x1x1 기본, 큰 블록도 가능
    public bool solid = true;
}
