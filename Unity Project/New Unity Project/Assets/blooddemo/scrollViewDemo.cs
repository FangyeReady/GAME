using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class scrollViewDemo : MonoBehaviour {

    private Color[] colors = {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow
    };
    private ScrollRect scrollRect;
    private Vector3 offset = new Vector3(0, 45, 0);
    private Vector3 LastVector3 = new Vector3(0, 45, 0);
    private Vector3 targetPos;
    private int curIndex = 0;
    private int lastIndex = 3;
    private Vector3 lastPos, firstPos;
    
    public Image[] images;
	// Use this for initialization
	void Start () {
        scrollRect = this.GetComponent<ScrollRect>();
        ResetColor();
        Invoke("DelayInit", 0.1f);
	}

    void DelayInit()
    {
        firstPos = images[0].transform.localPosition;
        lastPos = images[3].transform.localPosition;
        //Debug.Log(firstPos + "--" + lastPos);
    }

	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.M))
        {
           StartCoroutine("MoveInSpeed", 50);
        }
	}

    /// <summary>
    /// 按指定时间,单个移动
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator MoveInTime(float time)
    {
        int i = 0;
        float useTime = 0.0f;
        Vector3 targetPos;
        while (i < images.Length)
        {
            targetPos = images[i].transform.localPosition + offset;
            while (useTime <= time)
            {
                useTime += Time.deltaTime;
                images[i].transform.localPosition = Vector3.Lerp(images[i].transform.localPosition, targetPos, useTime / time);
                yield return null;
                
            }
            ++i;
            useTime = 0.0f;
        }      
    }

    /// <summary>
    /// 按指定速度，同时移动
    /// </summary>
    /// <param name="speed"></param>
    /// <returns></returns>
    IEnumerator MoveInSpeed(int speed)
    {
        float startTime = Time.time;
        float length = Vector3.Distance(images[0].transform.localPosition, images[0].transform.localPosition + offset);
        float curLength = 0.0f; 
        while (curLength <= length)
        {
            float dis = (Time.time - startTime) * speed;
            for (int i = 0; i < images.Length; i++) 
            {
                float useTime = dis / length;
                Vector3 targetPOS = images[i].transform.localPosition + offset;
                images[i].transform.localPosition = Vector3.Lerp(images[i].transform.localPosition, targetPOS, useTime);
            }
            curLength += dis;
            yield return null;
        }
        //    
        images[curIndex].transform.localPosition = lastPos;//images[lastIndex].transform.localPosition - LastVector3;
        lastIndex = curIndex;
        if (curIndex == 3)
        {
            curIndex = 0;
        }
        else
        {
            curIndex += 1;
        }  
    }


    private void ResetColor()
    {
        for (int i = 0; i < images.Length; i++)
        {
            images[i].color = colors[i];
        }
    }
}
