using System.Collections;

namespace shaco.Base
{
	[System.Serializable]
	public class AutoValue 
	{
		protected System.Type requestType = typeof(string);

		protected bool       ValueBool         = false;
		protected char       ValueChar         = ' ';
		protected short      ValueShort        = 0;
		protected int        ValueInt          = 0;
		protected long       ValueLong         = 0;
		protected float      ValueFloat        = 0;
		protected double     ValueDouble       = 0;
		protected string     ValueString       = string.Empty;

		public AutoValue(){}
		public AutoValue(object Value)
		{
			Set(Value);
		}

		static readonly protected System.Type TypeBool = typeof(bool);
		static readonly protected System.Type TypeChar = typeof(char);
		static readonly protected System.Type TypeShort = typeof(short);
		static readonly protected System.Type TypeInt = typeof(int);
		static readonly protected System.Type TypeLong = typeof(long);
		static readonly protected System.Type TypeFloat = typeof(float);
		static readonly protected System.Type TypeDouble = typeof(double);
		static readonly protected System.Type TypeString = typeof(string);


        public static implicit operator bool(AutoValue value) { return value.ValueBool; }
        public static implicit operator char(AutoValue value) { return value.ValueChar; }
        public static implicit operator short(AutoValue value) { return value.ValueShort; }
        public static implicit operator int(AutoValue value) { return value.ValueInt; }
        public static implicit operator long(AutoValue value) { return value.ValueLong; }
        public static implicit operator float(AutoValue value) { return value.ValueFloat; }
        public static implicit operator double(AutoValue value) { return value.ValueDouble; }
        public static implicit operator string(AutoValue value) {  return value.ValueString; }

		public object value
		{
			get { return Get(); }
			set { Set(value); }
		}

		public bool IsType(System.Type type)
        {
            return requestType == type;
        }

		virtual public object Get()
		{
			if (requestType == TypeBool) { return (object)ValueBool; }
			else if (requestType == TypeChar) { return (object)ValueChar; }
			else if (requestType == TypeShort) { return (object)ValueShort; }
			else if (requestType == TypeInt) { return (object)ValueInt; }
			else if (requestType == TypeLong) { return (object)ValueLong; }
			else if (requestType == TypeFloat) { return (object)ValueFloat; }
			else if (requestType == TypeDouble) { return (object)ValueDouble; }
			else if (requestType == TypeString) { return (object)ValueString; }
			else 
			{
				return null;
			}
		}

		virtual public bool Set(object Value)
		{
			if (Value == null)
				return false;

			System.Type typeTmp = Value.GetType();

			if (typeTmp == TypeBool) { ValueBool = (bool)Value; }
			else if (typeTmp == TypeChar) { ValueChar = (char)Value; }
			else if (typeTmp == TypeShort) { ValueShort = (short)Value; }
			else if (typeTmp == TypeInt) { ValueInt = (int)Value; }
			else if (typeTmp == TypeLong) { ValueLong = (long)Value; }
			else if (typeTmp == TypeFloat) { ValueFloat = (float)Value; }
			else if (typeTmp == TypeDouble) { ValueDouble = (double)Value; }
			else if (typeTmp == TypeString) { ValueString = (string)Value; }
			else 
			{
				return false;
			}
			requestType = typeTmp;
			return true;
		}
	}

}

