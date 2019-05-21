using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpriteDicing;

public class ChangeRolePic : MonoBehaviour {

    public DicedSpriteRenderer renderer;
    public DicedSpriteAtlas atlas;
    private int index = 0;
	// Use this for initialization
	void Start () {
        renderer = this.GetComponent<DicedSpriteRenderer>();
	}


    private void OnGUI()
    {
        if (GUI.Button(new Rect(50,50, 200, 100), "Click"))
        {
            index++;
            if (index >= atlas.SpritesCount)
            {
                index = 0;
            }
            renderer.SetDicedSprite(atlas.GetSprite(index.ToString()));
        }
    }


}
