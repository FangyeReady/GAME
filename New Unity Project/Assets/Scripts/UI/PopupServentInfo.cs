using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupServentInfo : PopupBase {

    public Image head;
    public GameObject star;
    public Transform parent;
    public Text name;
    public Text level;
    public Text loyal;
    public Text skillDesc;



    public void Init(ServentInfo info)
    {
        ResourcesLoader.Instance.SetSprite(StaticData.HEAD_ICON_PATH, "list" + info.ID.ToString(), (sp) => head.overrideSprite = sp);
        for (int i = 0; i < info.star; i++)
        {
            GameObject obj = Instantiate(star, parent);
            obj.SetActive(true);
        }

        name.text = info.Name;
        level.text = info.Level.ToString();
        loyal.text = info.Loyal.ToString();
        skillDesc.text = info.Desc;
    }


    private void OnCloseClick()
    {
        UIManager.Instance.RemoveWindow(this);
        Destroy(this.gameObject);
    }

}
