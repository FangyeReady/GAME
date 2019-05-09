using System.Collections;
using UnityEngine;

namespace shaco.UIStateChangedArgs 
{
    public class OnUIPreLoadArg : OnUIStateChangedBaseArg {}
    public class OnUIInitArg : OnUIStateChangedBaseArg {}
	public class OnUIOpenArg : OnUIStateChangedBaseArg {}
    public class OnUIResumeArg : OnUIStateChangedBaseArg {}
    public class OnUIHideArg : OnUIStateChangedBaseArg {}
    public class OnUICloseArg : OnUIStateChangedBaseArg { }
    public class OnUICustomArg : OnUIStateChangedBaseArg {}
	
	public class OnUIStateChangedBaseArg : shaco.Base.BaseEventArg
	{
		public string uiKey = string.Empty;
		public GameObject uiTarget = null;
	}
}