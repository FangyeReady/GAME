using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace shaco.Base
{
    public static partial class Utility
    {
        /// <summary>
        /// 获取类型全称
        /// <return>类型全称</return>
        /// </summary>
        static public string ToTypeString<T>()
        {
            return typeof(T).FullName;
        }

        /// <summary>
        /// 获取枚举所有类型
        /// </summary>
        /// <return>枚举所有类型</return>
        static public T[] ToEnums<T>()
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        /// <summary>
        /// 获取枚举
        /// <param name="value">枚举字符串类型全称</param>
        /// <return>枚举</return>
        /// </summary>
        static public T ToEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        /// <summary>
        /// 获取变量名字，在.Net6.0以上有nameof(T)可以作为替代
        /// <param name="expr">访问回调，例如获取class.A的变量名，则传入()=> this.A</param>
        /// <return>变量名字</return>
        /// </summary>
        static public string ToVariableName<T>(System.Linq.Expressions.Expression<Func<T>> expr)
        {
            return ((System.Linq.Expressions.MemberExpression)expr.Body).Member.Name;
        }

        /// <summary>
        /// 通过反射筛选具有指定属性的类型
        /// <param name="flags">属性类型标签</param>
        /// <param name="ignoreTypes">筛选过滤类型，不会被计算在返回值内</param>
        /// <return>使用了该属性的所有类型</return>
        /// </summary>
        static public System.Type[] GetAttributes<T>(System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, params System.Type[] ignoreTypes)
        {
            //加载程序集信息
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            var types = asm.GetExportedTypes();

            //验证指定自定义属性(使用的是4.0的新语法，匿名方法实现的)
            Func<System.Attribute[], bool> IsAttribute = o =>
             {
                 foreach (System.Attribute a in o)
                 {
                     if (a is T)
                         return true;
                 }
                 return false;
             };

            var retValue = types.Where(o =>
            {
                return IsAttribute(System.Attribute.GetCustomAttributes(o, true));
            }).ToList();

            //过滤忽略的类型
            if (null != ignoreTypes && ignoreTypes.Length > 0)
            {
                for (int i = ignoreTypes.Length - 1; i >= 0; --i)
                {
                    var typeTmp = ignoreTypes[i];
                    if (retValue.Contains(typeTmp))
                    {
                        retValue.Remove(typeTmp);
                    }
                }
            }

            //过滤忽略的属性
            for (int i = retValue.Count - 1; i >= 0; --i)
            {
                var constructorsTmp = retValue[i].GetConstructors(flags);
                if (null == constructorsTmp || constructorsTmp.Length == 0)
                {
                    retValue.RemoveAt(i);
                }
            }
            return retValue.ToArray();
        }

        /// <summary>
        /// 通过反射筛选具有指定属性的类型
        /// <param name="ignoreTypes">筛选过滤类型，不会被计算在返回值内</param>
        /// <return>使用了该属性的所有类型</return>
        /// </summary>
        static public System.Type[] GetAttributes<T>(params System.Type[] ignoreTypes)
        {
            return GetAttributes<T>(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, ignoreTypes);
        }

        /// <summary>
        /// 获取程序集中指定的类型，包括继承子类
        /// <param name="T">基类类型</param>
        /// <param name="assemblyNames">程序集</param>
        /// <return>程序集中查找到的类型</return>
        /// </summary>
        static public System.Type[] GetClasses<T>(string[] assemblyNames)
        {
			var typeBase = typeof(T);
            var retValue = new System.Collections.Generic.List<System.Type>();
            foreach (string assemblyName in assemblyNames)
            {
                var assembly = System.Reflection.Assembly.Load(assemblyName);
                if (assembly == null)
                {
                    continue;
                }

                System.Type[] types = assembly.GetTypes();
                foreach (System.Type type in types)
                {
                    if (type.IsClass && !type.IsAbstract && typeBase.IsAssignableFrom(type))
                    {
                        retValue.Add(type);
                    }
                }
            }
            return retValue.ToArray();
        }

        /// <summary>
        /// 获取程序集中指定的类型，包括继承子类
        /// <param name="T">基类类型</param>
        /// <return>程序集中查找到的类型</return>
        /// </summary>
        static public System.Type[] GetClasses<T>()
		{
			return GetClasses<T>(GlobalParams.DEFAULT_ASSEMBLY);
        }

        /// <summary>
        /// 获取程序集中指定的类型全称，包括继承子类
        /// <param name="T">基类类型</param>
        /// <return>程序集中查找到的类型全称</return>
        /// </summary>
        static public string[] GetClassNames<T>()
        {
            var classesTmp = GetClasses<T>();
            var retValue = new string[classesTmp.Length];
            for (int i = classesTmp.Length - 1; i >= 0; --i)
            {
                retValue[i] = classesTmp[i].FullName;
            }
            return retValue;
        }
    }
}