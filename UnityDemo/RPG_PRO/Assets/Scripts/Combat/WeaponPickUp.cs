using UnityEngine;

namespace  RPG.Combat
{
    public class WeaponPickUp : MonoBehaviour
    {
        [SerializeField] Weapon weapon;
        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<Fighter>().EquipWeapon(weapon);
                Destroy(this.gameObject);
            }
        }
    }
}