using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupStart : ObjBase {

    private void BeginToGame()
    {
        Utility.SwitchScene(StaticData.Scenes.Game);
        LoggerM.Log("Log Test");
        LoggerM.LogError("Log Test");
        LoggerM.LogWarning("Log Test");
    }

  
}
