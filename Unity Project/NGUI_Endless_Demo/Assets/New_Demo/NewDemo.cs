using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewDemo : MonoBehaviour {

    private class F
    {
        public virtual void Func()
        {
            Debug.Log("base function~!");
        }
    }

    private class S:F
    {
        public new void Func()
        {
            Debug.Log("son,s function~!");
        }

        public void Func2()
        {
            Debug.Log("func2!!!");
        }
    }

    private class SS : S
    {
        public new void Func() //override 此时会报错
        {
            Debug.Log("son,s son,s function~!");
        }
    }


	// Use this for initialization
	void Start () {
        F f = new S();
        f.Func();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
