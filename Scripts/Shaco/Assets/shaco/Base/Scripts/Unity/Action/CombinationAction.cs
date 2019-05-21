using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public class CombinationAction : ActionS
    {
        protected List<ActionS> _listActions = new List<ActionS>();

        static public CombinationAction Create(float duration, params ActionS[] actions)
        {
            var ret = new CombinationAction();
            ret.Duration = duration;

            for (int i = 0; i < actions.Length; ++i)
            {
                ret._listActions.Add(actions[i]);
            }

            return ret;
        }

        public override void RunAction(GameObject target)
        {
            base.RunAction(target);

            if (_listActions.Count == 0)
            {
                ActionS.LogError("CombinationAction.RunAction erorr: not have any Action !");
                return;
            }

            for (int i = 0; i < _listActions.Count; ++i)
            {
                _listActions[i].RunAction(target);
            }
        }

        public override float UpdateAction(float prePercent, float delayTime)
        {
            var ret = base.UpdateAction(prePercent, delayTime);

            for (int i = 0; i < _listActions.Count; ++i)
            {
                _listActions[i].UpdateAction(prePercent, delayTime);
            }

            return ret;
        }

        public override ActionS Clone()
        {
            var ret = new CombinationAction();

            for (int i = 0; i < _listActions.Count; ++i)
            {
                ret._listActions.Add(_listActions[i]);
            }

            return ret;
        }

        public override ActionS Reverse()
        {
            var ret = new CombinationAction();

            for (int i = _listActions.Count - 1; i >= 0; --i)
            {
                ret._listActions.Add(_listActions[i]);
            }

            return ret;
        }

        public override void PlayEndDirectly()
        {
            base.PlayEndDirectly();

            for (int i = 0; i < _listActions.Count; ++i)
            {
                _listActions[i].PlayEndDirectly();
            }
        }

        public override void Reset(bool isAutoPlay)
        {
            base.Reset(isAutoPlay);

            for (int i = 0; i < _listActions.Count; ++i)
            {
                _listActions[i].Reset(isAutoPlay);
            }
        }
    }
}
