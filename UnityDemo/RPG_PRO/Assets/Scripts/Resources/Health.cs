using UnityEngine;
using UnityEngine.AI;
using RPG.Core;
using RPG.Saving;
using RPG.Stats;

namespace  RPG.Resources
{
    public class Health:MonoBehaviour ,ISaveble
    {
        [SerializeField] float health = 20;

        private bool isDead = false;



        private void Start() {
            this.health = GetComponent<BaseStats>().GetHealthVal();
        }

        public void TakeDamage( float damage )
        {
            health = Mathf.Max(health - damage, 0);

           if( 0 == health) Die();
        }

        private void Die()
        {
            if(isDead) return;
            
            isDead = true;
            GetComponent<Animator>().SetTrigger("die");
            GetComponent<ActionScheduler>().CancelCurrentAction();
            GetComponent<NavMeshAgent>().enabled = false;
        }

        public bool IsDead()
        {
            return this.isDead;
        }

        public void RestoreState(object state)
        {
            health = (float) state;
            if ( health <= 0 )
                Die();
        }

        public object CaptureState()
        {
           return health;
        }
    }


    
}