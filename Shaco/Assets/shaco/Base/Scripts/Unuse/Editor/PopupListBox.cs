using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace shaco
{
    public class PopupListBox
    {
        public enum EventType
        {
            TouchDown,
            TouchUp
        }

        public delegate void CALL_FUNC_SELECT(string itemName, EventType type);

        public Vector3 ButtonSize = new Vector3(160, 20);
        public CALL_FUNC_SELECT OnSelectCallBack = null;

        private List<string> _listItemName = new List<string>();
        private bool _isShow = false;
        private Rect _rectAll = _rectZero;
        static private readonly Rect _rectZero = new Rect();

        public void show(params string[] menuNames)
        {
#if UNITY_EDITOR
            lock (_listItemName)
            {
                _listItemName.Clear();
                for (int i = 0; i < menuNames.Length; ++i)
                {
                    if (!string.IsNullOrEmpty(menuNames[i]))
                        _listItemName.Add(menuNames[i]);
                }

                if (_listItemName.Count > 0)
                    _isShow = true;
            }
#endif
        }

        public void removeMenu(string menuName)
        {
            lock (_listItemName)
            {
                _listItemName.Remove(menuName);
            }
        }

        public void hide()
        {
            _isShow = false;
            _rectAll = _rectZero;
        }

        public bool isActive()
        {
            return _isShow;
        }

        public void updateTouch(Vector3 touchPos)
        {
            if (_rectAll != _rectZero && !_rectAll.Contains(touchPos))
            {
                this.hide();
            }
        }

        public void UpdateDraw(Vector3 targetPos, EventType type)
        {
#if UNITY_EDITOR

            if (!isActive())
                return;

            _rectAll = new Rect(targetPos.x, targetPos.y, ButtonSize.x, 0);
            Rect rectFirst = new Rect(targetPos.x, targetPos.y, ButtonSize.x, ButtonSize.y);

            lock (_listItemName)
            {
                if (type == EventType.TouchUp)
                {
                    for (int i = 0; i < _listItemName.Count; ++i)
                    {
                        if (GUI.Button(rectFirst, _listItemName[i]))
                        {
                            if (OnSelectCallBack != null)
                                OnSelectCallBack(_listItemName[i], type);
                        }
                        rectFirst.y += ButtonSize.y;
                        _rectAll.height += ButtonSize.y;
                    }
                }
                else
                {
                    for (int i = 0; i < _listItemName.Count; ++i)
                    {
                        if (GUI.RepeatButton(rectFirst, _listItemName[i]))
                        {
                            if (OnSelectCallBack != null)
                                OnSelectCallBack(_listItemName[i], type);
                        }
                        rectFirst.y += ButtonSize.y;
                        _rectAll.height += ButtonSize.y;
                    }
                }
            }

#endif
        }
    }
}
