using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using shaco;

namespace shacoEditor
{
    public class LogicLinkerEditor : EditorWindow
    {
        public enum MouseDragTarget
        {
            None,
            ItemWindow,
            Frame
        }

        private LogicLinkerEditor _currentWindow = null;             //当前编辑窗口
        static public LogicLinkIDDetailEditor WindowLinkIDDetail = null;   //连接线信息窗口
        private float _doubleClickIntervalTime = 0.2f;                      //双击鼠标事件的间隔时间
        private float _itemFrameSize = 24.0f;                               //控件边框的厚度(单位:像素)
        private float _uiRowHeight = 16;                                    //ui每行高度
        private float _uiRowCount = 2;                                      //ui行数
        private float _lineWidth = 2.0f;                                    //线条宽度

        private LogicLinker[] _listLogicLinker;
        private LogicLinker.LinkItem _selectItem = null;
        private List<LogicLinker.LinkID> _selectLinkID = new List<LogicLinker.LinkID>();
        private LogicLinker _target = null;
        private PopupListBox _popupBoxMenu = null;
        private float _mouseTouchDownTime = 0;
        private Vector2 _mouseTouchDownPos = Vector2.zero;
        private Vector2 _mousePrevMovePos = Vector2.zero;
        private Vector2 _mouseMovePos = Vector2.zero;
        private Vector2 _mouseTouchDragPos = Vector2.zero;
        private bool _isWillDoubleClick = false;
        private bool _isDoubleClick = false;
        private bool _isMouseTouchInWindow = false;
        private MouseDragTarget _dragTarget = MouseDragTarget.None;
        private bool _isLinkMode = false;
        private bool _isAltHoldDown = false;
        private string _strEditItemDescription = string.Empty;
        private Event _eventCurrent = null;
        private GUIStyle _guistyleScrollBar = new GUIStyle();  //这里强制给了一个style让横向的scrollbar隐藏了
        private LogicLinker.LinkItem _itemCopy = null;

        //delete by shaco 2017/6/23
        //该方法暂时被弃用了，因为发现并没有什么卵用，不过作为代码参考放着吧
        // [MenuItem("shaco/LinkerWindow")]
        //delete end
        static void AddLinkerWindow()
        {
            var retValue = EditorHelper.GetWindow<LogicLinkerEditor>(null, true, "LogicLinker") as LogicLinkerEditor;
            retValue.wantsMouseMove = true;
            retValue.Show();
            retValue.init();
        }

        void OnEnable()
        {
            _currentWindow = EditorHelper.GetWindow<LogicLinkerEditor>(this, true, "LogicLinker");
        }

        private void init()
        {
            _listLogicLinker = GameObject.FindObjectsOfType<LogicLinker>();
            if (_listLogicLinker.Length == 1)
            {
                initWithScript(_listLogicLinker[0]);
            }
            _guistyleScrollBar.fixedWidth = 1;
        }

        private void initWithScript(LogicLinker target)
        {
            if (target == null)
            {
                GameObject objNew = new GameObject();
                objNew.name = objNew.GetType().Name;
                target = objNew.AddComponent<LogicLinker>();
            }
            _target = target;
            openLinkIDDetailWindow();
            target.init();
            initPopBox();

            EditorHelper.SetDirty(target.gameObject);
        }

        private void initPopBox()
        {
            if (_popupBoxMenu == null)
            {
                _popupBoxMenu = new PopupListBox();
                _popupBoxMenu.OnSelectCallBack = (string itemName, PopupListBox.EventType type) =>
                {
                    if (itemName == "Link")
                    {
                        _isLinkMode = true;
                    }
                    else if (itemName == "RemoveItem(Alt+Delete)")
                    {
                        if (_selectItem != null)
                        {
                            _target.removeLinkItem(_selectItem);
                            _selectItem = null;
                        }
                    }
                    else if (itemName == "Cut Line(Alt+Delete)")
                        checkCutLine();
                    else if (itemName == "Cut Lines")
                        checkCutLines();
                    else if (itemName == "Prev Step(Alt+←)")
                        _target.ItemOperatingStep.prevStep();
                    else if (itemName == "Next Step(Alt+→)")
                        _target.ItemOperatingStep.nextStep();
                };
            }
        }

