using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public class SwapMoveAction : MonoBehaviour
    {
        public bool OpenDebugMode = false;

        //参数1：当前显示的组件
        //参数2：true表示从左往右，false表示从右往左移动
        public System.Action<ListSelect, bool> OnWillMoveCallBack = null;
        public System.Action<ListSelect, bool> OnMoveEndCallBack = null;

        [System.Serializable]
        public class ListSelect
        {
            public GameObject selectObject;

            [HideInInspector]
            public int oldDepth = 0;

            public ListSelect prev = null;
            public ListSelect next = null;

            public Vector3 nextPos;
            public Quaternion nextRotation;
        }

        //public GameObject
        public bool OnlyRotateYAxis = true;
        public bool LockActionWhenMoving = true;
        public bool AutoHideBackMove = true;
        public float ActionDuration = 1.0f;
        public ListSelect CurrentSelect = new ListSelect();


        private bool _IsCompleted = true;
        private int _iMaxDepth = 0;
        private int _iMinDepth = 0;
        private ListSelect _firstSelect = null;

        // Use this for initialization
        void Start()
        {
            UpdateSelect();
        }

        void OnDisable()
        {
            doFirstSelectFunction((ListSelect select) =>
            {
                setDepthSafe(select.selectObject, select.oldDepth);
            });
        }

        public bool doAction(bool isLeftToRight)
        {
            if (transform.childCount <= 1)
                return false;

            if (!_IsCompleted)
            {
                if (LockActionWhenMoving)
                    return false;
                else
                {
                    doFirstSelectFunction((ListSelect select) =>
                                     {
                                         shaco.ActionS.StopActions(select.selectObject, true);
                                     });
                }
            }

            if (AutoHideBackMove && ActionDuration > 0)
            {
                ListSelect setActiveSelect = isLeftToRight ? _firstSelect : _firstSelect.prev;
                setActiveSelect.selectObject.SetActive(false);
            }

            doFirstSelectFunction((ListSelect select) =>
            {
                doSwapMoveAction(select, isLeftToRight);
            });

            //finish move in direct when action duration is 0
            if (ActionDuration == 0)
            {
                doFirstSelectFunction((ListSelect select) =>
                {
                    select.selectObject.transform.position = select.nextPos;
                    select.selectObject.transform.rotation = select.nextRotation;
                });
            }
            else
            {
                _IsCompleted = false;
            }

            CurrentSelect = isLeftToRight ? CurrentSelect.next : CurrentSelect.prev;
            updateDepth(isLeftToRight);

            if (OnWillMoveCallBack != null)
            {
                OnWillMoveCallBack(CurrentSelect, isLeftToRight);
            }

            return true;
        }

        public bool IsMoving()
        {
            return !_IsCompleted;
        }

        public void UpdateSelect()
        {
            if (this.transform.childCount == 0)
            {
                return;
            }

            resetSelect();
            CurrentSelect.selectObject = transform.GetChild(0).gameObject;
            CurrentSelect.oldDepth = getDepthSafe(CurrentSelect.selectObject);

            //link children list
            List<GameObject> listChildren = new List<GameObject>();
            for (int i = 0; i < transform.childCount; ++i)
            {
                GameObject child = transform.GetChild(i).gameObject;
                listChildren.Add(child);
            }

            ListSelect tmpCurrent = createSelect(listChildren[0]);
            _firstSelect = tmpCurrent;
            linkSelect(tmpCurrent, tmpCurrent);

            for (int i = 1; i < listChildren.Count; ++i)
            {
                var child = listChildren[i];

                ListSelect nextSelect = createSelect(child);
                linkSelect(nextSelect, tmpCurrent);

                //update min & max depth
                var depth = getDepthSafe(child);
                if (depth > _iMaxDepth)
                    _iMaxDepth = depth;
                if (depth < _iMinDepth)
                    _iMinDepth = depth;

                tmpCurrent = nextSelect;
            }

            //set current
            doFirstSelectFunction((ListSelect select) =>
            {

                if (select.selectObject == CurrentSelect.selectObject)
                    CurrentSelect = select;
            });
        }

        public ListSelect createSelect(GameObject selectObject)
        {
            ListSelect ret = new ListSelect();
            ret.oldDepth = getDepthSafe(selectObject);
            ret.selectObject = selectObject;
            ret.prev = null;
            ret.next = null;

            return ret;
        }

        public void linkSelect(ListSelect select, ListSelect prev)
        {
            select.prev = prev;
            select.next = prev.next;

            prev.next = select;

            if (select.next == _firstSelect)
                _firstSelect.prev = select;
        }

        public void resetSelect()
        {
            if (_firstSelect != null)
            {
                var nextSelect = _firstSelect;
                while (nextSelect != _firstSelect)
                {
                    var nextTmp = nextSelect.next;
                    nextSelect.selectObject = null;
                    nextSelect.prev = null;
                    nextSelect.next = null;
                    nextSelect = nextTmp;
                }
            }
        }

        public delegate void selectFunction(ListSelect select);
        public void doSelectFunction(selectFunction func, bool isNextForeach = true, ListSelect first = null)
        {
            if (CurrentSelect.selectObject == null)
            {
                return;
            }

            if (first == null)
                first = CurrentSelect;

            ListSelect tmpCurrent = first;

            if (isNextForeach)
            {
                do
                {
                    func(tmpCurrent);
                    tmpCurrent = tmpCurrent.next;
                } while (tmpCurrent != first);
            }
            else
            {
                do
                {
                    func(tmpCurrent);
                    tmpCurrent = tmpCurrent.prev;
                } while (tmpCurrent != first);
            }
        }

        public override string ToString()
        {
            var ret = this.name + " : ";

            doFirstSelectFunction((ListSelect select) =>
            {

                ret += select.selectObject.name;
                ret += "→";
            });
            ret = ret.Remove(ret.Length - 1, 1);
            return ret;
        }

        public bool isActionCompleted()
        {
            return LockActionWhenMoving ? _IsCompleted : true;
        }

        private void doSwapMoveAction(ListSelect select, bool isLeftToRight)
        {
            Vector3 Pos = new Vector3();

            Quaternion RotationTmp = new Quaternion();
            if (isLeftToRight)
            {
                Pos = select.prev.selectObject.transform.position;
                RotationTmp = select.prev.selectObject.transform.rotation;
            }
            else
            {
                Pos = select.next.selectObject.transform.position;
                RotationTmp = select.next.selectObject.transform.rotation;
            }

            select.nextPos = Pos;
            select.nextRotation = RotationTmp;

            if (ActionDuration != 0)
            {
                var moveTo = shaco.MoveTo.Create(Pos, ActionDuration);
                var rotateEulderAngle = RotationTmp.eulerAngles;
                if (OnlyRotateYAxis)
                {
                    var rotateEulderSrc = select.selectObject.transform.rotation.eulerAngles;
                    rotateEulderAngle.x = rotateEulderSrc.x;
                    rotateEulderAngle.z = rotateEulderSrc.z;
                }

                var acceAction = shaco.Accelerate.Create(moveTo,
                    new shaco.Accelerate.ControlPoint(0, 3.0f),
                    new shaco.Accelerate.ControlPoint(0.5f, 2.0f),
                    new shaco.Accelerate.ControlPoint(1, 0.2f),
                    shaco.Accelerate.AccelerateMode.ParabolaMode);

                acceAction.RunAction(select.selectObject);

                acceAction.onCompleteFunc += (shaco.ActionS action) =>
                {
                    _IsCompleted = true;

                    if (OnMoveEndCallBack != null)
                    {
                        OnMoveEndCallBack(CurrentSelect, isLeftToRight);
                    }

                    if (AutoHideBackMove)
                    {
                        ListSelect setActiveSelect = isLeftToRight ? _firstSelect.prev : _firstSelect;
                        setActiveSelect.selectObject.SetActive(true);
                    }
                };
            }
        }

        private void updateDepth(bool isNextForeach)
        {
            if (!isNextForeach)
            {
                ListSelect minDepthSelect = _firstSelect.prev;
                int depth = _iMinDepth;
                _firstSelect = minDepthSelect;

                doFirstSelectFunction((ListSelect select) =>
                {
                    setDepthSafe(select.selectObject, depth++);
                });
            }
            else
            {
                ListSelect maxDepthSelect = _firstSelect;
                int depth = _iMaxDepth;
                _firstSelect = maxDepthSelect.next;

                doFirstSelectFunction((ListSelect select) =>
                {
                    setDepthSafe(select.selectObject, depth--);
                });
            }

            if (OpenDebugMode)
                Log.Info(this.ToString());
        }

        public static int getDepthSafe(GameObject obj)
        {
            int ret = 0;

#if SUPPORT_NGUI
            var widgetTmp = obj.GetComponent<UIWidget>();
			ret = widgetTmp ? widgetTmp.depth : 0;
#else
            var spriteRender = obj.GetComponent<Renderer>();

            if (spriteRender != null)
            {
                ret = spriteRender.sortingOrder;
            }
            else
            {
                var parent = obj.transform.parent;
                if (parent == null)
                {
                    Log.Error("SwapMoveAction getDepthSafe error: parent is null");
                    return 0;
                }

                for (int i = 0; i < parent.childCount; ++i)
                {
                    var child = parent.GetChild(i).gameObject;
                    if (obj == child)
                    {
                        ret = i;
                        break;
                    }
                }
            }
#endif
            return ret;

        }

        public ListSelect getDepthSafe(int depth)
        {
            ListSelect ret = null;

            var tmpCurrent = CurrentSelect;

            do
            {
#if SUPPORT_NGUI
                var widgetTmp = tmpCurrent.selectObject.GetComponent<UIWidget>();
                if (widgetTmp.depth == depth)
                    ret = tmpCurrent;
#else
                var spriteRender = tmpCurrent.selectObject.GetComponent<Renderer>();

                if (spriteRender != null && spriteRender.sortingOrder == depth)
                {
                    ret = tmpCurrent;
                }
                else
                {
                    if (depth == tmpCurrent.selectObject.transform.GetSiblingIndex())
                    {
                        ret = tmpCurrent;
                    }
                }
#endif

                tmpCurrent = tmpCurrent.next;
            } while (tmpCurrent != CurrentSelect);

            return ret;
        }

        public static void setDepthSafe(GameObject obj, int depth)
        {
#if SUPPORT_NGUI
            var widgetTmp = obj.GetComponent<UIWidget>();
            if (widgetTmp)
                widgetTmp.depth = depth;
#else
            var spriteRender = obj.GetComponent<Renderer>();

            if (spriteRender != null)
            {
                spriteRender.sortingOrder = depth;
            }
            else
            {
                var parent = obj.transform.parent;
                if (parent == null)
                {
                    Log.Error("SwapMoveAction setDepthSafe error: parent is null");
                    return;
                }

                obj.transform.SetSiblingIndex(depth);
            }
#endif
        }

        private void doFirstSelectFunction(selectFunction func, bool isNextForeach = true)
        {
            doSelectFunction(func, isNextForeach, _firstSelect);
        }


        void OnGUI()
        {
            if (OpenDebugMode)
            {
                if (Input.GetKeyUp(KeyCode.LeftArrow))
                {
                    doAction(false);
                }
                if (Input.GetKeyUp(KeyCode.RightArrow))
                {
                    doAction(true);
                }
            }
        }
    }
}