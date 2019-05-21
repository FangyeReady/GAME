using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

//----------------------
//this scprit only can use on UGUI Unity4.6+
//----------------------
namespace shaco
{
    [RequireComponent(typeof(shaco.ScrollRectEx))]
    public partial class ListView : MonoBehaviour
    {
        public int Count
        {
            get { return _listItems.Count; }
        }

        //主滚动方向
        [HideInInspector]
        [SerializeField]
        private shaco.Direction scrollDirection = shaco.Direction.Right;
        //每小组组件滚动方向
        [HideInInspector]
        [SerializeField]
        private shaco.Direction groupItemDirection = shaco.Direction.Automatic;

        //是否居中所有组建布局 
        [HideInInspector]
        public bool isCenterLayout = false;
        //是否开启Debug调试模式
        [HideInInspector]
        public bool openDebugMode = false;
        //是否自动刷新滚动区域大小
        [HideInInspector]
        public bool autoUpdateContentSize = true;
        //第一个组件的位置
        [HideInInspector]
        public Vector3 firstItemOffset = new Vector3(0, 0, 0);
        //组件间距
        [HideInInspector]
        public Vector3 itemMargin = new Vector3(0, 0, 0);
        //每小组组件间距
        [HideInInspector]
        public Vector3 groupItemMargin = new Vector3(0, 0, 0);
        //每小组组件数量
        [HideInInspector]
        [Range(1, 1000)]
        public int eachOfGroup = 1;
        //当滚动回弹的时候自动刷新组件数量倍数，如果<=0则不自动刷新
        //该参数建议不要超过当前ListView已有组件数量，否则会自动使用ListView当前组件数量替代该参数
        [HideInInspector]
        [Range(1, 1000)]
        public int autoUpdateItemCountWhenSpringback = 0;
        //当滚动回弹的时候自动刷新组建最小下标
        [HideInInspector]
        public int autoUpdateItemMinIndex = 0;
        //当滚动回弹的时候自动刷新组建最大下标
        [HideInInspector]
        public int autoUpdateItemMaxIndex = 9;
        //是否同时设置多个拖拽最大边界
        [HideInInspector]
        public bool isMultipleDragOutOfBoundsSet = false;
        //拖拽超过边界最大比率(0~1.0)，当超过一定距离(距离=组建宽或高*拖拽比率)后停止拖拽
        //当该值为1的时候不检查拖拽范围，默认使用ListView自带的回弹
        [HideInInspector]
        [Range(0, 1)]
        public float maxDragOutOfFrontBoundsRatio = 1;

        [HideInInspector]
        [Range(0, 1)]
        public float maxDragOutOfBehindBoundsRatio = 1;

        //前置箭头，当滚动达到最前端的时候自动会隐藏
        [HideInInspector]
        public GameObject frontArrow;
        //后置箭头，当滚动达到最后端的时候自动会隐藏
        [HideInInspector]
        public GameObject behindArrow;

        //当所有组建准备自动刷新前的时候回调
        //[参数: 本次准备刷新的组建起始下标, 本次准备刷新的组建终止下标]
        //返回值:
        //true: 会自动开始刷新组建
        //false: 停止自动刷新所有组建
        public System.Func<int, int, bool> onItemsPrepareAutoUpdateCallBack = null;
        //当组建准备自动刷新的时候回调(如果返回true则会自动刷新，返回false则本次停止自动刷新)
        //[参数: 准备添加的新组建下标, 准备添加的新组建对象]
        //返回值:
        //true: 会自动添加并刷新该组建
        //false: 停止自动刷新该组建和它之后的所有组建
        public System.Func<int, bool> onItemWillAutoUpdateCallBack = null;
        //当组件自动刷新的时候回调
        //[参数: 新的组建下标, 新的组建对象]
        public System.Action<int, GameObject> onItemAutoUpdateCallBack = null;
        //当所有组建自动刷新完毕的时候回调
        //[参数: 本次刷新成功的组建起始下标, 本次刷新成功的组建终止下标]
        public System.Action<int, int> onItemsDidAutoUpdateCallBack = null;
        //当组建拖拽超出边界需要回弹的时候回调(参数:接近最大回弹临界值的百分比，范围0~1.0)
        public System.Action<float> onItemsDragOutOfBoundsCallBack = null;
        //当ListView的布局刷新成功后回掉 
        public System.Action onLayoutUpdateCallBack = null;

