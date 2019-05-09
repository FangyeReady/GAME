//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

#if UNITY_EDITOR || !UNITY_FLASH
#define REFLECTION_SUPPORT
#endif

#if REFLECTION_SUPPORT
using System.Reflection;
#endif

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Delegate callback that Unity can serialize and set via Inspector.
/// </summary>

[System.Serializable]
public class EventDelegateS
{
	//add by shaco 2016/6/12
	[System.Serializable]
	public class ListEventDelegate
	{
		public List<EventDelegateS> ListDelegate = new List<EventDelegateS>();

		public void AddCallBack(MonoBehaviour target, string methodName)
		{
			if (HasCallBack(target, methodName))
			{
				Debug.LogError("ListEventDelegate AddCallBack error: Has been added this callback target=" + target + " methodName=" + methodName);
				return;
			}

			CheckValid();

			EventDelegateS newDelegate = new EventDelegateS(target, methodName);
			ListDelegate.Add(newDelegate);
		}

		public void RemoveCallBack(MonoBehaviour target, bool removeAll = true)
		{
			for (int i = ListDelegate.Count - 1; i >= 0; --i)
			{
				if (target == ListDelegate[i].target)
				{
					ListDelegate.RemoveAt(i);
					if (!removeAll)
						break;
				}
			}
		}

		public bool HasCallBack(MonoBehaviour target, string methodName)
		{
			for (int i = 0; i < ListDelegate.Count; ++i)
			{
				var delegateTmp = ListDelegate[i];
				if (delegateTmp.target == target)
				{
					if (delegateTmp.methodName == methodName)
						return true;
				}
			}
			return false;
		}

		public void Execute()
		{
			EventDelegateS.Execute(ListDelegate);
		}

		public void SetParamater(int index, params object[] paramaters)
		{
			if (index < 0 || index > ListDelegate.Count - 1)
			{
				Debug.LogError("SetParamater error: out of ListDelegate range");
				return;
			}

			CheckValid();

			ListDelegate[index].SetParammater(paramaters);
		}

		public bool HasInvalidParamater(int index)
		{
			if (index < 0 || index > ListDelegate.Count - 1)
			{
				Debug.LogError("isValidParamater error: out of ListDelegate range");
				return true;
			}

			if (ListDelegate[index] == null)
				return true;

			for (int i = ListDelegate[index].mParameters.Length - 1; i >= 0; --i)
			{
				if (ListDelegate[index].mParameters[i] == null)
					return true;
			}
			return false;
		}

		public void CheckValid()
		{
			for (int i = ListDelegate.Count - 1; i >= 0; --i)
			{
				if (ListDelegate[i] == null || ListDelegate[i].target == null || string.IsNullOrEmpty(ListDelegate[i].methodName))
				{
					Debug.LogWarning("EventDelegateS CheckValid warning: have invalid target or method");
					ListDelegate.RemoveAt(i);
				}
			}
		}

		public int Count
		{
			get {return ListDelegate.Count;}
		}
	}
	//add end

	/// <summary>
	/// Delegates can have parameters, and this class makes it possible to save references to properties
	/// that can then be passed as function arguments, such as transform.position or widget.color.
	/// </summary>

	[System.Serializable]
	public class Parameter
	{
		public Object obj;
		public string field;

		public Parameter () { }
		public Parameter (Object obj, string field) { this.obj = obj; this.field = field; }
		public Parameter (object val) { mValue = val; }

		[System.NonSerialized] private object mValue;

		#if REFLECTION_SUPPORT
		[System.NonSerialized] public System.Type expectedType = typeof(void);

		// Cached values
		[System.NonSerialized] public bool cached = false;
		[System.NonSerialized] public PropertyInfo propInfo;
		[System.NonSerialized] public FieldInfo fieldInfo;

		/// <summary>
		/// Return the property's current value.
		/// </summary>
		[SerializeField] private shaco.AutoValue ValueAuto = new shaco.AutoValue();

