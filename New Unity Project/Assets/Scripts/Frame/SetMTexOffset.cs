using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetMTexOffset : MonoBehaviour {

    private Image img;

	void Start () {
        img = this.GetComponent<Image>();
 
	}

    float ofx = 0.0f;
    float ofy = 0.0f;
    void Update () {

        ofx += Time.deltaTime * 0.15f;
        ofy += Time.deltaTime * 0.15f;
        img.materialForRendering.SetTextureOffset("_MainTex", new Vector2(ofx, ofy));

    }
}
