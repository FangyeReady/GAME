using UnityEngine;
using System.Collections;

namespace shaco
{
    public partial class ListView
    {
        private bool _isOutOfFrontBounds = false;
        private bool _isOutOfBehindBounds = false;
        private shaco.Direction _prevDirection = shaco.Direction.Right;
        private Vector3 _prevfirstItemOffset = Vector3.zero;
        private Vector3 _previtemMargin = Vector3.zero;
        private Vector3 _prevgroupItemMargin = Vector3.zero;
        private bool _prevopenDebugMode = false;
        private int _preveachOfGroup = 0;
        private shaco.Direction _prevgroupItemDirection = shaco.Direction.Automatic;
        private Vector3 _oldContentSize = Vector3.zero;
        private UnityEngine.UI.Image _imageDebugContentDraw;
        private Vector3 _currentGroupItemPivotPrev = Vector3.zero;
        private Vector3 _currentGroupItemPivotNext = Vector3.zero;
        private Vector3 _fixContentOffsetWhenEndGrag = Vector3.zero;
        private Item _lastItemWhenEndGrag = null;
        private Item _firstItemWhenEndGrag = null;
        private int _itemIndexOffsetUseInSpringbackCallBack = 0;
        private ScrollRectEx _scrollRectContent = null;
        private Vector3 _prevContentLocalPosition = Vector3.zero;

        public void ChangeDirection(shaco.Direction dir)
        {
            if (dir == shaco.Direction.Automatic)
            {
                scrollDirection = _prevDirection;
                Log.Error("ListView ChangeDirection error: unsupport direction=" + dir);
                return;
            }
            scrollDirection = dir;
            _prevDirection = dir;
            SetUpdateListViewDirty();
        }

        public void ChangeGroupItemDirection(shaco.Direction dir)
        {
            groupItemDirection = dir;
            SetUpdateListViewDirty();
        }

        public void UpdateCenterLayout()
        {
            SetUpdateListViewDirty();
        }

        public CenterLayoutType GetCenterLayoutType()
        {
            CenterLayoutType retValue = CenterLayoutType.NoCenter;

            if (!_scrollRectContent.horizontal && _scrollRectContent.vertical)
            {
                retValue = CenterLayoutType.CenterHorizontalOnly;
            }
            else if (_scrollRectContent.horizontal && !_scrollRectContent.vertical)
            {
                retValue = CenterLayoutType.CenterVerticalOnly;
            }
            else if (!_scrollRectContent.horizontal && !_scrollRectContent.vertical)
            {
                retValue = CenterLayoutType.Center;
            }
            
            return retValue;
        }

        private void UpdateItem(int index)
        {
            if (index < 0 || index > _listItems.Count - 1)
            {
                Log.Error("ListView UpdateItem error: out of range");
                return;
            }
            var item = _listItems[index];
            RectTransform content = _scrollRectContent.content;

            item.current.SetActive(true);

            //calculate position
            Vector3 pivotDecide = Vector3.zero;
            Vector3 pivotAdd = Vector3.zero;

            switch (scrollDirection)
            {
                case shaco.Direction.Right: pivotDecide = shaco.Pivot.LeftBottom; pivotAdd = shaco.Pivot.RightBottom; break;
                case shaco.Direction.Left: pivotDecide = shaco.Pivot.RightBottom; pivotAdd = shaco.Pivot.LeftBottom; break;
                case shaco.Direction.Up: pivotDecide = shaco.Pivot.LeftBottom; pivotAdd = shaco.Pivot.LeftTop; break;
                case shaco.Direction.Down: pivotDecide = shaco.Pivot.LeftTop; pivotAdd = shaco.Pivot.LeftBottom; break;
                default: Log.Error("unsupport direction"); break;
            }

            if (index == 0)
            {
                if (scrollDirection == shaco.Direction.Left)
                    UnityHelper.SetLocalPositionByPivot(item.current, new Vector3(content.rect.width, 0), pivotDecide);
                else if (scrollDirection == shaco.Direction.Down)
                    UnityHelper.SetLocalPositionByPivot(item.current, new Vector3(0, content.rect.height), pivotDecide);
                else
                {
                    UnityHelper.SetLocalPositionByPivot(item.current, Vector3.zero, pivotDecide);
                }
                FixedLocalPositionByContent(item.current);
            }
            else
            {
                //first item of group
                if (index % eachOfGroup == 0)
                {
                    var prevItem = GetItem((index / eachOfGroup - 1) * eachOfGroup);
                    Vector3 prevPosition = UnityHelper.GetLocalPositionByPivot(prevItem, pivotAdd);
                    UnityHelper.SetLocalPositionByPivot(item.current, prevPosition, pivotDecide);

                    FixeditemMargin(item.current, itemMargin);
                }
                else
                {
                    //next item of group
                    Vector3 prevPosition = UnityHelper.GetLocalPositionByPivot(item.prev.current, _currentGroupItemPivotPrev);
                    UnityHelper.SetLocalPositionByPivot(item.current, prevPosition, _currentGroupItemPivotNext);

                    FixeditemMargin(item.current, groupItemMargin);
                }
            }
        }

