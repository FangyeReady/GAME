using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 工具类
/// </summary>
public static class Utility  {

    public static void SwitchScene(StaticData.Scenes scene)
    {
        SceneManager.LoadScene((int)scene);
        Debug.Log("Load Scene " + scene.ToString());
    }
}
