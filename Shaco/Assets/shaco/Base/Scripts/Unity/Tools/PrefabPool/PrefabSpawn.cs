using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class PrefabSpawn : shaco.Base.IObjectSpawn
    {
        public T CreateNewObject<T>(string key, string multiVersionControlRelativePath) where T : new()
        {
            if (typeof(GameObject) == typeof(T))
            {
                var gameObj = shaco.ResourcesEx.LoadResourcesOrLocal<GameObject>(key, multiVersionControlRelativePath);
                return (T)(object)MonoBehaviour.Instantiate(gameObj);
            }
            else
            {
                shaco.Log.Error("PrefabSpawn CreateNewObject error: not GameObject, key=" + key);
                return new T();
            }
        }

        public T CreateNewObject<T>(object obj) where T : new()
        {
            if (typeof(GameObject) == typeof(T))
            {
                var gameObj = obj as GameObject;
                if (null != gameObj)
                {
                    return (T)(object)MonoBehaviour.Instantiate(gameObj);
                }
                else 
                {
                    shaco.Log.Error("PrefabSpawn CreateNewObject error: not a GameObject, obj=" + obj);
                    return new T();
                }
            }
            else
            {
                shaco.Log.Error("PrefabSpawn CreateNewObject error: not GameObject, type=" + typeof(T));
                return new T();
            }
        }

        public void ActiveObject<T>(T obj)
        {
            if (typeof(GameObject) == typeof(T))
            {
                var gameObj = (GameObject)(object)obj;
                gameObj.SetActive(true);
            }
        }

        public void RecyclingAllObjects<T>(T[] objs)
        {
            for (int i = objs.Length - 1; i >= 0; --i)
            {
                RecyclingObject<T>(objs[i]);
            }
        }

        public void RecyclingObject<T>(T obj)
        {
            if (typeof(GameObject) == obj.GetType())
            {
                var gameObj = (GameObject)(object)obj;
				gameObj.SetActive(false);
            }
            else
            {
                obj = default(T);
            }
        }

        public void DestroyObjects<T>(T[] objs)
        {
            for (int i = objs.Length - 1; i >= 0; --i)
            {
                DestroyObject(objs[i]);
            }
        }

        public void DestroyObject<T>(T obj)
        {
            if (typeof(GameObject) == obj.GetType())
            {
                var gameObj = (GameObject)(object)obj;
                MonoBehaviour.Destroy(gameObj);
            }
            else
            {
                obj = default(T);
            }
        }
    }
}
