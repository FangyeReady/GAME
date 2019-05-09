using UnityEngine;
using System.Collections;

namespace shaco
{
    [RequireComponent(typeof(RichText))]
    public class LocalizationRichTextComponent : MonoBehaviour
    {
        public string languageKey
        {
            get { return _languageKey; }
            set { SetText(value); }
        }

		public int fontSize
		{
			get { return _fontSize; }
			set {
					if (value != _fontSize)
					{
						SetFontSize(value);
					}
				}
		}

        [HideInInspector]
        public string[] formatParams = new string[0];

        [HideInInspector]
        public RichText richText = null;

        [SerializeField]
        [HideInInspector]
        private string _languageKey = string.Empty;
		private int _fontSize = 0;

        void Start()
        {
            UpdateLocalizationText();
        }

        public void SetText(string languageKey)
        {
            CheckRichTextComponent();
            this._languageKey = languageKey;
            UpdateLocalizationText();
        }

		public void SetFontSize(int value)
		{
			CheckRichTextComponent();
			this._fontSize = value;
			UpdateLocalizationText();
		}

        public void CheckRichTextComponent()
        {
            if (null == richText)
            {
                var componentFind = GetComponent<RichText>();
                if (componentFind == null)
                {
                    richText = gameObject.AddComponent<RichText>();
                }
                else
                    richText = componentFind;
            }
        }

        private void UpdateLocalizationText()
        {
            CheckRichTextComponent();
            richText.text = shaco.Localization.GetTextFormat(_languageKey, formatParams);
			if (this._fontSize != 0)
			{
				richText.textModel.fontSize = this._fontSize;
			}
        }
    }
}

