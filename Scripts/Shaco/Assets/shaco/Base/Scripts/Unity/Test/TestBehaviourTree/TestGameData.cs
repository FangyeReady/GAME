using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestGameData : MonoBehaviour, shaco.Base.IBehaviourParam
{
    public List<Enemy> enemys = new List<Enemy>();

    public List<List<List<int>>> s1 = new List<List<List<int>>>();
    public Dictionary<string, string> s2 = new Dictionary<string, string>();
    public List<int> s3 = new List<int>() { 300, 400 };
    public List<TestGameData> s4 = new List<TestGameData>() { };
    public Dictionary<TestGameData, string> s5 = new Dictionary<TestGameData, string>();

    void Start()
    {
        s1.Add(new List<List<int>>()
        {
            new List<int>() { 100, 200 }
        });
        s2.Add("aa", "bb");
        s2.Add("123", "321");
    }

    void OnGUI()
    {
        float width = 160;
        float height = 30;

#if !UNITY_EDITOR
        width *= 4;
        height *= 4;
#endif

        GUILayoutOption[] optionTmp = new GUILayoutOption[] { GUILayout.Width(width), GUILayout.Height(height) };

        if (GUILayout.Button("SpwanEnemy", optionTmp))
        {
            var enemyObj = shaco.ResourcesEx.LoadResourcesOrLocal("enemy", typeof(GameObject)) as GameObject;
            this.enemys.Add(enemyObj.GetComponent<Enemy>());
            shaco.UnityHelper.ChangeParent(enemyObj, this.gameObject);
        }

        // if (GUILayout.Button("Load", optionTmp))
        // {
        //     _rootTree = _rootTree.LoadFromJsonPath(Application.dataPath + "/Resources/TestBehaviourTree.json");
        //     _rootTree.PrintAllChildren();
        // }

        // if (GUILayout.Button("Save", optionTmp))
        // {
        //     _rootTree.RemoveChildren();
        //     _rootTree.AddChild(new shaco.Base.BehaviourTree("1"));
        //     _rootTree.AddChild(new shaco.Base.BehaviourTree("2"));
        //     _rootTree.AddChild(new shaco.Base.BehaviourTree("3"));
        //     var child0 = _rootTree.GetChild(0);
        //     child0.AddChild(new shaco.Base.BehaviourTree("hello"));
        //     child0.AddChild(new shaco.Base.BehaviourTree("world"));
        //     var child0_1 = child0.GetChild(1);
        //     child0_1.AddChild(new shaco.Base.BehaviourTree("3_1"));
        //     child0_1.GetChild(0).AddSibling(new shaco.Base.BehaviourTree("3_2"));
        //     child0_1.GetChild(0).AddChild(new shaco.Base.BehaviourTree("4_1"));
        //     child0_1.GetChild(0).AddChild(new shaco.Base.BehaviourTree("4_2"));
        //     child0_1.GetChild(0).AddChild(new shaco.Base.BehaviourTree("4_3"));
        //     child0_1.GetChild(0).InsertChild(new shaco.Base.BehaviourTree("4_1_1"), 1);
        //     child0_1.GetChild(0).InsertChild(new shaco.Base.BehaviourTree("4_1_0"), 0);
        //     child0_1.GetChild(0).InsertChild(new shaco.Base.BehaviourTree("4_1_last"), child0_1.GetChild(0).Count);
        //     _rootTree.PrintAllChildren();
        //     _rootTree.SaveToJson(Application.dataPath + "/Resources/TestBehaviourTree.json");
        // }
    }
}
