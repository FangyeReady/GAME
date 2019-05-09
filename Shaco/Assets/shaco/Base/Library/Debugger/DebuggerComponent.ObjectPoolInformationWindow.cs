//------------------------------------------------------------
// Game Framework v3.x
// Copyright © 2013-2017 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//------------------------------------------------------------

#if DEBUG_WINDOW
using GameFramework;
using GameFramework.ObjectPool;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public partial class DebuggerComponent
    {
        private sealed class ObjectPoolInformationWindow : ScrollableDebuggerWindowBase
        {
            private ObjectPoolComponent m_ObjectPoolComponent = null;

            public override void Initialize(params object[] args)
            {
                m_ObjectPoolComponent = GameEntry.GetComponent<ObjectPoolComponent>();

                if (m_ObjectPoolComponent == null)
                {
                    Log.Fatal("Object pool component is invalid.");
                    return;
                }
            }

            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("Object Pool Information");
                GUILayout.BeginVertical("box");
                {
                    DrawItem("Object Pool Count", m_ObjectPoolComponent.Count.ToString());
                }
                GUILayout.EndVertical();
                ObjectPoolBase[] objectPools = m_ObjectPoolComponent.GetAllObjectPools(true);
                for (int i = 0; i < objectPools.Length; i++)
                {
                    DrawObjectPool(objectPools[i]);
                }
            }

            private void DrawObjectPool(ObjectPoolBase objectPool)
            {
                GUILayout.Label(string.Format("Object Pool: {0}", string.IsNullOrEmpty(objectPool.Name) ? "<Unnamed>" : objectPool.Name));
                GUILayout.BeginVertical("box");
                {
                    DrawItem("Type", objectPool.ObjectType.FullName);
                    DrawItem("Auto Release Interval", objectPool.AutoReleaseInterval.ToString());
                    DrawItem("Capacity", string.Format("{0} / {1} / {2}", objectPool.CanReleaseCount.ToString(), objectPool.Count.ToString(), objectPool.Capacity.ToString()));
                    DrawItem("Expire Time", objectPool.ExpireTime.ToString());
                    DrawItem("Priority", objectPool.Priority.ToString());
                    ObjectInfo[] objectInfos = objectPool.GetAllObjectInfos();
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Name");
                        GUILayout.Label("Locked");
                        GUILayout.Label(objectPool.AllowMultiSpawn ? "Count" : "In Use");
                        GUILayout.Label("Priority");
                        GUILayout.Label("Last Use Time");
                    }
                    GUILayout.EndHorizontal();

                    if (objectInfos.Length > 0)
                    {
                        for (int i = 0; i < objectInfos.Length; i++)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(objectInfos[i].Name);
                                GUILayout.Label(objectInfos[i].Locked.ToString());
                                GUILayout.Label(objectPool.AllowMultiSpawn ? objectInfos[i].SpawnCount.ToString() : objectInfos[i].IsInUse.ToString());
                                GUILayout.Label(objectInfos[i].Priority.ToString());
                                GUILayout.Label(objectInfos[i].LastUseTime.ToString("yyyy-MM-dd HH:mm:ss"));
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                    else
                    {
                        GUILayout.Label("<i>Object Pool is Empty ...</i>");
                    }
                }
                GUILayout.EndVertical();
            }
        }
    }
}
#endif