        void UpdateGUIEvent()
        {
            _eventCurrent = Event.current;
            if (_eventCurrent == null)
                return;

            if (_popupBoxMenu == null)
                initPopBox();
            _mouseMovePos = _eventCurrent.mousePosition;

            bool prevHoldDownFlag = _isAltHoldDown;
            _isAltHoldDown = _eventCurrent.alt;
            if (!prevHoldDownFlag && _isAltHoldDown)
            {
                _mousePrevMovePos = _mouseMovePos;
            }

            switch (_eventCurrent.type)
            {
                case EventType.MouseDown:
                    {
                        if (_isAltHoldDown || _dragTarget != MouseDragTarget.None)
                            break;

                        _mouseTouchDownPos = _eventCurrent.mousePosition;
                        var prevSelectItem = _selectItem;
                        var prevSelectLinkID = new List<LogicLinker.LinkID>();
                        for (int i = 0; i < _selectLinkID.Count; ++i)
                        {
                            prevSelectLinkID.Add(_selectLinkID[i]);
                        }

                        //如果点击的位置在ui层级，则不响应点击选择事件
                        if (_eventCurrent.mousePosition.y > _uiRowCount * _uiRowHeight + 5)
                        {
                            _isMouseTouchInWindow = true;
                            if (!_popupBoxMenu.isActive())
                            {
                                _selectItem = _target.selectItem(_eventCurrent.mousePosition);
                                if (_selectItem == null)
                                {
                                    _selectLinkID = _target.selectLinkID(_eventCurrent.mousePosition, _lineWidth);
                                    for (int i = 0; i < prevSelectLinkID.Count; ++i)
                                        prevSelectLinkID[i].IsSelect = false;
                                    for (int i = 0; i < _selectLinkID.Count; ++i)
                                        _selectLinkID[i].IsSelect = true;

                                    if (WindowLinkIDDetail != null)
                                        WindowLinkIDDetail.changeLinkID(_selectLinkID);
                                    if (_selectLinkID.Count != 0)
                                        _selectItem = prevSelectItem;
                                }

                                //when select changed and changed description
                                if (prevSelectItem != null && _isDoubleClick)
                                {
                                    onDescriptionChanged(prevSelectItem);
                                }

                                //when select changed
                                if (prevSelectItem != _selectItem)
                                {
                                    ResetDoubleClickFlag();
                                }
                                else if (_selectItem != null)
                                {
                                    float offsetTime = Time.realtimeSinceStartup - _mouseTouchDownTime;
                                    if (offsetTime <= _doubleClickIntervalTime)
                                    {
                                        _isWillDoubleClick = true;
                                        _strEditItemDescription = _selectItem.Description;
                                    }
                                }

                                //unfocus this window
                                GUI.FocusControl(string.Empty);
                            }
                        }
                        else
                            _isMouseTouchInWindow = false;
                        _mouseTouchDownTime = Time.realtimeSinceStartup;
                        _popupBoxMenu.updateTouch(_eventCurrent.mousePosition);

                        if (_isLinkMode)
                            _eventCurrent.Use();
                        break;
                    }
                case EventType.MouseDrag:
                    {
                        if (_isAltHoldDown || !_isMouseTouchInWindow)
                            break;

                        _mouseTouchDragPos = _eventCurrent.mousePosition;

                        if (_dragTarget != MouseDragTarget.ItemWindow && _selectItem != null)
                        {
                            _dragTarget = MouseDragTarget.Frame;
                        }

                        if (!_isWillDoubleClick && _dragTarget == MouseDragTarget.Frame)
                        {
                            _target.dragItem(_selectItem, _eventCurrent.mousePosition);
                        }
                        if (_dragTarget != MouseDragTarget.None)
                            _eventCurrent.Use();
                        break;
                    }
                case EventType.MouseUp:
                    {
                        if (_isAltHoldDown)
                            break;

                        _dragTarget = MouseDragTarget.None;
                        _mouseTouchDownPos = Vector2.zero;
                        _mouseTouchDragPos = Vector2.zero;

                        if (_isLinkMode)
                        {
                            _isLinkMode = false;
                            var prevSelectItem = _selectItem;
                            var selectTmp = _target.selectItem(_eventCurrent.mousePosition);

                            if (selectTmp != null && prevSelectItem != null && prevSelectItem != selectTmp)
                            {
                                if (!_target.isLink(prevSelectItem, selectTmp))
                                {
                                    _target.link(prevSelectItem, selectTmp);
                                }
                            }
                            _popupBoxMenu.hide();
                            ResetDoubleClickFlag();
                            _eventCurrent.Use();
                        }
                        break;
                    }
                case EventType.KeyDown:
                    {
                        if (_eventCurrent.alt)
                        {
                            if (_eventCurrent.keyCode == KeyCode.LeftArrow)
                                _target.ItemOperatingStep.prevStep();
                            else if (_eventCurrent.keyCode == KeyCode.RightArrow)
                                _target.ItemOperatingStep.nextStep();
                            else if (_eventCurrent.keyCode == KeyCode.UpArrow)
                            {
                                _itemCopy = _selectItem.Clone();
                                _target.cutLines(_itemCopy);
                            }
                            else if (_eventCurrent.keyCode == KeyCode.DownArrow)
                            {
                                if (_itemCopy != null)
                                {
                                    var itemCopyTmp = _itemCopy.Clone();
                                    itemCopyTmp.UUIDItem = _target.getUUID();
                                    itemCopyTmp.RectSize.position = getCenterPosition().position;
                                    _target.addLinkItem(itemCopyTmp);
                                }
                            }
                            else if ((_eventCurrent.keyCode == KeyCode.Delete || _eventCurrent.keyCode == KeyCode.Backspace))
                            {
                                checkCutLine();
                                if (_selectItem != null)
                                {
                                    _target.removeLinkItem(_selectItem);
                                    _selectItem = null;
                                }
                            }
                        }
                        break;
                    }
            }
        }

