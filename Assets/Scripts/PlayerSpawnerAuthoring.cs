using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct PlayerSpawner : IComponentData
{
    public Entity Player1;
    public Entity Player2;

    public float3 SecondPlayerSpawningPosition;
}

[DisallowMultipleComponent]
public class PlayerSpawnerAuthoring : MonoBehaviour
{
    public List<GameObject> PlayerPrefabs;

    class Baker : Baker<PlayerSpawnerAuthoring>
    {
        public override void Bake(PlayerSpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            PlayerSpawner component = default(PlayerSpawner);

            int secondPrefabIndex = 0;
            if (authoring.PlayerPrefabs.Count > 1)
                secondPrefabIndex = 1;
            component.Player1 = GetEntity(authoring.PlayerPrefabs[0], TransformUsageFlags.Dynamic);
            component.Player2 = GetEntity(authoring.PlayerPrefabs[secondPrefabIndex], TransformUsageFlags.Dynamic);

            float3 translatedPosition = authoring.transform.position;
            translatedPosition.z += 10;
            component.SecondPlayerSpawningPosition = translatedPosition;

            AddComponent(entity, component);
        }
    }
}