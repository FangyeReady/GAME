using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


/// <summary>
/// cinematic  电影的
/// track  轨迹
/// dolly 推拉摄像机
/// </summary>
namespace  RPG.Cinematics
{
    public class CinematicsTrigger : MonoBehaviour
    {
        private bool isAreadyTriggered = false;

        private PlayableDirector m_PlayDir;

        private void Start() {
            m_PlayDir = GetComponent<PlayableDirector>();
        }

        private void OnTriggerEnter(Collider other) {

            if( !isAreadyTriggered && other.transform.CompareTag("Player"))
            {
                isAreadyTriggered = true;
                m_PlayDir.Play();
            }   
        }
    }
}

