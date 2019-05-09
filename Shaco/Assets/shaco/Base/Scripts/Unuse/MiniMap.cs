using UnityEngine;
using System.Collections;

namespace shaco
{
    [RequireComponent(typeof(Camera))]
    public class MiniMap : MonoBehaviour
    {
        [Range(0, 1)]
        public float XOffset = 0;
        [Range(0, 1)]
        public float YOffset = 0;
        [Range(0, 1)]
        public float Width = 1;
        [Range(0, 1)]
        public float Height = 1;

        private float _prevXOffset = 0;
        private float _prevYOffset = 0;
        private float _prevWidth = 0;
        private float _prevHeight = 0;

        void Awake()
        {
        }

        void Reset()
        {
        }

        void OnValidate()
        {
            if (_prevXOffset != this.XOffset 
                || _prevYOffset != this.YOffset
                || _prevWidth != this.Width
                || _prevHeight != this.Height)
            {
                _prevXOffset = this.XOffset;
                _prevYOffset = this.YOffset;
                _prevWidth = this.Width;
                _prevHeight = this.Height;

                updateCameraContentSize();
            }
        }

        private void updateCameraContentSize()
        {
            var cameraTmp = this.GetComponent<Camera>();

			cameraTmp.rect = new Rect(XOffset, YOffset, Width, Height);
        }
    }
}