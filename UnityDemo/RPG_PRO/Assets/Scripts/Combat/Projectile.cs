using UnityEngine;

namespace  RPG.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] float speed = 10f;
        [SerializeField] bool isHoming = false;
        [SerializeField] GameObject[] firstDestoryObjs;
        [SerializeField] GameObject hitImpactPrefab;
        [SerializeField] float maxLifeTime = 10f;
        [SerializeField] float timeToWait = 0.05f;

        Health target;
        float damage = 10;

        private void Start() {
            this.transform.LookAt(GetAttackPoint());
        }

        private void Update() {
            if( null == target ) return;
            if( isHoming )
            {
                this.transform.LookAt(GetAttackPoint());
            }
            print("target is not null");
            this.transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }


        public void SetTarget(Health target, float damage)
        {
            this.target = target;
            this.damage = damage;

            Destroy(this.gameObject, maxLifeTime);
            print("set target~!" + target.name);
        }


        private Vector3 GetAttackPoint()
        {
            CapsuleCollider collider = target.GetComponent<CapsuleCollider>();
            if( null == collider ) return target.transform.position;
            return target.transform.position +( Vector3.up * collider.height * 0.5f );
        }

        private void OnTriggerEnter(Collider other) {
            Health hp = other.GetComponent<Health>();
            if ( hp != null && !hp.IsDead() )
            {
                hp.TakeDamage(damage);

                if (hitImpactPrefab != null)
                {
                  Instantiate(hitImpactPrefab, GetAttackPoint(), Quaternion.Euler(0,180,0));
                }
                    

                for (int i = 0; i < firstDestoryObjs.Length; i++)
                {
                    Destroy(firstDestoryObjs[i]);
                }


                Destroy(this.gameObject, timeToWait);
            }
        }
    }
}