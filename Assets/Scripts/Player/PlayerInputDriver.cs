using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SimpleMovementSystem))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerInputDriver : MonoBehaviour
{
    public float jumpForce = 5f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask;

    private InputSystem_Actions _input;
    private SimpleMovementSystem _moveSystem;
    private Rigidbody _rigidbody;
    private Animator _animator;

    private bool _isGrounded;

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

        _moveSystem.SetDirection(dir);
        _animator.SetMoveInput(dir);
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        if (_isGrounded)
        {
            // 수직 속도 초기화 후 점프 힘 추가
            Vector3 vel = _rigidbody.linearVelocity;
            vel.y = 0f;
            _rigidbody.linearVelocity = vel;

            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

            // 애니메이션 트리거는 확장 메서드 이용
            _animator.DoJump();
        }
    }

    private void Update()
    {
        // 아주 간단한 땅 체크 (발 아래 레이캐스트)
        _isGrounded = Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            groundCheckDistance + 0.1f,
            groundMask);

    }
}
