using UnityEngine;
using System.Collections;

namespace shaco
{
	[System.Serializable]
	public class AutoValue : shaco.Base.AutoValue
	{
		private Object     ValueUnityObject      = null;
		private Vector2    ValueUnityVector2     = Vector2.zero;
		private Vector3    ValueUnityVector3     = Vector3.zero;
		private Vector4    ValueUnityVector4     = Vector4.zero;
		private Rect       ValueUnityRect        = new Rect();
		private Color      ValueUnityColor       = new Color();
		private Bounds     ValueUnityBounds      = new Bounds();
        private Quaternion ValueUnityQuaternion  = new Quaternion();

		static readonly private System.Type TypeUnityObject = typeof(Object);
		static readonly private System.Type TypeUnityVector2 = typeof(Vector2);
		static readonly private System.Type TypeUnityVector3 = typeof(Vector3);
		static readonly private System.Type TypeUnityVector4 = typeof(Vector4);
		static readonly private System.Type TypeUnityRect = typeof(Rect);
		static readonly private System.Type TypeUnityColor = typeof(Color);
		static readonly private System.Type TypeUnityBounds = typeof(Bounds);
        static readonly private System.Type TypeUnityQuaternion = typeof(Quaternion);

        public static implicit operator Object(shaco.AutoValue value) { return value.ValueUnityObject; }
        public static implicit operator Vector2(shaco.AutoValue value) { return value.ValueUnityVector2; }
        public static implicit operator Vector3(shaco.AutoValue value) { return value.ValueUnityVector3; }
        public static implicit operator Vector4(shaco.AutoValue value) { return value.ValueUnityVector4; }
        public static implicit operator Rect(shaco.AutoValue value) { return value.ValueUnityRect; }
        public static implicit operator Color(shaco.AutoValue value) { return value.ValueUnityColor; }
        public static implicit operator Bounds(shaco.AutoValue value) { return value.ValueUnityBounds; }
        public static implicit operator Quaternion(shaco.AutoValue value) { return value.ValueUnityQuaternion; }

        public override object Get()
		{
			object retValue = base.Get();
			if (null == retValue)
			{
				if (requestType == TypeUnityObject) { retValue = (object)ValueUnityObject; }
                else if (requestType == TypeUnityVector2) { retValue = (object)ValueUnityVector2; }
                else if (requestType == TypeUnityVector3) { retValue = (object)ValueUnityVector3; }
                else if (requestType == TypeUnityVector4) { retValue = (object)ValueUnityVector4; }
                else if (requestType == TypeUnityRect) { retValue = (object)ValueUnityRect; }
                else if (requestType == TypeUnityColor) { retValue = (object)ValueUnityColor; }
                else if (requestType == TypeUnityBounds) { retValue = (object)ValueUnityBounds; }
                else if (requestType == TypeUnityQuaternion) { retValue = (object)ValueUnityQuaternion; }
                else
                {
                    Log.Error("AutoValue Get error: unsupport value requestType=" + requestType);
                }
			}
			return retValue;
		}

		public override bool Set(object Value)
		{
			if (null == Value)
			{
				return false;
			}

			bool retValue = base.Set(Value);
			if (!retValue)
			{
				System.Type typeTmp = Value.GetType();
                if (typeTmp.IsInherited<UnityEngine.Object>()) { ValueUnityObject = (Object)Value; }
                else if (typeTmp == TypeUnityVector2) { ValueUnityVector2 = (Vector2)Value; }
                else if (typeTmp == TypeUnityVector3) { ValueUnityVector3 = (Vector3)Value; }
                else if (typeTmp == TypeUnityVector4) { ValueUnityVector4 = (Vector4)Value; }
                else if (typeTmp == TypeUnityRect) { ValueUnityRect = (Rect)Value; }
                else if (typeTmp == TypeUnityColor) { ValueUnityColor = (Color)Value; }
                else if (typeTmp == TypeUnityBounds) { ValueUnityBounds = (Bounds)Value; }
                else if (typeTmp == TypeUnityQuaternion) { ValueUnityQuaternion = (Quaternion)Value; }
                else
                {
                    Log.Error("AutoValue Set error: unsupport value type=" + typeTmp.ToString());
					retValue = false;
                }
                requestType = typeTmp;
			}
			return retValue;
		}
	}

}

