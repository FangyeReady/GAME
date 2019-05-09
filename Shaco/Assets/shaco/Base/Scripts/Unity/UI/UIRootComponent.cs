using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using shaco.Base;

//-------------------------------------------------------------------------
//UI LifeCircle: OnUIInit -> OnUIOpen -> [Loop] -> OnUIClose
//                                         |
//                              OnUIResume -> OnUIHide
//                                   |           |
//                                   |     <-    |
//-------------------------------------------------------------------------

namespace shaco
{
    [DisallowMultipleComponent]
    public partial class UIRootComponent : MonoBehaviour
    {
        [SerializeField]
        private partial class UIState : MonoBehaviour, IUIState
        {
            public string key { get { return _key; } }
            public GameObject parent { get { return _parent; } }
            public System.Collections.ObjectModel.ReadOnlyCollection<UIPrefab> prefabs { get { return _prefabs.AsReadOnly(); } }
            public UIRootComponent uiRoot { get { return _uiRoot; } }
            public UIEvent uiEvent { get { return _uiEvent; } }

            public string _key = null;
            public GameObject _parent = null;
            [SerializeField]
            public List<UIPrefab> _prefabs = new List<UIPrefab>();
            public UIRootComponent _uiRoot = null;
            public UIEvent _uiEvent = new UIEvent();
        }

        private class UIPreLoadInfo
        {
            public UIState uiState = null;
            public List<UIPrefab> uiPrefabs = new List<UIPrefab>();
        }

        public int layerIndex
        {
            get { return _layerIndex; }
        }

        public int Count
        {
            get { return _uiDictionary.Count; }
        }

        [SerializeField]
        private int _layerIndex = 0;

        private Dictionary<string, UIState> _uiDictionary = new Dictionary<string, UIState>();
        private Dictionary<string, UIPreLoadInfo> _uiPreLoadCache = new Dictionary<string, UIPreLoadInfo>();
        private GameObject _uiPreLoadCacheParent = null;
        private IUIDepthChange _defaultUIDepthChange = new DefaultUIDepthChange();
        private IUIDepthChange _uiDepthChangeDelegate = null;

        void Awake()
        {
            UIManager.AddUIRootComponent(this);
        }

        void OnDestroy()
        {
            UIManager.RemoveUIRootComponent(this);
        }

        public T OpenUI<T>(bool allowedDuplicate, string multiVersionControlRelativePath, BaseEventArg arg = null) where T : UnityEngine.Component
        {
            GameObject retValue = null;
            var key = shaco.Base.Utility.ToTypeString<T>();

            UIState uiState = null;

            //if has been added, only set activie
            if (_uiDictionary.ContainsKey(key))
            {
                uiState = (UIState)_uiDictionary[key];

                bool isHided = !uiState.parent.activeInHierarchy;
                uiState.parent.SetActive(true);

                retValue = uiState.prefabs[0].prefab;

                if (isHided)
                {
                    uiState.uiEvent.DispatchEvent(uiState, arg, UIEvent.EventType.OnResume);
                }
                else
                {
                    if (allowedDuplicate)
                    {
                        retValue = OpenDuplicateUI(uiState, key, multiVersionControlRelativePath, arg).prefab;
                    }
                }
            }
            else
            {
                uiState = CreateUIStateFromLoadOrCahce(key);
                AddUIState(uiState);
                BindUIStatePrefab(uiState, multiVersionControlRelativePath);
                if (ChangeUIToScene(uiState, arg))
                {
                    retValue = uiState.prefabs[0].prefab;
                }
            }

            ChangeDepthAsTopDisplay(uiState.parent.transform);

            return retValue == null ? null : retValue.GetComponent<T>();
        }

        public void HideUI<T>(BaseEventArg arg = null) where T : UnityEngine.Component
        {
            var uiState = GetUIState<T>();
            if (uiState == null)
            {
                Log.Error("UIRootComponent HideUI error: ui is missing, key=" + Utility.ToTypeString<T>());
                return;
            }
            HideUIBase(uiState, arg);
        }

        public void CloseUI<T>(BaseEventArg arg = null) where T : UnityEngine.Component
        {
            var uiState = GetUIState<T>();
            if (uiState == null)
            {
                Log.Error("UIRootComponent CloseUI error: ui is missing, key=" + Utility.ToTypeString<T>());
                return;
            }
            CloseUIBase(uiState, arg);
        }

