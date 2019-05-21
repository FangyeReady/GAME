using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public partial class ListView : MonoBehaviour
    {
        private string strItemIndex = "0";
        void OnGUI()
        {
            if (!openDebugMode)
                return;

            float spaceTmp = Screen.width / 3;

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(spaceTmp);
                strItemIndex = GUILayout.TextField(strItemIndex);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(spaceTmp);
                if (GUILayout.Button("InterItem"))
                {
                    int indexTmp = 0;
                    int.TryParse(strItemIndex, out indexTmp);
                    var model = GetItemModel();
                    InsertItem(model, indexTmp);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(spaceTmp);
                if (GUILayout.Button("RemoveItem"))
                {
                    int indexTmp = 0;
                    int.TryParse(strItemIndex, out indexTmp);
                    RemoveItem(indexTmp);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(spaceTmp);
                if (GUILayout.Button("ClearItem"))
                {
                    ClearItem();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(spaceTmp);
                if (GUILayout.Button("LocationRandom(Action)"))
                {
                    int randIndex = shaco.Base.Utility.Random(GetItemStartDisplayIndex(), GetItemEndDisplayIndex() + 1);
                    LocationActionByItemIndex(randIndex, 1.0f);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(spaceTmp);
                if (GUILayout.Button("LocationByIndex"))
                {
                    int indexTmp = 0;
                    int.TryParse(strItemIndex, out indexTmp);
                    LocationByItemIndex(indexTmp);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(spaceTmp);
                if (GUILayout.Button("LocationToFront(Action)"))
                {
                    LocationActionByItemIndex(GetItemEndDisplayIndex() + shaco.Base.Utility.Random(2, 2), 1.0f);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(spaceTmp);
                if (GUILayout.Button("LocationToBehind(Action)"))
                {
                    LocationActionByItemIndex(GetItemStartDisplayIndex() - shaco.Base.Utility.Random(2, 2), 1.0f);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(spaceTmp);
                if (GUILayout.Button("LocationActionByWorldPosition"))
                {
                    var randNum = shaco.Base.Utility.Random(0, _listItems.Count);
                    LocationActionByGameObject(GetItem(randNum), 1.0f);
                }
            }
            GUILayout.EndHorizontal();
        }

        void OnDrawGizmos()
        {
            if (!openDebugMode)
                return;

            Gizmos.color = new Color(1, 1, 1, 0.5f);

            var content = _scrollRectContent.content;
            if (content)
            {
                var trans = content.GetComponent<RectTransform>();
                if (trans)
                {
                    Vector3 worldSize = trans.TransformVector(trans.sizeDelta);
                    worldSize.z = 0;
                    Gizmos.DrawCube(trans.position, worldSize);
                }
            }
        }
    }
}