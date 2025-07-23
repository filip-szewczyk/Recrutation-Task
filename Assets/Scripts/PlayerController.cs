using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Rigidbody _rigidbody;

    public float MovementSpeed = 0.2f;
    Vector2 _movementValueVector;

    public float RotationSpeed = 0.2f;
    Vector3 _lookVector;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        Vector3 forwardMovementVector = -transform.forward * _movementValueVector.x;
        Vector3 sideMovementVector = transform.right * _movementValueVector.y;

        _rigidbody.MovePosition(_rigidbody.position + forwardMovementVector + sideMovementVector);
        transform.Rotate(_lookVector);
    }

    void OnMove(InputValue movementValue)
    {
        _movementValueVector = movementValue.Get<Vector2>() * MovementSpeed;
    }

    void OnLook(InputValue lookValue)
    {
        Vector2 lookValueVector = lookValue.Get<Vector2>() * RotationSpeed;
        _lookVector = new Vector3(0, lookValueVector.x, 0);
    }
}
