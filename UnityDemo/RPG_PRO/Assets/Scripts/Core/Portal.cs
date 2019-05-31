using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace  PRG.SceneManageMent
{
    enum SceneType
    {
        A,B,C,D,E
    }

    public class Portal : MonoBehaviour
    {
        [SerializeField] int SceneIndex = 0;
        [SerializeField] SceneType sceneType;

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Player"))
            {
               StartCoroutine(GoToNextScene());              
            }
        }


        private IEnumerator GoToNextScene()
        {
            if(SceneIndex < 0) yield break;

            DontDestroyOnLoad(this.gameObject);
            yield return SceneManager.LoadSceneAsync(SceneIndex);

            Vector3 spawnPos = Vector3.zero;
            foreach (Portal portal in FindObjectsOfType( typeof(Portal) ) )
            {
                if (portal == this) continue;
                if (portal.sceneType != this.sceneType) continue;
                spawnPos = portal.transform.Find("spawnPoint").position;

            }
            GameObject.FindGameObjectWithTag("Player").transform.position = spawnPos;  //即使都是子物体，貌似用position也是ok的

            Destroy(this.gameObject);
        }

    }
}


