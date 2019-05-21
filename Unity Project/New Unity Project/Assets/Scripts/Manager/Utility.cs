using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 工具类
/// </summary>
public static class Utility  {

    public static void SwitchScene(StaticData.Scenes scene)
    {
        SceneManager.LoadScene((int)scene);
        LoggerM.Log("Load Scene " + scene.ToString());
    }

    public static DateTime ToUtcTime(long secondsFromUtcStart)
    {
        DateTime utcStartTime = new DateTime(1970, 1, 1);
        const long tickPerSeconds = 1000 * 10000;
        return utcStartTime.AddTicks(secondsFromUtcStart * tickPerSeconds);
    }

    public static long ToTokyoDatatimeToUtc(DateTime time)
    {
        DateTime utcStartTime = new DateTime(1970, 1, 1);
        long t = (time.Ticks - utcStartTime.Ticks) / 10000;
        return t;
    }

    //C#生成一个时间戳：(这里以秒为单位)
    private static string GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds).ToString();
    }
    //C#时间戳转换为格式时间：(这里以秒为单位)
    private static DateTime GetTime(string timeStamp)
    {
        DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));  //时间戳起始点转为目前时区
        long lTime = long.Parse(timeStamp + "0000000");//转为long类型  
        TimeSpan toNow = new TimeSpan(lTime); //时间间隔
        return dtStart.Add(toNow); //加上时间间隔得到目标时间
    }

    /// <summary>
    /// 得到一个范围内的随机数
    /// </summary>
    /// <param name="min">1，最小角色id</param>
    /// <param name="max">100，最大角色id</param>
    /// <returns></returns>
    public static int GetRandomVal(int min, int max)
    {
        int seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        System.Random random = new System.Random(seed);

        int result = random.Next(min, max);//包含下限，不包含上限

        return result;
    }

    /// <summary>
    /// 得到某个程序是否在运行，若存在则在运行
    /// </summary>
    /// <param name="methodName"></param>
    /// <returns></returns>
    public static int GetProcessID(string methodName)
    {
        Process[] processes = Process.GetProcessesByName(methodName);

        foreach (var item in processes)
        {
            LoggerM.LogError(item.Id.ToString() + "---");
        }
        return 0;
    }
}
