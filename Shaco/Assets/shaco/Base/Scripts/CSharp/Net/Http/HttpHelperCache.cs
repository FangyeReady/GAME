using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class HttpHelperCache
    {
        static private List<HttpHelper> _listHttpHelper = new List<HttpHelper>();

        static public void AddHelper(HttpHelper http)
        {
            if (!GlobalParams.OpenDebugLog)
                return;

            if (!_listHttpHelper.Contains(http))
                _listHttpHelper.Add(http);
        }

        static public void RemoveHelper(HttpHelper http)
        {
            if (!GlobalParams.OpenDebugLog)
                return;
                
            if (_listHttpHelper.Contains(http))
                _listHttpHelper.Remove(http);
        }

        static public void OnApplicationStart()
        {
            if (!GlobalParams.OpenDebugLog)
                return;

            _listHttpHelper.Clear();
        }

        static public void OnApplicationQuit()
        {
            if (!GlobalParams.OpenDebugLog)
                return;

            for (int i = _listHttpHelper.Count - 1; i >= 0; --i)
            {
                if (_listHttpHelper[i] != null)
                {
                    _listHttpHelper[i].CloseClient();
                    Log.Info("HttpHelperCache: force close http helper");
                }
            }
            _listHttpHelper.Clear();
        }
    }
}
