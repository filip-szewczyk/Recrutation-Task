using Unity.Entities;
using UnityEngine;

public struct CameraControler : IComponentData { }

public class CameraControlerAuthoring : MonoBehaviour
{
    class CameraControlerBaker : Baker<CameraControlerAuthoring>
    {
        public override void Bake(CameraControlerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<CameraControler>(entity);
        }
    }
}
