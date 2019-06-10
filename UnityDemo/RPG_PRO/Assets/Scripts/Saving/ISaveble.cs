using System.Collections.Generic;
using UnityEngine;


namespace  RPG.Saving
{
    public interface ISaveble {
        void RestoreState(object state);
        object CaptureState();
    } 
}