        private void UpdateGroupItemDirection()
        {
            if (groupItemDirection == shaco.Direction.Automatic)
            {
                switch (scrollDirection)
                {
                    case shaco.Direction.Right: groupItemDirection = shaco.Direction.Down; break;
                    case shaco.Direction.Left: groupItemDirection = shaco.Direction.Down; break;
                    case shaco.Direction.Up: groupItemDirection = shaco.Direction.Right; break;
                    case shaco.Direction.Down: groupItemDirection = shaco.Direction.Right; break;
                    default: Log.Error("unsupport direction"); break;
                }
                UpdateGroupItemDirection();
            }
            else
            {
                switch (groupItemDirection)
                {
                    case shaco.Direction.Right: _currentGroupItemPivotPrev = shaco.Pivot.RightBottom; _currentGroupItemPivotNext = shaco.Pivot.LeftBottom; break;
                    case shaco.Direction.Left: _currentGroupItemPivotPrev = shaco.Pivot.LeftBottom; _currentGroupItemPivotNext = shaco.Pivot.RightBottom; break;
                    case shaco.Direction.Up: _currentGroupItemPivotPrev = shaco.Pivot.LeftTop; _currentGroupItemPivotNext = shaco.Pivot.LeftBottom; break;
                    case shaco.Direction.Down: _currentGroupItemPivotPrev = shaco.Pivot.LeftBottom; _currentGroupItemPivotNext = shaco.Pivot.LeftTop; break;
                    default: Log.Error("unsupport direction"); break;
                }
            }
        }

        private void UpdateContentLayout()
        {
            if (!autoUpdateContentSize)
                return;

            var content = _scrollRectContent.content;
            var allItemSize = GetAllSizeItem();

            float widthNew = content.rect.width;
            float heightNew = content.rect.height;
            Vector3 pivotContent = GetContentPivot();

            switch (scrollDirection)
            {
                case shaco.Direction.Right:
                    {
                        widthNew = allItemSize.x + firstItemOffset.x;
                        heightNew = GetMaxHeightItem();
                        break;
                    }
                case shaco.Direction.Left:
                    {
                        widthNew = allItemSize.x + firstItemOffset.x;
                        heightNew = GetMaxHeightItem();
                        break;
                    }
                case shaco.Direction.Up:
                    {
                        widthNew = GetMaxWidthItem();
                        heightNew = allItemSize.y + firstItemOffset.y;
                        break;
                    }
                case shaco.Direction.Down:
                    {
                        widthNew = GetMaxWidthItem();
                        heightNew = allItemSize.y + firstItemOffset.y;
                        break;
                    }
                default: Log.Error("updateContentSize error: unsupport direction !"); break;
            }

            content.pivot = pivotContent;
            content.sizeDelta = new Vector3(widthNew, heightNew);

            if (isCenterLayout)
            {
                pivotContent = CheckContentPivotWhenCenterLayout(pivotContent);
            }

            var pos1 = UnityHelper.GetLocalPositionByPivot(this.gameObject, pivotContent);
            UnityHelper.SetLocalPositionByPivot(content.gameObject, pos1, pivotContent);
            content.transform.localPosition -= this.transform.localPosition;
            
            CheckCompoment();
        }

