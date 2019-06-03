using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.SceneManagement;

namespace  RPG.Core
{
    public class PresistanceSpawner : MonoBehaviour
    {
        [SerializeField] GameObject presistanceEffectPrefab;

        private static bool hasSpwan = false;

        private void Awake() {
            if (hasSpwan) return;

            hasSpwan = true;

            CreatePresistanceEffect();
        }

        private void CreatePresistanceEffect()
        {
            GameObject effect = Instantiate(presistanceEffectPrefab);
            DontDestroyOnLoad(effect);
        }
    }
}
