using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpSlider : MonoBehaviour {

    public Slider slider;
    public Text curExpText;
    public Text levelUpText;
    public Text curLevelText;

    [SerializeField] float fillSpeed = 0.02f;
    [SerializeField] int nextLevelNeedExp = 50;
    [SerializeField] int expOffset = 15;
    [SerializeField] int level = 1;

	// Use this for initialization
	void Start () {
    // slider.onValueChanged.AddListener(CheckFillArea);

        curExpText.text = "0";
        levelUpText.text = nextLevelNeedExp.ToString();
        curLevelText.text = level.ToString();


        StartCoroutine(UpdateExpVal(1000));
	}	

    private void CheckFillArea(float val)
    {
        //Debug.Log(val.ToString());
        //if (val > 1.0f)
        //{
        //    slider.value = 0.0f;
        //}
        
    }

    private WaitForSeconds waitFor = new WaitForSeconds(0.25f);
    private IEnumerator UpdateExpVal(int getExp)
    {
        float targetVal = 0.0f;
        float curExpval = 0.0f;

        for (int i = getExp; i > 0; i -= nextLevelNeedExp)
        {
            targetVal = (i - nextLevelNeedExp >= 0) ? 1f : (i + 0.0f) / nextLevelNeedExp;
            nextLevelNeedExp += expOffset; 
            while (targetVal - slider.value >= 0.01f)
            {
                if (targetVal == 1f)
                {
                    slider.value += fillSpeed;
                    curExpval += nextLevelNeedExp * fillSpeed;
                }
                else
                {
                    slider.value = Mathf.Lerp(slider.value, targetVal, Time.deltaTime);
                    curExpval = slider.value * nextLevelNeedExp;
                }           
                int val = Mathf.RoundToInt(curExpval);
                curExpText.text = val.ToString();
                yield return null;
            } 
            if (targetVal == 1f)
            {
                level += 1;
                slider.value = 0.0f;
                curExpval = 0;
                curExpText.text = curExpval.ToString();
            }
            levelUpText.text = nextLevelNeedExp.ToString();
            curLevelText.text = level.ToString();
            yield return waitFor;
        }
    }

}