        private Vector3 GetContentPivot()
        {
            Vector3 retValue = shaco.Pivot.LeftTop;

            switch (groupItemDirection)
            {
                case shaco.Direction.Right:
                    {
                        retValue = scrollDirection == shaco.Direction.Up ? shaco.Pivot.LeftBottom : shaco.Pivot.LeftTop;
                        break;
                    }
                case shaco.Direction.Left:
                    {
                        retValue = scrollDirection == shaco.Direction.Down ? shaco.Pivot.RightTop : shaco.Pivot.RightBottom;
                        break;
                    }
                case shaco.Direction.Up:
                    {
                        retValue = scrollDirection == shaco.Direction.Left ? shaco.Pivot.RightBottom : shaco.Pivot.LeftBottom;
                        break;
                    }
                case shaco.Direction.Down:
                    {
                        retValue = scrollDirection == shaco.Direction.Left ? shaco.Pivot.RightTop : shaco.Pivot.LeftTop;
                        break;
                    }
                default: Log.Error("ListView GetContentPivot error: unsupport direction !"); break;
            }
            return retValue;
        }

        private float GetMaxWidthItem()
        {
            float ret = 0;
            for (int i = 0; i < _listItems.Count; ++i)
            {
                var rectTrans = _listItems[i].current.transform.GetComponent<RectTransform>();
                if (ret < rectTrans.rect.width)
                    ret = rectTrans.rect.width;
            }
            return ret;
        }

        private float GetMaxHeightItem()
        {
            float ret = 0;
            for (int i = 0; i < _listItems.Count; ++i)
            {
                var rectTrans = _listItems[i].current.transform.GetComponent<RectTransform>();
                if (ret < rectTrans.rect.height)
                    ret = rectTrans.rect.height;
            }
            return ret;
        }

        private Vector3 GetAllSizeItem()
        {
            Vector3 ret = Vector3.zero;
            for (int i = 0; i < _listItems.Count; i += eachOfGroup)
            {
                var rectTrans = _listItems[i].current.transform.GetComponent<RectTransform>();
                ret += new Vector3(rectTrans.rect.width, rectTrans.rect.height);
            }

            var absitemMargin = new Vector3(itemMargin.x, itemMargin.y, itemMargin.z);
            if (!isCenterLayout)
            {
                if (_scrollRectContent.horizontal)
                {
                    absitemMargin.x = Mathf.Abs(absitemMargin.x);
                }
                if (_scrollRectContent.vertical)
                {
                    absitemMargin.y = Mathf.Abs(absitemMargin.y);
                } 
            }
            ret += ((_listItems.Count - 1) / eachOfGroup) * absitemMargin;

            return ret;
        }

        private void FixedLocalPositionByContent(GameObject target)
        {
            var contentTmp = _scrollRectContent.content;

            var rectTrans = _scrollRectContent.content.GetComponent<RectTransform>();
            if (rectTrans == null)
            {
                Log.Error("FixedLocalPositionByContent error: dose not contain RectTransform !");
                return;
            }

            var rectTransTarget = target.GetComponent<RectTransform>();
            if (rectTransTarget == null)
            {
                Log.Error("setLocalPositionByArchor error: target dose not contain RectTransform !");
                return;
            }

            target.transform.localPosition -= new Vector3(contentTmp.pivot.x * contentTmp.rect.width, contentTmp.pivot.y * contentTmp.rect.height);

            if (firstItemOffset.x != 0 || firstItemOffset.y != 0)
            {
                target.transform.localPosition += new Vector3(firstItemOffset.x, firstItemOffset.y);
            }
        }

        private void FixeditemMargin(GameObject target, Vector3 margin)
        {
            target.transform.localPosition += margin;
        }

        private void OnScrollingCallBack()
        {
            CheckOutOfBoundsChanged();
        }

