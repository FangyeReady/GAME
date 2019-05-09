using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco.Base
{
    public partial class BehaviourRootTree : BehaviourTree
    {
        static private shaco.ActionS _actionUpdate = null;
        static private Dictionary<int, BehaviourRootTree> _runningTrees = new Dictionary<int, BehaviourRootTree>();

        static private bool _isForeachingUpdate = false;
        static private Dictionary<int, BehaviourRootTree> _willAddTrees = new Dictionary<int, BehaviourRootTree>();
        static private List<int> _willRemoveTrees = new List<int>();

        public void Start()
        {
            var key = this.GetHashCode();
            if (_isForeachingUpdate)
            {
                _willAddTrees.Add(key, this);
            }
            else
            {
                _runningTrees.Add(key, this);
            }
            CheckStart();
        }

        public void Stop()
        {
            var key = this.GetHashCode();
            if (_isForeachingUpdate)
            {
                _willRemoveTrees.Add(key);
            }
            else
            {
                _runningTrees.Remove(key);
            }
        }

        static public void BaseUpdate(float delayTime)
        {
            if (_willAddTrees.Count > 0)
            {
                _runningTrees.AddRange(_willAddTrees);
                _willAddTrees.Clear();
            }

            _isForeachingUpdate = true;
            foreach (var iter in _runningTrees)
            {
                try
                {
                    iter.Value.Update(delayTime);
                }
                catch (System.Exception e)
                {
                    Log.Error("e=" + e);
                    iter.Value.isRemoved = true;
                }
                if (iter.Value.isRemoved)
                {
                    _willRemoveTrees.Add(iter.Key);
                }
            }
            _isForeachingUpdate = false;

            if (_willRemoveTrees.Count > 0)
            {
                _runningTrees.RemoveRange(_willRemoveTrees);
                _willRemoveTrees.Clear();
            }
        }

        static public void StopAll()
        {
            _runningTrees.Clear();
        }

        private void CheckStart()
        {
            if (Application.isPlaying && (null == _actionUpdate || !_actionUpdate.isPlaying))
            {
                _actionUpdate = shaco.Repeat.CreateRepeatForver(shaco.DelayTime.Create(float.MaxValue));
                _actionUpdate.RunAction(shaco.GameEntry.GetComponentInstance<SceneManager>().gameObject);

                _actionUpdate.onFrameFunc += (float percent) =>
                {
                    shaco.Base.Utility.GetEplaseTime();
                    var delayTime = float.MaxValue * percent;
                    BaseUpdate(delayTime);
                };
            }
        }
    }
}

