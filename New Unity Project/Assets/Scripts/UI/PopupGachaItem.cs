using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class PopupGachaItem : PopupBase {

    public GameObject[] jishuStarAry;

    public Transform parent;
    public GameObject oushuStarPrefab;

    public Image avatarPic;
    public Text name;

    private Action action;


    public void Init(ServentInfo info, Action callBack)
    {
        action = callBack;
        name.text = info.Name;
        ResourcesLoader.Instance.SetSprite("path", "avatar" + info.ID.ToString(), (sp) => avatarPic.overrideSprite = sp);
        bool isDouble = info.star % 2 == 0 ? true : false;
        if (isDouble)
        {
            for (int i = 0; i < info.star; i++)
            {
                GameObject st = Instantiate(oushuStarPrefab, parent);
                st.SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < info.star; i++)
            {
                jishuStarAry[i].SetActive(true);
            }
        }
    }

    public void PlayAni()
    {
        //LoggerM.LogError("PlayAni~!!");
    }



    private void OnConfirmClick()
    {
        if (action != null)
        {
            action();
        }
        UIManager.Instance.RemoveWindow(this);
        Destroy(this.gameObject);
    }
}
