using UnityEngine;

namespace  RPG.Core
{
    public class DestoryAfterEffect : MonoBehaviour
    {
        private void Update() {
            if( !GetComponent<ParticleSystem>().IsAlive() )
            {
                Destroy(this.gameObject);
            }
        }
    }
}