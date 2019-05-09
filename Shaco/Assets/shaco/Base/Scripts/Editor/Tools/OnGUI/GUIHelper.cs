using UnityEngine;
using System.Collections;
using UnityEditor;

namespace shacoEditor
{
    public partial class GUIHelper
    {
        static private bool _isShowObjectPicker = false;

        static public void DrawOutline(Rect rect, float size = 1)
        {
            Color colorTmp = new Color();
            if (EditorGUIUtility.isProSkin)
            {
                colorTmp.r = 0.12f;
                colorTmp.g = 0.12f;
                colorTmp.b = 0.12f;
                colorTmp.a = 1.0f;
            }
            else
            {
                colorTmp.r = 0.6f;
                colorTmp.g = 0.6f;
                colorTmp.b = 0.6f;
                colorTmp.a = 1.333f;
            }
            DrawOutline(rect, size, colorTmp);
        }

        static public void DrawOutline(Rect rect, float size, Color color)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Color orgColor = GUI.color;
            GUI.color = GUI.color * color;

            //up
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, size), EditorGUIUtility.whiteTexture);

            //down
            GUI.DrawTexture(new Rect(rect.x, rect.yMax, rect.width, size), EditorGUIUtility.whiteTexture);

            //left
            GUI.DrawTexture(new Rect(rect.x, rect.y, size, rect.height + size), EditorGUIUtility.whiteTexture);

            //right
            GUI.DrawTexture(new Rect(rect.xMax - size, rect.y, size, rect.height + size), EditorGUIUtility.whiteTexture);

            GUI.color = orgColor;
        }

        /// <summary>
        /// 绘制对象选择窗口按钮，并在按钮点击后打开选择窗口
        /// <param name="prefix">按钮显示内容</param>
        /// <param name="pickObject">需要选择的对象</param>
        /// <return>当前对象</return>
        /// </summary>
        static public T ObjectPicker<T>(string prefix, T pickObject) where T : UnityEngine.Object
        {
            if (GUILayout.Button(prefix))
            {
                var controlId = GUIUtility.GetControlID(FocusType.Passive);
                typeof(EditorGUIUtility)
                        .GetMethod("ShowObjectPicker", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                        .MakeGenericMethod(typeof(T))
                        .Invoke(null, new object[] { null, true, string.Empty, controlId });
                _isShowObjectPicker = true;
            }

            if (_isShowObjectPicker && null != Event.current)
            {
                switch (Event.current.commandName)
                {
                    case "ObjectSelectorUpdated":
                        {
                            var currentPickObject = (T)EditorGUIUtility.GetObjectPickerObject();
                            if (pickObject != currentPickObject)
                            {
                                GUI.changed = true;
                            }
                            pickObject = currentPickObject;
                            break;
                        }
                    case "ObjectSelectorClosed":
                        {
                            var currentPickObject = (T)EditorGUIUtility.GetObjectPickerObject();
                            if (null != currentPickObject)
                            {
                                pickObject = currentPickObject;
                            }
                            _isShowObjectPicker = false;
                            break;
                        }
                    default: /*ignore*/ break;
                }
            }
            return pickObject;
        }
    }
}

