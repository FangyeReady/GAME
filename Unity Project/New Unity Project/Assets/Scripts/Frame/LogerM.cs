using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;


public static class LoggerM {

   // [Conditional("UNITY_EDITOR")]
    public static void Log(string des)
    {
        Debug.Log("[Log]---" + des);
    }

   // [Conditional("UNITY_EDITOR")]
    public static void LogError(string des)
    {
        Debug.LogError("[LogError]---" + des);
    }

    //[Conditional("UNITY_EDITOR")]
    public static void LogWarning(string des)
    {
        Debug.LogWarning("[LogWarning]---" + des);
    }
}