		public object value
		{
			get
			{
				if (ValueAuto != null) return ValueAuto.value;

				if (mValue != null) return mValue;

				if (!cached)
				{
					cached = true;
					fieldInfo = null;
					propInfo = null;

					if (obj != null && !string.IsNullOrEmpty(field))
					{
						System.Type type = obj.GetType();
		#if NETFX_CORE
		propInfo = type.GetRuntimeProperty(field);
		if (propInfo == null) fieldInfo = type.GetRuntimeField(field);
		#else
						propInfo = type.GetProperty(field);
						if (propInfo == null) fieldInfo = type.GetField(field);
		#endif
					}
				}
				if (propInfo != null) return propInfo.GetValue(obj, null);
				if (fieldInfo != null) return fieldInfo.GetValue(obj);
				if (obj != null) return obj;
		#if !NETFX_CORE
				if (expectedType != null && expectedType.IsValueType) return null;
		#endif
				return System.Convert.ChangeType(null, expectedType);
			}
			set
			{
				ValueAuto.value = value;

				mValue = value;
			}
		}

		/// <summary>
		/// Parameter type -- a convenience function.
		/// </summary>

		public System.Type type
		{
			get
			{
				if (mValue != null) return mValue.GetType();
				if (obj == null) return typeof(void);
				return obj.GetType();
			}
		}
		#else // REFLECTION_SUPPORT
		public object value { get { if (mValue != null) return mValue; return obj; } }
		#if UNITY_EDITOR || !UNITY_FLASH
		public System.Type type { get { if (mValue != null) return mValue.GetType(); return typeof(void); } }
		#else
		public System.Type type { get { if (mValue != null) return mValue.GetType(); return null; } }
		#endif
		#endif
	}

	[SerializeField] MonoBehaviour mTarget;
	[SerializeField] string mMethodName;
	[SerializeField] Parameter[] mParameters;

	//add by shaco 2016/12/16
	public enum ReturnType
	{
		Void,
		Bool,
		Int,
		Float,
		String
	}
	[SerializeField] ReturnType mRequstMethodReturnType = ReturnType.Void;
	//add end

	/// <summary>
	/// Whether the event delegate will be removed after execution.
	/// </summary>

	public bool oneShot = false;

	// Private variables
	public delegate void Callback();
	[System.NonSerialized] Callback mCachedCallback;
	[System.NonSerialized] bool mRawDelegate = false;
	[System.NonSerialized] bool mCached = false;
	#if REFLECTION_SUPPORT
	[System.NonSerialized] MethodInfo mMethod;
	[System.NonSerialized] ParameterInfo[] mParameterInfos;
	[System.NonSerialized] object[] mArgs;
	#endif

	/// <summary>
	/// Event delegate's target object.
	/// </summary>

	public MonoBehaviour target
	{
		get
		{
			return mTarget;
		}
		set
		{
			mTarget = value;
			mCachedCallback = null;
			mRawDelegate = false;
			mCached = false;
			#if REFLECTION_SUPPORT
			mMethod = null;
			mParameterInfos = null;
			#endif
			mParameters = null;
		}
	}

	/// <summary>
	/// Event delegate's method name.
	/// </summary>

	public string methodName
	{
		get
		{
			return mMethodName;
		}
		set
		{
			mMethodName = value;
			mCachedCallback = null;
			mRawDelegate = false;
			mCached = false;
			#if REFLECTION_SUPPORT
			mMethod = null;
			mParameterInfos = null;
			#endif
			mParameters = null;
		}
	}

	/// <summary>
	/// Optional parameters if the method requires them.
	/// </summary>

	public Parameter[] parameters
	{
		get
		{
			#if UNITY_EDITOR
			if (!mCached || !Application.isPlaying) Cache();
			#else
			if (!mCached) Cache();
			#endif
			return mParameters;
		}
	}

	/// <summary>
	/// Whether this delegate's values have been set.
	/// </summary>

	public bool isValid
	{
		get
		{
			#if UNITY_EDITOR
			if (!mCached || !Application.isPlaying) Cache();
			#else
			if (!mCached) Cache();
			#endif
			return (mRawDelegate && mCachedCallback != null) || (mTarget != null && !string.IsNullOrEmpty(mMethodName));
		}
	}

