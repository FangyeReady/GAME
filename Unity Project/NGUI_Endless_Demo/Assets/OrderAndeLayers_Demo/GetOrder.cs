using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetOrder : MonoBehaviour {

    private Image image;
    public int RenderQue = 0;

	// Use this for initialization
	void Start () {
        image = this.GetComponent<Image>();

       // Debug.LogError(this.gameObject.name + ">>>>>" + RenderQue);
        image.material.renderQueue = RenderQue;

    }
	
	// Update is called once per frame
	void Update () {

        //image.material.renderQueue = RenderQue;

        //Debug.LogError(this.gameObject.name + "-----" + image.material.renderQueue.ToString());
	}
}