        void OnGUI()
        {
            if (_target == null)
            {
                drawLogicLinkerSelectList();
                return;
            }

            if (_target == null)
                return;

            UpdateGUIEvent();
            updateAltCommand();

            //draw base buttons
            var guiButtonHeight = GUILayout.Height(_uiRowHeight);

            GUILayout.BeginHorizontal();
            GUILayout.TextArea("Current Select Item:(" + (_selectItem == null ? "null" : _selectItem.Description) + ")", guiButtonHeight);
            GUILayout.TextArea("Item Count:(" + _target.getLinkItemSize() + ")", guiButtonHeight);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("CreateItem", guiButtonHeight))
            {
                Rect rectCenter = getCenterPosition();
                _target.createLinkItem(rectCenter.x, rectCenter.y, rectCenter.width, rectCenter.height);
            }
            if (GUILayout.Button("RemoveItem", guiButtonHeight))
            {
                if (_selectItem != null)
                {
                    _target.removeLinkItem(_selectItem);
                    _selectItem = null;
                }
            }
            if (GUILayout.Button("ClearItem", guiButtonHeight))
            {
                _target.clear();
            }
            if (GUILayout.Button("ClearLinkLine", guiButtonHeight))
            {
                _target.cutAllLinkLine();
            }
            if (GUILayout.Button("Excute", guiButtonHeight))
            {
                if (_selectItem != null)
                {
                    bool success = true;
                    if (_selectItem.LinkFrom.Count == 0)
                        success = _target.tryExcuteWithUpdate(_selectItem, null);
                    else
                    {
                        var fromItemTmp = _target.getLinkItemFrom(_selectItem, _selectLinkID);
                        if (fromItemTmp != null)
                            success = _target.tryExcuteWithUpdate(_selectItem, fromItemTmp);
                        else
                            Log.Info("please select a link line to " + _selectItem);
                    }
                    if (!success)
                        Log.Info("Excute faild, doesn't meet the trigger condition");
                }
            }
            if (GUILayout.Button("OpenLinkIDDetail", guiButtonHeight))
            {
                openLinkIDDetailWindow();
            }

