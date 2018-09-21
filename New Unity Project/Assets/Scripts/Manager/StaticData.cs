using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Star
{
    one = 1,
    two,
    three,
    four,
    five,
    none = 0
}
/// <summary>
/// 角色信息
/// </summary>
public class PlayerInfo
{
    public uint ID;
    public string Name;
    public uint Coin;
    public uint BusinessLevel;
    public uint MaxServentNum;
    public string TotalTime;
    public List<ServentInfo> Servent;
}

public class ServentInfo
{
    public int ID;
    public string Name;
    public int Level;
    public int skillID;
    public string Desc;
    public int Loyal;
    public int star;
    public int tire;
    public int favorability;
}

public class GameSettingInfo
{
    public int HeadPic;
    public uint GachaCost;
    public int serventMin;
    public int serventMax;
    /// <summary>
    /// 可以用于道具栏的扩展。。。消耗金币
    /// </summary>
    public int cellCount;
    public List<int> none;
    public List<int> one;
    public List<int> two;
    public List<int> three;
    public List<int> four;
    public List<int> five;

    public List<int> propID;
}

public class ServentSkillInfo
{
    public Dictionary<string, int[]> skillID;
    public Dictionary<string, string> skillDesc;
}

/// <summary>
/// 道具的用处
/// </summary>
public class PropCfg
{
    public int tire;
    public int favorability;
    public int loyal;
    public int level;
    public int needServentnum;
    public int tiredefence;
    public int jindu;
    public int all;
    public int exp;
}

/// <summary>
/// 道具的描述
/// </summary>
public class PropInfo
{
    public string name;
    public string usedesc;
    public string id;
    public int price;
}


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
    public const string HEAD_ICON_PATH = "Pic/Head/RoleHead/";

    /// <summary>
    /// UI按钮路径
    /// </summary>
    public const string UI_PIC = "Pic/ColorfulButtons/";

    public const string CONFIG_PATH = "/CFG/";


  
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
