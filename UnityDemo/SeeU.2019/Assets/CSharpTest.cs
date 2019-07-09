using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSharpTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int a = int.MaxValue;
        int b = a + 1;

        print("VAL:" + (a + b) ); //  a,b同符号（+）， 然后 相加结果  符号相反（-） 则内存溢出
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