        private void OnBeginDragCallBack(UnityEngine.EventSystems.PointerEventData eventData)
        {
            _prevContentLocalPosition = GetTouchPosition();
        }

        private void OnEndDragCallBack(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (null == onItemAutoUpdateCallBack || autoUpdateItemCountWhenSpringback <= 0 || _listItems.Count == 0) return;

            shaco.Direction springBackDirection = shaco.Direction.Automatic;
            float outOfBoundsRate = 0;
            int autoUpdateItemCount = autoUpdateItemCountWhenSpringback;
            Vector3 newContentWorldPosition = Vector3.zero;
            Vector3 newContentPivot = Vector3.zero;
            CheckDragOutOfBounds(out springBackDirection, out outOfBoundsRate, out newContentWorldPosition, out newContentPivot);
            if (springBackDirection == shaco.Direction.Automatic)
            {
                return;
            }

            if (springBackDirection == shaco.Direction.Up || springBackDirection == shaco.Direction.Left)
            {
                int startIndex = _itemIndexOffsetUseInSpringbackCallBack + _listItems.Count;
                int endIndex = startIndex + autoUpdateItemCount - 1;
                CheckAutoItemSize(endIndex);
            }
            else if (springBackDirection == shaco.Direction.Down || springBackDirection == shaco.Direction.Right)
            {
                int startIndex = _itemIndexOffsetUseInSpringbackCallBack - 1;
                int endIndex = startIndex - autoUpdateItemCount + 1;
                CheckAutoItemSize(endIndex);
            }
        }

        private GameObject GetFrontItem()
        {
            GameObject retValue = null;
            switch (scrollDirection)
            {
                case shaco.Direction.Right:
                case shaco.Direction.Up: retValue = GetFirstItem(); break;
                case shaco.Direction.Left:
                case shaco.Direction.Down: retValue = GetLastItem(); break;
                default: Log.Error("unsupport direction"); break;
            }
            return retValue;
        }

        private GameObject GetBehindItem()
        {
            GameObject retValue = null;
            switch (scrollDirection)
            {
                case shaco.Direction.Right:
                case shaco.Direction.Up: retValue = GetLastItem(); break;
                case shaco.Direction.Left:
                case shaco.Direction.Down: retValue = GetFirstItem(); break;
                default: Log.Error("unsupport direction"); break;
            }
            return retValue;
        }

        private Vector3 GetTouchPosition()
        {
#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
            return Input.touchCount > 0 ? Input.GetTouch(0).position : Vector2.zero;
#else
            return Input.mousePosition;
#endif
        }

