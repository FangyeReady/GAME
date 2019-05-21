namespace shaco.Base
{
    public class WaitForFrame : IBehaviourEnumerator
    {
        private float _frame = 1;
        private float _currentFrame = 0; 

        public WaitForFrame(int frame = 1)
        {
            _frame = frame;
        }

        public override bool IsRunning()
        {
            return this._currentFrame++ < this._frame;
        }

        public override void Reset()
        {
            _frame = 1;
            _currentFrame = 0;
        }
    }
}

