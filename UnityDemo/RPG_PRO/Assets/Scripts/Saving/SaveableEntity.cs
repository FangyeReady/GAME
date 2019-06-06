using System;
using UnityEditor;
using UnityEngine;

namespace  RPG.Saving
{
    [ExecuteInEditMode]  //该类的OnGUI和Update等函数在编辑模式也也会被调用
    public class SaveableEntity : MonoBehaviour
    {

        [SerializeField] string uniqueIdentifier = string.Empty;     //System.Guid.NewGuid().ToString();
                                                                     //GUID： 即Globally Unique Identifier（全球唯一标识符）
                                                                     //GUID是一个通过特定算法产生的二进制长度为128位的数字标识符，用于指示产品的唯一性。


        public void SetState(object v)
        {
            SerializeableVector3 pos = (SerializeableVector3) v;
           this.transform.position = pos.ToVector3();
        }

        public string GetUniqueIdentifier()
        {
            return uniqueIdentifier;
        }

        public object GetState()
        {
            return new SerializeableVector3( this.transform.position );
        }


        private void Update() {
            if(Application.IsPlaying(this.gameObject)) return; //运行时不要赋值，切换完场景会导致改变
            if(string.IsNullOrEmpty(gameObject.scene.path) ) return;//在编辑预制体时（尤其是base prefab时），一定不能赋值，不然子对象都会被附上统一的值，并且这个值也没有机会去改变


            // print("Scene Name:" + gameObject.scene.name);
            // print(gameObject.scene.path);

            SerializedObject serializedObj = new SerializedObject(this);
            SerializedProperty porpurty = serializedObj.FindProperty("uniqueIdentifier");

            if (string.IsNullOrEmpty( porpurty.stringValue ) )
            {
                porpurty.stringValue = System.Guid.NewGuid().ToString();
                serializedObj.ApplyModifiedProperties();
            }
        }
    }
}