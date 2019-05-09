//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace shacoEditor
{
    [CustomEditor(typeof(EventDelegateS))]
    public class UIEventDelegateEditorS : Editor
    {
        //PlayBandMouseClick mTrigger;

        //void OnEnable ()
        //{
        //    mTrigger = target as PlayBandMouseClick;
        //    EditorPrefs.SetBool("PlayBandET6", PlayBandEventDelegate.IsValid(mTrigger.OnClick.ListDelegate));
        //}

        //public override void OnInspectorGUI ()
        //{
        //    GUILayout.Space(3f);
        //    PlayBandNGUIEditorTools.SetLabelWidth(80f);
        //    bool minimalistic = PlayBandNGUIEditorTools.minimalisticLook;
        //    DrawEvents("PlayBandET6", "On Click", mTrigger.OnClick.ListDelegate, minimalistic);
        //}

        //void DrawEvents (string key, string text, List<PlayBandEventDelegate> list, bool minimalistic)
        //{
        //    if (!PlayBandNGUIEditorTools.DrawHeader(text, key, false, minimalistic)) return;
        //    PlayBandNGUIEditorTools.BeginContents();
        //    PlayBandEventDelegateEditor.Field(mTrigger, list, null, null, minimalistic);
        //    PlayBandNGUIEditorTools.EndContents();
        //}
    }
}
#endif