        public IUIState OnCustomUI(BaseEventArg arg = null)
        {
            UIState topValidUIState = null;

            for (int i = this.transform.childCount - 1; i >= 0; --i)
            {
                var uiStateTmp = this.transform.GetChild(i).GetComponent<UIState>();

                if (null != uiStateTmp)
                {
                    for (int j = uiStateTmp.prefabs.Count - 1; j >= 0; --j)
                    {
                        var prefabTmp = uiStateTmp.prefabs[j];
                        if (prefabTmp.prefab.activeInHierarchy && !prefabTmp.methodOnCustom.IsNullOrEmpty())
                        {
                            topValidUIState = uiStateTmp;
                            topValidUIState.uiEvent.DispatchEvent(topValidUIState.key, uiStateTmp.prefabs[j], arg, UIEvent.EventType.OnCustom);
                            break;
                        }
                    }
                }


                if (null != topValidUIState)
                {
                    break;
                }
            }

            return topValidUIState;
        }

        public void PreLoadUI<T>(BaseEventArg arg = null, string multiVersionControlRelativePath = "", int preloadCount = 1) where T : UnityEngine.Component
        {
            var key = Utility.ToTypeString<T>();
            UIPreLoadInfo uiPreLoadFind = null;

            //check parent
            if (null == _uiPreLoadCacheParent)
            {
                _uiPreLoadCacheParent = new GameObject("UIPrefabPreLoadCache");
                UnityHelper.ChangeParentLocalPosition(_uiPreLoadCacheParent, this.gameObject);
                _uiPreLoadCacheParent.transform.SetAsFirstSibling();
            }

            //check ui state
            if (!_uiPreLoadCache.TryGetValue(key, out uiPreLoadFind))
            {
                uiPreLoadFind = new UIPreLoadInfo();
                uiPreLoadFind.uiState = CreateUIState(key);
                uiPreLoadFind.uiState.gameObject.SetActive(false);
                UnityHelper.ChangeParentLocalPosition(uiPreLoadFind.uiState.parent, _uiPreLoadCacheParent);
                _uiPreLoadCache.Add(key, uiPreLoadFind);
            }

            //check ui prefab
            for (int i = 0; i < preloadCount; ++i)
            {
                var uiPrefab = CreateUIPrefab(key, multiVersionControlRelativePath);
                if (null != uiPrefab && null != uiPrefab.prefab)
                {
                    uiPreLoadFind.uiPrefabs.Add(uiPrefab);

                    uiPrefab.prefab.SetActive(false);
                    shaco.UnityHelper.ChangeParentLocalPosition(uiPrefab.prefab, _uiPreLoadCacheParent);
                    uiPreLoadFind.uiState.uiEvent.DispatchEvent(key, uiPrefab, arg, UIEvent.EventType.OnPreLoad);
                }
                else 
                {
                    _uiPreLoadCache.Remove(key);
                }
            }
        }

        public int GetPreLoadUICount<T>() where T : UnityEngine.Component
        {
            var key = Utility.ToTypeString<T>();
            UIPreLoadInfo uiPreLoadFind = null;
            _uiPreLoadCache.TryGetValue(key, out uiPreLoadFind);
            return uiPreLoadFind == null ? 0 : uiPreLoadFind.uiPrefabs.Count;
        }

        public IUIState PopupUIAndHide(params System.Type[] igoreUIs)
        {
            return PopupUIAndHide(null, igoreUIs);
        }

        public IUIState PopupUIAndHide(BaseEventArg arg, params System.Type[] igoreUIs)
        {
            UIState retValue = PopupUI(igoreUIs);
            if (null == retValue) return retValue;

            HideUIBase(retValue, arg);
            return retValue;
        }

        public IUIState PopupUIAndClose(BaseEventArg arg, params System.Type[] igoreUIs)
        {
            UIState retValue = PopupUI(igoreUIs);
            if (null == retValue) return retValue;

            CloseUIBase(retValue, arg);
            return retValue;
        }

