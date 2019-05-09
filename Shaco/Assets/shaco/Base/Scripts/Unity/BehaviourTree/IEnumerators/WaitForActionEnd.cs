using UnityEngine;
using shaco.Base;

namespace shaco
{
    public class WaitForActionEnd : IBehaviourEnumerator
    {
        private shaco.ActionS _action = null;

        public WaitForActionEnd(shaco.ActionS action)
        {
            _action = action;
        }

        public override bool IsRunning()
        {
            return _action.Elapsed < _action.Duration;
        }

        public override void Reset()
        {
            _action.Reset(false);
        }
    }
}

