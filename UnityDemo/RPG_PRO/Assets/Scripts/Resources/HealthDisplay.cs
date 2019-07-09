using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace  RPG.Resources
{
    public class HealthDisplay : MonoBehaviour
    {
        Health player;
        Text healthText;

        private void Awake() {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
            healthText = GetComponent<Text>();
        }

        private void Update() {
            healthText.text = player.GetHealth().ToString();
        }
    }

}