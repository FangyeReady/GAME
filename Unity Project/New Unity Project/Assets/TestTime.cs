using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTime : MonoBehaviour {

    float lastTime = 0;
	// Use this for initialization
	void Start () {
        lastTime = Time.time;

        Debug.Log(lastTime);

      

		
	}
	
	// Update is called once per frame
	void Update () {

        lastTime = Time.time;

        Debug.Log(lastTime);

    }
}
