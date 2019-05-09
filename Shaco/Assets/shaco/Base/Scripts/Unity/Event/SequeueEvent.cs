using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class SequeueEvent : shaco.Base.BaseTree
    {
        public class CoroutineInfo
        {
            public IEnumerator coroutine;
            public string name = string.Empty;
        }

        public CoroutineInfo _coroutineInfo = new CoroutineInfo();
        public SequeueEvent _rootEvent = null;
        public SequeueEventComponent _actionTarget = null;

        private readonly string[] _className = new string[]
        {
            "shaco/Base/Scripts/CSharp/Event/EventManagerExtensionsClass.cs",
            "shaco/Base/Scripts/CSharp/Event/SequeueEvent.cs"
        };

        public SequeueEvent AppendEvent(IEnumerator coroutine)
        {
            SequeueEvent retValue = new SequeueEvent();

            if (_rootEvent == null)
            {
                _rootEvent = new SequeueEvent();
            }
            _rootEvent.AddChild(retValue);

            retValue._coroutineInfo.coroutine = coroutine;
            retValue._coroutineInfo.name = "Index=" + (_rootEvent.Count);
            retValue.name = retValue._coroutineInfo.name;
            retValue._rootEvent = _rootEvent;

            return retValue;
        }

        public void StopEvent()
        {
            if (null != _actionTarget)
            {
                MonoBehaviour.Destroy(_actionTarget.gameObject);
                _actionTarget = null;
                _rootEvent.RemoveChildren();
            }
        }

        public SequeueEventComponent Run()
        {
            var firstEvent = _rootEvent.child as SequeueEvent;

            if (null == firstEvent._actionTarget)
            {
                var newObj = new GameObject();
                newObj.name = "SequeueEventTemp";
                firstEvent._actionTarget = newObj.AddComponent<SequeueEventComponent>();
                firstEvent._actionTarget.statckLocation.GetStatck(_className);
                shaco.UnityHelper.ChangeParent(firstEvent._actionTarget.gameObject, ActionS.GetDelegateInvoke());
            }

            var eventsTmp = new List<CoroutineInfo>();
            firstEvent.ForeachSibling((shaco.Base.BaseTree tree) =>
            {
                var evetTmp = (SequeueEvent)tree;
                eventsTmp.Add(evetTmp._coroutineInfo);
                return true;
            });

            firstEvent._actionTarget.StartCoroutine(firstEvent._actionTarget.Run(firstEvent, eventsTmp));
            return firstEvent._actionTarget;
        }
    }
}