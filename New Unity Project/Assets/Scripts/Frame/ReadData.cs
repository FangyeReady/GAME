using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;

/// <summary>
/// 读取配置表
/// </summary>
public class ReadData :AutoStaticInstance<ReadData> {

    public void GetPlayerData(string path, out PlayerInfo info)
    {
        string str = File.ReadAllText(path);
        JsonData data = JsonMapper.ToObject(str);

        info = new PlayerInfo();
        info.name = data["Player"]["name"].ToString();
        info.level = int.Parse( data["Player"]["level"].ToString() );
        info.allexp = int.Parse( data["Player"]["allexp"].ToString() );
        var team = data["Player"]["team"];
        info.team = new int[team.Count];
        for (int i = 0; i < team.Count; i++)
        {
            info.team[i] = int.Parse(team[i].ToString());
        }             
    }
}
