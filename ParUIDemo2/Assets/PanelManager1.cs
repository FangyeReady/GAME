using UnityEngine;
using System.Collections.Generic;

public class PanelManager1 : MonoBehaviour {

    private int nowIndex = 0;
    public int guankaIndex = 0;
    public int MaxObjCount = 100;
    private List<GameObject> pos = new List<GameObject>();
    private List<UILabel> uILabels = new List<UILabel>();
    public UIScrollView scrollView;
    public UIGrid grid;
    public UICenterOnChild centerOnChild;
    public GameObject itemPrefab;
    public UIButton resetBtn;
	void Awake () {

        Init();    
	}

    private void Init()
    {
        nowIndex = guankaIndex;
        for (int i = 0; i < MaxObjCount; i++)
        {
            GameObject obj = NGUITools.AddChild(grid.gameObject, itemPrefab);
            obj.SetActive(true);
            pos.Add(obj);
            uILabels.Add(pos[i].GetComponentInChildren<UILabel>());
            uILabels[i].text = (nowIndex + i + 1).ToString();
        }
        scrollView.onDragFinished = ResetDrag;
        resetBtn.onClick.Add(new EventDelegate(ResetGridPos));
        
        Reset();
    }

    private void ResetGridPos()
    {
        centerOnChild.CenterOn(pos[nowIndex + 2].transform);
    }

    void Update () {

        if (scrollView.isDragging)
        {
            DisableCenterOnChild();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            ++guankaIndex;
            ++nowIndex;
            if (!centerOnChild.isActiveAndEnabled)
            {
                centerOnChild.enabled = true;
            }
            if (nowIndex >= pos.Count - 1)
            {
                Reset();
                return;
            }
            else if(nowIndex + 4 < pos.Count - 1)
            {
                centerOnChild.CenterOn(pos[nowIndex + 2].transform);
            }
            pos[nowIndex].GetComponent<UISprite>().color = Color.red;
            pos[nowIndex - 1].GetComponent<UISprite>().color = Color.white;
        }

	}

    private void Reset()
    {
        nowIndex = 0;
        for (int i = 0; i < uILabels.Count; i++)
        {
            uILabels[i].text = (guankaIndex + i + 1).ToString();
            if (nowIndex == i)
            {
                pos[nowIndex].GetComponent<UISprite>().color = Color.red;
            }
            else
            {
                pos[i].GetComponent<UISprite>().color = Color.white;
            }
        }  
        centerOnChild.CenterOn(pos[nowIndex + 2].transform);
    }

    private void DisableCenterOnChild()
    {
        centerOnChild.enabled = false;
    }
    private void ResetDrag()
    {
        DisableCenterOnChild();
        grid.Reposition();
        scrollView.ResetPosition();
    }
}
