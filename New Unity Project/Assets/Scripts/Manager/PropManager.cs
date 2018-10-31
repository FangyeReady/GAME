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

    public void UsePropItem(PropCfg cfg, ServentInfo servent)
    {
        servent.favorability += cfg.favorability;
        servent.Loyal += cfg.loyal;
        servent.tire += cfg.tire;

        PopupServentHome home = UIManager.Instance.GetWindow<PopupServentHome>();
        if (home != null)
        {        
            if (cfg.exp > 0)
            {
                home.AddExp(cfg.exp);
            }
            home.InitInfo(servent);
        }

    }

}
