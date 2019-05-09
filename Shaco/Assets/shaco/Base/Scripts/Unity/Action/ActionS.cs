using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//--------------------------------
//ActionS Using parameters is All global parameters, For example, do not use localEulerAngles
//--------------------------------
namespace shaco
{
    public partial class ActionS
    {
        public delegate void SS_CALLFUNC_FRAME(float percent);
        public delegate void SS_CAllFUNC_COMPLETE(ActionS action);

        public delegate bool SS_CALLFUN_1_B(ActionS param1);

        //-------------------------------
        //Action params
        //-------------------------------
        public GameObject Target = null;
        public float Elapsed = 0;
        public float Duration = 1;
        public string ActionName = "ActionS";
        public int Tag = 0;
        public bool isRemoved = false;
        public bool isPlaying = false;
        public bool isPaused = false;

        //-------------------------------
        //Callback Functions
        //-------------------------------
        public SS_CAllFUNC_COMPLETE onCompleteFunc = null;
        public SS_CALLFUNC_FRAME onFrameFunc = null;

        //-------------------------------
        //Instance params
        //-------------------------------
        private Dictionary<GameObject, List<ActionS>> _mapActions = new Dictionary<GameObject, List<ActionS>>();
        private float _fCurrentTime = 0;
        private bool _isMainUpdating = false;
        private bool _isAutoPlay = true;
        private bool _isAdded = false;

        List<RemoveData> _listAdd = new List<RemoveData>();
        List<RemoveData> _listRemove = new List<RemoveData>();

        private class RemoveData
        {
            public GameObject key;
            public ActionS value;
        }

        public ActionS()
        {
            ActionName = this.ToTypeString();
            CheckDelegate();
        }

        public void MainUpdate(float delayTime)
        {
            _isMainUpdating = true;
            shaco.Base.GameEntry.GetInstance<ActionS>()._fCurrentTime = delayTime;

            foreach (var listActions in _mapActions)
            {
                foreach (var valueAction in listActions.Value)
                {
                    if (!valueAction.isPaused && !valueAction.isRemoved)
                    {
                        if (valueAction == null || valueAction.Target == null)
                        {
                            ActionS.LogError("Target is null, update action error! ActionName=" + valueAction.ActionName);
                            AddRemove(listActions.Key, valueAction);
                            continue;
                        }

                        if (!IsActionAlive(valueAction))
                        {
                            AddRemove(listActions.Key, valueAction);
                        }
                        else
                        {
                            float prePercent = valueAction.GetCurrentPercent();
                            valueAction.UpdateAction(prePercent, shaco.Base.GameEntry.GetInstance<ActionS>()._fCurrentTime);
                        }
                    }
                }
            }

            _isMainUpdating = false;

            CheckRemove();
            CheckAdd();
        }

        public void RunActionWithoutPlay(GameObject target)
        {
            _isAutoPlay = false;
            RunAction(target);
            _isAutoPlay = true;
        }

        public void StopMe(bool isPlayEndWithDirectly = false)
        {
            if (isPlayEndWithDirectly)
            {
                this.PlayEndDirectly();
            }
            ActionS.AddRemove(Target, this);
        }

        public void Pause()
        {
            ActionS.PauseAction(Target, this);
        }

        public void Resume()
        {
            ActionS.ResumeAction(Target, this);
        }

        virtual public float UpdateAction(float prePercent, float delayTime)
        {
            return UpdateActionBase(prePercent, delayTime);
        }

        virtual public ActionS Clone()
        {
            ActionS.LogError(ActionName + "----not have Clone function");
            return new ActionS();
        }

        virtual public ActionS Reverse()
        {
            ActionS.LogError(ActionName + "----not have Reverse function");
            return new ActionS();
        }

        virtual public void Reset(bool isAutoPlay)
        {
            if (isAutoPlay)
                Elapsed = this.GetCurrentPercent() * Duration;
            else
                Elapsed = 0;
        }

