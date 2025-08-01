using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct PlayerSpawner : IComponentData
{
    public Entity Player1;
    public Entity Player2;
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
            PlayerSpawner spawnerComponent = default(PlayerSpawner);

            int secondPrefabIndex = 0;
            if (authoring.PlayerPrefabs.Count > 1)
                secondPrefabIndex = 1;
            spawnerComponent.Player1 = GetEntity(authoring.PlayerPrefabs[0], TransformUsageFlags.Dynamic);
            spawnerComponent.Player2 = GetEntity(authoring.PlayerPrefabs[secondPrefabIndex], TransformUsageFlags.Dynamic);

            AddComponent(entity, spawnerComponent);
        }
    }
}