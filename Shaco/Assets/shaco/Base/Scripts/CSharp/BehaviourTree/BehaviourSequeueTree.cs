using System.Collections;

namespace shaco.Base
{
    public class BehaviourSequeueTree : BehaviourDefaultTree
    {
        private BehaviourTree _currentChildTree = null;

        public override string GetDisplayName()
        {
            return "Sequeue";
        }

        public override bool Process()
        {
            _currentChildTree = this._child as BehaviourTree;

            this.SetOnProcessResultCallBack((bool isStoped) =>
            {
                if (!isStoped)
                {
                    ProcessNextChild();
                }
            });
            return base.Process();
        }

        private void ProcessNextChild()
        {
            if (null != _currentChildTree)
            {
                _currentChildTree.Process();
                _currentChildTree.SetOnProcessResultCallBack((bool isStoped) =>
                {
                    if (!isStoped)
                    {
                        if (_currentChildTree.IsLastChild())
                        {
                            _currentChildTree = null;
                        }
                        else 
                        {
                            _currentChildTree = _currentChildTree.next as BehaviourTree;
                        }
                        ProcessNextChild();
                    }
                });
            }
        }
    }
}