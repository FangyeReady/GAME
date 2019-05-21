using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public partial class ListView
    {
        private bool _isAutoUpdating = false;
        private bool _isLocationPositioning = false;

        public void AddItem(GameObject newItem)
        {
            InsertItem(newItem, _listItems.Count);
        }

        public void InitItemWithAutoUpdate(int startIndex, int endIndex, int showIndex = int.MinValue, bool isDestroy = false)
        {
            if (onItemAutoUpdateCallBack == null)
            {
                shaco.Log.Error("ListView+ItemOperator error: if you wan't call 'InitItemWithAutoUpdate', please set ListView.onItemAutoUpdateCallBack before");
                return;
            }

            if (startIndex > endIndex)
            {
                shaco.Log.Warning("ListView+ItemOperator warning: startIndex(" + startIndex + ") > " + " endIndex(" + endIndex + ")");
                startIndex = startIndex.SwapValue(ref endIndex);
            }

            if (showIndex != int.MinValue)
            {
                if (showIndex < startIndex)
                {
                    shaco.Log.Warning("ListView+ItemOperator warning: showdIndex(" + showIndex + ") < " + " startIndex(" + startIndex + ")");
                    showIndex = startIndex;
                }
                if (showIndex > endIndex)
                {
                    shaco.Log.Warning("ListView+ItemOperator warning: showdIndex(" + showIndex + ") > " + " endIndex(" + endIndex + ")");
                    showIndex = endIndex;
                }
            }

            ClearItem(isDestroy);

            _itemIndexOffsetUseInSpringbackCallBack = startIndex;
            CheckAutoItemSize(endIndex, false);
            _itemIndexOffsetUseInSpringbackCallBack = startIndex;

            int locationIndex = showIndex == int.MinValue ? startIndex : showIndex;
            LocationByItemIndex(locationIndex);
        }

        public void RefreshAllItemsWithAutoUpdate()
        {
            if (onItemAutoUpdateCallBack == null)
            {
                shaco.Log.Error("ListView+ItemOperator error: if you wan't call 'RefreshAllItemsWithAutoUpdate', please set ListView.onItemAutoUpdateCallBack before");
                return;
            }

            int startIndex = GetItemStartDisplayIndex();
            int endIndex = GetItemEndDisplayIndex();
            for (int i = startIndex; i <= endIndex; ++i)
            {
                onItemAutoUpdateCallBack(i, GetItem(i - startIndex));
            }
        }

        public void InsertItem(GameObject newItem, int index)
        {
            CheckCompoment();

            if (index < 0)
            {
                Log.Warning("InsertItem warning: out of range, but auto fixed index=" + index);
                index = 0;
            }
            else if (index > _listItems.Count)
            {
                Log.Warning("InsertItem warning: out of range, but auto fixed index=" + index);
                index = _listItems.Count;
            }

            Item itemTmp = new Item();
            _listItems.Insert(index, itemTmp);
            int prevIndex = index - 1;
            int nextIndex = index + 1;
            itemTmp.current = newItem;

            if (prevIndex < 0)
            {
                prevIndex = _listItems.Count - 1;
            }
            itemTmp.prev = _listItems[prevIndex];
            _listItems[prevIndex].next = itemTmp;

            if (nextIndex > _listItems.Count - 1)
            {
                nextIndex = 0;
            }
            itemTmp.next = _listItems[nextIndex];
            _listItems[nextIndex].prev = itemTmp;

            newItem.name = "Item" + index;
            ListView.ChangeParentLocal(newItem, _scrollRectContent.content.gameObject);
            newItem.transform.SetSiblingIndex(_listItems.Count - index - 1);

            //The default hidden item is displayed until the size of the content is refreshed
            //please see 'ListView+Layout.UpdateItem()'
            newItem.SetActive(false);

            SetUpdateListViewDirty();
        }

        public void AddItemByModel()
        {
            if (_itemModel != null)
                AddItem(GetItemModel());
        }

        public void InsertItemByModel(int index)
        {
            if (_itemModel != null)
                InsertItem(GetItemModel(), index);
        }

        public void RemoveItem(int index, bool isDestroy = true)
        {
            if (index < 0 || index > _listItems.Count - 1)
            {
                Log.Error("ListView RemoveItem error: out of range");
                return;
            }

            var item = _listItems[index];

            //cut connect
            item.prev.next = item.next;
            item.next.prev = item.prev;
            item.next = null;
            item.prev = null;

            //destroy object
            if (_listItems[index].current)
            {
                if (isDestroy)
                    UnityHelper.SafeDestroy(_listItems[index].current);
                else
                {
                    _listItems[index].current.gameObject.SetActive(false);
                    _listItemCache.Add(_listItems[index].current.gameObject);
                }
            }

            var itemTmp = _listItems[index];
            _listItems.RemoveAt(index);

            if (_listItems.Count > 0)
            {
                if (_firstItemWhenEndGrag == itemTmp) _firstItemWhenEndGrag = _listItems[_listItems.Count - 1];
                if (_lastItemWhenEndGrag == itemTmp) _lastItemWhenEndGrag = null;
            }
            SetUpdateListViewDirty();
        }

        public void RemoveItem(GameObject item, bool isDestroy = true)
        {
            int index = GetItemIndex(item);
            if (index >= 0 && index < _listItems.Count)
            {
                RemoveItem(index, isDestroy);
            }
            else
            {
                Log.Error("RemoveItem error: not find item =" + item);
            }
        }

        public void RemoveItemRange(int startIndex, int count, bool isDestroy = true)
        {
            if (startIndex < 0 || startIndex > _listItems.Count - 1 || count > _listItems.Count)
            {
                Log.Error("ListView RemoveItemRange erorr: out of range! startIndex=" + startIndex + " count=" + count + " item count=" + _listItems.Count);
                return;
            }

            for (int i = startIndex + count - 1; i >= startIndex; --i)
            {
                RemoveItem(i, isDestroy);
            }
        }

        public void SwapItem(int sourceIndex, int destinationIndex)
        {
            if (sourceIndex < 0 || sourceIndex > _listItems.Count - 1)
            {
                Log.Error("ListView SwapItem error: out of range: sourceIndex=" + sourceIndex + " count=" + _listItems.Count);
                return;
            }
            if (destinationIndex < 0 || destinationIndex > _listItems.Count - 1)
            {
                Log.Error("ListView SwapItem error: out of range: destinationIndex=" + destinationIndex + " count=" + _listItems.Count);
                return;
            }
            _listItems.SwapValue(sourceIndex, destinationIndex);
            SetUpdateListViewDirty();
        }

        public void ClearItem(bool isDestroy = true)
        {
            CheckCompoment();

            if (isDestroy)
            {
                for (int i = 0; i < _listItems.Count; ++i)
                {
                    UnityHelper.SafeDestroy(_listItems[i].current);
                }
            }
            else
            {
                for (int i = 0; i < _listItems.Count; ++i)
                {
                    _listItems[i].current.SetActive(false);
                    _listItemCache.Add(_listItems[i].current);
                }
            }
            _listItems.Clear();

            if (_oldContentSize.x > 0 && _oldContentSize.y > 0)
            {
                _scrollRectContent.content.sizeDelta = new Vector2(_oldContentSize.x, _oldContentSize.y);
                _scrollRectContent.content.transform.localPosition = Vector3.zero;
            }
            _itemIndexOffsetUseInSpringbackCallBack = 0;
            _firstItemWhenEndGrag = null;
            _lastItemWhenEndGrag = null;
            SetUpdateListViewDirty();
        }

        public void SetItemModel(GameObject item)
        {
            if (item == null)
            {
                Log.Error("SetItemModel error: item is null");
                return;
            }

            if (item.GetComponent<RectTransform>() == null)
            {
                Log.Error("SetItemModel error: item must contain RectTransform Compoment");
                return;
            }

            _itemModel = item;
            _itemModel.SetActive(false);
        }

        public GameObject GetItemModel()
        {
            if (_itemModel == null)
            {
                Log.Error("GetItemModel error: you don't set model, item model is null");
                return null;
            }
            else
            {
                var ret = Instantiate(_itemModel) as GameObject;
                ret.SetActive(true);
                return ret;
            }
        }

        public GameObject GetFirstItem()
        {
            GameObject retValue = null;
            if (_listItems.Count == 0)
            {
                Log.Error("ListView GetFirstItem error: listview is empty !");
                return retValue;
            }
            retValue = _listItems[0].current;
            return retValue;
        }

        public GameObject GetLastItem()
        {
            GameObject retValue = null;
            if (_listItems.Count == 0)
            {
                Log.Error("ListView GetFirstItem error: listview is empty !");
                return retValue;
            }
            retValue = _listItems[_listItems.Count - 1].current;
            return retValue;
        }

        public int GetItemStartDisplayIndex()
        {
            return _itemIndexOffsetUseInSpringbackCallBack;
        }

        public int GetItemEndDisplayIndex()
        {
            return _itemIndexOffsetUseInSpringbackCallBack + _listItems.Count - 1;
        }

        public int GetItemIndexByDispalyIndex(int displayIndex)
        {
            return displayIndex - _itemIndexOffsetUseInSpringbackCallBack;
        }

        public void Sort(SortCompareFunc compareFunc)
        {
            if (_listItems.Count == 0)
                return;

            List<GameObject> listSwap = new List<GameObject>();
            _listItems.Sort(new _SortCompareFunc(compareFunc));

            while (_listItems.Count > 0)
            {
                listSwap.Add(_listItems[0].current);
                RemoveItem(0, false);
            }

            for (int i = 0; i < listSwap.Count; ++i)
            {
                AddItem(listSwap[i]);
                listSwap[i].gameObject.SetActive(true);
            }
            SetUpdateListViewDirty();
        }

        public GameObject GetItem(int index)
        {
            GameObject ret = null;
            if (index < 0 || index > _listItems.Count - 1)
            {
                return ret;
            }
            else
            {
                ret = _listItems[index].current;
            }
            return ret;
        }

        public GameObject PopItemFromCacheOrCreateFromModel()
        {
            GameObject ret = null;
            if (_listItemCache.Count > 0)
            {
                ret = _listItemCache[_listItemCache.Count - 1];
                ret.SetActive(true);
                _listItemCache.RemoveAt(_listItemCache.Count - 1);
            }
            else
            {
                ret = GetItemModel();
                if (null == ret)
                {
                    shaco.Log.Error("ListView PopItemFromCacheOrCreateFromModel error: model is null, please call 'SetItemModel' at frist");
                }
            }
            return ret;
        }

        public int GetItemIndex(GameObject item)
        {
            int ret = -1;
            for (int i = 0; i < _listItems.Count; ++i)
            {
                if (_listItems[i].current == item)
                {
                    ret = i;
                    break;
                }
            }

            if (ret == -1)
            {
                Log.Error("GetItemIndex error: not find item =" + item);
                return ret;
            }
            else
                return ret;
        }

        public void LocationByItemIndex(int displayIndex)
        {
            CheckAutoItemSize(displayIndex);
            UpdateListView();

            var item = GetItem(displayIndex - GetItemStartDisplayIndex());
            var newWorldPosition = GetLocationItemWorldPositionWithConvert(item);

            PauseScrollingBase();
            _scrollRectContent.content.transform.position = newWorldPosition;
            ResumeScrollingBase();
        }

        /// <summary>
        /// 带动画的定位组建位置
        /// </summary>
        /// <param name="displayIndex">显示下标</param>
        /// <param name="moveDurationSeconds">移动动画执行时间</param>
        public bool LocationActionByItemIndex(int displayIndex, float moveDurationSeconds)
        {
            if (_isLocationPositioning)
            {
                shaco.Log.Warning("ListView+ItemOperator LocationActionByItemIndex warning: is location positioning... please wait");
                return false;
            }
            _isLocationPositioning = true;

            bool isFrontToBehind = displayIndex < _itemIndexOffsetUseInSpringbackCallBack;
            int addCount = CheckAutoItemSize(displayIndex, false);
            UpdateListView();

            if (!isFrontToBehind)
            {
                _itemIndexOffsetUseInSpringbackCallBack -= addCount;
            }
            var itemIndex = displayIndex - GetItemStartDisplayIndex();
            var item = GetItem(itemIndex);

            var newWorldPosition = GetLocationItemWorldPositionWithConvert(item);

            //If the location is in place, quickly stop action
            if (IsSamePlaceWithLocationMove(newWorldPosition))
            {
                _isLocationPositioning = false;
                _itemIndexOffsetUseInSpringbackCallBack += addCount;
                return false;
            }

            //pause scrolling
            PauseScrollingBase();

            //run move action
            var moveAction = RunLocatinAction(newWorldPosition, moveDurationSeconds);


            moveAction.onCompleteFunc += (shaco.ActionS ac) =>
            {
                ResumeScrollingBase();

                //delay remove items
                if (isFrontToBehind)
                {
                    RetainContentPositionByFirstItem();

                    if (addCount > 0)
                        RemoveItemRange(_listItems.Count - addCount, addCount, false);
                }
                else
                {
                    _itemIndexOffsetUseInSpringbackCallBack += addCount;
                    RetainContentPositionByLastItem();

                    if (addCount > 0)
                        RemoveItemRange(0, addCount, false);
                }
                _isLocationPositioning = false;
            };
            return true;
        }

        public void LocationActionByGameObject(GameObject target, float moveDurationSeconds)
        {
            LocationActionByWorldPosition(UnityHelper.GetWorldPositionByPivot(target, shaco.Pivot.Middle), moveDurationSeconds);
        }

        public void LocationActionByWorldPosition(Vector3 worldPosition, float moveDurationSeconds)
        {
            if (_isLocationPositioning)
            {
                shaco.Log.Warning("ListView+ItemOperator LocationActionByWorldPosition warning: is location positioning... please wait");
                return;
            }
            _isLocationPositioning = true;

            var newWorldPosition = GetLocationItemWorldPositionWithConvert(worldPosition);
            if (IsSamePlaceWithLocationMove(newWorldPosition))
            {
                _isLocationPositioning = false;
                return;
            }

            PauseScrollingBase();
            var moveAction = RunLocatinAction(newWorldPosition, moveDurationSeconds);
            moveAction.onCompleteFunc += (shaco.ActionS action) =>
            {
                _isLocationPositioning = false;
            };
            ResumeScrollingBase();
        }

        public void LocationByGameObject(GameObject target)
        {
            LocationByWorldPosition(UnityHelper.GetWorldPositionByPivot(target, shaco.Pivot.Middle));
        }

        public void LocationByWorldPosition(Vector3 worldPosition)
        {
            var newWorldPosition = GetLocationItemWorldPositionWithConvert(worldPosition);

            PauseScrollingBase();
            _scrollRectContent.content.transform.position = newWorldPosition;
            ResumeScrollingBase();
        }

        private bool IsSamePlaceWithLocationMove(Vector3 newWorldPosition)
        {
            if (newWorldPosition.x.Round(1) == _scrollRectContent.content.transform.position.x.Round(1)
             && newWorldPosition.y.Round(1) == _scrollRectContent.content.transform.position.y.Round(1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private Vector3 GetLocationItemWorldPositionWithConvert(GameObject target)
        {
            return GetLocationItemWorldPositionWithConvert(UnityHelper.GetWorldPositionByPivot(target, shaco.Pivot.Middle));
        }

        private Vector3 GetLocationItemWorldPositionWithConvert(Vector3 worldPosition)
        {
            var retValue = Vector3.zero;
            var newWorldPosition = Vector3.zero;
            var newContentPivot = Vector3.zero;

            switch (scrollDirection)
            {
                case shaco.Direction.Down:
                case shaco.Direction.Up:
                    {
                        newContentPivot = shaco.Pivot.LeftMiddle;
                        var offsetY = UnityHelper.GetWorldPositionByPivot(this.gameObject, newContentPivot).y - worldPosition.y;

                        //fix in listview content
                        var contentTopY = UnityHelper.GetWorldPositionByPivot(_scrollRectContent.content.gameObject, shaco.Pivot.LeftTop).y;
                        var listviewTopY = UnityHelper.GetWorldPositionByPivot(this.gameObject, shaco.Pivot.LeftTop).y;

                        var contentBottomY = UnityHelper.GetWorldPositionByPivot(_scrollRectContent.content.gameObject, shaco.Pivot.LeftBottom).y;
                        var listviewBottomY = UnityHelper.GetWorldPositionByPivot(this.gameObject, shaco.Pivot.LeftBottom).y;

                        if (listviewTopY - contentTopY > offsetY)
                            offsetY = listviewTopY - contentTopY;

                        if (listviewBottomY - contentBottomY < offsetY)
                            offsetY = listviewBottomY - contentBottomY;

                        newWorldPosition = new Vector3(_scrollRectContent.content.transform.position.x, _scrollRectContent.content.transform.position.y + offsetY);
                        break;
                    }
                case shaco.Direction.Right:
                case shaco.Direction.Left:
                    {
                        newContentPivot = shaco.Pivot.MiddleTop;
                        var offsetX = UnityHelper.GetWorldPositionByPivot(this.gameObject, newContentPivot).x - worldPosition.x;

                        //fix in listview content
                        var contentLeftX = UnityHelper.GetWorldPositionByPivot(_scrollRectContent.content.gameObject, shaco.Pivot.LeftTop).x;
                        var listviewLeftX = UnityHelper.GetWorldPositionByPivot(this.gameObject, shaco.Pivot.LeftTop).x;

                        var contentRightX = UnityHelper.GetWorldPositionByPivot(_scrollRectContent.content.gameObject, shaco.Pivot.RightBottom).x;
                        var listviewRightX = UnityHelper.GetWorldPositionByPivot(this.gameObject, shaco.Pivot.RightBottom).x;

                        if (listviewRightX - contentRightX > offsetX)
                            offsetX = listviewRightX - contentRightX;

                        if (listviewLeftX - contentLeftX < offsetX)
                            offsetX = listviewLeftX - contentLeftX;

                        newWorldPosition = new Vector3(_scrollRectContent.content.transform.position.x + offsetX, _scrollRectContent.content.transform.position.y);
                        break;
                    }
            }

            retValue = newWorldPosition;
            return retValue;
        }

        private int GetCollectionIndex(int displayIndex)
        {
            return displayIndex - _itemIndexOffsetUseInSpringbackCallBack;
        }

        private ActionS RunLocatinAction(Vector3 newPosition, float moveDurationSeconds)
        {
            //run move action
            var moveToAction = shaco.MoveTo.Create(newPosition, moveDurationSeconds);
            var accelerateAction = shaco.Accelerate.Create(moveToAction,
                new shaco.Accelerate.ControlPoint(0, 3.0f),
                new shaco.Accelerate.ControlPoint(0.5f, 2.0f),
                new shaco.Accelerate.ControlPoint(1, 0.2f));
            accelerateAction.RunAction(_scrollRectContent.content.gameObject);

            return accelerateAction;
        }

        private int CheckAutoItemSize(int endIndex, bool autoRemove = true)
        {
            int realAddCount = 0;
            if (_isAutoUpdating)
            {
                if (endIndex < GetItemStartDisplayIndex() || endIndex > GetItemEndDisplayIndex())
                {
                    shaco.Log.Warning("ListView+ItemOperator CheckAutoItemSize warning: updating now... endIndex is out of range, index=" + endIndex + " start=" + GetItemStartDisplayIndex() + " end=" + GetItemEndDisplayIndex());
                }
                return realAddCount;
            }

            _isAutoUpdating = true;

            if (null == onItemAutoUpdateCallBack)
            {
                if (endIndex < 0 || endIndex > _listItems.Count - 1)
                {
                    shaco.Log.Error("ListView CheckAutoItemSize error: can't auto update item size automatic when no 'onItemAutoUpdateCallBack' delegate ");
                }
            }
            else
            {
                if (endIndex >= _itemIndexOffsetUseInSpringbackCallBack)
                {
                    endIndex = endIndex < autoUpdateItemMaxIndex ? endIndex : autoUpdateItemMaxIndex;

                    int startIndex = _itemIndexOffsetUseInSpringbackCallBack + _listItems.Count;
                    var addCount = endIndex - startIndex + 1;
                    var maxDisplayItemCount = System.Math.Max(_listItems.Count, _itemIndexOffsetUseInSpringbackCallBack);
                    if (autoRemove && addCount > maxDisplayItemCount)
                    {
                        startIndex = endIndex - maxDisplayItemCount + 1;
                        int newAddCount = endIndex - startIndex + 1;
                        _itemIndexOffsetUseInSpringbackCallBack += addCount - newAddCount;
                        addCount = newAddCount;
                    }
                    var removeCount = System.Math.Min(maxDisplayItemCount, addCount);

                    if (addCount > 0)
                    {
                        RetainContentPositionByLastItem();
                    }

                    if (null != onItemsPrepareAutoUpdateCallBack && addCount > 0)
                    {
                        if (!onItemsPrepareAutoUpdateCallBack(startIndex, endIndex))
                        {
                            _isAutoUpdating = false;
                            return realAddCount;
                        }
                    }

                    for (int i = startIndex; i <= endIndex; ++i)
                    {
                        if (AutoUpdateItem(i, _listItems.Count, 0, removeCount-- > 0 ? autoRemove : false))
                        {
                            ++_itemIndexOffsetUseInSpringbackCallBack;
                            ++realAddCount;
                        }
                    }

                    if (null != onItemsDidAutoUpdateCallBack && realAddCount > 0)
                    {
                        onItemsDidAutoUpdateCallBack(startIndex, endIndex - (addCount - realAddCount));
                    }
                    if (realAddCount <= 0)
                    {
                        _lastItemWhenEndGrag = null;
                        _fixContentOffsetWhenEndGrag = Vector3.zero;
                    }
                }
                else
                {
                    endIndex = endIndex > autoUpdateItemMinIndex ? endIndex : autoUpdateItemMinIndex;

                    int startIndex = _itemIndexOffsetUseInSpringbackCallBack - 1;
                    var addCount = startIndex - endIndex + 1;
                    var maxDisplayItemCount = System.Math.Max(_listItems.Count, _itemIndexOffsetUseInSpringbackCallBack);
                    if (autoRemove && addCount > maxDisplayItemCount)
                    {
                        startIndex = endIndex + maxDisplayItemCount - 1;
                        int newAddCount = startIndex - endIndex + 1;
                        _itemIndexOffsetUseInSpringbackCallBack -= addCount - newAddCount;
                        addCount = newAddCount;
                    }
                    var removeCount = System.Math.Min(maxDisplayItemCount, addCount);
                    if (addCount > 0)
                    {
                        RetainContentPositionByFirstItem();
                    }

                    if (null != onItemsPrepareAutoUpdateCallBack && addCount > 0)
                    {
                        if (!onItemsPrepareAutoUpdateCallBack(startIndex, endIndex))
                        {
                            _isAutoUpdating = false;
                            return realAddCount;
                        }
                    }

                    for (int i = startIndex; i >= endIndex; --i)
                    {
                        if (AutoUpdateItem(i, 0, _listItems.Count - 1, removeCount-- > 0 ? autoRemove : false))
                        {
                            --_itemIndexOffsetUseInSpringbackCallBack;
                            ++realAddCount;
                        }
                    }

                    if (null != onItemsDidAutoUpdateCallBack && realAddCount > 0)
                    {
                        onItemsDidAutoUpdateCallBack(startIndex, endIndex + (addCount - realAddCount));
                    }
                    if (realAddCount == 0)
                    {
                        _firstItemWhenEndGrag = null;
                        _fixContentOffsetWhenEndGrag = Vector3.zero;
                    }
                }
            }

            _isAutoUpdating = false;
            return realAddCount;
        }

        private bool AutoUpdateItem(int itemIndex, int insertIndex, int removeIndex, bool autoRemove)
        {
            if (null != onItemWillAutoUpdateCallBack)
            {
                if (!onItemWillAutoUpdateCallBack(itemIndex))
                {
                    return false;
                }
            }

            if (autoRemove)
            {
                this.RemoveItem(removeIndex, false);
            }

            if (insertIndex < 0) insertIndex = 0;
            if (insertIndex > _listItems.Count) insertIndex = _listItems.Count;

            var newItem = PopItemFromCacheOrCreateFromModel();
            this.InsertItem(newItem, insertIndex);
            onItemAutoUpdateCallBack(itemIndex, newItem);

            return true;
        }
    }
}