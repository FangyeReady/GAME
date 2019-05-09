using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class ObjectPool
    {
        protected IObjectSpawn _objectSpwanInterface = null;
        protected Dictionary<string, List<object>> _objectsPool = new Dictionary<string, List<object>>();
        protected Dictionary<string, List<object>> _objectsInstantiatePool = new Dictionary<string, List<object>>();
        private Dictionary<object, string> _objectToKeys = new Dictionary<object, string>();

        //从对象池中实例化一个对象
        static public T Instantiate<T>(string key) where T : new()
        {
            return InstantiateBase<T, ObjectPool>(key, string.Empty);
        }

        //回收所有对象以便再次利用
        static public void RecyclingAllObjects(string key)
        {
            RecyclingAllObjectsBase<ObjectPool>(key);
        }

        //回收对象以便再次利用
        static public void RecyclingObject(object obj)
        {
            RecyclingObjectBase<ObjectPool>(obj);
        }

        //卸载所有对象
        static public void DestroyAllObjects(string key)
        {
            DestroyAllObjectsBase<ObjectPool>(key);
        }

        //卸载对象
        static public void DestroyObject(object obj)
        {
            DestroyObjectBase<ObjectPool>(obj);
        }

        //清空所有对象
        static public void Clear()
        {
            GameEntry.GetInstance<ObjectPool>().ClearBase();
        }

        static protected T InstantiateBase<T, POOL>(string key, string multiVersionControlRelativePath = "") where T : new() where POOL : ObjectPool
        {
            return GameEntry.GetInstance<POOL>().InstantiateBase<T>(key, multiVersionControlRelativePath);
        }

        static protected T InstantiateBase<T, POOL>(object obj) where T : new() where POOL : ObjectPool
        {
            return GameEntry.GetInstance<POOL>().InstantiateBase<T>(obj);
        }

        static protected object[] RecyclingAllObjectsBase<POOL>(string key) where POOL : ObjectPool
        {
            var instanceTmp = GameEntry.GetInstance<POOL>();
            var objs = instanceTmp.RemoveObjectFromPool(instanceTmp._objectsInstantiatePool, key, -1);
            if (objs.Length > 0)
            {
                instanceTmp.AddObjectToPool(instanceTmp._objectsPool, key, objs);
                instanceTmp._objectSpwanInterface.RecyclingAllObjects(objs);
            }

            return objs;
        }

        static protected object RecyclingObjectBase<POOL>(object obj) where POOL : ObjectPool
        {
            var instanceTmp = GameEntry.GetInstance<POOL>();
            var key = instanceTmp.ObjectToKey(obj);
            if (null != instanceTmp.RemoveObjectFromPool(instanceTmp._objectsInstantiatePool, key, obj))
            {
                instanceTmp.AddObjectToPool(instanceTmp._objectsPool, key, obj);
                instanceTmp._objectSpwanInterface.RecyclingObject(obj);
            }
            return obj;
        }

        static protected void DestroyAllObjectsBase<POOL>(string key) where POOL : ObjectPool
        {
            var instanceTmp = GameEntry.GetInstance<POOL>();
            var objsWithInstantiate = instanceTmp.RemoveObjectFromPool(instanceTmp._objectsInstantiatePool, key, -1);
            var objsWithCache = instanceTmp.RemoveObjectFromPool(instanceTmp._objectsPool, key, -1);

            if (objsWithInstantiate.Length > 0)
                instanceTmp._objectSpwanInterface.DestroyObjects(objsWithInstantiate);
            if (objsWithCache.Length > 0)
                instanceTmp._objectSpwanInterface.DestroyObjects(objsWithCache);

            System.GC.Collect();
        }

        static protected void DestroyObjectBase<POOL>(object obj) where POOL : ObjectPool
        {
            var instanceTmp = GameEntry.GetInstance<POOL>();
            var key = instanceTmp.ObjectToKey(obj);
            var objsWithInstantiate = instanceTmp.RemoveObjectFromPool(instanceTmp._objectsInstantiatePool, key, obj);
            var objsWithCache = instanceTmp.RemoveObjectFromPool(instanceTmp._objectsPool, key, obj);

            if (null != objsWithInstantiate)
                instanceTmp._objectSpwanInterface.DestroyObject(objsWithInstantiate);
            if (null != objsWithCache)
                instanceTmp._objectSpwanInterface.DestroyObject(objsWithCache);
        }

        protected T InstantiateBase<T>(string key, string multiVersionControlRelativePath = "") where T : new()
        {
            return InstantiateBase(key, () =>
            {
                return _objectSpwanInterface.CreateNewObject<T>(key, multiVersionControlRelativePath);
            });
        }

        protected T InstantiateBase<T>(object obj) where T : new()
        {
            return InstantiateBase(obj.ToString(), () =>
            {
                return _objectSpwanInterface.CreateNewObject<T>(obj);
            });
        }

        protected void ClearBase()
        {
            foreach (var iter in _objectsPool)
            {
                foreach (var obj in iter.Value)
                    _objectSpwanInterface.DestroyObject(obj);
            }
            _objectsPool.Clear();

            foreach (var iter in _objectsInstantiatePool)
            {
                foreach (var obj in iter.Value)
                    _objectSpwanInterface.DestroyObject(obj);
            }
            _objectsInstantiatePool.Clear();

            _objectToKeys.Clear();
            System.GC.Collect();
        }

        private void AddObjectToPool<T>(Dictionary<string, List<object>> objectsPool, string key, T obj)
        {
            AddObjectToPool(objectsPool, key, new T[] { obj });
        }

        private void AddObjectToPool<T>(Dictionary<string, List<object>> objectsPool, string key, T[] objs)
        {
            List<object> objects = null;
            if (!objectsPool.ContainsKey(key))
            {
                objectsPool.Add(key, new List<object>());
            }
            objects = objectsPool[key];

            for (int i = objs.Length - 1; i >= 0; --i)
                objects.Add(objs[i]);
        }

        private object[] RemoveObjectFromPool(Dictionary<string, List<object>> objectsPool, string key, int removeCount)
        {
            object[] retValue = new object[0];
            List<object> objects = null;
            if (objectsPool.TryGetValue(key, out objects))
            {
                if (objects.Count > 0)
                {
                    //should remove all
                    if (removeCount < 0)
                    {
                        removeCount = objects.Count;
                    }

                    int removeEndIndex = objects.Count - 1;
                    int removeStartIndex = objects.Count - removeCount;
                    retValue = new object[removeEndIndex - removeStartIndex + 1];

                    for (int i = removeEndIndex; i >= removeStartIndex; --i)
                    {
                        var removeObjTmp = objects[i];
                        retValue[i - removeStartIndex] = removeObjTmp;
                        if (_objectToKeys.ContainsKey(removeObjTmp))
                        {
                            _objectToKeys.Remove(removeObjTmp);
                        }
                        objects.RemoveAt(i);
                    }
                }

                if (objects.Count == 0)
                {
                    objectsPool.Remove(key);
                }
            }
            return retValue;
        }

        private object RemoveObjectFromPool<T>(Dictionary<string, List<object>> objectsPool, string key, T obj)
        {
            object retValue = null;
            List<object> objects = null;
            if (objectsPool.TryGetValue(key, out objects))
            {
                for (int i = objects.Count - 1; i >= 0; --i)
                {
                    var removeObjTmp = objects[i];
                    if (removeObjTmp == (object)obj)
                    {
                        if (_objectToKeys.ContainsKey(removeObjTmp))
                        {
                            _objectToKeys.Remove(removeObjTmp);
                        }
                        retValue = removeObjTmp;
                        objects.RemoveAt(i);
                        break;
                    }
                }
                if (objects.Count == 0)
                {
                    objectsPool.Remove(key);
                }
            }
            return retValue;
        }

        private T InstantiateBase<T>(string key, System.Func<T> createFunc) where T : new()
        {
            T retValue = default(T);

            if (!_objectsPool.ContainsKey(key))
            {
                retValue = createFunc();
            }
            else
            {
                var objectsList = _objectsPool[key];
                retValue = (T)(object)objectsList[objectsList.Count - 1];
                RemoveObjectFromPool(_objectsPool, key, 1);
                _objectSpwanInterface.ActiveObject(retValue);
            }

            UpdateObjectToKey(retValue, key);
            AddObjectToPool(_objectsInstantiatePool, key, retValue);
            return retValue;
        }

        private void UpdateObjectToKey(object obj, string key)
        {
            _objectToKeys[obj] = key;
        }

        private string ObjectToKey(object obj)
        {
            var retValue = string.Empty;
            _objectToKeys.TryGetValue(obj, out retValue);
            return retValue;
        }

        public ObjectPool()
        {
            _objectSpwanInterface = new ObjectSpwan();
        }
    }
}