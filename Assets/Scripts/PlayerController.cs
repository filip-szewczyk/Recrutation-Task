using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Rigidbody _rigidbody;
    Animator _animator;

    public float MovementSpeed = 0.2f;
    Vector2 _movementValueVector;

    public float RotationSpeed = 0.2f;
    Vector3 _lookVector;

    int _diagonalMovementHash;
    int _horizontalMovementHash;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        _diagonalMovementHash = Animator.StringToHash("DiagonalMovement");
        _horizontalMovementHash = Animator.StringToHash("HorizontalMovement");
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        Vector3 forwardMovementVector = transform.forward * _movementValueVector.y * MovementSpeed;
        Vector3 sideMovementVector = transform.right * _movementValueVector.x * MovementSpeed;

        _rigidbody.MovePosition(_rigidbody.position + forwardMovementVector + sideMovementVector);
        transform.Rotate(_lookVector);
    }

    void OnMove(InputValue movementValue)
    {
        _movementValueVector = movementValue.Get<Vector2>();

        _animator.SetFloat(_diagonalMovementHash, -_movementValueVector.x);
        _animator.SetFloat(_horizontalMovementHash, _movementValueVector.y);
    }

    void OnLook(InputValue lookValue)
    {
        Vector2 lookValueVector = lookValue.Get<Vector2>();
        _lookVector = new Vector3(0, lookValueVector.x, 0) * RotationSpeed;
    }
}
