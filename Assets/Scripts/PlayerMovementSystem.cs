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
    float3 _position;
    quaternion _rotation;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (input, player, transform) in SystemAPI.Query<RefRO<PlayerInput>, RefRW<Player>, RefRW<LocalTransform>>().WithAll<Simulate>())
        {
            player.ValueRW.YawAngle += input.ValueRO.Roatation.x * player.ValueRO.RotationSpeed;
            player.ValueRW.PitchAngle += input.ValueRO.Roatation.y * player.ValueRO.RotationSpeed;

            player.ValueRW.YawAngle = Utility.ClampAngle(player.ValueRO.YawAngle, float.MinValue, float.MaxValue);
            player.ValueRW.PitchAngle = Utility.ClampAngle(player.ValueRO.PitchAngle, player.ValueRO.MinPitch, player.ValueRO.MaxPitch);

            if (Utility.Magnitude(input.ValueRO.Movement) > player.ValueRO.Threshold)
            {
                transform.ValueRW.Rotation = Quaternion.Euler(0f, player.ValueRO.YawAngle, 0f);

                float angle = (float)Math.Atan2(input.ValueRO.Movement.x, input.ValueRO.Movement.y) * Mathf.Rad2Deg + player.ValueRO.YawAngle;
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

                transform.ValueRW.Position += new float3(direction.normalized) * player.ValueRO.Speed * SystemAPI.Time.DeltaTime;
            }
        }

        _position = new float3(0f, 0f, 0f);
        _rotation = Quaternion.Euler(0f, 0f, 0f);
        foreach (var (player, transform) in SystemAPI.Query<RefRO<Player>, RefRO<LocalTransform>>().WithAll<GhostOwnerIsLocal>())
        {
            _position = transform.ValueRO.Position;
            _rotation = Quaternion.Euler(player.ValueRO.PitchAngle, player.ValueRO.YawAngle, 0f);
        }

        foreach (var (camControler, transform) in SystemAPI.Query<RefRO<CameraControler>, RefRW<LocalTransform>>())
        {
            transform.ValueRW.Position = _position;
            transform.ValueRW.Rotation = _rotation;
        } 
    }
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct CameraMovementPrepareSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float3 _position = new float3(0f, 0f, 0f);
        quaternion _rotation = Quaternion.Euler(0f, 0f, 0f);

        foreach (var (camControler, transform) in SystemAPI.Query<RefRO<CameraControler>, RefRW<LocalTransform>>())
        {
            _position = transform.ValueRW.Position;
            _rotation = transform.ValueRW.Rotation;
        } 

        foreach (var (camControl, transform) in SystemAPI.Query<CameraControl, RefRO<LocalTransform>>().WithAll<GhostOwnerIsLocal>())
        {
            if (camControl.TargetTransform != null)
            {
                camControl.TargetTransform.position = _position;
                camControl.TargetTransform.rotation = _rotation;
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

        foreach (var (animData, transform, input) in SystemAPI.Query<AnimationData, RefRO<LocalTransform>, RefRO<PlayerInput>>())
        {
            animData.Transform.position = transform.ValueRO.Position;
            animData.Transform.rotation = transform.ValueRO.Rotation;

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