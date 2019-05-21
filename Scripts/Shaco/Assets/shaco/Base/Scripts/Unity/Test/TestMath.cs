using UnityEngine;
using System.Collections;

namespace shaco.Test
{
    public class TestMath : MonoBehaviour
    {

        void OnGUI()
        {
            float width = 160;
            float height = 30;

#if !UNITY_EDITOR
		width *= 4;
		height *= 4;
#endif

            GUILayoutOption[] layoutOptionTmp = new GUILayoutOption[] { GUILayout.Width(width), GUILayout.Height(height) };

            if (GUILayout.Button("simple", layoutOptionTmp))
            {
                var value = shaco.Base.Operator.CalculateDouble("3.5 + 4 * 2.6");
                Debug.Log("value=" + value);
            }
            if (GUILayout.Button("pow", layoutOptionTmp))
            {
                var value = shaco.Base.Operator.CalculateDouble("(4 ^ 2) / 8");
                Debug.Log("value=" + value);
            }
            if (GUILayout.Button("complex", layoutOptionTmp))
            {
                var value = shaco.Base.Operator.CalculateDouble("-5.0 * (3.2 + 4 / 2) - -10.9");
                Debug.Log("value=" + value);
            }
        }
    }
}