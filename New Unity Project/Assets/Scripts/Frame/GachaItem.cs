using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaItem : MonoBehaviour {

    public GameObject star;
    public  Image img;
    public Transform parent;
    public GameObject newFlag;
    private ServentInfo info;

    public void Init(ServentInfo info)
    {
        for (int i = 0; i < info.star; i++)
        {
            GameObject obj = Instantiate(star, parent);
            obj.SetActive(true);
        }
        ResourcesLoader.Instance.SetSprite(StaticData.HEAD_ICON_PATH, "list" + info.ID.ToString(), (sp) => img.overrideSprite = sp);
        newFlag.SetActive(!Player.Instance.IsAlreadyHas(info.ID));
        this.info = info;
    }


    private void OnHeadClick()
    {
        UIManager.Instance.OpenWindow<PopupServentInfo>(window=>window.Init(this.info));
    }
}
