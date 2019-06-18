using RPG.Saving;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace  RPG.SceneManagement
{
    public class SavingWrapper : MonoBehaviour
    {
        [SerializeField] float fadeInTime = 0.35f;
        private readonly string defaultFile = "save";
        private SavingSystem saveManager;
        private void Awake() {
            saveManager = this.GetComponent<SavingSystem>();
        }


        private IEnumerator Start() {
            Fader fader = GameObject.FindObjectOfType<Fader>();
            fader.FadeOutImmediately();     
            yield return saveManager.LoadLastScene(defaultFile);
            yield return fader.FadeIn(fadeInTime);

            print("called~!");
        }

        private void Update() {
            if( Input.GetKeyDown( KeyCode.S ))
            {
                Save();
            }

            if (Input.GetKeyDown( KeyCode.L ))
            {
                Load();
            }
        }

        public void Load()
        {
            saveManager.Load(defaultFile);
        }

        public void Save()
        {
            saveManager.Save(defaultFile);
        }
    }
}