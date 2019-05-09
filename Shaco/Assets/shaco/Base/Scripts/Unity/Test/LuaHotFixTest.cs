#if HOTFIX_ENABLE

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using XLua;
using System.Collections.Generic;
using System;
using System.Linq;

namespace shaco.Test
{
    public class LuaHotFixTest : MonoBehaviour
    {
        public string UrlServer;

        public Text TextProgress;
        public Text TextLog;
        public Text TextVersion;
        public Text TextUpdateState;
        public System.Func<string> getlogcallback = null;
        public List<int> ListTest;
        shaco.HotUpdateImportWWW _updateServer = new shaco.HotUpdateImportWWW();

        private shaco.Base.HttpHelper _http = new shaco.Base.HttpHelper();

        void Start()
        {
            updateTextDescription();
        }

        void FixedUpdate()
        {
            if (_http.IsCompleted())
            {
                Debug.Log("http completed !");

                var byteTmp = _http.GetDownloadByte();
                string ss = string.Empty;
                int maxLength = byteTmp.Length < 100 ? byteTmp.Length : 100;
                for (int i = 0; i < maxLength; ++i)
                    ss += byteTmp[i] - '0';
                Debug.Log(ss);

                _http.CloseClient();
            }

            if (_http.GetHttpState() == shaco.Base.HttpHelper.HttpState.Downloading || _http.GetHttpState() == shaco.Base.HttpHelper.HttpState.Uploading)
            {
                Debug.Log("http progress=" + _http.GetProgress());
            }
        }

        int Add(int a, int b)
        {
            return a - b;
        }

        int SubFunc(int a, int b)
        {
            return a + b;
        }

        Vector3 Vec(Vector3 a)
        {
            Debug.Log("c# Vec " + a.ToString());
            return a;
        }

        public string getLog()
        {
            return getlogcallback();
        }

        public void ButtonClicked()
        {

        }

        public IEnumerator LoopCallBack()
        {
            while (true)
            {
                yield return new WaitForSeconds(1.0f);
                Debug.Log("----------unity LoopCallBack---------");
            }
        }


        void OnGUI()
        {
            float width = 120;
            float height = 30;

#if !UNITY_EDITOR
        width *= 4;
        height *= 4;
#endif

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("RunWithFile", GUILayout.Width(width), GUILayout.Height(height)))
            {
                shaco.XLuaManager.RunWithFile("lua_example/LuaHotFixTest.lua", string.Empty, shaco.Base.FileDefine.FileExtension.Lua);
                getlogcallback += () =>
                {
                    return "this a unity c# log";
                };
                TextLog.text = "log result =" + getLog();
                Debug.Log(TextLog.text);

                StopCoroutine(LoopCallBack());
                StartCoroutine(LoopCallBack());
            }
            if (GUILayout.Button("RunWithFolder", GUILayout.Width(width), GUILayout.Height(height)))
            {
                shaco.XLuaManager.RunWithFolder("Lua", ResourceUpdateTest.multiVersionControlRelativePath, shaco.Base.FileDefine.FileExtension.Lua);
            }
            if (GUILayout.Button("Run Add Function", GUILayout.Width(width), GUILayout.Height(height)))
            {
                TextLog.text = "Add result =" + Add(1, 2);
                Debug.Log(TextLog.text);
            }
            // if (GUILayout.Button("testHttpUpload", GUILayout.Width(width), GUILayout.Height(height)))
            // {
            //     var head1 = new shaco.Base.HttpHelper.HttpComponent("live_key", "cavy_live");
            //     var head2 = new shaco.Base.HttpHelper.HttpComponent("live_secure", "e6}3XTkHLj.T");

            //     var body1 = new shaco.Base.HttpHelper.HttpComponent("user_id", "P67w4NMm3gGD4vnFd");
            //     var body2 = new shaco.Base.HttpHelper.HttpComponent("game_id", "1");
            //     var body3 = new shaco.Base.HttpHelper.HttpComponent("platform", "ios");
            //     var body4 = new shaco.Base.HttpHelper.HttpComponent("version", "1.1");
            //     var body5 = new shaco.Base.HttpHelper.HttpComponent("versionCode", "1");
            //     var body6 = new shaco.Base.HttpHelper.HttpComponent("duration", "100");

            //     //upload cavy life info
            //     _http.UploadAsync("http://pay.tunshu.com/live/api/v1/motion/logs",
            //         new shaco.Base.HttpHelper.HttpComponent[] { head1, head2 },
            //         new shaco.Base.HttpHelper.HttpComponent[] { body1, body2, body3, body4, body5, body6 });
            // }
            GUILayout.EndHorizontal();

        }

        private void updateTextDescription()
        {
            if (TextVersion != null)
                TextVersion.text = _updateServer.GetVersion();
        }
    }
}
#endif