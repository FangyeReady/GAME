using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  Test.Desc
{
    [ExecuteInEditMode]
    public class QuickBuildNewDemoScene : MonoBehaviour
    {
        [SerializeField] Help help;

        private void Start() {
           help.ShowLog();
        }
    }
}

