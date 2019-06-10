using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
namespace  RPG.Saving
{
    [ExecuteInEditMode]  //该类的OnGUI和Update等函数在编辑模式也也会被调用
    public class SaveableEntity : MonoBehaviour
    {
        //System.Guid.NewGuid().ToString();
        //GUID： 即Globally Unique Identifier（全球唯一标识符）
        //GUID是一个通过特定算法产生的二进制长度为128位的数字标识符，用于指示产品的唯一性。
        [SerializeField] string uniqueIdentifier = string.Empty;

        /// <summary>
        /// 用于存储所有的SaveableEntity
        /// </summary>
        /// <typeparam name="string"> uniqueIdentifier </typeparam>
        /// <typeparam name="object"> this </typeparam>
        /// <returns></returns>
        static Dictionary<string, SaveableEntity> globalIdentifierDic = new Dictionary<string, SaveableEntity>();

        public void RestoreState(object val)
        {
            Dictionary<string, object> states = (Dictionary<string, object>) val;
            foreach (ISaveble item in GetComponents<ISaveble>())
            {
                //states[item.GetType().ToString()] = item.CaptureState();
                string typeString = item.GetType().ToString();
                if( states.ContainsKey(typeString) )
                {
                    item.RestoreState( states[typeString] );
                }
            }
        }

        public object CaptureState()
        {
            Dictionary<string, object> states = new Dictionary<string, object>();
            foreach (ISaveble item in GetComponents<ISaveble>())
            {
                states[item.GetType().ToString()] = item.CaptureState();
            }
            return states;
        }

        public string GetUniqueIdentifier()
        {
            return uniqueIdentifier;
        }



        private void Update() {
            if(Application.IsPlaying(this.gameObject)) return; //运行时不要赋值，切换完场景会导致改变
            if(string.IsNullOrEmpty(gameObject.scene.path) ) return;//在编辑预制体时（尤其是base prefab时），一定不能赋值，不然子对象都会被附上统一的值，并且这个值也没有机会去改变


            // print("Scene Name:" + gameObject.scene.name);
            // print(gameObject.scene.path);

            SerializedObject serializedObj = new SerializedObject(this);
            SerializedProperty porpurty = serializedObj.FindProperty("uniqueIdentifier");

            if (string.IsNullOrEmpty( porpurty.stringValue ) || !IsUnique(porpurty.stringValue))
            {
                porpurty.stringValue = System.Guid.NewGuid().ToString();
                serializedObj.ApplyModifiedProperties();
            }
            globalIdentifierDic[porpurty.stringValue] = this;
        }

        private bool IsUnique(string cadicate)  //cadicate:候选
        {
            if (!globalIdentifierDic.ContainsKey(cadicate)) return true;  //不在dic中，不包含，说明这个是我特意设置的identifier

            if (globalIdentifierDic[cadicate].Equals(this)) return true;  //包含，并且等于自己，不需要重新赋值

            if (globalIdentifierDic[cadicate] == null) //包含，但是暂时不明白不知道为什么为空，说明原对象已被销毁，下面会重新赋值
            {
                globalIdentifierDic.Remove(cadicate);
                return true;//因为为空，所以不管有没有复制体，都不需要重新设置identifier, 下面只需要重新赋值dic，接下来即使有复制体也会自动使其重新赋值
                            //即，该操作保证了只有一个有效的identifier
                            //说那么多，反正此处如果有为空的数据，移除就好了，剩下的其它地方会好好处理的
            }

            if(globalIdentifierDic[cadicate].GetUniqueIdentifier() != cadicate) 
            {
                globalIdentifierDic.Remove(cadicate);
                return true;
            }

            return false;
        }
    }
}