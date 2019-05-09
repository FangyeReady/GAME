using UnityEngine;
using System.Collections;

namespace shaco
{
	public class HotUpdateImportInstance
	{
		private HotUpdateImportWWW _updateWWW = new HotUpdateImportWWW();
		private HotUpdateImportMemory _updateMemory = new HotUpdateImportMemory();

        public static HotUpdateImportMemory GetMemory()
		{
			return shaco.Base.GameEntry.GetInstance<HotUpdateImportInstance>()._updateMemory;
		}

		public static HotUpdateImportWWW GetWWW()
		{
			return shaco.Base.GameEntry.GetInstance<HotUpdateImportInstance>()._updateWWW;
		}
	}
}
