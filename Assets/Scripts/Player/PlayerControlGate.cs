using UnityEngine;

public class PlayerControlGate : MonoBehaviour
{
    [Header("Optional (auto-find if empty)")]
    [SerializeField] private MonoBehaviour[] controlScripts; // PlayerInputDriver, MouseLookController 등
    [SerializeField] private Rigidbody rb;

    [Header("Physics Lock in Edit")]
    [SerializeField] private bool lockPhysicsInEdit = true;

    private SimpleMovementSystem _moveSystem;

    private void Awake()
    {
        if (!rb) rb = GetComponentInChildren<Rigidbody>(); // 에셋 구조 복잡해도 찾기
        _moveSystem = GetComponentInChildren<SimpleMovementSystem>();

        // 인스펙터에서 비워두면 자동으로 찾아서 넣기
        if (controlScripts == null || controlScripts.Length == 0)
        {
            controlScripts = new MonoBehaviour[]
            {
                GetComponentInChildren<PlayerInputDriver>(),
                GetComponentInChildren<MouseLookController>(),
            };
        }
    }

    private void OnEnable()
    {
        if (ModeManager.Instance == null)
        {
            Debug.LogWarning("[PlayerControlGate] ModeManager.Instance is null.");
            return;
        }

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
        bool isPlay = (mode == GameMode.Play);

        // 1) 입력/카메라 회전 토글
        if (controlScripts != null)
        {
            foreach (var s in controlScripts)
            {
                if (s) s.enabled = isPlay;
            }
        }

        // 2) 이동 입력 잔상 제거
        if (!isPlay && _moveSystem != null)
            _moveSystem.SetDirection(Vector2.zero);

        // 3) 물리 고정
        if (lockPhysicsInEdit && rb != null)
        {
            if (!isPlay)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
            else
            {
                rb.isKinematic = false;
            }
        }
    }
}
