using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIManager : AutoStaticInstance<UIManager> {

    private List<PopupBase> m_AllWindowLists = new List<PopupBase>();
   // private Dictionary<string, GameObject> m_AllWindowPrefabs = new Dictionary<string, GameObject>();

    private Transform mtransform;
    public Transform Parent_Main {
        get {
            if (mtransform == null)
            {
                mtransform = GameObject.FindGameObjectWithTag("Main_Canvas").transform;
            }
            return mtransform;
        }
    }


    private GameObject maskPrefab;


    public override void InitManager()
    {
        base.InitManager();
        maskPrefab = ResourcesLoader.Instance.GetSimpleRes<GameObject>(StaticData.POPUP_PATH,"mask");
    }

    public T OpenWindow<T>(Action<T> callBack = null) where T: PopupBase
    {
        Type type = typeof(T);
        string windowName = type.Name;
        PopupBase window = GetWindow<T>();
        if (window == null)
        {
            GameObject prefab = Resources.Load<GameObject>(StaticData.POPUP_PATH + windowName);//FetchWindowPrefab(windowName);
            GameObject result = Instantiate(prefab, Parent_Main);
            result.transform.localPosition = Vector3.zero;
            result.transform.localScale = Vector3.one;

            window = result.GetComponent(type) as PopupBase;
            window.EnableGameObject();
            window.ObjectName = windowName;

            if (window.HasMask)
            {
                SetMask(result.transform);
            }

            m_AllWindowLists.Add(window);
        }
        else
        {
            window.EnableGameObject();
        }
        if (callBack != null)
        {
            callBack(window as T);
        }

        RebuildSortOrder();
        return window as T;
    }

    private Dictionary<int, List<Canvas>> windowCanvas = new Dictionary<int, List<Canvas>>();
    private void RebuildSortOrder()
    {
        windowCanvas.Clear();
        for (int i = 0; i < m_AllWindowLists.Count; i++)
        {
            var canvas = m_AllWindowLists[i].gameObject.GetComponent<Canvas>();
            if (canvas != null)
            {
                if (!windowCanvas.ContainsKey(canvas.sortingLayerID))
                {
                    windowCanvas.Add(canvas.sortingLayerID, new List<Canvas>());
                }
                windowCanvas[canvas.sortingLayerID].Add(canvas);
            }
        }

        foreach (var item in windowCanvas.Values)
        {
            for (int i = 0; i < item.Count; i++)
            {
                item[i].overrideSorting = true;
                item[i].sortingOrder = i + 1;
            }
        }
    }

    /// <summary>
    /// 加载窗口预制
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private GameObject FetchWindowPrefab(string name)
    {
        GameObject obj = Resources.Load<GameObject>(StaticData.POPUP_PATH + name);
        return obj;
       // if (!m_AllWindowPrefabs.ContainsKey(name))
       // {
            //obj = Resources.Load<GameObject>(StaticData.POPUP_PATH + name);
            //if (obj != null)
            //{
            //    m_AllWindowPrefabs.Add(name, obj);
            //}
        //}
        //return m_AllWindowPrefabs[name];
    }

    //private PopupBase GetWindow(string name)
    //{
    //    return m_AllWindowLists.Find(window=>window.ObjectName == name);
    //}

    /// <summary>
    /// 移除某个指定窗口
    /// </summary>
    /// <param name="obj"></param>
    public void RemoveWindow(PopupBase obj)
    {
       bool issuccess = m_AllWindowLists.Remove(obj);
       LoggerM.Log("remove window: " + issuccess.ToString());
    }

    /// <summary>
    /// 当窗口启用时，得到该窗口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetWindow<T>() where T : PopupBase
    {
        string name = typeof(T).Name;
        PopupBase popup = m_AllWindowLists.Find(w => w.ObjectName == name);
        if (popup != null && popup.gameObject.activeSelf)
        {
            return popup.gameObject.GetComponent<T>();
        }
        return null;
    }

    public void SetMask(Transform par)
    {
        RectTransform rect =  Instantiate(maskPrefab, par).GetComponent<RectTransform>();
        rect.SetAsFirstSibling();
        rect.localPosition = Vector3.zero;
        rect.localScale = Vector3.one;
    }

    public override void Save()
    {
        throw new NotImplementedException();
    }
}
