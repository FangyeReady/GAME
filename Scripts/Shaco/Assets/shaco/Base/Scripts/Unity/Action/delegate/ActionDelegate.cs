using UnityEngine;
using System.Collections;

namespace shaco
{
    public class ActionDelegate : MonoBehaviour
    {
        public bool isDrawDebug = false;

        void Start()
        {
            shaco.Base.GameEntry.GetInstance<ActionS>();
        }

        void OnDestroy()
        {
            HotUpdateDataCache.Unload();
        }

        void Update()
        {
            shaco.Base.GameEntry.GetInstance<ActionS>().MainUpdate(Time.deltaTime);
        }

        void OnGUI()
        {
            if (!isDrawDebug)
                return;
        }
    }
}