        private void CheckDragOutOfBounds(out shaco.Direction springBackDirection, out float outOfBoundsRate, out Vector3 newContentWorldPosition, out Vector3 newContentPivot)
        {
            springBackDirection = shaco.Direction.Automatic;
            outOfBoundsRate = -1;
            newContentWorldPosition = Vector3.zero;
            newContentPivot = Vector3.zero;

            var rectTrans = _scrollRectContent.content;
            var contentMoveOffset = GetTouchPosition() - _prevContentLocalPosition;

            float dragOffset = 0;
            float maxBoundsOffset = 0;
            bool isNegative = scrollDirection == shaco.Direction.Left || scrollDirection == shaco.Direction.Down;
            bool isHorizontol = scrollDirection == shaco.Direction.Left || scrollDirection == shaco.Direction.Right;
            bool isVertical = scrollDirection == shaco.Direction.Down || scrollDirection == shaco.Direction.Up;
            float frontOutOfBoundsRate = 0;
            float behindOutOfBoundsRate = 0;
            Vector3 pivotTmp = Vector3.zero;

            if (isMultipleDragOutOfBoundsSet)
            {
                frontOutOfBoundsRate = isNegative ? maxDragOutOfBehindBoundsRatio : maxDragOutOfFrontBoundsRatio;
                behindOutOfBoundsRate = isNegative ? maxDragOutOfFrontBoundsRatio : maxDragOutOfBehindBoundsRatio;
            }
            else
            {
                frontOutOfBoundsRate = maxDragOutOfFrontBoundsRatio;
                behindOutOfBoundsRate = maxDragOutOfFrontBoundsRatio;
            }

            if (isHorizontol)
            {
                if (contentMoveOffset.x < 0)
                {
                    pivotTmp = scrollDirection == shaco.Direction.Right && rectTrans.sizeDelta.x < this.GetComponent<RectTransform>().sizeDelta.x ? shaco.Pivot.LeftTop : shaco.Pivot.RightTop;
                    pivotTmp = CheckContentPivotWhenCenterLayout(pivotTmp);
                    var contentPositionTmp = UnityHelper.GetWorldPositionByPivot(rectTrans.gameObject, pivotTmp);
                    var listviewPositionTmp = UnityHelper.GetWorldPositionByPivot(this.gameObject, pivotTmp);
                    dragOffset = listviewPositionTmp.x - contentPositionTmp.x;
                    maxBoundsOffset = GetItemSizeByWorldPosition(GetBehindItem()).x * behindOutOfBoundsRate;

                    if (behindOutOfBoundsRate > 0 && dragOffset >= maxBoundsOffset * CHECK_OUT_OF_BOUNDS_FIXED_RATE)
                    {
                        springBackDirection = isNegative ? shaco.Direction.Right : shaco.Direction.Left;
                    }
                    if (dragOffset >= maxBoundsOffset)
                    {
                        newContentWorldPosition = listviewPositionTmp - new Vector3(maxBoundsOffset, 0);
                        newContentPivot = pivotTmp;
                    }
                }
                else
                {
                    pivotTmp = scrollDirection == shaco.Direction.Left && rectTrans.sizeDelta.x < this.GetComponent<RectTransform>().sizeDelta.x ? shaco.Pivot.RightTop : shaco.Pivot.LeftTop;
                    pivotTmp = CheckContentPivotWhenCenterLayout(pivotTmp);
                    var contentPositionTmp = UnityHelper.GetWorldPositionByPivot(rectTrans.gameObject, pivotTmp);
                    var listviewPositionTmp = UnityHelper.GetWorldPositionByPivot(this.gameObject, pivotTmp);
                    dragOffset = contentPositionTmp.x - listviewPositionTmp.x;
                    maxBoundsOffset = GetItemSizeByWorldPosition(GetFrontItem()).x * frontOutOfBoundsRate;

                    if (frontOutOfBoundsRate > 0 && dragOffset >= maxBoundsOffset * CHECK_OUT_OF_BOUNDS_FIXED_RATE)
                    {
                        springBackDirection = isNegative ? shaco.Direction.Left : shaco.Direction.Right;
                    }
                    if (dragOffset >= maxBoundsOffset)
                    {
                        newContentWorldPosition = listviewPositionTmp + new Vector3(maxBoundsOffset, 0);
                        newContentPivot = pivotTmp;
                    }
                }
            }
            else if (isVertical)
            {
                if (contentMoveOffset.y < 0)
                {
                    pivotTmp = scrollDirection == shaco.Direction.Up && rectTrans.sizeDelta.y < this.GetComponent<RectTransform>().sizeDelta.y ? shaco.Pivot.LeftBottom : shaco.Pivot.LeftTop;
                    pivotTmp = CheckContentPivotWhenCenterLayout(pivotTmp);
                    var contentPositionTmp = UnityHelper.GetWorldPositionByPivot(rectTrans.gameObject, pivotTmp);
                    var listviewPositionTmp = UnityHelper.GetWorldPositionByPivot(this.gameObject, pivotTmp);

                    dragOffset = listviewPositionTmp.y - contentPositionTmp.y;
                    maxBoundsOffset = GetItemSizeByWorldPosition(GetFrontItem()).y * behindOutOfBoundsRate;

                    if (behindOutOfBoundsRate > 0 && dragOffset >= maxBoundsOffset * CHECK_OUT_OF_BOUNDS_FIXED_RATE)
                    {
                        springBackDirection = isNegative ? shaco.Direction.Down : shaco.Direction.Up;
                    }
                    if (dragOffset >= maxBoundsOffset)
                    {
                        newContentWorldPosition = listviewPositionTmp - new Vector3(0, maxBoundsOffset);
                        newContentPivot = pivotTmp;
                    }
                }
                else
                {
                    pivotTmp = scrollDirection == shaco.Direction.Down && rectTrans.sizeDelta.y < this.GetComponent<RectTransform>().sizeDelta.y ? shaco.Pivot.LeftTop : shaco.Pivot.LeftBottom;
                    pivotTmp = CheckContentPivotWhenCenterLayout(pivotTmp);
                    var contentPositionTmp = UnityHelper.GetWorldPositionByPivot(rectTrans.gameObject, pivotTmp);
                    var listviewPositionTmp = UnityHelper.GetWorldPositionByPivot(this.gameObject, pivotTmp);

                    dragOffset = contentPositionTmp.y - listviewPositionTmp.y;
                    maxBoundsOffset = GetItemSizeByWorldPosition(GetBehindItem()).y * frontOutOfBoundsRate;

                    if (frontOutOfBoundsRate > 0 && dragOffset >= maxBoundsOffset * CHECK_OUT_OF_BOUNDS_FIXED_RATE)
                    {
                        springBackDirection = isNegative ? shaco.Direction.Up : shaco.Direction.Down;
                    }
                    if (dragOffset >= maxBoundsOffset)
                    {
                        newContentWorldPosition = listviewPositionTmp + new Vector3(0, maxBoundsOffset);
                        newContentPivot = pivotTmp;
                    }
                }
            }

            if (maxBoundsOffset > 0)
                outOfBoundsRate = dragOffset / maxBoundsOffset;
            else if (dragOffset > 0)
            {
                outOfBoundsRate = 1;
                _scrollRectContent.StopMovement();
            }
            else if (maxBoundsOffset == 0 && dragOffset == 0)
            {
                _scrollRectContent.StopMovement();
            }
        }