            GUILayout.EndHorizontal();

            //draw link items 
            _target.foreachLinkItems((LogicLinker.LinkItem item) =>
            {

                drawLinkItem(item);

            });

            //draw popup listbox 
            if (_selectItem != null && _popupBoxMenu != null)
            {
                _popupBoxMenu.UpdateDraw(new Vector3(
                    _selectItem.RectSize.x + _selectItem.RectSize.width,
                    _selectItem.RectSize.y), PopupListBox.EventType.TouchDown);
            }

            //draw link mode
            if (_isLinkMode && _selectItem != null)
            {
                GUIHelper.LineDraw.DrawLineWithArrow(_selectItem.RectSize.center, _mouseMovePos, _lineWidth);
            }
        }

        private void drawLogicLinkerSelectList()
        {
            if (_listLogicLinker == null)
                return;

            float widthTmp = 200;
            float heightTmp = 20;
            Rect rectCenter = new Rect(_currentWindow.position.width / 2 - widthTmp / 2, _currentWindow.position.height / 3 - heightTmp / 2,
                widthTmp, heightTmp);

            for (int i = _listLogicLinker.Length - 1; i >= 0; --i)
            {
                if (GUI.Button(rectCenter, _listLogicLinker[i].gameObject.name))
                {
                    initWithScript(_listLogicLinker[i]);
                    break;
                }
                rectCenter.y += heightTmp;
            }
        }

        private void drawDefaultItem(LogicLinker.LinkItem target, bool isShow)
        {
            bool isWaitExcuteItem = _target.isWaitExcute(target.UUIDItem);
            bool isPrevExcuteItem = _target.isPrevExcute(target.UUIDItem);
            Color oldColor = GUI.color;

            if (isWaitExcuteItem)
                GUI.color = _target.WaitExcuteItemColor;
            else if (isPrevExcuteItem)
                GUI.color = _target.ExcutedItemColor;

            Rect drawRect = new Rect();
            if (isShow)
            {
                drawRect = new Rect(target.RectSize.x, target.RectSize.y, target.RectSize.width - _itemFrameSize, target.RectSize.height);
            }

            GUI.Box(drawRect, string.Empty);

            //if position have decimal, text maybe blur, unity bug ?
            drawRect.x = (int)drawRect.x;
            drawRect.y = (int)drawRect.y;


            try
            {
                GUILayout.BeginArea(drawRect);
            }
            catch (Exception)
            {
                return;
            }

            target.ScrollPosition = GUILayout.BeginScrollView(target.ScrollPosition, _guistyleScrollBar, GUILayout.Width(drawRect.width));

            //draw default item
            GUILayoutOption preWidth = GUILayout.Width(drawRect.width);
            GUILayoutOption preHeight = GUILayout.Height(_uiRowHeight);

            //description
            GUILayout.BeginHorizontal();
            GUILayout.Label("Description:");

            target.Description = GUILayout.TextField(target.Description, preWidth, preHeight);
            GUILayout.EndHorizontal();

            //condition callback & excute callback
            //			target.IsShowCondition = DrawEventDelegate("Condition", target.funcCondition, target.IsShowCondition, drawRect.width, _uiRowHeight);
            target.IsShowExcute = DrawEventDelegate("Excute", target.funcExcute, target.IsShowExcute, drawRect.width, _uiRowHeight);

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            if (isShow)
                drawEditFrameSize(target);
            if (isWaitExcuteItem || isPrevExcuteItem)
                GUI.color = oldColor;
        }

