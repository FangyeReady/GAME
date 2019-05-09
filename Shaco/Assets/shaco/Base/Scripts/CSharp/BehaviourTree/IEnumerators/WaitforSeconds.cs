namespace shaco.Base
{
    public class WaitforSeconds : IBehaviourEnumerator
    {
        private float _seconds = 1;
        private float _currentSeconds = 0;

        public WaitforSeconds(float seconds)
		{
            this._seconds = seconds;
        }

        public override bool IsRunning()
		{
            return this._currentSeconds < this._seconds;
        }

        public override void Reset()
		{
            _currentSeconds = 0;
            _seconds = 1;
        }

        public override void Update(float elapseSeconds)
        {
            _currentSeconds += elapseSeconds;
        }
    }
}

