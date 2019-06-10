using UnityEngine;

namespace  RPG.Saving
{
    public class SavingWrapper : MonoBehaviour
    {
        private readonly string defaultFile = "save";

        SavingSystem saveManager;
        private void Awake() {
            saveManager = this.GetComponent<SavingSystem>();
        }


        private void Start() {
            Load();
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