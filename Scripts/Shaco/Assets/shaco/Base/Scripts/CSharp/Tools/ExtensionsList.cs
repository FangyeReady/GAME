using System.Collections;
using System.Collections.Generic;

static public class shaco_ExtensionsList
{
    static public List<T> ToArrayList<T>(this T[] list)
    {
        var ret = new List<T>();
        for (int i = 0; i < list.Length; ++i)
        {
            ret.Add(list[i]);
        }
        return ret;
    }

    static public T[] ToArray<T>(this List<T> list)
    {
        var ret = new T[list.Count];
        for (int i = 0; i < list.Count; ++i)
        {
            ret[i] = list[i];
        }
        return ret;
    }

    static public T2[] ToArray<T1, T2>(this T1[] list)
    {
        T2[] ret = new T2[list.Length];

        for (int i = 0; i < list.Length; ++i)
        {
            ret[i] = (T2)(object)list[i];
        }
        return ret;
    }

    static public List<T1> ToKeyArrayList<T1, T2>(this Dictionary<T1, T2> dic)
    {
        List<T1> ret = new List<T1>();

        foreach (var iter in dic)
        {
            ret.Add(iter.Key);
        }
        return ret;
    }

    static public List<T2> ToValueArrayList<T1, T2>(this Dictionary<T1, T2> dic)
    {
        List<T2> ret = new List<T2>();

        foreach (var iter in dic)
        {
            ret.Add(iter.Value);
        }
        return ret;
    }

    static public List<T2> ToArrayListConvert<T1, T2>(this T1[] list, bool printError = true)
    {
        var ret = new List<T2>();
        for (int i = 0; i < list.Length; ++i)
        {
            try
            {
                ret.Add((T2)System.Convert.ChangeType(list[i], typeof(T2)));
            }
            catch (System.Exception e)
            {
                if (printError)
                    shaco.Base.Log.Error("ToArrayConvert error: can't convert type [" + shaco.Base.Utility.ToTypeString<T1>() + "] To [" + shaco.Base.Utility.ToTypeString<T2>() + "] value=" + list[i] + "]\n" + e);
            }
        }
        return ret;
    }

    static public List<T2> ToArrayListConvert<T1, T2>(this List<T1> list, bool printError = true)
    {
        var ret = new List<T2>();
        for (int i = 0; i < list.Count; ++i)
        {
            try
            {
                ret.Add((T2)System.Convert.ChangeType(list[i], typeof(T2)));
            }
            catch (System.Exception e)
            {
                if (printError)
                    shaco.Base.Log.Error("ToArrayConvert error: can't convert type [" + shaco.Base.Utility.ToTypeString<T1>() + "] To [" + shaco.Base.Utility.ToTypeString<T2>() + "] value=" + list[i] + "]\n" + e);
            }
        }
        return ret;
    }

    static public T2[] ToArrayConvert<T1, T2>(this T1[] list, bool printError = true)
    {
        var ret = new T2[list.Length];
        for (int i = 0; i < list.Length; ++i)
        {
            try
            {
                ret[i] = (T2)System.Convert.ChangeType(list[i], typeof(T2));
            }
            catch (System.Exception e)
            {
                if (printError)
                    shaco.Base.Log.Error("ToArrayConvert error: can't convert type [" + shaco.Base.Utility.ToTypeString<T1>() + "] To [" + shaco.Base.Utility.ToTypeString<T2>() + "] value=" + list[i] + "\n" + e);
            }
        }
        return ret;
    }

    static public T2[] ToArrayConvert<T1, T2>(this List<T1> list, bool printError = true)
    {
        var ret = new T2[list.Count];
        for (int i = 0; i < list.Count; ++i)
        {
            try
            {
                ret[i] = (T2)System.Convert.ChangeType(list[i], typeof(T2));
            }
            catch (System.Exception e)
            {
                if (printError)
                    shaco.Base.Log.Error("ToArrayConvert error: can't convert type [" + shaco.Base.Utility.ToTypeString<T1>() + "] To [" + shaco.Base.Utility.ToTypeString<T2>() + "] value=" + list[i] + "]\n" + e);
            }
        }
        return ret;
    }

    static public void SwapValue<T>(this List<T> list, int sourceIndex, int destinationIndex)
    {
        if (list.Count <= 1)
            return;

        if (sourceIndex == destinationIndex)
        {
            shaco.Base.Log.Error("Exchange error: sourceIndex == destinationIndex");
            return;
        }

        if (sourceIndex < 0 || sourceIndex > list.Count - 1 || destinationIndex < 0 || destinationIndex > list.Count - 1)
        {
            shaco.Base.Log.Error("Exchange error: out of range sourceIndex=" + sourceIndex + " destinationIndex=" + destinationIndex + " Count=" + list.Count);
            return;
        }

        var sourceItem = list[sourceIndex];
        var desItem = list[destinationIndex];
        list[sourceIndex] = desItem;
        list[destinationIndex] = sourceItem;
    }

