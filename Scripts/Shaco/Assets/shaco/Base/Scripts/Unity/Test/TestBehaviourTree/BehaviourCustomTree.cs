using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//自定义树节点范例
public class BehaviourCustomTree : shaco.Base.BehaviourTree
{
    //节点数据
    private string customName = "custom";

    public override string ToString()
    {
        return "Custom";
    }

    //保存数据到json的格式化
    public override List<string> ToJson()
    {
        var retValue = base.ToJson();
        retValue.Add(customName);
        return retValue;
    }

    //从json读取数据
    public override void FromJson(shaco.Base.BehaviourTreeConfig.JsonInfo jsonData)
    {
        base.FromJson(jsonData);
        customName = jsonData.GetNextData();
    }

    //编辑器绘制方法回调
    public override void OnGUIDraw()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Button1")) Debug.Log("Button1");
        if (GUILayout.Button("Button2")) Debug.Log("Button2");
        if (GUILayout.Button("Button3")) Debug.Log("Button3");
        GUILayout.EndHorizontal();
        customName = GUILayout.TextArea(customName);
    }

    //在编辑器上的显示名字
    public override string GetDisplayName()
    {
        return "Custom";
    }

    //任务执行方法
    public override bool Process()
    {
        //设置任务结果回调
        this.SetOnProcessResultCallBack((bool isStoped) =>
        {
            if (!isStoped)
            {
                int currentIndex = 0;
                int randSelectIndex = shaco.Base.Utility.Random(0, Count);
                ForeachChildren((shaco.Base.BehaviourTree tree) =>
                {
                    if (currentIndex++ == randSelectIndex)
                    {
                        tree.Process();
                        return false;
                    }
                    else
                        return true;
                });
            }
        });
        return base.Process();
    }
}