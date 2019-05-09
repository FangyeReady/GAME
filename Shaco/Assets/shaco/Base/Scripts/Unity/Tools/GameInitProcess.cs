using UnityEngine;
using System.Collections;

namespace shaco
{
    //如果是Unity5.x以下版本(不含5.x)，则需要将该类作为Componet放置在游戏的启动之初
    public class GameInitProcess : MonoBehaviour
    {
        static private bool _isInited = false;

        void OnApplicationQuit()
        {
            shaco.Base.HttpHelperCache.OnApplicationQuit();
        }

        void Awake()
        {
            InitParams();
            StartApplicationCallBack();
        }

#if UNITY_5_3_OR_NEWER && UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif      
        static private void InitParams()
        {
            shaco.Base.FileDefine.persistentDataPath = Application.persistentDataPath;
            GameEntry.SetInstance<shaco.Base.ILog, shaco.Log>();
        }

#if UNITY_5_3_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        static private void StartApplicationCallBack()
        {
             if (_isInited)
                return;

            _isInited = true;

#if !UNITY_EDITOR
            shaco.Base.FileDefine.persistentDataPath = Application.persistentDataPath;
            GameEntry.SetInstance<shaco.Base.ILog, shaco.Log>();
#endif

#if DEBUG_LOG
            shaco.SceneManager.OpenDebugMode = true;
#else
            shaco.SceneManager.OpenDebugMode = false;
#endif

            GameEntry.GetComponentInstance<GameInitProcess>();
            shaco.Base.GameEntry.GetInstance<shaco.ActionS>();
            shaco.Base.HttpHelperCache.OnApplicationStart();
        }
    }
}
