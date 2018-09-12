using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : AutoStaticInstance<GameManager> {

    private static float gameTime = 0.0f;
    public static float GameTime { get { return gameTime; } }

    public IEnumerator Init()
    {
        CalcGameTime();
        yield return null;
    }

    private void Start()
    {
        StaticUpdater.Instance.UpdateEvent += AddGameTime;
        StaticUpdater.Instance.UpdateEvent += EscGame;
    }

    private void AddGameTime()
    {
        gameTime += Time.deltaTime;
    }

    private void EscGame()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Player.Instance.SetGameTime(gameTime);
            Player.Instance.RefreshData();
            Debug.LogError("OnDestory~!  " + Time.time);
        }
    }

    private void CalcGameTime()
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

    
}
