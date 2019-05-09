namespace shaco.Base
{
    public abstract class IBehaviourEnumerator : System.Collections.IEnumerator
    {
        public virtual object Current { get { return this; } }

        public virtual void Update(float elapseSeconds) { }

        public virtual bool MoveNext() { return true; }

        public abstract bool IsRunning();

        public abstract void Reset();

        static public implicit operator IBehaviourEnumerator(int frameCount)
        {
            return new WaitForFrame(frameCount);
        }

        static public implicit operator IBehaviourEnumerator(float seconds)
        {
            return new WaitforSeconds(seconds);
        }

        static public implicit operator IBehaviourEnumerator(bool isContinueProcess)
        {
            return (isContinueProcess ? (IBehaviourEnumerator)new ContinueProcess() : (IBehaviourEnumerator)new StopProcess());
        }
    }
}