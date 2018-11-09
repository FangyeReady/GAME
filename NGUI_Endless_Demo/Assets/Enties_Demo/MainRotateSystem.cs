
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class RotationSpeedSystem : JobComponentSystem
{
    [BurstCompile]
    struct RotationSpeedRotation : IJobProcessComponentData<Rotation, RotationSpeed>
    {
        public float dt;
        public void Execute(ref Rotation rotation, ref RotationSpeed speed)
        {
            rotation.Value = math.mul(math.normalize(rotation.Value), quaternion.AxisAngle(math.up(), speed.value * dt));
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new RotationSpeedRotation() { dt = Time.deltaTime };
        return job.Schedule(this, inputDeps);
    }

}
