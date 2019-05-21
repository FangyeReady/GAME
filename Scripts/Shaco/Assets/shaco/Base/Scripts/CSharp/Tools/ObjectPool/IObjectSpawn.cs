using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
	public interface IObjectSpawn
	{
		T CreateNewObject<T>(string key, string multiVersionControlRelativePath) where T : new();
        T CreateNewObject<T>(object obj) where T : new();

		void ActiveObject<T>(T obj);

		void RecyclingAllObjects<T>(T[] objs);

        void RecyclingObject<T>(T obj);

		void DestroyObjects<T>(T[] objs);

		void DestroyObject<T>(T obj);
	}
}

