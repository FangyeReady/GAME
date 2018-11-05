using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Object = UnityEngine.Object;


public class ResourcesLoader : AutoStaticInstance<ResourcesLoader> {

    private Dictionary<string, Object> resourcesCache = new Dictionary<string, Object>();
    private IEnumerator LoadResFromAB<T>(string name, Action<T> callback)where T:Object
    {
        if (!resourcesCache.ContainsKey(name))
        {
            //加载资源 
            yield return StartCoroutine(GetAssetBundle(name, (ab) =>
            {
                Object val = ab.LoadAsset(name);
                if (val != null)
                {
                    resourcesCache.Add(name, val);
                    T obj = val as T;
                    if (obj)
                    {
                        callback(obj);
                    }
                }
            })
            );
        }
        else
        {
            GetAssetFromCache<T>(name, callback);
        }
    }

    /// <summary>
    /// 从缓存中取得数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="callback"></param>
    private T GetAssetFromCache<T>(string name, Action<T> callback = null) where T : Object
    { 
        if (resourcesCache.ContainsKey(name))
        {
            T obj = resourcesCache[name] as T;
            if (obj != null && callback != null)
            {
                callback(obj); 
            }
            return obj;
        }
        else
        {
            LoggerM.LogError("未成功加载资源~:" + name);
            return null;
        }
    }

    /// <summary>
    /// 从AB中获取资源，此处的AB名和资源同名
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="callback"></param>
    public void GetRes<T>(string name, Action<T> callback = null)where T:Object
    {
        StartCoroutine(LoadResFromAB<T>(name, callback));
    }


    private IEnumerator GetAssetBundle ( string name, Action<AssetBundle> action)
    {
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/AssetBundles/" + name + ".ab");
        while (!request.isDone)
            yield return null;

        AssetBundle bundle = request.assetBundle;
        if (bundle != null)
        {
            action(bundle);
        }    
    }

    public void SetSprite(string path,string name, Action<Sprite> callBack)
    {
#if UNITY_EDITOR
        var sp = Resources.Load<Sprite>(path + name);
        if (sp != null)
        {
            callBack(sp);
        }
#else 
        StartCoroutine(LoadResources<Sprite>(name, callback));
#endif

    }

    /// <summary>
    /// 得到一般的资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public T GetSimpleRes<T>(string path, string name, Action<T> callback = null) where T : Object
    {
        Object val = Resources.Load( path + name);
        if (callback != null)
        {
            callback(val as T);
        }
        return val as T;
    }

    public override void Save()
    {
        throw new NotImplementedException();
    }
}
