using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//查找在视野中的敌人，并向它移动
public class FindEnemyInVision : shaco.Base.IBehaviourProcess
{
    public IEnumerator<shaco.Base.IBehaviourEnumerator> Process(shaco.Base.BehaviourTree tree)
	{
        shaco.Log.Info("find...");
        var gameData = tree.GetRoot().GetParameter<TestGameData>();
        var hero = tree.GetParameter<Hero>();

        //查找附近的敌人
        Enemy findEnemy = null;
        for (int i = gameData.enemys.Count - 1; i >= 0; --i)
        {
            var enemyTmp = gameData.enemys[i];
            var distance = Vector3.Distance(enemyTmp.transform.position, hero.transform.position);

            if (distance <= hero.eyeDistance && !enemyTmp.isTackedTarget)
            {
                //碰撞到敌人
                if (distance < hero.minDistanceToEnemy)
                {
                    tree.SetParameter(new TestAttackData(){ target = enemyTmp });
                    enemyTmp.isTackedTarget = true;
                }
                else 
                {
                    findEnemy = enemyTmp;
                    break;
                }
            }
        }

        //设置向敌人移动的量
        if (findEnemy != null)
        {
            tree.SetParameter(new TestMove()
            {
                moveTarget = hero.transform,
                moveOffset = (findEnemy.transform.position - hero.transform.position).normalized * hero.moveSpeed
            });
        }
        //暂时原地不动
        else 
        {
            Debug.Log("standby...");
            yield return new shaco.Base.StopProcess();
        }
    }
}

public class MoveProcess : shaco.Base.IBehaviourProcess
{
    public IEnumerator<shaco.Base.IBehaviourEnumerator> Process(shaco.Base.BehaviourTree tree)
    {
        // Debug.Log("will move...");
        // yield return new shaco.Base.WaitforSeconds(0.1f);
        Debug.Log("move...");
        var testMove = tree.GetParameter<TestMove>();
        testMove.moveTarget.position += new Vector3(testMove.moveOffset.x, 0, testMove.moveOffset.z);
        yield return null;
    }
}

public class AttackProcess : shaco.Base.IBehaviourProcess
{
    public IEnumerator<shaco.Base.IBehaviourEnumerator> Process(shaco.Base.BehaviourTree tree)
    {
        var attackData = tree.GetParameter<TestAttackData>();
        if (null == attackData)
        {
            yield return new shaco.Base.StopProcess();
        }

        Debug.Log("will attack target=" + attackData.target.name);
        tree.RemoveParameter<TestAttackData>();

        var testMove = tree.GetParameter<TestMove>();
        var action1 = shaco.RotateBy.Create(new Vector3(0, 50, 0), 0.5f);
        var action2 = action1.Reverse();
        var moveAction = shaco.Sequeue.Create(action1, action2);
        moveAction.RunAction(testMove.moveTarget.gameObject);

        //等待条件完毕
        // yield return new shaco.Base.WaitUntil(() =>
        // {
        //     return isActionEng;
        // });

        //等待5.0秒
        // yield return 5.0f;
        // yield return new shaco.Base.WaitforSeconds(5.0f);

        //等待动画执行完毕
        yield return new shaco.WaitForActionEnd(moveAction);
    }
}

public class AttackDamege : shaco.Base.IBehaviourProcess
{
    public IEnumerator<shaco.Base.IBehaviourEnumerator> Process(shaco.Base.BehaviourTree tree)
    {
        Debug.Log("AttackDamege...");
        yield return null;
    }
}

public class Test1 : shaco.Base.IBehaviourProcess
{
    public IEnumerator<shaco.Base.IBehaviourEnumerator> Process(shaco.Base.BehaviourTree tree)
    {
        Debug.Log("Test1");
        yield return null;
    }
}

public class Test2 : shaco.Base.IBehaviourProcess
{
    public IEnumerator<shaco.Base.IBehaviourEnumerator> Process(shaco.Base.BehaviourTree tree)
    {
        Debug.Log("Test2");
        yield return null;
    }
}

public class Test3 : shaco.Base.IBehaviourProcess
{
    public IEnumerator<shaco.Base.IBehaviourEnumerator> Process(shaco.Base.BehaviourTree tree)
    {
        Debug.Log("Test3");
        yield return null;
    }
}