using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaItem : MonoBehaviour {

    public GameObject star;
    public  Image img;
    public Transform parent;

    public void Init(int starNum, int id)
    {
        for (int i = 0; i < starNum; i++)
        {
            GameObject obj = Instantiate(star, parent);
            obj.SetActive(true);
        }
        ResourcesLoader.Instance.SetSprite(StaticData.HEAD_ICON_PATH, "list" + id.ToString(), (sp) => img.overrideSprite = sp);
    }
}
