using System.Collections;

namespace shaco.Base
{
    public class StackLocation
    {
        public string statck = string.Empty;
        public int statckLine = -1;
        public int numberOfCalls = 0;
        public double useMilliseconds = 0;

        private System.DateTime _nowTime = System.DateTime.Now;

        public void Reset()
        {
            statck = string.Empty;
            statckLine = -1;
            numberOfCalls = 0;
            useMilliseconds = 0;
        }

        public bool HasStatck()
        {
            return !string.IsNullOrEmpty(statck);
        }

        public void GetStatck(params string[] classPaths)
        {
            if (!GlobalParams.OpenDebugLog)
                return;

            if (classPaths.IsNullOrEmpty())
            {
                classPaths = new string[1] { FileHelper.GetCurrentSourceFilePath(2) };
            }

            int indexStatck = -1;
            for (int i = 0; i < classPaths.Length; ++i)
            {
                indexStatck = FileHelper.FindLastStatckLevelWhereCallLocated(classPaths[i]);
                if (indexStatck >= 0)
                    break;
            }

            if (indexStatck < 0)
            {
                indexStatck = FileHelper.FindLastStatckLevelWhereCallLocated(string.Empty);
            }

            ++indexStatck;
            ++numberOfCalls;

            var callAddEventStatckTmp = FileHelper.GetPathWhereCallLocated(indexStatck);
            statck = callAddEventStatckTmp;
            statckLine = FileHelper.GetFileLineNumberWhereCallLocated(indexStatck);
        }

        public void StartTimeSpanCalculate()
        {
            if (!GlobalParams.OpenDebugLog)
                return;

            _nowTime = System.DateTime.Now;
        }

        public void StopTimeSpanCalculate()
        {
            if (!GlobalParams.OpenDebugLog)
                return;

            var timeSpan = System.DateTime.Now - _nowTime;
            useMilliseconds = timeSpan.TotalMilliseconds;
        }

        public double GetTimeSpan()
        {
            return useMilliseconds;
        }

        public string GetPerformanceDescription()
        {
            if (!GlobalParams.OpenDebugLog)
                return string.Empty;

            var retValue = new System.Text.StringBuilder();
            if (numberOfCalls > 0)
            {
                retValue.Append(numberOfCalls);
                retValue.Append("(times)");
            }
            if (useMilliseconds > 0)
            {
                retValue.Append(useMilliseconds);
                retValue.Append("(ms)");
            }
            return retValue.ToString();
        }

        public StackLocation Clone()
        {
            StackLocation retValue = new StackLocation();
            retValue.statck = this.statck;
            retValue.statckLine = this.statckLine;
            retValue.numberOfCalls = this.numberOfCalls;
            retValue.useMilliseconds = this.useMilliseconds;
            return retValue;
        }
    }
}

