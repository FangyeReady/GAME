using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DateTimeTest : MonoBehaviour {

    private readonly DateTime old = new DateTime(2018, 11, 1);
	// Use this for initialization
	void Start () {
        int off = CalcDaysDiff(old, DateTime.Now);

        Debug.Log(off.ToString());
        TimeSpan days = old - DateTime.Now;
        Debug.Log(days.Days.ToString());

        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0));
        var date = startTime.AddSeconds(1540458219);


        Debug.LogError(date.ToString());
	}

    private int CalcDaysDiff(DateTime before, DateTime end)
    {
        TimeSpan sp = end.Subtract(before);
        return sp.Days;
    }
}
