using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System.IO;
[Hotfix]
public class HelloWorld : MonoBehaviour {

    private static LuaEnv m_luaEnv;

    public static LuaEnv LuaInstance
    {
        get {
            if (m_luaEnv == null)
            {
                m_luaEnv = new LuaEnv();
            }
            return m_luaEnv;
        }
    }


    private static HelloWorld helloWorld;
    public static HelloWorld Instance
    {
        get {
            return helloWorld;
        }
    }


    [XLua.CSharpCallLua]
    public delegate double LuaMax(double a, double b);


    private void Awake()
    {
        helloWorld = this;

        string str = @"print('hello world~!')";

        LuaInstance.DoString(str);



        var max = LuaInstance.Global.GetInPath<LuaMax>("math.max");

        Debug.Log("max:" + max(1, 2));


        InitHotFixScripts();

        Debug.Log(Add(1,2).ToString());

    }


    private void InitHotFixScripts()
    {
        string path = Application.dataPath + "/HotFixScrpts/";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var dir = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);

        for (int i = 0; i < dir.Length; i++)
        {
            string txt = File.ReadAllText(dir[i], System.Text.Encoding.UTF8);
            LuaInstance.DoString(txt);
        }
    }

    private int Add(int a, int b)
    {
        return a + b;
    }


}
