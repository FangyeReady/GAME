using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Start : ObjBase {

    private int AllCount = 1;
    private int count = 0;
    public Slider EnterProgress;
    private Text progressText;

    public Button startButton;
    private Vector3 resetPos;

    protected override void PreInit()
    {
        base.PreInit();
        progressText = EnterProgress.transform.Find("Text").GetComponent<Text>();
        resetPos = new Vector3(startButton.transform.localPosition.x, -100, startButton.transform.localPosition.z);
        StaticUpdater.Instance.UpdateEvent += RefeshProgress;

        StartCoroutine("InitData");
    }



    private void BeginToGame()
    {      
        Utility.SwitchScene(StaticData.Scenes.Game);
    }


    IEnumerator InitData()
    {
        yield return Player.Instance.InitInfo();
        ++count;
        yield return null;

        //可以开始游戏的逻辑
        LoggerM.Log(Time.unscaledTime.ToString() + "~~~");
        Invoke("Begin", 1f);
    }

    private void Begin()
    {
        startButton.transform.localPosition = resetPos;
        LoggerM.Log(Time.unscaledTime.ToString());
    }

    private void RefeshProgress()
    {
        EnterProgress.value = (count + 0.0f) / AllCount;
        progressText.text = (((count + 0.0f) / AllCount) * 100).ToString();
    }

    protected override void OnDisabled()
    {
        count = 0;
        StaticUpdater.Instance.UpdateEvent -= RefeshProgress;
        base.OnDisabled(); 
    }
}
