using UnityEngine;
using RPG.Control;
using UnityEngine.Playables;
using RPG.Core;

namespace RPG.Cinematics
{
    public class CinematicsControllRemover : MonoBehaviour
    {
        PlayerController player;
        private void Awake() {

            player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();

            PlayableDirector dir = GetComponent<PlayableDirector>();
            dir.played += DisableControll;
            dir.stopped += EnableControll;
        }

        private void DisableControll(PlayableDirector dir)
        {
            player.GetComponent<ActionScheduler>().CancelCurrentAction();
            player.enabled = false;
        }

        private void EnableControll(PlayableDirector dir)
        {
            player.enabled = true;
        }   
    }
}
