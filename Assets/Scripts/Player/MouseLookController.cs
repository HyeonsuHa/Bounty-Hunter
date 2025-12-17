using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLookController : MonoBehaviour
{
    [Header("Target (CameraOffset 넣기)")]
    [SerializeField] private Transform cameraTarget;

    [Header("Sensitivity")]
    [SerializeField] private float mouseSensitivity = 0.1f;

    [Header("Pitch Clamp (X 회전 제한)")]
    [SerializeField] private float minPitch = -40f;
    [SerializeField] private float maxPitch = 70f;

    private InputSystem_Actions _input;
    private float _pitchX; // 위/아래 (X 회전)
    private float _yawY;   // 좌/우 (Y 회전)

    private void Awake()
    {
        _input = new InputSystem_Actions();

        if (cameraTarget != null)
        {
            Vector3 e = cameraTarget.localEulerAngles;

            // X 초기화 (Pitch)
            _pitchX = (e.x > 180f) ? e.x - 360f : e.x;

            // Y 초기화 (Yaw) ← 너가 말한 “Y 버전으로 초기화” 여기!
            _yawY = (e.y > 180f) ? e.y - 360f : e.y;
        }
    }

    private void OnEnable()
    {
        _input.Player.Enable();
    }

    private void OnDisable()
    {
        _input.Player.Disable();
    }

    private void LateUpdate()
    {
        if (cameraTarget == null) return;

        Vector2 look = _input.Player.Look.ReadValue<Vector2>();

        // 마우스 위/아래 → X rotation
        _pitchX -= look.y * mouseSensitivity;
        _pitchX = Mathf.Clamp(_pitchX, minPitch, maxPitch);

        // 마우스 좌/우 → Y rotation
        _yawY += look.x * mouseSensitivity;

        // 최종 적용 (Z=0 고정)
        cameraTarget.localRotation = Quaternion.Euler(_pitchX, _yawY, 0f);
    }

    public void SetTarget(Transform t)
    {
        cameraTarget = t;
        if (cameraTarget != null)
        {
            Vector3 e = cameraTarget.localEulerAngles;
            _pitchX = (e.x > 180f) ? e.x - 360f : e.x;
            _yawY = (e.y > 180f) ? e.y - 360f : e.y;
        }
    }
}
