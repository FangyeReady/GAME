using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServentManager : AutoStaticInstance<ServentManager> {

    /// <summary>
    /// 初始化读取技能id表和技能说明
    /// </summary>
    public override void InitManager()
    {
        base.InitManager();
    }

    private int GetSkillID(Star star)
    {
        return 0;
    }

    private string GetSkillDesc(int skillID)
    {
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
