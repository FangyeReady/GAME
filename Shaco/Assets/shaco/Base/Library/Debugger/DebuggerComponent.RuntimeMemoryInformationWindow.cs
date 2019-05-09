//------------------------------------------------------------
// Game Framework v3.x
// Copyright © 2013-2017 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:i@jiangyin.me
//------------------------------------------------------------

#if DEBUG_WINDOW
using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace UnityGameFramework.Runtime
{
    public partial class DebuggerComponent
    {
        private sealed partial class RuntimeMemoryInformationWindow<T> : ScrollableDebuggerWindowBase where T : UnityEngine.Object
        {
            private const int ShowSampleCount = 300;

            private DateTime m_SampleTime = DateTime.MinValue;
            private long m_SampleSize = 0;
            private long m_DuplicateSampleSize = 0;
            private int m_DuplicateSimpleCount = 0;
            private List<Sample> m_Samples = new List<Sample>();

            protected override void OnDrawScrollableWindow()
            {
                float widthGUITmp = DebuggerComponent.DefaultWindowRect.width / 16;

                string typeName = typeof(T).Name;
                GUILayout.Label(string.Format("{0} Runtime Memory Information", typeName));
                GUILayout.BeginVertical("box");
                {
                    if (GUILayout.Button(string.Format("Take Sample for {0}", typeName), GUILayout.Height(DefaultWindowRect.height / 16)))
                    {
                        TakeSample();
                    }

                    if (m_SampleTime <= DateTime.MinValue)
                    {
                        GUILayout.Label(string.Format("Please take sample for {0} first.", typeName));
                    }
                    else
                    {
                        if (m_DuplicateSimpleCount > 0)
                        {
                            GUILayout.Label(string.Format("{0} {1}s ({2}) obtained at {3}, while {4} {1}s ({5}) might be duplicated.", m_Samples.Count.ToString(), typeName, GetSizeString(m_SampleSize), m_SampleTime.ToString("yyyy-MM-dd HH:mm:ss"), m_DuplicateSimpleCount.ToString(), GetSizeString(m_DuplicateSampleSize)));
                        }
                        else
                        {
                            GUILayout.Label(string.Format("{0} {1}s ({2}) obtained at {3}.", m_Samples.Count.ToString(), typeName, GetSizeString(m_SampleSize), m_SampleTime.ToString("yyyy-MM-dd HH:mm:ss")));
                        }

                        if (m_Samples.Count > 0)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(string.Format("{0} Name", typeName));
                                GUILayout.Label("Type", GUILayout.Width(widthGUITmp * 3));
                                GUILayout.Label("Size", GUILayout.Width(widthGUITmp * 3));
                            }
                            GUILayout.EndHorizontal();
                        }

                        int count = 0;
                        for (int i = 0; i < m_Samples.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(m_Samples[i].Highlight ? string.Format("<color=yellow>{0}</color>", m_Samples[i].Name) : m_Samples[i].Name);
                                GUILayout.Label(m_Samples[i].Highlight ? string.Format("<color=yellow>{0}</color>", m_Samples[i].Type) : m_Samples[i].Type, GUILayout.Width(widthGUITmp * 3));
                                GUILayout.Label(m_Samples[i].Highlight ? string.Format("<color=yellow>{0}</color>", GetSizeString(m_Samples[i].Size)) : GetSizeString(m_Samples[i].Size), GUILayout.Width(widthGUITmp * 3));
                            }
                            GUILayout.EndHorizontal();

                            count++;
                            if (count >= ShowSampleCount)
                            {
                                break;
                            }
                        }
                    }
                }
                GUILayout.EndVertical();
            }

            private void TakeSample()
            {
                m_SampleTime = DateTime.Now;
                m_SampleSize = 0L;
                m_DuplicateSampleSize = 0L;
                m_DuplicateSimpleCount = 0;
                m_Samples.Clear();

                T[] samples = Resources.FindObjectsOfTypeAll<T>();
                for (int i = 0; i < samples.Length; i++)
                {
                    long sampleSize = 0L;
#if UNITY_5_6_OR_NEWER
                    sampleSize = Profiler.GetRuntimeMemorySizeLong(samples[i]);
#else
                    sampleSize = Profiler.GetRuntimeMemorySize(samples[i]);
#endif
                    m_SampleSize += sampleSize;
                    m_Samples.Add(new Sample(samples[i].name, samples[i].GetType().Name, sampleSize));
                }

                m_Samples.Sort(SampleComparer);

                for (int i = 1; i < m_Samples.Count; i++)
                {
                    if (m_Samples[i].Name == m_Samples[i - 1].Name && m_Samples[i].Type == m_Samples[i - 1].Type && m_Samples[i].Size == m_Samples[i - 1].Size)
                    {
                        m_Samples[i].Highlight = true;
                        m_DuplicateSampleSize += m_Samples[i].Size;
                        m_DuplicateSimpleCount++;
                    }
                }
            }

            private string GetSizeString(long size)
            {
                if (size < 1024L)
                {
                    return string.Format("{0} Bytes", size.ToString());
                }

                if (size < 1024L * 1024L)
                {
                    return string.Format("{0} KB", (size / 1024f).ToString("F2"));
                }

                if (size < 1024L * 1024L * 1024L)
                {
                    return string.Format("{0} MB", (size / 1024f / 1024f).ToString("F2"));
                }

                if (size < 1024L * 1024L * 1024L * 1024L)
                {
                    return string.Format("{0} GB", (size / 1024f / 1024f / 1024f).ToString("F2"));
                }

                return string.Format("{0} TB", (size / 1024f / 1024f / 1024f / 1024f).ToString("F2"));
            }

            private int SampleComparer(Sample a, Sample b)
            {
                int result = b.Size.CompareTo(a.Size);
                if (result != 0)
                {
                    return result;
                }

                result = a.Type.CompareTo(b.Type);
                if (result != 0)
                {
                    return result;
                }

                return a.Name.CompareTo(b.Name);
            }
        }
    }
}
#endif