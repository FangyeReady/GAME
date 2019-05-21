using UnityEngine;
using System.Collections;

namespace shaco
{
    public class Repeat : ActionS
    {
		public delegate void SS_CallFUNC_LOOP_COMPLETE(int loop);

		public SS_CallFUNC_LOOP_COMPLETE onLoopCompleteFunc = null;

        protected ActionS ActionTarget;
        protected int _iLoop = 1;
        protected int _iCurrentTimes = 1;
        static public Repeat Create(ActionS action, int loop)
        {
            Repeat ret = new Repeat();
            ret.ActionTarget = action;
            ret._iLoop = loop;

            return ret;
        }

        static public Repeat CreateRepeatForver(ActionS action)
        {
            Repeat ret = new Repeat();
            ret.ActionTarget = action;
            ret._iLoop = -1;

            return ret;
        }

        public override void RunAction(GameObject target)
        {
            base.RunAction(target);
            if (ActionTarget != null)
                ActionTarget.RunActionWithoutPlay(target);
        }

        public override float UpdateAction(float prePercent, float delayTime)
        {
			if (Elapsed >= Duration)
			{
				SetActionAlive(false);
				return base.UpdateAction(prePercent, delayTime);
			}

            SetActionAlive(true);
            prePercent = ActionTarget.GetCurrentPercent();
            prePercent = base.UpdateAction(prePercent, delayTime);

            if (ActionTarget != null)
            {
                prePercent = ActionTarget.GetCurrentPercent();
                ActionTarget.UpdateAction(prePercent, delayTime);

                if (!IsActionAlive(ActionTarget))
                {
                    ++_iCurrentTimes;
                    if (_iCurrentTimes <= _iLoop || _iLoop == -1)
                    {
						if (onLoopCompleteFunc != null)
							onLoopCompleteFunc(_iCurrentTimes);

                        ActionTarget.Reset(true);
                        ActionTarget.RunActionWithoutPlay(ActionTarget.Target);
						this.SetActionAlive(true);
                    }
                    else
                        SetActionAlive(false);
                }
            }
            return prePercent;
        }

        public override ActionS Clone()
        {
            return Repeat.Create(ActionTarget, _iLoop);
        }

		public override void PlayEndDirectly ()
		{
			base.PlayEndDirectly ();
			
			if (ActionTarget != null)
			{
				ActionTarget.PlayEndDirectly();
			}
		}

		public override void Reset (bool isAutoPlay)
		{
			base.Reset (isAutoPlay);

			_iCurrentTimes = 1;

			if (ActionTarget != null)
			{
				ActionTarget.Reset(isAutoPlay);
			}
		}

		public int GetCurrentLoop()
		{
			return _iCurrentTimes;
		}
		
        public void SetActionTarget(ActionS action)
        {
            ActionTarget = action;
        }

        public ActionS GetActionTarget()
        {
            return ActionTarget;
        }
    }
}