﻿//------------------------------------------------------------
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
        private sealed class InputCompassInformationWindow : ScrollableDebuggerWindowBase
        {
            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("Input Compass Information");
                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Enable", GUILayout.Height(DefaultWindowRect.height / 18)))
                        {
                            Input.compass.enabled = true;
                        }
                        if (GUILayout.Button("Disable", GUILayout.Height(DefaultWindowRect.height / 18)))
                        {
                            Input.compass.enabled = false;
                        }
                    }
                    GUILayout.EndHorizontal();

                    DrawItem("Enabled:", Input.compass.enabled.ToString());
                    DrawItem("Heading Accuracy:", Input.compass.headingAccuracy.ToString());
                    DrawItem("Magnetic Heading:", Input.compass.magneticHeading.ToString());
                    DrawItem("Raw Vector:", Input.compass.rawVector.ToString());
                    DrawItem("Timestamp:", Input.compass.timestamp.ToString());
                    DrawItem("True Heading:", Input.compass.trueHeading.ToString());
                }
                GUILayout.EndVertical();
            }
        }
    }
}
#endif