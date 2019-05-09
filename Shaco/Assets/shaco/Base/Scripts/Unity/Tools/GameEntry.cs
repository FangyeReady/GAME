using UnityEngine;
using System.Collections;

namespace shaco
{
	public class GameEntry : shaco.Base.GameEntry
	{
		static public T AddComponentInstance<T>(bool isDontDestroyOnLoad = true) where T : UnityEngine.Component
        {
            T retValue = default(T);
            System.Type typeTmp = typeof(T);
            if (HasInstance(typeTmp))
            {
                Log.Error("AddComponentInstance error: has added instance type=" + shaco.Base.Utility.ToTypeString<T>());
                return null;
            }
            else
            {
                retValue = CreateComponentInstance<T>(isDontDestroyOnLoad);
                GetInstances().Add(typeTmp, retValue);
            }
            return retValue;
        }

        static public T GetComponentInstance<T>(bool isDontDestroyOnLoad = true) where T : UnityEngine.Component
        {
            System.Type typeTmp = typeof(T);

            if (!HasInstance(typeTmp))
            {
                return AddComponentInstance<T>();
            }
            else
            {
                return (T)GetInstances()[typeTmp];
            }
        }

		static public T CreateComponentInstance<T>(bool isDontDestroyOnLoad) where T : UnityEngine.Component
        {
            T retValue = default(T);
            var listFind = GameObject.FindObjectsOfType<T>();

            if (listFind.Length > 1)
            {
                for (int i = 1; i < listFind.Length; ++i)
                {
                    MonoBehaviour.DestroyImmediate(listFind[i]);
                }
            }

            if (listFind.Length > 0)
            {
                retValue = listFind[0];
            }

            if (retValue == null)
            {
                GameObject objTmp = new GameObject();
                retValue = objTmp.AddComponent<T>();
                objTmp.transform.name = retValue.GetType().FullName;
            }

            if (isDontDestroyOnLoad)
                UnityHelper.SafeDontDestroyOnLoad(retValue.gameObject);
            return retValue;
        }
	}
}
