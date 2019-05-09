using UnityEngine;
using System.Collections;

namespace shaco
{
    public class AnimationAutoDestroyComponent : MonoBehaviour
    {
        void Start()
        {
            if (null != GetComponent<Animator>())
            {
                var animationComponentTmp = GetComponent<Animator>();
                shaco.UnityHelper.CallOnAnimationEnd(animationComponentTmp, () =>
                {
                    MonoBehaviour.Destroy(this.gameObject);
                });
            }
            else if (null != GetComponent<Animator>())
            {
                var animationComponentTmp = GetComponent<Animation>();
                shaco.UnityHelper.CallOnAnimationEnd(animationComponentTmp, () =>
                {
                    MonoBehaviour.Destroy(this.gameObject);
                });
            }
            else
            {
                shaco.Log.Error("AnimationAutoDestroyComponent error: not support type");
            }
        }
    }
}

