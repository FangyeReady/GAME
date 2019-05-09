#if HOTFIX_ENABLE

using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace shaco
{
    public class CreateGenericClass
    {
        //c#常用泛型类
        static public List<object> List() { return new List<object>(); }
        static public List<object> List(IEnumerable<object> collection) { return new List<object>(collection); }
        static public Dictionary<object, object> Dictionary() { return new Dictionary<object, object>(); }
        static public Dictionary<object, object> Dictionary(IEqualityComparer<object> comparer) { return new Dictionary<object, object>(comparer); }
        static public Dictionary<object, object> Dictionary(IDictionary<object, object> dictionary) { return new Dictionary<object, object>(); }
        static public Dictionary<object, object> Dictionary(IDictionary<object, object> dictionary, IEqualityComparer<object> comparer) { return new Dictionary<object, object>(dictionary, comparer); }
        static public Queue<object> Queue() { return new Queue<object>(); }
        static public Stack<object> Stack() { return new Stack<object>(); }
        static public SortedDictionary<object, object> SortedDictionary() { return new SortedDictionary<object, object>(); }
        static public HashSet<object> HashSet() { return new HashSet<object>(); }
        static public KeyValuePair<object, object> KeyValuePair() { return new KeyValuePair<object, object>(); }

        //shaco框架常用泛型类
        static public shaco.Base.EventCallBack<object> EventCallBack() { return new shaco.Base.EventCallBack<object>(); }
    }
}
#endif