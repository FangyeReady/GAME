using System.Collections;
using System.Collections.Generic;
using System;

static public class shaco_ExtensionsConvert
{
    /// <summary>
    /// 获取数据的二进制数组
    /// <param name="value">数据</param>
    /// <return>二进制数组</return>
    /// </summary>
    static public byte[] ToByteArrary(this float value) { return BitConverter.GetBytes(value); }
    static public byte[] ToByteArrary(this ulong value) { return BitConverter.GetBytes(value); }
    static public byte[] ToByteArrary(this ushort value) { return BitConverter.GetBytes(value); }
    static public byte[] ToByteArrary(this long value) { return BitConverter.GetBytes(value); }
    static public byte[] ToByteArrary(this double value) { return BitConverter.GetBytes(value); }
    static public byte[] ToByteArrary(this short value) { return BitConverter.GetBytes(value); }
    static public byte[] ToByteArrary(this char value) { return BitConverter.GetBytes(value); }
    static public byte[] ToByteArrary(this bool value) { return BitConverter.GetBytes(value); }
    static public byte[] ToByteArrary(this int value) { return BitConverter.GetBytes(value); }

    /// <summary>
    /// 二进制数组转数据
    /// <param name="value">二进制数组</param>
    /// <param name="startIndex">二进制数组数据开始下标</param>
    /// <return>数据</return>
    /// </summary>
    static public bool ToBoolean(this byte[] value, int startIndex = 0) { return BitConverter.ToBoolean(value, startIndex); }
    public static char ToChar(byte[] value, int startIndex = 0) { return BitConverter.ToChar(value, startIndex); }
    public static double ToDouble(byte[] value, int startIndex = 0) { return BitConverter.ToDouble(value, startIndex); }
    public static short ToInt16(byte[] value, int startIndex = 0) { return BitConverter.ToInt16(value, startIndex); }
    public static int ToInt32(byte[] value, int startIndex = 0) { return BitConverter.ToInt32(value, startIndex); }
    public static long ToInt64(byte[] value, int startIndex = 0) { return BitConverter.ToInt64(value, startIndex); }
    public static float ToSingle(byte[] value, int startIndex = 0) { return BitConverter.ToSingle(value, startIndex); }
    public static string ToString(byte[] value, int startIndex = 0) { return BitConverter.ToString(value, startIndex); }
    public static string ToString(byte[] value, int startIndex, int length) { return BitConverter.ToString(value, startIndex, length); }
    public static ushort ToUInt16(byte[] value, int startIndex = 0) { return BitConverter.ToUInt16(value, startIndex); }
    public static uint ToUInt32(byte[] value, int startIndex = 0) { return BitConverter.ToUInt32(value, startIndex); }
    public static ulong ToUInt64(byte[] value, int startIndex = 0) { return BitConverter.ToUInt64(value, startIndex); }
}