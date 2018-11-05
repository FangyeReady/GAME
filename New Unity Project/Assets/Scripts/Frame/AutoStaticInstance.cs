using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AutoStaticInstance<T> : MonoBehaviour where T: MonoBehaviour
{
    private static MonoBehaviour _instance;

    public static T Instance
    {
        get {

            if (_instance == null)
            {
                GameObject obj = new GameObject(typeof(T).Name);//typeof不是取类型？
                _instance = obj.AddComponent<T>();
                GameObject.DontDestroyOnLoad(obj);  
            }
            return _instance as T;
        }
    }

    public virtual void InitManager()
    {

    }

    public abstract void Save();
}
