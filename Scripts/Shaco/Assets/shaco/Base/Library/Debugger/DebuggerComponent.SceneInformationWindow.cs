//------------------------------------------------------------
// Game Framework v3.x
// Copyright © 2013-2017 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//------------------------------------------------------------

#if DEBUG_WINDOW
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public partial class DebuggerComponent
    {
        private sealed class SceneInformationWindow : ScrollableDebuggerWindowBase
        {
            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("Scene Information");
                GUILayout.BeginVertical("box");
                {
#if UNITY_5_3_OR_NEWER
                    var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

                    DrawItem("Scene Count:", UnityEngine.SceneManagement.SceneManager.sceneCount.ToString());
                    DrawItem("Scene Count In Build Settings:", UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings.ToString());
                    DrawItem("Active Scene Name:", activeScene.name);
                    DrawItem("Active Scene Path:", activeScene.path);
                    DrawItem("Active Scene Build Index:", activeScene.buildIndex.ToString());
                    DrawItem("Active Scene Is Dirty:", activeScene.isDirty.ToString());
                    DrawItem("Active Scene Is Loaded:", activeScene.isLoaded.ToString());
                    DrawItem("Active Scene Root Count:", activeScene.rootCount.ToString());
#endif
                }
                GUILayout.EndVertical();
            }
        }
    }
}
#endif
