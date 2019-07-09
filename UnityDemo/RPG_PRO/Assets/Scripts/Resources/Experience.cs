using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Saving;

namespace RPG.Resources
{
    public class Experience : MonoBehaviour
    {
        [SerializeField] float Exp;

        public void GainExperience(float val)
        {
            this.Exp += val;
        }
    }
}
