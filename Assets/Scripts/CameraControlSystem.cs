using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial class CameraControlSystem : SystemBase
{
    bool _cameraSet;

    protected override void OnCreate()
    {
        RequireForUpdate<Player>();
        _cameraSet = false;
    }

    protected override void OnUpdate()
    {
        if (_cameraSet)
            return;

        foreach (var target in SystemAPI.Query<CameraControl>().WithAll<GhostOwnerIsLocal>())
        {
            target.TargetTransform = GameObject.FindGameObjectWithTag("CameraTarget").GetComponent<Transform>();
            _cameraSet = true;
        }
    }
}