using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

public class AssetBundleManager : MonoBehaviour
{
    public static string path = Path.Combine(Application.streamingAssetsPath, "AssetBundle/");

    void Update()
    {
        print("edite mode~");
        if(Input.GetKeyDown(KeyCode.L))
        {
           GameObject obj = LoadObjFromAssetBundle("cubeab", "Cube") as GameObject;
           if( null != obj)
             Instantiate(obj);
        }

    }


    public static Object LoadObjFromAssetBundle(string AbName, string assetName)
    {
        AssetBundle ab = AssetBundle.LoadFromFile( path + AbName + ".assetbundle"); //此处加后缀是因为manifest是没有后缀的，如果不加unity就会去找manifest
        if (null != ab)
        {
            Object prefab = ab.LoadAsset(assetName);
            return prefab;
        }
        return null;
    }

    public IEnumerator LoadObjFromNetWork(string AbName)
    {
        var uri = new Uri(path + AbName + ".assetbundle");
        UnityWebRequest netRequest =  UnityWebRequestAssetBundle.GetAssetBundle (uri);
        yield return netRequest.SendWebRequest();
        AssetBundle ab = DownloadHandlerAssetBundle.GetContent(netRequest); 
    }
}
