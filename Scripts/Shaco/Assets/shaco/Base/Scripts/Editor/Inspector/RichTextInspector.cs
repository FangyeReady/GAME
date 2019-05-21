using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using shaco.Base;

namespace shacoEditor
{

    [CustomEditor(typeof(shaco.RichText))]
    public class RichTextInspector : Editor
    {
        private Object _assetNew = null;
        private string _inputKey = string.Empty;
        private shaco.RichText.TextType _inputType = shaco.RichText.TextType.Text;
        private bool _isInputFolder = false;
        private List<string> _listWillRemoveValidAsset = new List<string>();
        private bool _isAutoUseFullName = false;
        private shaco.RichText.CharacterFolderInfo _currentCharacterFolderInfo = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var targetTmp = target as shaco.RichText;
            DrawRichTextInspector(targetTmp);
        }

        public void DrawRichTextInspector(shaco.RichText target)
        {
            if (null == target) return;

            this.Repaint();

            bool requireUpdateLayout = false;

            //text model
            GUI.changed = false;
            target.textModel = (UnityEngine.UI.Text)EditorGUILayout.ObjectField("Text Model", target.textModel, typeof(UnityEngine.UI.Text), true);
            if (GUI.changed)
            {
                requireUpdateLayout |= GUI.changed;
                if (null != target.textModel && target.textModel.resizeTextForBestFit)
                {
                    target.textModel.resizeTextMinSize = target.textModel.fontSize;
                    target.textModel.resizeTextMaxSize = target.textModel.fontSize;
                }
            }

            //auto wraper
            GUI.changed = false;
            target.autoWrap = EditorGUILayout.Toggle("Auto Wrap", target.autoWrap);
            requireUpdateLayout |= GUI.changed;

            //draw anchor
            GUI.changed = false;
            GUILayout.BeginVertical("box");
            {
                target.textAnchor = GUILayoutHelper.DrawAnchor("Text Anchor", target.textAnchor);
            }
            GUILayout.EndVertical();
            requireUpdateLayout |= GUI.changed;
            if (GUI.changed)
            {
                target.contentAnchor = target.textAnchor;
                shaco.UnityHelper.SetPivotByLocalPosition(target.gameObject, target.contentAnchor.ToPivot());
            }

            //draw margin
            GUI.changed = false;
            target.margin = EditorGUILayout.Vector3Field("Margin", target.margin);
            requireUpdateLayout |= GUI.changed;

            // GUI.changed = false;
            // target.contentAnchor = DrawAnchor("Content Anchor", target.contentAnchor);
            // requireUpdateLayout |= GUI.changed;
            // if (GUI.changed)
            //     shaco.UnityHelper.SetPivotByLocalPosition(target.gameObject, shaco.RichText.AnchorToPivot(target.contentAnchor));

            if (requireUpdateLayout)
                target.text = target.text;

            //draw text
            if (target.gameObject.GetComponent<shaco.LocalizationRichTextComponent>() == null)
            {
                GUILayout.BeginHorizontal();
                {
                    GUI.changed = false;
                    EditorGUILayout.PrefixLabel("Text");
                    var textTmp = EditorGUILayout.TextArea(target.text);
                    if (GUI.changed)
                    {
                        requireUpdateLayout = true;
                        target.text = textTmp;
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginVertical("box");
            {
                DrawNewAPI(target);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            {
                bool isShowCharacters = true;
                GUILayout.BeginHorizontal();
                {
                    isShowCharacters = GUILayoutHelper.DrawHeader("Characters", "RichTextEditorCharacter", null);

                    GUILayout.BeginVertical();
                    {
                        GUILayout.Space(0);
                        if (GUILayout.Button("Refresh", GUILayout.Width(56)))
                        {
                            for (int i = target.characterFolderPaths.Count - 1; i >= 0; --i)
                            {
                                var folderInfoTmp = target.characterFolderPaths[i];
                                var assetFolder = AssetDatabase.LoadAssetAtPath(folderInfoTmp.path, typeof(Object));
                                if (null == assetFolder)
                                {
                                    shaco.Log.Error("The folder has been lost and we will remove its record, path=" + folderInfoTmp.path);
                                    target.characterFolderPaths.RemoveAt(i);
                                }
                                else
                                    AddCharacterWithFolder(target, assetFolder);
                            }
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
                if (isShowCharacters)
                {
                    target.ForeachCharacters((shaco.RichText.CharacterInfo character) =>
                    {
                        GUILayout.BeginHorizontal();
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            {
                                EditorGUILayout.EnumPopup(character.type, GUILayout.Width(50));
                            }
                            EditorGUI.EndDisabledGroup();

                            EditorGUILayout.TextField(character.key, GUILayout.Width(Screen.width / 2));

                            EditorGUI.BeginDisabledGroup(true);
                            {
                                switch (character.type)
                                {
                                    case shaco.RichText.TextType.Image:
                                        {
                                            var spriteTmp = AssetDatabase.LoadAssetAtPath(character.value, typeof(Sprite)) as Sprite;
                                            if (null == spriteTmp) _listWillRemoveValidAsset.Add(character.key);
                                            EditorGUILayout.ObjectField(spriteTmp, typeof(Sprite), true);
                                            break;
                                        }
                                    case shaco.RichText.TextType.Prefab:
                                        {
                                            var prefabTmp = AssetDatabase.LoadAssetAtPath(character.value, typeof(GameObject)) as GameObject;
                                            if (null == prefabTmp) _listWillRemoveValidAsset.Add(character.key);
                                            EditorGUILayout.ObjectField(prefabTmp, typeof(GameObject), true);
                                            break;
                                        }
                                    default: shaco.Log.Info("unsupport type=" + character.type); break;
                                }
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        GUILayout.EndHorizontal();
                        return true;
                    });

                    if (_listWillRemoveValidAsset.Count > 0)
                    {
                        for (int i = _listWillRemoveValidAsset.Count - 1; i >= 0; --i)
                            target.RemoveCharacter(_listWillRemoveValidAsset[i]);
                        _listWillRemoveValidAsset.Clear();
                    }
                }
            }
            GUILayout.EndVertical();

            if (requireUpdateLayout)
            {
                EditorHelper.SetDirty(target);
            }
        }

        private void DrawNewAPI(shaco.RichText target)
        {
            Sprite newSprite = null;
            GameObject newPrefab = null;
            Object newFolder = null;

            if (GUILayoutHelper.DrawHeader("New Character", "RichTextEditorNew"))
            {
                if (_assetNew == null)
                {
                    GUI.changed = false;
                    newSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", newSprite, typeof(Sprite), true, GUILayout.Height(16));
                    if (GUI.changed)
                    {
                        _assetNew = newSprite;
                        _inputType = shaco.RichText.TextType.Image;
                        _isInputFolder = false;
                        AutoSetInputKey(_assetNew);
                    }

                    GUI.changed = false;
                    newPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", newPrefab, typeof(GameObject), true);
                    if (GUI.changed)
                    {
                        _assetNew = newPrefab;
                        _inputType = shaco.RichText.TextType.Prefab;
                        _isInputFolder = false;
                        AutoSetInputKey(_assetNew);
                    }

                    GUI.changed = false;
                    newFolder = EditorGUILayout.ObjectField("Folder", newFolder, typeof(Object), true);
                    if (GUI.changed && null != newFolder)
                    {
                        if (AssetDatabase.GetAssetPath(newFolder).EndsWith("Resources"))
                        {
                            _assetNew = newFolder;
                            _isInputFolder = true;
                        }
                        else 
                            shaco.Log.Error("folder must be 'Resources'");
                    }
                }
                else
                {
                    if (_isInputFolder)
                        GUILayout.Label("Comfirn add all character from folder");
                    else
                        _inputKey = EditorGUILayout.TextField("New Key", _inputKey);

                    _isAutoUseFullName = GUILayout.Toggle(_isAutoUseFullName, "UseFullName");
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Yes"))
                        {
                            if (_isInputFolder)
                            {
                                AddCharacterWithFolder(target, _assetNew);
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(_inputKey))
                                {
                                    AddCharacter(target, _inputKey, _assetNew, _inputType);
                                }
                            }
                            ResetInput();
                        }
                        if (GUILayout.Button("No"))
                        {
                            ResetInput();
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Clear"))
                    target.ClearCharacters();
            }
        }

        private void AutoSetInputKey(Object asset)
        {
            if (null == asset) return;

            var path = AssetDatabase.GetAssetPath(asset);
            _inputKey = path;
        }

        private void ResetInput()
        {
            _inputKey = string.Empty;
            _assetNew = null;
            _isInputFolder = false;
        }

        private string GetFullPath(Object asset)
        {
            if (null == asset)
            {
                shaco.Log.Error("RichTextEditor GetFullPath error: asset is null");
                return string.Empty;
            }

            var path = AssetDatabase.GetAssetPath(asset);
            path = path.Replace("\\", shaco.Base.FileDefine.PATH_FLAG_SPLIT).Remove("Assets/");
            path = Application.dataPath + "/" + path;
            return path;
        }

        private void AddCharacter(shaco.RichText target, string key, Object asset, shaco.RichText.TextType type)
        {
            if (string.IsNullOrEmpty(key) || asset == null)
                return;
 
            if (!_isAutoUseFullName)
                key = FileHelper.GetLastFileName(key);

            if (target.HasCharacter(key))
                return;

            var newCharacter = new shaco.RichText.CharacterInfo();

            newCharacter.key = key;
            newCharacter.type = type;
            newCharacter.value = AssetDatabase.GetAssetPath(asset);
            target.AddCharacter(newCharacter);
        }

        private void AddCharacterWithFolder(shaco.RichText target, Object asset)
        {
            if (null == asset)
            {
                shaco.Log.Info("RichTextEditor AddCharacterWithFolder error: asset is null");
                return;
            }

            var fullPath = GetFullPath(asset);
            if (!FileHelper.ExistsDirectory(fullPath))
                shaco.Log.Error("Please Select a folder");
            else
            {
                AddCharacterFolderSafe(target, asset);

                List<string> allPaths = new List<string>();
                FileHelper.GetSeekPath(fullPath, ref allPaths, ".png", ".jpg", ".jpeg", ".bmp", ".prefab");
                var projectPath = Application.dataPath.Remove("Assets");
                for (int i = 0; i < allPaths.Count; ++i)
                {
                    var pathTmp = allPaths[i];
                    var assetPath = pathTmp.Remove(projectPath);

                    if (pathTmp.EndsWith(".prefab"))
                        AddCharacter(target, assetPath, AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject, shaco.RichText.TextType.Prefab);
                    else
                        AddCharacter(target, assetPath, AssetDatabase.LoadAssetAtPath(assetPath, typeof(Sprite)) as Sprite, shaco.RichText.TextType.Image);
                }
            }
        }

        private void AddCharacterFolderSafe(shaco.RichText target, Object asset)
        {
            bool findFolder = false;
            var pathAsset = AssetDatabase.GetAssetPath(asset);

            for (int i = target.characterFolderPaths.Count - 1; i >= 0; --i)
            {
                if (target.characterFolderPaths[i].path == pathAsset)
                {
                    _currentCharacterFolderInfo = target.characterFolderPaths[i];
                    findFolder = true;
                    break;
                }
            }

            if (!findFolder)
            {
                _currentCharacterFolderInfo = new shaco.RichText.CharacterFolderInfo();
                _currentCharacterFolderInfo.path = pathAsset;
                _currentCharacterFolderInfo.isAutoUseFullName = _isAutoUseFullName;
                target.characterFolderPaths.Add(_currentCharacterFolderInfo);
            }
        }
    }
}