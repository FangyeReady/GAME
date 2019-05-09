using UnityEngine;
using System.Collections;

namespace shaco
{
    public class TextOrigin : Object
    {
        public string text
        {
            get { return _text; }
            set { _text = value; }
        }

        public byte[] bytes
        {
            get { return _bytes.IsNullOrEmpty() ? _text.ToByteArray() : _bytes; }
            set { _bytes = value; }
        }

        public override string ToString()
        {
            return text;
        }

        public bool success 
        {
            get { return !_bytes.IsNullOrEmpty() || !string.IsNullOrEmpty(_text); }
        }

        private byte[] _bytes = new byte[0];
        private string _text = string.Empty;
    }
}