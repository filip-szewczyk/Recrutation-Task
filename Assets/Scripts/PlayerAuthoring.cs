using Unity.Entities;
using UnityEngine;

public struct Player : IComponentData
{
    public float Speed;
}

public class AnimationPrefab : IComponentData
{
    public GameObject Prefab;
}

public class AnimationData : IComponentData
{
    public Transform Transform;
    public Animator Animator;
}

[DisallowMultipleComponent]
public class PlayerAuthoring : MonoBehaviour
{
    public GameObject Prefab;

    public float Speed = 1f;

    class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            Player dataComponent = new Player();
            dataComponent.Speed = authoring.Speed;

            AnimationPrefab animationComponent = new AnimationPrefab();
            animationComponent.Prefab = authoring.Prefab;

            AddComponent(entity, dataComponent);
            AddComponentObject(entity, animationComponent);
        }
    }

}

