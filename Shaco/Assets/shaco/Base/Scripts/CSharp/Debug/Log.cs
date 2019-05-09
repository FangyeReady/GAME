using System.Collections;

namespace shaco.Base
{
	public class Log : ILog
    {
        static public void Info(string message)
        {
            if (GlobalParams.OpenDebugLog) GetInstance().InfoDelegate(message);
        }

        static public void InfoFormat(string message, params object[] args)
        {
            if (GlobalParams.OpenDebugLog) GetInstance().InfoFormatDelegate(message, args);
        }

        static public void Warning(string message)
        {
            if (GlobalParams.OpenDebugLog) GetInstance().WarningDelegate(message);
        }

        static public void Error(string message)
        {
            if (GlobalParams.OpenDebugLog) GetInstance().ErrorDelegate(message);
        }

        static public void Exception(string message)
        {
            if (GlobalParams.OpenDebugLog) GetInstance().ExceptionDelegate(message);
        }
        
        public void InfoDelegate(string message)
        {
            System.Console.WriteLine("Info:" + message);
        }

        public void InfoFormatDelegate(string message, params object[] args)
        {
            System.Console.WriteLine("InfoFormat:" + string.Format(message, args));
        }

        public void WarningDelegate(string message)
        {
            System.Console.WriteLine("Warning:" + message);
        }

        public void ErrorDelegate(string message)
        {
            System.Console.WriteLine("Error:" + message);
        }

        public void ExceptionDelegate(string message)
        {
            throw new System.Exception(message);
        }

        static private ILog GetInstance()
        {
            if (!GameEntry.HasInstance<shaco.Base.ILog>())
                return GameEntry.GetInstance<shaco.Base.Log>();
            else
                return GameEntry.GetInstance<shaco.Base.ILog>();
        }
    }
}

