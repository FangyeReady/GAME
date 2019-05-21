using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Conditional_Demo : MonoBehaviour {

    public GUIContent content;
    private void Start()
    {
        
    }

    [Conditional("CAN_USE")]
    void FuncA()
    {
        Utilty.Log("can use");
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(20,20, 250, 100), content))
        {
            FuncA();
        }
    }
}
