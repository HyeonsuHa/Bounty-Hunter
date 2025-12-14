using Unity.Cinemachine;
using UnityEngine;

public class CameraModeChanger : MonoBehaviour
{
    [Header("Cinemachine VCams")]
    [SerializeField] private CinemachineCamera vcamPlay;
    [SerializeField] private CinemachineCamera vcamBuild;

    [Header("Priority")]
    [SerializeField] private int playPriority = 20;
    [SerializeField] private int buildPriority = 20;
    [SerializeField] private int inactivePriority = 0;

    [Header("Optional: Build Camera Target")]
    [SerializeField] private Transform buildTarget; // 맵 중앙을 가리키는 타겟
    [SerializeField] private bool snapBuildCamOnEnter = true;

    private void OnEnable()
    {
        ModeManager.Instance.OnModeChanged += HandleMode;
        HandleMode(ModeManager.Instance.CurrentMode);
    }

    private void OnDisable()
    {
        if (ModeManager.Instance != null)
            ModeManager.Instance.OnModeChanged -= HandleMode;
    }

    private void HandleMode(GameMode mode)
    {
        if (mode == GameMode.Play)
        {
            SetPlay();
        }
        else
        {
            SetBuild();
        }
    }

    private void SetPlay()
    {
        if (vcamPlay) vcamPlay.Priority = playPriority;
        if (vcamBuild) vcamBuild.Priority = inactivePriority;
        Debug.Log($"[Cam] PlayPrio:{vcamPlay.Priority} BuildPrio:{vcamBuild.Priority}");
    }

    private void SetBuild()
    {
        if (vcamBuild) vcamBuild.Priority = buildPriority;
        if (vcamPlay) vcamPlay.Priority = inactivePriority;

        if (snapBuildCamOnEnter && vcamBuild && buildTarget)
        {
            // Follow/LookAt을 buildTarget으로 강제 스냅(세팅 실수 방지용)
            vcamBuild.Follow = buildTarget;
            vcamBuild.LookAt = buildTarget; // 필요 없으면 null로 둬도 됨
        }
        Debug.Log($"[Cam] PlayPrio:{vcamPlay.Priority} BuildPrio:{vcamBuild.Priority}");
    }
}
