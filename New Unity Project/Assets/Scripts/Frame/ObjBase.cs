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

    public void Hide()
    {
        if (this.gameObject != null)
        {
            this.gameObject.transform.localPosition = new Vector3(10000, 0, 0);
        }
    }

    public void Show()
    {
        if (this.gameObject != null)
        {
            this.gameObject.transform.localPosition = Vector3.zero;
        }
    }

    public void UnLoadObj()
    {
        if (this.gameObject != null)
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}
