using UnityEngine;
using System.Collections.Generic;
using System;
using System.Diagnostics;

/// <summary>
/// 优化NGUI UIScrollView在数据量过多时卡顿，创建过多的GameObject对象，造成资源浪费
/// </summary>
public class UIScrollViewLoop : MonoBehaviour
{
    public enum ArrangementDir
    {
        Horizontal,
        Vertical,
    }

    public ArrangementDir arrangement = ArrangementDir.Horizontal;
    public Transform itemParent;
    public int maxPerLine = 0;
    public Vector2 itemGap = Vector2.one;

    public interface IItemObj
    {
        void OnShowItem(GameObject obj, object _data);
    }

    /// <summary>
    /// item对像一定要包含UIWidget组件
    /// </summary>
    public class LoopItemInfo
    {
        public GameObject itemObj = null;
        public UIWidget widget = null; //用来检测item是否被裁剪
        public int dataIndex = -1; //当前item在实际整个scrollview中的索引位置，即对应在数据列表中的索引
        public Vector2 gridPos = Vector2.zero; //在网格中的位置
    }

    UIScrollView scrollView;
    GameObject itemObj;
    List<object> datasList; //滑动列表中要显示的所有数据
    List<LoopItemInfo> itemsList; //当前创建的Item对象
    LoopItemInfo firstItem; //itemsList的第一个元素
    LoopItemInfo lastItem; //itemsList的最后一个元素

    public delegate void DelegateHandler(GameObject obj, object data);
    DelegateHandler OnItemInit;
    Queue<LoopItemInfo> itemLoop = new Queue<LoopItemInfo>(); //对象池队列,优化频繁的创建与销毁


    //MINE
    public UICenterOnChild centerOnChild;
    public GameObject itemPrefab;
    public UIButton resetBtn;
    public int maxObjCount = 100;
    private int guankaIndex = 0;
    //模拟每个关卡的信息集合
    private List<object> myDatas = new List<object>();
    /// <summary>
    /// 关卡数据结构体
    /// </summary>
    private class MyData
    {
        public MyData(string i = "")
        {
            index = i;
            color = Color.white;
        }
        public string index;
        public Color color;
    }

    /// <summary>
    /// 拖拽开始时调用
    /// </summary>
    private void DisableCenterOnChild()
    {
        centerOnChild.enabled = false;
    }
    /// <summary>
    /// 拖拽结束后回调
    /// </summary>
    private void ResetDrag()
    {
        centerOnChild.enabled = true;
       // scrollView.ResetPosition();
    }

    /// <summary>
    /// 利用数据信息
    /// </summary>
    class LoopItem : IItemObj
    {
        public void OnShowItem(GameObject obj, object _data)
        {
            string str = (_data as MyData).index;
            Color color = (_data as MyData).color;
            obj.name = str;
            obj.GetComponentInChildren<UILabel>().text = str;
            obj.GetComponent<UISprite>().color = color;
        }
    }
   
