﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticData {

    public enum Scenes
    {
        Start = 0,
        Game = 1,
    }

    /// <summary>
    /// 窗口预制体的路径
    /// </summary>
    public const string POPUP_PATH = "Popup/";

    /// <summary>
    /// 音频加载路径
    /// </summary>
    public const string AUDIO_CLIP_PATH = "Music/";

    /// <summary>
    /// 头像加载路径
    /// </summary>
    public const string HEAD_ICON_PATH = "Pic/Head/";

    /// <summary>
    /// UI按钮路径
    /// </summary>
    public const string UI_PIC = "Pic/ColorfulButtons/";




  
    //BGM
    public const string failed = "failed";
    public const string Success = "Success";
    public const string EnterScene = "EnterScene";
    public const string LevelUp = "LevelUp";
    //Clcik
    public const string Click = "click";
    //Song
    public static string[] Song = new string[3] { "weinixieshi","konggangqu","qitian"};


}
