using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct PlayerMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (input, player, transform) in SystemAPI.Query<RefRO<PlayerInput>, RefRO<Player>, RefRW<LocalTransform>>().WithAll<Simulate>())
        {
            float2 movement = math.normalizesafe(input.ValueRO.Movement) * player.ValueRO.Speed * SystemAPI.Time.DeltaTime;
            transform.ValueRW.Position += new float3(movement.x, 0f, movement.y);
        }
    }
}

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct PlayerAnimationMovementSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        //var buffer = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        var buffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (anim, entity) in SystemAPI.Query<AnimationPrefab>().WithEntityAccess())
        {
            Debug.Log(entity);
            GameObject animationObject = GameObject.Instantiate(anim.Prefab);

            AnimationData component = new AnimationData();
            component.Transform = animationObject.transform;
            component.Animator = animationObject.GetComponent<Animator>();

            buffer.AddComponent(entity, component);

            buffer.RemoveComponent<AnimationPrefab>(entity);
        }
        buffer.Playback(state.EntityManager);

        foreach (var (animation, transform, input) in SystemAPI.Query<AnimationData, RefRO<LocalTransform>, RefRO<PlayerInput>>())
        {
            animation.Transform.position = transform.ValueRO.Position;
            animation.Transform.rotation = transform.ValueRO.Rotation;

            animation.Animator.SetFloat("DiagonalMovement", input.ValueRO.Movement.x);
            animation.Animator.SetFloat("HorizontalMovement", input.ValueRO.Movement.y);
        }
    }
}