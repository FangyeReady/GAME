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
        if (GUI.Button(new Rect(10, 10, 200, 100), "Auto"))
        {
            fileNames = Directory.GetFiles(autopath, "*.ab");
            foreach (var item in fileNames)
            {
                Debug.Log("Auto:" + Path.GetFileNameWithoutExtension(item));
                AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(item);
                AssetBundle bundle = assetBundleCreateRequest.assetBundle;
                Object @object = bundle.LoadAsset(Path.GetFileNameWithoutExtension(item));
                
                GameObject obj = Instantiate(@object, Vector3.zero, Quaternion.identity) as GameObject;
                //重新赋值shader
                ResetShader(obj);
            }
        }

        if (GUI.Button(new Rect(10, 120, 200, 100), "Other"))
        {
            fileNames = Directory.GetFiles(otherpath, "*.ab");       
            foreach (var item in fileNames)
            {
                if (Path.GetFileNameWithoutExtension(item) == "materaial")
                {
                    AssetBundleCreateRequest _ssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(item);
                    AssetBundle _bundle = _ssetBundleCreateRequest.assetBundle;
                    _bundle.LoadAllAssets();
                }  
            }

            foreach (var item in fileNames)
            {
                if (Path.GetFileNameWithoutExtension(item) == "materaial")
                {
                    continue;
                }
                Debug.Log("Other:" + Path.GetFileNameWithoutExtension(item));
                AssetBundleCreateRequest _ssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(item);
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

        }
    }


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
