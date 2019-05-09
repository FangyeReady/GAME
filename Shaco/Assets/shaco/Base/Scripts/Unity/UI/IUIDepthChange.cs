using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    /// <summary>
    /// UI的显示深度变化
    /// </summary>
    public interface IUIDepthChange
    {
        /// <summary>
        /// 改变UI的显示深度，并防止到最上层
        /// </summary>
        void ChangeDepthAsTopDisplay(Transform target);
    }
}