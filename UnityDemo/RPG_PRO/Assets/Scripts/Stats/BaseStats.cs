using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  RPG.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [Range(1,99)]
        [SerializeField] int startingLevel = 1;
        [SerializeField] CharacterType type;
        [SerializeField] Progression progression = null;

        public float GetHealthVal()
        {
            return progression.GetHealthByType(type, startingLevel);
        }
    }
}
