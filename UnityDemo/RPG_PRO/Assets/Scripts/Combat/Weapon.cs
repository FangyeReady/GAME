using UnityEngine;
using RPG.Resources;
namespace RPG.Combat
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/Weapon", order = 0)]
    public class Weapon : ScriptableObject
    {
        [SerializeField] GameObject equipPrefab;
        [SerializeField] GameObject projectilePrefab;
        [SerializeField] AnimatorOverrideController weaponOverrideController;
        [SerializeField] float weaponRange = 10f;
        [SerializeField] float weaponDamage = 5f;
        [SerializeField] bool isRightHand = true;

        const string WeaponName = "Weapon";
        
        public void Spawn(Transform rightHand,Transform leftHand, Animator controller)
        {
            Transform equipPos = GetHandTransform(rightHand, leftHand);

            DestoryOldWeapon(rightHand, leftHand);

            if (equipPrefab != null)
            {
                GameObject weapon = Instantiate(equipPrefab, equipPos);
                weapon.name = WeaponName;
                weapon.transform.localPosition = Vector3.zero;
                weapon.transform.localScale = Vector3.one;
                weapon.transform.localRotation = Quaternion.identity;
            }

            var baseOverrideController = controller.runtimeAnimatorController as AnimatorOverrideController;//RuntimeAnimatorController 是 AnimatorOverrideController的父类
            if (weaponOverrideController != null)
            {
                controller.runtimeAnimatorController = weaponOverrideController;
            }
            else if(baseOverrideController != null)
            {
                controller.runtimeAnimatorController = baseOverrideController.runtimeAnimatorController;//用这个baseOverrideController，是不行的，必须用baseOverrideController.runtimeAnimatorController
            }

        }

        private static void DestoryOldWeapon(Transform leftHand, Transform rightHand)
        {
            Transform oldWeapon = leftHand.Find(WeaponName);
            
            if(null == oldWeapon)
                oldWeapon = rightHand.Find(WeaponName);

            if(null == oldWeapon) return;

            oldWeapon.name = "DESTORYING this GameObject";
            Destroy(oldWeapon.gameObject);
        }

        private Transform GetHandTransform(Transform rightHand, Transform leftHand)
        {
            return isRightHand ? rightHand : leftHand;
        }

        public bool HasProjectile()
        {
            return projectilePrefab != null;
        }

        public void SpawnLongRangeBullet(Transform rightHand, Transform leftHand, Health target)
        {
            Transform equipPos = GetHandTransform(rightHand, leftHand).transform;
            Projectile projectile = Instantiate(projectilePrefab, equipPos.position, Quaternion.identity).GetComponent<Projectile>();
            projectile.SetTarget(target, weaponDamage);
        }

        public float GetDamege()
        {
            return weaponDamage;
        }

        public float GetRange()
        {
            return weaponRange;
        }
    }
}