using UnityEngine;
using System.Collections;

namespace shaco
{
    public class RotateTo : RotateBy
    {
        protected Vector3 _vec3EulerAngleSrc;

        static public new RotateTo Create(Vector3 endAngle, float duration)
        {
            RotateTo ret = new RotateTo();
            ret._vec3EulerAngleEnd = endAngle; 
            ret.Duration = duration;

            return ret;
        }

        public override void RunAction(GameObject target)
        {
            _vec3EulerAngleSrc = target.transform.eulerAngles;
            _vec3EulerAngle = _vec3EulerAngleEnd - target.transform.eulerAngles;

            base.RunAction(target);
        }

        public override ActionS Clone()
        {
            return RotateTo.Create(_vec3EulerAngleEnd, Duration);
        }

        
        public override ActionS Reverse()
        {
            shaco.Log.Error("RotateTo not have Reverse function");
            return null;
        }
    }
}
