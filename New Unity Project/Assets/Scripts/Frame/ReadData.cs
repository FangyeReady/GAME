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
        string[] none = ((string)data["StarNone"]).Split('-');
        string[] one =  ((string)data["StarOne"]).Split('-');
        string[] two =   ((string)data["StarTwo"]).Split('-');
        string[] three = ((string)data["StarThree"]).Split('-');
        string[] four = ((string)data["StarFour"]).Split('-');
        string[] five = ((string)data["StarFive"]).Split('-');

        info.none.min = int.Parse(none[0]);
        info.none.max = int.Parse(none[1]);

        info.one.min = int.Parse(one[0]);
        info.one.max = int.Parse(one[1]);

        info.two.min = int.Parse(two[0]);
        info.two.max = int.Parse(two[1]);

        info.three.min = int.Parse(three[0]);
        info.three.max = int.Parse(three[1]);

        info.four.min = int.Parse(four[0]);
        info.four.max = int.Parse(four[1]);

        info.five.min = int.Parse(five[0]);
        info.five.max = int.Parse(five[1]);


    }

    
}
