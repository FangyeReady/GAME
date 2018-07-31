using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIManager : AutoStaticInstance<UIManager> {

    private List<PopupBase> m_AllWindowLists = new List<PopupBase>();
    private Dictionary<string, GameObject> m_AllWindowPrefabs = new Dictionary<string, GameObject>();


    public T OpenWindow<T>(Type type, Action<T> callBack = null) where T: PopupBase
    {
        string windowName = typeof(Type).Name;
        PopupBase window = GetWindow(name);
        if (window == null)
        {
            GameObject prefab = FetchWindowPrefab(name);
            GameObject result = Instantiate(prefab);
            window = result.GetComponent(type) as PopupBase;
            window.EnableGameObject();
            window.ObjectName = windowName;
            m_AllWindowLists.Add(window);
        }
        return window as T;
    }

    /// <summary>
    /// 加载窗口预制
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private GameObject FetchWindowPrefab(string name)
    {
        GameObject obj = null;
        if (!m_AllWindowPrefabs.ContainsKey(name))
        {
            obj = Resources.Load<GameObject>(StaticData.POPUP_PATH + name);
            if (obj != null)
            {
                m_AllWindowPrefabs.Add(name, obj);
            }
        }  
        return obj;
    }

    private PopupBase GetWindow(string name)
    {
        return m_AllWindowLists.Find(window=>window.ObjectName == name);
    }
}
