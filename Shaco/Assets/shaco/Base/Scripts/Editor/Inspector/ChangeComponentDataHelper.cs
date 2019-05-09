using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public static class ChangeComponentDataHelper
    {
		public enum ValueType
		{
			String = 0,
			Bool = 1,
            Int = 2,
			Float = 3,
			UnityObject = 4,
			UnityVector2 = 5,
            UnityVector3 = 6,
            UnityVector4 = 7,
            UnityRect = 8,
            UnityColor = 9,
            UnityBounds = 10,
        }

        static private readonly string[] VALUE_TYPES = new string[]
        {
            "String", "Bool", "Int", "Float",
            "UnityObject", "UnityVector2" , "UnityVector3" , "UnityVector4", 
			"UnityRect", "UnityColor" , "UnityBounds"
        };

        /// <summary>
        /// 绘制自动类型输出对象
        /// <param name="autoValue">自动类型数据</param>
        /// <param name="valueType">期望类型</param>
        /// <return>当前数据</return>
        /// </summary>
        static public ValueType DrawValueInput(shaco.AutoValue autoValue, ValueType valueType)
        {
            ValueType retValue = valueType;
            retValue = (ValueType)EditorGUILayout.Popup("ValueType", (int)retValue, VALUE_TYPES);
            switch (valueType)
            {
                case ValueType.Bool: autoValue.Set(EditorGUILayout.Toggle(autoValue)); break;
                case ValueType.Int: autoValue.Set(EditorGUILayout.IntField(autoValue)); break;
                case ValueType.Float: autoValue.Set(EditorGUILayout.FloatField(autoValue)); break;
                case ValueType.String: autoValue.Set(EditorGUILayout.TextArea(autoValue)); break;
                case ValueType.UnityObject: autoValue.Set(EditorGUILayout.ObjectField(autoValue, typeof(Object), true)); break;
                case ValueType.UnityVector2: autoValue.Set(EditorGUILayout.Vector2Field(string.Empty, autoValue)); break;
                case ValueType.UnityVector3: autoValue.Set(EditorGUILayout.Vector3Field(string.Empty, autoValue)); break;
                case ValueType.UnityVector4: autoValue.Set(EditorGUILayout.Vector4Field(string.Empty, autoValue)); break;
                case ValueType.UnityRect: autoValue.Set(EditorGUILayout.RectField(autoValue)); break;
                case ValueType.UnityColor: autoValue.Set(EditorGUILayout.ColorField(autoValue)); break;
                case ValueType.UnityBounds: autoValue.Set(EditorGUILayout.BoundsField(autoValue)); break;
                default: Debug.LogError("ChangeComponentDataHelper DrawValueInput error: unsupport type=" + valueType.ToString()); break;
            }
			return retValue;
        }

        /// <summary>
        /// 获取对象的值
        /// <param name="target">对象</param>
        /// <param name="propertyPath">属性名字</param>
        /// <return>属性值</return>
        /// </summary>
        static public object GetSerializedPropertyValue(UnityEngine.Object target, string propertyPath)
        {
			object retValue = null;
            SerializedProperty serializedProperty = new SerializedObject(target).FindProperty(propertyPath);
			if (null == serializedProperty)
			{
				Debug.LogError("ChangeComponentDataHelper GetSerializedPropertyValue error: not found property by name=" + propertyPath + " target=" + target);
				return retValue;
			}

            switch (serializedProperty.propertyType)
            {
                case SerializedPropertyType.Boolean: retValue = serializedProperty.boolValue; break;
                case SerializedPropertyType.Integer: retValue = serializedProperty.intValue; break;
                case SerializedPropertyType.Float: retValue = serializedProperty.floatValue; break;
                case SerializedPropertyType.String: retValue = serializedProperty.stringValue; break;
                case SerializedPropertyType.ObjectReference: retValue = serializedProperty.objectReferenceValue; break;
                case SerializedPropertyType.Vector2: retValue = serializedProperty.vector2Value; break;
                case SerializedPropertyType.Vector3: retValue = serializedProperty.vector3Value; break;
                case SerializedPropertyType.Vector4: retValue = serializedProperty.vector4Value; break;
                case SerializedPropertyType.Rect: retValue = serializedProperty.rectValue; break;
                case SerializedPropertyType.Color: retValue = serializedProperty.colorValue; break;
                case SerializedPropertyType.Bounds: retValue = serializedProperty.boundsValue; break;
                default: Debug.LogError("ChangeComponentDataHelper GetSerializedPropertyValue error: unsupport type=" + serializedProperty.propertyType); break;
            }
			return retValue;
        }

        /// <summary>
        /// 设置对象的值
        /// <param name="target">对象</param>
        /// <param name="propertyPath">属性名字</param>
        /// <param name="autoValue">自动类型数据</param>
        /// </summary>
        static public void SetSerializedPropertyValue(UnityEngine.Object target, string propertyPath, shaco.AutoValue autoValue)
        {
			SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty serializedProperty = serializedObject.FindProperty(propertyPath);
            if (null == serializedProperty)
            {
                Debug.LogError("ChangeComponentDataHelper SetSerializedPropertyValue error: not found property by name=" + propertyPath + " target=" + target);
                return;
            }

            switch (serializedProperty.propertyType)
            {
                case SerializedPropertyType.Boolean: serializedProperty.boolValue = autoValue; break;
                case SerializedPropertyType.Integer: serializedProperty.intValue = autoValue; break;
                case SerializedPropertyType.Float: serializedProperty.floatValue = autoValue; break;
                case SerializedPropertyType.String: serializedProperty.stringValue = autoValue; break;
                case SerializedPropertyType.ObjectReference:
                    {
                        serializedProperty.objectReferenceValue = autoValue;
                        if (null == serializedProperty.objectReferenceValue && autoValue.IsType(typeof(Texture2D)))
                        {
                            Texture2D texTmp = (Texture2D)autoValue;
                            var filePath = AssetDatabase.GetAssetPath(texTmp);
                            serializedProperty.objectReferenceValue = AssetDatabase.LoadAssetAtPath(filePath, typeof(Sprite)) as Sprite;
                        }
                        break;
                    }
                case SerializedPropertyType.Vector2: serializedProperty.vector2Value = autoValue; break;
                case SerializedPropertyType.Vector3: serializedProperty.vector3Value = autoValue; break;
                case SerializedPropertyType.Vector4: serializedProperty.vector4Value = autoValue; break;
                case SerializedPropertyType.Rect: serializedProperty.rectValue = autoValue; break;
                case SerializedPropertyType.Color: serializedProperty.colorValue = autoValue; break;
                case SerializedPropertyType.Bounds: serializedProperty.boundsValue = autoValue; break;
                default: Debug.LogError("ChangeComponentDataHelper SetSerializedPropertyValue error: unsupport type=" + serializedProperty.propertyType); break;
            }

            serializedObject.ApplyModifiedProperties();
            EditorHelper.SetDirty(target);
        }
    }
}