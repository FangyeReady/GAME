using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public partial class UIRootComponent : MonoBehaviour
    {
        public void HideUITarget<T>(GameObject target, shaco.Base.BaseEventArg arg = null) where T : UnityEngine.Component
        {
            var key = typeof(T).ToTypeString();
            var uiState = GetUIState(key);
            if (uiState == null)
            {
                Log.Error("UIRootComponent HideUITarget error: ui is missing, key=" + key);
                return;
            }
            HideUITargetBase<T>(uiState, target, arg);
        }

        public void CloseUITarget<T>(GameObject target, shaco.Base.BaseEventArg arg = null) where T : UnityEngine.Component
        {
            var key = typeof(T).ToTypeString();
            var uiState = GetUIState(key);
            if (uiState == null)
            {
                Log.Error("UIRootComponent CloseUITarget error: ui is missing, key=" + key);
                return;
            }
            CloseUITargetBase<T>((UIState)uiState, target, arg);
        }

        private UIPrefab OpenDuplicateUI(UIState uiState, string key, string multiVersionControlRelativePath, shaco.Base.BaseEventArg arg)
        {
            UIPrefab retValue = null;
            retValue = CreateUIPrefabFromLoadOrCache(key, multiVersionControlRelativePath);

            if (null == retValue)
            {
                Log.Error("UIRootComponent OpenDuplicateUI error: can't creat ui, key=" + key);
                return retValue;
            }

            uiState._prefabs.Add(retValue);

            ChangeUITargetToScene(uiState, retValue, arg, uiState.parent);

            retValue.prefab.SetActive(true);

            return retValue;
        }

        private void HideUITargetBase<T>(IUIState uiState, GameObject target, shaco.Base.BaseEventArg arg) where T : UnityEngine.Component
        {
            if (null == uiState) return;

            var findUITarget = GetUITargetInUIState<T>(uiState, target);
            if (null != findUITarget)
            {
                var key = typeof(T).ToTypeString();
                uiState.uiEvent.DispatchEvent(key, findUITarget, arg, UIEvent.EventType.OnHide);
                findUITarget.prefab.SetActive(false);
            }
        }

        private void CloseUITargetBase<T>(UIState uiState, GameObject target, shaco.Base.BaseEventArg arg) where T : UnityEngine.Component
        {
            if (null == uiState) return;

            var findUITarget = GetUITargetInUIState<T>(uiState, target);
            if (null != findUITarget)
            {
                var key = typeof(T).ToTypeString();
                uiState.uiEvent.DispatchEvent(key, findUITarget, arg, UIEvent.EventType.OnClose);
                RemoveUITarget(uiState, findUITarget);
            }
        }

        private UIPrefab GetUITargetInUIState<T>(IUIState uiState, GameObject target) where T : UnityEngine.Component
        {
            UIPrefab retValue = null;
            if (null == uiState)
            {
                return retValue;
            }

            for (int i = uiState.prefabs.Count - 1; i >= 0; --i)
            {
                var prefabTmp = uiState.prefabs[i].prefab;
                if (prefabTmp == target.gameObject)
                {
                    retValue = uiState.prefabs[i];
                    break;
                }
            }
            return retValue;
        }

        private void RemoveUITarget(UIState uiState, UIPrefab uiPrefab)
        {
            for (int i = uiState.prefabs.Count - 1; i >= 0; --i)
            {
                var prefabTmp = uiState.prefabs[i];
                if (prefabTmp == uiPrefab)
                {
                    prefabTmp.prefab.gameObject.SetActive(false);
                    MonoBehaviour.Destroy(prefabTmp.prefab.gameObject);
                    uiState._prefabs.RemoveAt(i);
                    break;
                }
            }

            if (uiState.prefabs.Count == 0)
            {
                RemoveUI(uiState);
            }
        }

        private void ChangeUITargetToScene(UIState uiState, UIPrefab newPrefab, shaco.Base.BaseEventArg arg, GameObject parent)
        {
            uiState.uiEvent.DispatchEvent(uiState.key, newPrefab, arg, UIEvent.EventType.OnInit);
            shaco.UnityHelper.ChangeParentLocalPosition(newPrefab.prefab, parent);
            uiState.uiEvent.DispatchEvent(uiState.key, newPrefab, arg, UIEvent.EventType.OnOpen);
        }
    }
}