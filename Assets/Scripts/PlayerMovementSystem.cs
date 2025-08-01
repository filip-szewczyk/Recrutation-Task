using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct PlayerMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (input, player, transform) in SystemAPI.Query<RefRO<PlayerInput>, RefRW<Player>, RefRW<LocalTransform>>().WithAll<Simulate>())
        {
            float _yawAngle = player.ValueRO.YawAngle;
            float _pitchAngle = player.ValueRO.PitchAngle;

            _yawAngle += input.ValueRO.Roatation.x * player.ValueRO.RotationSpeed;
            _pitchAngle += input.ValueRO.Roatation.y * player.ValueRO.RotationSpeed;

            _yawAngle = Utility.ClampAngle(_yawAngle, float.MinValue, float.MaxValue);
            _pitchAngle = Utility.ClampAngle(_pitchAngle, player.ValueRO.MinPitch, player.ValueRO.MaxPitch);

            player.ValueRW.YawAngle = _yawAngle;
            player.ValueRW.PitchAngle = _pitchAngle;

            transform.ValueRW.Rotation = Quaternion.Euler(_pitchAngle, _yawAngle, 0f);

            if (Utility.Magnitude(input.ValueRO.Movement) > player.ValueRO.Threshold)
            {
                float angle = (float)Math.Atan2(input.ValueRO.Movement.x, input.ValueRO.Movement.y) * Mathf.Rad2Deg + _yawAngle;
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                
                transform.ValueRW.Position += new float3(direction.normalized) * player.ValueRO.Speed * SystemAPI.Time.DeltaTime;
            }
        }
    }
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct CameraMovementPrepareSystem : ISystem
{
    float _pitchAngle;
    float _yawAngle;

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (camControl, playerData, transform, input) in SystemAPI.Query<CameraControl, RefRW<Player>, RefRO<LocalTransform>, RefRW<PlayerInput>>().WithAll<GhostOwnerIsLocal>())
        {
            if (camControl.TargetTransform != null)
            {
                camControl.TargetTransform.position = transform.ValueRO.Position;
                camControl.TargetTransform.rotation = transform.ValueRO.Rotation;       
            }
        }
    }
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct PlayerAnimationMovementSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var buffer = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        foreach (var (animPrefab, entity) in SystemAPI.Query<AnimationPrefab>().WithEntityAccess())
        {
            GameObject animObject = GameObject.Instantiate(animPrefab.Prefab);

            AnimationData animDataComponent = new AnimationData();
            animDataComponent.Transform = animObject.transform;
            animDataComponent.Animator = animObject.GetComponent<Animator>();

            buffer.AddComponent(entity, animDataComponent);
            buffer.RemoveComponent<AnimationPrefab>(entity);
        }

        foreach (var (animData, entity) in SystemAPI.Query<AnimationData>().WithNone<LocalToWorld>().WithEntityAccess())
        {
            GameObject.Destroy(animData.Transform.gameObject);
            buffer.RemoveComponent<AnimationData>(entity);
        }

        foreach (var (player, animData, transform, input) in SystemAPI.Query<Player, AnimationData, RefRO<LocalTransform>, RefRO<PlayerInput>>())
        {
            if (Utility.Magnitude(input.ValueRO.Movement) > player.Threshold)
            {
                animData.Transform.position = transform.ValueRO.Position;

                Quaternion animRotation = transform.ValueRO.Rotation;
                animData.Transform.rotation = Quaternion.Euler(0f, animRotation.eulerAngles.y, 0f);
            }

            animData.Animator.SetFloat("DiagonalMovement", input.ValueRO.Movement.x);
            animData.Animator.SetFloat("HorizontalMovement", input.ValueRO.Movement.y);
        }
    }
}

public class Utility
{
    public static float Magnitude(float2 value)
    {
        return math.sqrt(value.x * value.x + value.y * value.y);
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f)
            angle += 360f;
        if (angle > 360f)
            angle -= 360f;

        return Mathf.Clamp(angle, min, max);
    }
}