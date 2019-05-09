using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextExclueEmoj : MonoBehaviour 
{
	private UnityEngine.UI.InputField _inputTarget = null;
    private UnityEngine.UI.Text _textTarget = null;
	private string _prevText = string.Empty;

	void Start()
	{
        _inputTarget = GetComponent<UnityEngine.UI.InputField>();
        _textTarget = GetComponent<UnityEngine.UI.Text>();

		if (null != _textTarget)
		{
            _prevText = _textTarget.text;
        }
		else if (null != _inputTarget)
        {
            _prevText = _inputTarget.text;
        }
	}

	void Update()	
	{
		if (null != _textTarget)
		{
			if (_prevText != _textTarget.text)
			{
                //屏蔽emoji 
                string result = System.Text.RegularExpressions.Regex.Replace(_textTarget.text, @"\p{Cs}", "");
                _textTarget.text = result;
                _prevText = _textTarget.text;
			}
		}
        else if (null != _inputTarget)
        {
            if (_prevText != _inputTarget.text)
            {
                //屏蔽emoji 
                string result = System.Text.RegularExpressions.Regex.Replace(_inputTarget.text, @"\p{Cs}", "");
                _inputTarget.text = result;
                _prevText = _inputTarget.text;
            }
        }
	}
}
