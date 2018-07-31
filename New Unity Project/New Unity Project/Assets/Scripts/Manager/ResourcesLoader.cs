using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ResourcesLoader : AutoStaticInstance<ResourcesLoader> {

    private Dictionary<int, object> allResources = new Dictionary<int, object>();
    public T LoadResources<T>(string path)where T:Object
    {
        T val = Resources.Load<T>(path);
        return val;
    }

    private T FetchResources<T>(int hashCode) where T : Object
    {

        if (allResources.ContainsKey(hashCode))
        {
            return allResources[hashCode] as T;
        }
        return null;
    }
}
