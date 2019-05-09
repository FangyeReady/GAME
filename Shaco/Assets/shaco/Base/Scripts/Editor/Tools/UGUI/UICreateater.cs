using UnityEngine;
using System.Collections;
using UnityEditor;

namespace shacoEditor
{
    public class UICreateater
    {
        [MenuItem("GameObject/UI/RichText")]
        static public void CreateRichText()
        {
            CreateUIInScene<shaco.RichText>();
        }

        [MenuItem("GameObject/UI/ListView")]
        static public void CreateListView()
        {
            var script = CreateUIInScene<shaco.ListView>();
            script.CheckCompoment();
        }

        [MenuItem("GameObject/UI/ScrollBarEx", false)]
        static public void CreateListViewScrollBarEx()
        {
            var selectGameObject = Selection.activeGameObject;
            var newObj = new GameObject();
            newObj.name = "ScrollBar";
            var background = newObj.AddComponent<UnityEngine.UI.Image>();
#if UNITY_5_3_OR_NEWER
            background.raycastTarget = false;
#endif
            background.color = Color.black;
            shaco.UnityHelper.ChangeParentLocalPosition(newObj, selectGameObject);

            var script = CreateUIInScene<shaco.ScrollBarEx>(newObj);
            script.name = "ScrollBarValue";
            script.handleRect = script.GetComponent<RectTransform>();
#if UNITY_5_3_OR_NEWER
            script.gameObject.AddComponent<UnityEngine.UI.Image>().raycastTarget = false;
#endif            
            script.Background = background;

            var listView = selectGameObject.GetComponent<shaco.ListView>();
            script.AutoSetDirection(listView);
        }

        [MenuItem("GameObject/UI/ScrollBarEx", true)]
        static public bool CreateListViewScrollBarExValid()
        {
            return Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<shaco.ListView>() != null;
        }

        [MenuItem("GameObject/UI/NumberLoopScrollAction")]
        static public void CreateNumberLoopScrollAction()
        {
            CreateUIInScene<shaco.NumberLoopScrollAction>();
        }

        static private T CreateUIInScene<T>(GameObject parent = null) where T : UnityEngine.Component
        {
            T retValue = default(T);
            if (Selection.activeGameObject != null)
            {
                var newObj = new GameObject();
                shaco.UnityHelper.ChangeParentLocalPosition(newObj, parent ?? Selection.activeGameObject);
                retValue = newObj.AddComponent<T>();
                newObj.name = typeof(T).Name;
                Selection.activeGameObject = newObj;

                EditorHelper.SetDirty(Selection.activeGameObject);
            }
            return retValue;
        }
    }
}

