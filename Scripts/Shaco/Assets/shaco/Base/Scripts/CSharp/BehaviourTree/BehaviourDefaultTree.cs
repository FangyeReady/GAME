using System.Collections;

namespace shaco.Base
{
    public class BehaviourDefaultTree : BehaviourTree
    {
        public override string GetDisplayName()
        {
            return "Default";
        }

        public override bool Process()
        {
            this.SetOnProcessResultCallBack((bool isStoped) =>
            {
                if (!isStoped)
                {
                    ForeachChildren((shaco.Base.BehaviourTree tree) =>
                    {
                        tree.Process();
                        return true;
                    });
                }
            });
            return base.Process();
        }
    }
}