using UnityEngine;
using Unity.Cinemachine;

public class EditorCameraZoom : MonoBehaviour
{
    [SerializeField] private CinemachineCamera vcam;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 30f;

    private CinemachinePositionComposer _composer;

    private void Awake()
    {
        if (!vcam) vcam = GetComponent<CinemachineCamera>();
        _composer = vcam.GetComponent<CinemachinePositionComposer>();
    }

    private void Update()
    {
        if (ModeManager.Instance.CurrentMode != GameMode.Edit)
            return;

        Vector2 scroll = ModeManager.Instance.Input.EditMode.Zoom.ReadValue<Vector2>();
        if (Mathf.Abs(scroll.y) < 0.01f)
            return;

        float dist = _composer.CameraDistance;
        dist -= scroll.y * zoomSpeed * Time.deltaTime;
        dist = Mathf.Clamp(dist, minDistance, maxDistance);

        _composer.CameraDistance = dist;
    }
}
