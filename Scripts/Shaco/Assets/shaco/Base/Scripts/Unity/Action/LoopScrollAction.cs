
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace shaco
{
    [RequireComponent(typeof(RectTransform))]
    public class LoopScrollAction : MonoBehaviour
    {
        //滚动方向
        public shaco.Direction scrollDirection = shaco.Direction.Up;

        //滚动速度
        public float perNumberScrollTime = 0.1f; 

        //每轮滚动完毕回掉
        //<滚动第几轮>
        //返回false则立即停止滚动
        public System.Func<int, bool> onScrollCallBack;

        //滚动动画接口，如果为空则使用默认滚动动画
        //<滚动偏移量(本地坐标)，滚动时间>
        //返回动画对象
        public System.Func<Vector3, float, ActionS> delegateScrollAction;

        //滚动对象，自动用子节点填充该数组
        private List<RectTransform> scrollTargets = new List<RectTransform>();

        private GridLayoutGroup layoutGroup;

        private readonly int MOVE_ACTION_TAG = 10;
        
        private bool _isScrolling = false;

        //请求停止滚动
        private bool _isRequestStopScroll = false;

        //执行滚动动画
        /// <param name="scrollTimes">总滚动循环次数，如果小于0则无限循环</param>
        public bool RunScrollAction(int scrollTimes = 0, bool autoUpdateLayout = true)
        {
            if (autoUpdateLayout)
            {
                UpdateTextLayout();
            }

            if (scrollTargets.Count <= 1)
            {
                shaco.Log.Error("LoopScrollAction RunScrollAction error: no action target");
                return false;
            }

            if (_isScrolling)
            {
                shaco.Log.Warning("LoopScrollAction RunScrollAction warning: scrolling, please wait...");
                return false;
            }
            _isScrolling = true;

            var firstTmp = scrollTargets[0];
            var rectTransTmp = firstTmp.GetComponent<RectTransform>();
            if (rectTransTmp == null)
            {
                shaco.Log.Error("LoopScrollAction RunScrollAction error: first action target missing 'RectTransform'");
                return false;
            }

            _isRequestStopScroll = false;

            StartCoroutine(DelayRunScrollAction(scrollDirection, perNumberScrollTime, 1, scrollTimes));
            return true;
        }

        public void UpdateTextLayout()
        {
            CheckLayoutGroup(scrollDirection);
            UpdateActionTargets();
        }

        //获取滚动对象
        public RectTransform GetScrollTarget(int index)
        {
            RectTransform retValue = null;
            if (index < 0 || index > scrollTargets.Count - 1)
            {
                shaco.Log.Error("LoopScrollAction GetScrollTarget error: out of range, index=" + index + " count=" + scrollTargets.Count);
                return retValue;
            }
            return scrollTargets[index];
        }

        //停止滚动
        public void StopScroll()
        {
            _isRequestStopScroll = true;
            for (int i = 0; i < scrollTargets.Count; ++i)
            {
                scrollTargets[i].gameObject.StopActionByTag(MOVE_ACTION_TAG);   
            }
        }

        //刷新动画对象
        private void UpdateActionTargets()
        {
            scrollTargets.Clear();
            foreach (var child in this.transform)
            {
                var rectTransTmp = child as RectTransform;
                if (rectTransTmp == null)
                {
                    shaco.Log.Error("LoopScrollAction CheckLayoutGroup error: not have 'RectTransform' Component, target=" + child);
                }
                else
                {
                    scrollTargets.Add(rectTransTmp);
                }
            }
        }

        private IEnumerator DelayRunScrollAction(shaco.Direction scrollDirection, float duration, int currentMoveTims, int scrollTimes)
        {
            yield return 1;
            RunScrollActionLoop(scrollDirection, duration, currentMoveTims, scrollTimes);
        }

        private void RunScrollActionLoop(shaco.Direction scrollDirection, float duration, int currentMoveTims, int scrollTimes)
        {
            if (scrollTargets.Count <= 1)
                return;
            
            var moveDelta = GetScrollDelta(scrollDirection);
            ActionS moveActionTmp = null;
            
            for (int i = 0; i < scrollTargets.Count; ++i)
            {
                var targetTmp = scrollTargets[i];

                if (null == delegateScrollAction)
                {
                    moveActionTmp = targetTmp.gameObject.MoveBy(moveDelta, duration, false);
                }
                else 
                {
                    moveActionTmp = delegateScrollAction(moveDelta, duration);
                }
                moveActionTmp.Tag = MOVE_ACTION_TAG;
            }

            if (null != moveActionTmp)
            {
                moveActionTmp.onCompleteFunc += (ActionS ac) =>
                {
                    if (!Application.isPlaying)
                    {
                        return;
                    }

                    moveActionTmp.StopMe();

                    //回滚第一个组建到最后一个去
                    if (scrollTargets.Count > 1 && scrollTimes == 0)
                    {
                        var firstTmp = scrollTargets[0];
                        var lastTmp = scrollTargets[scrollTargets.Count - 1];

                        //设置在末尾位置
                        var pivotGetTmp = Vector3.zero;
                        var pivotSetTmp = Vector3.zero;
                        switch (scrollDirection)
                        {
                            case shaco.Direction.Up: break;
                            case shaco.Direction.Down: pivotGetTmp = shaco.Pivot.MiddleTop; pivotSetTmp = shaco.Pivot.MiddleBottom; break;
                            case shaco.Direction.Left: break;
                            case shaco.Direction.Right: break;
                            default: shaco.Log.Error("LoopScrollAction RunScrollActionLoop error: unsupport direction=" + scrollDirection); break;
                        }
                        UnityHelper.SetLocalPositionByPivot(firstTmp.gameObject, UnityHelper.GetLocalPositionByPivot(lastTmp.gameObject, pivotGetTmp), pivotSetTmp);
                        firstTmp.transform.SetAsLastSibling();

                        //移动节点顺序
                        scrollTargets.RemoveAt(0);
                        scrollTargets.Add(firstTmp);
                    }
                    
                    if (scrollTimes <= 0 || currentMoveTims < scrollTimes)
                    {
                        bool goOnScroll = true;
                        if (null != onScrollCallBack)
                        {
                            goOnScroll = onScrollCallBack(currentMoveTims);
                        }
                        if (goOnScroll && !_isRequestStopScroll)
                        {
                            RunScrollActionLoop(scrollDirection, duration, ++currentMoveTims, scrollTimes);
                        }
                    }
                    else 
                    {
                        StartCoroutine(DelayPauseScrollingFlag());
                    }
                };
            }
        }

        private IEnumerator DelayPauseScrollingFlag()
        {
            yield return 1;
            _isScrolling = false;
        }

        private Vector3 GetScrollDelta(shaco.Direction scrollDirection)
        {
            var retValue = Vector3.zero;

            switch (scrollDirection)
            {
                case shaco.Direction.Up:
                    {
                        retValue = new Vector3(0, layoutGroup.cellSize.y);
                        break;
                    }
                case shaco.Direction.Down:
                    {
                        retValue = new Vector3(0, -layoutGroup.cellSize.y);
                        break;
                    }
                case shaco.Direction.Left:
                    {
                        retValue = new Vector3(-layoutGroup.cellSize.x, 0);
                        break;
                    }
                case shaco.Direction.Right:
                    {
                        retValue = new Vector3(layoutGroup.cellSize.x, 0);
                        break;
                    }
                default: shaco.Log.Error("LoopScrollAction GetScrollDelta error: unsupport direction=" + scrollDirection); break;
            }
            return retValue;
        }

        private void CheckLayoutGroup(shaco.Direction scrollDirection)
        {
            if (null == layoutGroup)
            {
                layoutGroup = this.gameObject.GetComponent<GridLayoutGroup>();
                if (null == layoutGroup)
                {
                    layoutGroup = this.gameObject.AddComponent<GridLayoutGroup>();
                }
            }

            if (null != layoutGroup)
            {
                layoutGroup.cellSize = UnityHelper.GetRealSize(this.GetComponent<RectTransform>());

                switch (scrollDirection)
                {
                    case shaco.Direction.Up:
                        {
                            layoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
                            layoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
                            layoutGroup.childAlignment = TextAnchor.UpperLeft;
                            break;
                        }
                    case shaco.Direction.Down:
                        {
                            layoutGroup.startCorner = GridLayoutGroup.Corner.LowerLeft;
                            layoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
                            layoutGroup.childAlignment = TextAnchor.LowerLeft;
                            break;
                        }
                    case shaco.Direction.Left:
                        {
                            layoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
                            layoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
                            layoutGroup.childAlignment = TextAnchor.UpperLeft;
                            break;
                        }
                    case shaco.Direction.Right:
                        {
                            layoutGroup.startCorner = GridLayoutGroup.Corner.UpperRight;
                            layoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
                            layoutGroup.childAlignment = TextAnchor.UpperRight;
                            break;
                        }
                    default: shaco.Log.Error("LoopScrollAction CheckLayoutGroup error: unsupport direction=" + scrollDirection); break;
                }
            }

            layoutGroup.enabled = false;
            layoutGroup.enabled = true;
        }
    }
}