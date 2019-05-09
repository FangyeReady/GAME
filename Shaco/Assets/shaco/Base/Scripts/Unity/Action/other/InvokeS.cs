using UnityEngine;
using System.Collections;
using System;

namespace shaco
{
    public class InvokeS
    {
        static public ActionS Run(Action func, float delaySeconds, GameObject bindTarget = null)
        {
            ActionS ret = null;

            var actionTmp = shaco.DelayTime.Create(delaySeconds);
            actionTmp.onCompleteFunc += (shaco.ActionS ac) => { func(); };
            actionTmp.RunAction(bindTarget == null ? ActionS.GetDelegateInvoke() : bindTarget);

            ret = actionTmp;

            return ret;
        }

        //func: loopCount, isNextLoop
        static public ActionS Run(Action<int, bool> func, float delaySeconds, float intervalSeconds, int loop, GameObject bindTarget = null)
        {
            ActionS ret = null;
            bool isNextLoop = false;

            var actionTmp = shaco.DelayTime.Create(delaySeconds);

            var repeatTmp = Repeat.Create(actionTmp, loop + 1);
            repeatTmp.onLoopCompleteFunc = (int loopCount) =>
            {
                if (repeatTmp.GetCurrentLoop() > 2)
                    isNextLoop = true;

                if (repeatTmp.GetCurrentLoop() > 1)
                {
                    actionTmp.Duration = intervalSeconds;
                }
            };

            repeatTmp.onFrameFunc = (float per) =>
            {

                int loopTmp = repeatTmp.GetCurrentLoop() - 1;
                if (loopTmp <= 0)
                {
                    func(1, false);
                }
                else
                {
                    func(loopTmp, isNextLoop);
                }
                if (isNextLoop)
                    isNextLoop = false;
            };

            ret = repeatTmp;
            repeatTmp.RunAction(bindTarget == null ? ActionS.GetDelegateInvoke() : bindTarget);

            return ret;
        }

        static public void CancelAllInvoke()
        {
            var delegateTmp = ActionS.GetDelegateInvoke();
            if (delegateTmp != null)
                ActionS.StopActions(delegateTmp);
        }
    }

    public class WaitFor
    {
        static public ActionS Run(System.Func<bool> callbackIn, System.Action callbackOut, GameObject bindTarget = null)
        {
            var repeat = Repeat.CreateRepeatForver(DelayTime.Create(10.0f));
            repeat.onFrameFunc += (float percent) =>
            {
                if (!repeat.isRemoved && callbackIn())
                {
                    repeat.StopMe();
                    callbackOut();
                }
            };
            repeat.RunAction(bindTarget == null ? ActionS.GetDelegateInvoke() : bindTarget);

            return repeat;
        }
    }
}
