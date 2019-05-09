using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace shaco
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GestureInputController))]
    public class PageView : MonoBehaviour
    {
        //滚动方向
        public shaco.Direction scrollDirection = shaco.Direction.Right;
        //每页间距
        public Vector2 margin = Vector2.zero;
        //ui的根结点对象，如果该值为空，则不会响应手势滑动
        public Canvas canvasUI = null;

        [SerializeField]
        private GridLayoutGroup _gridGroupModel = null;
        [SerializeField]
        private GameObject _itemMode = null;
        private List<GameObject> _panelPages = new List<GameObject>();
        private List<List<GameObject>> _items = new List<List<GameObject>>();
        private GestureInputController _inputController;
        private bool _isUpdateLayoutDirty = true;
        private GameObject _itemTempCache = null;
        private GameObject _panelPage = null;
        private shaco.Direction _prevScrollDirection;
        private Vector2 _prevMagin = Vector2.zero;
        //移动交换动画站位用
        private GameObject _placeholderObject = null;

        void OnValidate()
        {
            if (_prevScrollDirection != scrollDirection)
            {
                _prevScrollDirection = scrollDirection;
                SetUpdateLayoutDirty();
            }

            if (_prevMagin != margin)
            {
                _prevMagin = margin;
                SetUpdateLayoutDirty();
            }
        }

        void Start()
        {
            _prevScrollDirection = scrollDirection;
            _prevMagin = margin;

            if (_gridGroupModel == null)
            {
                shaco.Log.Error("PageView forget set GridLayoutGroup model");
            }

            _inputController = this.GetComponent<GestureInputController>();
            _inputController.onGestureDirection.AddListener(OnGestureDragDirectionCallBack);

            CheckComponent();
        }

        void Update()
        {
            UpdateLayout();
        }

        public void LocationWithItemIndex(int itemIndex)
        {
            int itemCountInPerPage = GetItemsCountInPerPage();
            int pageIndex = itemIndex / itemCountInPerPage;
            LocationWithPageIndex(pageIndex);
        }

        public void LocationWithPageIndex(int pageIndex)
        {
            CheckComponent();
            UpdateLayout();

            var swapScript = _panelPage.GetComponent<SwapMoveAction>();
            if (null == swapScript)
                return;

            var realPageIndex = GetSafePageIndex(pageIndex);

            var durationTmp = swapScript.ActionDuration;
            swapScript.ActionDuration = 0;
            swapScript.LockActionWhenMoving = false;

            bool isNegative = scrollDirection == shaco.Direction.Left || scrollDirection == shaco.Direction.Up;
            if (realPageIndex < _panelPages.Count / 2)
            {
                for (int i = 0; i < realPageIndex; ++i)
                {
                    swapScript.doAction(!isNegative);
                }
            }
            else
            {
                for (int i = realPageIndex; i < _panelPages.Count; ++i)
                {
                    swapScript.doAction(isNegative);
                }
            }
            swapScript.ActionDuration = durationTmp;
            swapScript.LockActionWhenMoving = true;
        }

        //手势滑动方向事件
        public void OnGestureDragDirectionCallBack()
        {
            if (null == canvasUI)
                return;

            if (null == _inputController)
            {
                shaco.Log.Error("PageView OnGestureDragDirectionCallBack errro: missing input controller");
                return;
            }

            switch (canvasUI.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    {
                        if (!RectTransformUtility.RectangleContainsScreenPoint(this.GetComponent<RectTransform>(), _inputController.currentTouchDownPosition, canvasUI.worldCamera))
                            return;
                        break;
                    }
                default:
                    {
                        if (!RectTransformUtility.RectangleContainsScreenPoint(this.GetComponent<RectTransform>(), _inputController.currentTouchDownPosition, canvasUI.worldCamera))
                            return;
                        break;
                    }
            }

            switch (_inputController.currentGestureDirection)
            {
                case shaco.Direction.Left:
                case shaco.Direction.Up:
                    {
                        TurnFrontPage();
                        break;
                    }
                case shaco.Direction.Right:
                case shaco.Direction.Down:
                    {
                        TurnBehindPage();
                        break;
                    }
            }
        }

        //往前翻页
        public void TurnFrontPage()
        {
            if (_panelPages.Count <= 1)
                return;

            CheckComponent();

            bool isNegative = scrollDirection == shaco.Direction.Left || scrollDirection == shaco.Direction.Up;
            _panelPage.GetComponent<SwapMoveAction>().doAction(!isNegative);
        }

        //往后翻页
        public void TurnBehindPage()
        {
            if (_panelPages.Count <= 1)
                return;

            CheckComponent();

            bool isNegative = scrollDirection == shaco.Direction.Left || scrollDirection == shaco.Direction.Up;
            _panelPage.GetComponent<SwapMoveAction>().doAction(isNegative);
        }

        //添加组建
        public void AddItem(GameObject item)
        {
            InsertItem(item, GetItemsCount());
        }

        //插入组建
        public void InsertItem(GameObject item, int insertIndex)
        {
            if (null == item)
            {
                shaco.Log.Error("PageView InsertItem error: item is null");
                return;
            }

            CheckComponent();

            int maxItemsCount = GetItemsCountInPerPage();
            int pageIndex = insertIndex / maxItemsCount;
            if (pageIndex < 0 || pageIndex > _items.Count)
            {
                shaco.Log.Error("PageView InsertItem error: out of range, insert index=" + insertIndex + " pageIndex=" + pageIndex + " count=" + _items.Count);
                return;
            }

            if (pageIndex > _items.Count - 1)
            {
                _items.Insert(pageIndex, new List<GameObject>());
            }

            var arrayTmp = _items[pageIndex];
            if (arrayTmp.Count >= maxItemsCount)
            {
                if (pageIndex + 1 < _items.Count)
                {
                    _items[pageIndex].Add(item);
                }
                else
                {
                    _items.Insert(pageIndex + 1, new List<GameObject>());
                    _items[pageIndex + 1].Add(item);
                }
            }
            else
            {
                _items[pageIndex].Insert(insertIndex % maxItemsCount, item);
            }

            //rename
            item.name = "Item" + insertIndex;
            shaco.UnityHelper.ChangeParentLocalPosition(item, this.gameObject);

            SetUpdateLayoutDirty();
        }

        //移除组建
        public void RemoveItem(int removeIndex)
        {
            if (_items.Count == 0)
                return;

            CheckComponent();

            int maxItemsCount = GetItemsCountInPerPage();
            int pageIndex = removeIndex / maxItemsCount;
            if (pageIndex < 0 || pageIndex > _items.Count - 1)
            {
                shaco.Log.Error("PageView RemoveItem error: out of range, remove index=" + removeIndex + " pageIndex=" + pageIndex + " count=" + _items.Count);
                return;
            }

            int subIndex = removeIndex % maxItemsCount;
            var pageItemsTmp = _items[pageIndex];
            var itemTmp = pageItemsTmp[subIndex];
            shaco.UnityHelper.ChangeParentLocalPosition(itemTmp, _itemTempCache);
            itemTmp.gameObject.SetActive(false);

            pageItemsTmp.RemoveAt(subIndex);
            if (pageItemsTmp.Count == 0)
                _items.RemoveAt(pageIndex);
            SetUpdateLayoutDirty();
        }

        //移除组建
        public void RemoveItem(GameObject item)
        {
            var findItemIndex = -1;
            for (int i = 0; i < _items.Count; ++i)
            {
                var pageItemsTmp = _items[i];
                for (int j = 0; j < pageItemsTmp.Count; ++j)
                {
                    if (pageItemsTmp[j] == item)
                    {
                        findItemIndex = i;
                        break;
                    }
                }
                if (findItemIndex >= 0)
                {
                    break;
                }
            }

            if (findItemIndex == -1)
            {
                shaco.Log.Error("PageView RemoveItem error: not found item=" + item);
                return;
            }
            RemoveItem(findItemIndex);
            SetUpdateLayoutDirty();
        }

        //清空组建
        public void ClearItem()
        {
            for (int i = _items.Count - 1; i >= 0; --i)
            {
                RemoveItem(i);
            }
            SetUpdateLayoutDirty();
        }

        //获取组建
        public GameObject GetItem(int index)
        {
            GameObject retValue = null;
            int maxItemsCount = GetItemsCountInPerPage();
            int pageIndex = index / maxItemsCount;

            if (pageIndex < 0 || pageIndex > _items.Count)
            {
                shaco.Log.Error("PageView InsertItem error: out of range, insert index=" + index + " pageIndex=" + pageIndex + " count=" + _items.Count);
                return retValue;
            }

            var arrayTmp = _items[pageIndex];
            retValue = arrayTmp[index % maxItemsCount];
            return retValue;
        }

        //通过克隆木板添加组建
        public void AddItembyModel()
        {
            AddItem(GetModelItem());
        }

        //获取克隆模板
        public GameObject GetModelItem()
        {
            CheckComponent();

            if (null == _itemMode)
                return null;
            else
            {
                if (_itemTempCache.transform.childCount > 0)
                {
                    var childTmp = _itemTempCache.transform.GetChild(0).gameObject;
                    shaco.UnityHelper.ChangeParentLocalPosition(childTmp, null);
                    childTmp.gameObject.SetActive(true);
                    return childTmp;
                }
                else
                    return MonoBehaviour.Instantiate(_itemMode) as GameObject;
            }
        }

        //获取每页组建数量
        public int GetItemsCountInPerPage()
        {
            CheckComponent();

            return _gridGroupModel == null ? 1 : GetMaxItemsCountInPage(_gridGroupModel.gameObject);
        }

        //获取组建数量
        public int GetItemsCount()
        {
            CheckComponent();

            var retValue = 0;
            if (_items.Count > 0)
            {
                int itemsCountPer = GetItemsCountInPerPage();
                retValue = (itemsCountPer * (_items.Count - 1)) + _items[_items.Count - 1].Count;
            }
            return retValue;
        }

        private void UpdatePlaceHolderPosition()
        {
            if (_items.Count == 0 || _items[0].Count == 0 || null == _gridGroupModel)
                return;

            //创建一个占位用节点，优化动画移动方式
            if (null == _placeholderObject)
            {
                _placeholderObject = new GameObject();
                _placeholderObject.name = "PlaceholderTemp";
                shaco.UnityHelper.ChangeParentLocalPosition(_placeholderObject, _panelPage);
            }
            _placeholderObject.transform.SetAsFirstSibling();
            _placeholderObject.transform.localPosition = -new Vector3(_gridGroupModel.GetComponent<RectTransform>().rect.size.x, 0);
        }

        private void OnWillSwapMoveCallBack(SwapMoveAction.ListSelect currentSelect, bool isLeftToRight)
        {
            if (null == _panelPage || null == _placeholderObject)
                return;

            if (currentSelect.next.selectObject == _placeholderObject)
            {
                var swapScript = _panelPage.GetComponent<SwapMoveAction>();
                swapScript.LockActionWhenMoving = false;
                swapScript.OnWillMoveCallBack -= OnWillSwapMoveCallBack;

                swapScript.doAction(isLeftToRight);

                swapScript.OnWillMoveCallBack += OnWillSwapMoveCallBack;
            }
        }

        //创建新的页面
        private GameObject CreateNewPage()
        {
            GameObject retValue = null;
            if (null == _gridGroupModel)
            {
                retValue = new GameObject();
                retValue.AddComponent<RectTransform>();
                retValue.AddComponent<GridLayoutGroup>();
            }
            else
            {
                retValue = MonoBehaviour.Instantiate(_gridGroupModel.gameObject) as GameObject;
            }

            shaco.UnityHelper.ChangeParentLocalPosition(retValue, _panelPage);

            int insertIndex = _panelPages.Count;

            _panelPages.Insert(insertIndex, retValue);
            retValue.name = "page" + _panelPages.Count;
            return retValue;
        }

        //获取页面已经有的组建数量 
        private int GetItemsCountInPage(GameObject page)
        {
            return page.transform.childCount;
        }

        //获取页面可以显示的最大组建数量
        private int GetMaxItemsCountInPage(GameObject page)
        {
            var rectTransTmp = page.GetComponent<RectTransform>();
            var gridLayoutGroup = page.GetComponent<GridLayoutGroup>();

            int maxWidthCount = (int)rectTransTmp.sizeDelta.x / (int)gridLayoutGroup.cellSize.x;
            int maxHeightCount = (int)rectTransTmp.sizeDelta.y / (int)gridLayoutGroup.cellSize.y;

            if (maxWidthCount == 0 || maxHeightCount == 0)
            {
                shaco.Log.Error("PageView GetMaxItemsCountInPage error: valid size, width=" + maxWidthCount + " height=" + maxHeightCount);
            }

            return maxWidthCount * maxHeightCount;
        }

        private int GetSafePageIndex(int pageIndex)
        {
            if (pageIndex > _panelPages.Count - 1)
                pageIndex = 0;
            else if (pageIndex < 0)
                pageIndex = _panelPages.Count - 1;
            return pageIndex;
        }

        private void SetUpdateLayoutDirty()
        {
#if UNITY_EDITOR
            _isUpdateLayoutDirty = true;
            if (!Application.isPlaying)
                Update();
#else
            _isUpdateLayoutDirty = true;
#endif
        }

        private void UpdateLayout()
        {
            if (!_isUpdateLayoutDirty)
                return;
            _isUpdateLayoutDirty = false;

            if (_items.Count == 0)
                return;

            ResetItemToCache();
            CheckPageCount();

            for (int i = 0; i < _items.Count; ++i)
            {
                var pageTmp = _panelPages[i];
                var pageItemsTmp = _items[i];

                for (int j = 0; j < pageItemsTmp.Count; ++j)
                {
                    shaco.UnityHelper.ChangeParentLocalPosition(pageItemsTmp[j], pageTmp);
                }
            }

            UpdatePagesLayout();
            UpdateSwapMoveActionSelect();
        }

        private void CheckPageCount()
        {
            if (_panelPages.Count == 0)
            {
                CreateNewPage();
            }

            int itemsCount = GetItemsCount();
            int maxCountInPerPage = GetMaxItemsCountInPage(_panelPages[0]);
            int addCount = (itemsCount % maxCountInPerPage > 0 ? 1 : 0);
            int requirePageCount = itemsCount / maxCountInPerPage;
            requirePageCount += addCount;

            for (int i = _panelPages.Count; i < requirePageCount; ++i)
            {
                CreateNewPage();
            }
        }

        private void UpdateSwapMoveActionSelect()
        {
            if (_items.Count == 0)
                return;

            UpdatePlaceHolderPosition();

            var swapScript = _panelPage.GetComponent<SwapMoveAction>();
            swapScript.UpdateSelect();
        }

        private void ResetItemToCache()
        {
            CheckComponent();

            for (int i = 0; i < _items.Count; ++i)
            {
                var pageItemsTmp = _items[i];

                for (int j = 0; j < pageItemsTmp.Count; ++j)
                {
                    shaco.UnityHelper.ChangeParentLocalPosition(pageItemsTmp[j], _itemTempCache);
                }
            }
        }

        private void UpdatePagesLayout()
        {
            var transformTmp = this.GetComponent<RectTransform>();
            var widthPerPage = transformTmp.sizeDelta.x;
            var heightPerPage = transformTmp.sizeDelta.y;

            int offsetIndex = 0;

            for (int i = 0; i < _panelPages.Count; ++i)
            {
                UpdatePageLayout(_panelPages[i], offsetIndex++, widthPerPage, heightPerPage);
            }
        }

        private void UpdatePageLayout(GameObject panelPage, int index, float widthPerPage, float heightPerPage)
        {
            if (index < 0 || index > _panelPages.Count - 1)
            {
                shaco.Log.Error("PageView UpdatePageLayout error: out of range index=" + index + " count=" + _panelPages.Count);
                return;
            }

            var pagePosition = Vector3.zero;
            switch (scrollDirection)
            {
                case shaco.Direction.Down:
                    {
                        pagePosition = new Vector3(0, -heightPerPage * index);
                        break;
                    }
                case shaco.Direction.Up:
                    {
                        pagePosition = new Vector3(0, heightPerPage * index);
                        break;
                    }
                case shaco.Direction.Right:
                    {
                        pagePosition = new Vector3(widthPerPage * index, 0);
                        break;
                    }
                case shaco.Direction.Left:
                    {
                        pagePosition = new Vector3(-widthPerPage * index, 0);
                        break;
                    }
                default: shaco.Log.Error("PageView UpdatePagesLayout error: unsupport type=" + scrollDirection); break;
            }

            pagePosition += new Vector3(margin.x * index, margin.y * index);

            panelPage.transform.localPosition = pagePosition;
            panelPage.transform.SetSiblingIndex(index);
        }

        private void CheckComponent()
        {
            //create item temp cache
            if (null == _itemTempCache)
            {
                _itemTempCache = new GameObject();
                _itemTempCache.name = "ItemTempCache";
                shaco.UnityHelper.ChangeParentLocalPosition(_itemTempCache, this.gameObject);
            }

            //create panel page
            if (null == _panelPage)
            {
                _panelPage = new GameObject();
                _panelPage.name = "PanelPage";
                shaco.UnityHelper.ChangeParentLocalPosition(_panelPage, this.gameObject);
            }

            //set grid model as pageview size
            if (null != _gridGroupModel)
                _gridGroupModel.GetComponent<RectTransform>().sizeDelta = this.GetComponent<RectTransform>().rect.size;
            else
                shaco.Log.Error("PageView missing set Grid Group Model...");

            //auto set move action script
            var swapScript = _panelPage.GetComponent<SwapMoveAction>();
            if (null == swapScript)
            {
                swapScript = _panelPage.AddComponent<SwapMoveAction>();
                swapScript.OnWillMoveCallBack = OnWillSwapMoveCallBack;
                swapScript.AutoHideBackMove = true;
            }
        }
    }
}