using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbitInput : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CameraPivotController pivot; // 비워두면 자동탐색

    [Header("Orbit")]
    [SerializeField] private float yawSpeed = 180f;   // deg/sec
    [SerializeField] private float pitchSpeed = 90f;  // deg/sec
    [SerializeField] private float minPitch = 15f;
    [SerializeField] private float maxPitch = 60f;

    [Header("Options")]
    [SerializeField] private bool enablePitch = true;

    [Header("Input")]
    [SerializeField] private bool holdRightMouse = true;

    private float _yaw;
    private float _pitch;

    private void Awake()
    {
        ResolvePivot();
        SyncFromPivot();
    }

    private void OnEnable()
    {
        ResolvePivot();
        SyncFromPivot();
    }

    private void ResolvePivot()
    {
        // 1) 같은 오브젝트에 있으면 그걸 사용
        if (!pivot) pivot = GetComponent<CameraPivotController>();

        // 2) 그래도 없으면 씬에서 찾아서 연결 (CameraPivot에 붙여두면 보통 여기까지 안 옴)
        if (!pivot) pivot = FindFirstObjectByType<CameraPivotController>();
    }

    private void SyncFromPivot()
    {
        if (pivot == null) return;
        _pitch = pivot.worldEuler.x;
        _yaw = pivot.worldEuler.y;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
    }

    private void Update()
    {
        if (pivot == null) return;

        if (holdRightMouse && !(Mouse.current?.rightButton.isPressed ?? false))
            return;

        // New Input System 마우스 delta
        Vector2 delta = Mouse.current?.delta.ReadValue() ?? Vector2.zero;

        float mx = delta.x;
        float my = delta.y;

        // delta는 프레임당 픽셀 이동량이므로 Time.deltaTime을 곱하지 않는 쪽이 보통 더 자연스러움.
        // (원하면 아래 속도 값을 조절해서 감도 맞추면 됨)
        _yaw += mx * yawSpeed * 0.01f;

        if (enablePitch)
        {
            _pitch -= my * pitchSpeed * 0.01f;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }

        var e = pivot.worldEuler;
        e.y = _yaw;
        e.x = enablePitch ? _pitch : e.x;
        pivot.worldEuler = e;
    }
}
