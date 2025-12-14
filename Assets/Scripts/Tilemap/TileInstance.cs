using UnityEngine;

public class TileInstance : MonoBehaviour
{
    public TileDefinition definition;
    public Vector3Int coord;   // 이 블록의 "기준" 좌표(보통 min corner)
}
