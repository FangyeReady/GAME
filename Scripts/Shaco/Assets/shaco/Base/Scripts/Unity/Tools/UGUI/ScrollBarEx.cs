using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace shaco
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class ScrollBarEx : Scrollbar
    {
        public Image Background;

		public void AutoSetDirection(shaco.ListView listView)
		{
            var scrollRect = listView.GetComponent<ScrollRect>();
			if (null == scrollRect)
			{
				shaco.Log.Error("ScrollBarEx AutoSetDirection error: missing Component 'ScrollRect' ");
				return;
			}

			var rectTrans = GetComponent<RectTransform>();
            //left bottom
            rectTrans.offsetMin = new Vector2(0, 0);
            //-right -top
            rectTrans.offsetMax = new Vector2(-0, -0);

            switch (listView.GetScrollDirection())
            {
                case shaco.Direction.Right:
                    {
                        this.direction = UnityEngine.UI.Scrollbar.Direction.RightToLeft;
                        scrollRect.horizontalScrollbar = this;
                        break;
                    }
                case shaco.Direction.Left:
                    {
                        this.direction = UnityEngine.UI.Scrollbar.Direction.LeftToRight;
                        scrollRect.horizontalScrollbar = this;
                        break;
                    }
                case shaco.Direction.Down:
                    {
                        this.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;
                        scrollRect.verticalScrollbar = this;
                        break;
                    }
                case shaco.Direction.Up:
                    {
                        this.direction = UnityEngine.UI.Scrollbar.Direction.TopToBottom;
                        scrollRect.verticalScrollbar = this;
                        break;
                    }
                default: shaco.Log.Error("CreateListViewScrollBarEx error: unsupport type=" + listView.GetScrollDirection()); break;
            }
		}

        protected override void Start()
        {
            base.Start();

            handleRect = this.GetComponent<RectTransform>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (null != Background) Background.enabled = true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (null != Background) Background.enabled = false;
        }
    }
}

