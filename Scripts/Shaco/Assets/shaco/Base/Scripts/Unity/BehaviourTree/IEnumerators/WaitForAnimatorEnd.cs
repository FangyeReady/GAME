using UnityEngine;
using shaco.Base;

namespace shaco
{
    public class WaitForAnimatorEnd : IBehaviourEnumerator
    {
        private Animator _animator = null;
        private int _animationIndex = 0;

        public WaitForAnimatorEnd(Animator animator, int animationIndex)
        {
            _animator = animator;
            _animationIndex = animationIndex;
        }

        public override bool IsRunning()
        {
            var animationTmp = _animator.GetCurrentAnimatorStateInfo(_animationIndex);
            return animationTmp.normalizedTime < 1.0f;
        }

        public override void Reset()
        {

        }
    }
}

