using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public interface IBehaviourProcess
    {
        /// <summary>
        /// Behaviour execution method
        /// </summary>
        /// <returns>if return 'true', it means execution successful and interrupt other process</returns>
        IEnumerator<shaco.Base.IBehaviourEnumerator> Process(BehaviourTree tree);
    }
}

