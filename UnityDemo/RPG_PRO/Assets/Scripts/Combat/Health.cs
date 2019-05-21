using UnityEngine;


namespace  RPG.Combat
{
    public class Health:MonoBehaviour
    {
        [SerializeField] float health = 20;
        public void TakeDamage( float damage )
        {
            health = Mathf.Max( health - damage, 0 );
            Debug.Log("Health:" + health);
        }

    }
}