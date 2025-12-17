using UnityEngine;

public class PlayerPlacementManager : MonoBehaviour
{
    public static PlayerPlacementManager Instance { get; private set; }

    public bool IsPlayerPlaced => CurrentPlayer != null;
    public GameObject CurrentPlayer { get; private set; }

    public event System.Action<GameObject> OnPlayerPlaced;
    public event System.Action OnPlayerRemoved;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>플레이어 타일(프리팹 인스턴스)을 등록</summary>
    public void RegisterPlayer(GameObject playerGO)
    {
        if (playerGO == null) return;

        if (CurrentPlayer != null && CurrentPlayer != playerGO)
        {
            Destroy(CurrentPlayer);
        }

        CurrentPlayer = playerGO;

        var respawn = CurrentPlayer.GetComponent<PlayerRespawnController>();
        if (respawn) respawn.CacheSpawnPose();

        OnPlayerPlaced?.Invoke(CurrentPlayer);
    }

    /// <summary>플레이어가 삭제되었을 때</summary>
    public void UnregisterPlayer()
    {
        UnregisterPlayer(CurrentPlayer);
    }

    public void UnregisterPlayer(GameObject playerGO)
    {
        if (CurrentPlayer == null) return;
        if (playerGO != null && playerGO != CurrentPlayer) return;

        CurrentPlayer = null;
        OnPlayerRemoved?.Invoke();
    }

    public void RemoveExistingPlayerFromGridIfAny(MapGrid3D grid)
    {
        if (grid == null) return;
        if (CurrentPlayer == null) return;

        if (grid.TryGetCellOfInstance(CurrentPlayer, out var coord))
        {
            // grid.RemoveAt에서 Destroy가 일어나므로
            grid.RemoveAt(coord);
        }
        else
        {
            Destroy(CurrentPlayer);
        }

        CurrentPlayer = null;
        OnPlayerRemoved?.Invoke();
    }
}
