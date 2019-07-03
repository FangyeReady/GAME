using UnityEngine;

namespace RPG.Stats
{
    [CreateAssetMenu(fileName = "Progression", menuName = "Stats/Progression", order = 0)]
    public class Progression : ScriptableObject
    {
        [SerializeField] CharacterBaseInfo[] characterBaseInfo;

        [System.Serializable]
        class CharacterBaseInfo
        {
            [SerializeField] CharacterType characterType;
            [SerializeField] float[] health;
        }
    }
}