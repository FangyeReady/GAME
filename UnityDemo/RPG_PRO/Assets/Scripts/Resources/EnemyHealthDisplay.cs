using System.Collections;
using System.Collections.Generic;
using RPG.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace  RPG.Resources
{
    public class EnemyHealthDisplay : MonoBehaviour
    {
        Fighter player;
        Health enemy;
        Text healthText;

        private void Awake()
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Fighter>();
            healthText = GetComponent<Text>();
        }

        private void Update()
        {
            if(player.GetCurTarget() == null) {
                healthText.text = "N/A";
                print("ui no target~!");
                return;
            }
            Health enemy = player.GetCurTarget();
            healthText.text = enemy.GetHealth().ToString();
        }
    }

}