        private Vector3 CheckContentPivotWhenCenterLayout(Vector3 pivot)
        {
            if (isCenterLayout)
            {
                if (!_scrollRectContent.horizontal)
                {
                    pivot.x = shaco.Pivot.Middle.x;
                }
                if (!_scrollRectContent.vertical)
                {
                    pivot.y = shaco.Pivot.Middle.y;
                }
            }
            return pivot;
        }

        private Vector2 GetItemSizeByWorldPosition(GameObject target)
        {
            var posLeftBottom = UnityHelper.GetWorldPositionByPivot(target, shaco.Pivot.LeftBottom);
            var posRightTop = UnityHelper.GetWorldPositionByPivot(target, shaco.Pivot.RightTop);
            return new Vector2(posRightTop.x - posLeftBottom.x, posRightTop.y - posLeftBottom.y);
        }

        private void RetainContentPositionByFirstItem()
        {
            if (_listItems.Count <= 0)
                return;

            _firstItemWhenEndGrag = _listItems[0];
            var rectTrans = _scrollRectContent.content;
            var contentRect = new Rect(rectTrans.localPosition.x, rectTrans.localPosition.y, rectTrans.rect.width, rectTrans.rect.height);
            var firstItemTrans = _firstItemWhenEndGrag.current.GetComponent<RectTransform>();
            var oldPosition = firstItemTrans.localPosition;

            _fixContentOffsetWhenEndGrag = contentRect.position + new Vector2(oldPosition.x, oldPosition.y);
        }

        private void RetainContentPositionByLastItem()
        {
            if (_listItems.Count <= 0)
                return;

            _lastItemWhenEndGrag = _listItems[_listItems.Count - 1];
            var rectTrans = _scrollRectContent.content;
            var contentRect = new Rect(rectTrans.localPosition.x, rectTrans.localPosition.y, rectTrans.rect.width, rectTrans.rect.height);
            var lastItemTrans = _lastItemWhenEndGrag.current.GetComponent<RectTransform>();
            var oldPosition = lastItemTrans.localPosition;

            _fixContentOffsetWhenEndGrag = contentRect.position + new Vector2(oldPosition.x, oldPosition.y);
        }

