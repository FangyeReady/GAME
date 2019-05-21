using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class shaco_ExtensionsPrefabPool
{
	static public GameObject InstantiateWithPool(this string key)
	{
		return shaco.PrefabPool.Instantiate(key);
	}

    static public GameObject InstantiateWithPool(this GameObject obj)
    {
        return shaco.PrefabPool.Instantiate(obj);
    }

	static public GameObject RecyclingWithPool(this GameObject obj)
	{
		return shaco.PrefabPool.RecyclingObject(obj);
	}

	static public GameObject[] RecyclingAllGameObjectsWithPool(this string key)
    {
        return shaco.PrefabPool.RecyclingAllObjects(key);
    }

    static public GameObject[] RecyclingAllGameObjectsWithPool(this GameObject obj)
    {
        return shaco.PrefabPool.RecyclingAllObjects(obj.ToString());
    }

	static public void DestroyWithPool(this GameObject obj)
	{
		shaco.PrefabPool.DestroyObject(obj);
	}

	static public void DestroyAllGameObjectsWithPool(this string key)
	{
		shaco.PrefabPool.DestroyAllObjects(key);
	}

    static public void DestroyAllGameObjectsWithPool(this GameObject obj)
    {
        shaco.PrefabPool.DestroyAllObjects(obj.ToString());
    }

}
