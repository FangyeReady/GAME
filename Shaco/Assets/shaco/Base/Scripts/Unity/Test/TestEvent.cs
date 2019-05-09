using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Threading;

namespace shaco.Test
{
    public class TestEvent : MonoBehaviour
    {
        public GameObject Target;
        public EventDelegateS ss;
        public Texture2D tt;

        private shaco.Base.EventCallBack _eventCallBack = new shaco.Base.EventCallBack();
        private shaco.Base.EventCallBack<int> _eventCallBackArg = new shaco.Base.EventCallBack<int>();
        private System.Collections.Generic.List<GameObject> _listTmp = new System.Collections.Generic.List<GameObject>();

        void cc()
        {
            _eventCallBack.ClearCallBack();
            _eventCallBackArg.ClearCallBack();

            int a = 2;
            shaco.Base.EventManager.AddEvent<TestArg>(a, callfunc1, true);
            this.AddAutoRealeaseEvent<TestArg2>(callfunc2);
            _eventCallBack.AddCallBack(this, callfunc3);
            _eventCallBackArg.AddCallBack(this, callfunc4);
            Target.AddEvent<TestArg2>(callfunc2_2);
        }

        void OnGUI()
        {
            float width = 160;
            float height = 30;

#if !UNITY_EDITOR
		width *= 4;
		height *= 4;
#endif

            GUILayoutOption[] layoutOptionTmp = new GUILayoutOption[] { GUILayout.Width(width), GUILayout.Height(height) };

            if (GUILayout.Button("AddCallBack", layoutOptionTmp))
            {
                cc();
            }

            if (GUILayout.Button("invoke 1", layoutOptionTmp))
            {
                var argTmp = new TestArg();
                this.InvokeEvent(argTmp);
            }

            if (GUILayout.Button("invoke 2", layoutOptionTmp))
            {
                shaco.Base.EventManager.InvokeEvent(new TestArg2());
            }

            if (GUILayout.Button("remove event id with sender", layoutOptionTmp))
            {
                Target.RemoveEvent<TestArg2>();
            }

            if (GUILayout.Button("remove event id", layoutOptionTmp))
            {
                shaco.Base.EventManager.RemoveEvent<TestArg2>();
            }

            if (GUILayout.Button("remove sender all", layoutOptionTmp))
            {
                Target.RemoveAllEvent();
            }

            if (GUILayout.Button("remove callback one 1", layoutOptionTmp))
            {
                shaco.Base.EventManager.RemoveEvent<TestArg>(callfunc1);
            }

            if (GUILayout.Button("remove callback one 2", layoutOptionTmp))
            {
                shaco.Base.EventManager.RemoveEvent<TestArg2>(callfunc2);
            }

            if (GUILayout.Button("destroy target", layoutOptionTmp))
            {
                Destroy(Target.gameObject);
            }

            if (GUILayout.Button("remove current event manager", layoutOptionTmp))
            {
                shaco.Base.EventManager.RemoveCurrentEventManager();
                for (int i = 0; i < _listTmp.Count; ++i)
                {
                    Destroy(_listTmp[i]);
                }
                _listTmp.Clear();
            }

            if (GUILayout.Button("invoke sequeue event", layoutOptionTmp))
            {
                this.InvokeSequeueEvent(do1(), do2());
            }

#if UNITY_EDITOR
            if (GUILayout.Button("location event", layoutOptionTmp))
            {
                if (_eventCallBack.Count > 0)
                {
                    var path = _eventCallBack[0].CallAddEventStack.statck;
                    var line = _eventCallBack[0].CallAddEventStack.statckLine;
                    var indexTmp = path.IndexOf("Assets/");
                    if (indexTmp >= 0)
                        path = path.Substring(indexTmp, path.Length - indexTmp);
                    UnityEditor.AssetDatabase.OpenAsset(UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)), line);

                }
            }

            if (GUILayout.Button("location event arg", layoutOptionTmp))
            {
                if (_eventCallBackArg.Count > 0)
                {
                    var path = _eventCallBackArg[0].CallAddEventStack.statck;
                    var line = _eventCallBackArg[0].CallAddEventStack.statckLine;
                    var indexTmp = path.IndexOf("Assets/");
                    if (indexTmp >= 0)
                        path = path.Substring(indexTmp, path.Length - indexTmp);
                    UnityEditor.AssetDatabase.OpenAsset(UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)), line);
                }
            }
#endif

        }
        public int aa = 100;

        void callfunc1(object sender, shaco.Base.BaseEventArg arg)
        {
            if (arg as TestArg != null)
            {
                shaco.Log.Info("1 event id=" + arg.eventID + " message=" + (arg as TestArg).message + " target=" + this);
            }
            else
            {
                shaco.Log.Info("1 event id=" + arg.eventID + " message=" + (arg as TestArg2).message + " target=" + this);
            }

            shaco.Base.EventManager.RemoveEvent<TestArg>();

            shaco.Base.EventManager.AddEvent<TestArg>(aa, callfunc1, false);
            shaco.Base.EventManager.RemoveAllEvent(aa);

            float b = 3;
            shaco.Base.EventManager.AddEvent<TestArg>(b, callfunc1, false);

            shaco.Base.EventManager.AddEvent<TestArg>(this, callfunc1, false);
            this.RemoveAllEvent();
        }

        void callfunc2(object sender, shaco.Base.BaseEventArg arg)
        {
            shaco.Log.Info("2 event id=" + arg.eventID + " message=" + (arg as TestArg2).message + " target=" + this);
        }

        void callfunc2_2(object sender, shaco.Base.BaseEventArg arg)
        {
            shaco.Log.Info("2_2 event id=" + arg.eventID + " message=" + (arg as TestArg2).message + " target=" + this);
        }

        void callfunc3(object sender)
        {
            shaco.Log.Info("3 sender=" + sender);
        }

        void callfunc4(object sender, int arg)
        {
            shaco.Log.Info("4 sender=" + sender + " arg=" + arg);
        }

        IEnumerator do1()
        {
            yield return new WaitForSeconds(1.0f);
            Debug.Log("1111");
        }

        IEnumerator do2()
        {
            yield return new WaitForSeconds(3.0f);
            Debug.Log("2222");
        }
    }
}