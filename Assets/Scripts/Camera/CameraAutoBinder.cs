using UnityEngine;
using Unity.Cinemachine;

public class CameraAutoBinder : MonoBehaviour
{
    [SerializeField] private CinemachineCamera playVcam;  
    [SerializeField] private MouseLookController mouseLook;

    [Header("Player pivot name (optional)")]
    [SerializeField] private string pivotName = "CameraPivot";

    private void OnEnable()
    {
        if (PlayerPlacementManager.Instance == null) return;

        PlayerPlacementManager.Instance.OnPlayerPlaced += HandlePlayerPlaced;
        PlayerPlacementManager.Instance.OnPlayerRemoved += HandlePlayerRemoved;

        if (PlayerPlacementManager.Instance.CurrentPlayer != null)
            HandlePlayerPlaced(PlayerPlacementManager.Instance.CurrentPlayer);
    }

    private void OnDisable()
    {
        if (PlayerPlacementManager.Instance == null) return;

        PlayerPlacementManager.Instance.OnPlayerPlaced -= HandlePlayerPlaced;
        PlayerPlacementManager.Instance.OnPlayerRemoved -= HandlePlayerRemoved;
    }

    private void HandlePlayerPlaced(GameObject playerGO)
    {
        if (!playerGO) return;

        var pivot = playerGO.transform.Find(pivotName);
        Transform follow = pivot ? pivot : playerGO.transform;

        if (playVcam != null)
        {
            // CinemachineCamera에는 보통 Target(또는 Follow/LookAt 역할)이 "Tracking Target"으로 존재
            // 버전에 따라 API가 약간 다를 수 있어서 가장 안전한 방식으로: Transform을 직접 넣는 필드를 노출해서 인스펙터로 연결해도 됨.
            playVcam.Target.TrackingTarget = follow;
            playVcam.Target.LookAtTarget = follow; // LookAt 안 쓰면 지워도 됨
        }

        if (mouseLook != null)
            mouseLook.SetTarget(follow);
    }

    private void HandlePlayerRemoved()
    {
        if (playVcam != null)
        {
            playVcam.Target.TrackingTarget = null;
            playVcam.Target.LookAtTarget = null;
        }

        if (mouseLook != null)
            mouseLook.SetTarget(null);
    }
}
