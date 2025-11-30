using UnityEngine;

public class SimpleMovementSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody _rigidbody;

    [Header("Move")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed = 10f;   // ★ 회전 속도 추가!

    // input direction from outside (x = left/right, y = forward/back)
    private Vector2 _direction = Vector2.zero;

    // world-space base forward direction (fixed at start)
    private Vector3 _baseForward;
    private Vector3 _rightDir;
    private Vector3 _leftDir;

    [Header("Turn Angles")]
    [SerializeField] private float rightAngle = 90f;
    [SerializeField] private float leftAngle = -90f;

    private const float EPS = 0.1f;

    private void Awake()
    {
        _baseForward = transform.forward;
        _baseForward.y = 0f;
        _baseForward.Normalize();

        _rightDir = Quaternion.Euler(0f, rightAngle, 0f) * _baseForward;
        _rightDir.y = 0f;
        _rightDir.Normalize();

        _leftDir = Quaternion.Euler(0f, leftAngle, 0f) * _baseForward;
        _leftDir.y = 0f;
        _leftDir.Normalize();
    }

    public void SetDirection(Vector2 dir)
    {
        _direction = dir;
    }

    public void SetSpeed(float speed)
    {
        _moveSpeed = speed;
    }

    private void FixedUpdate()
    {
        if (_direction.sqrMagnitude < 0.0001f)
            return;

        float rawX = _direction.x;
        float rawY = _direction.y;

        bool forwardPressed = rawY > EPS;
        bool backwardPressed = rawY < -EPS;
        bool rightPressed = rawX > EPS;
        bool leftPressed = rawX < -EPS;
        bool verticalPressed = forwardPressed || backwardPressed;

        Vector2 logicDir = Vector2.zero;
        if (rightPressed) logicDir.x += 1f;
        if (leftPressed) logicDir.x -= 1f;
        if (forwardPressed) logicDir.y += 1f;
        if (backwardPressed) logicDir.y -= 1f;

        Vector3 move = new Vector3(logicDir.x, 0f, logicDir.y);

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        Vector3 newPos =
            _rigidbody.position + move * (_moveSpeed * Time.fixedDeltaTime);
        _rigidbody.MovePosition(newPos);

        // ==== rotation direction ====
        Vector3 rotDir = Vector3.zero;

        if (verticalPressed && !rightPressed && !leftPressed)
            rotDir = _baseForward;
        else if (!verticalPressed && rightPressed)
            rotDir = _rightDir;
        else if (!verticalPressed && leftPressed)
            rotDir = _leftDir;
        else if (forwardPressed && rightPressed)
            rotDir = (_baseForward + _rightDir).normalized;
        else if (forwardPressed && leftPressed)
            rotDir = (_baseForward + _leftDir).normalized;
        else if (backwardPressed && rightPressed)
            rotDir = (_baseForward + _leftDir).normalized;
        else if (backwardPressed && leftPressed)
            rotDir = (_baseForward + _rightDir).normalized;
        else
            rotDir = move;

        // ==== 회전 적용 ====
        if (rotDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(rotDir);

            // ★ Inspector에서 조절 가능한 회전 속도 적용!
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                _rotationSpeed * Time.fixedDeltaTime);
        }
    }
}
