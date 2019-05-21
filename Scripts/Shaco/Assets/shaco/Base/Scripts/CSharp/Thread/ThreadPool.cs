using System.Collections;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// 线程池管理类
/// </summary>
namespace shaco.Base
{
    public class ThreadPool
    {
        /// <summary>
        /// 设置线程池队列中最大的线程数量，超过该数量的新增线程会持续等待，直到有旧线程退出腾出新的坑位
        /// <param name=""> </param>
        /// <return></return>
        /// </summary>
        static public void SetMaxThreads(int count)
        {
            System.Threading.ThreadPool.SetMaxThreads(count, count);
        }

        /// <summary>
        /// 运行线程
        /// <param name="callbackInvoke">线程内执行方法</param>
        /// </summary>
        static public void RunThread(ThreadStart callbackInvoke)
        {
            System.Threading.ThreadPool.QueueUserWorkItem((object value) =>
            {
                callbackInvoke();
            });
        }

        /// <summary>
        /// 运行带安全回调的线程
        /// <param name="callbackInvoke">子线程调用，执行任务方法，不允许调用Unity相关函数</param>
        /// <param name="callbackEnd">主线程调用，任务执行完毕回调，可以安全使用Unity相关函数</param>
        /// </summary>
        static public void RunThreadSafeCallBack(System.Action callbackInvoke, System.Action callbackEnd)
        {
            bool isCompleted = false;

            shaco.Base.WaitFor.Run(() =>
            {
                return isCompleted;
            }, () =>
            {
                if (null != callbackEnd)
                {
                    callbackEnd();
                }
            });

            RunThread(() =>
            {
                callbackInvoke();
                isCompleted = true;
            });
        }
    }
}