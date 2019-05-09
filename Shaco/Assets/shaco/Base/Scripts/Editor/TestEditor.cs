using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using shaco.Base;

namespace shacoEditor
{
    public class TestEditor : EditorWindow
    {
        private TestEditor _currentWindow = null;
        private Object _pickObj = null;

        private string _prevSelectPath = string.Empty;

        [MenuItem("shaco/Editor/TestEditor %#t")]
        static void openTestWindows()
        {
            var retValue = EditorHelper.GetWindow<TestEditor>(null, true, "TestEditor") as TestEditor;
            retValue.Show();
            retValue.autoRepaintOnSceneChange = true;
        }

        void OnEnable()
        {
            _currentWindow = EditorHelper.GetWindow<TestEditor>(this, true, "TestEditor");
        }

        void OnGUI()
        {
            if (_currentWindow == null)
                return;

            if (GUILayout.Button("PrintMD5(FromFile)"))
            {
                var pathTmp = OpenFilePanel();
                if (!string.IsNullOrEmpty(pathTmp))
                {
                    var filename = FileHelper.GetLastFileName(pathTmp);
                    Debug.Log("filename md5:" + filename + "\n" + FileHelper.MD5FromFile(pathTmp));
                }
            }

            if (GUILayout.Button("PrintMD5(FromByte)"))
            {
                var pathTmp = OpenFilePanel();
                if (!string.IsNullOrEmpty(pathTmp))
                {
                    var filename = FileHelper.GetLastFileName(pathTmp);
                    var bytesTmp = FileHelper.ReadAllByteByUserPath(pathTmp);
                    Debug.Log("filename md5:" + filename + "\n" + FileHelper.MD5FromByte(bytesTmp));
                }
            }

            if (GUILayout.Button("PrintMD5(FromString)"))
            {
                var pathTmp = OpenFilePanel();
                if (!string.IsNullOrEmpty(pathTmp))
                {
                    var filename = FileHelper.GetLastFileName(pathTmp);
                    var stringTmp = FileHelper.ReadAllByUserPath(pathTmp);
                    Debug.Log("filename md5:" + filename + "\n" + FileHelper.MD5FromString(stringTmp));
                }
            }

            if (GUILayout.Button("PrintMD5(FromAssetBundleConfig)"))
            {
                var pathTmp = OpenFilePanel();
                if (!string.IsNullOrEmpty(pathTmp))
                {
                    var filename = FileHelper.GetLastFileName(pathTmp);
                    var stringTmp = FileHelper.ReadAllByUserPath(pathTmp);

                    //computer main md5
                    var versionControlTmp = shaco.HotUpdateHelper.JsonToVersionControl(stringTmp);
                    string allFileMD5 = string.Empty;
                    for (int i = 0; i < versionControlTmp.ListAssetBundles.Count; ++i)
                    {
                        allFileMD5 += versionControlTmp.ListAssetBundles[i].AssetBundleMD5;
                    }
                    string strMainMD5 = shaco.Base.FileHelper.MD5FromString(allFileMD5);
                    Debug.Log("filename md5:" + filename + "\n" + strMainMD5);
                }
            }

            if (GUILayout.Button("IsSameFile"))
            {
                var stringTmp1 = string.Empty;
                var stringTmp2 = string.Empty;
                var pathTmp = OpenFilePanel();
                if (!string.IsNullOrEmpty(pathTmp))
                {
                    stringTmp1 = FileHelper.ReadAllByUserPath(pathTmp);
                }
                pathTmp = OpenFilePanel();
                if (!string.IsNullOrEmpty(pathTmp))
                {
                    stringTmp2 = FileHelper.ReadAllByUserPath(pathTmp);
                }
                if (!string.IsNullOrEmpty(stringTmp1) && !string.IsNullOrEmpty(stringTmp2))
                {
                    bool isSame = true;
                    for (int i = 0; i < stringTmp1.Length; ++i)
                    {
                        if (i > stringTmp2.Length - 1)
                        {
                            isSame = false;
                            Debug.Log("not equal index=" + i);
                            break;
                        }
                        else if (stringTmp1[i] != stringTmp2[i])
                        {
                            isSame = false;
                            int value1 = (int)stringTmp1[i];
                            int value2 = (int)stringTmp2[i];
                            Debug.Log("not equal index=" + i + " str1=" + stringTmp1[i] + " str2=" + stringTmp2[i] + " v1=" + value1 + " v2=" + value2);
                            break;
                        }
                    }
                    Debug.Log("compare result=" + isSame + " len1=" + stringTmp1.Length + " len2=" + stringTmp2.Length);
                }
            }

            GUILayout.BeginHorizontal();
            {
                _pickObj = EditorGUILayout.ObjectField(_pickObj, typeof(UnityEngine.Object), true);
                _pickObj = GUIHelper.ObjectPicker("Show Picker", _pickObj);
                if (GUI.changed)
                {
                    this.Repaint();
                }
            }
            GUILayout.EndHorizontal();
        }

        private string OpenFilePanel()
        {
            var retValue = EditorUtility.OpenFilePanel("Select a file", string.IsNullOrEmpty(_prevSelectPath) ? Application.dataPath : _prevSelectPath, string.Empty);
            _prevSelectPath = retValue;
            return retValue;
        }
    }
}