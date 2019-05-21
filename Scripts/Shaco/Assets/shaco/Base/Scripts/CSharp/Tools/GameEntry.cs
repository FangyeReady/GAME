using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
	[System.Serializable]
	public class GameEntry
    {
		static private GameEntry _instance = null;
        protected Dictionary<System.Type, object> _instances = new Dictionary<System.Type, object>();

		static public T AddInstance<T>() where T : class
		{
			T retValue = default(T);
			GameEntry instanceTmp = GetInstance();
            System.Type typeTmp = typeof(T);
            if (HasInstance(typeTmp))
			{
				Log.Error("AddInstance error: has added instance type=" + Utility.ToTypeString<T>());
                return null;
			}
			else 
			{
				retValue = (T)typeTmp.Instantiate();
				instanceTmp._instances.Add(typeTmp, retValue);
			}
			return retValue;
		}

		static public To SetInstance<From, To>() where To : class
        {
            To retValue = (To)(typeof(To).Instantiate());
            GameEntry instanceTmp = GetInstance();
            System.Type keyTmp = typeof(From);
            if (HasInstance(keyTmp))
            {
                instanceTmp._instances[keyTmp] = retValue;
            }
            else
            {
                instanceTmp._instances.Add(keyTmp, retValue);
            }
            return retValue;
        }

		static public T GetInstance<T>() where T : class
		{
			GameEntry instanceTmp = GetInstance();
			System.Type typeTmp = typeof(T);

			if (!HasInstance(typeTmp))
			{
				return AddInstance<T>();
			}
			else 
			{
				return (T)instanceTmp._instances[typeTmp];
			}
		}

		static public bool HasInstance<T>() where T : class
		{
			return HasInstance(typeof(T));
		}

		static public bool HasInstance(System.Type type)
        {
            return GetInstance()._instances.ContainsKey(type);
        }

		static public bool RemoveIntance<T>() where T : class
		{
			return RemoveIntance(typeof(T));
		}

		static public bool RemoveIntance(object obj)
		{
			return RemoveIntance(obj.GetType());
		}

        static public bool RemoveIntance(System.Type type)
		{
			if (!HasInstance(type))
			{
				Log.Error("RemoveIntance error: not find instance type=" + type.ToTypeString());
				return false;
			}
			else 
			{
				GetInstance()._instances.Remove(type);
				return true;
			}
		}

		static public void ClearIntances()
		{
			GetInstance()._instances.Clear();
		}
		
		static protected GameEntry GetInstance()
		{
			if (null == _instance)
			{
				_instance = new GameEntry();
			}
			return _instance;
		}

		static protected Dictionary<System.Type, object> GetInstances()
		{
			return GetInstance()._instances;
		}
    }
}

