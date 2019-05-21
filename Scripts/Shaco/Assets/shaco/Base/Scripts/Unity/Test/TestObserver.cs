using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco.Test
{
    public class TestObserver : MonoBehaviour
    {
        /// <summary>
        /// 自定义的数据主体
        /// </summary>
        public class CustomSubject<T> : shaco.Base.Subject<T>
        {
            public override void Notify()
            {
                base.Notify();
                base.Notify();
            }
        }

        /// <summary>
        /// 自定义数据观察对象
        /// </summary>
        public class CustomObserver<T> : shaco.Base.Observer<T>
        {
            public override void OnUpdateCallBack()
            {
                base.OnUpdateCallBack();
                Debug.Log("custom observer todo.....");
            }
        }

        public shaco.Base.ISubject<int> data1 = new shaco.Base.Subject<int>();
        public shaco.Base.ISubject<float> data2 = new CustomSubject<float>();
        public shaco.Base.ISubject<bool> data3 = new shaco.Base.Subject<bool>();

        private GameObject _tmpTarget = null;

        void Start()
        {
            _tmpTarget = new GameObject();

            // var t = this.GetType();
            // var b = t.GetField("data1");
            // Debug.Log(b);

            data1.OnValueUpdate((int value) =>
            {
                Debug.Log("update data12=" + value);
            }).OnValueInit((int value) =>
            {
                Debug.Log("init data12=" + value);
            }).Start(this);

            data1.OnSubjectValueUpdate((subject) =>
            {
                Debug.Log("update data22=" + subject.value);
            }).Start(this);

            data2.OnValueUpdate<float, CustomObserver<float>>(fffffff).Start();

            data3.OnSubjectValueUpdate(subject => Debug.Log("data3=" + subject.value)).Start(_tmpTarget);
        }

        private void fffffff(shaco.Base.ISubject<float> subject)
        {
            Debug.Log("data2=" + subject.value);
        }

        void OnGUI()
        {
            if (GUILayout.Button("Change data 1"))
            {
                data1.value = shaco.Base.Utility.Random();
            }

            if (GUILayout.Button("Change data 2"))
            {
                data2.value = shaco.Base.Utility.Random();
            }

            if (GUILayout.Button("Change data 3"))
            {
                data3.value = shaco.Base.Utility.Random(0, 1) == 0 ? false : true;
            }

            if (GUILayout.Button("Destroy and auto release 1"))
            {
                MonoBehaviour.Destroy(_tmpTarget);
            }
        }
    }
}