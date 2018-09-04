using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BuildAssetBundles {

    [MenuItem("Assets/Select Build")]
    static void BuildBundleWithSelect()
    {
        Object[] objects = Selection.objects;
        string[] names = new string[objects.Length];
        AssetBundleBuild[] builds = new AssetBundleBuild[objects.Length];
        for (int i = 0; i < objects.Length; i++)
        {
            names[i] = AssetDatabase.GetAssetPath(objects[i]);
            Debug.Log(names[i]);
            builds[i].assetBundleName = objects[i].name;
            builds[i].assetBundleVariant = "ab";
            builds[i].assetNames = new string[] { names[i]};
        }
        BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath + "/AssetBundles/", builds, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
        Debug.Log("Select Build Success~!");
    }
}
