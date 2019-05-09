using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public partial class Coroutine
    {
        private class WhileCoroutine : shaco.Base.IBehaviourProcess
        {
            public class Param : shaco.Base.IBehaviourParam
            {
                public int maxLoopInPerFrame = 0;
                public System.Action callbackInvoke = null;
                public System.Func<bool> callbackFinish = null;
            }

            public IEnumerator<shaco.Base.IBehaviourEnumerator> Process(shaco.Base.BehaviourTree tree)
            {
                var param = tree.GetParameter<WhileCoroutine.Param>();
                bool shouldBreak = false;

                while (!shouldBreak)
                {
                    for (int i = 0; i < param.maxLoopInPerFrame; ++i)
                    {
                        if (!param.callbackFinish())
                        {
                            shouldBreak = true;
                            break;
                        }
                        else 
                        {
                            param.callbackInvoke();
                        }
                    }
                    yield return 1;
                };

                tree.RemoveMe();
            }
        }

        /// <summary>
        /// 执行循环事件
        /// </summary>
        /// <param name="callbackFinish">判断持续循环放昂发，如果有返回false则停止循环</param>
        /// <param name="callbackInvoke">执行方法</param>
        /// <param name="maxLoopInPerFrame">每一帧的循环最大执行的事件次数，次数越大每帧消耗cpu时间越高</param>
        static public void While(System.Func<bool> callbackFinish, System.Action callbackInvoke, int maxLoopInPerFrame = 3)
        {
           Coroutine.RunCoroutineDefault<WhileCoroutine>(new WhileCoroutine.Param()
           {
               maxLoopInPerFrame = maxLoopInPerFrame,
               callbackInvoke = callbackInvoke,
               callbackFinish = callbackFinish
           });
        }
    }
}