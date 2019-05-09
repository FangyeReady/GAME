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
        private sealed class OperationSettingsWindow : ScrollableDebuggerWindowBase
        {
            protected override void OnDrawScrollableWindow()
            {
                float heightGUITmp = DefaultWindowRect.height / 18;

                GUILayout.Label("Operation Settings");
                GUILayout.BeginVertical("box");
                {
                    ObjectPoolComponent objectPoolComponent = GameEntry.GetComponent<ObjectPoolComponent>();
                    if (objectPoolComponent != null)
                    {
                        if (GUILayout.Button("Object Pool Release", GUILayout.Height(heightGUITmp)))
                        {
                            objectPoolComponent.Release();
                        }

                        if (GUILayout.Button("Object Pool Release All Unused", GUILayout.Height(heightGUITmp)))
                        {
                            objectPoolComponent.ReleaseAllUnused();
                        }
                    }
                    
                    if (GUILayout.Button("Unload Unused Assets", GUILayout.Height(heightGUITmp)))
                    {
                        Resources.UnloadUnusedAssets();
                    }

                    if (GUILayout.Button("Shutdown Game Framework (None)", GUILayout.Height(heightGUITmp)))
                    {
                        GameEntry.Shutdown(ShutdownType.None);
                    }
                    if (GUILayout.Button("Shutdown Game Framework (Restart)", GUILayout.Height(heightGUITmp)))
                    {
                        GameEntry.Shutdown(ShutdownType.Restart);
                    }
                    if (GUILayout.Button("Shutdown Game Framework (Quit)", GUILayout.Height(heightGUITmp)))
                    {
                        GameEntry.Shutdown(ShutdownType.Quit);
                    }
                }
                GUILayout.EndVertical();
            }
        }
    }
}
#endif