using UnityEngine;
using System.Collections;

namespace shaco
{
    public class ScaleBy : ActionS
    {
        protected Vector3 _vec3Scale;
        protected Vector3 _vec3ScaleEnd;
        static public ScaleBy Create(Vector3 scale, float duration)
        {
            ScaleBy ret = new ScaleBy();
            ret._vec3Scale = scale;
            ret.Duration = duration;

            return ret;
        }
        public override void RunAction(GameObject target)
        {
            base.RunAction(target);

            _vec3ScaleEnd = target.transform.localScale + _vec3Scale;

            this.onCompleteFunc += (shaco.ActionS action) =>
            {
                if (_vec3ScaleEnd != Target.transform.localScale)
                {
                    Target.transform.localScale = _vec3ScaleEnd;
                }
            };
        }

        public override float UpdateAction(float prePercent, float delayTime)
        {
            Target.transform.localScale += _vec3Scale * prePercent;

            return base.UpdateAction(prePercent, delayTime); ;
        }

        public override ActionS Clone()
        {
            return ScaleBy.Create(_vec3Scale, Duration);
        }

        public override ActionS Reverse()
        {
            return ScaleBy.Create(-_vec3Scale, Duration);
        }
    }
}
