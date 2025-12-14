using UnityEngine;

public class PlayerPlacementManager : MonoBehaviour
{
    public static PlayerPlacementManager Instance { get; private set; }

    [Header("Enable when player placed")]
    [SerializeField] private GameObject[] enableOnPlaced;

    private TileInstance _playerTile;

    public bool IsPlayerPlaced => _playerTile != null;

    private void Awake()
    {
        Instance = this;
        SetSystemsActive(false);
    }

    public bool CanPlacePlayer()
    {
        return _playerTile == null;
    }

    public void RegisterPlayer(TileInstance tile)
    {
        if (tile == null) return;
        if (_playerTile != null) return;

        _playerTile = tile;
        SetSystemsActive(true);
    }

    public void UnregisterPlayer()
    {
        _playerTile = null;
        SetSystemsActive(false);
    }

    public bool IsThisPlayerTile(TileInstance tile)
    {
        return tile != null && tile == _playerTile;
    }

    private void SetSystemsActive(bool active)
    {
        if (enableOnPlaced == null) return;
        foreach (var go in enableOnPlaced)
            if (go) go.SetActive(active);
    }
}
