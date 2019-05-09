using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using shaco.Base;

namespace shaco
{
    public class UIManager
    {
        private List<UIRootComponent> _roots = new List<UIRootComponent>();

        static public T OpenUI<T>(BaseEventArg arg = null) where T : UnityEngine.Component
        {
            var layerAttribute = ComponentTypeToLayerAttribute<T>();
            return GetUIRootComponent(layerAttribute.layerIndex).OpenUI<T>(layerAttribute.allowedDuplicate, layerAttribute.multiVersionControlRelativePath, arg);
        }

        static public void HideUI<T>(BaseEventArg arg = null) where T : UnityEngine.Component
        {
            GetUIRootComponent(ComponentTypeToLayerAttribute<T>().layerIndex).HideUI<T>(arg);
        }

        static public void CloseUI<T>(BaseEventArg arg = null) where T : UnityEngine.Component
        {
            GetUIRootComponent(ComponentTypeToLayerAttribute<T>().layerIndex).CloseUI<T>(arg);
        }

        static public void CustomUI(BaseEventArg arg = null, int layerIndex = 0)
        {
            GetUIRootComponent(layerIndex).OnCustomUI(arg);
        }

        static public void HideUITarget<T>(GameObject target, BaseEventArg arg = null) where T : UnityEngine.Component
        {
            GetUIRootComponent(ComponentTypeToLayerAttribute<T>().layerIndex).HideUITarget<T>(target, arg);
        }

        static public void CloseUITarget<T>(GameObject target, BaseEventArg arg = null) where T : UnityEngine.Component
        {
            GetUIRootComponent(ComponentTypeToLayerAttribute<T>().layerIndex).CloseUITarget<T>(target, arg);
        }

        static public void HideUITarget<T>(UnityEngine.Component target, BaseEventArg arg = null) where T : UnityEngine.Component
        {
            GetUIRootComponent(ComponentTypeToLayerAttribute<T>().layerIndex).HideUITarget<T>(target.gameObject, arg);
        }

        static public void CloseUITarget<T>(UnityEngine.Component target, BaseEventArg arg = null) where T : UnityEngine.Component
        {
            GetUIRootComponent(ComponentTypeToLayerAttribute<T>().layerIndex).CloseUITarget<T>(target.gameObject, arg);
        }

        static public void PreLoadUI<T>(BaseEventArg arg = null, int preloadCount = 1) where T : UnityEngine.Component
        {
            var layerAttribute = ComponentTypeToLayerAttribute<T>();
            GetUIRootComponent(layerAttribute.layerIndex).PreLoadUI<T>(arg, layerAttribute.multiVersionControlRelativePath, preloadCount);
        }

        static public void PreLoadUIOnlyOne<T>(BaseEventArg arg = null) where T : UnityEngine.Component
        {
            //如果之前没有预加载过该ui，则预加载一次
            if (GetPreLoadUICount<T>() == 0)
            {
                PreLoadUI<T>(arg, 1);
            }
        }

        static public int GetPreLoadUICount<T>() where T : UnityEngine.Component
        {
            var layerAttribute = ComponentTypeToLayerAttribute<T>();
            return GetUIRootComponent(layerAttribute.layerIndex).GetPreLoadUICount<T>();
        }

        static public shaco.IUIState PopupUIAndHide(BaseEventArg arg = null, int layerIndex = 0, params System.Type[] igoreUIs)
        {
            return GetUIRootComponent(layerIndex).PopupUIAndHide(arg, igoreUIs);
        }

        static public shaco.IUIState PopupUIAndClose(BaseEventArg arg = null, int layerIndex = 0, params System.Type[] igoreUIs)
        {
            return GetUIRootComponent(layerIndex).PopupUIAndClose(arg, igoreUIs);
        }

        static public void AddUIRootComponent(UIRootComponent uiRoot)
        {
            var instanceTmp = GameEntry.GetInstance<UIManager>();
            if (instanceTmp._roots.Contains(uiRoot))
            {
                Log.Error("UIManager AddUIRootComponent error: The ui root has been added !");
            }
            else
            {
                uiRoot.name = uiRoot.name + "[layer:" + uiRoot.layerIndex + "]";
                instanceTmp.EnsureRootComponentCache(uiRoot.layerIndex);
                if (instanceTmp._roots[uiRoot.layerIndex] != null)
                    Log.Error("UIManager AddUIRootComponent error: The UIRoot has been set at this layer index, uiRoot=" + uiRoot + " layer index=" + uiRoot.layerIndex);
                else
                    instanceTmp._roots[uiRoot.layerIndex] = uiRoot;
            }
        }

