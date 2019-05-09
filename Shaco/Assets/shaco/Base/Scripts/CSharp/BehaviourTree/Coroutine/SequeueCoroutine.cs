using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public partial class Coroutine
    {
        public class SequeueCallBack : shaco.Base.IBehaviourParam
        {
            public System.Action callbackInvoke = null;
            public System.Func<bool> callbackFinish = null;
            public System.Func<bool> callbackBreak = null;

            public SequeueCallBack(System.Action callbackInvoke, System.Func<bool> callbackFinish)
            {
                this.callbackInvoke = callbackInvoke;
                this.callbackFinish = callbackFinish;
            }
            public SequeueCallBack(System.Action callbackInvoke, System.Func<bool> callbackFinish, System.Func<bool> callbackBreak)
            {
                this.callbackInvoke = callbackInvoke;
                this.callbackFinish = callbackFinish;
                this.callbackBreak = callbackFinish;
            }
            public SequeueCallBack() { }
        }

        private class SequeueCoroutine : shaco.Base.IBehaviourProcess
        {
            public class Param : shaco.Base.IBehaviourParam
            {
                public int currentInvokeIndex = 0;
                public SequeueCallBack[] callbacks = null;
            }

            public IEnumerator<shaco.Base.IBehaviourEnumerator> Process(shaco.Base.BehaviourTree tree)
            {
                var param = tree.GetParameter<SequeueCoroutine.Param>();
                if (param.currentInvokeIndex < 0 || param.currentInvokeIndex > param.callbacks.Length - 1)
                {
                    tree.RemoveMe();
                    yield break;
                }

                do
                {
                    var currentCallBack = param.callbacks[param.currentInvokeIndex];
                    currentCallBack.callbackInvoke();
                    if (null != currentCallBack.callbackBreak && currentCallBack.callbackBreak())
                    {
                        tree.RemoveMe();
                        yield break;
                    }
                    if (currentCallBack.callbackFinish())
                    {
                        ++param.currentInvokeIndex;
                    }
                    yield return 1;

                } while (param.currentInvokeIndex < param.callbacks.Length);
                
                tree.RemoveMe();
            }
        }

        /// <summary>
        /// 执行队列事件
        /// </summary>
        /// <param name="callbacks">需要依次执行的事件方法</param>
        static public void Sequeue(params SequeueCallBack[] callbacks)
        {
            if (callbacks.IsNullOrEmpty())
            {
                Log.Error("SequeueCoroutine Sequeue error: no data");
                return;
            }
            Coroutine.RunCoroutineDefault<SequeueCoroutine>(new SequeueCoroutine.Param()
            {
                currentInvokeIndex = 0,
                callbacks = callbacks
            });
        }
    }
}