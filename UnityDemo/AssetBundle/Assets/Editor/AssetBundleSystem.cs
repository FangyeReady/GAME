
using UnityEngine;
using System.IO;
using UnityEditor;

public class AssetBundleSystem 
{
    public static string path = Path.Combine(Application.streamingAssetsPath, "AssetBundle");

    [UnityEditor.MenuItem("AssetBundle/DoBundle")]
    static void DoBundle()
    {
        if(!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }

}
