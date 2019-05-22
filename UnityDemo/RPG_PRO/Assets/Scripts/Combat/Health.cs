using UnityEngine;


namespace  RPG.Combat
{
    public class Health:MonoBehaviour
    {
        [SerializeField] float health = 20;

        private bool isDead = false;



        public void TakeDamage( float damage )
        {
            health = Mathf.Max(health - damage, 0);
            Debug.Log("Health:" + health);

           if( 0 == health) Die();
        }

        private void Die()
        {
            if(isDead) return;
            
            isDead = true;
            GetComponent<Animator>().SetTrigger("die");
        }

        public bool IsDead()
        {
            return this.isDead;
        }



    }


    
}