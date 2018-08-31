using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// 测试打包AB依赖和Shader加载问题
/// </summary>
public static class AssetBundle_Demo_Editor {

    static string outPath = Application.dataPath + "/AssetBundle_Demo/AssetBundles";

    [MenuItem("Assets/Auto Build")]
    static void BuildAssetBundleAuto()
    {
       BuildPipeline.BuildAssetBundles(outPath + "/Auto/", BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
       Debug.Log("build~! success~!");
    }

    [MenuItem("Assets/Other Build")]
    static void BuildAssetBundleOther()
    {
        AssetBundleBuild[] bundleBuild = new AssetBundleBuild[4];
        bundleBuild[0].assetBundleName = "materaial";
        bundleBuild[0].assetBundleVariant = "ab";
        bundleBuild[0].assetNames = new string[] {
            "Assets/AssetBundle_Demo/addtive.mat",
            "Assets/AssetBundle_Demo/belnd.mat"
        };
        bundleBuild[1].assetBundleName = "Cube";
        bundleBuild[1].assetBundleVariant = "ab";
        bundleBuild[1].assetNames = new string[] {
            "Assets/AssetBundle_Demo/Cube.prefab"
        };
        bundleBuild[2].assetBundleName = "Sphere";
        bundleBuild[2].assetBundleVariant = "ab";
        bundleBuild[2].assetNames = new string[] {
             "Assets/AssetBundle_Demo/Sphere.prefab"
        };
        bundleBuild[3].assetBundleName = "Capsule";
        bundleBuild[3].assetBundleVariant = "ab";
        bundleBuild[3].assetNames = new string[] {
             "Assets/AssetBundle_Demo/Capsule.prefab"
        };

        BuildPipeline.BuildAssetBundles(outPath + "/Other/", bundleBuild, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);

    }

    [MenuItem("Assets/Select Build")]
    static void BuildAssetBundleSelect()
    {
        Object[] objets =  Selection.objects;
        AssetBundleBuild[] assetBundles = new AssetBundleBuild[objets.Length];
        string[] paths = new string[objets.Length];
        for (int i = 0; i < paths.Length; i++)
        {
            paths[i] = AssetDatabase.GetAssetPath(objets[i]);
            assetBundles[i].assetBundleName = Path.GetFileNameWithoutExtension(paths[i]);
            assetBundles[i].assetBundleVariant = "ab";
            assetBundles[i].assetNames = new string[] { paths[i] };
        }

        BuildPipeline.BuildAssetBundles(outPath + "/Select/", assetBundles, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
        Debug.Log("build~! success~!");
    }
}
