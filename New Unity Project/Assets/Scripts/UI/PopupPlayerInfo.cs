using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupPlayerInfo : PopupBase {

    public Text playerName;
    public Text level;
    public Text coin;
    public Text maxServentNum;
    public Text serventNum;
    public Text totalTime;
    public RectTransform rect;
    public Image head;
    public GameObject prefab;


    protected override void Init()
    {
        base.Init();
        PlayerInfo playerInfo = Player.Instance.PlayerInfos;
        playerName.text = playerInfo.Name;
        level.text = playerInfo.BusinessLevel.ToString();
        coin.text = playerInfo.Coin.ToString();
        maxServentNum.text = playerInfo.MaxServentNum.ToString();
        serventNum.text = playerInfo.Servent.Count.ToString();
        totalTime.text = playerInfo.TotalTime.ToString();
        ResourcesLoader.Instance.SetSprite(StaticData.HEAD_ICON_PATH, "list" + GameManager.Instance.GameSettingInfos.HeadPic.ToString(), (sp) => head.overrideSprite = sp);
        CreateItems(playerInfo.Servent);

        StaticUpdater.Instance.UpdateEvent += UpdateTotalTime;
    }


    private void OnHeadClick()
    {
        LoggerM.LogError("OnHeadClick");
        UIManager.Instance.OpenWindow<PopupChooseHead>();
    }

    private void OnCloseClick()
    {
        base.OnClose();
    }

    private void CreateItems(List<ServentInfo> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Image img = Instantiate(prefab, rect).GetComponent<Image>();
            if (img != null)
            {
                img.gameObject.SetActive(true);
                ResourcesLoader.Instance.SetSprite(StaticData.HEAD_ICON_PATH, "list" + list[i].ID.ToString(), (sp) => img.overrideSprite = sp);
            }
        }
    }


    private void UpdateTotalTime()
    {
        totalTime.text = Player.Instance.PlayerInfos.TotalTime.ToString();
    }

    public void SetHeadPic(int id)
    {
        ResourcesLoader.Instance.SetSprite(StaticData.HEAD_ICON_PATH, "list" + id.ToString(), (sp) => head.overrideSprite = sp);
        GameManager.Instance.GameSettingInfos.HeadPic = id;
    }

    protected override void OnDestoryed()
    {
        base.OnDestoryed();
        StaticUpdater.Instance.UpdateEvent -= UpdateTotalTime;
    }



}
