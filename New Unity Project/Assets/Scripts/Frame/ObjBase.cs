using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjBase : MonoBehaviour {



    /// <summary>
    /// 名字
    /// </summary>
    public string ObjectName
    {
        set;
        get;
    }

    //
    private void Awake()
    {
        PreInit();
    }
    private void Start()
    {
        Init();
    }


    private void OnEnable()
    {
        OnEnabled();
    }

    private void OnDisable()
    {
        OnDisabled();
    }

    private void OnDestroy()
    {
        OnDestoryed();
    }



    protected virtual void PreInit()
    {

    }

    protected virtual void Init()
    {

    }

    protected virtual void OnEnabled()
    {

    }

    protected virtual void OnDisabled()
    {

    }

    protected virtual void OnDestoryed()
    {

    }


    /// <summary>
    /// 销毁或关闭用
    /// </summary>
    public virtual void UnLoad()
    {

    }

    public void EnableGameObject()
    {
        if (this.gameObject != null)
        {
            this.gameObject.SetActive(true);
        }
    }

    public void DisableGameObject()
    {
        if (this.gameObject != null)
        {
            this.gameObject.SetActive(false);
        }
    }

    public void Close()
    {
        UIManager.Instance.RemoveWindow(this as PopupBase);
        Destroy(this.gameObject);
    }
}