        //组件模板
        [SerializeField]
        private GameObject _itemModel;

        private List<Item> _listItems = new List<Item>();
        private List<GameObject> _listItemCache = new List<GameObject>();
        private bool _isUpdateListviewDirty = true;
        private bool _willResumeHorizontalScroll = false;
        private bool _willResumeVerticalScroll = false;
        private bool _isCustomPauseScrolling = false;

        void Start()
        {
            CheckCompoment();
            if (_itemModel != null)
            {
                SetItemModel(_itemModel);
            }

            _prevDirection = scrollDirection;
            _prevfirstItemOffset = firstItemOffset;
            _previtemMargin = itemMargin;
            _prevgroupItemMargin = groupItemMargin;
            _prevopenDebugMode = openDebugMode;
            _preveachOfGroup = eachOfGroup;
            _prevgroupItemDirection = groupItemDirection;
            _oldContentSize = new Vector3(_scrollRectContent.content.rect.width, _scrollRectContent.content.rect.height);

            CheckOutOfBoundsChanged();
        }

        void Reset()
        {
            CheckCompoment();
        }

        void OnDestroy()
        {
            if (_itemModel != null)
            {
                Destroy(_itemModel);
                _itemModel = null;
            }
        }

        public void OnValidate()
        {
            if (_prevDirection != scrollDirection)
            {
                groupItemDirection = shaco.Direction.Automatic;
                ChangeDirection(scrollDirection);
            }

            if (_prevfirstItemOffset != firstItemOffset)
            {
                SetUpdateListViewDirty();
                _prevfirstItemOffset = firstItemOffset;
            }

            if (_previtemMargin != itemMargin)
            {
                SetUpdateListViewDirty();
                _previtemMargin = itemMargin;
                _prevgroupItemMargin = groupItemMargin;
            }

            if (_prevopenDebugMode != openDebugMode)
            {
                CheckCompoment();
                _prevopenDebugMode = openDebugMode;
            }

            if (_prevgroupItemMargin != groupItemMargin)
            {
                if (eachOfGroup <= 1)
                {
                    _prevgroupItemMargin = Vector3.zero;
                    groupItemMargin = Vector3.zero;
                    Log.Error("set groupItemMargin error, eachOfGroup must > 1");
                }
                else
                {
                    SetUpdateListViewDirty();
                    _prevgroupItemMargin = groupItemMargin;
                }
            }

            if (_preveachOfGroup != eachOfGroup)
            {
                if (eachOfGroup < 1)
                {
                    Log.Error("set eachOfGroup error: value must > 0");
                    _preveachOfGroup = 1;
                    eachOfGroup = 1;
                }
                else
                {
                    SetUpdateListViewDirty();
                    _preveachOfGroup = eachOfGroup;
                }
            }

            if (_prevgroupItemDirection != groupItemDirection)
            {
                SetUpdateListViewDirty();
                _prevgroupItemDirection = groupItemDirection;
            }
        }

        public void PauseScrolling()
        {
            PauseScrollingBase();
            _isCustomPauseScrolling = true;
        }

        public void ResumeScrolling()
        {
            _isCustomPauseScrolling = false;
            ResumeScrollingBase();
        }

        private void PauseScrollingBase()
        {
            if (_isCustomPauseScrolling)
                return;

            _willResumeHorizontalScroll |= _scrollRectContent.horizontal;
            _willResumeVerticalScroll |= _scrollRectContent.vertical;

            if (_willResumeHorizontalScroll)
                _scrollRectContent.horizontal = false;
            if (_willResumeVerticalScroll)
                _scrollRectContent.vertical = false;
            _scrollRectContent.StopMovement();
        }

        private void ResumeScrollingBase()
        {
            if (_isCustomPauseScrolling)
                return;

            if (_willResumeHorizontalScroll)
            {
                _scrollRectContent.horizontal = true;
                _willResumeHorizontalScroll = false;
            }
            if (_willResumeVerticalScroll)
            {
                _scrollRectContent.vertical = true;
                _willResumeVerticalScroll = false;
            }
        }