	/// <summary>
	/// Whether the target script is actually enabled.
	/// </summary>

	public bool isEnabled
	{
		get
		{
			#if UNITY_EDITOR
			if (!mCached || !Application.isPlaying) Cache();
			#else
			if (!mCached) Cache();
			#endif
			if (mRawDelegate && mCachedCallback != null) return true;
			if (mTarget == null) return false;
			MonoBehaviour mb = (mTarget as MonoBehaviour);
			return (mb == null || mb.enabled);
		}
	}

	//add by shaco 2016/12/18
	public class Entry
	{
		public Component target;
		public string name;
	}
	public List<Entry> ListMethodEntry = new List<Entry>();
	//add end

	public EventDelegateS () { }
	public EventDelegateS (Callback call) { Set(call); }
	public EventDelegateS (MonoBehaviour target, string methodName) { Set(target, methodName); }

	/// <summary>
	/// GetMethodName is not supported on some platforms.
	/// </summary>

	#if REFLECTION_SUPPORT
	#if !UNITY_EDITOR && NETFX_CORE
	static string GetMethodName (Callback callback)
	{
	System.Delegate d = callback as System.Delegate;
	return d.GetMethodInfo().Name;
	}

	static bool IsValid (Callback callback)
	{
	System.Delegate d = callback as System.Delegate;
	return d != null && d.GetMethodInfo() != null;
	}
	#else
	static string GetMethodName (Callback callback) { return callback.Method.Name; }
	static bool IsValid (Callback callback) { return callback != null && callback.Method != null; }
	#endif
	#else
	static bool IsValid (Callback callback) { return callback != null; }
	#endif

	/// <summary>
	/// Equality operator.
	/// </summary>

	public override bool Equals (object obj)
	{
		if (obj == null) return !isValid;

		if (obj is Callback)
		{
			Callback callback = obj as Callback;
			#if REFLECTION_SUPPORT
			if (callback.Equals(mCachedCallback)) return true;
			MonoBehaviour mb = callback.Target as MonoBehaviour;
			return (mTarget == mb && string.Equals(mMethodName, GetMethodName(callback)));
			#elif UNITY_FLASH
			return (callback == mCachedCallback);
			#else
			return callback.Equals(mCachedCallback);
			#endif
			}

			if (obj is EventDelegateS)
			{
			EventDelegateS del = obj as EventDelegateS;
			return (mTarget == del.mTarget && string.Equals(mMethodName, del.mMethodName));
			}
			return false;
			}

			static int s_Hash = "EventDelegateS".GetHashCode();

			/// <summary>
			/// Used in equality operators.
			/// </summary>

			public override int GetHashCode () { return s_Hash; }

			/// <summary>
			/// Set the delegate callback directly.
			/// </summary>

			void Set (Callback call)
			{
			Clear();

			if (call != null && IsValid(call))
			{
			#if REFLECTION_SUPPORT
			mTarget = call.Target as MonoBehaviour;

			if (mTarget == null)
			{
			mRawDelegate = true;
			mCachedCallback = call;
			mMethodName = null;
			}
			else
			{
			mMethodName = GetMethodName(call);
			mRawDelegate = false;
			}
			#else
			mRawDelegate = true;
			mCachedCallback = call;
			#endif
			}
			}

			/// <summary>
			/// Set the delegate callback using the target and method names.
			/// </summary>

			public void Set (MonoBehaviour target, string methodName)
			{
			Clear();
			mTarget = target;
			mMethodName = methodName;
			}

			/// <summary>
			/// Cache the callback and create the list of the necessary parameters.
			/// </summary>

