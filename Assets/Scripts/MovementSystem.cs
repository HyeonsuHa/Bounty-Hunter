using UnityEngine;

public class SimpleMovementSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody _rigidbody;

    [Header("Move")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed = 10f;

    [Header("Direction Reference (usually CameraPivot)")]
    [SerializeField] private Transform _directionReference;

    private Vector2 _direction = Vector2.zero;

    private Vector3 _baseForward;
    private Vector3 _baseRight;

    [Header("Turn Angles")]
    [SerializeField] private float rightAngle = 90f;
    [SerializeField] private float leftAngle = -90f;

    private const float EPS = 0.1f;

    private void Awake()
    {
        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody>();

        if (_directionReference == null)
            _directionReference = transform;
    }

    public void SetDirection(Vector2 dir)
    {
        _direction = dir;
    }

    public void SetSpeed(float speed)
    {
        _moveSpeed = speed;
    }

    private void RecalculateBasis()
    {
        Vector3 fwd = _directionReference.forward;
        fwd.y = 0f;

        if (fwd.sqrMagnitude < 0.0001f)
            fwd = Vector3.forward;

        _baseForward = fwd.normalized;

        _baseRight = Quaternion.Euler(0f, 90f, 0f) * _baseForward;
        _baseRight.y = 0f;
        _baseRight.Normalize();
    }

    private void FixedUpdate()
    {
        // camera(or pivot) might have rotated, so update every frame
        RecalculateBasis();

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

        // move relative to camera
        Vector3 move =
            _baseRight * logicDir.x +
            _baseForward * logicDir.y;

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        Vector3 newPos =
            _rigidbody.position + move * (_moveSpeed * Time.fixedDeltaTime);
        _rigidbody.MovePosition(newPos);

        // rotation dirs based on camera forward
        Vector3 rightRotDir =
            Quaternion.Euler(0f, rightAngle, 0f) * _baseForward;
        rightRotDir.y = 0f;
        rightRotDir.Normalize();

        Vector3 leftRotDir =
            Quaternion.Euler(0f, leftAngle, 0f) * _baseForward;
        leftRotDir.y = 0f;
        leftRotDir.Normalize();

        Vector3 rotDir = Vector3.zero;

        if (verticalPressed && !rightPressed && !leftPressed)
            rotDir = _baseForward;
        else if (!verticalPressed && rightPressed)
            rotDir = rightRotDir;
        else if (!verticalPressed && leftPressed)
            rotDir = leftRotDir;
        else if (forwardPressed && rightPressed)
            rotDir = (_baseForward + rightRotDir).normalized;
        else if (forwardPressed && leftPressed)
            rotDir = (_baseForward + leftRotDir).normalized;
        else if (backwardPressed && rightPressed)
            rotDir = (_baseForward - rightRotDir).normalized;
        else if (backwardPressed && leftPressed)
            rotDir = (_baseForward - leftRotDir).normalized;
        else
            rotDir = move;

        if (rotDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(rotDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                _rotationSpeed * Time.fixedDeltaTime
            );
        }
    }
}
