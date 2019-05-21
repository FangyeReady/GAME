using UnityEngine;
using System.Collections;

public static partial class Unity4xSupport
{
#if !UNITY_5_3_OR_NEWER
    static public Object LoadAsset(this AssetBundle assetBundle, string filename, System.Type type)
	{
        return assetBundle.Load(filename, type);
    }

    static public AssetBundleRequest LoadAssetAsync(this AssetBundle assetBundle, string filename, System.Type type)
    {
        return assetBundle.LoadAsync(filename, type);
    }

    static public Object[] LoadAllAssets(this AssetBundle assetBundle)
    {
        return assetBundle.LoadAll();
    }

    static public Object[] LoadAllAssets(this AssetBundle assetBundle, System.Type type)
    {
        return assetBundle.LoadAll(type);
    }
#endif
}