using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class UILayerAttribute : System.Attribute
    {
        public int layerIndex = 0;
        public bool allowedDuplicate = false;
        public string multiVersionControlRelativePath = string.Empty;

        public UILayerAttribute() {}

        public UILayerAttribute(int layerIndex = 0, bool allowedDuplicate = false, string multiVersionControlRelativePath = "")
        {
            this.layerIndex = layerIndex;
            this.allowedDuplicate = allowedDuplicate;
            this.multiVersionControlRelativePath = multiVersionControlRelativePath;
        }

		public UILayerAttribute(int layerIndex)
		{
			this.layerIndex = layerIndex;
		}

        public UILayerAttribute(bool allowedDuplicate)
        {
            this.allowedDuplicate = allowedDuplicate;
        }

        public UILayerAttribute(string multiVersionControlRelativePath)
        {
            this.multiVersionControlRelativePath = multiVersionControlRelativePath;
        }
    }
}