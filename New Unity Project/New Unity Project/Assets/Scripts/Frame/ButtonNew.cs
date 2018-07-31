using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonNew : Button {

    public SoundType sound = SoundType.Click;
    public GameObject target;
  
    public string MethodName = string.Empty;

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        Debug.Log("ButtonClick!");
        SoundManager.Instance.PlaySound(sound, StaticData.ClickA);//此处ID暂时不填，因为还没有做好资源准备

        //SendMessageUpwards(MethodName, this.gameObject, SendMessageOptions.DontRequireReceiver);
        target.SendMessage(MethodName, this.gameObject, SendMessageOptions.DontRequireReceiver);
    }
}
