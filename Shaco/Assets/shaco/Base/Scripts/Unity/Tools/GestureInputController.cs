using UnityEngine;
using System.Collections;

namespace shaco
{
    //支持Andoird、iOS、PC平台
    public class GestureInputController : MonoBehaviour
    {
        public enum TouchState
        {
            None,
            TOUCH_DOWN,
            TOUCH,
            TOUCH_UP
        }

        //手势事件回调
        public UnityEngine.Events.UnityEvent onGestureDirection;

        //按下触摸位置
        [HideInInspector]
        public Vector3 currentTouchDownPosition = Vector3.zero;

        //当前触摸位置
        public Vector3 currentTouchPosition { get { return fingerCurrent; } }

        //当前手势滑动方向
        [HideInInspector]
        public shaco.Direction currentGestureDirection = shaco.Direction.None;
        //当前点击状态
        [HideInInspector]
        public TouchState touchState = TouchState.None;
        //当前滑动距离
        [HideInInspector]
        public Vector3 deltaPosition 
        {
            get { return fingerSegment; }
        }
        
        //滑动的敏感度，超过该范围则判定为有滑动方向
        private float fingerActionSensitivity = Screen.width * 1 / 30;
        private Vector3 fingerBegin = Vector3.zero;
        private Vector3 fingerCurrent = Vector3.zero;
        private Vector3 fingerSegment = Vector3.zero;

        void Update()
        {
            switch (touchState)
            {
                case TouchState.TOUCH_UP: touchState = TouchState.None; break;
                case TouchState.TOUCH_DOWN: touchState = TouchState.TOUCH; break;
                default: break;
            }

#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
#else 
            if (Input.GetKeyDown(KeyCode.Mouse0))
#endif
            {
                if (touchState == TouchState.None)
                {
                    touchState = TouchState.TOUCH_DOWN;
                    fingerBegin = GetTouchPosition();
                    currentTouchDownPosition = fingerBegin;
                }
            }

            if (touchState == TouchState.TOUCH)
            {
                fingerCurrent = GetTouchPosition();
                fingerSegment = fingerCurrent - fingerBegin;
                fingerBegin = GetTouchPosition();
            }

            CalculateGestureDirection();

#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
#else
            if (Input.GetKeyUp(KeyCode.Mouse0))
#endif
            {
                DispatchGestureDirectionEvent();

                touchState = TouchState.TOUCH_UP;
                currentGestureDirection = shaco.Direction.None;
            }
        }

        private Vector3 GetTouchPosition()
        {
#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
            return Input.touchCount > 0 ? Input.GetTouch(0).position : Vector2.zero;
#else
            return Input.mousePosition;
#endif
        }

        private void CalculateGestureDirection()
        {
            if (touchState == TouchState.TOUCH)
            {
                float fingerDistance = fingerSegment.x * fingerSegment.x + fingerSegment.y * fingerSegment.y;

                if (fingerDistance > (fingerActionSensitivity * fingerActionSensitivity))
                {
                    if (Mathf.Abs(fingerSegment.x) > Mathf.Abs(fingerSegment.y))
                    {
                        fingerSegment.y = 0;
                    }
                    else
                    {
                        fingerSegment.x = 0;
                    }

                    if (fingerSegment.x == 0)
                    {
                        if (fingerSegment.y > 0)
                        {
                            CheckDisableGestureDirection(shaco.Direction.Up);
                        }
                        else
                        {
                            CheckDisableGestureDirection(shaco.Direction.Down);
                        }
                    }
                    else if (fingerSegment.y == 0)
                    {
                        if (fingerSegment.x > 0)
                        {
                            CheckDisableGestureDirection(shaco.Direction.Right);
                        }
                        else
                        {
                            CheckDisableGestureDirection(shaco.Direction.Left);
                        }
                    }
                }
            }
        }

        private void CheckDisableGestureDirection(shaco.Direction direction)
        {
            if (shaco.Direction.None == currentGestureDirection || direction == currentGestureDirection)
            {
                currentGestureDirection = direction;
            }
            else
            {
                currentGestureDirection = shaco.Direction.None;
            }
        }

        private void DispatchGestureDirectionEvent()
        {
            if (shaco.Direction.None != currentGestureDirection)
            {
                if (null != onGestureDirection)
                {
                    onGestureDirection.Invoke();
                }
            }
        }
    }
}