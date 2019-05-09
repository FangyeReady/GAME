using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class WaitFor
    {
        private class InvokeInfo
        {
            public System.Func<bool> callbackIn = null;
            public System.Action callbackOut = null;

        }
        static private List<InvokeInfo> _invokes = new List<InvokeInfo>();

        /// <summary>
        /// 计时器刷新函数，只有在非Unity引擎中调用
        /// <param name="delayTime">当前帧经过的时间(单位：秒)</param>
        /// </summary>
        static public void Update(float delayTime)
        {
            for (int i = _invokes.Count - 1; i >= 0; --i)
            {
                if (_invokes[i].callbackIn())
                {
                    _invokes[i].callbackOut();
                    _invokes.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 开启一个条件计时器
        /// <param name="callbackIn">条件判断方法</param>
        /// <param name="callbackOut">条件执行方法，条件判断方法返回true的时候执行</param>
        /// </summary>
        static public void Run(System.Func<bool> callbackIn, System.Action callbackOut)
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_PS4 || UNITY_XBOXONE || UNITY_WSA || UNITY_WEBGL
            //如果是unity引擎，会自动有主线程来调用waitfor的计时器
            shaco.WaitFor.Run(callbackIn, callbackOut);
#else
            //如果非unity引擎，需要手动调用shaco.Base.WaitFor.Update来执行计时器
            _invokes.Add(new InvokeInfo() { callbackIn = callbackIn, callbackOut = callbackOut });
#endif
        }
    }
}
