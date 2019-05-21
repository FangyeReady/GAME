using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace shaco
{
    public class ScrollRectEx : ScrollRect
    {
        public System.Action onScrollingCallBack = null;
        public System.Action<PointerEventData> onBeginDragCallBack = null;
        public System.Action<PointerEventData> onEndDragCallBack = null;

        private Vector3 _prevContentPosition = Vector3.zero;

        protected override void Start()
        {
            base.Start();
            _prevContentPosition = content.transform.position;
        }


        protected override void LateUpdate()
        {
            //only run on playing mode
            if (!Application.isPlaying)
            {
                return;
            }
            
            if (null != onScrollingCallBack && _prevContentPosition != content.transform.position)
            {
                onScrollingCallBack();
                _prevContentPosition = content.transform.position;
            }

            base.LateUpdate();
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            if (null != onBeginDragCallBack)
            {
                onBeginDragCallBack(eventData);
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            if (null != onEndDragCallBack)
            {
                onEndDragCallBack(eventData);
#if UNITY_5_3_OR_NEWER
                SetDirtyCaching();
#endif
            }
        }
    }
}