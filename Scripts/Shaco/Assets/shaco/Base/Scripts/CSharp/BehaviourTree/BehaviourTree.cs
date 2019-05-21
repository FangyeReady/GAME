using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    [BehaviourProcessTree]
    public partial class BehaviourTree : BaseTree
    {
        public BehaviourRootTree GetRoot()
        {
            return _root as BehaviourRootTree;
        }

        public void SetParameter(IBehaviourParam param) { GetRootTree().SetParameter(param);  }
        public T GetParameter<T>() where T : IBehaviourParam { return GetRootTree().GetParameter<T>();  }
        public void SetParameters(ICollection<IBehaviourParam> param) { GetRootTree().SetParameters(param); }
        public ICollection<IBehaviourParam> GetParameters() { return GetRootTree().GetParameters(); }
        public bool HasParameter<T>() where T : IBehaviourParam { return GetRootTree().HasParameter<T>(); }
        public bool HasParameters(IBehaviourParam[] keys) { return GetRootTree().HasParameters(keys); }
        public void RemoveParameter<T>() where T : IBehaviourParam { GetRootTree().RemoveParameter<T>();  }
        public void RemoveParameters(IBehaviourParam[] keys) { GetRootTree().RemoveParameters(keys); }

        private System.Action<bool> _onProcessResultCallBack = null;
        private IBehaviourProcess _processInterface = null;
        private bool _isRunning = false;

        /// <summary>
        /// 将节点数据序列化为json数据数组
        /// <param name=""> </param>
        /// <return></return>
        /// </summary>
        virtual public List<string> ToJson()
        {
            var retValue = new List<string>();
            retValue.Add(GetType().FullName);
            retValue.Add(name);
            retValue.Add(string.Empty);
#if UNITY_EDITOR
            retValue.Add(UnityEditor.AssetDatabase.GetAssetPath(this.editorAssetProcess));
#endif
            return retValue;
        }

        /// <summary>
        /// 从json数组数组中依次获取节点数据，与ToJson的顺序相对应
        /// <param name=""> </param>
        /// <return></return>
        /// </summary>
        virtual public void FromJson(BehaviourTreeConfig.JsonInfo jsonInfo)
        {
            if (!jsonInfo.jsonDataArray.IsArray)
            {
                Log.Error("BehaviourTree ToJson error: not array !");
                return;
            }

            jsonInfo.GetNextData();//ignore typename
            this.name = jsonInfo.GetNextData();
#if UNITY_EDITOR
            jsonInfo.GetNextData();//ignore children array
            this.editorAssetPathProcess = jsonInfo.GetNextData();
            this.editorAssetProcess = UnityEditor.AssetDatabase.LoadAssetAtPath(this.editorAssetPathProcess, typeof(UnityEngine.TextAsset)) as UnityEngine.TextAsset;
#endif
        }

        virtual public void OnGUIDraw()
        {

        }
        
        virtual public string GetDisplayName()
        {
            return this.GetType().FullName;
        }

        virtual public bool Process()
        {
            return GetRoot().StartProcess(_processInterface, this);
        }

        public void StartRunning()
        {
            _isRunning = true;
        }

        public void StopRunning()
        {
            _isRunning = false;
        }

        public bool IsRunning()
        {
            return _isRunning;
        }
        public void ForeachChildren(System.Func<BehaviourTree, bool> callback)
        {
            ForeachChildren((BaseTree tree) =>
            {
                return callback(tree as BehaviourTree);
            });
        }

        public void ForeachAllChildren(System.Func<BehaviourTree, int, int, bool> callback)
        {
            ForeachAllChildren((BaseTree tree, int index, int level) =>
            {
                return callback(tree as BehaviourTree, index, level);
            });
        }

        public void BindProcess<T>() where T : IBehaviourProcess, new()
        {
            this._processInterface = new T();
            this.name = this._processInterface.ToTypeString();
        }

        public bool InitProcees()
        {
            if (!string.IsNullOrEmpty(this.name))
            {
                var process = (IBehaviourProcess)typeof(IBehaviourProcess).Assembly.CreateInstance(this.name);
                if (null == process)
                {
                    Log.Error("BehaviourTree InitProcees erorr: can't find script name=" + this.name);
                    return false;
                }
                else
                    this._processInterface = process;
                return true;
            }
            else
                return false;
        }

        public void SetOnProcessResultCallBack(System.Action<bool> callback)
        {
            _onProcessResultCallBack = callback;
        }

        public void OnProcessResult(bool isStoped)
        {
            if (null != _onProcessResultCallBack)
            {
                _onProcessResultCallBack(isStoped);
            }
        }

        protected BehaviourTree()
        {

        }

        private BehaviourRootTree GetRootTree()
        {
            var retValue = this as BehaviourRootTree;
            return null == retValue ? this.GetRoot() : retValue;
        }
    }
}