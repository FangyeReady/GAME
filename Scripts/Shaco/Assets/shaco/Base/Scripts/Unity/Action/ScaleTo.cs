using UnityEngine;
using System.Collections;

namespace shaco
{
    public class ScaleTo : ScaleBy
    {  
        static public new ScaleTo Create(Vector3 endScale, float duration)
        {
            ScaleTo ret = new ScaleTo();
            ret._vec3ScaleEnd = endScale;
            ret.Duration = duration;

            return ret;
        }

        public override void RunAction(GameObject target)
        {
            _vec3Scale = _vec3ScaleEnd - target.transform.localScale;
            base.RunAction(target);
        }

        public override ActionS Clone()
        {
            return ScaleTo.Create(_vec3ScaleEnd, Duration);
        }

        public override ActionS Reverse()
        {
            shaco.Log.Error("ScaleTo not have Reverse function");
            return null;
        }
    }
}
