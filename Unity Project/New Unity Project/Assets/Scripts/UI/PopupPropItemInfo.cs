using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PopupPropItemInfo : PopupBase {

    public Text propname;
    public Text desc;
    public Text price;

    public Image bg;

    protected override void PreInit()
    {
        base.PreInit();
        HasMask = false;
    }

    public void InitInfo(string name, string desc, string price)
    {
        float x = Input.mousePosition.x - Screen.width / 2 + bg.rectTransform.rect.width / 2;
        float y = Input.mousePosition.y - Screen.height / 2 - bg.rectTransform.rect.height / 2;
        Vector3 uiPos = new Vector3(x, y, 0.0f);
        this.gameObject.transform.localPosition = uiPos;

        propname.text = string.Format("<color=#00FF00>{0}</color>", name);
        this.desc.text = desc;
        this.price.text = string.Format("<color=#F9F900>{0} G</color>", price);
    }
}
