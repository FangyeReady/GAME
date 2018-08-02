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
        Debug.Log("---PreInit---");
    }

    protected virtual void Init()
    {
        Debug.Log("---Init---");
    }

    protected virtual void OnEnabled()
    {
        Debug.Log("---OnEnabled---");
    }

    protected virtual void OnDisabled()
    {
        Debug.Log("---OnDisabled---");
    }

    protected virtual void OnDestoryed()
    {
        Debug.Log("---OnDestoryed---");
    }


    /// <summary>
    /// 销毁或关闭用
    /// </summary>
    public virtual void UnLoad()
    {
        Debug.Log("---UnLoad---");
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
}
