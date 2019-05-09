using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class UIStateChangeSave
    {
        public class UIStateChangedInfo
        {
            public string uiKey = string.Empty;
            public UIPrefab uiPrefab = null;
            public UIEvent.EventType eventType = UIEvent.EventType.None;
            public shaco.Base.StackLocation statckLocationUI = null;
        }

        static public bool isUIStateInsepctorOpened = false;

        private List<UIStateChangedInfo> _uiStatesChangedInfo = new List<UIStateChangedInfo>();

        static public void SaveUIStateChangedInfo(string uiKey, UIPrefab uiPrefab, shaco.Base.StackLocation stackLocationUI, UIEvent.EventType eventType)
        {
#if !UNITY_EDITOR
			return;
#else
            if (!isUIStateInsepctorOpened)
                return;
#endif
            var newInfo = new UIStateChangedInfo();
            newInfo.uiKey = uiKey;
            newInfo.eventType = eventType;
            newInfo.uiPrefab = uiPrefab;
            newInfo.statckLocationUI = stackLocationUI.Clone();

            GameEntry.GetInstance<UIStateChangeSave>()._uiStatesChangedInfo.Add(newInfo);
        }

        static public System.Collections.ObjectModel.ReadOnlyCollection<UIStateChangedInfo> GetAllUIStateChangedInfo()
        {
            return GameEntry.GetInstance<UIStateChangeSave>()._uiStatesChangedInfo.AsReadOnly();
        }

        static public void ClearUIStateChangedInfo()
        {
            GameEntry.GetInstance<UIStateChangeSave>()._uiStatesChangedInfo.Clear();
        }
    }
}
