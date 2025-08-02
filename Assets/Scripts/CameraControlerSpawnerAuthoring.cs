using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct CameraControlerSpawner : IComponentData
{
    public Entity CamControler;
}

public class CameraControlerSpawnerAuthoring : MonoBehaviour
{
    public GameObject CameraControlerPrefab;

    class CmaeraControlerSpawnerBaker : Baker<CameraControlerSpawnerAuthoring>
    {
        public override void Bake(CameraControlerSpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            CameraControlerSpawner spawnerComponent = new CameraControlerSpawner();
            spawnerComponent.CamControler = GetEntity(authoring.CameraControlerPrefab, TransformUsageFlags.Dynamic);

            AddComponent(entity, spawnerComponent);
        }
    }
}

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct SpawnCameraControlerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CameraControlerSpawner>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (camControlspawner, entity) in SystemAPI.Query<CameraControlerSpawner>().WithEntityAccess())
        {
            CameraControlerSpawner camControlSpawner = SystemAPI.GetSingleton<CameraControlerSpawner>();
            commandBuffer.Instantiate(camControlSpawner.CamControler);

            commandBuffer.RemoveComponent<CameraControlerSpawner>(entity);
        }
        commandBuffer.Playback(state.EntityManager);
    }
}