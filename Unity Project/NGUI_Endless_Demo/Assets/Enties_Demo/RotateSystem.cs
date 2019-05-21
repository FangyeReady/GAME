using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class RotateSystem : ComponentSystem
{


    struct SphereObj
    {
       public SphereData sphere;
       public Transform transform;
    }

    protected override void OnUpdate()
    {
        foreach (var item in GetEntities<SphereObj>())
        {
            item.transform.Rotate(Vector3.up, Time.deltaTime * item.sphere.Speed);
        }
    }
}
