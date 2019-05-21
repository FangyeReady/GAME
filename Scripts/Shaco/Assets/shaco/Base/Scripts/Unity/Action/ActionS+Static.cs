using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public partial class ActionS
    {
        private static ActionDelegate _actionDelegate = null;


        static public bool HasAction(GameObject target, ActionS findAction)
        {
            bool ret = false;
            var instanceTmp = shaco.Base.GameEntry.GetInstance<ActionS>();
            instanceTmp._DoActions(target, (ActionS action) =>
                                   {
                                       if (target == action.Target && action == findAction)
                                       {
                                           ret = true;
                                           return false;
                                       }
                                       else
                                           return true;
                                   });

            return ret;
        }

        static public ActionS FindAction(GameObject target, string name)
        {
            ActionS ret = null;
            var instanceTmp = shaco.Base.GameEntry.GetInstance<ActionS>();
            instanceTmp._DoActions(target, (ActionS action) =>
                                   {
                                       if (action.ActionName == name)
                                       {
                                           ret = action;
                                           return false;
                                       }
                                       else
                                           return true;
                                   });

            return ret;
        }

        static public bool HasAction(GameObject target)
        {
            bool ret = false;
            var instanceTmp = shaco.Base.GameEntry.GetInstance<ActionS>();
            instanceTmp._DoActions(target, (ActionS action) =>
                                   {
                                       ret = true;
                                       return false;
                                   });

            return ret;
        }

        static public bool HasActionWithTag(GameObject target, int tag)
        {
            bool ret = false;
            var instanceTmp = shaco.Base.GameEntry.GetInstance<ActionS>();
            instanceTmp._DoActions(target, (ActionS action) =>
                                   {
                                       if (action.Tag == tag)
                                       {
                                           ret = true;
                                           return false;
                                       }
                                       else
                                           return true;
                                   });

            return ret;
        }

        static public void StopAllAction(bool isPlayEndWithDirectly = false)
        {
            var instanceTmp = shaco.Base.GameEntry.GetInstance<ActionS>();
            foreach (var listAction in instanceTmp._mapActions)
            {
                foreach (var action in listAction.Value)
                {
                    if (isPlayEndWithDirectly)
                        action.PlayEndDirectly();
                    AddRemove(action.Target, action);
                }
            }
        }

        static public void StopActions(GameObject target, bool isPlayEndWithDirectly = false)
        {
            var instanceTmp = shaco.Base.GameEntry.GetInstance<ActionS>();
            instanceTmp._DoActions(target, (ActionS action) =>
                                   {
                                       if (isPlayEndWithDirectly)
                                           action.PlayEndDirectly();

                                       AddRemove(target, action);
                                       return true;
                                   });
        }

        static public void StopAction(GameObject target, ActionS action, bool isPlayEndWithDirectly = false)
        {
            var instanceTmp = shaco.Base.GameEntry.GetInstance<ActionS>();
            instanceTmp._DoActions(target, (ActionS actionTmp) =>
                                   {
                                       if (actionTmp == action)
                                       {
                                           if (isPlayEndWithDirectly)
                                               actionTmp.PlayEndDirectly();
                                           AddRemove(target, actionTmp);
                                           return false;
                                       }
                                       else
                                       {
                                           return true;
                                       }
                                   });
        }

        static public void StopAction<T>(GameObject target, bool isPlayEndWithDirectly = false) where T : ActionS
        {
            StopActionByName(target, shaco.Base.Utility.ToTypeString<T>(), isPlayEndWithDirectly);
        }

        static public void StopActionByName(GameObject target, string actionClassName, bool isPlayEndWithDirectly = false)
        {
            var instanceTmp = shaco.Base.GameEntry.GetInstance<ActionS>();
            instanceTmp._DoActions(target, (ActionS action) =>
                                   {
                                       if (action.ActionName == actionClassName)
                                       {
                                           if (isPlayEndWithDirectly)
                                               action.PlayEndDirectly();
                                           AddRemove(target, action);
                                       }
                                       return true;
                                   });
        }

        static public void StopActionByTag(GameObject target, int actionTag, bool isPlayEndWithDirectly = false)
        {
            var instanceTmp = shaco.Base.GameEntry.GetInstance<ActionS>();
            instanceTmp._DoActions(target, (ActionS action) =>
                                   {
                                       if (action.Tag == actionTag)
                                       {
                                           if (isPlayEndWithDirectly)
                                               action.PlayEndDirectly();
                                           AddRemove(target, action);
                                       }
                                       return true;
                                   });
        }

        static public void PauseAction(GameObject target)
        {
            var instanceTmp = shaco.Base.GameEntry.GetInstance<ActionS>();
            instanceTmp._DoActions(target, (ActionS action) =>
                                   {
                                       action.isPaused = true;
                                       return true;
                                   });
        }

        static public void PauseAction(GameObject target, ActionS find)
        {
            var instanceTmp = shaco.Base.GameEntry.GetInstance<ActionS>();
            instanceTmp._DoActions(target, (ActionS action) =>
                                   {
                                       if (find == action)
                                           action.isPaused = true;
                                       return true;
                                   });
        }

        static public void PauseActionByTag(GameObject target, int actionTag)
        {
            var instanceTmp = shaco.Base.GameEntry.GetInstance<ActionS>();
            instanceTmp._DoActions(target, (ActionS action) =>
                                   {
                                       if (action.Tag == actionTag)
                                           action.isPaused = true;
                                       return true;
                                   });
        }

        static public void ResumeAction(GameObject target)
        {
            var instanceTmp = shaco.Base.GameEntry.GetInstance<ActionS>();
            instanceTmp._DoActions(target, (ActionS action) =>
                                   {
                                       action.isPaused = false;
                                       return true;
                                   });
        }

        static public void ResumeAction(GameObject target, ActionS find)
        {
            var instanceTmp = shaco.Base.GameEntry.GetInstance<ActionS>();
            instanceTmp._DoActions(target, (ActionS action) =>
                                   {
                                       if (find == action)
                                           action.isPaused = false;
                                       return true;
                                   });
        }

        static public void ResumeActionByTag(GameObject target, int actionTag)
        {
            var instanceTmp = shaco.Base.GameEntry.GetInstance<ActionS>();
            instanceTmp._DoActions(target, (ActionS action) =>
                                   {
                                       if (action.Tag == actionTag)
                                           action.isPaused = false;
                                       return true;
                                   });
        }

        static public Dictionary<GameObject, List<ActionS>> GetAllActions()
        {
            return shaco.Base.GameEntry.GetInstance<ActionS>()._mapActions;
        }

        static public GameObject GetDelegateInvoke()
        {
            var target = GetDelegateMonoBehaviour();
            if (null == target)
            {
                var retValue = GameObject.FindObjectOfType<GameObject>();
                if (null == retValue)
                {
                    retValue = new GameObject("ActionSTempDelegate");
                }
                return retValue;
            }
            else
            {
                return target.gameObject;
            }
        }

        static public MonoBehaviour GetDelegateMonoBehaviour()
        {
            if (!Application.isPlaying)
            {
                var retValue = GameObject.FindObjectOfType<MonoBehaviour>();

                if (null == retValue)
                {
                    Debug.LogWarning("no behaviour target in current scene");
                    retValue = new GameObject().AddComponent<ActionDelegate>();
                }
                return retValue;
            }
            else
            {
                shaco.Base.GameEntry.GetInstance<ActionS>();
                return _actionDelegate;
            }
        }

        static public void DestroyInstance()
        {
            shaco.InvokeS.CancelAllInvoke();
            foreach (var value in shaco.Base.GameEntry.GetInstance<ActionS>()._mapActions.Values)
            {
                var listActions = value;
                if (listActions != null)
                {
                    for (int j = listActions.Count - 1; j >= 0; --j)
                    {
                        MonoBehaviour.DestroyImmediate(listActions[j].Target);
                    }
                    listActions.Clear();
                }
            }

            if (_actionDelegate != null)
                MonoBehaviour.DestroyImmediate(_actionDelegate.gameObject);
            shaco.Base.GameEntry.GetInstance<ActionS>()._mapActions.Clear();
        }

        public static void LogError(string msg)
        {
            Log.Error("ActionS->" + msg);
        }

        public static void LogWarning(string msg)
        {
            Log.Warning("ActionS->" + msg);
        }

        static private void CheckRemove()
        {
            var instanceTmp = shaco.Base.GameEntry.GetInstance<ActionS>();
            if (instanceTmp._listRemove.Count == 0)
                return;

            foreach (var data in instanceTmp._listRemove)
            {
                __CheckRemove(data.key, data.value);
            }
            instanceTmp._listRemove.Clear();
        }

        static private void __CheckRemove(GameObject key, ActionS value)
        {
            var instanceTmp = shaco.Base.GameEntry.GetInstance<ActionS>();
            bool isFind = false;
            List<ActionS> listAction;

            if (instanceTmp._mapActions.TryGetValue(key, out listAction))
            {
                value.isPlaying = false;

                int prevListCount = listAction.Count;
                listAction.Remove(value);
                if (prevListCount == listAction.Count)
                {
                    Log.Error("ActionS+Static __CheckRemove error: not find remove data by key=" + key + " value=" + value);
                }

                if (listAction.Count == 0)
                {
                    instanceTmp._mapActions.Remove(key);
                }
                isFind = true;
            }
            else
            {
                for (int i = instanceTmp._listAdd.Count - 1; i >= 0; --i)
                {
                    if (instanceTmp._listAdd[i].key == key && instanceTmp._listAdd[i].value == value)
                    {
                        isFind = true;
                        instanceTmp._listAdd.RemoveAt(i);
                        break;
                    }
                }
            }

            if (!isFind)
                ActionS.LogError("not find remove data by key=" + key + " value=" + value);
        }

        static private void CheckAdd()
        {
            var instanceTmp = shaco.Base.GameEntry.GetInstance<ActionS>();

            foreach (var data in instanceTmp._listAdd)
            {
                _AddAction(data.key, data.value);
            }
            instanceTmp._listAdd.Clear();
        }

        static private void AddRemove(GameObject key, ActionS value)
        {
            if (value.isRemoved || !value._isAdded)
                return;

            var instanceTmp = shaco.Base.GameEntry.GetInstance<ActionS>();

            RemoveData rData = new RemoveData();
            rData.key = key;
            rData.value = value;
            value.isRemoved = true;
            value.isPlaying = false;
            value._isAdded = false;

            instanceTmp._listRemove.Add(rData);
        }

        static private void AddAction(GameObject key, ActionS value)
        {
            var instanceTmp = shaco.Base.GameEntry.GetInstance<ActionS>();

            value._isAdded = true;

            if (!instanceTmp._isMainUpdating)
                _AddAction(key, value);
            else
            {
                RemoveData rData = new RemoveData();
                rData.key = key;
                rData.value = value;
                instanceTmp._listAdd.Add(rData);
            }
        }

        static private void _AddAction(GameObject key, ActionS value)
        {
            List<ActionS> listActions;
            if (!shaco.Base.GameEntry.GetInstance<ActionS>()._mapActions.TryGetValue(key, out listActions))
            {
                listActions = new List<ActionS>();
                listActions.Add(value);
                shaco.Base.GameEntry.GetInstance<ActionS>()._mapActions.Add(key, listActions);
            }
            else
            {
                listActions.Add(value);
            }
        }
    }
}
