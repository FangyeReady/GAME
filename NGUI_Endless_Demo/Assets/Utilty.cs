using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;


public static class Utilty {
 
    [Conditional("UNITY_EDITOR")]
    public static void Log(string str)
    {
       UnityEngine.Debug.Log("[Log] " + str);
    }
}
