using System.Collections;
using System;
using System.Linq;

namespace shaco.Base
{
    public static partial class Utility
    {
        static private DateTime _prevDateTime = DateTime.Now;
        
        static public T Instantiate<T>()
        {
            return (T)(typeof(T).Instantiate());
        }

        static public object Instantiate(string typeName)
        {
            object retValue = null;
            for (int i = 0; i < GlobalParams.DEFAULT_ASSEMBLY.Length; ++i)
            {
                var assemblyTmp = System.Reflection.Assembly.Load(GlobalParams.DEFAULT_ASSEMBLY[i]);
                if (null != assemblyTmp)
                {
                    retValue = assemblyTmp.CreateInstance(typeName);
                    if (null != retValue)
                    {
                        break;
                    }
                }
            }
            return retValue;
        }

        static public TimeSpan GetEplaseTime()
        {
            var nowTimeTmp = DateTime.Now;
            var retValue = nowTimeTmp - _prevDateTime;
            _prevDateTime = nowTimeTmp;
            return retValue;
        }

        static public int Random()
        {
            return GetRandomSeed().Next();
        }

        /// <summary>
        /// include min, exclude max
        /// </summary>
        static public int Random(int min, int max)
        {
            return GetRandomSeed().Next(min, max);
        }

        /// <summary>
        /// include min, exclude max, and keep 8 decial places
        /// </summary>
        static public float Random(float min, float max)
        {
            int maxDigit = 100000000;
            int randNumber = GetRandomSeed().Next((int)(min * maxDigit), (int)(max * maxDigit));
            var retValue = (float)randNumber / (float)maxDigit;
            
            return retValue;
        }

        /// <summary>
        /// compare t1 and t2
        /// </summary>
        /// <returns></returns> 0: t1 == t2    -1: t1 < t2     1: t1 > t2
        public delegate int COMPARE_CALLFUNC(object t1, object t2);

        //todo: get current system time
        static public System.DateTime GetCurrentTime()
        {
            var ret = System.DateTime.Now;
            return ret;
        }

        //todo: get how many days this month 
        static public int GetDaysOfCurrentMonth()
        {
            var nowTime = GetCurrentTime();
            return System.DateTime.DaysInMonth(nowTime.Year, nowTime.Month);
        }

        /// <summary>
        /// convert week of enumeration to index 
        /// </summary>
        /// <param name="dayOfWeek"></param> 
        /// <returns></returns>
        static public int GetWeekOfIndex(System.DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case System.DayOfWeek.Monday: return 0;
                case System.DayOfWeek.Tuesday: return 1;
                case System.DayOfWeek.Wednesday: return 2;
                case System.DayOfWeek.Thursday: return 3;
                case System.DayOfWeek.Friday: return 4;
                case System.DayOfWeek.Saturday: return 5;
                case System.DayOfWeek.Sunday: return 6;
                default: Log.Error("getWeekOfIndex error: invalid nowTime.DayOfWeek=" + dayOfWeek); return 0;
            }
        }

        static private Random GetRandomSeed()
        {
            long tick = DateTime.Now.Ticks;
            Random retValue = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
            return retValue;
        }

        /// <summary>
        /// 判断字符串是否为数字
        /// </summary>
        static public bool IsNumber(string str)
        {
            bool ret = true;
            for (int i = 0; i < str.Length; ++i)
            {
                var c = str[i];
                if (c < '0' || c > '9')
                {
                    if (c == '.')
                    {
                        if (i == 0 || i == str.Length - 1)
                            ret = false;
                    }
                    else if (c == '-')
                    {
                        if (i != 0)
                            ret = false;
                    }
                    else
                    {
                        ret = false;
                    }
                }
                if (!ret)
                    break;
            }
            if (string.IsNullOrEmpty(str))
                ret = false;
            return ret;
        }

        /// <summary>
        /// 取范围值，在最小和最大之间
        /// <param name="value">当前值</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <return>在最小和最大之间的值</return>
        /// </summary>
        static public float Clamp(float value, float min, float max)
        {
            if (value < min) value = min;
            if (value > max) value = max;
            return value;
        }

        /// <summary>
        /// 断言类型判断
        /// <param name="expected">期望类型</param>
        /// <param name="actual">实际类型</param>
        /// </summary>
        static public void AssetAreEqual<T>(T expected, T actual)
        {
            System.Diagnostics.Debug.Assert(actual.Equals(expected));
        }
    }
}