using UnityEngine;
using shaco.Base;

namespace shaco
{
    public class WaitForAnimationEnd : IBehaviourEnumerator
    {
        private Animation _animation = null;

        public WaitForAnimationEnd(Animation animation)
        {
            _animation = animation;
        }

        public override bool IsRunning()
        {
            return _animation.isPlaying;
        }

        public override void Reset()
        {

        }
    }
}

