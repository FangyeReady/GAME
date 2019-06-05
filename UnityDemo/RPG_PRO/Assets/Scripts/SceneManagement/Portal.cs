using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG.SceneManagement
{
    enum PortalPoint
    {
        A,B,C,D,E
    }

    /// <summary>
    /// Portal:大门，传送门
    /// </summary>
    public class Portal : MonoBehaviour
    {
        [SerializeField] int SceneIndex = 0;
        [SerializeField] PortalPoint sceneType = PortalPoint.A;

        [SerializeField] float timeToFadeOut = 0.5f;
        [SerializeField] float timeToFadeIn = 1f;
        [SerializeField] float timeToWait = 0.5f;



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
           
            Fader presistanceEffect = GameObject.FindObjectOfType<Fader>();

            yield return presistanceEffect.FadeOut( timeToFadeOut );//特效播放完成后才开始加载场景
            yield return SceneManager.LoadSceneAsync(SceneIndex);

            foreach (Portal portal in FindObjectsOfType( typeof(Portal) ) )
            {
                if (portal == this) continue;
                if (portal.sceneType != this.sceneType) continue;
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                player.transform.position = portal.transform.Find("spawnPoint").position;  //即使都是子物体，貌似用position也是ok的
                player.transform.rotation = portal.transform.Find("spawnPoint").rotation;
                break;
            }
            yield return new WaitForSeconds(timeToWait);//跳转成功后，稍微停顿一下再把画面显示出来
            yield return presistanceEffect.FadeIn(timeToFadeIn);

            Destroy(this.gameObject);
        }

    }
}


