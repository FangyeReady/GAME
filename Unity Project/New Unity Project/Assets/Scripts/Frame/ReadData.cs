using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using System.Text;
using System;

/// <summary>
/// 读取配置表
/// </summary>
public class ReadData :AutoStaticInstance<ReadData> {

    public void GetPlayerData(string path, out PlayerInfo info)
    {
        string str = File.ReadAllText(path);
        JsonData data = JsonMapper.ToObject(str);

        info = new PlayerInfo();
        info.ID = (uint)data["ID"];
        info.Name = (string)data["Name"];
        info.Coin = (uint)data["Coin"];
        info.BusinessLevel = (uint)data["BusinessLevel"];
        info.MaxServentNum = (uint)data["MaxServentNum"];
        info.TotalTime = (string)data["TotalTime"];
        info.Servent = new List<ServentInfo>();
        info.propID = new List<PropData>();
        JsonData servent = JsonMapper.ToObject(data["Servent"].ToJson());
        JsonData propID = JsonMapper.ToObject(data["propID"].ToJson());

        for (int i = 0; i < propID.Count; i++)
        {
            PropData prop = JsonMapper.ToObject<PropData>(propID[i].ToJson());
            info.propID.Add(prop);
        }

        for (int i = 0; i < servent.Count; i++)
        {
            ServentInfo serventInfo = JsonMapper.ToObject<ServentInfo>(servent[i].ToJson());
            info.Servent.Add(serventInfo);
        }
        //存储数据
        //Player.Instance.RefreshData();
    }


    public void SaveData(string path, string data)
    {
        //FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate);
        //byte[] bytes = Encoding.UTF8.GetBytes(data);
        //fileStream.Write(bytes,0,bytes.Length);
        //fileStream.Close();
    }


    public override void Save()
    {
        
    }


    public void GetGameSettingData(string path, out GameSettingInfo info)
    {
        string str = File.ReadAllText(path);
        JsonData data = JsonMapper.ToObject(str);

        info = new GameSettingInfo();
        info.HeadPic = (int)data["HeadPic"];
        if (info.HeadPic.Equals(0))
        {
            info.HeadPic = Player.Instance.PlayerInfos.Servent[0].ID;
        }
        info.GachaCost = (uint)data["GachaCost"];
        info.cellCount = (int)data["cellCount"];
        info.none = JsonMapper.ToObject<List<int>>(data["none"].ToJson());
        info.one = JsonMapper.ToObject < List<int> > (data["one"].ToJson());
        info.two = JsonMapper.ToObject < List<int> > (data["two"].ToJson());
        info.three = JsonMapper.ToObject < List<int> > (data["three"].ToJson());
        info.four = JsonMapper.ToObject < List<int> > (data["four"].ToJson());
        info.five = JsonMapper.ToObject < List<int> > (data["five"].ToJson());
        //info.propID = JsonMapper.ToObject<List<int>>(data["propID"].ToJson());

    }


    public void GetServentSkillData(string pathid, string pathdesc, out ServentSkillInfo info)
    {
        info = new ServentSkillInfo();
        //info.skillID = new Dictionary<string, uint>();
        //info.skillDesc = new Dictionary<string, string>();

        string str = File.ReadAllText(pathid);
        info.skillID = JsonMapper.ToObject<Dictionary<string, int[]>>(str);
        str = File.ReadAllText(pathdesc);
        info.skillDesc = JsonMapper.ToObject<Dictionary<string, string>>(str);
    }

    public void GetServentLevel(string pathlevel, out ServentLevel info)
    {
        info = new ServentLevel();
        string str = File.ReadAllText(pathlevel);
        JsonData data = JsonMapper.ToObject(str);
        info.Levels = JsonMapper.ToObject<List<int>>(data["Level"].ToJson());
    }

    public void GetPropInfoData(string path, out Dictionary<string, PropInfo> info)
    {
        Byte[] bytes = File.ReadAllBytes(path);
        string str = UTF8Encoding.UTF8.GetString(bytes);
        //FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        //Byte[] bytes = new Byte[stream.Length];
        //int count = stream.Read(bytes, 0, (int)stream.Length);
        //stream.Dispose();
        //string str = UTF8Encoding.UTF8.GetString(bytes);
        info = JsonMapper.ToObject<Dictionary<string, PropInfo>>(str);    
    }

    public void GetPropCfgData(string path, out Dictionary<string, PropCfg> info)
    {
        string str = File.ReadAllText(path);
        info = JsonMapper.ToObject<Dictionary<string, PropCfg>>(str);
    }

}
