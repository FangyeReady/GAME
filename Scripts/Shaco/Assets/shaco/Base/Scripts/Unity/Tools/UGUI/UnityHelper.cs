using UnityEngine;
using System.Collections;

namespace shaco
{
    public partial class UnityHelper
    {
        /// <summary>
        /// add 'BoxCollider' Componet when UGUI by automatic
        /// </summary>
        /// <param name="target"></param>
        static public void AddColliderWithUGUI(GameObject target)
        {
            var colider2D = target.GetComponent<Collider2D>();

            if (target.GetComponent<Collider>() == null && colider2D == null)
            {
                var rectTrans = target.GetComponent<RectTransform>();
                if (rectTrans)
                {
                    var boxCollider = target.AddComponent<BoxCollider>();
                    boxCollider.size = new Vector2(rectTrans.rect.width, rectTrans.rect.height);
                }
                else
                {
                    target.AddComponent<BoxCollider>();
                    Log.Warning("missing Collider, AddCompoment the best yourself");
                }
            }
        }

#if UNITY_5_3_OR_NEWER
        /// <summary>
        /// set raycast target status of target and all its children, if not active in scene, will ignore
        /// </summary>
        static public void SetAllRaycastTarget(GameObject target, bool isRaycastTarget)
        {
            if (!target.activeInHierarchy)
            {
                return;
            }

            var graphicTmp = target.GetComponent<UnityEngine.UI.Graphic>();
            if (null != graphicTmp)
            {
                graphicTmp.raycastTarget = isRaycastTarget;
            }

            ForeachChildren(target, (int index, GameObject child) =>
            {
                graphicTmp = child.GetComponent<UnityEngine.UI.Graphic>();
                if (null != graphicTmp)
                {
                    graphicTmp.raycastTarget = isRaycastTarget;
                }
                return true;
            });
        }
#endif
    }
}