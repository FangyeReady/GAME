using System.Collections;

static public class shaco_ExtensionsUtility 
{
    /// <summary>
    /// 判断对象是否为空，支持c#和unity空引用对象的判断
    /// <param name="obj">对象</param>
    /// <return>是否为空</return>
    /// </summary>
	static public bool IsNull(this object obj)
    {
        if (null == obj)
            return true;
        else
        {
            string typeNameTmp = obj.ToString();
            return (typeNameTmp.Length == 4 && typeNameTmp == "null");
        }
    }

    /// <summary>
    /// 获取对象类型全称
    /// <param name="obj">对象</param>
    /// <return>类型全称</return>
    /// </summary>
	static public string ToTypeString(this object obj)
    {
        return obj.IsNull() ? "null" : obj.GetType().FullName;
    }

    /// <summary>
    /// 获取类型全称
    /// <param name="type">类型</param>
    /// <return>类型全称</return>
    /// </summary>
    static public string ToTypeString(this System.Type type)
    {
        return type.IsNull() ? "null" : type.FullName;
    }
	
    /// <summary>
    /// 通过类型实例化一个对象
    /// <param name="type">类型</param>
    /// <return>实例化的对象，如果实例化失败则返回整型0</return>
    /// </summary>
    static public object Instantiate(this System.Type type)
    {
        if (type == typeof(short) || type == typeof(int) || type == typeof(long) || type == typeof(float) || type == typeof(double))
        {
            return 0;
        }
        else if (type == typeof(string)) return string.Empty;
        else if (type == typeof(bool)) return false;
        else if (type == typeof(char)) return '0';
        return type.IsNull() || type.GetConstructors().Length == 0 ? null : type.Assembly.CreateInstance(type.FullName);
    }

    /// <summary>
    /// 判断类型继承关系
    /// <param name="type">type</param>
    /// <return>继承自PARENT_TYPE返回true，反之false</return>
    /// </summary>
    static public bool IsInherited<PARENT_TYPE>(this System.Type type)
	{
		bool retValue = false;

        //如果和自身类型一样，则立即返回
        if (type == typeof(PARENT_TYPE))
        {
            return true;
        }

		if (null == type)
        {
            shaco.Base.Log.Exception("ExtensionUtility IsInherited error: type is null");
            return retValue;
        }

        var parentType = typeof(PARENT_TYPE);
        retValue = type.IsSubclassOf(parentType);

        if (!retValue)
        {
            var interfaces = type.GetInterfaces();
            for (int i = interfaces.Length - 1; i >= 0; --i)
            {
                if (interfaces[i] == parentType)
                {
                    retValue = true;
                    break;
                }
            }
        }        
        return retValue;
	}

    /// <summary>
    /// 通过字符串调用对象方法 
    /// <param name="target">对象</param>
    /// <param name="method">方法名字</param>
    /// <param name="parameters">方法参数</param>
    /// <return>调用成功返回true, 反之false</return>
    /// </summary>
    static public bool InvokeMethod(object target, string method, params object[] parameters)
    {
		if (null == target)
		{
			shaco.Base.Log.Exception("ExtensionUtility InvokeMethod error: target is null");
			return false;
		}
		if (string.IsNullOrEmpty(method))
		{
			shaco.Base.Log.Exception("ExtensionUtility InvokeMethod error: method is empty");
            return false;
		}

        var methodTmp = target.GetType().GetMethod(method);
        if (null == methodTmp)
        {
            shaco.Base.Log.Exception("ExtensionUtility InvokeMethod error: not found method by name=" + method);
            return false;
        }
        methodTmp.Invoke(target, parameters);
        return true;
    }

    /// <summary>
    /// 交换2个对象引用
    /// <param name="me">当前对象</param>
    /// <param name="other">需要被交换的对象的引用</param>
    /// <return>交换后的当前对象</return>
    /// </summary>
    static public T SwapValue<T>(this T me, ref T other)
    {
        T exchangeValue = me;
        me = other;
        other = exchangeValue;
        return me;
    }

    /// <summary>
    /// 判断字符串是否为数字
    /// <param name="str">字符串</param>
    /// <return>为数字返回true，反之false</return>
    /// </summary>
    static public bool IsNumber(this string str)
    {
        return shaco.Base.Utility.IsNumber(str);
    }

    /// <summary>
    /// 给字典添加一个字典数据
    /// <param name="map">当前字典</param>
    /// <param name="other">需要添加的字典数据</param>
    /// </summary>
    static public void AddRange<KEY, VALUE>(this System.Collections.Generic.Dictionary<KEY, VALUE> map, System.Collections.Generic.Dictionary<KEY, VALUE> other)
    {
        if (null != other)
        {
            foreach (var iter in other)
            {
                if (!map.ContainsKey(iter.Key))
                {
                    map.Add(iter.Key, iter.Value);
                }
                else 
                {
                    shaco.Base.Log.Error("Dictionary AddRange error: duplicate key=" + iter.Key);
                }
            }
        }
        else 
        {
            shaco.Base.Log.Error("Dictionary AddRange error: invalid param");
        }
    }

    /// <summary>
    /// 批量移除字典数据
    /// <param name="map">当前字典</param>
    /// <param name="keys">需要移除的字典键值</param>
    /// </summary>
    static public void RemoveRange<KEY, VALUE>(this System.Collections.Generic.Dictionary<KEY, VALUE> map, System.Collections.Generic.List<KEY> keys)
    {
        if (null != keys)
        {
            foreach (var iter in keys)
            {
                if (map.ContainsKey(iter))
                {
                    map.Remove(iter);
                }
                else
                {
                    shaco.Base.Log.Error("Dictionary RemoveRange[List] error: not found key=" + iter);
                }
            }
        }
        else
        {
            shaco.Base.Log.Error("Dictionary RemoveRange[List] error: invalid param");
        }
    }

    //// <summary>
    /// 获取类型中包含的属性
    /// <param name="type">类型</param>
    /// <return>如果包含该属性返回属性对象，否则返回null</return>
    /// </summary>
    static public T GetAttribute<T>(this System.Type type) where T : System.Attribute
    {
        T retValue = null;
        var attributeType = typeof(T);
        if (type.IsDefined(attributeType, false))
        {
            retValue = (type.GetCustomAttributes(attributeType, false)[0] as T);
        }
        return retValue;
    }
}