        public IUIState GetTopUI(bool isIgnoreActived)
        {
            UIState retValue = null;
            for (int i = this.transform.childCount - 1; i >= 0; --i)
            {
                var uiStateTarget = this.transform.GetChild(i);
                if (isIgnoreActived || uiStateTarget.gameObject.activeInHierarchy)
                {
                    retValue = uiStateTarget.GetComponent<UIState>();
                    break;
                }
            }
            return retValue;
        }

        public IUIState GetUIState<T>() where T : UnityEngine.Component
        {
            var key = Utility.ToTypeString<T>();
            return GetUIState(key);
        }

        public List<IUIState> GetAllUIState()
        {
            var retValue = new List<IUIState>();

            foreach (var iter in _uiDictionary)
            {
                retValue.Add(iter.Value);
            }
            return retValue;
        }

        public void Foreach(System.Func<IUIState, bool> callback)
        {
            foreach (var iter in _uiDictionary)
            {
                if (!callback(iter.Value))
                {
                    break;
                }
            }
        }

        public void ClearUI()
        {
            foreach (var iter in _uiDictionary)
            {
                Destroy(iter.Value.parent.gameObject);
            }
            _uiDictionary.Clear();
        }

        public void SetUIDepthChangeDelegate(IUIDepthChange del)
        {
            _uiDepthChangeDelegate = del;
        }

        private void ChangeDepthAsTopDisplay(Transform target)
        {
            _defaultUIDepthChange.ChangeDepthAsTopDisplay(target);
            if (null != _uiDepthChangeDelegate)
            {
                _uiDepthChangeDelegate.ChangeDepthAsTopDisplay(target);
            }
        }

        private IUIState GetUIState(string key)
        {
            return _uiDictionary.ContainsKey(key) ? _uiDictionary[key] : null;
        }

        private UIState PopupUI(params System.Type[] igoreUI)
        {
            UIState retValue = null;
            var childount = this.transform.childCount;
            for (int i = childount - 1; i >= 0; --i)
            {
                var uiStateTarget = this.transform.GetChild(i);
                if (uiStateTarget.gameObject.activeInHierarchy)
                {
                    bool isIgnoreUI = false;
                    var uiStateTmp = uiStateTarget.GetComponent<UIState>();

                    if (null != uiStateTmp)
                    {
                        for (int j = igoreUI.Length - 1; j >= 0; --j)
                        {
                            if (uiStateTmp.key == igoreUI[j].ToTypeString())
                            {
                                isIgnoreUI = true;
                                break;
                            }
                        }
                    }

                    if (!isIgnoreUI)
                    {
                        retValue = uiStateTmp;
                        break;
                    }
                }
            }
            return retValue;
        }

        private void HideUIBase(IUIState uiState, BaseEventArg arg)
        {
            if (null == uiState) return;

            uiState.uiEvent.DispatchEvent(uiState, arg, UIEvent.EventType.OnHide);
            uiState.parent.SetActive(false);
        }

        private void CloseUIBase(IUIState uiState, BaseEventArg arg)
        {
            if (null == uiState) return;

            uiState.uiEvent.DispatchEvent(uiState, arg, UIEvent.EventType.OnClose);
            RemoveUI(uiState);
        }

        private UIState CreateUIStateFromLoadOrCahce(string key)
        {
            UIState retValue = (UIState)GetUIState(key);

            if (null == retValue)
            {
                UIPreLoadInfo uiPreLoadFind = null;
                if (!_uiPreLoadCache.TryGetValue(key, out uiPreLoadFind))
                {
                    retValue = CreateUIState(key);
                }
                else
                {
                    retValue = uiPreLoadFind.uiState;
                }
                retValue.parent.SetActive(true);
            }
            else
            {
                shaco.Log.Info("UIRootComponent CreateUIStateFromLoadOrCahce error: has created ui, key=" + key);
            }
            return retValue;
        }

        private UIState CreateUIState(string key)
        {
            UIState retValue = null;
            var parentNew = new GameObject();
            retValue = parentNew.AddComponent<UIState>();

            parentNew.name = key;
            retValue._key = key;
            retValue._parent = parentNew;
            retValue._uiRoot = this;
            shaco.UnityHelper.ChangeParentLocalPosition(retValue.parent, this.gameObject);

            return retValue;
        }