        private bool DrawEventDelegate(string prefixName, List<EventDelegateS> listEvent, bool flag, float width, float height)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(flag ? "▼" : "▶", GUILayout.Width(width / 2), GUILayout.Height(height)))
            {
                flag = !flag;
            }
            GUILayout.Label(prefixName, GUILayout.Width(width / 2), GUILayout.Height(height));
            GUILayout.EndHorizontal();
            if (flag)
            {
                if (prefixName == "Excute")
                    EventDelegateEditorS.DrawEvent(_target.gameObject, listEvent, typeof(void));
                else
                    EventDelegateEditorS.DrawEvent(_target.gameObject, listEvent, typeof(bool));
            }
            return flag;
        }

        private void drawEditFrameSize(LogicLinker.LinkItem target)
        {
            //edit frame size
            Rect rectOut = new Rect(target.RectSize);
            Rect rectInner = new Rect(rectOut.x, rectOut.y,
                rectOut.width - _itemFrameSize, rectOut.height);

            if (_dragTarget == MouseDragTarget.None)
            {
                if (_target.isInWindowFrame(target, _mouseTouchDownPos, rectInner))
                {
                    _dragTarget = MouseDragTarget.ItemWindow;
                    _mouseTouchDragPos = _mouseTouchDownPos;
                }
            }
            if (_dragTarget == MouseDragTarget.ItemWindow && _selectItem == target)
            {
                Vector2 offsetSrc = _mouseTouchDownPos - target.RectSize.position;
                Vector2 offsetDes = _mouseTouchDragPos - target.RectSize.position;
                Vector2 offsetCur = offsetDes - offsetSrc;

                target.RectSize.size += offsetCur;

                Vector3 minSizeTmp = target.DefaultRectSize.size;
                target.RectSize.width = Math.Max(minSizeTmp.x, target.RectSize.width);
                target.RectSize.height = Math.Max(minSizeTmp.y, target.RectSize.height);

                _mouseTouchDownPos = _mouseTouchDragPos;
            }

            var rectFrame = new Rect(rectInner.x + rectInner.width, rectInner.y,
                target.RectSize.width - rectInner.width,
                rectInner.height);
            GUI.Button(rectFrame, "-\n+");
        }

        private void drawEditItem(LogicLinker.LinkItem target)
        {
            //edit item
            if (!_isDoubleClick)
            {
                _isDoubleClick = _isWillDoubleClick;
            }
            else
            {
                //edit description
                float rateTmp = 1.0f / 1;
                //				float maxHeight = _selectItem.RectSize.height;
                Rect rectText = new Rect(_selectItem.RectSize.x, _selectItem.RectSize.y, _selectItem.RectSize.width, _selectItem.RectSize.height * rateTmp);
                //Rect rectOpenToolkit = new Rect(rectText.x, rectText.y + maxHeight * (rateTmp * 1), rectText.width, rectText.height);

                //_strEditItemDescription = GUI.TextField(rectText, _strEditItemDescription);
                if (GUI.Button(rectText, "Toolkit"))
                {
                    if (!_popupBoxMenu.isActive())
                        _popupBoxMenu.show("Link", "RemoveItem(Alt+Delete)", "Cut Line(Alt+Delete)", "Cut Lines", "Prev Step(Alt+←)", "Next Step(Alt+→)");
                }
            }
        }

        private void drawLinkLine(LogicLinker.LinkItem target)
        {
            //draw link to lines
            Color colorOld = GUI.color;
            for (int i = 0; i < target.LinkTo.Count; ++i)
            {
                var itemTmp = _target.getLinkItem(target.LinkTo[i].UUIDTo);

                if (target.LinkTo[i].IsSelect)
                    GUI.color = Color.red;

                var crossLine = GUIHelper.LineDraw.DrawLineWithArrowAndCrossRect(
                    target.RectSize.center,
                    itemTmp.RectSize.center,
                    target.RectSize,
                    itemTmp.RectSize,
                    _lineWidth);

                if (target.LinkTo[i].IsSelect)
                    GUI.color = colorOld;

                target.LinkTo[i].startPos = crossLine.CrossPointStart;
                target.LinkTo[i].endPos = crossLine.CrossPointEnd;
            }
        }

        private void drawLinkItem(LogicLinker.LinkItem target)
        {
            bool isEditMode = _isWillDoubleClick && _selectItem != null && target == _selectItem;
            if (isEditMode)
            {
                drawEditItem(target);
                drawDefaultItem(target, false);
            }
            else
                drawDefaultItem(target, true);

            drawLinkLine(target);
        }

        private int getMinItemRowCount(LogicLinker.LinkItem item)
        {
            int ret = 1; //first item is description
            if (item.IsShowCondition)
                ret += item.funcCondition.Count == 0 ? 1 : item.funcCondition.Count + 1;
            if (item.IsShowExcute)
                ret += item.funcExcute.Count == 0 ? 1 : item.funcExcute.Count + 1;
            return ret;
        }

        private void onDescriptionChanged(LogicLinker.LinkItem prevSelect)
        {
            if (prevSelect != null && _strEditItemDescription != prevSelect.Description)
            {
                prevSelect.Description = _strEditItemDescription; ;
            }
        }

        private void updateAltCommand()
        {
            if (!_isAltHoldDown)
                return;

            Vector2 moveOffset = _mouseMovePos - _mousePrevMovePos;
            _mousePrevMovePos = _mouseMovePos;

            _target.foreachLinkItems((LogicLinker.LinkItem item) =>
            {

                item.RectSize.position += moveOffset;
            });
        }

        private void checkCutLine()
        {
            for (int i = 0; i < _selectLinkID.Count; ++i)
            {
                _target.cutLine(_selectLinkID[i]);
            }
            _selectLinkID.Clear();
        }

        private void checkCutLines()
        {
            if (_selectItem != null)
                _target.cutLines(_selectItem);
        }

        private void ResetDoubleClickFlag()
        {
            _isWillDoubleClick = false;
            _isDoubleClick = false;
        }

        private Rect getCenterPosition()
        {
            float widthTmp = 260;
            float heightTmp = 130;
            return new Rect(_currentWindow.position.width / 2 - widthTmp / 2, _currentWindow.position.height / 2 - heightTmp / 2,
                260, 130);
        }

        private void openLinkIDDetailWindow()
        {
            WindowLinkIDDetail = EditorWindow.GetWindow(typeof(LogicLinkIDDetailEditor), true, "LinkIDDetail") as LogicLinkIDDetailEditor;
            WindowLinkIDDetail.logicLinkerTarget = _target;
            WindowLinkIDDetail.changeLinkID(_selectLinkID);
            WindowLinkIDDetail.Show();
        }

        void OnFocus()
        {
            //Log.Info("当窗口获得焦点时调用一次");
        }

        void OnLostFocus()
        {
            //Log.Info("当窗口丢失焦点时调用一次");
        }

        void OnHierarchyChange()
        {
            //Log.Info("当Hierarchy视图中的任何对象发生改变时调用一次");
        }

        void OnProjectChange()
        {
            //Log.Info("当Project视图中的资源发生改变时调用一次");
        }

        void OnInspectorUpdate()
        {
            //Log.Info("窗口面板的更新");
            //这里开启窗口的重绘，不然窗口信息不会刷新
            //this.Repaint();
        }

        void OnSelectionChange()
        {
            //当窗口出去开启状态，并且在Hierarchy视图中选择某游戏对象时调用
            //			foreach (Transform t in Selection.transforms)
            //			{
            //				//有可能是多选，这里开启一个循环打印选中游戏对象的名称
            //				//Log.Info("OnSelectionChange" + t.name);
            //			}
        }

        void OnDestroy()
        {
            //Log.Info("当窗口关闭时调用");
            if (_target)
            {
                _target.Reset();

                EditorHelper.SetDirty(_target.gameObject);

                if (WindowLinkIDDetail != null)
                    WindowLinkIDDetail.Close();

                //we must do this when the window is closed, otherwise, prefab data not saved 
                PrefabUtility.DisconnectPrefabInstance(_target.gameObject);
                PrefabUtility.RecordPrefabInstancePropertyModifications(_target.gameObject);
                PrefabUtility.ReconnectToLastPrefab(_target.gameObject);
            }
        }
    }
}