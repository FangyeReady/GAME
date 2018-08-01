using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticUpdater : AutoStaticInstance<StaticUpdater> {

    public delegate void UpdateDelegateHandler();
    public event UpdateDelegateHandler UpdateEvent;
    public event UpdateDelegateHandler FixUpdateEvent;
    private void Update()
    {
        if (UpdateEvent != null)
        {
            UpdateEvent();
        }
    }

    private void FixedUpdate()
    {
        if (FixUpdateEvent != null)
        {
            FixUpdateEvent();
        }
    }

    public void ClearAllEvents()
    {
        UpdateEvent = null;
        FixUpdateEvent = null;
    }
}
