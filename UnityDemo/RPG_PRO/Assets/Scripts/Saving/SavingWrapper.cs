using UnityEngine;

namespace  RPG.Saving
{
    public class SavingWrapper : MonoBehaviour
    {
        private readonly string defaultFile = "savedata/save.sav";

        private void Update() {
            if( Input.GetKeyDown( KeyCode.S ))
            {
                SavingSystem.Save(defaultFile);
            }

            if(Input.GetKeyDown( KeyCode.L ))
            {
                SavingSystem.Load(defaultFile);
            }
        }
    }
}