        public void CheckCompoment()
        {
            if (null == GetComponent<ScrollRectEx>())
                _scrollRectContent = this.gameObject.AddComponent<ScrollRectEx>();
            else
                _scrollRectContent = this.GetComponent<ScrollRectEx>();

            if (_scrollRectContent.content == null)
            {
                //create scroll content automatic
                var contentNew = new GameObject();
                contentNew.name = "content";
                ListView.ChangeParentLocal(contentNew, this.gameObject);
                var transNew = contentNew.AddComponent<RectTransform>();
                transNew.sizeDelta = GetComponent<RectTransform>().sizeDelta;
                _scrollRectContent.content = transNew;
            }
            else
            {
                _scrollRectContent.onScrollingCallBack = OnScrollingCallBack;
                _scrollRectContent.onBeginDragCallBack = OnBeginDragCallBack;
                _scrollRectContent.onEndDragCallBack = OnEndDragCallBack;
            }

#if UNITY_5_3_OR_NEWER
            if (GetComponent<RectMask2D>() == null)
            {
                gameObject.AddComponent<RectMask2D>();
            }
#else
            if (GetComponent<Mask>() == null)
            {
                gameObject.AddComponent<Mask>();
            }
#endif

            if (openDebugMode)
            {
                if (_imageDebugContentDraw == null)
                {
                    var content = _scrollRectContent.content;
                    _imageDebugContentDraw = content.GetComponent<Image>();
                    if (_imageDebugContentDraw == null)
                    {
                        _imageDebugContentDraw = content.gameObject.AddComponent<Image>();
                        _imageDebugContentDraw.sprite = null;
                        _imageDebugContentDraw.color = new Color(1, 1, 1, 0.5f);
                    }
                    var rectTrans = _imageDebugContentDraw.GetComponent<RectTransform>();
                    rectTrans.sizeDelta = content.sizeDelta;
                }
                _imageDebugContentDraw.enabled = true;
            }
            else
            {
                if (_imageDebugContentDraw)
                {
                    _imageDebugContentDraw.enabled = false;
                }
            }
        }

        public shaco.Direction GetScrollDirection()
        {
            return scrollDirection;
        }

        public shaco.Direction GetGroupDirection()
        {
            return groupItemDirection;
        }

        public void SetGroupDirection(shaco.Direction direction)
        {
            groupItemDirection = direction;
        }

        public void RefreshAutoUpdateItems()
        {
            if (onItemAutoUpdateCallBack != null)
            {
                int startIndex = GetItemStartDisplayIndex();
                int count = _listItems.Count;

                for (int i = 0; i < count; ++i)
                    onItemAutoUpdateCallBack(i + startIndex, _listItems[i].current);
            }
        }

        private void Update()
        {
            UpdateListView();
        }

        private void UpdateListView()
        {
            if (_isUpdateListviewDirty)
            {
                _isUpdateListviewDirty = false;

                CheckCompoment();

                UpdateGroupItemDirection();
                UpdateContentLayout();

                for (int i = 0; i < _listItems.Count; ++i)
                {
                    UpdateItem(i);
                }

                FixScrollContentPositionWhenEndDrag();

                if (null != onLayoutUpdateCallBack)
                {
                    onLayoutUpdateCallBack();
                }
            }
        }

        private void SetUpdateListViewDirty()
        {
#if UNITY_EDITOR
            _isUpdateListviewDirty = true;
            if (!Application.isPlaying)
                Update();
#else
            _isUpdateListviewDirty = true;
#endif
        }

        //static public void SortQuickly(List<int> listValue, int begin, int end)
        //{
        //    if (begin >= end)
        //        return;

        //    int left = begin;
        //    int right = end;
        //    int key = listValue[begin];

        //    while (left < right)
        //    {
        //        while (left < right && key <= listValue[right]) --right;

        //        listValue[left] = listValue[right];

        //        while (left < right && key >= listValue[left]) ++left;

        //        listValue[right] = listValue[left];
        //    }

        //    listValue[left] = key;
        //    SortQuickly(listValue, begin, left - 1);
        //    SortQuickly(listValue, left + 1, end);
        //}
    }
}
