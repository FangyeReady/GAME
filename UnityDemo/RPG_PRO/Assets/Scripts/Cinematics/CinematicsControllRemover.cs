using UnityEngine;
using RPG.Control;
using UnityEngine.Playables;
namespace RPG.Cinematics
{
    public class CinematicsControllRemover : MonoBehaviour
    {
        private void Awake() {
            PlayableDirector dir = GetComponent<PlayableDirector>();
            dir.played += DisableControll;
            dir.stopped += EnableControll;
        }

        private void DisableControll(PlayableDirector dir)
        {
            GameObject.FindWithTag("Player").GetComponent<PlayerController>().enabled = false;
        }

        private void EnableControll(PlayableDirector dir)
        {
            GameObject.FindWithTag("Player").GetComponent<PlayerController>().enabled = true;
        }   
    }
}
