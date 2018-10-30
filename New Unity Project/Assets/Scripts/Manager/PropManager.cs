using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropManager : AutoStaticInstance<PropManager> {

    private Dictionary<string, PropInfo> _propinfo;
    public Dictionary<string, PropInfo> PropInfos { get { return _propinfo; } }

    private Dictionary<string, PropCfg> _propcfgs;
    public Dictionary<string, PropCfg> PropCfgs { get { return _propcfgs; } }

    private string pathinfo = StaticData.CONFIG_PATH + "PropInfo.txt";
    private string pathcfg = StaticData.CONFIG_PATH + "PropCfg.txt";

    public override void InitManager()
    {
        base.InitManager();
        pathinfo = Application.dataPath + pathinfo;
        pathcfg = Application.dataPath + pathcfg;
        ReadData.Instance.GetPropInfoData(pathinfo, out _propinfo);
        ReadData.Instance.GetPropCfgData(pathcfg, out _propcfgs);

        StaticUpdater.Instance.UpdateEvent += CalcClickTime;
    }


    public PropInfo GetPropInfoByID(string id)
    {
        if (PropInfos.ContainsKey(id))
        {
            return PropInfos[id];
        }
        return null;
    }


    public PropCfg GetPropCfgByID(string id)
    {
        if (PropCfgs.ContainsKey(id))
        {
            return PropCfgs[id];
        }
        return null;
    }

    int addExp = 0;
    public void UsePropItem(PropCfg cfg, ServentInfo servent)
    {
        addExp += cfg.exp;
        if (IsDoubleClick())
        {
            //UsePropItem(cfg, servent);
            return;
        }
        servent.favorability += cfg.favorability;
        servent.Loyal += cfg.loyal;
        servent.tire += cfg.tire;

        if (addExp > 0)
        {
            PopupServentHome home = UIManager.Instance.GetWindow<PopupServentHome>();
            if (home != null)
            {
                home.AddExp(addExp);
                home.InitInfo(servent);
                addExp = 0;
            }   
        }

    }

    int count = 0;
    private bool IsDoubleClick()
    {
        isClick = true;
        count += 1;
        if (clickTime <= 0.5f && count >= 2)
        {
            clickTime = 0f;
            return true;
        }
        else if (clickTime > 0.5f)
        {
            count = 1;
            clickTime = 0f;
        }
        return false;
    }

    bool isClick = false;
    float clickTime = 0.0f;
    private void CalcClickTime()
    {
        if (isClick)
        {
            clickTime += Time.deltaTime;
        }
    }

    private void OnDestroy()
    {
        isClick = false;
        clickTime = 0f;
        StaticUpdater.Instance.UpdateEvent -= CalcClickTime;
    }




}
