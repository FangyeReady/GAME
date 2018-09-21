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

}
