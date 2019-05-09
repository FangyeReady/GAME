using System.Collections;

namespace shaco.Base
{
    /// <summary>
    /// 事件基础类信息，所有事件都需要继承该类
    /// </summary>
    public class BaseEventArg : System.Object
    {
        public string eventID = string.Empty;

        public BaseEventArg()
        {
            eventID = GetType().FullName;
        }
    }
}
