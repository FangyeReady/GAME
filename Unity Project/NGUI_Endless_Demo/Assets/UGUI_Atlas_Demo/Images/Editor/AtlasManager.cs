using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
public class AtlasManager {

    [MenuItem("MyMenu/AtlasMaker")]
    static private void MakeAtlas()
    {
        string spriteDir = Application.dataPath + "/UGUI_Atlas_Demo/Images/Sprites";

        if (!Directory.Exists(spriteDir))
        {
            Directory.CreateDirectory(spriteDir);
        }

        DirectoryInfo rootDirInfo = new DirectoryInfo(Application.dataPath + "/UGUI_Atlas_Demo/Images/");
        foreach (DirectoryInfo dirInfo in rootDirInfo.GetDirectories())
        {
            foreach (FileInfo pngFile in dirInfo.GetFiles("*.jpg", SearchOption.AllDirectories))
            {
                string allPath = pngFile.FullName;
                string assetPath = allPath.Substring(allPath.IndexOf("Assets"));
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);//Resources.Load<Sprite>(assetPath);
                GameObject go = new GameObject(sprite.name);
                go.AddComponent<SpriteRenderer>().sprite = sprite;
                allPath = spriteDir + "/" + sprite.name + ".prefab";
                string prefabPath = allPath.Substring(allPath.IndexOf("Assets"));
                PrefabUtility.CreatePrefab(prefabPath, go);
                GameObject.DestroyImmediate(go);
            }
        }

        AssetDatabase.Refresh();
    }


    [MenuItem("MyMenu/Build Assetbundle")]
    static private void BuildAssetBundle()
    {
        string dir = Application.dataPath + "/StreamingAssets";

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        DirectoryInfo rootDirInfo = new DirectoryInfo(Application.dataPath + "/UGUI_Atlas_Demo/Images/");
        foreach (DirectoryInfo dirInfo in rootDirInfo.GetDirectories())
        {
            List<string> assets = new List<string>();
            string path = dir + "/" + dirInfo.Name + ".assetbundle";
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = dirInfo.Name;
            build.assetBundleVariant = ".assetbundle";
            foreach (FileInfo pngFile in dirInfo.GetFiles("*.jpg", SearchOption.AllDirectories))
            {
                string allPath = pngFile.FullName;
                string assetPath = allPath.Substring(allPath.IndexOf("Assets"));
                // assets.Add(AssetDatabase.LoadAssetAtPath<Sprite>(assetPath));
                assets.Add(assetPath);
            }

            build.assetNames = assets.ToArray();
            if (assets.Count > 0)
            {
                if (BuildPipeline.BuildAssetBundles(dir, new AssetBundleBuild[1] { build }, BuildAssetBundleOptions.ChunkBasedCompression, GetBuildTarget()))
                {
                    Debug.LogError("build success~!");
                }
            }
           
        }
    }
    static private BuildTarget GetBuildTarget()
    {
        BuildTarget target = BuildTarget.Android;
#if UNITY_STANDALONE
            target = BuildTarget.StandaloneWindows;
#elif UNITY_IPHONE
			target = BuildTarget.iPhone;
#elif UNITY_ANDROID
			target = BuildTarget.Android;
#endif
        return target;
    }
}
