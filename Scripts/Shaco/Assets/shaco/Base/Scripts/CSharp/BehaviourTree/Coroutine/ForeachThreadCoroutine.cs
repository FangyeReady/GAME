using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace shaco.Base
{
    public partial class Coroutine
    {
        private class ForeachThreadCoroutine
        {
            public ICollection collections = null;
            public System.Func<bool> callbackLoop = null;
            public System.Func<object, bool> callbackData = null;
            public System.Action<float> callbackProgress = null;
            public int count = 0;
            public object lockKey = new System.Object();
            public bool shouldQuitThread = false;

            public void Run()
            {
                lock (lockKey)
                {
                    int indexTmp = 0;
                    int totalCount = null == this.collections ? this.count : this.collections.Count;

                    if (totalCount <= 0)
                    {
                        if (null != callbackProgress) callbackProgress(1);
                    }
                    else
                    {
                        if (null == this.collections)
                        {
                            totalCount = this.count;
                            for (int i = 0; i < this.count; ++i)
                            {
                                indexTmp = OnceForeach(indexTmp, totalCount, null);
                                if (indexTmp < 0)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            totalCount = this.collections.Count;
                            foreach (var data in this.collections)
                            {
                                indexTmp = OnceForeach(indexTmp, totalCount, data);
                                if (indexTmp < 0)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            private int OnceForeach(int index, int totalCount, object data)
            {
                if (shouldQuitThread)
                    return -1;

                try
                {
                    //用户主动停止了遍历
                    if ((null != this.callbackData && !this.callbackData(data))
                    || (null != this.callbackLoop && !this.callbackLoop()))
                    {
                        if (null != this.callbackProgress)
                        {
                            this.callbackProgress(1);
                            Thread.CurrentThread.Abort();
                        }
                        return -1;
                    }
                    ++index;

                    if (null != this.callbackProgress)
                    {
                        this.callbackProgress((float)index / (float)totalCount);
                    }
                }
                catch (System.Exception e)
                {
                    Log.Exception("ForeachThreadCoroutine erorr: e=" + e);
                }
                return index;
            }
        }

        /// <summary>
        /// 遍历集合
        /// </summary>
        /// <param name="collections">需要遍历的集合对象</param>
        /// <param name="callbackData">遍历的回调数据，如果有返回false则停止循环</param>非线程安全，如果需要在外部访问集合数据，需要使用该函数返回的object对象进行lock处理
        /// <param name="callbackProgress">遍历的进度，范围(0 ~ 1)</param>线程安全
        /// <returns>返回正在使用的线程锁对象</returns>
        static public object ForeachAsync(ICollection collections, System.Func<object, bool> callbackData, System.Action<float> callbackProgress)
        {
            var coroutine = new ForeachThreadCoroutine();
            coroutine.collections = collections;
            coroutine.callbackData = callbackData;
            coroutine.callbackProgress = JoinInMainThread(coroutine, callbackProgress);
            shaco.Base.ThreadPool.RunThread(coroutine.Run);
            return coroutine.lockKey;
        }

        /// <summary>
        /// 遍历循环次数
        /// </summary>
        /// <param name="count">需要循环的次数</param>
        /// <param name="callbackLoop">遍历的回调数据，如果有返回false则停止循环</param>非线程安全，如果想要在外部访问集合数据，需要使用该函数返回的object对象进行lock处理
        /// <param name="callbackProgress">遍历的进度，范围(0 ~ 1)</param>线程安全
        /// <returns>返回正在使用的线程锁对象</returns>
        static public object ForeachCountAsync(int count, System.Func<bool> callbackLoop, System.Action<float> callbackProgress)
        {
            var coroutine = new ForeachThreadCoroutine();
            coroutine.count = count;
            coroutine.callbackLoop = callbackLoop;
            coroutine.callbackProgress = JoinInMainThread(coroutine, callbackProgress);
            shaco.Base.ThreadPool.RunThread(coroutine.Run);
            return coroutine.lockKey;
        }

        static private System.Action<float> JoinInMainThread(ForeachThreadCoroutine coroutine, System.Action<float> callbackProgress)
        {
            float progressTmp = 0;
            System.Action<float> progressCallBackTmp = (float progress) =>
            {
                progressTmp = progress;
            };

            shaco.Base.WaitFor.Run(() =>
            {
                if (progressTmp < 1.0f)
                {
                    callbackProgress(progressTmp);
                }
                return progressTmp >= 1.0f;
            },
            () =>
            {
                coroutine.shouldQuitThread = true;
                callbackProgress(1.0f);
            });
            return progressCallBackTmp;
        }
    }
}