        private void FixScrollContentPositionWhenEndDrag()
        {
            if (_fixContentOffsetWhenEndGrag != Vector3.zero)
            {
                var scrollContentTmp = _scrollRectContent.content;

                var oldContentLocalPos = scrollContentTmp.localPosition;

                //fix last item
                if (null != _lastItemWhenEndGrag)
                {
                    var posTmp = _lastItemWhenEndGrag.current.transform.localPosition;
                    var newScrollContentPos = new Vector3(0, 0, scrollContentTmp.localPosition.z);
                    scrollContentTmp.localPosition = newScrollContentPos - posTmp + _fixContentOffsetWhenEndGrag;
                    _lastItemWhenEndGrag = null;
                }
                //fix first item
                if (null != _firstItemWhenEndGrag)
                {
                    var posTmp = _firstItemWhenEndGrag.current.transform.localPosition;
                    var newScrollContentPos = new Vector3(0, 0, scrollContentTmp.localPosition.z);
                    scrollContentTmp.localPosition = newScrollContentPos - posTmp + _fixContentOffsetWhenEndGrag;
                    _firstItemWhenEndGrag = null;
                }

                switch (scrollDirection)
                {
                    case shaco.Direction.Right:
                    case shaco.Direction.Left: scrollContentTmp.localPosition = new Vector3(scrollContentTmp.localPosition.x, oldContentLocalPos.y, scrollContentTmp.localPosition.z); break;
                    case shaco.Direction.Down:
                    case shaco.Direction.Up: scrollContentTmp.localPosition = new Vector3(oldContentLocalPos.x, scrollContentTmp.localPosition.y, scrollContentTmp.localPosition.z); break;
                    default: Log.Error("unsupport type !"); break;
                }
                _fixContentOffsetWhenEndGrag = Vector3.zero;
            }
        }

        private bool IsItemInVisibleArea(Item item)
        {
            var rectArea = _scrollRectContent.content.rect;
            var rectItem = item.current.GetComponent<RectTransform>().rect;
            return rectArea.Contains(rectItem);
        }

        private void CheckOutOfBoundsChanged()
        {
            if (_listItems.Count == 0) return;

            CheckRemainCountPromptArrow();
            CheckOutOfBoundsRatioLimit();

            //check out of bounds callback 
            if (null != onItemsDragOutOfBoundsCallBack)
            {
                shaco.Direction springBackDirection = shaco.Direction.Automatic;
                float outOfBoundsRate = -1;
                Vector3 newContentWorldPosition = Vector3.zero;
                Vector3 newContentPivot = Vector3.zero;
                CheckDragOutOfBounds(out springBackDirection, out outOfBoundsRate, out newContentWorldPosition, out newContentPivot);
                if (outOfBoundsRate > 0.1f)
                {
                    if (outOfBoundsRate > 1)
                        outOfBoundsRate = 1;

                    onItemsDragOutOfBoundsCallBack(outOfBoundsRate);
                }
            }
        }

