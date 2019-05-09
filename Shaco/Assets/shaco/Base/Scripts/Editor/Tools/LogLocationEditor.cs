using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace shacoEditor
{
    public static class LogLocationEditor
    {
        static private bool _isOpening = false;

        [UnityEditor.Callbacks.OnOpenAssetAttribute(-1)]
        static private bool OnOpenAsset(int instanceID, int line)
        {
            if (_isOpening)
                return false;

            var logeditorConfigs = LogLocationInspector.GetLocationConfig();

            var statckTrack = GetStackTrace();
            var fileNames = new string[0];
            if (!string.IsNullOrEmpty(statckTrack))
            {
                fileNames = statckTrack.Split('\n');
            }

            //堆栈数量只有2个，可能是编译报错，不做定位处理
            if (fileNames.Length <= 2)
                return false;

            foreach (var iter in logeditorConfigs)
            {
                var configTmp = iter.Value;
                UpdateLogInstanceID(configTmp);
                if (instanceID == configTmp.instanceID)
                {
                    var fileName = GetCurrentFullFileName(fileNames);
                    var fileLine = LogFileNameToFileLine(fileName);
                    if (fileLine < 0)
                    {
                        return false;
                    }

                    _isOpening = true;
                    var relativePath = string.Empty;
                    if (fileName.StartsWith("Assets"))
                    {
                        relativePath = fileName.RemoveBehind(":");
                    }
                    else
                    {
                        relativePath = "Assets" + fileName.Remove(Application.dataPath).RemoveBehind(":");
                    }
                    var loadAsset = AssetDatabase.LoadAssetAtPath(relativePath, typeof(UnityEngine.Object));
                    AssetDatabase.OpenAsset(loadAsset, fileLine);
                    _isOpening = false;
                    return true;
                }
            }

            return false;
        }

        static private string GetStackTrace()
        {
            var consoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
            var fieldInfo = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            var consoleWindowInstance = fieldInfo.GetValue(null);

            if (null != consoleWindowInstance)
            {
                if ((object)EditorWindow.focusedWindow == consoleWindowInstance)
                {
                    // Get ListViewState in ConsoleWindow
                    // var listViewStateType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ListViewState");
                    // fieldInfo = consoleWindowType.GetField("m_ListView", BindingFlags.Instance | BindingFlags.NonPublic);
                    // var listView = fieldInfo.GetValue(consoleWindowInstance);

                    // Get row in listViewState
                    // fieldInfo = listViewStateType.GetField("row", BindingFlags.Instance | BindingFlags.Public);
                    // int row = (int)fieldInfo.GetValue(listView);

                    // Get m_ActiveText in ConsoleWindow
                    fieldInfo = consoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
                    string activeText = fieldInfo.GetValue(consoleWindowInstance).ToString();

                    return activeText;
                }
            }
            return string.Empty;
        }

        static private void UpdateLogInstanceID(LogLocationInspector.LogEditorConfig config)
        {
            if (config.instanceID > 0)
            {
                return;
            }

            var assetLoadTmp = AssetDatabase.LoadAssetAtPath(config.logScriptPath, typeof(UnityEngine.Object));
            if (null != assetLoadTmp)
            {
                config.instanceID = assetLoadTmp.GetInstanceID();
            }
        }

        static private string GetCurrentFullFileName(string[] fileNames)
        {
            string retValue = string.Empty;
            var logeditorConfigs = LogLocationInspector.GetLocationConfig();
            string defaultPathAssets = "Assets/";

            for (int i = 0; i < fileNames.Length; ++i)
            {
                //获取普通堆栈信息
                var fileNameAndLine = fileNames[i].Substring(defaultPathAssets, ")");

                //如果是异常堆栈信息，标识符稍微有点不一样
                if (string.IsNullOrEmpty(fileNameAndLine))
                {
                    fileNameAndLine = fileNames[i].Substring(defaultPathAssets, " ");
                }

                if (!string.IsNullOrEmpty(fileNameAndLine))
                {
                    fileNameAndLine = defaultPathAssets + fileNameAndLine;
                    var subFileName = fileNameAndLine.Substring("", ":");
                    bool isCutomIngoreFile = false;

                    foreach (var iter in logeditorConfigs)
                    {
                        //用文件名判断是否为忽略文件
                        if (subFileName == iter.Value.logScriptPath)
                        {
                            isCutomIngoreFile = true;
                            break;
                        }
                    }

                    if (!isCutomIngoreFile)
                    {
                        retValue = fileNameAndLine;
                        break;
                    }
                }
            }
            return retValue;
        }

        static private int LogFileNameToFileLine(string fileName)
        {
            int findIndex = ParseFileLineStartIndex(fileName);
            if (findIndex < 0)
                return -1;

            string stringParseLine = string.Empty;
            for (int i = findIndex; i < fileName.Length; ++i)
            {
                if (i < 0 || i > fileName.Length - 1)
                {
                    shaco.Log.Error("LogLocationEditor LogFileNameToFileLine error: out of range, index=" + findIndex + " length=" + fileName.Length);
                    break;
                }
                var charCheck = fileName[i];
                if (!IsNumber(charCheck))
                {
                    break;
                }
                else
                {
                    stringParseLine += charCheck;
                }
            }

            return string.IsNullOrEmpty(stringParseLine) ? -1 : int.Parse(stringParseLine);
        }

        static private int ParseFileLineStartIndex(string fileName)
        {
            int retValue = -1;
            for (int i = fileName.Length - 1; i >= 0; --i)
            {
                var charCheck = fileName[i];
                bool isNumber = IsNumber(charCheck);
                if (isNumber)
                {
                    retValue = i;
                }
                else
                {
                    if (retValue != -1)
                    {
                        break;
                    }
                }
            }
            return retValue;
        }

        static private bool IsNumber(char c)
        {
            return c >= '0' && c <= '9';
        }
    }
}