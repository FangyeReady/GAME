namespace shaco.Base
{
    public class WaitUntil : IBehaviourEnumerator
    {
        private System.Func<bool> _predicate = null;

        public WaitUntil(System.Func<bool> predicate)
        {
            _predicate = predicate;
        }

        public override bool IsRunning()
        {
            return !_predicate();
        }

        public override void Reset()
        {
            _predicate = null;
        }
    }
}