        virtual public void PlayEndDirectly()
        {
            if (onCompleteFunc != null)
                onCompleteFunc(this);

            Elapsed = Duration;
        }

        virtual public void RunAction(GameObject target)
        {
            CheckDelegate();
            if (HasAction(target, this))
            {
                LogWarning("Have been to add the action");
            }

            Target = target;
            Elapsed = 0;
            isRemoved = false;

            if (ActionName == "ActionS")
                ActionName = GetType().FullName;

            //run action by directly
            if (Duration <= 0.0f)
            {
                if (onFrameFunc != null)
                    onFrameFunc(1.0f);
                if (onCompleteFunc != null)
                    onCompleteFunc(this);
                return;
            }

            if (_isAutoPlay)
            {
                ActionS.AddAction(target, this);
                isPlaying = true;
            }
        }

        public float GetCurrentPercent()
        {
            if (this.Duration <= 0)
                return 1.0f;
            else
                return shaco.Base.GameEntry.GetInstance<ActionS>()._fCurrentTime / this.Duration;
        }

        public float GetElapsedPercent()
        {
            if (this.Duration <= 0)
                return 1.0f;
            else
                return this.Elapsed / this.Duration + GetCurrentPercent();
        }

        public float GetRemainPercent()
        {
            if (this.Duration <= 0)
                return 0;
            else
                return (this.Duration - this.Elapsed) / this.Duration;
        }

        protected bool CheckDelegate()
        {
            if (!Application.isPlaying)
                return true;

            if (null == _actionDelegate)
            {
                var findObject = GameObject.FindObjectOfType<ActionDelegate>();

                if (findObject == null)
                {
                    GameObject objTmp = new GameObject();
                    findObject = objTmp.AddComponent<ActionDelegate>();
                }

                ActionS._actionDelegate = findObject;
                findObject.transform.name = "ActionS_Delegate";
                UnityHelper.SafeDontDestroyOnLoad(findObject.gameObject);
            }

            return true;
        }

        protected void SetActionAlive(bool isAlive)
        {
            if (isAlive)
            {
                Elapsed = 0;
                Duration = 9999999;
            }
            else
            {
                Elapsed = 1;
                Duration = 1;
            }
        }

        protected void _DoActions(GameObject target, SS_CALLFUN_1_B doFunc)
        {
            var instanceTmp = shaco.Base.GameEntry.GetInstance<ActionS>();
            List<ActionS> listAction;
            if (instanceTmp._mapActions.TryGetValue(target, out listAction))
            {
                for (int i = 0; i < listAction.Count; ++i)
                {
                    if (!doFunc(listAction[i]))
                        break;
                }
            }
            else
            {
                for (int i = 0; i < _listAdd.Count; ++i)
                {
                    if (_listAdd[i].key == target)
                    {
                        if (!doFunc(_listAdd[i].value))
                            break;
                    }
                }
            }
        }

        protected bool IsActionAlive(ActionS action)
        {
            if (action.Duration - action.Elapsed < 0.001f)
                action.Elapsed = action.Duration;

            return action.Elapsed < action.Duration;
        }

        protected float UpdateActionBase(float prePercent, float delayTime)
        {
            if (isRemoved)
            {
                return 1.0f;
            }

            float remainPercent = this.GetRemainPercent();

            if (prePercent > remainPercent)
            {
                prePercent = remainPercent < 0 ? 0 : remainPercent;
            }

            Elapsed += Duration * prePercent;

            if (!IsActionAlive(this))
            {
                if (onFrameFunc != null)
                {
                    onFrameFunc(1.0f);
                }

                if (onCompleteFunc != null)
                    onCompleteFunc(this);

                AddRemove(this.Target, this);
            }
            else if (onFrameFunc != null)
            {
                onFrameFunc(Elapsed / Duration);
            }

            return prePercent;
        }
    }
}