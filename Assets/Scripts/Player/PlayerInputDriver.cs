using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SimpleMovementSystem))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerInputDriver : MonoBehaviour
{
    public float jumpHeight = 1.5f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask;

    private InputSystem_Actions _input;
    private SimpleMovementSystem _moveSystem;
    private Rigidbody _rigidbody;
    private Animator _animator;

    private bool _isGrounded;

    private Vector2 _currentMoveInput;
    private bool _wasGrounded;

    private void Awake()
    {
        _moveSystem = GetComponent<SimpleMovementSystem>();
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        _input = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _input.Player.Enable();

        _input.Player.Move.performed += OnMove;
        _input.Player.Move.canceled += OnMove;

        _input.Player.Jump.performed += OnJump;
    }

    private void OnDisable()
    {
        _input.Player.Move.performed -= OnMove;
        _input.Player.Move.canceled -= OnMove;
        _input.Player.Jump.performed -= OnJump;

        _input.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 dir = ctx.ReadValue<Vector2>();
        _currentMoveInput = dir;

        _moveSystem.SetDirection(dir);
        _animator.SetMoveInput(dir);
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (!_isGrounded) return;

        // 제자리 점프도 가능: 입력이 0이어도 에어락 걸면 그냥 0으로 유지됨
        _moveSystem.SetAirLock(true, _currentMoveInput);

        // 점프 시작 시 수직 속도 리셋
        Vector3 vel = _rigidbody.linearVelocity;
        vel.y = 0f;
        _rigidbody.linearVelocity = vel;

        // 원하는 높이만큼 올라가기 위한 초기 속도 v = sqrt(2gh)
        float g = Mathf.Abs(Physics.gravity.y);
        float jumpVelocity = Mathf.Sqrt(2f * g * jumpHeight);

        _rigidbody.AddForce(Vector3.up * jumpVelocity, ForceMode.VelocityChange);

        _animator.DoJump();
    }

    private void Update()
    {
        // 아주 간단한 땅 체크 (발 아래 레이캐스트)
        _isGrounded = Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            groundCheckDistance + 0.1f,
            groundMask);
        if (!_wasGrounded && _isGrounded)
        {
            _moveSystem.SetAirLock(false, Vector2.zero);
        }
    }
}
