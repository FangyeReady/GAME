namespace shaco.Base
{
    public class StopProcess : IBehaviourEnumerator
    {
        public StopProcess()
        {

        }

        public override bool IsRunning()
        {
            return false;
        }

        public override void Reset()
        {

        }
    }
}

