using UnityEngine;

[CreateAssetMenu(menuName = "Tile/TileDefinition")]
public class TileDefinition : ScriptableObject
{
    public string displayName;
    public GameObject prefab;
    public Vector3Int size = Vector3Int.one;

    public Sprite icon;
    public bool isPlayerTile;
}