using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class PropItem : ObjBase, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler,IDragHandler,IEndDragHandler{

    public Image img;
    public Text text;

    private int num = 0;
    private string id;
    private PropInfo propInfo;
    private PropCfg propCfg;
    private PopupPropItemInfo iteminfo = null;
    private ServentInfo servent;


    private Transform parent;
    private bool isDraging = false;


    protected override void Init()
    {
        base.Init();
        this.parent = this.transform.parent;
    }

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
        if (!isDraging)
        {
            this.num -= 1;
            RefreshPropUI();
            PropManager.Instance.UsePropItem(propCfg, servent);
        }
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDraging = true;
        this.transform.parent = UIManager.Instance.GetWindow<PopupServentHome>().transform;
        this.transform.localPosition = Vector3.zero;
        this.transform.localScale = Vector3.one;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float x = Input.mousePosition.x - Screen.width / 2;
        float y = Input.mousePosition.y - Screen.height / 2;
        Vector2 vector2 = new Vector2(x, y);
        this.transform.localPosition = vector2;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDraging = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        ChangeCell(other);
    }

    private void OnTriggerStay(Collider other)
    {
        ChangeCell(other);
    }

    private void ChangeCell(Collider other)
    {
        if (isDraging == false && other.CompareTag("cell"))
        {
            if (this.parent != null && !this.parent.Equals(other.transform))
            {
                LoggerM.Log("in~~");
                if (other.transform.childCount == 1)
                {
                    var item = other.transform.GetChild(0);
                    item.transform.SetParent(this.parent);
                    item.transform.localPosition = Vector3.zero;
                    item.transform.localScale = Vector3.one;
                }
                this.parent = other.transform;
                this.gameObject.transform.SetParent(this.parent);
                this.transform.localPosition = Vector3.zero;
                this.transform.localScale = Vector3.one;
            }
        }
    }
}
