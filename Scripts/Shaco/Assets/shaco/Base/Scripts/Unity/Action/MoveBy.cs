using UnityEngine;
using System.Collections;

namespace shaco
{
    public class MoveBy : ActionS
    {
        protected Vector3 _vec3Position;
        protected Vector3 _vec3PositionEnd;
        protected bool _isRelativeMove = true;
        protected bool _isWorldPosition = true;

        static public MoveBy Create(Vector3 offsetPosition, float duration, bool isWorldPosition = true)
        {
            MoveBy ret = new MoveBy();
            ret._vec3Position = offsetPosition;
            ret.Duration = duration;
            ret._isWorldPosition = isWorldPosition;

            return ret;
        }

        public override void RunAction(GameObject target)
        {
            base.RunAction(target);

            if (_isRelativeMove)
            {
                var vec3DirectionOffset = target.transform.TransformDirection(_vec3Position);
                _vec3PositionEnd = GetRealPosition() + vec3DirectionOffset;
            }
            else
            {
                _vec3PositionEnd = GetRealPosition() + _vec3Position;
            }

            this.onCompleteFunc += (shaco.ActionS action) =>
            {
                if (_vec3PositionEnd != GetRealPosition())
                {
                    SetRealPosition(_vec3PositionEnd);
                }
            };
        }

        public override float UpdateAction(float prePercent, float delayTime)
        {
            var moveOffset = prePercent * _vec3Position;
            if (_isRelativeMove && _isWorldPosition)
            {
                Target.transform.Translate(moveOffset);
            }
            else
            {
                SetRealPosition(GetRealPosition() + moveOffset);
            }

            return base.UpdateAction(prePercent, delayTime);
        }

        public override ActionS Clone()
        {
            return MoveBy.Create(_vec3Position, Duration);
        }

        public override ActionS Reverse()
        {
            return MoveBy.Create(-_vec3Position, Duration);
        }

        private Vector3 GetRealPosition()
        {
            return _isWorldPosition ? Target.transform.position : Target.transform.localPosition;
        }

        private void SetRealPosition(Vector3 realPosition)
        {
            if (_isWorldPosition)
            {
                Target.transform.position = realPosition;
            }
            else
            {
                Target.transform.localPosition = realPosition;
            }
        }
    }
}
