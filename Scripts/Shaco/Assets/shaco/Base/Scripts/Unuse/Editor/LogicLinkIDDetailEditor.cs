using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace shacoEditor
{
    public class LogicLinkIDDetailEditor : EditorWindow
    {
        public shaco.LogicLinker logicLinkerTarget = null;

        private List<shaco.LogicLinker.LinkItem> _listItemFrom = new List<shaco.LogicLinker.LinkItem>();
        private List<shaco.LogicLinker.LinkItem> _listItemTo = new List<shaco.LogicLinker.LinkItem>();

        public void changeLinkID(List<shaco.LogicLinker.LinkID> selectLinkID)
        {
            if (logicLinkerTarget == null)
            {
                shaco.Log.Error("logicLinkerTarget is null");
                return;
            }

            _listItemFrom.Clear();
            _listItemTo.Clear();

            if (selectLinkID != null)
            {
                for (int i = 0; i < selectLinkID.Count; ++i)
                {
                    var itemTmpFrom = logicLinkerTarget.getLinkItem(selectLinkID[i].UUIDFrom);
                    var itemTmpTo = logicLinkerTarget.getLinkItem(selectLinkID[i].UUIDTo);
                    _listItemFrom.Add(itemTmpFrom);
                    _listItemTo.Add(itemTmpTo);
                }
            }
        }

        void OnGUI()
        {
            if (_listItemFrom.Count == 0 || _listItemTo.Count == 0 || _listItemFrom.Count != _listItemTo.Count)
                return;

            //draw condition
            for (int i = 0; i < _listItemFrom.Count; ++i)
            {
                var itemTmpFrom = _listItemFrom[i];
                var itemTmpTo = _listItemTo[i];
                GUILayout.Label(itemTmpFrom.Description + " -> " + itemTmpTo.Description);
                EventDelegateEditorS.DrawEvent(logicLinkerTarget.gameObject, itemTmpFrom.funcCondition, typeof(bool));
            }
        }

        void OnInspectorUpdate()
        {
            //Log.Info("窗口面板的更新");
            //这里开启窗口的重绘，不然窗口信息不会刷新
            // this.Repaint();
        }

        void OnDestroy()
        {
            LogicLinkerEditor.WindowLinkIDDetail = null;
        }
    }
}