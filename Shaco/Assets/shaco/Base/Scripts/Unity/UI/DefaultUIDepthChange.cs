using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    /// <summary>
    /// 默认的UI显示深度变化方法，以UGUI为准
    /// </summary>
    public class DefaultUIDepthChange : IUIDepthChange
    {
        public void ChangeDepthAsTopDisplay(Transform target)
        {
            target.SetAsLastSibling();
        }
    }
}