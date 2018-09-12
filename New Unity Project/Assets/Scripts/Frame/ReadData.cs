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
            if (serventInfo.ID == "0")
            {
                serventInfo.ID = UnityEngine.Random.Range(4001, 4126).ToString();//待优化
                //还需要对serventinfo进行填充
                //to do..
            }
            info.Servent.Add(serventInfo);
        }
        //存储数据
        Player.Instance.RefreshData();
    }


    public void SavePlayerData(string path, string data)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        //File.WriteAllText(path, data);
        FileStream fileStream = new FileStream(path, FileMode.CreateNew);
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        fileStream.Write(bytes,0,bytes.Length);
        fileStream.Close();
    }

    
}
