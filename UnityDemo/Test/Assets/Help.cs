using UnityEngine;

namespace Test.Desc
{
    
    [CreateAssetMenu(fileName = "Help", menuName = "Test/Help", order = 0)]
    public class Help : ScriptableObject {
        
        public void ShowLog()
        {
            Debug.Log ("1. Shift + Ctrl + 鼠标  使选中的模型吸附于其它模型（Toggle Tool需要设为Center)\n2. 选择模型 按住V键，并设置顶点即可使对象之间以指定顶点的吸附");
        }
    }

}