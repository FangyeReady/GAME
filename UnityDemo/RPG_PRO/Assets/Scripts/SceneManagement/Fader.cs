using UnityEngine;
using System.Collections;

namespace  RPG.SceneManagement
{
    public class Fader : MonoBehaviour
    {
        CanvasGroup canvasFader;
        private void Start() {
            canvasFader = this.GetComponent<CanvasGroup>();

            //StartCoroutine( FadeOutIn() );
        }

        /// <summary>
        /// only for test
        /// </summary>
        /// <returns></returns>
        IEnumerator FadeOutIn( )
        {
            yield return FadeOut(3f);

            yield return FadeIn(2f);
        }

        public IEnumerator FadeOut( float time )
        {
            while( canvasFader.alpha < 1f )
            {
                canvasFader.alpha += Time.deltaTime / time;
                yield return null;
            }
        }

        public IEnumerator FadeIn(float time)
        {
            while (canvasFader.alpha > 0f )
            {
                canvasFader.alpha -= Time.deltaTime / time;
                yield return null;
            }
        }


    }
}