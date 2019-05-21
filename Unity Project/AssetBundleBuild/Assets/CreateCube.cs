using UnityEngine;

public class CreateCube : MonoBehaviour {

    public static string abPath = "Assets/Assetbundles/";
    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.A))
        {
            //无效
            AssetBundle ab = Resources.Load<AssetBundle>("image0.assetbundle");//AssetBundle.LoadFromFile(abPath + "Assetbundles");
            if (ab != null)
            {
                AssetBundleManifest info = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                var dependences = info.GetAllDependencies("image1.assetbundle");

                for (int i = 0; i < dependences.Length; i++)
                {
                    string deName = dependences[i];
                    Debug.Log(deName);
                    AssetBundle deAsset = AssetBundle.LoadFromFile(abPath + deName);
                }
                AssetBundle assetBundle = AssetBundle.LoadFromFile(abPath + "image1.assetbundle");
                GameObject asset = assetBundle.LoadAsset<GameObject>("image1");
                GameObject.Instantiate<GameObject>(asset);
                ab.Unload(false);
            }
            else
            {
                Debug.LogError("Faild");
            }
           
        }
	}
}
