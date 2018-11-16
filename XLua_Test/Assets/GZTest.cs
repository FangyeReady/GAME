using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
[Hotfix]
public class GZTest {

    private int a = 0;
    public GZTest(int val)
    {
        this.a = val;
        Debug.Log("this is C# GZ: " + a.ToString());
    }
}
