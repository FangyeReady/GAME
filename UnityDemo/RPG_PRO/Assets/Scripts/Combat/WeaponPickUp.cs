using UnityEngine;
using System;
using System.Collections;

namespace  RPG.Combat
{
    public class WeaponPickUp : MonoBehaviour
    {
        [SerializeField] Weapon weapon;
        [SerializeField] float spawnTime = 5f;

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<Fighter>().EquipWeapon(weapon);
                StartCoroutine(ReSpawnWeanponPickUp());
            }
        }

        IEnumerator ReSpawnWeanponPickUp()
        {
            SetWeaponShowState(false);
            yield return new WaitForSeconds(spawnTime);
            SetWeaponShowState(true);
        }

        private void SetWeaponShowState(bool state)
        {
            GetComponent<CapsuleCollider>().enabled = state;
            foreach (Transform item in transform)
            {
                item.gameObject.SetActive(state);
            }
        }
    }
}