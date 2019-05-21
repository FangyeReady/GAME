using System.Collections;

namespace shaco.Base
{
    public class BehaviourRandomTree : BehaviourDefaultTree
    {
        public override string GetDisplayName()
        {
            return "Random";
        }

        public override bool Process()
        {
            this.SetOnProcessResultCallBack((bool isStoped) =>
            {
                if (!isStoped)
                {
                    int currentIndex = 0;
                    int randSelectIndex = shaco.Base.Utility.Random(0, Count);
                    ForeachChildren((shaco.Base.BehaviourTree tree) =>
                    {
                        if (currentIndex++ == randSelectIndex)
                        {
                            tree.Process();
                            return false;
                        }
                        else
                            return true;
                    });
                }
            });
            return base.Process();
        }
    }
}