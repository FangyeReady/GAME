using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogTest : MonoBehaviour {

    float timeInterval = 1f;
    float time = 0.0f;
	// Update is called once per frame
	void Update () {

        time += Time.deltaTime;
        if (time >= timeInterval)
        {
            Debug.Log("this is a Log Test");
            time = 0.0f;
        }
		
	}
}
