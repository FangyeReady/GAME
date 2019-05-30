using UnityEngine;
using RPG.Control;
using UnityEngine.Playables;
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
            player.enabled = false;
        }

        private void EnableControll(PlayableDirector dir)
        {
            player.enabled = true;
        }   
    }
}
