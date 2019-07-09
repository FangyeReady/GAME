using System.Collections;
using System.Collections.Generic;
using RPG.Saving;
using UnityEngine;

namespace  RPG.Stats
{
    public class BaseStats : MonoBehaviour, ISaveble
    {
        [Range(1,99)]
        [SerializeField] int startingLevel = 1;
        [SerializeField] CharacterType type;
        [SerializeField] Progression progression = null;

        public float GetProgressionVal(Stat st)
        {
            return progression.GetProgressionValueByStat(type, startingLevel, st);
        }

        public object CaptureState()
        {
            return startingLevel;
        }


        public void RestoreState(object state)
        {
           this.startingLevel = (int)state;
        }
    }
}
