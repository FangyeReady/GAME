using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class PrefabPool : shaco.Base.ObjectPool
    {
        //从对象池中实例化一个对象
        static public GameObject Instantiate(string key, string multiVersionControlRelativePath = "")
        {
            return shaco.Base.ObjectPool.InstantiateBase<GameObject, PrefabPool>(key, multiVersionControlRelativePath);
        }

        //从对象池中实例化一个对象，默认以obj的名字作为key值
        static public GameObject Instantiate(GameObject obj)
        {
            return shaco.Base.ObjectPool.InstantiateBase<GameObject, PrefabPool>(obj);
        }

        //回收所有对象以便再次利用
        new static public GameObject[] RecyclingAllObjects(string key)
        {
            var objs = shaco.Base.ObjectPool.RecyclingAllObjectsBase<PrefabPool>(key);

            for (int i = objs.Length - 1; i >= 0; --i)
            {
                shaco.GameEntry.GetComponentInstance<PrefabPoolCompnnet>().ChangeParentToPrefabPoolComponent(objs[i] as GameObject);
            }
            return objs.ToArrayConvert<object, GameObject>();
        }

        //回收对象以便再次利用
        static public GameObject RecyclingObject(GameObject obj)
        {
            shaco.Base.ObjectPool.RecyclingObjectBase<PrefabPool>(obj);
            shaco.GameEntry.GetComponentInstance<PrefabPoolCompnnet>().ChangeParentToPrefabPoolComponent(obj as GameObject);
            return obj;
        }

        //卸载所有对象
        new static public void DestroyAllObjects(string key)
        {
            DestroyAllObjectsBase<PrefabPool>(key);
        }

        //卸载对象
        new static public void DestroyObject(object obj)
        {
            DestroyObjectBase<PrefabPool>(obj);
        }

        //清空所有对象
        new static public void Clear()
        {
            GameEntry.GetInstance<PrefabPool>().ClearBase();
        }

        public PrefabPool()
        {
            _objectSpwanInterface = new PrefabSpawn();
        }
    }
}