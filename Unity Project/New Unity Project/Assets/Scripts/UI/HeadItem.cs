using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadItem : MonoBehaviour {

    private void OnClick()
    {
        PopupPlayerInfo popup = UIManager.Instance.GetWindow<PopupPlayerInfo>();
        if (popup)
        {
            int id = int.Parse(this.gameObject.name); 
            popup.SetHeadPic(id);
        }
    }
}
