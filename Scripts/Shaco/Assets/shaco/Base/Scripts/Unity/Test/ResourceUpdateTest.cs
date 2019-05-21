using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Reflection;

namespace shaco.Test
{
    public class ResourceUpdateTest : MonoBehaviour
    {
        public const string multiVersionControlRelativePath = "1_0_7";

        public string urlVersion;
        public UnityEngine.UI.Text TextProgressImprecise;
        public UnityEngine.UI.Text TextProgressPrecision;
        public UnityEngine.UI.Text TextDownloadSpeed;
        public GameObject UIRootParent;
        public UnityEngine.UI.Image ImageTarget;
        private shaco.HotUpdateImportWWW _updateServer = new shaco.HotUpdateImportWWW();

        void Awake()
        {
            // shaco.Localization.LoadWIthJsonResourcesPathLanguage(string.Empty, SystemLanguage.Chinese);
        }

        void Start()
        {
            shaco.SceneManager.OpenDebugMode = true;
            shaco.HotUpdateHelper.SetDynamicResourceAddress("http://ltsres-dev.playfun.me/HotUpdate/" + multiVersionControlRelativePath);
        }

        void Update()
        {

        }

        void OnGUI()
        {
            float width = Screen.width / 3 * 2;
            float height = Screen.height / 10;

            if (GUILayout.Button("CheckUpdate", GUILayout.Width(width), GUILayout.Height(height)))
            {
                _updateServer.CheckUpdate(urlVersion, false, multiVersionControlRelativePath);

                _updateServer.onCheckVersionEnd.AddCallBack(this, (object sender) =>
                {
                    shaco.Log.Info("Version=" + _updateServer.GetVersion() + " need update size=" + _updateServer.GetCurrentNeedUpdateDataSize() + " status=" + _updateServer.GetStatusDescription());
                });

                _updateServer.onProcessIng.AddCallBack(this, (object sender) =>
                {
                    TextProgressPrecision.text = _updateServer.GetDownloadResourceProgress().ToString();
                    TextDownloadSpeed.text = "Speed:" + _updateServer.GetDownloadSpeedFormatString();

                    shaco.Log.Info("progress =" + _updateServer.GetDownloadResourceProgress());
                });

                _updateServer.onProcessEnd.AddCallBack(this, (object sender) =>
                {
                    if (_updateServer.HasError())
                    {
                        shaco.Log.Info("ResourceUpdateTest error: msg=" + _updateServer.GetLastError(), Color.white);
                    }

                    if (_updateServer.IsCompleted())
                    {
                        shaco.Log.Info("check test end status=" + _updateServer.GetStatusDescription(), Color.white);

                        // shaco.Log.Info("read txt=" + shaco.ResourcesEx.LoadResourcesOrLocal("TextResource"));
                        // shaco.ResourcesEx.LoadResourcesOrLocalAsync("TextResource", (UnityEngine.Object obj)=> { shaco.Log.Info("read txt async=" + obj); });
                        // shaco.Log.Info("read png=" + shaco.ResourcesEx.LoadResourcesOrLocal<Sprite>("uiPackage"));
                        // shaco.ResourcesEx.LoadResourcesOrLocalAsync("uiPackage", (UnityEngine.Object obj) => { shaco.Log.Info("read png async=" + obj); });
                        // shaco.Log.Info("read internal png=" + shaco.ResourcesEx.LoadResourcesOrLocal("uiPackage/btn_blue_01"));
                        // shaco.ResourcesEx.LoadResourcesOrLocalAsync("uiPackage/btn_blue_01", (UnityEngine.Object obj) => { shaco.Log.Info("read internal png async=" + obj); });
                        // shaco.Log.Info("read all png=" + shaco.ResourcesEx.LoadAllResourcesOrLocal("uiPackage").Length);
                        // shaco.ResourcesEx.LoadAllResourcesOrLocalAsync("uiPackage", (UnityEngine.Object[] objs)=> { shaco.Log.Info("read all png async=" + objs.Length); });
                    }
                });
            }

            if (GUILayout.Button("CheckUpdateLocalOnly", GUILayout.Width(width), GUILayout.Height(height)))
            {
                var isNeedUpdate = shaco.HotUpdateHelper.CheckUpdateLocalOnly("1.0", multiVersionControlRelativePath);
                Debug.Log("update resource flag=" + isNeedUpdate);
            }

            if (GUILayout.Button("LoadAssetbundle", GUILayout.Width(width), GUILayout.Height(height)))
            {
                shaco.ResourcesEx.LoadResourcesOrLocalAsync("1", typeof(Sprite), (obj)=>
                {
                    ImageTarget.sprite = obj as Sprite;
                });
            }

            if (GUILayout.Button("LoadAssetbundleAsync(MultiVersion)", GUILayout.Width(width), GUILayout.Height(height)))
            {
                // shaco.HotUpdateImportInstance.GetMemory().CreateByMemoryAsyncByUserPath("/Users/liuchang/Desktop/VersionControl@@iOS/assets/resources/1_0_7/1.assetbundle");
                // shaco.HotUpdateImportInstance.GetMemory().onProcessEnd.AddCallBack(this, (sender)=>
                // {
                //     Debug.Log(shaco.HotUpdateImportInstance.GetMemory().Read("TestUI_2"));
                // });
                shaco.ResourcesEx.LoadResourcesOrLocalAsync<Sprite>("configuration/levels_custom", (UnityEngine.Object obj) =>
                {
                    Debug.Log("LoadAssetbundle=" + obj);
                }, (float percent) =>
                {
                    Debug.Log("load progress=" + percent);
                }, multiVersionControlRelativePath);
            }

            if (GUILayout.Button("LoadAssetbundleAsync(Sequeue)", GUILayout.Width(width), GUILayout.Height(height)))
            {
                var resourceSequeue = new shaco.ResourcesOrLocalSequeue();

                resourceSequeue.AddRequest<Sprite>("configuration/album_pic", (UnityEngine.Object obj) =>
                {
                    Debug.Log("LoadAssetbundle album_pic=" + obj);
                }, multiVersionControlRelativePath);
                resourceSequeue.AddRequest("configuration/ad", (UnityEngine.Object obj) =>
                {
                    Debug.Log("LoadAssetbundle ad=" + obj);
                }, multiVersionControlRelativePath);

                resourceSequeue.Start((float percent) =>
                {
                    Debug.Log("load progress=" + percent);
                });
            }

            if (GUILayout.Button("UnloadAssetbundle", GUILayout.Width(width), GUILayout.Height(height)))
            {
                shaco.ResourcesEx.UnloadAssetBundleLocal("configuration/levels_custom", false, multiVersionControlRelativePath);

                //清理全部缓存资源
                // shaco.HotUpdateDataCache.Unload(false);
            }

            if (GUILayout.Button("ExistsResourcesOrLocal", GUILayout.Width(width), GUILayout.Height(height)))
            {
                bool exists = shaco.ResourcesEx.ExistsResourcesOrLocal("configuration/levels_custom", multiVersionControlRelativePath);
                Debug.Log("exists=" + exists);
            }

            if (GUILayout.Button("LoadScene(AssetBundle)", GUILayout.Width(width), GUILayout.Height(height)))
            {
                var m = new shaco.HotUpdateImportMemory();
                m.CreateByMemoryAsyncByUserPath("/Users/liuchang/Desktop/VersionControl@@iOS/assets/ngui/examples/scenes/2.assetbundle");
                m.onProcessEnd.AddCallBack(this, (object sender) =>
                {
                    m.Read("Assets/NGUI/Examples/Scenes/Example 0 - Control Widgets");
                });
            }

            if (GUILayout.Button("LoadSprite(Original)", GUILayout.Width(width), GUILayout.Height(height)))
            {
                var spriteTmp = shaco.ResourcesEx.LoadResourcesOrLocal<Sprite>("1_original", multiVersionControlRelativePath);
                Debug.Log(spriteTmp);
            }
        }
    }
}