        private void CheckRemainCountPromptArrow()
        {
            if (frontArrow == null && behindArrow == null) return;

            float offsetTmp = 0.5f;
            bool shouldShowFrontArrow = false;
            bool shouldHideFrontArrow = false;
            bool shouldShowBehindArrow = false;
            bool shouldHideBehindArrow = false;
            int startIndex = _itemIndexOffsetUseInSpringbackCallBack;
            int endIndex = _itemIndexOffsetUseInSpringbackCallBack + _listItems.Count;
            bool haveMoreFrontItem = startIndex > autoUpdateItemMinIndex;
            bool haveMoreBehindItem = endIndex < autoUpdateItemMaxIndex;

            switch (scrollDirection)
            {
                case shaco.Direction.Left:
                case shaco.Direction.Right:
                    {
                        var posLeftContent = UnityHelper.GetWorldPositionByPivot(_scrollRectContent.content.gameObject, shaco.Pivot.LeftBottom);
                        var posRightContent = UnityHelper.GetWorldPositionByPivot(_scrollRectContent.content.gameObject, shaco.Pivot.RightBottom);
                        var posLeftView = UnityHelper.GetWorldPositionByPivot(this.gameObject, shaco.Pivot.LeftBottom);
                        var posRightView = UnityHelper.GetWorldPositionByPivot(this.gameObject, shaco.Pivot.RightBottom);
                        if (posLeftContent.x + offsetTmp >= posLeftView.x)
                            shouldHideFrontArrow = true;
                        else
                            shouldShowFrontArrow = true;
                        if (posRightContent.x - offsetTmp <= posRightView.x)
                            shouldHideBehindArrow = true;
                        else
                            shouldShowBehindArrow = true;

                        if (scrollDirection == shaco.Direction.Left)
                        {
                            haveMoreFrontItem = haveMoreFrontItem.SwapValue(ref haveMoreBehindItem);
                        }
                        break;
                    }
                case shaco.Direction.Up:
                case shaco.Direction.Down:
                    {
                        var posUpContent = UnityHelper.GetWorldPositionByPivot(_scrollRectContent.content.gameObject, shaco.Pivot.LeftTop);
                        var posDownContent = UnityHelper.GetWorldPositionByPivot(_scrollRectContent.content.gameObject, shaco.Pivot.LeftBottom);
                        var posUpView = UnityHelper.GetWorldPositionByPivot(this.gameObject, shaco.Pivot.LeftTop);
                        var posDownView = UnityHelper.GetWorldPositionByPivot(this.gameObject, shaco.Pivot.LeftBottom);

                        if (posUpContent.y <= posUpView.y + offsetTmp)
                            shouldHideFrontArrow = true;
                        else
                            shouldShowFrontArrow = true;
                        if (posDownContent.y + offsetTmp >= posDownView.y)
                            shouldHideBehindArrow = true;
                        else
                            shouldShowBehindArrow = true;

                        if (scrollDirection == shaco.Direction.Up)
                        {
                            haveMoreFrontItem = haveMoreFrontItem.SwapValue(ref haveMoreBehindItem);
                        }
                        break;
                    }
                default: Log.Error("unsupport direction=" + scrollDirection); break;
            }

            //check arrow active
            if (shouldShowFrontArrow && _isOutOfFrontBounds)
            {
                if (null != frontArrow)
                    frontArrow.SetActive(true);
                _isOutOfFrontBounds = false;
            }
            else if (shouldHideFrontArrow && !_isOutOfFrontBounds && !haveMoreFrontItem)
            {
                if (null != frontArrow)
                    frontArrow.SetActive(false);
                _isOutOfFrontBounds = true;
            }

            if (shouldShowBehindArrow && _isOutOfBehindBounds)
            {
                if (null != behindArrow)
                    behindArrow.SetActive(true);
                _isOutOfBehindBounds = false;
            }
            else if (shouldHideBehindArrow && !_isOutOfBehindBounds && !haveMoreBehindItem)
            {
                if (null != behindArrow) behindArrow.SetActive(false);
                _isOutOfBehindBounds = true;
            }
        }

        private void CheckOutOfBoundsRatioLimit()
        {
            if (_listItems.Count <= 0 || maxDragOutOfFrontBoundsRatio == 1.0f && maxDragOutOfBehindBoundsRatio == 1.0f)
                return;

            shaco.Direction sprintbackDirection = shaco.Direction.Automatic;
            float outOfBoundsRate = 0;
            Vector3 newContentWorldPosition = Vector3.zero;
            Vector3 newContentPivot = Vector3.zero;

            CheckDragOutOfBounds(out sprintbackDirection, out outOfBoundsRate, out newContentWorldPosition, out newContentPivot);
            if (outOfBoundsRate >= 1.0f)
            {
                shaco.UnityHelper.SetWorldPositionByPivot(_scrollRectContent.content.gameObject, newContentWorldPosition, newContentPivot);
            }
        }

        static public void ChangeParentLocal(GameObject target, GameObject parent)
        {
            var oldPos = target.transform.localPosition;
            var oldScale = target.transform.localScale;

            target.transform.SetParent(parent.transform);

            target.transform.localPosition = oldPos;
            target.transform.localScale = oldScale;
        }
    }
}