			void Cache ()
			{
			mCached = true;
			if (mRawDelegate) return;

			#if REFLECTION_SUPPORT
			//modify by shaco 2016/12/16
			//todo: fixed callback error when return type not void
			if (mMethod == null || mMethod.Name != mMethodName || mTarget != target)
			//if (mCachedCallback == null || (mCachedCallback.Target as MonoBehaviour) != mTarget || GetMethodName(mCachedCallback) != mMethodName)
			//modify end
			{
			if (mTarget != null && !string.IsNullOrEmpty(mMethodName))
			{
			System.Type type = mTarget.GetType();
			#if NETFX_CORE
			try
			{
			IEnumerable<MethodInfo> methods = type.GetRuntimeMethods();

			foreach (MethodInfo mi in methods)
			{
			if (mi.Name == mMethodName)
			{
			mMethod = mi;
			break;
			}
			}
			}
			catch (System.Exception ex)
			{
			Debug.LogError("Failed to bind " + type + "." + mMethodName + "\n" +  ex.Message);
			return;
			}
			#else // NETFX_CORE
			for (mMethod = null; type != null; )
			{
			try
			{
			mMethod = type.GetMethod(mMethodName, BindingFlags.Instance | BindingFlags.Public);
			if (mMethod != null) break;
			}
			catch (System.Exception ex) 
			{ 
			Debug.LogWarning(ex.Message);
			}
			#if UNITY_WP8 || UNITY_WP_8_1
			// For some odd reason Type.GetMethod(name, bindingFlags) doesn't seem to work on WP8...
			try
			{
			mMethod = type.GetMethod(mMethodName);
			if (mMethod != null) break;
			}
			catch (System.Exception) { }
			#endif
			type = type.BaseType;
			}
			#endif // NETFX_CORE

			if (mMethod == null)
			{
				Debug.LogError("Could not find method '" + mMethodName + "' on " + mTarget.GetType(), mTarget);

				//add by shaco 2016/12/16
				methodName = string.Empty;
				target = null;
				//add end
				return;
			}

			if (getRequstMethodReturnType() != mMethod.ReturnType)
			{
				//modify by shaco 2016/12/16
				Debug.LogError(mTarget.GetType() + "." + mMethodName + " must have a ("+ mRequstMethodReturnType +") return type.", mTarget);
				//modify end
				return;
			}

			// Get the list of expected parameters
			mParameterInfos = mMethod.GetParameters();

			//modify by shaco 2016/12/16
			if (mParameterInfos.Length == 0 && getRequstMethodReturnType() == typeof(void))
			{
				// No parameters means we can create a simple delegate for it, optimizing the call
			#if NETFX_CORE
			mCachedCallback = (Callback)mMethod.CreateDelegate(typeof(Callback), mTarget);
			#else
				mCachedCallback = (Callback)System.Delegate.CreateDelegate(typeof(Callback), mTarget, mMethodName);
			#endif
				mArgs = null;
				mParameters = null;
				return;
			}
			else mCachedCallback = null;
			//modify end

			// Allocate the initial list of parameters
			if (mParameters == null || mParameters.Length != mParameterInfos.Length)
			{
				mParameters = new Parameter[mParameterInfos.Length];
				for (int i = 0, imax = mParameters.Length; i < imax; ++i)
					mParameters[i] = new Parameter();
			}

			// Save the parameter type
			for (int i = 0, imax = mParameters.Length; i < imax; ++i)
				mParameters[i].expectedType = mParameterInfos[i].ParameterType;

			//add by shaco 2016/12/18
			//todo: refresh method list when target changed
			if (mTarget != target)
			{
				ListMethodEntry.Clear();
			}
			//add end
		}
	}
			#endif // REFLECTION_SUPPORT
}

/// <summary>
/// Execute the delegate, if possible.
/// This will only be used when the application is playing in order to prevent unintentional state changes.
/// </summary>

public bool Execute ()
{
	//add by shaco 2016/6/12
	//TODO: if target is null, we wan't callback
	if (mTarget == null)
		return false;
	//add end

	#if !REFLECTION_SUPPORT
	if (isValid)
	{
	if (mRawDelegate) mCachedCallback();
	else mTarget.SendMessage(mMethodName, SendMessageOptions.DontRequireReceiver);
	return true;
	}
	#else
	#if UNITY_EDITOR
	if (!mCached || !Application.isPlaying) Cache();
	#else
	if (!mCached) Cache();
	#endif
	if (mCachedCallback != null)
	{
	#if !UNITY_EDITOR
	mCachedCallback();
	#else
		if (Application.isPlaying)
		{
			mCachedCallback();
		}
		else if (mCachedCallback.Target != null)
		{
			// There must be an [ExecuteInEditMode] flag on the script for us to call the function at edit time
			System.Type type = mCachedCallback.Target.GetType();
			//#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6
			object[] objs = type.GetCustomAttributes(typeof(ExecuteInEditMode), true);
			// #else
			//				object[] objs = type.GetCustomAttributes(typeof(ExecuteInEditModeAttribute), true);
			// #endif
			if (objs != null /*&& objs.Length > 0*/) mCachedCallback();
		}
	#endif
		return true;
	}

	if (mMethod != null)
	{
	#if UNITY_EDITOR
		// There must be an [ExecuteInEditMode] flag on the script for us to call the function at edit time
		if (mTarget != null && !Application.isPlaying)
		{
			System.Type type = mTarget.GetType();
			// #if UNITY_4_3 || UNITY_4_5 || UNITY_4_6
			object[] objs = type.GetCustomAttributes(typeof(ExecuteInEditMode), true);
			// #else
			//				object[] objs = type.GetCustomAttributes(typeof(ExecuteInEditModeAttribute), true);
			// #endif
			if (objs == null /*|| objs.Length == 0*/) return true;
		}
	#endif
		int len = (mParameters != null) ? mParameters.Length : 0;

		if (len == 0)
		{
			mMethod.Invoke(mTarget, null);
		}
		else
		{
			// Allocate the parameter array
			if (mArgs == null || mArgs.Length != mParameters.Length)
				mArgs = new object[mParameters.Length];

			// Set all the parameters
			for (int i = 0, imax = mParameters.Length; i < imax; ++i)
				mArgs[i] = mParameters[i].value;

			// Invoke the callback
			try
			{
				mMethod.Invoke(mTarget, mArgs);
			}
			catch (System.ArgumentException ex)
			{
				string msg = "Error calling ";

				if (mTarget == null) msg += mMethod.Name;
				else msg += mTarget.GetType() + "." + mMethod.Name;

				msg += ": " + ex.Message;
				msg += "\n  Expected: ";

				if (mParameterInfos.Length == 0)
				{
					msg += "no arguments";
				}
				else
				{
					msg += mParameterInfos[0];
					for (int i = 1; i < mParameterInfos.Length; ++i)
						msg += ", " + mParameterInfos[i].ParameterType;
				}

				msg += "\n  Received: ";

				if (mParameters.Length == 0)
				{
					msg += "no arguments";
				}
				else
				{
					msg += mParameters[0].type;
					for (int i = 1; i < mParameters.Length; ++i)
						msg += ", " + mParameters[i].type;
				}
				msg += "\n";
				Debug.LogError(msg);
			}

			// Clear the parameters so that references are not kept
			for (int i = 0, imax = mArgs.Length; i < imax; ++i)
			{
				if (mParameterInfos[i].IsIn || mParameterInfos[i].IsOut)
				{
					mParameters[i].value = mArgs[i];
				}
				mArgs[i] = null;
			}
		}
		return true;
	}
	#endif
	return false;
}

//add by shaco 2016/6/12
public void Execute(params object[] paramaters)
{
	if (target == null)
		return;

	SetParammater(paramaters);

	this.Execute();
}

public void SetParammater(params object[] paramaters)
{
	if (paramaters == null || paramaters.Length == 0)
	{
		Debug.LogError("Faild to call Execute: mParameters is empty !");
		return;
	}

	bool needCreate = true;
	if (mParameters != null && paramaters.Length == mParameters.Length)
	{
		needCreate = false;
	}

	if (needCreate)
	{
		mParameters = new Parameter[paramaters.Length];
		for (int i = 0; i < paramaters.Length; ++i)
		{
			mParameters[i] = new Parameter(paramaters[i]);
		}
	}
	else 
	{
		for (int i = 0; i < paramaters.Length; ++i)
		{
			mParameters[i].value = paramaters[i];
		}
	}
}
//add end

//add by shaco 2016/12/16
public object[] getParameters()
{
	// Allocate the parameter array
	if (mArgs == null || mArgs.Length != mParameters.Length)
		mArgs = new object[mParameters.Length];

