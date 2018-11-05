using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageRotate : MonoBehaviour {

    private Image img;
    private float angle = 0.0f;

    [Range(0.1f, 1)]
    public float speed = 0.1f;
	
	void Start () {

        img = this.GetComponent<Image>();
	}
	
	void Update () {

        angle += Time.deltaTime * speed;
        this.gameObject.transform.Rotate(Vector3.forward, angle);

	}
}