        static public void RemoveUIRootComponent(UIRootComponent uiRoot)
        {
            var instanceTmp = GameEntry.GetInstance<UIManager>();
            if (!instanceTmp._roots.Contains(uiRoot))
            {
                if (instanceTmp._roots.Count > 0)
                {
                    Log.Error("UIManager RemoveUIRootComponent error: not find ui root=" + uiRoot);
                }
            }
            else
            {
                instanceTmp._roots.Remove(uiRoot);
            }
        }

        static public shaco.IUIState GetTopUI(bool isIgnoreActived, int layerIndex = 0)
        {
            shaco.IUIState retValue = null;
            var uiRoot = GetUIRootComponent(layerIndex);
            if (null != uiRoot)
            {
                retValue = uiRoot.GetTopUI(isIgnoreActived);
            }
            return retValue;
        }

        static public shaco.IUIState GetUIState<T>() where T : UnityEngine.Component
        {
            shaco.IUIState retValue = null;
            var uiRoot = GetUIRootComponent(ComponentTypeToLayerAttribute<T>().layerIndex);
            if (null != uiRoot)
            {
                retValue = uiRoot.GetUIState<T>();
            }
            return retValue;
        }

        static public T GetUIComponent<T>(bool onlyActivedUI = false) where T : UnityEngine.Component
        {
            T retValue = default(T);
            var uiRoot = GetUIRootComponent(ComponentTypeToLayerAttribute<T>().layerIndex);
            if (null != uiRoot)
            {
                var uiStateTmp = uiRoot.GetUIState<T>();
                if (null != uiStateTmp && uiStateTmp.prefabs.Count > 0)
                {
                    var findUIComponent = uiStateTmp.prefabs[0].prefab.GetComponent<T>();
                    if (!onlyActivedUI || (onlyActivedUI && findUIComponent.gameObject.activeInHierarchy))
                    {
                        retValue = findUIComponent;
                    }
                }
            }
            return retValue;
        }

        static public List<T> GetUIComponents<T>(bool onlyActivedUI = false) where T : UnityEngine.Component
        {
            List<T> retValue = new List<T>();

            var uiRoot = GetUIRootComponent(ComponentTypeToLayerAttribute<T>().layerIndex);
            if (null != uiRoot)
            {
                var uiStateTmp = uiRoot.GetUIState<T>();
                if (null != uiStateTmp)
                {
                    for (int i = 0; i < uiStateTmp.prefabs.Count; ++i)
                    {
                        T componentTmp = uiStateTmp.prefabs[i].prefab.GetComponent<T>();
                        if (null != componentTmp)
                        {
                            if (!onlyActivedUI || (onlyActivedUI && componentTmp.gameObject.activeInHierarchy))
                            {
                                retValue.Add(componentTmp);
                            }
                        }
                    }
                }
            }

            return retValue;
        }

        static public GameObject GetUIGameObjectWithTag(string tag)
        {
            GameObject retValue = null;

            ForeachAllUIGameObject((GameObject obj) =>
            {
                if (obj.tag == tag)
                {
                    retValue = obj;
                    return false;
                }
                else
                    return true;
            });
            return retValue;
        }

        static public List<GameObject> GetUIGameObjectsWithTag(string tag)
        {
            var retValue = new List<GameObject>();

            ForeachAllUIGameObject((GameObject obj) =>
            {
                if (obj.tag == tag)
                {
                    retValue.Add(obj);
                }
                return true;
            });
            return retValue;
        }

        static public GameObject GetUIGameObjectWithLayer(int layer)
        {
            GameObject retValue = null;

            ForeachAllUIGameObject((GameObject obj) =>
            {
                if (obj.layer == layer)
                {
                    retValue = obj;
                    return false;
                }
                else
                    return true;
            });
            return retValue;
        }

        static public List<GameObject> GetUIGameObjectsWithLayer(int layer)
        {
            var retValue = new List<GameObject>();

            ForeachAllUIGameObject((GameObject obj) =>
            {
                if (obj.layer == layer)
                {
                    retValue.Add(obj);
                }
                return true;
            });
            return retValue;
        }

        static public void ForeachActiveUIRoot(System.Action<UIRootComponent> callback)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Log.Warning("UIManager warning: 'ForeachActiveUIRoot' cannot be used when the editor is not running");
                return;
            }
#endif
            var instanceTmp = GameEntry.GetInstance<UIManager>();
            if (instanceTmp._roots.Count == 0)
            {
                // Log.Error("UIManaer error: not found 'UIRootComponent', please check 'UIManager.AddUIRootComponent'");
                return;
            }

