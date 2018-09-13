﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServentManager : AutoStaticInstance<ServentManager> {

    private ServentSkillInfo serventSkill;
    private string pathid = StaticData.CONFIG_PATH + "ServentSkillID.txt";
    private string pathdesc = StaticData.CONFIG_PATH + "ServentSkillDesc.txt";

    /// <summary>
    /// 初始化读取技能id表和技能说明
    /// </summary>
    public override void InitManager()
    {
        base.InitManager();
        pathid = Application.dataPath + pathid;
        pathdesc = Application.dataPath + pathdesc;
        ReadData.Instance.GetServentSkillData(pathid, pathdesc, out serventSkill);
    }

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

}