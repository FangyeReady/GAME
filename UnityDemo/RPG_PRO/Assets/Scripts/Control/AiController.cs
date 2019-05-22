using RPG.Combat;
using UnityEngine;

namespace RPG.Core {

    public class AiController : MonoBehaviour {

        [SerializeField] float toPlayerDistance = 10f;

        private GameObject player;
        private Fighter m_Fighter;
        private Health m_Health;
        //private NavMeshAgent navAgent;

        private void Start () {
            player = GameObject.FindGameObjectWithTag ("Player");
            m_Fighter = GetComponent<Fighter> ();
            m_Health = GetComponent<Health> ();
            //navAgent = GetComponent<NavMeshAgent> ();
        }

        private void Update () {
            if ( !m_Health.IsDead() && IsPlayerInRange () && m_Fighter.CanAttack (tg: player.gameObject) ) {
                m_Fighter.Attack (player);
            } else {
                m_Fighter.Cancel ();
            }

        }

        private bool IsPlayerInRange () {
            float distance = Vector3.Distance (this.transform.position, player.transform.position);
            return distance < toPlayerDistance;
        }
    }
}