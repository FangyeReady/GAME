using UnityEngine;
using System.Collections;

static public class shaco_ExtensionsUI
{
    static public string AutoUnload(this UnityEngine.Component target, string pathAssetBundle, bool unloadAllLoadedObjects = true)
    {
        return shaco.UIManager.SetAutoUnloadAssetBundle(target, pathAssetBundle, unloadAllLoadedObjects);
    }

    static public bool UnloadAssetBundle(this UnityEngine.Component target, string pathAssetBundle, bool unloadAllLoadedObjects = true)
    {
        var layerAttribute = target.GetType().GetAttribute<shaco.UILayerAttribute>();
        return shaco.HotUpdateDataCache.UnloadAssetBundle(pathAssetBundle, layerAttribute.multiVersionControlRelativePath, unloadAllLoadedObjects);
    } 
}