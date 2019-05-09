/*
 * @Author: mikey.zhaopeng 
 * @Date: 2018-05-29 18:44:16 
 * @Last Modified by: mikey.zhaopeng
 * @Last Modified time: 2018-05-29 18:52:09
 */

#if DEBUG_WINDOW
namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 性能分析辅助器接口。
    /// </summary>
    public class ProfilerHelper : GameFramework.Utility.Profiler.IProfilerHelper
    {
        public ProfilerHelper(System.Threading.Thread thread)
        {

        }

        /// <summary>
        /// 开始采样。
        /// </summary>
        /// <param name="name">采样名称。</param>
        public void BeginSample(string name)
        {
#if UNITY_5_3_OR_NEWER
            UnityEngine.Profiling.Profiler.BeginSample(name);
#else
            UnityEngine.Profiler.BeginSample(name);
#endif
        }

        /// <summary>
        /// 结束采样。
        /// </summary>
        public void EndSample()
        {
#if UNITY_5_3_OR_NEWER
            UnityEngine.Profiling.Profiler.EndSample();
#else
            UnityEngine.Profiler.EndSample();
#endif
        }
    }
}
#endif