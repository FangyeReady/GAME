using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;

[Hotfix]
public class UITest : MonoBehaviour {

    public Text NoticeText;

    private int _sx = 0;
    public int SX
    {
        set { _sx = value; }
        get { return _sx; }
    }

    private void Start()
    {

        Debug.Log("SX: " + SX.ToString());
        SetNotice();
        GZTest gZ = new GZTest(10);
        SX = 1;
        Debug.Log("SX: " + SX.ToString());
    }

    private void SetNotice()
    {
        NoticeText.text = "this is a C# str.";
    }

    private int Add(int a, int b)
    {
        return a + b;
    }
}
