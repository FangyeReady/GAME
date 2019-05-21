using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class blooddemo : MonoBehaviour {

    private Slider slider;
    public RectTransform[] bloodImage;
    private int curIndex = 2;

    private void Awake()
    {
        slider = this.GetComponent<Slider>();
        slider.value = 1f;
        slider.fillRect = bloodImage[curIndex--];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            slider.value -= 0.2f;
            if (slider.value <= 0f)
            {
                slider.fillRect = bloodImage[curIndex];
                slider.value = 1f;  
                if (curIndex - 1 >= 0)
                {
                    --curIndex;
                }
            }
        }
    }

}
