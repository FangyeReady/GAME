using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class TransparentTo : TransparentBy
    {
        static public new TransparentTo Create(float endAlpha, float duration)
        {
            TransparentTo ret = new TransparentTo();
            ret._alphaEnd = endAlpha; 
            ret.Duration = duration;

            return ret;
        }

        public override void RunAction(GameObject target)
        {
            _alpha = _alphaEnd - GetCurrentAlpha(target);
            base.RunAction(target);
        }

        public override ActionS Clone()
        {
            return TransparentTo.Create(_alphaEnd, Duration);
        }

        public override ActionS Reverse()
        {
            shaco.Log.Error("TransparentTo not have Reverse function");
            return null;
        }
    }
}
