using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace shaco
{
    //仅使用在Untiy4.6+版本
    [RequireComponent(typeof(RectTransform))]
    public class RichText : MonoBehaviour
    {
        [System.Serializable]
        public enum TextType
        {
            Text,
            Image,
            Prefab
        }

        [System.Serializable]
        public class CharacterInfo
        {
            public TextType type = TextType.Image;
            public string key = string.Empty;
            public string value = string.Empty;
        }

        [System.Serializable]
        public class CharacterFolderInfo
        {
            public string path = string.Empty;
            public bool isAutoUseFullName = true;
        }

        private class ContentInfo
        {
            public string value = string.Empty;
            public GameObject item = null;
            public bool isReturn = false;
        }

        public string text
        {
            get { return _text; }
            set { SetText(value); }
        }

        [HideInInspector]
        public Text textModel = null;

        [HideInInspector]
        public bool autoWrap = true;

        [HideInInspector]
        public TextAnchor textAnchor = TextAnchor.UpperLeft;

        [HideInInspector]
        public TextAnchor contentAnchor = TextAnchor.UpperLeft;

        [HideInInspector]
        public Vector2 margin = Vector2.zero;

        [HideInInspector]
        public List<CharacterFolderInfo> characterFolderPaths = new List<CharacterFolderInfo>();

        [SerializeField]
        [HideInInspector]
        private List<CharacterInfo> _savedCharacters = new List<CharacterInfo>();

        [SerializeField]
        [HideInInspector]
        private string _text = string.Empty;

        private Dictionary<string, CharacterInfo> _searchCharacters = new Dictionary<string, CharacterInfo>();
        private List<ContentInfo> _displayCharacters = new List<ContentInfo>();
        private const string SPLIT_FLAG = "@";
        private bool _updateLayoutDirty = true;
        private GameObject contentParent = null;

        private readonly string DEFEAULT_DYNAMIC_CHARACTER_FOLDER = "richtextpng";

        void Start()
        {
            if (null != textModel)
            {
                textModel.enabled = false;
            }

            SetText(_text);
        }

        void Update()
        {
            UpdateLayout();
        }

        void OnValidate()
        {
            UpdateListDataToDictionaryData();
        }

        void OnDestroy()
        {
            _displayCharacters.Clear();
        }

        public void SetText(string text)
        {
            _displayCharacters.Clear();

            UnityHelper.RemoveChildren(this.gameObject);
            contentParent = null;
            CheckComponents();

            _text = text;

            if (string.IsNullOrEmpty(text)) return;

            var texts = text.Split(SPLIT_FLAG);
            bool needReturn = false;

            for (int i = 0; i < texts.Length; ++i)
            {
                var textTmp = texts[i];

                if (textTmp.Contains("\n"))
                {
                    var textsTmp = textTmp.Split('\n');
                    for (int j = 0; j < textsTmp.Length; ++j)
                    {
                        if (j == textsTmp.Length - 1 && string.IsNullOrEmpty(textsTmp[j])) continue;

                        var contentTmp = SetTextWithRichText(textsTmp[j]);
                        if (needReturn)
                        {
                            needReturn = false;
                            contentTmp.isReturn = true;
                        }
                        else
                            contentTmp.isReturn = j > 0;
                    }
                }
                else
                {
                    var contentTmp = SetTextWithRichText(texts[i]);
                    if (needReturn)
                    {
                        needReturn = false;
                        contentTmp.isReturn = true;
                    }
                }

                if (textTmp.Length > 0 && textTmp[textTmp.Length - 1] == '\n')
                    needReturn = true;
            }

            SetUpdateLayoutDirty();
        }

        public void ForeachCharacters(System.Func<CharacterInfo, bool> callback)
        {
            foreach (var iter in _searchCharacters)
            {
                if (!callback(iter.Value))
                    break;
            }
        }

        public bool HasCharacter(string key)
        {
            return _searchCharacters.ContainsKey(key);
        }

        public bool AddCharacter(CharacterInfo character)
        {
            if (_searchCharacters.ContainsKey(character.key)) return false;
            _searchCharacters.Add(character.key, character);
            _savedCharacters.Add(character);
            return true;
        }

        public bool RemoveCharacter(string key)
        {
            if (!_searchCharacters.ContainsKey(key)) return false;
            _searchCharacters.Remove(key);
            for (int i = _savedCharacters.Count - 1; i >= 0; --i)
            {
                if (_savedCharacters[i].key == key)
                {
                    _savedCharacters.RemoveAt(i);
                    break;
                }
            }
            return true;
        }

        public void ClearCharacters()
        {
            _searchCharacters.Clear();
            _savedCharacters.Clear();
            characterFolderPaths.Clear();
        }

        private void CheckComponents()
        {
            if (null == contentParent)
            {
                contentParent = new GameObject("Content");
                UnityHelper.ChangeParentLocalPosition(contentParent, this.gameObject);
            }
        }

        private void SetUpdateLayoutDirty()
        {
            _updateLayoutDirty = true;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UpdateLayout();
            }
#endif
        }

        private ContentInfo SetTextWithRichText(string text)
        {
            ContentInfo newContent = null;
            CharacterInfo characterTmp = null;
            if (!_searchCharacters.ContainsKey(text))
            {
                if (text.EndsWith(".png") && !string.IsNullOrEmpty(DEFEAULT_DYNAMIC_CHARACTER_FOLDER))
                {
                    characterTmp = new CharacterInfo();
                    characterTmp.type = TextType.Image;
#if UNITY_EDITOR
                    characterTmp.value = shaco.Base.FileHelper.ContactPath("Assets/Resources/" + DEFEAULT_DYNAMIC_CHARACTER_FOLDER, text);
#else
                    characterTmp.value = shaco.Base.FileHelper.ContactPath(DEFEAULT_DYNAMIC_CHARACTER_FOLDER, text.Remove(".png"));
#endif
                }

                if (null == characterTmp && null == newContent)
                {
                    CharacterInfo contentTmp = new CharacterInfo();
                    contentTmp.value = text;
                    newContent = CreateText(contentTmp);
                }
            }

            if (null == newContent)
            {
                if (null == characterTmp)
                {
                    characterTmp = _searchCharacters[text];
                }

                switch (characterTmp.type)
                {
                    case TextType.Text: newContent = CreateText(characterTmp); break;
                    case TextType.Image: newContent = CreateImage(characterTmp); break;
                    case TextType.Prefab: newContent = CreatePrefab(characterTmp); break;
                    default: shaco.Log.Error("RichText SetTextWithRichText error: unsupport type=" + characterTmp.type); break;
                }
            }

            newContent.item.SetActive(false);

            _displayCharacters.Add(newContent);
            return newContent;
        }

        private ContentInfo CreateText(CharacterInfo character)
        {
            var retValue = new ContentInfo();
            Text textTmp = null;
            if (null == textModel)
            {
                retValue.item = CreateGameObject();
                textTmp = retValue.item.AddComponent<Text>();
                if (null == textTmp.font)
                {
#if UNITY_5_3_OR_NEWER
                    textTmp.font = Font.CreateDynamicFontFromOSFont("Arial", 16);
#else
                    textTmp.font = new Font("Arial");
                    textTmp.fontSize = 16;
#endif
                }
            }
            else
            {
                textTmp = MonoBehaviour.Instantiate(textModel) as Text;
                textTmp.enabled = true;
                retValue.item = textTmp.gameObject;
                retValue.item.SetActive(true);
                UnityHelper.ChangeParentLocalPosition(retValue.item, contentParent.gameObject);
            }

            var transTmp = GetComponent<RectTransform>();
            var transText = textTmp.GetComponent<RectTransform>();

            //设置锚点
            textTmp.alignment = textAnchor;

            //根据文本的alignment自动设置锚点
            transText.pivot = textTmp.alignment.ToPivot();

            //获取显示行数
            textTmp.text = string.Empty;
            var heightLine = textTmp.preferredHeight;

            //如果是best fit模式，需要把空格先替换为其他字符然后再进行计算宽度
            textTmp.text = textTmp.resizeTextForBestFit ? character.value.Replace(' ', '_') : character.value;

            float pivotOffset = 0.0f;
            if (transText.pivot.x == 0.5f)
            {
                pivotOffset = 1.0f;
            }
            else if (transText.pivot.x == 0.0f)
            {
                pivotOffset = 1 - transTmp.pivot.x;
            }
            else if (transText.pivot.x == 1.0f)
            {
                pivotOffset = transTmp.pivot.x;
            }
            else
            {
                shaco.Log.Error("RichText CreateText error: unsupport text pivot=" + transText.pivot);
            }

            var widthLine = Mathf.Min(transTmp.rect.width * pivotOffset, textTmp.preferredWidth);
            var realPreferredSize = new Vector2(widthLine, heightLine);

            if (autoWrap)
            {
                var viewWidth = transTmp.rect.width;
                int lineCount = (int)(textTmp.preferredWidth / (viewWidth)) + 1;
                realPreferredSize = new Vector2(realPreferredSize.x, realPreferredSize.y * lineCount);

                //TextSpacing脚本暂不支持多行的文字
                if (lineCount > 1 && HasTextSpacing(textTmp))
                {
                    shaco.Log.Warning("RichText CreateText warning: unsupport multi line when use 'TextSpacing' component for the time being...");
                }
            }

            //先粗略设置Text一次宽高
            transText.sizeDelta = realPreferredSize;

            //再根据Text精准计算宽高
            transText.sizeDelta = new Vector2(realPreferredSize.x + GetOffsetWidthByTextSpacing(textTmp), textTmp.preferredHeight + 1);

            if (textTmp.resizeTextForBestFit)
                textTmp.text = character.value;

            return retValue;
        }

        //是否存在TextSpacing脚本
        private bool HasTextSpacing(UnityEngine.UI.Text newText)
        {
#if UNITY_5_3_OR_NEWER
            return newText.GetComponent<shaco.TextSpacing>() != null;
#else
            return false;
#endif
        }

        //获取TextSpacing脚本偏移宽度
        private float GetOffsetWidthByTextSpacing(UnityEngine.UI.Text newText)
        {
            float retValue = 0;

            if (null != textModel)
            {
#if UNITY_5_3_OR_NEWER
                var scriptTextSpacing = textModel.GetComponent<shaco.TextSpacing>();
                if (null != scriptTextSpacing)
                {
                    retValue = (newText.text.Length - 1) * scriptTextSpacing.offset;
                }
#endif
            }
            return retValue;
        }

        private ContentInfo CreateImage(CharacterInfo character)
        {
            var retValue = new ContentInfo();
            retValue.item = CreateGameObject();
            var imageTmp = retValue.item.AddComponent<Image>();
            imageTmp.sprite = LoadAsset<Sprite>(character.value);
            imageTmp.SetNativeSize();

            return retValue;
        }

        private ContentInfo CreatePrefab(CharacterInfo character)
        {
            var retValue = new ContentInfo();
            var obj = LoadAsset<GameObject>(character.value);
            retValue.item = MonoBehaviour.Instantiate(obj) as GameObject;
            retValue.item.SetActive(true);
            UnityHelper.ChangeParentLocalPosition(retValue.item.gameObject, contentParent.gameObject);
            var transTmp = retValue.item.GetComponent<RectTransform>();
            if (transTmp == null)
                shaco.Log.Error("RichText CreatePrefab error: missing 'RectTransform' prefab=" + retValue);
            return retValue;
        }

        private T LoadAsset<T>(string path) where T : Object
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T;
#else
            return ResourcesEx.LoadResourcesOrLocal<T>(GetAssetPathWithoutResourcesFolder(path));
#endif
        }

        private GameObject CreateGameObject()
        {
            var retValue = new GameObject();
            var transTmp = retValue.GetComponent<RectTransform>();
            if (transTmp == null)
                transTmp = retValue.AddComponent<RectTransform>();
            transTmp.pivot = textAnchor.ToPivot();

            UnityHelper.ChangeParentLocalPosition(retValue, contentParent.gameObject);
            return retValue;
        }

        private void UpdateLayout()
        {
            if (!_updateLayoutDirty) return;
            _updateLayoutDirty = false;

            if (_displayCharacters.Count == 0) return;

            var transItemPrev = _displayCharacters[0].item.GetComponent<RectTransform>();
            transItemPrev.localPosition = Vector2.zero;
            var transItemsPrevLine = new List<RectTransform>();
            transItemsPrevLine.Add(transItemPrev);

            transItemPrev.gameObject.SetActive(true);

            for (int i = 1; i < _displayCharacters.Count; ++i)
            {
                var contentTmp = _displayCharacters[i];
                var transItemNext = contentTmp.item.GetComponent<RectTransform>();

                contentTmp.item.SetActive(true);

                SetItemLayout(contentTmp, ref transItemPrev, transItemNext, transItemsPrevLine);
                if (CheckAutoWrap(contentTmp, transItemsPrevLine))
                    SetItemLayout(contentTmp, ref transItemPrev, transItemNext, transItemsPrevLine);

                transItemPrev = transItemNext;
                transItemsPrevLine.Add(transItemNext);
            }

            UpdatePrevLineItemsLayoutWhenMiddleAnchor(transItemsPrevLine);
            UpdateItemsLayoutWhenMiddleAnchor();
        }

        private bool CheckAutoWrap(ContentInfo contentInfo, List<RectTransform> transItemsPrevLine)
        {
            if (!autoWrap || contentInfo.isReturn) return false;

            var transTmp = contentInfo.item.GetComponent<RectTransform>();
            var transMax = this.GetComponent<RectTransform>();

            var allItemSize = transTmp.sizeDelta;
            for (int i = 0; i < transItemsPrevLine.Count; ++i)
            {
                allItemSize += transItemsPrevLine[i].GetComponent<RectTransform>().sizeDelta;
            }

            if (allItemSize.x > transMax.sizeDelta.x)
            {
                contentInfo.isReturn = true;
                return true;
            }
            else
                return false;
        }

        private void SetItemLayout(ContentInfo contentInfo, ref RectTransform transItemPrev, RectTransform transItemNext, List<RectTransform> transItemsPrevLine)
        {
            Rect rectItemNext = new Rect(0, 0, transItemNext.sizeDelta.x, transItemNext.sizeDelta.y);
            Vector3 pivotNext = Vector3.zero;

            if (contentInfo.isReturn)
            {
                var transFirstItem = transItemsPrevLine[0];
                float offsetVertical = 0;

                bool isNegative = textAnchor == TextAnchor.LowerLeft || textAnchor == TextAnchor.LowerCenter || textAnchor == TextAnchor.LowerRight;
                Vector3 pivotPrevLine = Vector3.zero;
                if (isNegative)
                {
                    pivotPrevLine = new Vector3(transFirstItem.pivot.x, 1);
                    pivotNext = new Vector3(transFirstItem.pivot.x, 0);
                    offsetVertical = margin.y;
                }
                else
                {
                    pivotPrevLine = new Vector3(transFirstItem.pivot.x, 0);
                    pivotNext = new Vector3(transFirstItem.pivot.x, 1);
                    offsetVertical = -margin.y;
                }

                rectItemNext.position = shaco.UnityHelper.GetLocalPositionByPivot(transFirstItem.gameObject, pivotPrevLine) + new Vector3(0, offsetVertical);

                UpdatePrevLineItemsLayoutWhenMiddleAnchor(transItemsPrevLine);
                transItemsPrevLine.Clear();
            }
            else
            {
                Vector3 pivotSource = Vector3.zero;
                Vector3 pivotDes = Vector3.zero;
                float offsetHorizontal = 0;
                if (transItemPrev.pivot.x == 0.5f)
                {
                    pivotSource = shaco.Pivot.RightMiddle;
                    pivotDes = shaco.Pivot.LeftMiddle;
                    offsetHorizontal = -margin.x;
                }
                else
                {
                    pivotSource = new Vector3(1 - transItemPrev.pivot.x, 0.5f);
                    pivotDes = new Vector3(transItemPrev.pivot.x, 0.5f);
                    offsetHorizontal = margin.x;
                }

                rectItemNext.position = shaco.UnityHelper.GetLocalPositionByPivot(transItemPrev.gameObject, pivotSource) + new Vector3(offsetHorizontal, 0);
                pivotNext = pivotDes;
            }

            shaco.UnityHelper.SetLocalPositionByPivot(transItemNext.gameObject, rectItemNext.position, pivotNext);
        }

        private void UpdatePrevLineItemsLayoutWhenMiddleAnchor(List<RectTransform> transItemsPrevLine)
        {
            if (transItemsPrevLine.Count < 2)
                return;

            if (contentAnchor != TextAnchor.UpperCenter && contentAnchor != TextAnchor.MiddleCenter && contentAnchor != TextAnchor.LowerCenter)
                return;

            var firstItem = transItemsPrevLine[0];
            var lastItem = transItemsPrevLine[transItemsPrevLine.Count - 1];
            var leftMiddlePos = shaco.UnityHelper.GetLocalPositionByPivot(firstItem.gameObject, shaco.Pivot.LeftMiddle);
            var rightMiddlePos = shaco.UnityHelper.GetLocalPositionByPivot(lastItem.gameObject, shaco.Pivot.RightMiddle);

            var rectTmp = new Rect(leftMiddlePos.x, leftMiddlePos.y, rightMiddlePos.x, rightMiddlePos.y);

            var offsetPosOfContentCenter = rectTmp.center - this.GetComponent<RectTransform>().rect.center;
            rectTmp.position -= new Vector2(offsetPosOfContentCenter.x + firstItem.rect.width / 4, 0);

            shaco.UnityHelper.SetLocalPositionByPivot(firstItem.gameObject, rectTmp.min, shaco.Pivot.LeftMiddle);
            var prevItem = firstItem;
            for (int i = 1; i < transItemsPrevLine.Count; ++i)
            {
                var itemTmp = transItemsPrevLine[i];
                var prevItemPos = shaco.UnityHelper.GetLocalPositionByPivot(prevItem.gameObject, shaco.Pivot.RightMiddle);
                shaco.UnityHelper.SetLocalPositionByPivot(itemTmp.gameObject, prevItemPos, shaco.Pivot.LeftMiddle);

                prevItem = itemTmp;
            }
        }

        private void UpdateItemsLayoutWhenMiddleAnchor()
        {
            if (_displayCharacters.Count <= 1)
                return;

            if (contentAnchor != TextAnchor.MiddleLeft && contentAnchor != TextAnchor.MiddleCenter && contentAnchor != TextAnchor.MiddleRight)
                return;

            var firstItem = _displayCharacters[0];
            var lastItem = _displayCharacters[_displayCharacters.Count - 1];

            float topY = UnityHelper.GetLocalPositionByPivot(firstItem.item, shaco.Pivot.MiddleTop).y;
            float downY = UnityHelper.GetLocalPositionByPivot(lastItem.item, shaco.Pivot.MiddleBottom).y;

            var allHeight = topY - downY;
            var maxTop = allHeight / 2;
            var offsetPositionY = maxTop - UnityHelper.GetLocalPositionByPivot(firstItem.item, shaco.Pivot.MiddleTop).y;

            for (int i = 0; i < _displayCharacters.Count; ++i)
            {
                var itemTmp = _displayCharacters[i];
                itemTmp.item.transform.localPosition += new Vector3(0, offsetPositionY, 0);
            }
        }

        private string GetAssetPathWithoutResourcesFolder(string path)
        {
            //因为Resources目录比较特殊，需要去除前面的相对路径才能正常读取
            int indexFind = path.IndexOf("Resources/");
            if (indexFind >= 0)
                path = path.Remove(0, indexFind + "Resources/".Length);

            indexFind = path.IndexOf(ResourcesEx.DEFAULT_PREFIX_PATH_LOWER);
            if (indexFind >= 0)
                path = path.Remove(0, indexFind + ResourcesEx.DEFAULT_PREFIX_PATH_LOWER.Length);

            return path;
        }

        private void UpdateListDataToDictionaryData()
        {
            if (_searchCharacters.Count > 0) return;

            for (int i = _savedCharacters.Count - 1; i >= 0; --i)
            {
                var characterTmp = _savedCharacters[i];
                _searchCharacters.Add(characterTmp.key, characterTmp);
            }
        }
    }
}
