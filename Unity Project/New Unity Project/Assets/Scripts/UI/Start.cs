using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;

public class Start : ObjBase {

    public Slider EnterProgress;
    private Text progressText;

    public Button startButton;
    private Vector3 resetPos;

    protected override void PreInit()
    {
        base.PreInit();
        progressText = EnterProgress.transform.Find("Text").GetComponent<Text>();
        resetPos = new Vector3(startButton.transform.localPosition.x, -100, startButton.transform.localPosition.z);
        //StaticUpdater.Instance.UpdateEvent += RefeshProgress;

        StartCoroutine("InitData");
    }


    private void BeginToGame()
    {
        Utility.SwitchScene(StaticData.Scenes.Game);
    }

    private readonly int AllCount = 5;
    private float process = 0.0f;
    IEnumerator InitData()
    {
        Player.Instance.InitManager();
        GameManager.Instance.InitManager();
        UIManager.Instance.InitManager();
        ServentManager.Instance.InitManager();
        PropManager.Instance.InitManager();

        float loadingTime = 2f;
        float deltaTime = 0f;
        while (AllCount - process > 0.01f)
        {
            deltaTime += Time.deltaTime;
            process = Mathf.Lerp(process, AllCount, deltaTime / loadingTime);
            yield return null;

            EnterProgress.value = (process + 0.0f) / AllCount;
            progressText.text = (((process + 0.0f) / AllCount) * 100).ToString("f2");
            yield return null;
        }
        progressText.text = "100";
        //可以开始游戏的逻辑
        Begin();
        yield return new WaitForSeconds(0.1f);
    }

    private void Begin()
    {
        startButton.transform.localPosition = resetPos;
    }

    protected override void OnDisabled()
    {
        base.OnDisabled(); 
    }
}
