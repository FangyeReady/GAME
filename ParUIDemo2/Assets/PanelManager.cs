using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;


public class PoolManager
{
    #region 单例
    private PoolManager() { }
    private static PoolManager _instance;
    public static PoolManager Instance
    {
        get {
            if (null == _instance)
            {
                _instance = new PoolManager();
            }
            return _instance;
        }
    }
    #endregion


    private Dictionary<string, Pool> mPoolContainers = new Dictionary<string, Pool>();


    public Pool GetPool(PoolBuilder builder)
    {
        if (mPoolContainers.ContainsKey(builder.poolName))
        {
            return mPoolContainers[builder.poolName];
        }

        Pool tpool  = builder.CreatePool();
        mPoolContainers.Add(builder.poolName, tpool);

        return tpool;
    }

    //PS 注意程序结束时，要统一清空

}

public abstract class PoolBuilder
{
    protected Pool mPool;
    public string poolName;
    public abstract Pool CreatePool();
}

public class Pool
{
    private Stack<GameObject> itemContainer = new Stack<GameObject>();
    private GameObject prefab;
    private System.Type type;
    private const int AddCount = 10;

    public Pool(string prefabPath, System.Type type)
    {
        prefab = Resources.Load<GameObject>(prefabPath);
        this.type = type;
    }

    /// <summary>
    /// 取得对象
    /// </summary>
    /// <returns></returns>
    public GameObject GetItem()
    {
        if (itemContainer.Count == 0)
        {
            CreateItems();
        }
        return itemContainer.Pop();
    }

    /// <summary>
    /// 回收
    /// </summary>
    /// <param name="obj"></param>
    public void RecycleObj(GameObject obj)
    {
        GameObject item = obj as GameObject;
        ResetItem(item);
        itemContainer.Push(obj);
    }

    /// <summary>
    /// 创建对象
    /// </summary>
    public void CreateItems()
    {
        for (int i = 0; i < AddCount; i++)
        {   
            GameObject item = GameObject.Instantiate(prefab);
            item.gameObject.name = type.FullName;
            item.AddComponent(type);
            ResetItem(item);
            itemContainer.Push(item);
        }
    }

    private void ResetItem( GameObject item)
    {
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale = Vector3.one;
        item.SetActive(false);
    }
}



public class Pool_LabelItem_Builder : PoolBuilder
{

    public Pool_LabelItem_Builder() { poolName = typeof(Pool_LabelItem_Builder).FullName; }


    public override Pool CreatePool()
    {
        mPool = new Pool("itemPrefab", typeof(Item_Label));
        return mPool;
    }

}

public class Item_Label : MonoBehaviour
{
    public bool isUsed = false;
    public UILabel iLabel;

    private void Awake()
    {
        iLabel = this.transform.Find("Label").GetComponent<UILabel>();
    }

    public void SetValue(string text)
    {
        iLabel.text = text;
    }
}

public class PanelManager : MonoBehaviour {

    private int nowIndex = 0;
    public int guankaIndex = 0;
    private int MaxObjCount = 10;
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
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        nowIndex = guankaIndex;

        Pool_LabelItem_Builder item_Builder = new Pool_LabelItem_Builder();
        Pool itemPool = PoolManager.Instance.GetPool(item_Builder);

        for (int i = 0; i < MaxObjCount; i++)
        {
            GameObject obj = itemPool.GetItem();//NGUITools.AddChild(grid.gameObject, itemPrefab);
            obj.transform.SetParent(grid.transform);
            obj.SetActive(true);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.GetComponent<Item_Label>().SetValue(i.ToString());
            pos.Add(obj);
            //uILabels.Add(pos[i].GetComponentInChildren<UILabel>());
            //uILabels[i].text = (nowIndex + i + 1).ToString();
        }
        grid.Reposition();
        scrollView.ResetPosition();
        //scrollView.onDragFinished = ResetDrag;
        //resetBtn.onClick.Add(new EventDelegate(ResetGridPos));        
        //Reset();
        ////centerOnChild.CenterOn(pos[43 + 2].transform);
        //stopwatch.Stop();
        //UnityEngine.Debug.LogError(stopwatch.Elapsed.ToString());
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
        //grid.Reposition();
        //scrollView.ResetPosition();
    }
}
