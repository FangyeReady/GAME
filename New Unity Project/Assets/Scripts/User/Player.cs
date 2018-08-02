using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : AutoStaticInstance<Player> {

    PlayerInfo playerInfo;

    public string Name
    { get { return playerInfo.name; } }

    public int Level
    { get { return playerInfo.level; }
      private set { playerInfo.level = value; }
    }

    public int AllExp
    { get { return playerInfo.allexp; } }

    public int CurrentExp
    { get { return GetCurrentExpAndLevel(); }}

    public IEnumerator InitInfo()
    {
        ReadData.Instance.GetPlayerData(Application.dataPath + "/" + StaticData.CONFIG_PATH +
            "playerInfo.txt", out playerInfo);
        yield return null;
    }


    private int GetCurrentExpAndLevel()
    {
        int tempExp = AllExp;
        int baseExp = 1000;
        int offset = 150;
        int level = 0;
        int curExp = 0;
        while (true)
        {
            tempExp -= baseExp;
            if (tempExp < 0)
            {
                curExp = tempExp;
                break;
            }
            baseExp += offset;
            ++level;
        }
        this.Level = level;
        return curExp;
    }

    

}
