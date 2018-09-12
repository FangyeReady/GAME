using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : AutoStaticInstance<Player> {

    private PlayerInfo _playerInfo;
    public PlayerInfo PlayerInfos
    {
        get { return _playerInfo; }
    }


    private string path = StaticData.CONFIG_PATH + "PlayerInfo.txt";


    public IEnumerator InitInfo()
    {
        path = Application.dataPath + path;
        ReadData.Instance.GetPlayerData(path, out _playerInfo);
        yield return null;
    }


    public void RefreshData()
    {
        JsonData data = JsonMapper.ToJson(_playerInfo);
        ReadData.Instance.SavePlayerData(path, data.ToString());
    }

    public void SetGameTime(float time)
    {
        string totaltime = string.Empty;
        int min = (int)time / 60;//分钟
        int h = min / 60; //小时
        int day = h / 24;//天


        //show
        int second = (int)time % 60;
        int min_show = min % 60;
        int h_show = h % 24;
        int day_show = h / 24;

        totaltime = string.Format("{0}d{1}h{2}m{3}s", day_show, h_show, min_show, second);

        _playerInfo.TotalTime = totaltime;

        LoggerM.LogError(totaltime);
    }


}
