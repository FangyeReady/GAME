using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace shaco
{
    [RequireComponent(typeof(RectTransform))]
    public class DockArea : MonoBehaviour
    {
        private class DockTargetTransfrom
        {
            public Vector3 oldPosition;
            public Vector3 oldScale;
            public Vector2 oldPivot;
            public Vector2 oldSize;
        }

        //每一帧都会检测停靠位置，当前置或者后置节点对象transfrom变化的时候，自身的transfrom都会自动一起变化
        public bool updateInPerFrame = true;

        [HideInInspector]
        public Vector3 margin = Vector3.zero;
        [HideInInspector]
        public TextAnchor dockAnchor;
        [HideInInspector]
        public RectTransform dockTarget;

        private bool _isUpdateLayoutDirty = true;
        private DockTargetTransfrom _prevTargetTransform = new DockTargetTransfrom();
        private Text _textTarget;
        private RectTransform _rectTransformComponent;

        public void SetUpdateLayoutDirty()
        {
            if (null == dockTarget)
                return;

#if UNITY_EDITOR
            _isUpdateLayoutDirty = true;
            if (!Application.isPlaying)
                Update();
#else
            _isUpdateLayoutDirty = true;
#endif
        }

        void Start()
        {
            UpdateLayout();
        }

        public void Update()
        {
            CheckTargetsTransformChanged();
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            if (!_isUpdateLayoutDirty)
                return;
            _isUpdateLayoutDirty = false;

            _textTarget = dockTarget.GetComponent<Text>();
            _rectTransformComponent = GetComponent<RectTransform>();

            SetLayoutByTarget(dockTarget, dockAnchor);
            this.transform.localPosition += margin;
        }

        private void SetLayoutByTarget(RectTransform targetDock, TextAnchor anchor)
        {
            var middlePivot = new Vector2(shaco.Pivot.Middle.x, shaco.Pivot.Middle.y);
            if (this.GetComponent<Text>() != null && _rectTransformComponent.pivot != middlePivot)
            {
                Debug.LogWarning("DockAreaWarning SetLayout warning: pivot is not support when target is text, please use alignment instead");
                _rectTransformComponent.pivot = middlePivot;
            }
            if (targetDock.GetComponent<Text>() != null && targetDock.pivot != middlePivot)
            {
                Debug.LogWarning("DockAreaWarning SetLayout warning: pivot is not support when dock target is text, please use alignment instead");
                targetDock.pivot = middlePivot;
            }

            var pivotSet = anchor.ToPivot();
            var pivotGet = anchor.ToNegativePivot();
            var fixedPosTmp = UnityHelper.GetWorldPositionByPivot(targetDock.gameObject, pivotGet);

            var fixedOffset = GetFixedOffsetWhenText(targetDock, _textTarget);

            UnityHelper.SetWorldPositionByPivot(_rectTransformComponent.gameObject, fixedPosTmp, pivotSet);
            _rectTransformComponent.localPosition += fixedOffset;
        }

        static private Vector3 GetFixedOffsetWhenText(RectTransform targetDock, Text textTarget)
        {
            var retValue = Vector3.zero;
            if (null != textTarget)
            {
                //horizontal
                switch (textTarget.alignment)
                {
                    case TextAnchor.UpperLeft:
                    case TextAnchor.MiddleLeft:
                    case TextAnchor.LowerLeft:
                        {
                            retValue.x = textTarget.preferredWidth * targetDock.pivot.x - targetDock.sizeDelta.x * targetDock.pivot.x;
                            break;
                        }
                    case TextAnchor.UpperRight:
                    case TextAnchor.MiddleRight:
                    case TextAnchor.LowerRight:
                        {
                            retValue.x = targetDock.sizeDelta.x * targetDock.pivot.x - textTarget.preferredWidth * targetDock.pivot.x;
                            break;
                        }
                    default: break;
                }

                //vertical
                switch (textTarget.alignment)
                {
                    case TextAnchor.LowerLeft:
                    case TextAnchor.LowerCenter:
                    case TextAnchor.LowerRight:
                        {
                            retValue.y = textTarget.preferredHeight * targetDock.pivot.y - targetDock.sizeDelta.y * targetDock.pivot.y;
                            break;
                        }
                    case TextAnchor.UpperLeft:
                    case TextAnchor.UpperCenter:
                    case TextAnchor.UpperRight:
                        {
                            retValue.y = targetDock.sizeDelta.y * targetDock.pivot.y - textTarget.preferredHeight * targetDock.pivot.y;
                            break;
                        }
                    default: break;
                }
            }

            if (targetDock.pivot.x < 0)
            {
                retValue.x = -retValue.x;
            }
            if (targetDock.pivot.y < 0)
            {
                retValue.y = -retValue.y;
            }
            return retValue;
        }

        private void CheckTargetsTransformChanged()
        {
            if (updateInPerFrame)
            {
                bool isTransformChanged = false;
                isTransformChanged |= CheckTargetTransformChanged(_prevTargetTransform);

                if (isTransformChanged)
                {
                    SetUpdateLayoutDirty();
                }
            }
        }

        private bool CheckTargetTransformChanged(DockTargetTransfrom oldTransform)
        {
            bool retValue = false;
            if (null == dockTarget || _rectTransformComponent == null)
                return retValue;

            if (oldTransform.oldPosition != dockTarget.transform.position)
            {
                oldTransform.oldPosition = dockTarget.transform.position;
                retValue = true;
            }

            if (oldTransform.oldScale != dockTarget.transform.localScale)
            {
                oldTransform.oldScale = dockTarget.transform.localScale;
                retValue = true;
            }

            if (oldTransform.oldPivot != dockTarget.pivot)
            {
                oldTransform.oldPivot = dockTarget.pivot;
                retValue = true;
            }

            if (null != _textTarget)
            {
                var size = UnityHelper.GetTextRealSize(_textTarget);
                if (oldTransform.oldSize != size)
                {
                    oldTransform.oldSize = size;
                    retValue = true;
                }
            }
            else
            {
                if (oldTransform.oldSize != _rectTransformComponent.sizeDelta)
                {
                    oldTransform.oldSize = _rectTransformComponent.sizeDelta;
                    retValue = true;
                }
            }
            return retValue;
        }
    }
}