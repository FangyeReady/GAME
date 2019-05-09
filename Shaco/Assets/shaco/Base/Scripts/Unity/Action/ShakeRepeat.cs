using UnityEngine;
using System.Collections;

namespace shaco
{
    public class ShakeRepeat : ActionS
    {
        private Vector3 _shakeRepeatDistance = Vector3.zero;
        private int _loop = 1;
        private shaco.ActionS _actionTarget = null;

        static public ShakeRepeat Create(Vector3 shakeRepeatDistance, int loop, float duration)
        {
            ShakeRepeat ret = new ShakeRepeat();
            ret._shakeRepeatDistance = shakeRepeatDistance;
            ret._loop = loop;
            ret.Duration = duration;

            return ret;
        }

        static public ShakeRepeat CreateShakeForever(Vector3 shakeRepeatDistance, float duration)
        {
            return Create(shakeRepeatDistance, -1, duration);
        }

        public override void RunAction(GameObject target)
        {
            base.RunAction(target);

            var move1 = shaco.MoveTo.Create(target.transform.position + _shakeRepeatDistance, this.Duration / this._loop / 2);
            var move2 = shaco.MoveTo.Create(target.transform.position, this.Duration / this._loop / 2);
            var sequeue = shaco.Sequeue.Create(move1, move2);

            if (_loop == 1)
                _actionTarget = sequeue;
            else if (_loop > 1)
                _actionTarget = shaco.Repeat.Create(sequeue, _loop);
            else
            {
                _actionTarget = shaco.Repeat.CreateRepeatForver(sequeue);
            }

            _actionTarget.RunActionWithoutPlay(target);
        }

        public override float UpdateAction(float prePercent, float delayTime)
        {
            SetActionAlive(true);
            prePercent = base.UpdateAction(prePercent, delayTime);
            var newPercent = _actionTarget.UpdateAction(prePercent, delayTime);

            if (!IsActionAlive(_actionTarget))
            {
                SetActionAlive(false);
            }

            return newPercent;
        }

        public override ActionS Clone()
        {
            return ShakeRepeat.Create(_shakeRepeatDistance, _loop, Duration);
        }

        public override ActionS Reverse()
        {
            return ShakeRepeat.Create(-_shakeRepeatDistance, _loop, Duration);
        }

        public override void PlayEndDirectly()
        {
            base.PlayEndDirectly();
            if (_actionTarget != null)
                _actionTarget.PlayEndDirectly();
        }
    }
}

