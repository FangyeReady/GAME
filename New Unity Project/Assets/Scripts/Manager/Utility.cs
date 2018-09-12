using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 工具类
/// </summary>
public static class Utility  {

    public static void SwitchScene(StaticData.Scenes scene)
    {
        SceneManager.LoadScene((int)scene);
        Debug.Log("Load Scene " + scene.ToString());
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
}
