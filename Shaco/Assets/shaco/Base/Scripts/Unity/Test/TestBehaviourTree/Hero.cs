using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMove : shaco.Base.IBehaviourParam
{
	//移动距离
    public Vector3 moveOffset;

    //需要移动的对象
    public Transform moveTarget;
}

public class Hero : MonoBehaviour, shaco.Base.IBehaviourParam
{
    public shaco.Base.BehaviourRootTree behaviourRoot = new shaco.Base.BehaviourRootTree();
    public TestGameData gameData;

    //视野距离，超过该记录的敌人不会被看到
    public float eyeDistance = 250;

    //角色与敌人的最小距离，小于该距离的敌人不会被跟踪移动
    public float minDistanceToEnemy = 50;

    //移动速度
    public float moveSpeed = 0.1f;

    void Start()
	{
        behaviourRoot.LoadFromResourcesOrLocal("Hero");

        behaviourRoot.SetParameter(this);
        behaviourRoot.SetParameter(gameData);
        behaviourRoot.Start();
    }

	// void Update()
	// {
    //     behaviourRoot.Update(Time.deltaTime);
    // }
}