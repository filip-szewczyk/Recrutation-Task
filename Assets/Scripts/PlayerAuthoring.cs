using Unity.Entities;
using UnityEngine;

public struct Player : IComponentData
{
    public float Speed;
    public float RotationSpeed;

    public float MaxPitch;
    public float MinPitch;

    public float YawAngle;
    public float PitchAngle;
    public float Threshold;
}

public class CameraControl : IComponentData
{
    public Transform TargetTransform;
    public float RotationSpeed;
    public float MaxPitch;
    public float MinPitch;
    public float Pitch;
    public float Yaw;
}

public class AnimationPrefab : IComponentData
{
    public GameObject Prefab;
}

public class AnimationData : ICleanupComponentData
{
    public Transform Transform;
    public Animator Animator;
}

[DisallowMultipleComponent]
public class PlayerAuthoring : MonoBehaviour
{
    public GameObject AnimationPrefab;
    public GameObject CameraTarget;

    public float Speed = 1f;

    public float CameraRotationSpeed = 1f;
    public float CameraMaxPitch = 70f;
    public float CameraMinPitch = 40f;

    float _threshold = 0.1f;

    public Animator Animator;

    class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            Player dataComponent = new Player();
            dataComponent.Speed = authoring.Speed;
            dataComponent.Threshold = authoring._threshold;
            dataComponent.RotationSpeed = authoring.CameraRotationSpeed;
            dataComponent.MaxPitch = authoring.CameraMaxPitch;
            dataComponent.MinPitch = authoring.CameraMinPitch;

            AnimationPrefab animComponent = new AnimationPrefab();
            animComponent.Prefab = authoring.AnimationPrefab;

            CameraControl cameraControlComponent = new CameraControl();
            cameraControlComponent.RotationSpeed = authoring.CameraRotationSpeed;
            cameraControlComponent.MaxPitch = authoring.CameraMaxPitch;
            cameraControlComponent.MinPitch = authoring.CameraMinPitch;

            AddComponent(entity, dataComponent);
            AddComponentObject(entity, animComponent);
            AddComponentObject(entity, cameraControlComponent);
        }
    }

}

