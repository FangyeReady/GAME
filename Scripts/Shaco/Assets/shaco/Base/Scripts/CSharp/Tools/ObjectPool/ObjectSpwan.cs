using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class ObjectSpwan : IObjectSpawn
    {
        public T CreateNewObject<T>(string key, string multiVersionControlRelativePath) where T : new()
        {
            return new T();
        }

        public T CreateNewObject<T>(object obj) where T : new()
        {
            return new T();
        }

		public void ActiveObject<T>(T obj)
		{
			///...do nothing
		}

        public void RecyclingAllObjects<T>(T[] objs)
        {
            for (int i = objs.Length - 1; i >= 0; --i)
            {
                RecyclingObject(objs[i]);
            }
        }

        public void RecyclingObject<T>(T obj)
        {
            obj = default(T);
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
            obj = default(T);
        }
    }
}
