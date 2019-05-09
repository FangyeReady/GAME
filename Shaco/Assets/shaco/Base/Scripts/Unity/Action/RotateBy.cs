using UnityEngine;
using System.Collections;

namespace shaco
{
    public class RotateBy : ActionS
    {
        protected Vector3 _vec3EulerAngle = Vector3.zero;
        protected Vector3 _vec3EulerAngleEnd = Vector3.zero;
        protected Vector3 _vec3Current = Vector3.zero;

        static public RotateBy Create(Vector3 angle, float duration)
        {
            if (RotateBy.isZero(angle))
            {
                angle.x = 1; angle.y = 1; angle.z = 1;
            }
            RotateBy ret = new RotateBy();
            ret._vec3EulerAngle = angle;
            ret.Duration = duration;

            return ret;
        }

        public override void RunAction(GameObject target)
        {
            base.RunAction(target);

            _vec3Current = target.transform.eulerAngles;

            _vec3EulerAngleEnd = target.transform.eulerAngles + _vec3EulerAngle;

            this.onCompleteFunc += (shaco.ActionS action) =>
            {
                if (_vec3EulerAngleEnd != Target.transform.eulerAngles)
                {
                    Target.transform.eulerAngles = _vec3EulerAngleEnd;
                }
            };
        }

        public override float UpdateAction(float prePercent, float delayTime)
        {
            _vec3Current += _vec3EulerAngle * prePercent;
            _vec3Current.x %= 360;
            _vec3Current.y %= 360;
            _vec3Current.z %= 360;

            Target.transform.rotation = Quaternion.Euler(_vec3Current);

            return base.UpdateAction(prePercent, delayTime);
        }

        public override ActionS Clone()
        {
            return RotateBy.Create(_vec3EulerAngle, Duration);
        }

        public override ActionS Reverse()
        {
            return RotateBy.Create(-_vec3EulerAngle, Duration);
        }

        static bool isZero(Vector3 value)
        {
            return value.x == 0 && value.y == 0 && value.z == 0;
        }
    }
}
