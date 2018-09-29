using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupServentHome : PopupBase {

    public Transform headParent;
    public GameObject headItem;

    public Transform cellParent;
    public GameObject cellPrefab;
    public GameObject propPrefab;

    public Image headPic;
    public Text Name;
    public Text SkillDesc;
    public Text Level;
    public Text Loyal;
    public Text expNow;
    public Text expAll;
    public Slider expSlider;

    //从配置表里面读
    public Text serAll;
    public Text serHave;
    public Text tire;
    public Text favorability;

    public Dropdown dropdown;

    private List<GameObject> masks = new List<GameObject>();
    private List<ServentHeadItem> heads = new List<ServentHeadItem>();
    private List<GameObject> gameObjects = new List<GameObject>();

    private ServentInfo curServent;

    protected override void Init()
    {
        base.Init();
        CreateItems();

        dropdown.onValueChanged.AddListener(SetValue);
        SetValue(dropdown.value);
    }

    private void CreateItems()
    {
        CreateHeadItems();
        CreateCellItems();
        CreatePropItems();

        StartCoroutine("UpdateServentLevel", 12000);
    }

    private void CreateHeadItems()
    {
        var list = Player.Instance.PlayerInfos.Servent;
        for (int i = 0; i < list.Count; i++)
        {
            GameObject obj = Instantiate(headItem, headParent);
            obj.SetActive(true);
            obj.name = i.ToString();
            gameObjects.Add(obj);

            ServentHeadItem item = obj.GetComponent<ServentHeadItem>();
            item.Init(list[i]);
            heads.Add(item);
            masks.Add(item.mask);
        }

        RefreshChooseState(0, list[0].ID);
    }

    private List<Transform> cellList = new List<Transform>();
    private void CreateCellItems()
    {
        int count = GameManager.Instance.GameSettingInfos.cellCount;
        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(cellPrefab, cellParent);
            obj.SetActive(true);
            cellList.Add(obj.transform);
        }
    }

    private void CreatePropItems()
    {
        //to do..
        var list = Player.Instance.PlayerInfos.propID;
        for (int i = 0; i < list.Count; i++)
        {
            var item = Instantiate(propPrefab, GetEmptyCell());
            item.SetActive(true);
            item.transform.localScale = Vector3.one;
            item.transform.localPosition = Vector3.zero;
            PropItem info = item.GetComponent<PropItem>();
            info.InitProp(list[i].id, list[i].num);
        }
    }

    private Transform GetEmptyCell()
    {
        for (int i = 0; i < cellList.Count; i++)
        {
            if (cellList[i].childCount == 0)
            {
                return cellList[i];
            }
        }
        return null;
    }


    public void RefreshChooseState(int index, int serID)
    {
        for (int i = 0; i < masks.Count; i++)
        {
            masks[i].SetActive( i == index);
        }
        ServentInfo servent = Player.Instance.PlayerInfos.Servent.Find(ser=>ser.ID == serID);
        if (servent != null)
        {
            curServent = servent;
            InitInfo(servent);
        }
    }

    private void InitInfo(ServentInfo info)
    {
        Name.text = info.Name;
        Level.text = info.Level.ToString();
        Loyal.text = info.Loyal.ToString();
        tire.text = info.tire.ToString();
        favorability.text = info.favorability.ToString();
        SkillDesc.text = info.Desc.ToString();
        serHave.text = Player.Instance.PlayerInfos.Servent.Count.ToString();
        serAll.text = GameManager.Instance.GetServentCount().ToString();

        ResourcesLoader.Instance.SetSprite(StaticData.HEAD_ICON_PATH, "list" + info.ID.ToString(), sp => headPic.sprite = sp);
    }

    private void SetValue(int val)
    {
        switch (val)
        {
            case 0:
                heads.Sort((a,b)=> {
                    int result = a.info.Level.CompareTo(b.info.Level);//根据level排序，result为1则从小到大，-1为从大到小
                    if(result == 0)//为0则相等
                    {
                        return -a.info.star.CompareTo(b.info.star);//同上
                    }
                    return -result;
                });
                SortItems();
                break;
            case 1:
                heads.Sort((a, b) =>
                {
                    int result = a.info.star.CompareTo(b.info.star);//根据level排序，result为1则从小到大，-1为从大到小
                    if (result == 0)//为0则相等
                    {
                        return -a.info.Level.CompareTo(b.info.Level);//同上
                    }
                    return -result;
                });
                SortItems();
                break;
            case 2:
                heads.Sort((a, b) =>
                {
                    int result = a.info.ID.CompareTo(b.info.ID);//根据level排序，result为1则从小到大，-1为从大到小
                    if (result == 0)//为0则相等
                    {
                        return -a.info.Level.CompareTo(b.info.Level);//同上
                    }
                    return -result;
                });
                SortItems();
                break;
            default:
                break;
        }
    }

    private void SortItems()
    {
        List<ServentInfo> infos = new List<ServentInfo>();
        for (int i = 0; i < heads.Count; i++)
        {
            infos.Add(heads[i].info);
        }
        for (int i = 0; i < gameObjects.Count; i++)
        {
            var item = gameObjects[i].GetComponent<ServentHeadItem>();
            item.Init(infos[i]);
        }
        RefreshChooseState(0, infos[0].ID);
    }


    private float fillSpeed = 0.02f;
    private IEnumerator UpdateServentLevel(int getExp)
    {
        float targetVal = 0.0f;
        int nextLevelNeedExp = ServentManager.Instance.GetServentLevelExp(curServent.Level);
        for (int i = getExp; i > 0; i -= nextLevelNeedExp)
        {
            targetVal = (i - nextLevelNeedExp >= 0) ? 1f : (i + 0.0f) / nextLevelNeedExp;
            while (targetVal - expSlider.value >= 0.01f)
            {
                if (targetVal == 1f)
                {
                    expSlider.value += fillSpeed;
                    curServent.nowexp += (int)(nextLevelNeedExp * fillSpeed);
                }
                else
                {
                    expSlider.value = Mathf.Lerp(expSlider.value, targetVal, Time.deltaTime);
                    curServent.nowexp = (int)(expSlider.value * nextLevelNeedExp);
                }
                expNow.text = curServent.nowexp.ToString();
                yield return null;
            }
            if (targetVal == 1f)
            {
                curServent.Level += 1;
                expSlider.value = 0.0f;
                curServent.nowexp = 0;
                expNow.text = "0";
                nextLevelNeedExp = ServentManager.Instance.GetServentLevelExp(curServent.Level);
            }
            expAll.text = nextLevelNeedExp.ToString();
            expNow.text = curServent.nowexp.ToString();
            Level.text = curServent.Level.ToString();
            yield return new WaitForSeconds(0.25f);
        }
    }

    private void OnCloseClick()
    {
        base.OnClose();
    }


}
