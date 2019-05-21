using UnityEngine;
using System.Collections;

namespace shaco
{
    public class MoveTo : MoveBy
    {
        static public new MoveTo Create(Vector3 endPosition, float duration, bool isWorldPosition = true)
        {
            MoveTo ret = new MoveTo();
            ret._vec3PositionEnd = endPosition;
            ret.Duration = duration;
            ret._isRelativeMove = false;
            ret._isWorldPosition = isWorldPosition;

            return ret;
        }

        public override void RunAction(GameObject target)
        {
            _vec3Position = _vec3PositionEnd - target.transform.position;

            base.RunAction(target);
        }

        public override ActionS Clone()
        {
            return MoveTo.Create(_vec3PositionEnd, Duration);
        }

        public override ActionS Reverse()
        {
            shaco.Log.Error("MoveTo not have Reverse function");
            return null;
        }
    }
}
