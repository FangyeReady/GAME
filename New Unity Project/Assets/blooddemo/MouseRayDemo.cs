using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseRayDemo : MonoBehaviour {
    private Ray2D ray;
    private RaycastHit2D hit;
    private LayerMask targetLayerMask;
    private Image image;

	// Use this for initialization
	void Start () {
        targetLayerMask = 1 << LayerMask.NameToLayer("Test");
        image = this.GetComponent<Image>();

        Debug.LogError(targetLayerMask.value);
	}
	
	// Update is called once per frame
	void Update () {

        Debug.DrawLine(new Vector2(100, 100), this.transform.position, Color.red);
        hit = Physics2D.Linecast(new Vector2(100, 100), this.transform.position, targetLayerMask.value);
        if (hit)
        {
            var it = hit.transform.gameObject;
            image = it.GetComponent<Image>();
            image.color = Color.red;
        }
        else
        {
            Debug.Log("1");
        }
	}
}
