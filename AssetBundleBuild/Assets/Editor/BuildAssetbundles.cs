using UnityEngine;
using UnityEditor;
namespace TreeEditor
{
    public static class BuildAssetbundles
    {

        private static string prefabsPath = "Assets/Prefabs/";
        public static string abPath = "Assets/Assetbundles/";
        [MenuItem("Assets/Build")]
        private static void BuiltAsset()
        {
            AssetBundleBuild[] assetBundleBuildArray = new AssetBundleBuild[4];
            string[] assetsNames = { "image0.prefab", "image1.prefab", "image2.prefab" };

            for (int i = 0; i < assetBundleBuildArray.Length; i++)
            {
                if (i == 3)
                {
                    break;
                }
                string name = assetsNames[i].Replace(".prefab", string.Empty);
                assetBundleBuildArray[i].assetBundleName = name;
                assetBundleBuildArray[i].assetBundleVariant = "assetbundle";
                assetBundleBuildArray[i].assetNames = new string[1] { prefabsPath + assetsNames[i] };
            }
            assetBundleBuildArray[3].assetBundleName = "pic";
            assetBundleBuildArray[3].assetBundleVariant = "assetbundle";
            assetBundleBuildArray[3].assetNames = new string[2] { "Assets/Pic/stage_bg_1.jpg", "Assets/Pic/Materials/stage_bg_2.mat" };
            BuildPipeline.BuildAssetBundles("Assets/Assetbundles", assetBundleBuildArray, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget/*BuildTarget.StandaloneWindows64*/);
        }
    }
    
}

