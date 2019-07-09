using UnityEngine;

namespace RPG.Stats
{
    [CreateAssetMenu(fileName = "Progression", menuName = "Stats/Progression", order = 0)]
    public class Progression : ScriptableObject
    {
        [SerializeField] CharacterBaseInfo[] characterBaseInfo;


        public float GetProgressionValueByStat(CharacterType type, int level, Stat st)
        {
            foreach (var item in characterBaseInfo)
            {
                if (item.characterType == type)
                {
                    foreach (var info in item.infos)
                    {
                        if( info.st == st && info.values.Length > 0)//&& info.values.Length > level)
                        {
                            int index = Mathf.Clamp(level, 0, info.values.Length - 1);
                            return info.values[index];
                        }
                    }
                }
            }
            return 0;
        }

        [System.Serializable]
        class CharacterBaseInfo
        {
            [Header("---------------CharacterType-----------------")]
           public CharacterType characterType;
            [Header("---------------------------------------------")]
           public Info[] infos;
        }

        [System.Serializable]
        class Info
        {
           public Stat st;
           public float[] values;
        }
    }
}