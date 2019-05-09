using UnityEngine;
using System.Collections;

namespace shaco
{
    public class LocalizationComponent : MonoBehaviour
    {
        public enum TargetType
        {
            None,
            Text,
            Image,
            Prefab
        }

        public TargetType type = TargetType.Text;
        public string languageKey
        {
            get { return _languageKey; }
            set
            {
                if (_languageKey != value)
                {
                    _languageKey = value;
                    UpdateLocalization();
                }
            }
        }

		public int fontSize
		{
			get { return _fontSize; }
			set
			{
				if (_fontSize != value)
				{
					_fontSize = value;
				}
			}
		}

        [HideInInspector]
        public string[] formatParams = new string[0];

        private bool isInited = false;

        [SerializeField]
        [HideInInspector]
        private string _languageKey = string.Empty;
		private int _fontSize = 0;

        void OnEnable()
        {
            if (isInited)
                UpdateLocalization();
        }

        void Start()
        {
            if (!isInited)
            {
                isInited = true;
                UpdateLocalization();
            }
        }

        public void UpdateLocalization()
        {
            switch (type)
            {
                case TargetType.None: /*do nothing*/ break;
                case TargetType.Text: InitWithText(); break;
                case TargetType.Image: InitWithImage(); break;
                case TargetType.Prefab: InitWithPrefab(); break;
                default: shaco.Log.Error("LocalizationComponent error: unsupport type=" + type); break;
            }
        }

        private void InitWithText()
        {
            var textTargetTmp = this.GetComponent<UnityEngine.UI.Text>();
            if (null == textTargetTmp)
            {
                Log.Error("LocalizationComponent InitWithText error: Require UnityEngine.UI.Text Component ! target name=" + this.name);
                return;
            }
            var localizationFont = shaco.Localization.GetUnityFont();
            if (Application.isPlaying && null != localizationFont && textTargetTmp.font != localizationFont)
                textTargetTmp.font = localizationFont;
            textTargetTmp.text = shaco.Localization.GetTextFormat(languageKey, formatParams);

#if UNITY_EDITOR
            //force to update text in scene
            UnityEditor.EditorUtility.SetDirty(textTargetTmp);
#endif
        }

        private void InitWithImage()
        {
            var imageTargetTmp = this.GetComponent<UnityEngine.UI.Image>();
            if (null == imageTargetTmp)
            {
                Log.Error("LocalizationComponent InitWithImagePath error: Require UnityEngine.UI.Image Component ! target name=" + this.name);
                return;
            }
            var pathTmp = shaco.Localization.GetTextFormat(languageKey, formatParams);
            imageTargetTmp.sprite = shaco.Localization.GetResource<Sprite>(pathTmp);

            if (imageTargetTmp.sprite == null)
            {
                Log.Error("LocalizationComponent InitWithImagePath error: cannot load resource key=" + languageKey + " path=" + pathTmp);
            }
        }

        private void InitWithPrefab()
        {
            var pathTmp = shaco.Localization.GetTextFormat(languageKey, formatParams);
            var newPrefab = shaco.Localization.GetResource<GameObject>(pathTmp);
            if (null == newPrefab)
            {
                Log.Error("LocalizationComponent InitWithPrefab error: cannot load resource key=" + languageKey + " path=" + pathTmp);
                return;
            }
            else
            {
                newPrefab = MonoBehaviour.Instantiate(newPrefab) as GameObject;
            }

            shaco.UnityHelper.ChangeParentLocalPosition(newPrefab, this.transform.parent.gameObject);
            MonoBehaviour.Destroy(this.gameObject);
        }
    }
}

