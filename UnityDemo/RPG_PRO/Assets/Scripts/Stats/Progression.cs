using UnityEngine;

namespace RPG.Stats
{
    [CreateAssetMenu(fileName = "Progression", menuName = "Stats/Progression", order = 0)]
    public class Progression : ScriptableObject
    {
        [SerializeField] CharacterBaseInfo[] characterBaseInfo;


        public float GetHealthByType(CharacterType type, int level)
        {
            foreach (var item in characterBaseInfo)
            {
                if (item.characterType == type)
                {
                    int index = Mathf.Clamp(level - 1, 0, item.health.Length - 1);
                    Debug.Log(index);
                    return item.health[index];
                }
            }
            return 0;
        }

        [System.Serializable]
        class CharacterBaseInfo
        {
           public CharacterType characterType;
           public float[] health;
        }
    }
}