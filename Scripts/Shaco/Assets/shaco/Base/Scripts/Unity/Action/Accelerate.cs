using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public class Accelerate : ActionS
    {
        public class ControlPoint
        {
            public float TimePercent;
            public float SpeedRate;

            public ControlPoint(float timePercent = 0, float speedRate = 0)
            { TimePercent = timePercent; SpeedRate = speedRate; }

            public void checkSafeData()
            {
                if (TimePercent < 0)
                    TimePercent = 0;
                if (TimePercent > 1)
                    TimePercent = 1;
                if (SpeedRate <= 0)
                    SpeedRate = 1.0f;
            }
        }

        public enum AccelerateMode
        {
            StraightMode,
            ParabolaMode
        }
        
        protected float Speed;
        protected ActionS ActionTarget;
        protected AccelerateMode ActionMode;

        protected ControlPoint ControlBegin = new ControlPoint(0, 0);
        protected ControlPoint ControlMiddle = new ControlPoint(0, 0);
        protected ControlPoint ControlEnd = new ControlPoint(0, 0);

        static private List<GameObject> _listTestDraw = new List<GameObject>();

        static public Accelerate Create(ActionS action, ControlPoint begin, ControlPoint middle, ControlPoint end, AccelerateMode mode = AccelerateMode.ParabolaMode)
        {
            Accelerate ret = new Accelerate();

            begin.checkSafeData();
            middle.checkSafeData();
            end.checkSafeData();

            ret.ControlBegin = begin;
            ret.ControlMiddle = middle;
            ret.ControlEnd = end;
            ret.ActionTarget = action;
            ret.ActionMode = mode;

            return ret;
        }

        public override void RunAction(GameObject target)
        {
            base.RunAction(target);

            ActionTarget.RunActionWithoutPlay(target);

            foreach(var obj in _listTestDraw)
            {
                GameObject.Destroy(obj);
            }
            _listTestDraw.Clear();
        }

        public void SetActionTarget(ActionS action)
        {
            ActionTarget = action;
        }

        public override ActionS Clone()
        {
            return Create(ActionTarget, ControlBegin, ControlMiddle, ControlEnd, ActionMode);
        }

        override public float UpdateAction(float prePercent, float delayTime)
        {
			if (Elapsed >= Duration)
			{
				SetActionAlive(false);
				return base.UpdateAction(prePercent, delayTime);
			}

            SetActionAlive(true);

            float scaleRate = 1.0f;
            float timePercent = ActionTarget.Elapsed / ActionTarget.Duration;

            //run action by directly
            if (ActionTarget.Duration <= 0.0f)
            {
                SetActionAlive(false);
                var completedPercent = base.UpdateAction(1.0f, 0);
                completedPercent = ActionTarget.UpdateAction(1.0f, 0);
                return completedPercent;
            }

            if (AccelerateMode.StraightMode == ActionMode)
            {
                if (timePercent >= ControlBegin.TimePercent && timePercent <= ControlMiddle.TimePercent)
                {
                    scaleRate = MathS.GetYValueOfLineEquation
                        (ControlBegin.TimePercent, ControlBegin.SpeedRate,
                        ControlMiddle.TimePercent, ControlMiddle.SpeedRate, timePercent);
                }
                else if (timePercent >= ControlMiddle.TimePercent)
                {
                    scaleRate = MathS.GetYValueOfLineEquation
                        (ControlMiddle.TimePercent, ControlMiddle.SpeedRate,
                        ControlEnd.TimePercent, ControlEnd.SpeedRate, timePercent);
                }
            }
            else if (AccelerateMode.ParabolaMode == ActionMode)
            {
                scaleRate = MathS.GetYValueOfParabolaEquation(
                    ControlBegin.TimePercent, ControlBegin.SpeedRate,
                    ControlMiddle.TimePercent, ControlMiddle.SpeedRate,
                    ControlEnd.TimePercent, ControlEnd.SpeedRate,
                    timePercent);
            }

            //check action over
            if (ActionTarget.Elapsed >= ActionTarget.Duration)
            {
                SetActionAlive(false);
            }

            float newPercent = ActionTarget.GetCurrentPercent() * scaleRate;
            float newDelayTime = delayTime * scaleRate;
            newPercent = base.UpdateAction(newPercent, newDelayTime);
            newPercent = ActionTarget.UpdateAction(newPercent, newDelayTime);
            return newPercent;
        }

		public override void PlayEndDirectly ()
		{
			base.PlayEndDirectly ();
			
			if (ActionTarget != null)
				ActionTarget.PlayEndDirectly();
		}

        public override void Reset(bool isAutoPlay)
        {
            base.Reset(isAutoPlay);

            if (ActionTarget != null)
                ActionTarget.Reset(isAutoPlay);
        }

        public override ActionS Reverse()
        {
            Accelerate ret = new Accelerate();
            ActionTarget = ActionTarget.Reverse();
            ret.SetActionTarget(ActionTarget);
            return ret;  
        }
    }
}
