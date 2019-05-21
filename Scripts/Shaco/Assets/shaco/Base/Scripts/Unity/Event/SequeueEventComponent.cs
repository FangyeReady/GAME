using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class SequeueEventComponent : MonoBehaviour
    {
        public shaco.Base.StackLocation statckLocation = new shaco.Base.StackLocation();

        public IEnumerator Run(SequeueEvent sequeueEvent, List<SequeueEvent.CoroutineInfo> coroutines)
        {
            for (int i = 0; i < coroutines.Count; ++i)
            {
                yield return coroutines[i].coroutine;
            }

            sequeueEvent.StopEvent();
        }
    }
}