    //moveOffset: -1(move all values behind to front) 1(move all values front to behind)
    static public void MoveValues<T>(this List<T> list, int moveOffset)
    {
        if (list.Count <= 1)
            return;

        if (moveOffset > 0)
        {
            var firstData = list[0];
            for (int i = 0; i < list.Count - 1; ++i)
            {
                list[i] = list[i + 1];
            }
            list[list.Count - 1] = firstData;
        }
        else
        {
            var lastData = list[list.Count - 1];
            for (int i = list.Count - 1; i >= 1; --i)
            {
                list[i] = list[i - 1];
            }
            list[0] = lastData;
        }
    }

    static public bool IsNullOrEmpty(this IDictionary dic)
    {
        return dic == null || dic.Count == 0;
    }

    static public bool IsNullOrEmpty(this IList list)
    {
        return list == null || list.Count == 0;
    }

    static public void CopyFrom<T>(this List<T> list, List<T> other)
    {
        list.Clear();
        int countTmp = other.Count;
        for (int i = 0; i < countTmp; ++i)
        {
            list.Add(other[i]);
        }
    }

    static public void CopyFrom<T>(this List<T> list, T[] other)
    {
        list.Clear();
        int countTmp = other.Length;
        for (int i = 0; i < countTmp; ++i)
        {
            list.Add(other[i]);
        }
    }

    static public void CopyFrom<T>(this T[] list, List<T> other)
    {
        int countTmp = other.Count;
        list = new T[countTmp];
        for (int i = 0; i < countTmp; ++i)
        {
            list[i] = other[i];
        }
    }

    static public void CopyFrom<T>(this T[] list, T[] other)
    {
        int countTmp = other.Length;
        list = new T[countTmp];
        for (int i = 0; i < countTmp; ++i)
        {
            list[i] = other[i];
        }
    }

    static public int IndexOf<T>(this T[] list, T find)
    {
        int retValue = -1;
        for (int i = 0; i < list.Length; ++i)
        {
            if (list[i].Equals(find))
            {
                retValue = i;
                break;
            }
        }
        return retValue;
    }

    /// <summary>
    /// 移除数组中标记的组建
    /// <param name="tag">要被移除的查找标记</param>
    /// </summary>
    static public void Trim<T>(this List<T> list, T tag)
    {
        for (int i = list.Count - 1; i >= 0; --i)
        {
            if (list[i].Equals(tag))
            {
                list.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 判断下标是否越界
    /// <param name="list">数组对象</param>
    /// <param name="index">当前下标</param>
    /// <param name="erorrMessagePrefix">发生越界错误的提示文本前缀</param>
    /// <return>true:越界 false:不越界</return>
    /// </summary>
    static public bool IsOutOfRange<T>(this IList<T> list, int index, string erorrMessagePrefix = "")
    {
        bool retValue = true;

        if (null == list || 0 == list.Count)
        {
            shaco.Base.Log.Error(erorrMessagePrefix + ": list is empty, index=" + index);
            return retValue;
        }

        retValue = (index < 0 || index > list.Count - 1);

#if DEBUG_LOG
        if (retValue && !string.IsNullOrEmpty(erorrMessagePrefix))
        {
            var errorString = new System.Text.StringBuilder(erorrMessagePrefix);
            errorString.Append(": out of range, index=" + index + " count=" + list.Count);
            errorString.Append("\n");

            for (int i = 0; i < list.Count; ++i)
            {
                errorString.Append("[");
                errorString.Append(i);
                errorString.Append("]");
                errorString.Append(list[i]);
                errorString.Append(((i + 1) % 4 == 0) ? "\n" : "\t");
            }
            shaco.Base.Log.Error(errorString.ToString());
        }
#endif
        return retValue;
    }

    static public bool IsOutOfRange<KEY, VALUE>(this IDictionary<KEY, VALUE> dic, KEY key, string erorrMessagePrefix = "")
    {
        bool retValue = true;

        if (null == dic || 0 == dic.Count)
        {
            shaco.Base.Log.Error(erorrMessagePrefix + ": dictionary is empty, key=" + key);
            return retValue;
        }

        retValue = null == key || !dic.ContainsKey(key);

#if DEBUG_LOG
        if (retValue && !string.IsNullOrEmpty(erorrMessagePrefix))
        {
            var errorString = new System.Text.StringBuilder(erorrMessagePrefix);
            errorString.Append(": not found int dictionary, key=" + key + " count=" + dic.Count);
            errorString.Append("\n");

            int index = 0;
            foreach (var iter in dic)
            {
                errorString.Append("[");
                errorString.Append(iter.Key);
                errorString.Append("]");

                var typeValueTmp = iter.Value.GetType();

                if (typeValueTmp.IsClass || typeValueTmp.IsInherited<System.Collections.ICollection>())
                {
                    errorString.Append(typeValueTmp.Name);
                }
                else
                {
                    errorString.Append(iter.Value);
                }
                errorString.Append(((index++ + 1) % 3 == 0) ? "\n" : "\t");
            }
            shaco.Base.Log.Error(errorString.ToString());
        }
#endif

        return retValue;
    }
}