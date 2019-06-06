using UnityEngine;

namespace RPG.Saving
{
    [System.Serializable]
    public class SerializeableVector3 
    {
        float x;
        float y;
        float z;

        public SerializeableVector3(Vector3 vec)
        {
            x = vec.x;
            y = vec.y;
            z = vec.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x,y,z);
        }
    }
}