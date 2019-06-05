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

        private void Update() {
            if( Input.GetKeyDown( KeyCode.S ))
            {
                saveManager.Save(defaultFile);
            }

            if(Input.GetKeyDown( KeyCode.L ))
            {
                saveManager.Load(defaultFile);
            }
        }
    }
}