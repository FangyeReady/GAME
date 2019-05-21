using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public class Sequeue : ActionS
    {
        protected List<ActionS> _listActions = new List<ActionS>();
        protected int _iActionIndex = 0;

        static public Sequeue Create(params ActionS[] actions)
        {
            Sequeue ret = new Sequeue();
            for (int i = 0; i < actions.Length; ++i)
            {
                if (null != actions[i])
                    ret._listActions.Add(actions[i]);
            }

            return ret;
        }

        static public Sequeue Create(List<ActionS> actions)
        {
            Sequeue ret = new Sequeue();
            for (int i = 0; i < actions.Count; ++i)
            {
                if (null != actions[i])
                    ret._listActions.Add(actions[i]);
            }

            return ret;
        }

        public override void RunAction(GameObject target)
        {
            base.RunAction(target);
            _listActions[_iActionIndex].RunActionWithoutPlay(target);
        }

        public override float UpdateAction(float prePercent, float delayTime)
        {
            SetActionAlive(true);

            var currentAction = _listActions[_iActionIndex];
            prePercent = base.UpdateAction(prePercent, delayTime);

            if (!IsActionAlive(currentAction))
            {
                if (++_iActionIndex >= _listActions.Count)
                {
                    SetActionAlive(false);
                }
                else
                {
                    currentAction = _listActions[_iActionIndex];
                    currentAction.RunActionWithoutPlay(Target);
                }
            }

            if (Elapsed >= Duration)
            {
                SetActionAlive(false);
                return base.UpdateAction(prePercent, delayTime);
            }

            float newPercent = currentAction.GetCurrentPercent();
            currentAction.UpdateAction(newPercent, delayTime);
            return newPercent;
        }

        public override ActionS Clone()
        {
            ActionS[] actions = new ActionS[_listActions.Count];
            for (int i = 0; i < _listActions.Count; ++i)
            {
                actions[i] = _listActions[i];
            }
            return Sequeue.Create(actions);
        }

        public override ActionS Reverse()
        {
            ActionS[] actions = new ActionS[_listActions.Count];
            for (int i = _listActions.Count - 1; i >= 0; --i)
            {
                actions[i] = _listActions[i];
            }
            return Sequeue.Create(actions);
        }

        public override void Reset(bool isAutoPlay)
        {
            base.Reset(isAutoPlay);
            _iActionIndex = 0;

            foreach (var action in _listActions)
            {
                action.Reset(isAutoPlay);
            }
        }

		public override void PlayEndDirectly ()
		{
			base.PlayEndDirectly ();

            for (int i = _iActionIndex; i < _listActions.Count; ++i)
			{
				var ActionTarget = _listActions[i];
				if (ActionTarget != null)
				{
                    if (!ActionTarget.isPlaying)
                        ActionTarget.RunActionWithoutPlay(this.Target);

                    ActionTarget.PlayEndDirectly();
				}
			}
            _iActionIndex = _listActions.Count - 1;
		}
    }
}
