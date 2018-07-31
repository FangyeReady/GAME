using System.Collections;
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

    


    //100为失败BGM,001为序列号
    //101为成功BGM,001为序列号
    //102为进入Game场景开始的BGM,001为序列号
    //103为升级BGM,001为序列号
    //201为事件BGM,001为序列号,以此类推
    //301001为点击BGM
    //401开始为歌曲

    public const int Failed = 100001;
    public const int Success = 101001;
    public const int EnterScene = 102001;
    public const int LevelUp = 103001;
    public const int ClickA = 301001;
    public static int[] Song = new int[3] { 401001,401002,401003};


}
