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
        private sealed class PathInformationWindow : ScrollableDebuggerWindowBase
        {
            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("Path Information");
                GUILayout.BeginVertical("box");
                {
                    DrawItem("Data Path:", Application.dataPath);
                    DrawItem("Persistent Data Path:", Application.persistentDataPath);
                    DrawItem("Streaming Assets Path:", Application.streamingAssetsPath);
                    DrawItem("Temporary Cache Path:", Application.temporaryCachePath);
                }
                GUILayout.EndVertical();
            }
        }
    }
}
#endif
