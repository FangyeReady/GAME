using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, shaco.Base.IBehaviourParam
{
    //标记为已经找到的目标，下次不再寻找它
    public bool isTackedTarget = false;
}
