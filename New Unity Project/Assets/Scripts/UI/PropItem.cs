using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PropItem : ObjBase {

    public Image img;
    public Text text;


    public void InitProp(string spname, int num)
    {
        ResourcesLoader.Instance.SetSprite(StaticData.UI_PROP, spname, (sp) =>
        {
            img.sprite = sp;
        });

        text.text = num.ToString();
    }
}
