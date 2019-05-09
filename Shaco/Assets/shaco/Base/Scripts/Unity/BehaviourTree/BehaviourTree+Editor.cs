using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
namespace shaco.Base
{
    public partial class BehaviourTree
    {
        public Rect editorDrawPosition = new Rect();
        public TextAsset editorAssetProcess = null;
        public string editorAssetPathProcess = string.Empty;
        public bool editorHasInValidParam = false;

        public void CopyEditorDataFrom(BehaviourTree other)
        {
            this.name = other.name;
            this.editorDrawPosition = other.editorDrawPosition;
            this.editorAssetProcess = other.editorAssetProcess;
            this.editorAssetPathProcess = other.editorAssetPathProcess;
            this.editorHasInValidParam = other.editorHasInValidParam;
        }
    }
}
#endif

