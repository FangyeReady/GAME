using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class AssetBundle_Demo : MonoBehaviour {

    private const string outpath = "Assets/AssetBundle_Demo/AssetBundles";
    private const string autopath = outpath + "/Auto/";
    private const string otherpath = outpath + "/Other/";
    private string[] fileNames;


    private void OnGUI()
    {
        //每个资源都包含了自己的shader
        //直接一股脑添加资源，然后重新赋值shader
        if (GUI.Button(new Rect(10, 10, 200, 100), "Auto"))
        {
            fileNames = Directory.GetFiles(autopath, "*.ab");
            foreach (var item in fileNames)
            {
                Debug.Log("Auto:" + Path.GetFileNameWithoutExtension(item));
                LoadAsset(item);
            }
        }

        //shader单独打包
        //先加载指定shader，再加载资源，然后重新赋值shader
        if (GUI.Button(new Rect(10, 120, 200, 100), "Other"))
        {
            fileNames = Directory.GetFiles(otherpath, "*.ab");       
            foreach (var item in fileNames)
            {
                if (Path.GetFileNameWithoutExtension(item) == "materaial")
                {
                    AssetBundleCreateRequest _ssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(item);
                    AssetBundle _bundle = _ssetBundleCreateRequest.assetBundle;
                    //_bundle.LoadAllAssets();
                }  
            }

            foreach (var item in fileNames)
            {
                if (Path.GetFileNameWithoutExtension(item) == "materaial")
                {
                    continue;
                }
                Debug.Log("Other:" + Path.GetFileNameWithoutExtension(item));
                LoadAssets(item);
            }

        }

        if (GUI.Button(new Rect(10, 240, 200, 100), "Select"))
        {
            string path = Application.dataPath + "/AssetBundle_Demo/AssetBundles/Select/";
            string[] abNames = new string[] {
                "capsule.ab","cube.ab","sphere.ab"
            };
            AssetBundle mainBundle = AssetBundle.LoadFromFile(path + "Select");
            AssetBundleManifest manifest = mainBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            for (int i = 0; i < abNames.Length; i++)
            {
                GetDependences(path, abNames[i], manifest);
                LoadAsset(path + abNames[i]);
            }

        }

    }

    /// <summary>
    /// 从AB中读取资源,一个AB可以有N个资源
    /// </summary>
    /// <param name="path"></param>
    private void LoadAssets(string path)
    {
        AssetBundleCreateRequest _ssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(path);
        AssetBundle _bundle = _ssetBundleCreateRequest.assetBundle;
        var objs = _bundle.GetAllAssetNames();
        foreach (var asset in objs)
        {
            string t = Path.GetFileName(asset);
            var obj = _bundle.LoadAsset(t);
            GameObject gameObject = Instantiate(obj, Vector3.zero, Quaternion.identity) as GameObject;
            //重新赋值shader
            ResetShader(obj);
        }
    }

    /// <summary>
    /// 从AB中读取资源,一个AB一个资源
    /// </summary>
    /// <param name="path">AB的路径,资源名同AB名</param>
    private void LoadAsset(string path)
    {
        AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(path);
        AssetBundle bundle = assetBundleCreateRequest.assetBundle;
        Object @object = bundle.LoadAsset(Path.GetFileNameWithoutExtension(path));
        GameObject obj = Instantiate(@object, Vector3.zero, Quaternion.identity) as GameObject;
        //重新赋值shader
        ResetShader(obj);
    }

    /// <summary>
    /// 读取该AB名的所有依赖
    /// </summary>
    /// <param name="path"></param>
    /// <param name="assetName"></param>
    /// <param name="manifest"></param>
    private static void GetDependences(string path, string abName, AssetBundleManifest manifest)
    {
        string[] dependensInfo = manifest.GetAllDependencies(abName);
        for (int j = 0; j < dependensInfo.Length; j++)
        {
            if (!IsLoaded(j, dependensInfo))
            {
                AssetBundleCreateRequest assetBundleRequest = AssetBundle.LoadFromFileAsync(path + dependensInfo[j]);
                AssetBundle it = assetBundleRequest.assetBundle;
            }
        }
    }


    /// <summary>
    /// 依赖是否已加载，防止重复加载
    /// </summary>
    /// <param name="j"></param>
    /// <param name="dependensInfo"></param>
    /// <returns></returns>
    private static bool IsLoaded(int j, string[] dependensInfo)
    {
        var bundles = AssetBundle.GetAllLoadedAssetBundles();
        foreach (var item in bundles)
        {
            if (item.name == dependensInfo[j])
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 重新赋值shader
    /// </summary>
    /// <param name="obj"></param>
    public void ResetShader(UnityEngine.Object obj)
    {
        List<Material> materials = new List<Material>();
        if (obj is Material)
        {
            Material m = obj as Material;
            materials.Add(m);
        }
        else if (obj is GameObject)
        {
            GameObject gameObject = obj as GameObject;

            //Renderer
            var renderers = gameObject.GetComponentsInChildren<Renderer>();
            if (renderers != null)
            {
                foreach (Renderer item in renderers)
                {
                    Material[] marry = item.sharedMaterials;
                    for (int i = 0; i < marry.Length; i++)
                    {
                        materials.Add(marry[i]);
                    }    
                }
            }

            //ParticalSystem
            var particalSystem = gameObject.GetComponentsInChildren<ParticleSystemRenderer>();
            if (particalSystem != null)
            {
                foreach (var item in particalSystem)
                {
                    Material[] marray = item.sharedMaterials;
                    for (int i = 0; i < marray.Length; i++)
                    {
                        materials.Add(marray[i]);
                    }
                }
            }

            //Graphic
            var graphs = gameObject.GetComponentsInChildren<Graphic>();
            if (null != graphs)
            {
                foreach (var item in graphs)
                    materials.Add(item.material);
            }

            //重新赋值
            for (int i = 0; i < materials.Count; i++)
            {
                Material m = materials[i];
                if (m == null)
                {
                    continue;
                }
                string name = m.shader.name;
                Shader shader = Shader.Find(name);
                if (shader != null)
                {
                    m.shader = shader;
                }
                else
                {
                    Debug.LogError("Missing shader: " + name);
                }
            }
        }
    }
}
