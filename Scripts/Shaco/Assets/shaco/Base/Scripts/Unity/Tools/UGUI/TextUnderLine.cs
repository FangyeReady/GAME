using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace shaco
{
    [RequireComponent(typeof(Text))]
    public class TextUnderLine : MonoBehaviour
    {
        //下划线偏移位置
        public Vector3 offset;

        //下划线颜色
        public Color color = Color.black;

        private Text _textTarget = null;
        private Text _textUnderLine = null;
        private string _prevTextValue = string.Empty;
        private Vector3 _prevOffset = Vector3.zero;
        private Color _prevColor = Color.black;

        void Reset()
        {
            color = this.GetComponent<Text>().color;
        }

        void Start()
        {
            UpdateUnderLine();
        }

        void Update()
        {
            if (null != _textTarget)
            {
                bool needUpdate = false;
                if (_prevTextValue.Length != _textTarget.text.Length && _prevTextValue != _textTarget.text)
                {
                    needUpdate = true;
                }

                if (_prevOffset != offset)
                {
                    needUpdate = true;
                }

                if (color != _prevColor)
                {
                    needUpdate = true;
                }

                if (needUpdate)
                {
                    UpdateUnderLine();
                }
            }
        }

        public void UpdateUnderLine()
        {
            _textTarget = GetComponent<Text>();
            _prevOffset = offset;
            _prevColor = color;
            CreateUnderLine(_textTarget);
            _textUnderLine.color = color;
        }

        private void CreateUnderLine(Text text)
        {
            if (text == null)
                return;

            //克隆Text，获得相同的属性 
            if (null == _textUnderLine)
            {
                _textUnderLine = Instantiate(text) as Text;
                _textUnderLine.name = "Underline";
                TextUnderLine duplicateComponent = _textUnderLine.GetComponent<TextUnderLine>();
                if (null != duplicateComponent)
                {
                    MonoBehaviour.Destroy(duplicateComponent);
                }

                shaco.UnityHelper.ChangeParentLocalPosition(_textUnderLine.gameObject, text.gameObject);
            }
            else
            {
                _textUnderLine.gameObject.SetActive(true);
            }

            RectTransform rt = _textUnderLine.rectTransform;

            //设置下划线坐标和位置  
            rt.anchoredPosition3D = Vector3.zero;
            rt.offsetMax = Vector2.zero;
            rt.offsetMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.anchorMin = Vector2.zero;

            rt.localPosition += offset;

            _prevTextValue = text.text;
            _textUnderLine.text = "_";
            float perlineWidth = _textUnderLine.preferredWidth;      //单个下划线宽度  

            float width = text.preferredWidth;
            int lineCount = (int)Mathf.Round(width / perlineWidth);
            for (int i = 1; i < lineCount; i++)
            {
                _textUnderLine.text += "_";
            }
        }
    }
}