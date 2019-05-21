using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public partial class BehaviourRootTree : BehaviourTree
    {
        public bool isRemoved = false;

        private class EnumeratorInfo
        {
            public IBehaviourProcess process = null;
            public IEnumerator enumerator = null;
            public BehaviourTree tree = null;

            public void Reset()
            {
                enumerator = null;
                tree = null;
            }
        }

        private class ExcueteEnumeratorInfo
        {
            public IBehaviourProcess process = null;
            public BehaviourTree tree = null;

            public void Reset()
            {
                tree = null;
                process = null;
            }
        }

        private Dictionary<int, IBehaviourParam> _parameters = new Dictionary<int, IBehaviourParam>();
        private Dictionary<string, BaseTree> _searchTress = new Dictionary<string, BaseTree>();
        private Dictionary<IBehaviourProcess, EnumeratorInfo> _enumerators = new Dictionary<IBehaviourProcess, EnumeratorInfo>();
        private List<EnumeratorInfo> _willAddEnumerators = new List<EnumeratorInfo>();
        private List<EnumeratorInfo> _willRemoveEnumerators = new List<EnumeratorInfo>();
        private List<ExcueteEnumeratorInfo> _excueteEnumerators = new List<ExcueteEnumeratorInfo>();

        public bool LoadFromJsonPath(string path)
        {
            bool retValue = BehaviourTreeConfig.LoadFromJsonPath(path, this);
            this.InitProcesses();
            return retValue;
        }

        public bool LoadFromJson(string json)
        {
            bool retValue = BehaviourTreeConfig.LoadFromJson(json, this);
            this.InitProcesses();
            return retValue;
        }

        public void SaveToJson(string path)
        {
            BehaviourTreeConfig.SaveToJson(this, path);
        }

        public BehaviourRootTree()
        {
            this.name = "RootTree";
        }

        public BaseTree GetTree<T>() where T : IBehaviourProcess
        {
            var name = Utility.ToTypeString<T>();
            if (string.IsNullOrEmpty(name)) return null;

            if (!_searchTress.ContainsKey(name))
            {
                Log.Error("BehaviourRootTree GetTree error: not find tree by name=" + name);
                return null;
            }
            else
                return _searchTress[name];
        }

        public bool HasTree(string name)
        {
            return _searchTress.ContainsKey(name);
        }

        public void Update(float elapseSeconds)
        {
            if (_excueteEnumerators.Count > 0)
            {
                foreach (var iter in _excueteEnumerators)
                {
                    if (!_enumerators.ContainsKey(iter.process))
                    {
                        iter.tree.StopRunning();
                    }
                }
                _excueteEnumerators.Clear();
            }

            UpdateEnumerator(elapseSeconds);

            if (_enumerators.Count == 0 && _willAddEnumerators.Count == 0)
            {
                this.Process();

                //excute children process
                ForeachChildren((BehaviourTree tree) =>
                {
                    tree.Process();
                    return true;
                });
            }
        }

        public override bool Process()
        {
            //根节点不做执行任务处理
            // this.SetOnProcessResultCallBack((bool isStoped) =>
            // {
            //     if (!isStoped)
            //     {
            //         ForeachChildren((shaco.Base.BehaviourTree tree) =>
            //         {
            //             tree.Process();
            //             return true;
            //         });
            //     }
            // });
            return base.Process();
        }

        new public void SetParameter(IBehaviourParam param)
        {
            if (null == param)
            {
                Log.Error("BehaviourRootTree SetParameter error: param is null");
                return;
            }

            if (GlobalParams.OpenDebugLog)
            {
                if (param.GetHashCode() == 0)
                {
                    Log.Error("BehaviourRootTree SetParameter error: param is null");
                    return;
                }
            }

            _parameters[param.GetType().GetHashCode()] = param;
        }

        new public T GetParameter<T>() where T : IBehaviourParam
        {
            var key = typeof(T).GetHashCode();
            if (_parameters.ContainsKey(key))
            {
                return (T)_parameters[key];
            }
            else
            {
                return default(T);
            }
        }

        new public ICollection<IBehaviourParam> GetParameters()
        {
            return _parameters.Values;
        }

        new public void SetParameters(ICollection<IBehaviourParam> param)
        {
            foreach (var p in param)
            {
                SetParameter(p);
            }
        }

        new public bool HasParameter<T>() where T : IBehaviourParam
        {
            var key = typeof(T).GetHashCode();
            return _parameters.ContainsKey(key);
        }

        new public bool HasParameters(IBehaviourParam[] keys)
        {
            bool retValue = false;
            for (int i = keys.Length - 1; i >= 0; --i)
            {
                var key = keys[i].GetType().GetHashCode();
                if (_parameters.ContainsKey(key))
                {
                    retValue = true;
                    break;
                }
            }
            return retValue;
        }

        new public void RemoveParameter<T>() where T : IBehaviourParam
        {
            var key = typeof(T).GetHashCode();
            if (!_parameters.ContainsKey(key))
            {
                Log.Error("BehaviourRootTree RemoveParameter error: not found parameter by key=" + shaco.Base.Utility.ToTypeString<T>());
            }
            else
            {
                _parameters.Remove(key);
            }
        }

        new public void RemoveParameters(IBehaviourParam[] keys)
        {
            for (int i = keys.Length - 1; i >= 0; --i)
            {
                var key = keys[i].GetType().GetHashCode();
                if (!_parameters.ContainsKey(key))
                {
                    Log.Error("BehaviourRootTree RemoveParameters error: not found parameter by key=" + key);
                }
                else
                {
                    _parameters.Remove(key);
                }
            }
        }

        public void ClearParameter()
        {
            _parameters.Clear();
        }

        protected override void AddSearchTree(BaseTree tree)
        {
            var key = tree.fullName;
            if (string.IsNullOrEmpty(key)) return;

            if (HasTree(key))
                Log.Error("BehaviourRootTree AddSearchTree error: has same key=" + key);
            else
                _searchTress.Add(key, tree);
        }

        protected override void RemoveSearchTree(BaseTree tree)
        {
            var key = tree.fullName;
            if (string.IsNullOrEmpty(key)) return;

            if (tree.IsRoot())
            {
                ((BehaviourRootTree)tree).isRemoved = true;
            }
            else 
            {
                if (!HasTree(key))
                    Log.Error("BehaviourRootTree RemoveSearchTree error: not find key=" + key);
                else
                {

                    _searchTress.Remove(key);
                }
            }
        }

        protected override void ClearSearchTree()
        {
            _searchTress.Clear();
        }

        public bool StartProcess(IBehaviourProcess process, BehaviourTree tree)
        {
            bool retValue = false;

            //当没有设置任务的时候，默认跳过该任务
            if (null == process)
            {
                return retValue;
            }

            var infoNew = ObjectPool.Instantiate<EnumeratorInfo>(typeof(EnumeratorInfo).FullName);
            infoNew.process = process;
            infoNew.enumerator = process.Process(tree);
            infoNew.tree = tree;
            _willAddEnumerators.Add(infoNew);
            retValue = true;

            return retValue;
        }

        private void InitProcesses()
        {
            this.ForeachAllChildren((BehaviourTree tree, int index, int level) =>
            {
                tree.InitProcees();
                return true;
            });
        }

        //计算迭代器任务(IBehaviourEnumerator)
        private void UpdateEnumerator(float elapseSeconds)
        {
            foreach (var iter in _enumerators)
            {
                var currentEnumerator = iter.Value.enumerator;

                //处理首次任务
                if (null == currentEnumerator.Current)
                {
                    currentEnumerator.MoveNext();

                    var infoTmp = ObjectPool.Instantiate<ExcueteEnumeratorInfo>(typeof(ExcueteEnumeratorInfo).FullName);
                    infoTmp.process = iter.Key;
                    infoTmp.tree = iter.Value.tree;
                    _excueteEnumerators.Add(infoTmp);

                    infoTmp.tree.StartRunning();
                }

                //处理被动和主动停止任务
                if (currentEnumerator.Current == null || currentEnumerator.Current is StopProcess)
                {
                    AddWillRemoveEnumerator(currentEnumerator, iter.Key, iter.Value);
                    continue;
                }

                //处理任务进度
                var behaviourEnumerator = (IBehaviourEnumerator)(currentEnumerator.Current);
                if (!behaviourEnumerator.IsRunning())
                {
                    //处理任务进度，自动跳转
                    MoveNextProcess(iter);
                }
                else 
                {
                    behaviourEnumerator.Update(elapseSeconds);
                }
            }

            //执行完毕即将被移除执行队列的任务
            if (_willRemoveEnumerators.Count > 0)
            {
                foreach (var iter in _willRemoveEnumerators)
                {
                    iter.Reset();
                    ObjectPool.RecyclingObject(iter);
                    _enumerators.Remove(iter.process);
                }
                _willRemoveEnumerators.Clear();
            }


            //检查需要添加的任务
            if (_willAddEnumerators.Count > 0)
            {
                foreach (var iter in _willAddEnumerators)
                {
                    _enumerators.Add(iter.process, iter);
                }
                _willAddEnumerators.Clear();
                UpdateEnumerator(elapseSeconds);
            }
        }

        private void AddWillRemoveEnumerator(IEnumerator currentEnumerator, IBehaviourProcess key, EnumeratorInfo value)
        {
            //丢失了任务
            if (currentEnumerator == null)
            {
                value.tree.OnProcessResult(false);
            }
            else if (currentEnumerator.Current is StopProcess)
            {
                value.tree.OnProcessResult(true);
            }
            else if (currentEnumerator.Current is ContinueProcess)
            {
                value.tree.OnProcessResult(false);
            }
            //可能是yield break
            else 
            {
                value.tree.OnProcessResult(false);
            } 
            _willRemoveEnumerators.Add(value);
        }

        //自动跳转任务进度
        private void MoveNextProcess(System.Collections.Generic.KeyValuePair<IBehaviourProcess, EnumeratorInfo> iter)
        {
            var nextEnumerator = iter.Value.enumerator;

            //自动跳转任务，如果跳转失败，则继续下阶段任务
            if (!nextEnumerator.MoveNext())
            {
                AddWillRemoveEnumerator(null, iter.Key, iter.Value);
            }
        }
    }
}