#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class CustomEnumsField
    {
        public System.Action onEnumWillChangeCallBack = null;
        public System.Action onEnumChangedCallBack = null;

        public int currentSelectIndex = 0;
        private List<string> _customEnums = new List<string>();
        private string[] _customEnumsDisplay = new string[0];

        private SelectMode _currentSelectMode = SelectMode.None;
        private string _currentInputEnumString = string.Empty;

        private readonly float SUB_BUTTON_WIDTH = 125f;

        private enum SelectMode
        {
            None,
            Add,
            Remove,
            Rename
        }

        public CustomEnumsField(params string[] defaultValues)
        {
            _customEnums = defaultValues.ToArrayList();
            _customEnumsDisplay = defaultValues;
        }

        public bool Contains(string enumString)
        {
            return _customEnums.Contains(enumString);
        }

        public void SetEnum(string enumString)
        {
            if (!_customEnums.Contains(enumString))
            {
                Debug.LogError("CustomEnumsField SetEnum error: not found enum=" + enumString);
            }
            else 
            {
                currentSelectIndex = _customEnums.IndexOf(enumString);
            }
        }

        public string[] GetEnumsDisplay()
        {
            return _customEnumsDisplay;
        }

        public void SetEnumDisplay(string[] enumsDisplay)
        {
            _customEnumsDisplay = enumsDisplay;
            _customEnums = _customEnumsDisplay.ToArrayList();
        }

        public void UpdateEnumsDisplay()
        {
            _customEnumsDisplay = _customEnums.ToArray();
        }

        public string GetCurrentEnumString()
        {
            if (currentSelectIndex < 0 || currentSelectIndex > _customEnumsDisplay.Length - 1)
            {
                Debug.LogError("_customEnums GetCurrentEnumString error: out of range, index=" + currentSelectIndex + " count=" + _customEnumsDisplay.Length);
                currentSelectIndex = 0;
            }
            return _customEnumsDisplay[currentSelectIndex];
        }

        public void DrawEnums(string prefix = "Enum:")
        {
            GUILayout.BeginHorizontal();
            {
                GUI.changed = false;
                var selectChanelTmp = EditorGUILayout.Popup(prefix, currentSelectIndex, _customEnumsDisplay);
                if (GUI.changed)
                {
                    if (null != onEnumWillChangeCallBack) onEnumWillChangeCallBack();
                    currentSelectIndex = selectChanelTmp;
                    if (null != onEnumChangedCallBack) onEnumChangedCallBack();
                    _currentSelectMode = SelectMode.None;
                }

                float subButtonOffset = 2;

                switch (_currentSelectMode)
                {
                    case SelectMode.None:
                        {
                            if (GUILayout.Button("Add", GUILayout.Width((_customEnums.Count <= 1 ? SUB_BUTTON_WIDTH : SUB_BUTTON_WIDTH / 2 - subButtonOffset))))
                            {
                                _currentSelectMode = SelectMode.Add;
                            }
                            if (_customEnums.Count > 1 && GUILayout.Button("Remove", GUILayout.Width(SUB_BUTTON_WIDTH / 2 - subButtonOffset)))
                            {
                                _currentSelectMode = SelectMode.Remove;
                                _currentInputEnumString = GetCurrentEnumString();
                            }
                            if (GUILayout.Button("Rename",  GUILayout.Width(SUB_BUTTON_WIDTH / 2)))
                            {
                                _currentSelectMode = SelectMode.Rename;
                                _currentInputEnumString = GetCurrentEnumString();
                            }
                            break;
                        }
                    case SelectMode.Add:
                    case SelectMode.Remove:
                        {
                            if (_currentSelectMode == SelectMode.Add)
                            {
                                _currentInputEnumString = GUILayout.TextField(_currentInputEnumString);
                            }

                            if (!string.IsNullOrEmpty(_currentInputEnumString) && GUILayout.Button("OK", GUILayout.Width(SUB_BUTTON_WIDTH / 2 - subButtonOffset)))
                            {
                                if (_currentSelectMode == SelectMode.Add)
                                {
                                    if (_customEnums.Contains(_currentInputEnumString))
                                    {
                                        Debug.LogError("BuildInspector+Version DrawChannel error: duplicate channel=" + _currentInputEnumString);
                                    }
                                    else
                                    {
                                        _customEnums.Add(_currentInputEnumString);
                                        UpdateEnumsDisplay();
                                        if (null != onEnumWillChangeCallBack) onEnumWillChangeCallBack();
                                        currentSelectIndex = _customEnumsDisplay.Length - 1;
                                        if (null != onEnumChangedCallBack) onEnumChangedCallBack();
                                    }
                                }
                                else if (_currentSelectMode == SelectMode.Remove)
                                {
                                    if (currentSelectIndex > _customEnums.Count - 2)
                                    {
                                        if (null != onEnumWillChangeCallBack) onEnumWillChangeCallBack();
                                        currentSelectIndex = currentSelectIndex - 1;
                                        if (null != onEnumChangedCallBack) onEnumChangedCallBack();
                                    }

                                    _customEnums.RemoveAt(selectChanelTmp);
                                    UpdateEnumsDisplay();
                                }
                                _currentSelectMode = SelectMode.None;
                            }
                            if (GUILayout.Button("Cancel", GUILayout.Width(SUB_BUTTON_WIDTH / 2 - subButtonOffset)))
                            {
                                _currentSelectMode = SelectMode.None;
                            }
                            break;
                        }
                    case SelectMode.Rename:
                    {
                        _currentInputEnumString = GUILayout.TextField(_currentInputEnumString);
                        if (!string.IsNullOrEmpty(_currentInputEnumString) && GUILayout.Button("OK", GUILayout.Width(SUB_BUTTON_WIDTH / 2 - subButtonOffset)))
                        {
                            _customEnums[currentSelectIndex] = _currentInputEnumString;
                            UpdateEnumsDisplay();
                            if (null != onEnumWillChangeCallBack) onEnumWillChangeCallBack();
                            if (null != onEnumChangedCallBack) onEnumChangedCallBack();
                            _currentSelectMode = SelectMode.None;
                        }
                        if (GUILayout.Button("Cancel", GUILayout.Width(SUB_BUTTON_WIDTH / 2 - subButtonOffset)))
                        {
                            _currentSelectMode = SelectMode.None;
                        }
                        break;
                    }
                    default: Debug.LogError("BuildInspector+Version DrawChannel error: unsupport channel select mode=" + _currentSelectMode); break;
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}

#endif