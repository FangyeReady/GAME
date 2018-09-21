using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : AutoStaticInstance<GameManager> {

    private static float gameTime = 0.0f;
    public static float GameTime { get { return gameTime; } }

    private GameSettingInfo _gameSettingInfo;
    public GameSettingInfo GameSettingInfos { get { return _gameSettingInfo; } }


    private string path = StaticData.CONFIG_PATH + "GameSettingInfo.txt";

    public override void InitManager()
    {
        base.InitManager();
        InitGameTime();
        path = Application.dataPath + path;
        ReadData.Instance.GetGameSettingData(path, out _gameSettingInfo);
    }


    private void Start()
    {
        StaticUpdater.Instance.UpdateEvent += AddGameTime;
        StaticUpdater.Instance.UpdateEvent += EscGame;
    }

    float intervalTime = 1f;
    float incraseTime = 0f;
    private void AddGameTime()
    {
        gameTime += Time.deltaTime;
        incraseTime += Time.deltaTime;
        if (incraseTime >= intervalTime)
        {
            Player.Instance.SetGameTime(gameTime);
            incraseTime = 0f;
        }
        
    }

    private void EscGame()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Player.Instance.SetGameTime(gameTime);
            Player.Instance.RefreshData();
            this.RefreshData();
            Application.Quit();
            Debug.LogError("OnDestory~!  " + Time.time);
        }
    }

    private void InitGameTime()
    {
        string time = Player.Instance.PlayerInfos.TotalTime;
        int d_index = time.LastIndexOf('d');
        string day = time.Substring(0, d_index);
        int h_index = time.LastIndexOf('h');
        string h = time.Substring(d_index + 1, h_index - d_index - 1);
        int min_index = time.LastIndexOf('m');
        string m = time.Substring(h_index + 1, min_index - h_index - 1);
        int s_index = time.LastIndexOf('s');
        string s = time.Substring(min_index + 1, s_index - min_index - 1);


        int iday = int.Parse(day);
        int ih = int.Parse(h);
        int im = int.Parse(m);
        int ise = int.Parse(s);

        gameTime += iday * 24 * 60 * 60 + ih * 60 * 60 + im * 60 + ise;

    }


    private void RefreshData()
    {
        JsonData data = JsonMapper.ToJson(_gameSettingInfo);
        ReadData.Instance.SaveData(path, data.ToString());
    }

    public int GetServentCount()
    {
        return _gameSettingInfo.none.Count + _gameSettingInfo.one.Count + _gameSettingInfo.two.Count +
            _gameSettingInfo.three.Count + _gameSettingInfo.four.Count + _gameSettingInfo.five.Count;
    }
    
}
