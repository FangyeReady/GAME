using UnityEngine;
using System.Collections;
using UnityEditor;

namespace shacoEditor
{
    public class HotUpdatePathIntputEditor : EditorWindow
    {
        public delegate void ON_SET_PATH_CALLBACK(string path);

        private ON_SET_PATH_CALLBACK _onSetPathCallBack = null;
        private string _strPathInput = string.Empty;
        private HotUpdatePathIntputEditor _currentWindow = null;

        public static void OpenHotUpdatePathInputWindow(ON_SET_PATH_CALLBACK callback, Rect position)
        {
            var retValue = EditorWindow.GetWindow(typeof(HotUpdatePathIntputEditor), true, "HotUpdatePathIntputEditor") as HotUpdatePathIntputEditor;
            retValue._onSetPathCallBack = callback;
            retValue.maxSize = new Vector2(1000, 100);
            retValue.position = new Rect(position.x + position.width / 2 - retValue.position.width / 2,
                                               position.y + position.height / 2 - retValue.position.height / 2,
                                               retValue.position.width,
                                               retValue.position.height);
            retValue.Show();
            retValue.autoRepaintOnSceneChange = true;
        }

        void OnGUI()
        {
            UpdateGUIEvent();

            _strPathInput = EditorGUILayout.TextField("Path", _strPathInput);
            if (!string.IsNullOrEmpty(_strPathInput) && GUILayout.Button("OK"))
            {
                OnOKEvent();
            }
            if (GUILayout.Button("Cancel"))
            {
                if (_onSetPathCallBack != null)
                {
                    _onSetPathCallBack(string.Empty);
                }
                _currentWindow.Close();
            }
        }

        void UpdateGUIEvent()
        {
            var eventCurrent = Event.current;
            if (eventCurrent == null)
                return;

            switch (eventCurrent.type)
            {
                case EventType.KeyUp:
                    {
                        if (eventCurrent.keyCode == KeyCode.Return || eventCurrent.keyCode == KeyCode.KeypadEnter)
                        {
                            OnOKEvent();
                        }
                        break;
                    }
                default: break;
            }
        }

        void OnOKEvent()
        {
            if (_onSetPathCallBack != null)
            {
                _onSetPathCallBack(_strPathInput);
            }
            _currentWindow.Close();
        }

        void OnDestroy()
        {
            if (_onSetPathCallBack != null)
            {
                _onSetPathCallBack(string.Empty);
            }
        }
    }
}