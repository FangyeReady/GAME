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

        JsonData servent = JsonMapper.ToObject(data["Servent"].ToJson());
        
        for (int i = 0; i < servent.Count; i++)
        {
            ServentInfo serventInfo = JsonMapper.ToObject<ServentInfo>(servent[i].ToJson());
            if (serventInfo.ID.Equals(0))
            {
                serventInfo.ID = UnityEngine.Random.Range(GameManager.Instance.GameSettingInfos.serventMin,
                                                          GameManager.Instance.GameSettingInfos.serventMax);//待优化
                //还需要对serventinfo进行填充
                //to do..
            }
            info.Servent.Add(serventInfo);
        }
        //存储数据
        Player.Instance.RefreshData();
    }


    public void SaveData(string path, string data)
    {
        FileStream fileStream = new FileStream(path, FileMode.Create);
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        fileStream.Write(bytes,0,bytes.Length);
        fileStream.Close();
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
        info.propID = JsonMapper.ToObject<List<int>>(data["propID"].ToJson());
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

    public void GetPropInfoData(string path, out Dictionary<string, PropInfo> info)
    {
        string str = File.ReadAllText(path);
        info = JsonMapper.ToObject<Dictionary<string, PropInfo>>(str);
        
    }

    public void GetPropCfgData(string path, out Dictionary<string, PropCfg> info)
    {
        string str = File.ReadAllText(path);
        info = JsonMapper.ToObject<Dictionary<string, PropCfg>>(str);
    }

}
