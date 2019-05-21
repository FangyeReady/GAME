using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupBase : ObjBase {

    public virtual void OnClose()
    {
        //DisableGameObject();
        Close();
    }

    public void Close()
    {
        UIManager.Instance.RemoveWindow(this as PopupBase);
        Destroy(this.gameObject);
    }

    public bool HasMask = true;
}
