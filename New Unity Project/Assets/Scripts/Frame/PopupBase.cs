using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupBase : ObjBase {

    public virtual void OnClose()
    {
        //DisableGameObject();
        base.Close();
    }
}
