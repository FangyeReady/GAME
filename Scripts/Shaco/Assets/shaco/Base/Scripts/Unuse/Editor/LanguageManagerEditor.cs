#if !USE_UGUI
#if UNITY_4_6_OR_NEWER || UNITY_5_3_OR_NEWER
#define USE_UGUI
#endif
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    [CustomEditor(typeof(shaco.LanguageManager))]
    [ExecuteInEditMode]
    [CanEditMultipleObjects]
    public class LanguageManagerEditor : Editor
    {
#if SUPPORT_NGUI
        private shaco.LanguageManager.Info _infoNGUISelect = null;
        private int _iNGUISelectIndex = 0;
#endif

        public override void OnInspectorGUI()
        {
            bool isValueChanged = false;
            var _target = target as shaco.LanguageManager;

            base.OnInspectorGUI();

            var listResourceFolder = new EditorHelper.ListHelper<shaco.LanguageManager.Info>();
            listResourceFolder.AutoListSize("ResourceFolder", _target.ListInfo, (shaco.LanguageManager.Info value) =>
            {
                return value == null || value.target == null;
            });

            float spaceHorizontal = 30;
            float spaceEnglishLabel = 13;

            for (int i = 0; i < _target.ListInfo.Count; ++i)
            {
                if (_target.ListInfo[i] == null)
                {
                    _target.ListInfo[i] = new shaco.LanguageManager.Info();
                    isValueChanged = true;
                }

                var infoTmp = _target.ListInfo[i];

                //draw target
                GUILayout.BeginHorizontal();

                if (infoTmp.target)
                {
                    if (GUILayout.Button(infoTmp.isHide ? "▼" : "▶", GUILayout.Width(30)))
                    {
                        infoTmp.isHide = !infoTmp.isHide;
                        isValueChanged = true;

                        //check params valid 
                        if (infoTmp.isHide)
                        {
                            infoTmp.isHide = _target.isValidInfoParams(infoTmp);
                            if (!infoTmp.isHide)
                            {
                                shaco.Log.Info(infoTmp.target + ": has invalid params");
                            }
                        }
                    }
                }
                else
                {
                    GUI.color = Color.green;
                    GUILayout.Label("new", GUILayout.Width(30));
                    GUI.color = Color.white;
                }

                var prevTarget = infoTmp.target;
                infoTmp.target = EditorGUILayout.ObjectField(infoTmp.target, typeof(GameObject), true) as GameObject;
                if (prevTarget != infoTmp.target)
                {
                    _target.updateTargetInfo(infoTmp);
                }
                GUILayout.EndHorizontal();

                if (infoTmp.isHide)
                    continue;

                bool b1 = false;
                bool b2 = false;
#if USE_UGUI
                b1 = infoTmp.type == shaco.LanguageManager.TargetType.UGUI_Image;
#endif
#if SUPPORT_NGUI
				b1 = infoTmp.type == shaco.LanguageManager.TargetType.NGUI_Sprite2D;    
#endif

                if (infoTmp.target && (b1 || b2))
                {
                    //draw type
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(spaceHorizontal);
                    EditorGUILayout.EnumPopup(infoTmp.type);
                    GUILayout.EndHorizontal();

                    //draw flag wheather to set native image size when language changed by automatic
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(spaceHorizontal);

                    GUILayout.Label("SetNativeSize");

                    bool prevValue1 = infoTmp.isSetNativeSize;
                    infoTmp.isSetNativeSize = EditorGUILayout.Toggle(infoTmp.isSetNativeSize);
                    if (prevValue1 != infoTmp.isSetNativeSize)
                        isValueChanged = true;

                    GUILayout.EndHorizontal();
                }

                //draw values
                bool isValidParam = false;
                switch (infoTmp.type)
                {
#if USE_UGUI
                    case shaco.LanguageManager.TargetType.UGUI_Image:
#endif
#if SUPPORT_NGUI
                    case shaco.LanguageManager.TargetType.NGUI_Sprite2D:
#endif
                    case shaco.LanguageManager.TargetType.PlaceHolder1:
                        {
                            for (int j = 0; j < infoTmp.sprites.Length; ++j)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(spaceHorizontal);

                                GUILayout.Label(((shaco.LanguageManager.LanguageType)j).ToString());
                                if (j == (int)shaco.LanguageManager.LanguageType.English) GUILayout.Space(spaceEnglishLabel);

                                var prevValue = infoTmp.sprites[j];
                                isValidParam = infoTmp.sprites[j] != null;

                                if (!isValidParam) GUI.color = Color.red;
                                infoTmp.sprites[j] = EditorGUILayout.ObjectField(infoTmp.sprites[j], typeof(Sprite), true) as Sprite;
                                if (!isValidParam) GUI.color = Color.white;

                                if (prevValue != infoTmp.sprites[j])
                                    isValueChanged = true;

                                GUILayout.EndHorizontal();
                            }
                            break;
                        }
#if USE_UGUI
                    case shaco.LanguageManager.TargetType.UGUI_Text:
                    case shaco.LanguageManager.TargetType.shaco_RichText:
#endif
#if SUPPORT_NGUI
                    case shaco.LanguageManager.TargetType.NGUI_Label:
#endif
                    case shaco.LanguageManager.TargetType.PlaceHolder2:
                        {
                            for (int j = 0; j < infoTmp.texts.Length; ++j)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(spaceHorizontal);
                                GUILayout.Label(((shaco.LanguageManager.LanguageType)j).ToString());

                                var prevValue = infoTmp.texts[j];
                                isValidParam = infoTmp.texts[j] != null;

                                if (!isValidParam) GUI.color = Color.red;
                                infoTmp.texts[j] = EditorGUILayout.TextArea(infoTmp.texts[j]);
                                if (!isValidParam) GUI.color = Color.white;

                                if (prevValue != infoTmp.texts[j])
                                    isValueChanged = true;

                                GUILayout.EndHorizontal();
                            }
                            break;
                        }
#if SUPPORT_NGUI
                    case shaco.LanguageManager.TargetType.NGUI_SpriteAtals:
                        {
                            if (infoTmp.texts.Length != infoTmp.atals.Length)
                            {
                                shaco.Log.Error("texts length must is equal to atals length");
                                break;
                            }
                            for (int j = 0; j < infoTmp.atals.Length; ++j)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(spaceHorizontal);
                                GUILayout.Label(((shaco.LanguageManager.LanguageType)j).ToString());
                                GUILayout.EndHorizontal();

                                var prevValue = infoTmp.texts[j];
                                var prevValue2 = infoTmp.atals[j];
                                isValidParam = infoTmp.texts[j] != null;
                                isValidParam = infoTmp.atals[j] != null;

                                GUILayout.BeginHorizontal();
                                GUILayout.Space(-15);
                                GUILayout.Space(spaceHorizontal * 2);
                                
                                if (!isValidParam) GUI.color = Color.red;
                                infoTmp.atals[j] = EditorGUILayout.ObjectField("Atals", infoTmp.atals[j], typeof(UIAtlas)) as UIAtlas;

                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();
                                GUILayout.Space(spaceHorizontal * 2);

                                if (infoTmp.atals[j] && GUILayout.Button("Sprite", "DropDown", GUILayout.Width(60)))
                                {
                                    _infoNGUISelect = infoTmp;
                                    _iNGUISelectIndex = j;
                                    NGUISettings.atlas = infoTmp.atals[j];
                                    NGUISettings.selectedSprite = infoTmp.texts[j];
                                    SpriteSelector.Show((string spriteName) =>{

                                        _infoNGUISelect.texts[_iNGUISelectIndex] = spriteName;
                                        prevValue = spriteName;
                                    });
                                }

                                GUILayout.Label(infoTmp.texts[j], "HelpBox");

                                if (!isValidParam) GUI.color = Color.white;

                                if (prevValue != infoTmp.texts[j] || prevValue2 != infoTmp.atals[j])
                                    isValueChanged = true;

                                GUILayout.EndHorizontal();
                            }
                            break;
                        }
#endif
                    case shaco.LanguageManager.TargetType.GameObject:
                        {
                            for (int j = 0; j < infoTmp.targetGameObjects.Length; ++j)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(spaceHorizontal);
                                GUILayout.Label(((shaco.LanguageManager.LanguageType)j).ToString());
                                if (j == (int)shaco.LanguageManager.LanguageType.English) GUILayout.Space(spaceEnglishLabel);

                                var prevValue = infoTmp.targetGameObjects[j].targetGameObject;
                                isValidParam = infoTmp.targetGameObjects[j].targetGameObject != null;

                                if (!isValidParam) GUI.color = Color.red;
                                infoTmp.targetGameObjects[j].targetGameObject = EditorGUILayout.ObjectField(infoTmp.targetGameObjects[j].targetGameObject, typeof(GameObject), true) as GameObject;
                                if (!isValidParam) GUI.color = Color.white;

                                if (prevValue != infoTmp.targetGameObjects[j].targetGameObject)
                                {
                                    isValueChanged = true;

                                    //if previous value is valid gamebject, we will destroy old gameobject
                                    if (prevValue != null && infoTmp.targetGameObjects[j].isAutoDestroy)
                                    {
                                        DestroyImmediate(prevValue);
                                    }

                                    var pathCheck = AssetDatabase.GetAssetPath(infoTmp.targetGameObjects[j].targetGameObject);
                                    infoTmp.targetGameObjects[j].isResourcePrefab = !string.IsNullOrEmpty(pathCheck);
                                    infoTmp.targetGameObjects[j].isAutoDestroy = false;
                                }

                                GUILayout.EndHorizontal();
                            }
                            break;
                        }
                    default: break;
                }

                //draw callback
                if (infoTmp.target)
                {
                    EventDelegateEditorS.Field(_target.gameObject, infoTmp.listCallBack, spaceHorizontal);
                }
            }

            GUILayout.Label("Language: " + shaco.LanguageManager.getCurrentLanguage());

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("ChangeLanguage", GUILayout.Height(40)))
            {
                var curLanguage = shaco.LanguageManager.getCurrentLanguage();
                ++curLanguage;
                if (curLanguage == shaco.LanguageManager.LanguageType.Count)
                    curLanguage = shaco.LanguageManager.LanguageType.Chineses;

                shaco.LanguageManager.changeLanguage(curLanguage);

                isValueChanged = true;
            }

            if (GUILayout.Button("ClearAll", GUILayout.Height(40)))
            {
                _target.ListInfo.Clear();
            }
            GUILayout.EndHorizontal();

            if (isValueChanged)
                EditorHelper.SetDirty(_target.gameObject);
        }
    }
}