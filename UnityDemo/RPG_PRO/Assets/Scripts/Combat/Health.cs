using UnityEngine;
using UnityEngine.AI;
using RPG.Core;

namespace  RPG.Combat
{
    public class Health:MonoBehaviour
    {
        [SerializeField] float health = 20;

        private bool isDead = false;
        private ActionScheduler m_ActionScheduler;
        private NavMeshAgent navAgent;

        private void Start() {
            m_ActionScheduler = GetComponent<ActionScheduler>();
            navAgent = GetComponent<NavMeshAgent>();
        }


        public void TakeDamage( float damage )
        {
            health = Mathf.Max(health - damage, 0);
            //Debug.Log("Health:" + health);

           if( 0 == health) Die();
        }

        private void Die()
        {
            if(isDead) return;
            
            isDead = true;
            GetComponent<Animator>().SetTrigger("die");
            m_ActionScheduler.CancelCurrentAction();
            navAgent.enabled = false;
        }

        public bool IsDead()
        {
            return this.isDead;
        }



    }


    
}