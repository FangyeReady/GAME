//------------------------------------------------------------
// Game Framework v3.x
// Copyright © 2013-2017 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//------------------------------------------------------------

#if DEBUG_WINDOW
using GameFramework;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public partial class DebuggerComponent
    {
        private sealed class GeneralSettingsWindow : ScrollableDebuggerWindowBase
        {
            private DebuggerComponent m_DebuggerComponent = null;
            private SettingComponent m_SettingComponent = null;
            private BaseComponent m_BaseComponent = null;
            // private float m_LastIconX = 0f;
            // private float m_LastIconY = 0f;
            // private float m_LastWindowX = 0f;
            // private float m_LastWindowY = 0f;
            // private float m_LastWindowWidth = 0f;
            // private float m_LastWindowHeight = 0f;
            private float m_LastWindowScale = 0f;

            public override void Initialize(params object[] args)
            {
                m_DebuggerComponent = GameEntry.GetComponent<DebuggerComponent>();
                if (m_DebuggerComponent == null)
                {
                    Log.Fatal("Debugger component is invalid.");
                    return;
                }

                m_SettingComponent = GameEntry.GetComponent<SettingComponent>();
                if (m_SettingComponent == null)
                {
                    Log.Fatal("Setting component is invalid.");
                    return;
                }

                m_BaseComponent = GameEntry.GetComponent<BaseComponent>();
                if (m_BaseComponent == null)
                {
                    Log.Fatal("Base component is invalid.");
                    return;
                }
                // m_LastIconX = m_SettingComponent.GetFloat("Debugger.Icon.X", DefaultIconRect.x);
                // m_LastIconY = m_SettingComponent.GetFloat("Debugger.Icon.Y", DefaultIconRect.y);
                // m_LastWindowX = m_SettingComponent.GetFloat("Debugger.Window.X", DefaultWindowRect.x);
                // m_LastWindowY = m_SettingComponent.GetFloat("Debugger.Window.Y", DefaultWindowRect.y);
                // m_LastWindowWidth = m_SettingComponent.GetFloat("Debugger.Window.Width", DefaultWindowRect.width);
                // m_LastWindowHeight = m_SettingComponent.GetFloat("Debugger.Window.Height", DefaultWindowRect.height);
                m_DebuggerComponent.WindowScale = m_LastWindowScale = m_SettingComponent.GetFloat("Debugger.Window.Scale", DefaultWindowScale);
                // m_DebuggerComponent.IconRect = new Rect(m_LastIconX, m_LastIconY, DefaultIconRect.width, DefaultIconRect.height);
                // m_DebuggerComponent.WindowRect = new Rect(m_LastWindowX, m_LastWindowY, m_LastWindowWidth, m_LastWindowHeight);
            }

            public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
            {
                // if (m_LastIconX != m_DebuggerComponent.IconRect.x)
                // {
                //     m_LastIconX = m_DebuggerComponent.IconRect.x;
                //     m_SettingComponent.SetFloat("Debugger.Icon.X", m_DebuggerComponent.IconRect.x);
                // }

                // if (m_LastIconY != m_DebuggerComponent.IconRect.y)
                // {
                //     m_LastIconY = m_DebuggerComponent.IconRect.y;
                //     m_SettingComponent.SetFloat("Debugger.Icon.Y", m_DebuggerComponent.IconRect.y);
                // }

                // if (m_LastWindowX != m_DebuggerComponent.WindowRect.x)
                // {
                //     m_LastWindowX = m_DebuggerComponent.WindowRect.x;
                //     m_SettingComponent.SetFloat("Debugger.Window.X", m_DebuggerComponent.WindowRect.x);
                // }

                // if (m_LastWindowY != m_DebuggerComponent.WindowRect.y)
                // {
                //     m_LastWindowY = m_DebuggerComponent.WindowRect.y;
                //     m_SettingComponent.SetFloat("Debugger.Window.Y", m_DebuggerComponent.WindowRect.y);
                // }

                // if (m_LastWindowWidth != m_DebuggerComponent.WindowRect.width)
                // {
                //     m_LastWindowWidth = m_DebuggerComponent.WindowRect.width;
                //     m_SettingComponent.SetFloat("Debugger.Window.Width", m_DebuggerComponent.WindowRect.width);
                // }

                // if (m_LastWindowHeight != m_DebuggerComponent.WindowRect.height)
                // {
                //     m_LastWindowHeight = m_DebuggerComponent.WindowRect.height;
                //     m_SettingComponent.SetFloat("Debugger.Window.Height", m_DebuggerComponent.WindowRect.height);
                // }

                if (m_LastWindowScale != m_DebuggerComponent.WindowScale)
                {
                    m_LastWindowScale = m_DebuggerComponent.WindowScale;
                    m_SettingComponent.SetFloat("Debugger.Window.Scale", m_DebuggerComponent.WindowScale);
                }
            }

            protected override void OnDrawScrollableWindow()
            {
                float widthGUITmp = DebuggerComponent.DefaultWindowRect.width / 10;

                GUILayout.Label("Window Settings");
                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Position:");
                        GUILayout.Label("Drag window caption to move position.");
                    }
                    GUILayout.EndHorizontal();

                    // GUILayout.BeginHorizontal();
                    // {
                    //     float width = m_DebuggerComponent.WindowRect.width;
                    //     GUILayout.Label("Width:");
                    //     if (GUILayout.RepeatButton("-"))
                    //     {
                    //         width--;
                    //     }
                    //     width = GUILayout.HorizontalSlider(width, 100f, Screen.width - widthGUITmp / 2);
                    //     if (GUILayout.RepeatButton("+"))
                    //     {
                    //         width++;
                    //     }
                    //     width = Mathf.Clamp(width, 100f, Screen.width - widthGUITmp / 2);
                    //     if (width != m_DebuggerComponent.WindowRect.width)
                    //     {
                    //         // m_DebuggerComponent.WindowRect = new Rect(m_DebuggerComponent.WindowRect.x, m_DebuggerComponent.WindowRect.y, width, m_DebuggerComponent.WindowRect.height);
                    //     }
                    // }
                    // GUILayout.EndHorizontal();

                    // GUILayout.BeginHorizontal();
                    // {
                    //     float height = m_DebuggerComponent.WindowRect.height;
                    //     GUILayout.Label("Height:");
                    //     if (GUILayout.RepeatButton("-"))
                    //     {
                    //         height--;
                    //     }
                    //     height = GUILayout.HorizontalSlider(height, widthGUITmp * 2, Screen.height - widthGUITmp / 2);
                    //     if (GUILayout.RepeatButton("+"))
                    //     {
                    //         height++;
                    //     }
                    //     height = Mathf.Clamp(height, 100f, Screen.height - widthGUITmp / 2);
                    //     if (height != m_DebuggerComponent.WindowRect.height)
                    //     {
                    //         // m_DebuggerComponent.WindowRect = new Rect(m_DebuggerComponent.WindowRect.x, m_DebuggerComponent.WindowRect.y, m_DebuggerComponent.WindowRect.width, height);
                    //     }
                    // }
                    // GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("TimeScale:");

                        GUI.changed = false;
                        var inputSpeed = GUILayout.TextField(m_BaseComponent.GameSpeed.ToString());
                        if (GUI.changed)
                        {
                            float parseValue = 0;
                            if (float.TryParse(inputSpeed, out parseValue))
                            {
                                m_BaseComponent.GameSpeed = parseValue;
                            }
                        }

                        if (GUILayout.Button("-"))
                        {
                            m_BaseComponent.GameSpeed -= 0.5f;
                        }
                        m_BaseComponent.GameSpeed = GUILayout.HorizontalSlider(m_BaseComponent.GameSpeed, 0.1f, 10.0f);
                        if (GUILayout.Button("+"))
                        {
                            m_BaseComponent.GameSpeed += 0.5f;
                        }
                        m_BaseComponent.GameSpeed = Mathf.Clamp(m_BaseComponent.GameSpeed, 0.5f, 10.0f);
                    }
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("Reset Window Settings", GUILayout.Height(widthGUITmp / 2)))
                    {
                        m_DebuggerComponent.ResetDebuggerWindowRect();
                    }
                    if (GUILayout.Button("DeleteAllPlayerPrefs", GUILayout.Height(widthGUITmp / 2)))
                    {
                        PlayerPrefs.DeleteAll();
                    }
                }
                GUILayout.EndVertical();
            }


        }
    }
}
#endif