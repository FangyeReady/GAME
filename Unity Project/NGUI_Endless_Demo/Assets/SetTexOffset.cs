using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTexOffset : MonoBehaviour {

    private Material mat;
    private Vector2 offset;
    public float speed;
	// Use this for initialization
	void Start () {
        mat = gameObject.GetComponent<Renderer>().sharedMaterial;
        offset = mat.GetTextureOffset("_MainTex");

       // mat.renderQueue = 0;
        //mat.renderQueue += 9150;

       // Debug.LogError("Q:  " + gameObject.name + ">>>>>" + mat.renderQueue.ToString());
	}
	
	// Update is called once per frame
	void Update () {

        offset.x += speed * Time.deltaTime;
        mat.SetTextureOffset("_MainTex", offset);
        //Debug.LogError("Q:  " + gameObject.name + "----" + mat.renderQueue.ToString());
    }

}
