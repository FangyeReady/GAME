using UnityEngine;
using System.Collections;

namespace shaco
{
    public class ParticleAutoDestroy : MonoBehaviour
    {
        public delegate void OnDestroyCallBack(GameObject target);
        public OnDestroyCallBack OnDestroyCallFunc = null;
        public bool AudoDestroyTarget = false;

        private ParticleSystem[] particleSystems;
        private bool isEnd = false;

        void Start()
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>();
            isEnd = false;
        }

        void OnDisable()
        {
            OnParticleEnd();
        }

        void Update()
        {
            bool allStopped = true;

            foreach (ParticleSystem ps in particleSystems)
            {
#if UNITY_5_6_OR_NEWER
                if ((!ps.isStopped && ps.time < ps.main.duration) && !ps.main.loop)
#else
                if ((!ps.isStopped && ps.time < ps.duration) && !ps.loop)
#endif
                {
                    allStopped = false;
                }
            }

            if (allStopped)
            {
                OnParticleEnd();
            }
        }

        void OnParticleEnd()
        {
            if (isEnd)
                return;

            isEnd = true;
            if (OnDestroyCallFunc != null)
                OnDestroyCallFunc(this.gameObject);

            if (AudoDestroyTarget)
                Destroy(gameObject);
        }
    }
}