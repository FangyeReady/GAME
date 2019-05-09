using UnityEngine;
using System.Collections;
using System.Reflection;

namespace shaco.Test
{
    public class TestUI : MonoBehaviour
    {
        public shaco.UIRootComponent uiRoot;
        public shaco.ListView listView;
        public shaco.PageView pageView;
        public GameObject frontArrow;
        public GameObject beginArrow;
        public UnityEngine.UI.Text textOutOfBoundsRate;
        public shaco.NumberLoopScrollAction loopScrollAction;

        public void OnEnable()
        {
            // shaco.Localization.Clear();
            // if (shaco.Base.FileHelper.ExistsFile(Application.dataPath + "/Resources/CONSOLA.TTF"))
            // {
            //     shaco.Localization.SetUnityFont(shaco.ResourcesEx.LoadResourcesOrLocal<Font>("CONSOLA"));
            // }
            // shaco.Localization.SetCurrentLanguage(SystemLanguage.Chinese);
            // shaco.Localization.LoadWithJsonResourcesOrLocalPath("LocalizationTest", false);
        }

        void Start()
        {
            listView.onItemAutoUpdateCallBack = onItemAutoUpdateCallBack;
            listView.onItemsDragOutOfBoundsCallBack = (float rate) =>
            {
                textOutOfBoundsRate.text = rate.ToString();
            };
            listView.onItemWillAutoUpdateCallBack = (int index) =>
            {
                return true;
            };
            listView.onItemsPrepareAutoUpdateCallBack = (int startIndex, int endIndex) =>
            {
                Debug.Log("prepare update item start=" + startIndex + " end=" + endIndex);
                return true;
            };
            listView.onItemsDidAutoUpdateCallBack = (int startIndex, int endIndex) =>
            {
                Debug.Log("did update item start=" + startIndex + " end=" + endIndex);
            };

            listView.InitItemWithAutoUpdate(0, 2);
        }

        public void onItemAutoUpdateCallBack(int index, GameObject item)
        {
            var scriptTmp = item.GetComponent<TestListViewItem>();
            scriptTmp.num = index;
            item.name = scriptTmp.num.ToString();
            scriptTmp.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = scriptTmp.num.ToString();
        }

        GameObject CreateItem(int num)
        {
            var ret = listView.PopItemFromCacheOrCreateFromModel();
            ret.GetComponent<TestListViewItem>().num = num;
            ret.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = num.ToString();
            return ret;
        }

        void Update()
        {
            // 返回键  
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                shaco.UIManager.CustomUI(new TestArg());
            }
        }

        void OnGUI()
        {
            float width = 160;
            float height = 30;

#if !UNITY_EDITOR
		width *= 4;
		height *= 4;
#endif

            GUILayoutOption[] optionTmp = new GUILayoutOption[] { GUILayout.Width(width), GUILayout.Height(height) };


            if (GUILayout.Button("PageView(add)", optionTmp))
            {
                pageView.AddItembyModel();
            }

            if (GUILayout.Button("PageView(remove)", optionTmp))
            {
                pageView.RemoveItem(0);
            }

            if (GUILayout.Button("preload_ui", optionTmp))
            {
                shaco.UIManager.PreLoadUI<TestUI_2>(new TestArg());
            }

            if (GUILayout.Button("show_ui", optionTmp))
            {
                shaco.UIManager.OpenUI<TestUI_2>(new TestArg());
            }

            if (GUILayout.Button("add_ui_callback", optionTmp))
            {
                this.gameObject.AddEvent<shaco.UIStateChangedArgs.OnUIResumeArg>(
                    (object sender, shaco.Base.BaseEventArg arg) =>
                    {
                        var argTmp = arg as shaco.UIStateChangedArgs.OnUIResumeArg;
                        Debug.Log("resume sender=" + sender + " key=" + argTmp.uiKey + " target=" + argTmp.uiTarget);
                    });

                this.gameObject.AddEvent<shaco.UIStateChangedArgs.OnUIHideArg>(
                    (object sender, shaco.Base.BaseEventArg arg) =>
                    {
                        var argTmp = arg as shaco.UIStateChangedArgs.OnUIHideArg;
                        Debug.Log("hide sender=" + sender + " key=" + argTmp.uiKey + " target=" + argTmp.uiTarget);
                    });
            }

            if (GUILayout.Button("hide_ui", optionTmp))
            {
                shaco.UIManager.HideUI<TestUI_2>(new TestArg());
            }

            if (GUILayout.Button("close_ui", optionTmp))
            {
                shaco.UIManager.CloseUI<TestUI_2>(new TestArg());
            }

            if (GUILayout.Button("popup_hide", optionTmp))
            {
                shaco.UIManager.PopupUIAndHide(new TestArg(), 1);
            }

            if (GUILayout.Button("popup_close", optionTmp))
            {
                shaco.UIManager.PopupUIAndClose(new TestArg(), 1);
            }

            if (GUILayout.Button("gettop_ui", optionTmp))
            {
                Debug.Log("top_ui=" + shaco.UIManager.GetTopUI(true, 1).key);
            }

            if (GUILayout.Button("gettop_active_ui", optionTmp))
            {
                Debug.Log("top_ui=" + shaco.UIManager.GetTopUI(false, 1).key);
            }

            if (GUILayout.Button("test_loop_scroll", optionTmp))
            {
                loopScrollAction.text = shaco.Base.Utility.Random(10, 100000).ToString();
            }
        }
    }
}