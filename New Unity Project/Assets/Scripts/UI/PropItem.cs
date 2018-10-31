using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class PropItem : ObjBase, IPointerEnterHandler, IPointerExitHandler{

    public Image img;
    public Text text;

    private int num = 0;
    private string id;
    private PropInfo propInfo;
    private PropCfg propCfg;
    private PopupPropItemInfo iteminfo = null;
    private ServentInfo servent;

    public void InitProp(string id, int num)
    {
        ResourcesLoader.Instance.SetSprite(StaticData.UI_PROP, id, (sp) =>
        {
            img.sprite = sp;
            text.text = num.ToString();
        });
        this.id = id;
        this.num = num;
        propInfo = PropManager.Instance.GetPropInfoByID(this.id);
        propCfg = PropManager.Instance.GetPropCfgByID(this.id);     
    }

    public void SetTargetServent(ServentInfo servent)
    {
        this.servent = servent;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        iteminfo = UIManager.Instance.OpenWindow<PopupPropItemInfo>(window => window.InitInfo(propInfo.name, propInfo.usedesc, propInfo.price.ToString()));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (iteminfo != null)
        {
            iteminfo.Hide();
        }
    }

    private void UseItem()
    {
        this.num -= 1; 
        RefreshPropUI();
        PropManager.Instance.UsePropItem(propCfg, servent);
    }

    private void RefreshPropUI()
    {
        text.text = num.ToString();

        if (iteminfo != null)
        {
            iteminfo.Hide();
        }
        if (this.num <= 0)
        {
            var list = Player.Instance.PlayerInfos.propID;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].id == this.id)
                {
                    list.RemoveAt(i);
                    break;
                }
            }
            base.UnLoadObj();
        }
    }
}