            for (int i = instanceTmp._roots.Count - 1; i >= 0; --i)
            {
                var uiRootTmp = instanceTmp._roots[i];
                if (null != uiRootTmp && uiRootTmp.gameObject.activeInHierarchy)
                {
                    callback(uiRootTmp);
                }
            }
        }

        static public int GetUIRootCount()
        {
            return GameEntry.GetInstance<UIManager>()._roots.Count;
        }

        static public UIRootComponent GetUIRootComponent(int layerIndex = 0)
        {
            UIRootComponent retValue = null;
            var instanceTmp = GameEntry.GetInstance<UIManager>();

            if (layerIndex < 0 || layerIndex > instanceTmp._roots.Count - 1)
                Log.Error("UIManager GetUIRootComponent error: out of range index=" + layerIndex + " count=" + instanceTmp._roots.Count);
            else
                retValue = instanceTmp._roots[layerIndex];

            if (null == retValue)
                Log.Error("UIManager GetUIRootComponent error: There is no UIRoot at the current layer index=" + layerIndex);
            return retValue;
        }

        static public void ClearUI()
        {
            ForeachActiveUIRoot((UIRootComponent uiRoot) =>
            {
                uiRoot.ClearUI();
            });
        }

        static public string SetAutoUnloadAssetBundle(UnityEngine.Component target, string pathAssetBundle, bool unloadAllLoadedObjects)
        {
            var layerAttribute = target.GetType().GetAttribute<UILayerAttribute>();
            return GetUIRootComponent(layerAttribute.layerIndex).SetAutoUnloadAssetBundle(target, pathAssetBundle, layerAttribute.multiVersionControlRelativePath, unloadAllLoadedObjects);
        }

        /// <summary>
        /// 设置ui深度变化的委托方法
        /// <param name="del">深度变化的委托方法</param>
        /// <param name="layerIndex">ui根节点下标，当为-1的时候默认设置所有ui根节点</param>
        /// </summary>
        static public void SetUIDepthChangeDelegate(IUIDepthChange del, int layerIndex = -1)
        {
            if (layerIndex < 0)
            {
                var instanceTmp = GameEntry.GetInstance<UIManager>();
                for (int i = instanceTmp._roots.Count - 1; i >= 0; --i)
                {
                    instanceTmp._roots[i].SetUIDepthChangeDelegate(del);
                }
            }
            else 
            {
                GetUIRootComponent(layerIndex).SetUIDepthChangeDelegate(del);
            }
        }

        static private UILayerAttribute ComponentTypeToLayerAttribute<T>() where T : UnityEngine.Component
        {
            return typeof(T).GetAttribute<UILayerAttribute>();
        }

        static private bool ComponentTypeToDuplicateUI<T>() where T : UnityEngine.Component
        {
            var retValue = false;
            var typeComponent = typeof(T);

            if (typeComponent.IsDefined(typeof(UILayerAttribute), false))
            {
                retValue = (typeComponent.GetCustomAttributes(typeof(UILayerAttribute), false)[0] as UILayerAttribute).allowedDuplicate;
            }

            return retValue;
        }

        static private void ForeachAllUIGameObject(System.Func<GameObject, bool> callback)
        {
            var instanceTmp = GameEntry.GetInstance<UIManager>();
            bool needBreak = false;

            for (int i = instanceTmp._roots.Count - 1; i >= 0; --i)
            {
                var rootTmp = instanceTmp._roots[i];
                var allUIStateTmp = rootTmp.GetAllUIState();
                for (int j = allUIStateTmp.Count - 1; j >= 0; --j)
                {
                    var uiStateTmp = allUIStateTmp[j];

                    for (int k = uiStateTmp.prefabs.Count - 1; k >= 0; --k)
                    {
                        var gameObjectTmp = uiStateTmp.prefabs[k].prefab;

                        if (!callback(gameObjectTmp))
                        {
                            needBreak = true;
                        }

                        UnityHelper.ForeachChildren(gameObjectTmp, (int index, GameObject child) =>
                        {
                            if (!callback(child))
                            {
                                needBreak = true;
                                return false;
                            }
                            else
                                return true;
                        });

                        if (needBreak)
                        {
                            return;
                        }
                    }
                }
            }
        }

        private void EnsureRootComponentCache(int layerIndex)
        {
            int addCount = layerIndex - _roots.Count + 1;
            if (addCount > 0)
                _roots.AddRange(new UIRootComponent[addCount]);
        }
    }
}

