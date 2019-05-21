using System.Collections;
using UnityEngine;
using System;

namespace shaco
{
    public class Log : shaco.Base.ILog
    {
        public class LogConfig
        {
            public string message = string.Empty;
            public Color color = new Color(0.58f, 0.58f, 0.58f, 1);

            public LogConfig(string message) { this.message = message; }
            public LogConfig(string message, Color color) { this.message = message; this.color = color; }
        }
        
        static public void Info(string message)
        {
            if (shaco.Base.GlobalParams.OpenDebugLog) GetInstance().InfoDelegate(message);
        }

        static public void InfoFormat(string message, params object[] args)
        {
            if (shaco.Base.GlobalParams.OpenDebugLog) GetInstance().InfoFormatDelegate(message, args);
        }

        static public void Warning(string message)
        {
            if (shaco.Base.GlobalParams.OpenDebugLog) GetInstance().WarningDelegate(message);
        }

        static public void Error(string message)
        {
            if (shaco.Base.GlobalParams.OpenDebugLog) GetInstance().ErrorDelegate(message);
        }

        static public void Exception(string message)
        {
            if (shaco.Base.GlobalParams.OpenDebugLog) GetInstance().ExceptionDelegate(message);
        }

        static public void Info(string message, Color color)
        {
            if (shaco.Base.GlobalParams.OpenDebugLog) Debug.Log(MakeRichtextLog(message, color));
        }

        static public void Info(params LogConfig[] message)
        {
            if (shaco.Base.GlobalParams.OpenDebugLog)
            {
                var allMsg = string.Empty;
                for (int i = 0; i < message.Length; ++i)
                {
                    allMsg += MakeRichtextLog(message[i].message, message[i].color);
                }
                Debug.Log(allMsg);
            }
        }

        static public void LogBreak(string message)
        {
            Debug.Log(message);
            Debug.Break();
        }

        public void InfoDelegate(string message)
        {
            if (shaco.Base.GlobalParams.OpenDebugLog) Debug.Log(message);
        }

        public void InfoFormatDelegate(string message, params object[] args)
        {
            if (shaco.Base.GlobalParams.OpenDebugLog) Debug.Log(string.Format(message, args));
        }

        public void WarningDelegate(string message)
        {
            if (shaco.Base.GlobalParams.OpenDebugLog) Debug.LogWarning(message);
        }

        public void ErrorDelegate(string message)
        {
            if (shaco.Base.GlobalParams.OpenDebugLog) Debug.LogError(message);
        }

        public void ExceptionDelegate(string message)
        {
            if (shaco.Base.GlobalParams.OpenDebugLog) throw new System.Exception(message);
        }

		static private string MakeRichtextLog(string message, Color color)
        {
            string r = Convert.ToString((int)(color.r * 255), 16); if (r.Length == 1) r += r[0];
            string g = Convert.ToString((int)(color.g * 255), 16); if (g.Length == 1) g += r[0];
            string b = Convert.ToString((int)(color.b * 255), 16); if (b.Length == 1) b += r[0];
            string a = Convert.ToString((int)(color.a * 255), 16); if (a.Length == 1) a += r[0];

            string ret = string.Format("<color=#{0}>{1}</color>", r + g + b + a, message);
            return ret;
        }

        static private shaco.Base.ILog GetInstance()
        {
            return shaco.GameEntry.GetInstance<shaco.Log>();
        }
    }
}

