using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class UIManagerConfig
    {
        private string _prefabPath = "Assets/Resources/";

        static public string GetFullPrefabPath(string key)
        {
            var instanceTmp = GameEntry.GetInstance<UIManagerConfig>();
            var retValue = KeyToPath(shaco.Base.FileHelper.ContactPath(instanceTmp._prefabPath, key));
            return retValue;
        }

        static public string KeyToPath(string key)
        {
            return key.Replace('.', '/');
        }

        static public void SetPrefabPath(string path)
        {
            GameEntry.GetInstance<UIManagerConfig>()._prefabPath = path;
        }

        static public string GetPrefabPath()
        {
            return GameEntry.GetInstance<UIManagerConfig>()._prefabPath;
        }
    }
}