	// Set all the parameters
	for (int i = 0, imax = mParameters.Length; i < imax; ++i)
		mArgs[i] = mParameters[i].value;

	return mArgs;
}

public ParameterInfo getParameterInfo(int index)
{
	if (index < 0 || index > mParameterInfos.Length - 1)
	{
		Debug.Log("getParameterInfo error: out of mParameterInfos range index=" + index);
		return null;
	}
	return mParameterInfos[index];
}

public T Execute<T>(params object[] parameters)
{
	T ret = default(T);
	if (target == null)
		return ret;

	if (!mCached) Cache();

	if (parameters.Length > 0)
	{
		SetParammater(parameters);
		ret = (T)mMethod.Invoke(mTarget, getParameters());
	}
	else if (this.mParameters.Length > 0)
	{
		ret = (T)mMethod.Invoke(mTarget, getParameters());
	}
	else
		ret = (T)mMethod.Invoke(mTarget, null);

	return ret;
}

public System.Type getRequstMethodReturnType()
{
	switch (mRequstMethodReturnType)
	{
	case ReturnType.Void: return typeof(void);
	case ReturnType.Bool: return typeof(bool);
	case ReturnType.Int: return typeof(int);
	case ReturnType.Float: return typeof(float);
	case ReturnType.String: return typeof(string);
	default: Debug.LogError("unsupport return type!"); return typeof(void);
	}
}

public void setRequstMethodReturnType(ReturnType type)
{
	mRequstMethodReturnType = type;
}

public void setRequstMethodReturnType(System.Type type)
{
	if (type == typeof(void))
		mRequstMethodReturnType = ReturnType.Void;
	else if (type == typeof(bool))
		mRequstMethodReturnType = ReturnType.Bool;
	else if (type == typeof(int))
		mRequstMethodReturnType = ReturnType.Int;
	else if (type == typeof(float))
		mRequstMethodReturnType = ReturnType.Float;
	else if (type == typeof(string))
		mRequstMethodReturnType = ReturnType.String;
	else
	{
		Debug.LogError("unsupport return type!");
		mRequstMethodReturnType = ReturnType.Void;
	}
}
//add end

/// <summary>
/// Clear the event delegate.
/// </summary>

public void Clear ()
{
	mTarget = null;
	mMethodName = null;
	mRawDelegate = false;
	mCachedCallback = null;
	mParameters = null;
	mCached = false;
	#if REFLECTION_SUPPORT
	mMethod = null;
	mParameterInfos = null;
	mArgs = null;
	#endif
}

/// <summary>
/// Convert the delegate to its string representation.
/// </summary>

public override string ToString ()
{
	if (mTarget != null)
	{
		string typeName = mTarget.GetType().ToString();
		int period = typeName.LastIndexOf('.');
		if (period > 0) typeName = typeName.Substring(period + 1);

		if (!string.IsNullOrEmpty(methodName)) return typeName + shaco.Base.FileDefine.PATH_FLAG_SPLIT + methodName;
		else return typeName + "/[delegate]";
	}
	return mRawDelegate ? "[delegate]" : null;
}

/// <summary>
/// Execute an entire list of delegates.
/// </summary>

static public void Execute (List<EventDelegateS> list)
{
	if (list != null)
	{
		for (int i = 0; i < list.Count; )
		{
			EventDelegateS del = list[i];

			if (del != null)
			{
				#if !UNITY_EDITOR && !UNITY_FLASH
				try
				{
				del.Execute();
				}
				catch (System.Exception ex)
				{
				if (ex.InnerException != null) Debug.LogError(ex.InnerException.Message);
				else Debug.LogError(ex.Message);
				}
				#else
				del.Execute();
				#endif

				if (i >= list.Count) break;
				if (list[i] != del) continue;

				if (del.oneShot)
				{
					list.RemoveAt(i);
					continue;
				}
			}
			++i;
		}
	}
}

/// <summary>
/// Convenience function to check if the specified list of delegates can be executed.
/// </summary>

static public bool IsValid (List<EventDelegateS> list)
{
	if (list != null)
	{
		for (int i = 0, imax = list.Count; i < imax; ++i)
		{
			EventDelegateS del = list[i];
			if (del != null && del.isValid)
				return true;
		}
	}
	return false;
}

/// <summary>
/// Assign a new event delegate.
/// </summary>

static public EventDelegateS Set (List<EventDelegateS> list, Callback callback)
{
	if (list != null)
	{
		EventDelegateS del = new EventDelegateS(callback);
		list.Clear();
		list.Add(del);
		return del;
	}
	return null;
}

/// <summary>
/// Assign a new event delegate.
/// </summary>

static public void Set (List<EventDelegateS> list, EventDelegateS del)
{
	if (list != null)
	{
		list.Clear();
		list.Add(del);
	}
}

/// <summary>
/// Append a new event delegate to the list.
/// </summary>

static public EventDelegateS Add (List<EventDelegateS> list, Callback callback) { return Add(list, callback, false); }

/// <summary>
/// Append a new event delegate to the list.
/// </summary>

static public EventDelegateS Add (List<EventDelegateS> list, Callback callback, bool oneShot)
{
	if (list != null)
	{
		for (int i = 0, imax = list.Count; i < imax; ++i)
		{
			EventDelegateS del = list[i];
			if (del != null && del.Equals(callback))
				return del;
		}

		EventDelegateS ed = new EventDelegateS(callback);
		ed.oneShot = oneShot;
		list.Add(ed);
		return ed;
	}
	Debug.LogWarning("Attempting to add a callback to a list that's null");
	return null;
}

/// <summary>
/// Append a new event delegate to the list.
/// </summary>

static public void Add (List<EventDelegateS> list, EventDelegateS ev) { Add(list, ev, ev.oneShot); }

/// <summary>
/// Append a new event delegate to the list.
/// </summary>

static public void Add (List<EventDelegateS> list, EventDelegateS ev, bool oneShot)
{
	if (ev.mRawDelegate || ev.target == null || string.IsNullOrEmpty(ev.methodName))
	{
		Add(list, ev.mCachedCallback, oneShot);
	}
	else if (list != null)
	{
		for (int i = 0, imax = list.Count; i < imax; ++i)
		{
			EventDelegateS del = list[i];
			if (del != null && del.Equals(ev))
				return;
		}

		EventDelegateS copy = new EventDelegateS(ev.target, ev.methodName);
		copy.oneShot = oneShot;

		if (ev.mParameters != null && ev.mParameters.Length > 0)
		{
			copy.mParameters = new Parameter[ev.mParameters.Length];
			for (int i = 0; i < ev.mParameters.Length; ++i)
				copy.mParameters[i] = ev.mParameters[i];
		}

		list.Add(copy);
	}
	else Debug.LogWarning("Attempting to add a callback to a list that's null");
}

/// <summary>
/// Remove an existing event delegate from the list.
/// </summary>

static public bool Remove (List<EventDelegateS> list, Callback callback)
{
	if (list != null)
	{
		for (int i = 0, imax = list.Count; i < imax; ++i)
		{
			EventDelegateS del = list[i];

			if (del != null && del.Equals(callback))
			{
				list.RemoveAt(i);
				return true;
			}
		}
	}
	return false;
}

/// <summary>
/// Remove an existing event delegate from the list.
/// </summary>

static public bool Remove (List<EventDelegateS> list, EventDelegateS ev)
{
	if (list != null)
	{
		for (int i = 0, imax = list.Count; i < imax; ++i)
		{
			EventDelegateS del = list[i];

			if (del != null && del.Equals(ev))
			{
				list.RemoveAt(i);
				return true;
			}
		}
	}
	return false;
}
}

