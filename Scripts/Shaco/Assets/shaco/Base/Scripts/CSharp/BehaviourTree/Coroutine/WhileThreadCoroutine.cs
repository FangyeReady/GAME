using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace shaco.Base
{
    public partial class Coroutine
    {
        private class WhileThreadCoroutine
        {
            public Thread thread = null;
            public System.Func<bool> callbackFinish = null;
            public System.Action callbackInvoke = null;
			public object lockKey = new System.Object();

			public void Run()
			{
                lock (lockKey)
				{
                    while (true)
                    {
                        if (!this.callbackFinish())
                        {
                            break;
                        }
                        else
                        {
                            this.callbackInvoke();
                        }
                    };
				}
			}
        }

        /// <summary>
        /// 持续while循环，非线程安全，如果想要在外部访问，必须lock返回的对象
        /// </summary>
        /// <param name="callbackFinish">判断持续循环放昂发，如果有返回false则停止循环</param>
        /// <param name="callbackInvoke">执行方法</param>
        /// <returns>返回正在使用的线程锁对象</returns>
        static public object WhileAsync(System.Func<bool> callbackFinish, System.Action callbackInvoke)
        {
            var coroutine = new WhileThreadCoroutine();
			coroutine.callbackFinish = callbackFinish;
			coroutine.callbackInvoke = callbackInvoke;
            ThreadPool.RunThread(coroutine.Run);
			return coroutine.lockKey;
        }
    }
}