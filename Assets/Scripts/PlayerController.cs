using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Animator _animator;

    public float MovementSpeed = 0.2f;
    Vector3 _movement;

    public GameObject CameraTarget;
    public float RotationSpeed = 0.2f;
    public float MinCameraPitch = 30f;
    public float MaxCameraPitch = 70f;
    float _targetYaw;
    float _targetPitch;
    Vector3 _rotation;

    int _diagonalMovementHash;
    int _horizontalMovementHash;

    float _threshold = 0.1f;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _diagonalMovementHash = Animator.StringToHash("DiagonalMovement");
        _horizontalMovementHash = Animator.StringToHash("HorizontalMovement");

        _targetYaw = CameraTarget.transform.rotation.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        Move();
        Rotate();
    }

    void Move()
    {
        if (_movement.magnitude > _threshold)
        {
            float angle = (float)Math.Atan2(_movement.x, _movement.z) * Mathf.Rad2Deg + CameraTarget.transform.eulerAngles.y;
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

            transform.rotation = Quaternion.Euler(0f, _targetYaw, 0f);
            transform.position += direction.normalized * MovementSpeed;
        }
    }

    void Rotate()
    {
        _targetYaw += _rotation.y * RotationSpeed;
        _targetPitch += _rotation.z * RotationSpeed;

        _targetYaw = ClampAngle(_targetYaw, float.MinValue, float.MaxValue);
        _targetPitch = ClampAngle(_targetPitch, MinCameraPitch, MaxCameraPitch);

        CameraTarget.transform.rotation = Quaternion.Euler(_targetPitch, _targetYaw, 0f);
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f)
            angle += 360f;
        if (angle > 360f)
            angle -= 360f;

        return Mathf.Clamp(angle, min, max);
    }

    void OnMove(InputValue input)
    {
        Vector2 movementValue = input.Get<Vector2>();
        _movement = new Vector3(movementValue.x, 0f, movementValue.y);

        _animator.SetFloat(_diagonalMovementHash, -movementValue.x);
        _animator.SetFloat(_horizontalMovementHash, movementValue.y);
    }
    void OnLook(InputValue input)
    {
        Vector2 lookValue = input.Get<Vector2>();
        _rotation = new Vector3(0, lookValue.x, lookValue.y);
    }
}
