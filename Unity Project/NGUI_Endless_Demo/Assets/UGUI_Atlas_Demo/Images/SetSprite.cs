using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetSprite : MonoBehaviour {

    public Image img;
    Sprite iSprite;

    void Start () {

        iSprite = Resources.Load<GameObject>("list4001").GetComponent<SpriteRenderer>().sprite;

	}
	
	
	void Update () {

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (iSprite != null)
            {
                img.sprite = iSprite;
            }
        }
	}
}
