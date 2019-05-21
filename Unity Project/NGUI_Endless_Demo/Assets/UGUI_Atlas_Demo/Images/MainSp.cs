using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainSp : MonoBehaviour
{


    AssetBundle assetbundle = null;
    void Start()
    {
        CreatImage(loadSprite("image0"));
        CreatImage(loadSprite("image1"));
    }

    private void CreatImage(Sprite sprite)
    {
        GameObject go = new GameObject(sprite.name);
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.parent = transform;
        go.transform.localScale = Vector3.one;
        Image image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.SetNativeSize();
    }

    private Sprite loadSprite(string spriteName)
    {
#if USE_ASSETBUNDLE
		if(assetbundle == null)
			assetbundle = AssetBundle.CreateFromFile(Application.streamingAssetsPath +"/Main.assetbundle");
				return assetbundle.Load(spriteName) as Sprite;
#else
        return Resources.Load<GameObject>("Sprite/" + spriteName).GetComponent<SpriteRenderer>().sprite;
#endif
    }


}
