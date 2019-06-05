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

        //n 为所有的帧数  time为用户所规定的时间 curAlpha为当前的alpha值

        // n = time / Time.deltaTime

        // curAlpha = 1 / n

        // curAlpha = 1 / (tiem/Time.deltaTime)

        // curAlpha = 1 * Time.deltaTime / time

        // curAlpha = Time.deltaTime / time

        //总的alpha就等于每一帧的curAlpha相加

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