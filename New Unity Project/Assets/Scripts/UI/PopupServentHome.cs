using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupServentHome : PopupBase {

    public Transform headParent;
    public GameObject headItem;

    public Image headPic;
    public Text Name;
    public Text SkillDesc;
    public Text Level;
    public Text Loyal;
    public Text expNow;
    public Text expAll;

    //从配置表里面读
    public Text serAll;
    public Text serHave;

    private List<GameObject> masks = new List<GameObject>();


    protected override void Init()
    {
        base.Init();
        CreatItems();
    }

    private void CreatItems()
    {
        var list = Player.Instance.PlayerInfos.Servent;
        for (int i = 0; i < list.Count; i++)
        {
            GameObject obj = Instantiate(headItem, headParent);
            obj.SetActive(true);
            obj.name = i.ToString();
            ServentHeadItem item = obj.GetComponent<ServentHeadItem>();
            item.Init(list[i]); 
            masks.Add(item.mask);
        }

        RefreshChooseState(0, list[0].ID);
    }

    public void RefreshChooseState(int index, int serID)
    {
        for (int i = 0; i < masks.Count; i++)
        {
            masks[i].SetActive( i == index);
        }

        ServentInfo servent = Player.Instance.PlayerInfos.Servent.Find(ser=>ser.ID == serID);
        if (servent != null)
        {
            InitInfo(servent);
        }
    }

    private void InitInfo(ServentInfo info)
    {
        Name.text = info.Name;
        Level.text = info.Level.ToString();
        Loyal.text = info.Loyal.ToString();
        SkillDesc.text = info.Desc.ToString();
        serHave.text = Player.Instance.PlayerInfos.Servent.Count.ToString();
        serAll.text = GameManager.Instance.GetServentCount().ToString();

        ResourcesLoader.Instance.SetSprite(StaticData.HEAD_ICON_PATH, "list" + info.ID.ToString(), sp => headPic.sprite = sp);
    }
}
