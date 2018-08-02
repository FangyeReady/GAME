using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uObject = UnityEngine.Object;


public class ResourcesLoader : AutoStaticInstance<ResourcesLoader> {

    private Dictionary<string, uObject> resourcesCache = new Dictionary<string, uObject>();
    public T LoadResources<T>(string path, string name)where T:Object
    {
        if (!resourcesCache.ContainsKey(name))
        {
            T val = Resources.Load<T>(path + name);
            if (val)
            {
                resourcesCache.Add(name, val);
                return resourcesCache[name] as T;
            }
            return null;
        }
        return resourcesCache[name] as T;
    }

}