        private UIPrefab BindUIStatePrefab(UIState uiState, string multiVersionControlRelativePath)
        {
            var retValue = CreateUIPrefabFromLoadOrCache(uiState.key, multiVersionControlRelativePath);
            if (null == retValue)
            {
                Log.Error("UIRootComponent BindUIStatePrefab error: can't creat ui, key=" + uiState.key);
                return retValue;
            }

            uiState._prefabs.Add(retValue);
            return retValue;
        }

        private bool ChangeUIToScene(UIState uiState, BaseEventArg arg)
        {
            if (!uiState.uiEvent.DispatchEvent(uiState, arg, UIEvent.EventType.OnInit))
                return false;
                
            shaco.UnityHelper.ChangeParentLocalPosition(uiState.parent, this.gameObject);
            bool hadValidUI = false;

            for (int i = uiState.prefabs.Count - 1; i >= 0; --i)
            {
                if (null != uiState.prefabs[i].prefab)
                {
                    shaco.UnityHelper.ChangeParentLocalPosition(uiState.prefabs[i].prefab.gameObject, uiState.parent);
                    hadValidUI = true;
                }
            }
            uiState.uiEvent.DispatchEvent(uiState, arg, UIEvent.EventType.OnOpen);
            return hadValidUI;
        }

        private UIPrefab CreateUIPrefabFromLoadOrCache(string key, string multiVersionControlRelativePath)
        {
            if (_uiPreLoadCache.ContainsKey(key))
            {
                var uiCacheTmp = _uiPreLoadCache[key];
                var retValue = uiCacheTmp.uiPrefabs[uiCacheTmp.uiPrefabs.Count - 1];
                uiCacheTmp.uiPrefabs.RemoveAt(uiCacheTmp.uiPrefabs.Count - 1);

                retValue.prefab.SetActive(true);

                if (uiCacheTmp.uiPrefabs.Count == 0)
                {
                    _uiPreLoadCache.Remove(key);
                }
                return retValue;
            }
            else
            {
                return CreateUIPrefab(key, multiVersionControlRelativePath);
            }
        }

        private UIPrefab CreateUIPrefab(string key, string multiVersionControlRelativePath)
        {
            var prefabPath = UIManagerConfig.GetFullPrefabPath(key);
            var resourcesPathTmp = "Resources/";
            var retValue = new UIPrefab(null);

            var pathTmp = prefabPath;
            int indexFind = pathTmp.IndexOf(resourcesPathTmp);
            if (indexFind >= 0)
            {
                pathTmp = pathTmp.Remove(0, indexFind + resourcesPathTmp.Length);
            }

            var newPrefab = shaco.ResourcesEx.LoadResourcesOrLocal<GameObject>(pathTmp, multiVersionControlRelativePath);
            if (null != newPrefab)
            {
                var componets = ((GameObject)MonoBehaviour.Instantiate(newPrefab)).GetComponents<UnityEngine.Component>();
                retValue = new UIPrefab(componets);
            }
            return retValue;
        }

        private bool AddUIState(UIState uiState)
        {
            if (_uiDictionary.ContainsKey(uiState.key))
            {
                Log.Error("UIRootComponent AddUI error: the ui has been added, key=" + uiState.key);
                return false;
            }
            _uiDictionary.Add(uiState.key, uiState);
            return true;
        }

        private void RemoveUI(IUIState uiState)
        {
            uiState.uiEvent.RestStatckLocations();

            _uiDictionary.Remove(uiState.key);
            uiState.parent.gameObject.SetActive(false);
            UnityHelper.SafeDestroy(uiState.parent.gameObject);

            UIPreLoadInfo uiPreLoadFind = null;
            if (_uiPreLoadCache.TryGetValue(uiState.key, out uiPreLoadFind))
            {
                for (int i = uiPreLoadFind.uiPrefabs.Count - 1; i >= 0; --i)
                {
                    UnityHelper.SafeDestroy(uiPreLoadFind.uiPrefabs[i].prefab);
                }
                uiPreLoadFind.uiPrefabs.Clear();
                _uiPreLoadCache.Remove(uiState.key);
            }

            CheckAutoAssetBundlesRemove((UIState)uiState);
            
            System.GC.Collect();
        }
    }
}