using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[System.Serializable]
public struct RotationSpeed : IComponentData
{
    [Range(0, 100)]
    public float value;
}
public class RotationSpeedComponent : ComponentDataWrapper<RotationSpeed> {


}
