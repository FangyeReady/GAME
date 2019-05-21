using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public partial class Coroutine
    {
        private class ForeachCoroutine : shaco.Base.IBehaviourProcess
        {
            public class Param : shaco.Base.IBehaviourParam
            {
                public ICollection collections = null;
                public System.Func<bool> callbackLoop = null;
                public System.Func<object, bool> callbackData = null;
                public System.Action<float> callbackProgress = null;
                public int count = 0;
                public float speed = 1;
            }

            public IEnumerator<shaco.Base.IBehaviourEnumerator> Process(shaco.Base.BehaviourTree tree)
            {
                var foreachParamTmp = tree.GetParameter<Param>();
                int indexTmp = 0;
                int totalCount = null == foreachParamTmp.collections ? foreachParamTmp.count : foreachParamTmp.collections.Count;
                int perAddCount = (int)(totalCount * foreachParamTmp.speed);
                if (perAddCount <= 0)
                {
                    perAddCount = 1;
                }

                if (null == foreachParamTmp.collections)
                {
                    totalCount = foreachParamTmp.count;
                    for (int i = 0; i < foreachParamTmp.count; ++i)
                    {
                        bool onceLoopEnd = false;
                        indexTmp = OnceForeach(foreachParamTmp, indexTmp, perAddCount, totalCount, null, ref onceLoopEnd);
                        if (onceLoopEnd)
                        {
                            yield return 1;
                        }
                        if (indexTmp < 0)
                        {
                            break;
                        }  
                    }
                }
                else 
                {
                    totalCount = foreachParamTmp.collections.Count;
                    foreach (var data in foreachParamTmp.collections)
                    {
                        bool onceLoopEnd = false;
                        indexTmp = OnceForeach(foreachParamTmp, indexTmp, perAddCount, totalCount, data, ref onceLoopEnd);
                        if (onceLoopEnd)
                        {
                            yield return 1;
                        }
                        if (indexTmp < 0)
                        {
                            break;
                        }
                    }
                }

                if (null != foreachParamTmp.callbackProgress && indexTmp % perAddCount != 0)
                {
                    foreachParamTmp.callbackProgress(1);
                }
                tree.RemoveMe();
            }

            static private int OnceForeach(Param param, int index, int perAddCount, int totalCount, object data, ref bool onceLoopEnd)
            {
                //用户主动停止了遍历
                if ((null != param.callbackData && !param.callbackData(data))
                || (null != param.callbackLoop && !param.callbackLoop()))
                {
                    if (null != param.callbackProgress)
                    {
                        param.callbackProgress(1);
                    }
                    return -1;
                }
                ++index;

                //一组遍历结束，即将等待下一帧，然后开始下一组遍历
                if (index % perAddCount == 0)
                {
                    if (null != param.callbackProgress)
                    {
                        param.callbackProgress((float)index / (float)totalCount);
                    }
                    onceLoopEnd = true;
                }
                return index;
            }
        }

        /// <summary>
        /// 遍历集合
        /// </summary>
        /// <param name="collections">需要遍历的集合对象</param>
        /// <param name="callbackData">遍历的回调数据，如果有返回false则停止循环</param>
        /// <param name="callbackProgress">遍历的进度，范围(0 ~ 1)</param>
        /// <param name="speed">遍历速度，数值越大线程越卡顿，遍历速度越快，反之线程不卡，遍历速度变慢</param>
        static public void Foreach(ICollection collections, System.Func<object, bool> callbackData, System.Action<float> callbackProgress = null, float speed = 0.05f)
        {
            if (null == collections || callbackData == null)
            {
                Log.Error("Coroutine Foreach error: invalid params");
                return;
            }

            RunCoroutineDefault<ForeachCoroutine>(new ForeachCoroutine.Param()
            {
                collections = collections,
                callbackData = callbackData,
                callbackProgress = callbackProgress,
                speed = speed,
            });
        }

        /// <summary>
        /// 遍历循环次数
        /// </summary>
        /// <param name="count">需要循环的次数</param>
        /// <param name="callbackLoop">遍历的回调数据，如果有返回false则停止循环</param>
        /// <param name="callbackProgress">遍历的进度，范围(0 ~ 1)</param>
        /// <param name="speed">遍历速度，数值越大线程越卡顿，遍历速度越快，反之线程不卡，遍历速度变慢</param>
        static public void ForeachCount(int count, System.Func<bool> callbackLoop, System.Action<float> callbackProgress = null, float speed = 0.05f)
        {
            RunCoroutineDefault<ForeachCoroutine>(new ForeachCoroutine.Param()
            {
                count = count,
                callbackLoop = callbackLoop,
                callbackProgress = callbackProgress,
                speed = speed,
            });
        }

        static public void RunCoroutineDefault<T>(params shaco.Base.IBehaviourParam[] param) where T : shaco.Base.IBehaviourProcess, new()
        {
            var rootTreeTmp = new shaco.Base.BehaviourRootTree();
            var treeTmp = new shaco.Base.BehaviourDefaultTree();
            treeTmp.BindProcess<T>();
            rootTreeTmp.AddChild(treeTmp);

            for (int i = 0; i < param.Length; ++i)
            {
                rootTreeTmp.SetParameter(param[i]);
            }
            rootTreeTmp.Start();
        }
    }
}