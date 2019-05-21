using UnityEngine;
using System.Collections;

namespace shaco.Test
{
    [shaco.UILayer(1, true, ResourceUpdateTest.multiVersionControlRelativePath)]
    public class TestUI_2 : MonoBehaviour
    {
        static int aa = 0;

        void OnUIPreLoad(shaco.Base.BaseEventArg arg)
        {
            var testArg = arg as TestArg;
            Debug.Log("OnUIPreLoad====" + testArg.message + " name=" + this.name);
        }

        void OnUIInit(shaco.Base.BaseEventArg arg)
        {
            var testArg = arg as TestArg;
            Debug.Log("OnUIInit====" + testArg.message + " name=" + this.name);
            Debug.Log("load assetbundle=" + shaco.ResourcesEx.LoadResourcesOrLocal(this.AutoUnload("configuration/levels_custom")));
            shaco.ResourcesEx.LoadResourcesOrLocalAsync(this.AutoUnload("configuration/ad"), (Object readObj) =>
            {
                Debug.Log("load async assetbundle=" + readObj);
            });
            this.name = aa++.ToString();
        }

        void OnUIOpen(shaco.Base.BaseEventArg arg)
        {
            var testArg = arg as TestArg;
            Debug.Log("OnUIOpen====" + testArg.message + " name=" + this.name);
        }

        void OnUIHide(shaco.Base.BaseEventArg arg)
        {
            var testArg = arg as TestArg;
            Debug.Log("OnUIHide====" + testArg.message + " name=" + this.name);
        }
        void OnUIResume(shaco.Base.BaseEventArg arg)
        {
            var testArg = arg as TestArg;
            Debug.Log("OnUIResume====" + testArg.message + " name=" + this.name);
        }
        void OnUIClose(shaco.Base.BaseEventArg arg)
        {
            var testArg = arg as TestArg;
            Debug.Log("OnUIClose====" + testArg.message + " name=" + this.name);
        }

        public void OnUICustom(shaco.Base.BaseEventArg arg)
        {
            var testArg = arg as TestArg;
            Debug.Log("OnUICustom====" + testArg.message + " name=" + this.name);
        }

        public void OnClickHideMe()
        {
            shaco.UIManager.HideUITarget<TestUI_2>(this, new TestArg());
        }

        public void OnClickCloseMe()
        {
            shaco.UIManager.CloseUITarget<TestUI_2>(this, new TestArg());
        }

        void Start()
        {

        }
    }
}