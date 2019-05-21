using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class SubjectManager
    {
        public class SubjectLocation
        {
            public Dictionary<IObserverBase, ObserverLocation> observserLocations = new Dictionary<IObserverBase, ObserverLocation>();
        }

        public class ObserverLocation
        {
            /// <summary>
            /// 初始化观察对象与数据绑定时候的堆栈信息
            /// </summary>
            public StackLocation stackLocationObserverInit = new StackLocation();

            /// <summary>
            /// 观测数据被改变时候的堆栈信息
            /// </summary>
            public StackLocation stackLocationValueChange = new StackLocation();

            /// <summary>
            /// 观察者的初始回调方法
            /// </summary>
            public System.Delegate callbackInitDelegate = null;

			/// <summary>
			/// 观察者的数据刷新回调方法
			/// </summary>
			public System.Delegate callbackUpdateDelegate = null;
        }

        /// <summary>
        /// 默认数据主体绑定对象
        /// </summary>
        public const string DEFAULT_NULL_KEY = "default(null)";

        /// <summary>
        /// 堆栈信息中需要查找的类名
        /// </summary>
        static private readonly string[] _classNames = new string[]
        {
            "shaco/Base/Scripts/CSharp/Observer/ObserverExtension.cs",
            "shaco/Base/Scripts/Unity/Observer/ObserverExtension.cs",
            "shaco/Base/Scripts/CSharp/Observer/Subject.cs",
            "shaco/Base/Scripts/CSharp/Observer/SubjectManager.cs",
        };

        /// <summary>
        /// 所有正在使用的数据主体，[数据主体绑定对象，[数据主体，观测者堆栈信息]]]
        /// </summary>
        private Dictionary<object, Dictionary<ISubjectBase, SubjectLocation>> _subjectStatckLocations = new Dictionary<object, Dictionary<ISubjectBase, SubjectLocation>>();

        /// <summary>
        /// 添加数据观测者
        /// <param name="subject">数据主体</param>
        /// <param name="observer">数据观测者</param>
        /// </summary>
        static public bool AddObserver<T>(ISubject<T> subject, IObserver<T> observer)
        {
            if (!GlobalParams.OpenDebugLog)
                return false;

            ObserverLocation findObserverLocationInfo = GetOrCreateObserverLocation(subject, observer, true);
            if (null != findObserverLocationInfo)
            {
                findObserverLocationInfo.callbackInitDelegate = observer.callbackInit;
                findObserverLocationInfo.callbackUpdateDelegate = observer.callbackUpdate;
                return true;
            }
            else 
            {
                return false;
            }
        }

        /// <summary>
        /// 移除数据观测者
        /// <param name="subject">数据主体</param>
        /// <param name="observer">数据观测者</param>
        /// </summary>
        static public bool RemoveObserver<T>(ISubject<T> subject, IObserver<T> observer)
        {
            if (!GlobalParams.OpenDebugLog)
                return false;

            SubjectLocation findSubjectLocationInfo = GetOrCreateSubjectLocation(subject, false);

            findSubjectLocationInfo.observserLocations.Remove(observer);

			if (0 == findSubjectLocationInfo.observserLocations.Count)
			{
                var instanceTmp = GameEntry.GetInstance<SubjectManager>();
                var key = subject.GetBindTarget();
                instanceTmp._subjectStatckLocations.Remove(key);
			}
			return true;
        }

		/// <summary>
		/// 移除数据主体
		/// <param name="subject">数据主体</param>
		/// </summary>
        static public bool RemoveSubject<T>(ISubject<T> subject)
        {
            if (!GlobalParams.OpenDebugLog)
                return false;

            SubjectLocation findSubjectLocationInfo = GetOrCreateSubjectLocation(subject, false);
            if (null != findSubjectLocationInfo)
            {
                var instanceTmp = GameEntry.GetInstance<SubjectManager>();
                var key = subject.GetBindTarget();
                instanceTmp._subjectStatckLocations.Remove(key);
            }
			return true;
        }

        /// <summary>
        /// 数据即将初始化
        /// <param name="subject">数据主体</param>
        /// <param name="observer">数据观测者</param>
        /// </summary>
        static public bool WillValueInit<T>(ISubject<T> subject, IObserver<T> observer)
        {
            ObserverLocation findLocationInfo = GetOrCreateObserverLocation(subject, observer, false);
            if (null != findLocationInfo)
            {
                findLocationInfo.stackLocationObserverInit.StartTimeSpanCalculate();
            }
            return true;
        }

        /// <summary>
        /// 数据初始化完毕
        /// <param name="subject">数据主体</param>
        /// <param name="observer">数据观测者</param>
        /// </summary>
        static public bool ValueInited<T>(ISubject<T> subject, IObserver<T> observer)
        {
            ObserverLocation findLocationInfo = GetOrCreateObserverLocation(subject, observer, false);
            if (null != findLocationInfo)
            {
                findLocationInfo.stackLocationObserverInit.StopTimeSpanCalculate();
                findLocationInfo.stackLocationObserverInit.GetStatck(_classNames);
            }
            return true;
        }

        /// <summary>
        /// 数据即将发生变化
        /// <param name="subject">数据主体</param>
        /// <param name="observer">数据观测者</param>
        /// </summary>
        static public bool WillValueUpdate<T>(ISubject<T> subject, IObserver<T> observer)
		{
            ObserverLocation findLocationInfo = GetOrCreateObserverLocation(subject, observer, false);
            if (null != findLocationInfo)
            {
                findLocationInfo.stackLocationValueChange.StartTimeSpanCalculate();
            }
            return true;
		}

        /// <summary>
        /// 数据已经发生变化
        /// <param name="subject">数据主体</param>
        /// <param name="observer">数据观测者</param>
        /// </summary>
        static public bool ValueUpdated<T>(ISubject<T> subject, IObserver<T> observer)
		{
            ObserverLocation findLocationInfo = GetOrCreateObserverLocation(subject, observer, false);
			if (null != findLocationInfo)
			{
                findLocationInfo.stackLocationValueChange.StopTimeSpanCalculate();
                findLocationInfo.stackLocationValueChange.GetStatck(_classNames);
			}
            return true;
		}

		/// <summary>
		/// 获取所有数据主体的定位信息
		/// </summary>
		static public Dictionary<object, Dictionary<ISubjectBase, SubjectLocation>> GetSubjectStatckLocations()
		{
            return GameEntry.GetInstance<SubjectManager>()._subjectStatckLocations;
		}

        /// <summary>
        /// 获取或者创建数据主体堆栈信息
        /// <param name="subject">数据主体</param>
        /// <param name="autoCreateWhenNotFound">是否当没有找到观测者的时候，自动创建并添加到列表中</param>
        /// <return>观测者堆栈信息</return>
        /// </summary>
        static public SubjectLocation GetOrCreateSubjectLocation<T>(ISubject<T> subject, bool autoCreateWhenNotFound)
        {
            SubjectLocation retVale = null;

            var instanceTmp = GameEntry.GetInstance<SubjectManager>();
            Dictionary<ISubjectBase, SubjectLocation> findLocationInfos = null;

            var key = subject.GetBindTarget();

            if (null == key)
            {
                Log.Error("SubjectManager GetOrCreateSubjectLocation error: key is null");
                return retVale;
            }

            if (!instanceTmp._subjectStatckLocations.TryGetValue(key, out findLocationInfos))
            {
                findLocationInfos = new Dictionary<ISubjectBase, SubjectLocation>();

                if (autoCreateWhenNotFound)
                {
                    instanceTmp._subjectStatckLocations.Add(key, findLocationInfos);
                }
                else
                {
                    Log.Error("SubjectManager GetOrCreateSubjectLocation error: not found subjects location, target=" + key + " subject=" + subject);
                    return retVale;
                }
            }

            if (!findLocationInfos.TryGetValue(subject, out retVale))
            {
                retVale = new SubjectLocation();

                if (autoCreateWhenNotFound)
                {
                    findLocationInfos.Add(subject, retVale);
                }
                else
                {
                    Log.Error("SubjectManager GetOrCreateSubjectLocation error: not found subject location, subject=" + subject);
                    return retVale;
                }
            }
            return retVale;
        }

        /// <summary>
        /// 获取或者创建观测者堆栈信息
        /// <param name="subject">数据主体</param>
        /// <param name="observer">观测者</param>
        /// <param name="autoCreateWhenNotFound">是否当没有找到观测者的时候，自动创建并添加到列表中</param>
        /// <return>观测者堆栈信息</return>
        /// </summary>
        static public ObserverLocation GetOrCreateObserverLocation<T>(ISubject<T> subject, IObserver<T> observer, bool autoCreateWhenNotFound)
        {
            ObserverLocation retVale = null;

            SubjectLocation findSubjectLocationInfo = GetOrCreateSubjectLocation(subject, autoCreateWhenNotFound);

            if (null != findSubjectLocationInfo)
            {
                bool isFindObserverLocation = findSubjectLocationInfo.observserLocations.TryGetValue(observer, out retVale);
                if (autoCreateWhenNotFound)
                {
                    if (isFindObserverLocation)
                    {
                        Log.Error("SubjectManager GetOrCreateObserverLocation error: has duplicate observer=" + observer + " in subject=" + subject);
                        return retVale;
                    }
                    else 
                    {
                        retVale = new ObserverLocation();
                        findSubjectLocationInfo.observserLocations.Add(observer, retVale);
                    }
                }
                else if (!isFindObserverLocation)
                {
                    Log.Error("SubjectManager GetOrCreateObserverLocation error: not found observer=" + observer + " in subject=" + subject);
                }
            }
            return retVale;
        }
    }
}