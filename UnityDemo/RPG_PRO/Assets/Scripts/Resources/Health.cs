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
            this.health = GetComponent<BaseStats>().GetProgressionVal( Stat.Health );
        }

        public void TakeDamage( GameObject instigator ,float damage )
        {
            health = Mathf.Max(health - damage, 0);

           if( 0 == health) 
            {
                Die();
                AwardExperience(instigator);
            }
        }

        public float GetHealth()
        {
            return health;
        }

        private void Die()
        {
            if(isDead) return;
            
            isDead = true;
            GetComponent<Animator>().SetTrigger("die");
            GetComponent<ActionScheduler>().CancelCurrentAction();
            GetComponent<NavMeshAgent>().enabled = false;
        }

        private void AwardExperience(GameObject instigator)
        {
            Experience exp = instigator.GetComponent<Experience>();
            if (exp != null)
            {
                exp.GainExperience( GetComponent<BaseStats>().GetProgressionVal( Stat.RewardRxp )  );
            }
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