    private void Awake()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        for (int i = 0; i < maxObjCount; i++)
        {
            MyData data = new MyData();
            data.index = (i + 1).ToString();
            myDatas.Add(data);
        }
        Init(itemPrefab, myDatas, new LoopItem());
        scrollView.ResetPosition();
        scrollView.onDragFinished = ResetDrag;
        resetBtn.onClick.Add(new EventDelegate(BackToNowIndexGuanKa));
        stopwatch.Stop();
        UnityEngine.Debug.LogError(stopwatch.Elapsed.ToString());
    }

    /// <summary>
    /// 重置关卡信息
    /// </summary>
    private void ResetItemInfo()
    {
        for (int i = 0; i < datasList.Count; i++)
        {
            (datasList[i] as MyData).color = Color.white;
        }
    }

    /// <summary>
    /// 初始化ScrollView
    /// </summary>
    /// <param name="_itemObj"></param>
    /// <param name="datas"></param>
    /// <param name="IItem"></param>
    public void Init(GameObject _itemObj, List<object> datas, UIScrollViewLoop.IItemObj IItem)
    {
        scrollView = this.transform.GetComponentInChildren<UIScrollView>();
        if (_itemObj == null || scrollView == null || itemParent == null)
        {
            //Debug.LogError("有属性没有赋值");
            return;
        }

        //CleanChild(itemParent);
        itemObj = _itemObj;
        datasList = datas;
        itemsList = null;
        firstItem = null;
        lastItem = null;
        itemLoop.Clear();

        this.OnItemInit = IItem.OnShowItem;

        Validate();
    }

    void Update()
    {
        Validate();
    }

    void Validate()
    {
        if (datasList == null || datasList.Count == 0)
            return;

        if (itemsList == null || itemsList.Count == 0)
        {
            itemsList = new List<LoopItemInfo>();

            LoopItemInfo item = GetItemFromLoop();
            RefreshShowItemInfo(ref item, guankaIndex, datasList[guankaIndex]);
            firstItem = lastItem = item;
            itemsList.Add(item);
            SetCenterObj();
            scrollView.ResetPosition();
        }

        //
        if (scrollView.isDragging)
        {
            DisableCenterOnChild();
        }
        //模拟关卡调整
        if (Input.GetKeyDown(KeyCode.A))
        {
            SetCenterObj();
        }
        //

        bool all_invisible = true;
        foreach (LoopItemInfo item in itemsList)
        {
            if (item.widget.isVisible == true)
            {
                all_invisible = false;
            }
        }

        if (all_invisible == true)
            return;

        //检测前端是否要增减
        if (firstItem.widget.isVisible)
        {
            if (firstItem.dataIndex > 0)
            {
                LoopItemInfo item = GetItemFromLoop();
                //在前面添加一个item，初始化：数据索引、大小、位置、显示
                int index = firstItem.dataIndex - 1;
                AddToFront(firstItem, item, index, datasList[index]);
                firstItem = item;
                itemsList.Insert(0, item);
            }
        }
        else
        {
            //判断是否将它移除
            //条件：自身是不可见的；且它后一个item也是不可见的（或被被裁剪过半的）.
            //这有个隐含条件是itemsList.Count>=2.
            if (itemsList.Count >= 2 && itemsList[0].widget.isVisible == false && itemsList[1].widget.isVisible == false)
            {
                itemsList.Remove(firstItem);
                PutItemToLoop(firstItem);
                firstItem = itemsList[0];
            }
        }

        //检测后端是否要增减
        if (lastItem.widget.isVisible)
        {
            if (lastItem.dataIndex < datasList.Count - 1)
            {
                LoopItemInfo item = GetItemFromLoop();
                //在后面添加一个item，初始化：数据索引、大小、位置、显示
                int index = lastItem.dataIndex + 1;
                AddToBack(lastItem, item, index, datasList[index]);
                lastItem = item;
                itemsList.Add(item);
            }
        }
        else
        {
            //判断是否将它移除
            //条件：自身是不可见的；且它前一个item也是不可见的（或被被裁剪过半的）
            //这有个隐含条件是itemsList.Count>=2
            if (itemsList.Count >= 2 && itemsList[itemsList.Count - 1].widget.isVisible == false && itemsList[itemsList.Count - 2].widget.isVisible == false)
            {
                itemsList.Remove(lastItem);
                PutItemToLoop(lastItem);
                lastItem = itemsList[itemsList.Count - 1];
            }
        }
    }

    private void SetCenterObj()
    {
        ++guankaIndex;
        UnityEngine.Debug.LogError("IN--" + guankaIndex.ToString());
        if (!centerOnChild.isActiveAndEnabled)
        {
            centerOnChild.enabled = true;
        }


        Transform centerTransform = itemParent.Find((guankaIndex + 2).ToString());
        Transform preTransform = itemParent.Find((guankaIndex - 1).ToString());
        Transform guankaTransform = itemParent.Find(guankaIndex.ToString());
 
        if (guankaTransform != null)
        {
            ResetItemInfo();
            MyData data = datasList[guankaIndex - 1] as MyData;
            data.color = Color.red;
            guankaTransform.GetComponent<UISprite>().color = data.color;
           
            centerOnChild.CenterOn(centerTransform);
        }
        else
        {
            BackToNowIndexGuanKa();
        }

        if (preTransform != null)
        {
            MyData data = datasList[guankaIndex - 2] as MyData;
            data.color = Color.white;
            preTransform.GetComponent<UISprite>().color = data.color;
        } 

    }

    private void BackToNowIndexGuanKa()
    {
        //--guankaIndex;
        //// int nowIndex = int.Parse(centerOnChild.centeredObject.name);//centerOnChild.centeredObject.name;

        //int nowIndex = 30;
        ////UnityEngine.Debug.LogError(centerOnChild.centeredObject.name);
        //if (nowIndex > guankaIndex)
        //{
        //    //前
        //    LoopItemInfo item = GetItemFromLoop();
        //    //在前面添加一个item，初始化：数据索引、大小、位置、显示
        //    int index = guankaIndex - 1;
        //    UnityEngine.Debug.LogError("前--" + index.ToString());
        //    AddToFront(firstItem, item, index, datasList[index]);
        //    firstItem = item;
        //    itemsList.Insert(0, item);
        //    centerOnChild.CenterOn(firstItem.itemObj.transform);
        //}
        //else {
        //    UnityEngine.Debug.LogError("后");
        //    //后
        //    LoopItemInfo item = GetItemFromLoop();
        //    //在后面添加一个item，初始化：数据索引、大小、位置、显示
        //    int index = guankaIndex + 1;
        //    AddToBack(lastItem, item, index, datasList[index]);
        //    lastItem = item;
        //    itemsList.Add(item);
        //    centerOnChild.CenterOn(lastItem.itemObj.transform);
        //}
 
       
        UnityEngine.Debug.Log("1");
    }

    LoopItemInfo CreateItem()
    {
        GameObject go = NGUITools.AddChild(itemParent.gameObject, itemObj);
        LoopItemInfo item = new LoopItemInfo();
        item.itemObj = go;
        go.SetActive(true);
        if (go.transform.GetComponent<UIWidget>() != null)
        {
            item.widget = go.transform.GetComponent<UIWidget>();
        }
        else
        {
            UnityEngine.Debug.LogError("item 中没有找到 widget 对象.");
        }
        return item;
    }

    /// <summary>
    /// 刷新ScrollView中Item的信息
    /// </summary>
    /// <param name="item"></param>
    /// <param name="dataIndex"></param>
    /// <param name="data"></param>
    void RefreshShowItemInfo(ref LoopItemInfo item, int dataIndex, object data)
    {
        item.dataIndex = dataIndex;
        int gridx = 0;
        int gridy = 0;
        if (item.dataIndex == 0 || maxPerLine == 0)
        {
            gridx = 0;
            gridy = 0;
        }
        else
        {
            if (arrangement == ArrangementDir.Horizontal)
            {
                gridx = item.dataIndex % maxPerLine;
                gridy = item.dataIndex / maxPerLine;
            }
            else
            {
                gridx = item.dataIndex / maxPerLine;
                gridy = item.dataIndex % maxPerLine;
            }
        }
        item.gridPos = new Vector2(gridx, gridy);

        if (OnItemInit != null)
        {
            OnItemInit(item.itemObj, data);
        }
        item.itemObj.transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// 在itemsList前面补上一个item
    /// </summary>
    void AddToFront(LoopItemInfo priorItem, LoopItemInfo newItem, int newIndex, object newData)
    {
        RefreshShowItemInfo(ref newItem, newIndex, newData);
        //计算新item的位置
        if (maxPerLine > 1)
        {
            newItem.itemObj.transform.localPosition = new Vector3((newItem.widget.width + itemGap.x) * newItem.gridPos.x, (newItem.widget.height + itemGap.y) * newItem.gridPos.y * -1, 0f);
        }
        else
        {
            if (scrollView.movement == UIScrollView.Movement.Horizontal)
            {
                //float offsetX = priorItem.widget.width * 0.5f + itemGapX + newItem.widget.width * 0.5f;
                ////if (movement == ArrangeDirection.Right_to_Left)
                ////    offsetX *= -1f;
                //newItem.itemObj.transform.localPosition = priorItem.itemObj.transform.localPosition - new Vector3(offsetX, 0f, 0f);

                float offsetX = priorItem.widget.width * 0.5f + itemGap.x + newItem.widget.width * 0.5f;
                newItem.itemObj.transform.localPosition = priorItem.itemObj.transform.localPosition - new Vector3(offsetX, 0f, 0f);
            }
            else
            {
                //float offsetY = priorItem.widget.height * 0.5f + itemGapY + newItem.widget.height * 0.5f;
                ////if (movement == ArrangeDirection.Down_to_Up)
                ////    offsetY *= -1f;
                //newItem.itemObj.transform.localPosition = priorItem.itemObj.transform.localPosition + new Vector3(0f, offsetY, 0f);

                float offsetY = priorItem.widget.height * 0.5f + itemGap.y + newItem.widget.height * 0.5f;
                newItem.itemObj.transform.localPosition = priorItem.itemObj.transform.localPosition + new Vector3(0f, offsetY, 0f);
            }
        }
    }

    /// <summary>
    /// 在itemsList后面补上一个item
    /// </summary>
    void AddToBack(LoopItemInfo backItem, LoopItemInfo newItem, int newIndex, object newData)
    {
        RefreshShowItemInfo(ref newItem, newIndex, newData);
        //计算新item的位置
        if (maxPerLine > 1)
        {
            newItem.itemObj.transform.localPosition = new Vector3((newItem.widget.width + itemGap.x) * newItem.gridPos.x, (newItem.widget.height + itemGap.y) * newItem.gridPos.y * -1, 0f);
        }
        else
        {
            if (scrollView.movement == UIScrollView.Movement.Horizontal)
            {
                //float offsetX = backItem.widget.width * 0.5f + itemGapX + newItem.widget.width * 0.5f;
                ////if (movement == ArrangeDirection.Right_to_Left)
                ////    offsetX *= -1f;
                //newItem.itemObj.transform.localPosition = backItem.itemObj.transform.localPosition + new Vector3(offsetX, 0f, 0f);

                float offsetX = backItem.widget.width * 0.5f + itemGap.x + newItem.widget.width * 0.5f;
                newItem.itemObj.transform.localPosition = backItem.itemObj.transform.localPosition + new Vector3(offsetX, 0f, 0f);
            }
            else
            {
                //float offsetY = backItem.widget.height * 0.5f + itemGapY + newItem.widget.height * 0.5f;
                ////if (movement == ArrangeDirection.Down_to_Up)
                ////    offsetY *= -1f;
                //newItem.itemObj.transform.localPosition = backItem.itemObj.transform.localPosition - new Vector3(0f, offsetY, 0f);

                float offsetY = backItem.widget.height * 0.5f + itemGap.y + newItem.widget.height * 0.5f;
                newItem.itemObj.transform.localPosition = backItem.itemObj.transform.localPosition - new Vector3(0f, offsetY, 0f);
            }
        }
    }

    void CleanChild(Transform trans)
    {
        for (int i = 0; i < trans.childCount; i++)
        {
            GameObject.Destroy(trans.GetChild(i).gameObject);
        }
    }

    #region 对象池性能相关

    /// <summary>
    /// 从对象池中取一个item
    /// </summary>
    /// <returns></returns>
    LoopItemInfo GetItemFromLoop()
    {
        LoopItemInfo item;
        if (itemLoop.Count <= 0)
        {
            item = CreateItem();
        }
        else
        {
            item = itemLoop.Dequeue();
        }
        item.itemObj.SetActive(true);
        return item;
    }

    /// <summary>
    /// 将要移除的item放入对象池中，对象池数量现在为3，可自定义
    /// </summary>
    /// <param name="item"></param>
    void PutItemToLoop(LoopItemInfo item)
    {
        if (itemLoop.Count >= 3)
        {
            GameObject.Destroy(item.itemObj);
            return;
        }
        item.dataIndex = -1;
        item.gridPos = Vector2.one * -1;
        item.itemObj.SetActive(false);
        itemLoop.Enqueue(item);
    }

    #endregion

}


