using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Servent Manager.
/// </summary>
public class ServentManager : AutoStaticInstance<ServentManager> {

    /// <summary>
    /// skill info.
    /// </summary>
    private ServentSkillInfo serventSkill;
    /// <summary>
    /// skill level info.
    /// </summary>
    private ServentLevel serventLevel;
    private string pathid = StaticData.CONFIG_PATH + "ServentSkillID.txt";
    private string pathdesc = StaticData.CONFIG_PATH + "ServentSkillDesc.txt";
    private string pathlevel = StaticData.CONFIG_PATH + "ServentLevel.txt";

    /// <summary>
    /// 初始化读取技能id表和技能说明
    /// </summary>
    public override void InitManager()
    {
        base.InitManager();
        pathid = Application.dataPath + pathid;
        pathdesc = Application.dataPath + pathdesc;
        pathlevel = Application.dataPath + pathlevel;
        ReadData.Instance.GetServentSkillData(pathid, pathdesc, out serventSkill);
        ReadData.Instance.GetServentLevel(pathlevel, out serventLevel);
    }

    /// <summary>
    /// get skill id.
    /// </summary>
    /// <param name="star"></param>
    /// <returns></returns>
    private int GetSkillID(Star star)
    {
        if (serventSkill.skillID.ContainsKey(star.ToString()))
        {
            int length = serventSkill.skillID[star.ToString()].Length;
            int min = serventSkill.skillID[star.ToString()][0];
            int max = serventSkill.skillID[star.ToString()][length - 1];
            return Utility.GetRandomVal(min, max + 1);
        }
        return 0;
    }

    /// <summary>
    /// get skill id.
    /// </summary>
    /// <param name="skillID"></param>
    /// <returns></returns>
    private string GetSkillDesc(int skillID)
    {
        if (serventSkill.skillDesc.ContainsKey(skillID.ToString()))
        {
            return serventSkill.skillDesc[skillID.ToString()];
        }
        return string.Empty;
    }

    public void SetServentInfo(Star star, int result, ref ServentInfo servent)
    {
        servent.ID = result;
        servent.Name = "Servent" + result.ToString();
        servent.Level = 1;
        servent.Loyal = 100;
        servent.skillID = GetSkillID(star);
        servent.Desc = GetSkillDesc(servent.skillID);
        servent.star = (int)star;
    }

    public int GetServentNextLevelExp(int level)
    {
        if (level >=0 && level < serventLevel.Levels.Count)
        {
            return serventLevel.Levels[level];
        }
        return 0;
    }

    
    public void SetServentLevelByAddExp(ServentInfo servent, int addExp)
    {
        int allExp = GetServentNextLevelExp(servent.Level);
        if (allExp == 0)
        {
            return;
        }
        int needExp = allExp - servent.nowexp;

        int temp = addExp - needExp;
        if (temp > 0)
        {
            servent.Level += 1;
            SetServentLevelByAddExp(servent, temp);
        }
        else if (temp == 0)
        {
            servent.Level += 1;
            return;
        }
        else
        {
            servent.nowexp += addExp;
            return;
        }
    }

    public override void Save()
    {
        throw new System.NotImplementedException();
    }
}
