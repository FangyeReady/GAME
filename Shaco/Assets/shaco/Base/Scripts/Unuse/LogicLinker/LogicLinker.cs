using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
	public class LogicLinker : MonoBehaviour
	{
		public delegate void CALL_FUNC_FOREACH_ITEM(LinkItem item);

		[System.Serializable]
		public class UUID
		{
			public string id = string.Empty; //作为字典查找的key值

			public override bool Equals (object obj)
			{
				UUID other = obj as UUID;
				return this.id == other.id;
			}

            public static bool operator == (UUID v1, UUID v2)
            {
                return v1.Equals(v2);
            }

            public static bool operator !=(UUID v1, UUID v2)
            {
                return !v1.Equals(v2);
            }

			public override int GetHashCode ()
			{
				return this.id.GetHashCode();
			}

            public override string ToString()
            {
                return "id=" + id;
            }

			public UUID Clone()
			{
				UUID ret = new UUID();
				ret.id = id;
				return ret;
			}
		}

		public class UUIDCompare : IEqualityComparer<shaco.LogicLinker.UUID>
		{
			public bool Equals (UUID v1, UUID v2)
			{
				return v1.id == v2.id;
			}

			public int GetHashCode (UUID v1)
			{
				return v1.id.GetHashCode();
			}
		}

		[System.Serializable]
		public class LinkID
		{
			public bool IsSelect = false;          	//当前是否有被选中
			public UUID UUIDFrom = new UUID(); 		//连接来源
			public UUID UUIDTo = new UUID();		//连接对象
			public Vector3 startPos = Vector3.zero; //2个组件矩形范围的交叉起始点
			public Vector3 endPos = Vector3.zero;   //2个组件矩形范围的交叉终止点

			public LinkID(UUID from, UUID to, bool isSelect)
			{
				this.UUIDFrom = from;
				this.UUIDTo = to;
				this.IsSelect = isSelect;
			}
			public LinkID(UUID from, UUID to)
			{
				this.UUIDFrom = from;
				this.UUIDTo = to;
			}
			public LinkID(){}

			public LinkID Clone()
			{
				LinkID ret = new LinkID();
				ret.IsSelect = IsSelect;
				ret.UUIDFrom = UUIDFrom.Clone();
				ret.UUIDTo = UUIDTo.Clone();
				ret.startPos = new Vector3(startPos.x, startPos.y, startPos.z);
				ret.endPos = new Vector3(endPos.x, endPos.y, endPos.z);
				return ret;
			}
		}

		[System.Serializable]
		public class LinkItem
		{
			public UUID UUIDItem = new UUID();                  //识别用唯一id
			public Rect RectSize = new Rect();                  //绘制矩形位置
			public Vector2 ScrollPosition = new Vector2();      //滚动视口位置
			public string Description = string.Empty;                     //组件名字
			public List<LinkID> LinkTo = new List<LinkID>();    //因为c#无法对类中List<类>的类型进行序列化
			public List<LinkID> LinkFrom = new List<LinkID>();  //所以这里的UUID类型表示LinkItem的UUIDItem
			public List<EventDelegateS> funcCondition = new List<EventDelegateS>();   //触发LinkTo对象条件
			public List<EventDelegateS> funcExcute = new List<EventDelegateS>();      //被触发时，执行事件回调

			public Rect DefaultRectSize = new Rect();           //默认初始化矩形位置
			public Vector2 DragOffset = Vector2.zero;           //拖拽偏移量
			public bool IsShowCondition = true;                 //是否显示触发条件
			public bool IsShowExcute = true;                    //是否显示执行事件

			public override string ToString()
			{
				return this.Description;
			}

			public void Reset()
			{
				RectSize = DefaultRectSize;
				Description = string.Empty;
				LinkTo.Clear();
				LinkFrom.Clear();
				funcCondition = null;
				funcExcute = null;
			}

			public LinkItem Clone()
			{
				LinkItem ret = new LinkItem();
				ret.UUIDItem = UUIDItem.Clone();
				ret.RectSize = new Rect(RectSize.x, RectSize.y, RectSize.width, RectSize.height);
				ret.ScrollPosition = new Vector2(ScrollPosition.x, ScrollPosition.y);
				ret.Description = Description;
				foreach (var value in LinkTo)
					ret.LinkTo.Add(value);
				foreach (var value in LinkFrom)
					ret.LinkFrom.Add(value);
				foreach (var value in funcCondition)
					ret.funcCondition.Add(new EventDelegateS(value.target, value.methodName));
				foreach (var value in funcExcute)
					ret.funcExcute.Add(new EventDelegateS(value.target, value.methodName));
				ret.DefaultRectSize = new Rect(DefaultRectSize.x, DefaultRectSize.y, DefaultRectSize.width, DefaultRectSize.height);
				ret.DragOffset = new Vector2(DragOffset.x, DragOffset.y);
				ret.IsShowCondition = IsShowCondition;
				ret.IsShowExcute = IsShowExcute;
				return ret;
			}
		}

		public class WaitTryExcute
		{
			public LinkItem current = null;
			public LinkItem from = null;
		}

		public class ExcuteInfo
		{
			public Dictionary<UUID, WaitTryExcute> _mapExcute = new Dictionary<UUID, WaitTryExcute>(new UUIDCompare());  //执行的组件字典
			public List<UUID> _listAddExcuteKey = new List<UUID>();												         //下一帧需要添加的执行的组件键值
			public List<WaitTryExcute> _listAddExcuteValue = new List<WaitTryExcute>();								     //下一帧需要添加的执行的组件值
			public List<UUID> _listRemoveExcute = new List<UUID>();												         //下一帧需要移除的执行的组件键值
		}

		public EventDelegateS ButtonClickLinkScript;                                        //用于在编辑器中的按钮事件
		public List<UUID> ListLinkItemKey = new List<UUID>(); 								//组件键值，序列化用
		public List<LinkItem> ListLinkItemValue = new List<LinkItem>();						//组件值，序列化用
		public Color ExcutedItemColor = new Color(111.0f / 255, 1, 1, 1);                   //上一次执行的组件颜色  
		public Color WaitExcuteItemColor = Color.red;   			                    	//等待执行的组件在编辑窗口的绘制颜色
		[HideInInspector] [System.NonSerialized]
		public LogicLinkerOperatingStep ItemOperatingStep = new LogicLinkerOperatingStep();//操作步骤的恢复

		private Dictionary<UUID, LinkItem> MapLinkItems = new Dictionary<UUID, LinkItem>(new UUIDCompare()); //逻辑连接组件字典
		private ExcuteInfo _waitExcuteInfo = new ExcuteInfo();                                               //等待执行的组件信息
		private List<UUID> _prevExcuteInfo = new List<UUID>();                                               //上次执行的组件信息
		private long _uuidMake = 0;

		void OnValidate()
		{
			UpdateMapDatas();
		}

		void Start()
		{
			init();

			foreach (var key in MapLinkItems.Keys)
			{
				var value = MapLinkItems[key];
				if (value.LinkFrom.Count == 0)
					this.tryExcute(value, null);
			}
		}

		void Update()
		{
			baseUpdate();
		}

		//基础逻辑刷新
		void baseUpdate()
		{
			foreach (var key in _waitExcuteInfo._mapExcute.Keys)
			{
				var value = _waitExcuteInfo._mapExcute[key];
				tryExcute(value.current, value.from);
			}

			updateWaitExcuteMap();
		}

		public bool init()
		{
			ItemOperatingStep.setTargetLogicLinker(this);
			UpdateMapDatas();
			return true;
		}

		//遍历所有组件
		public void foreachLinkItems(CALL_FUNC_FOREACH_ITEM callfunc)
		{
			if (callfunc == null)
			{
				Log.Error("foreachLinkItems erorr: callfunc is null");
				return;
			}
			foreach (var key in MapLinkItems.Keys)
			{
				callfunc(MapLinkItems[key]);
			}
		}

		//获取组件数量
		public int getLinkItemSize()
		{
			return MapLinkItems.Count;
		}

		//获取该键值是否存在与字典中
		public bool containKey(UUID key)
		{
			return MapLinkItems.ContainsKey(key);
		}

		//创建组件
		public LinkItem createLinkItem(float posX = 0, float posY = 0, float width = 260, float height = 130)
		{
			LinkItem ret = new LinkItem();
			ret.UUIDItem = getUUID();
			ret.DefaultRectSize = new Rect(posX, posY, width, height);
			ret.RectSize = new Rect(posX, posY, width, height);

			addLinkItem(ret);

			return ret;
		}

		//添加组件到字典中
		public void addLinkItem(LinkItem item)
		{
			ItemOperatingStep.recordOperation();
			MapLinkItems.Add(item.UUIDItem, item);
			ListLinkItemKey.Add(item.UUIDItem);
			ListLinkItemValue.Add(item);

			for (int i = 0; i < item.funcCondition.Count; ++i)
			{
				item.funcCondition[i].setRequstMethodReturnType(typeof(bool));
			}
		}

		//移除组件，并断开连线
		public bool removeLinkItem(LinkItem item)
		{
			if (!MapLinkItems.ContainsKey(item.UUIDItem))
			{
				Log.Error("removeLinkItem error: not find item=" + item);
				return false;
			}
			else
			{
				ItemOperatingStep.recordOperation();

				//check remove
				cutLines(item);
				MapLinkItems.Remove(item.UUIDItem);
				ListLinkItemKey.Remove(item.UUIDItem);
				for (int j = ListLinkItemValue.Count - 1; j >= 0; --j)
				{
					if (ListLinkItemValue[j].UUIDItem == item.UUIDItem)
					{
						ListLinkItemValue.RemoveAt(j);
						break;
					}
				}

				return true;
			}
		}

		//连线2个组件 src -> des
		public void link(LinkItem src, LinkItem des)
		{
			ItemOperatingStep.recordOperation();
			
			//link src next
			addSafe(src.LinkTo, new LinkID(src.UUIDItem, des.UUIDItem));

			//link des prev
			addSafe(des.LinkFrom, new LinkID(src.UUIDItem, des.UUIDItem));
		}

		//切断与该组件相关的所有连线
		public void cutLines(LinkItem itemTarget)
		{
			ItemOperatingStep.recordOperation();

			//cut prev
			for (int i = 0; i < itemTarget.LinkFrom.Count; ++i)
			{
				removeSafe(getLinkItem(itemTarget.LinkFrom[i].UUIDFrom).LinkTo, itemTarget.UUIDItem);
			}

			//cut next
			for (int i = 0; i < itemTarget.LinkTo.Count; ++i)
			{
				removeSafe(getLinkItem(itemTarget.LinkTo[i].UUIDTo).LinkFrom, itemTarget.UUIDItem);
			}

			//cut current
			itemTarget.LinkFrom.Clear();
			itemTarget.LinkTo.Clear();
		}

		//切换一条连线
		public void cutLine(LinkID lineTarget)
		{
			var itemFrom = getLinkItem(lineTarget.UUIDFrom);
			var itemTo = getLinkItem(lineTarget.UUIDTo);

			ItemOperatingStep.recordOperation();

			//cut prev -> next
			removeSafe(itemFrom.LinkTo, lineTarget.UUIDTo);

			//cut next -> prev
			removeSafe(itemTo.LinkFrom, lineTarget.UUIDFrom);
		}

		//清空字典
		public void clear()
		{
			ItemOperatingStep.recordOperation();

			ListLinkItemKey.Clear();
			ListLinkItemValue.Clear();
			MapLinkItems.Clear();
		}

		//切换所有组件连线
		public void cutAllLinkLine()
		{
			ItemOperatingStep.recordOperation();

			foreach (var key in MapLinkItems.Keys)
			{
				var value = MapLinkItems[key];
				value.LinkFrom.Clear();
				value.LinkTo.Clear();
			}
		}

		//筛选出touchPos点上的组件
		public LinkItem selectItem(Vector2 touchPos)
		{
			LinkItem ret = null;

			List<UUID> listKeysTmp = new List<UUID>();
			foreach (var key in MapLinkItems.Keys)
			{
				listKeysTmp.Add(key);
			}

			for (int i = listKeysTmp.Count - 1; i >= 0; --i)
			{
				var value = MapLinkItems[listKeysTmp[i]];
				LinkItem itemTmp = value;
				if (itemTmp.RectSize.Contains(touchPos))
				{
					ret = itemTmp;
					break;
				}
			}

			if (ret != null)
			{
				ret.DragOffset = touchPos - ret.RectSize.position;
			}
			return ret;
		}

		//筛选出touchPos点上的连线
		public List<LinkID> selectLinkID(Vector3 touchPos, float lineWidth)
		{
			List<LinkID> ret = new List<LinkID>();

			foreach (var key in MapLinkItems.Keys)
			{
				var value = MapLinkItems[key];

				for (int i = 0; i < value.LinkTo.Count; ++i)
				{
					var linkID = value.LinkTo[i];
					float checkFlag = MathS.PointToSideOfLine(
						linkID.startPos.x, linkID.startPos.y, 
						linkID.endPos.x, linkID.endPos.y, 
						touchPos.x, touchPos.y);

					float startToEndMagnitude = (linkID.endPos - linkID.startPos).magnitude;
					if (Mathf.Abs(checkFlag) < 500 * lineWidth
						&& (touchPos - linkID.startPos).magnitude <= startToEndMagnitude
						&& (touchPos - linkID.endPos).magnitude <= startToEndMagnitude)
					{
						ret.Add(linkID);
					}
				}
			}

			return ret;
		}

		//重置字典和当前选中的连线
		public void Reset()
		{
			unSelectAllLinkID();

			if (!Application.isPlaying)
				_waitExcuteInfo._mapExcute.Clear();
			_prevExcuteInfo.Clear();
			ItemOperatingStep.setTargetLogicLinker(this);
		}

		//重置当前选中的连线
		private void unSelectAllLinkID()
		{
			foreach (var key in MapLinkItems.Keys)
			{
				var value = MapLinkItems[key];
				for (int i = 0; i < value.LinkTo.Count; ++i)
				{
					value.LinkTo[i].IsSelect = false;
				}
				for (int i = 0; i < value.LinkFrom.Count; ++i)
				{
					value.LinkFrom[i].IsSelect = false;
				}
			}
		}

		//拖拽组件
		public void dragItem(LinkItem item, Vector2 touchPos)
		{
			if (item == null)
				return;
			item.RectSize.position = touchPos - item.DragOffset;
		}

		//判断组件是否有线相连
		public bool isLink(LinkItem src, LinkItem des)
		{
			bool isLinkTo = false;
			bool isLinkFrom = false;
			for (int i = 0; i < src.LinkTo.Count; ++i)
			{
				if (src.LinkTo[i].UUIDTo == des.UUIDItem)
				{
					isLinkTo = true;
					break;
				}
			}
			for (int i = 0; i < des.LinkFrom.Count; ++i)
			{
				if (des.LinkFrom[i].UUIDFrom == src.UUIDItem)
				{
					isLinkFrom = true;
					break;
				}
			}
			if (isLinkTo != isLinkFrom)
			{
				Log.Error(string.Format("isLink erorrL src={0} des={1} did not match the link", src, des));
			}

			return isLinkTo && isLinkFrom;
		}

		//尝试执行组件方法，并自动刷新逻辑
        public bool tryExcuteWithUpdate(LinkItem current, LinkItem from, object[] ConditionParam = null, object[] ExcuteParam = null)
        {
			bool ret = tryExcute(current, from, ConditionParam, ExcuteParam);
            baseUpdate();

			if (!Application.isPlaying)
				unSelectAllLinkID();

            return ret;
        }

		//根据当前组件和组件连线，获取组件来源方，如果连线正确，返回来源方组件，反之返回null
		public LinkItem getLinkItemFrom(LinkItem current, LinkID linkID)
		{
			return current.UUIDItem == linkID.UUIDTo ? getLinkItem(linkID.UUIDFrom) : null;
		}
		public LinkItem getLinkItemFrom(LinkItem current, List<LinkID> listLinkID)
		{
			LinkItem ret = null;
			for (int i = 0; i < listLinkID.Count; ++i)
			{
				ret = getLinkItemFrom(current, listLinkID[i]);
				if (ret != null)
					break;
			}
			return ret;
		}

		/// <summary>
		/// 根据条件执行方法
		/// </summary>
		/// <param name="target"></param> 组件
		/// <param name="ConditionParam"></param> 判断条件参数，如果都返回true，则执行方法
		/// <param name="ExcuteParam"></param> 执行方法参数
		public bool tryExcute(LinkItem current, LinkItem from, object[] ConditionParam = null, object[] ExcuteParam = null)
		{
			if (current == null)
			{
				Log.Error("Excute erorr: target is null");
				return false;
			}
			bool isCheckOK = true;

			if (from == null)
				isCheckOK = true;
			else 
			{
				for (int i = 0; i < from.funcCondition.Count; ++i)
				{
					var eventConition = from.funcCondition[i];

					if (eventConition != null && eventConition.target != null)
					{
						bool check = true;
						if (ConditionParam != null)
							check = eventConition.Execute<bool>(ConditionParam);
						else
							check = eventConition.Execute<bool>();
						if (!check)
						{
							isCheckOK = false;
							break;
						}
					}
				}
			}

			if (isCheckOK)
			{
				//excute current function
				for (int i = 0; i < current.funcExcute.Count; ++i)
				{
					var eventExcute = current.funcExcute[i];
					if (eventExcute != null)
					{
						if (ExcuteParam != null)
							eventExcute.Execute(ExcuteParam);
						else
							eventExcute.Execute();
					}
				}

				//add wait excute item
				for (int i = 0; i < current.LinkTo.Count; ++i)
				{
					var itemTmp = getLinkItem(current.LinkTo[i].UUIDTo);

					//add wait excute items
					if (_waitExcuteInfo._mapExcute.ContainsKey(itemTmp.UUIDItem))
						Log.Warning("the same excute item has added description=" + itemTmp.Description);
					else
					{
						addExcuteItem(itemTmp, current, _waitExcuteInfo);
					}
				}

				//remove has excuted item
				_waitExcuteInfo._listRemoveExcute.Add(current.UUIDItem);
				if (from != null)
					_prevExcuteInfo.Remove(from.UUIDItem);

				//add prev excuted item
				_prevExcuteInfo.Add(current.UUIDItem);
			}

			return isCheckOK;
		}

		/// <summary>
		/// 判断touchPos点击位置是否在组件的矩形范围内
		/// </summary>
		/// <returns>在组件内返回true，反之false
		/// <param name="target">Target.</param> 组件对象
		/// <param name="touchPos">Touch position.</param> 触摸点击位置
		/// <param name="rectInner">Rect inner.</param> 内圈矩形大小
		public bool isInWindowFrame(LinkItem target, Vector2 touchPos, Rect rectInner)
		{
			bool ret = false;

			Rect rectOut = new Rect(target.RectSize);

			if (!rectInner.Contains(touchPos) && rectOut.Contains(touchPos))
			{
				ret = true;
			}
			else
				ret = false;

			return ret;
		}

		//获取组件对象
		public LinkItem getLinkItem(UUID key)
		{
			LinkItem ret = null;
			if (!MapLinkItems.ContainsKey(key))
			{
				Log.Error("getLinkItem error: not find value by key=" + key);
				return ret;
			}
			else
			{
				ret = MapLinkItems[key];
				return ret;
			}
		}

		//获取组件对象
		public LinkItem getLinkItem(string description)
		{
			LinkItem ret = null;
			for (int i = 0; i < ListLinkItemValue.Count; ++i)
			{
				if (ListLinkItemValue[i].Description == description)
				{
					ret = ListLinkItemValue[i];
					break;
				}
			}
			return ret;
		}

		//获取组件连线对象
		public LinkID getLinkID(LinkItem from, LinkItem to)
		{
			LinkID ret = null;
			for (int i = 0; i < from.LinkTo.Count; ++i)
			{
				if (from.LinkTo[i].UUIDTo == to.UUIDItem)
				{
					ret = from.LinkTo[i];
					break;
				}
			}
			return ret;
		}
			
		public bool isWaitExcute(UUID key)
		{
			return _waitExcuteInfo._mapExcute.ContainsKey(key);
		}

		public bool isPrevExcute(UUID key)
		{
			return _prevExcuteInfo.Contains(key);
		}

		//设置条件函数参数
		public bool setConditionParameters(LinkItem target, MonoBehaviour funcTarget, string methodName, params object[] parameters)
		{
			bool ret = setFuncParameters(target.funcCondition, funcTarget, methodName, parameters);
			for (int i = 0; i < target.funcCondition.Count; ++i)
			{
				target.funcCondition[i].setRequstMethodReturnType(typeof(bool));
			}
			return ret;
		}

		//设置执行函数参数
		public bool setExcuteParameters(LinkItem target, MonoBehaviour funcTarget, string methodName, params object[] parameters)
		{
			return setFuncParameters(target.funcExcute, funcTarget, methodName, parameters);
		}

		private void addExcuteItem(LinkItem current, LinkItem from, ExcuteInfo info)
        {
			if (!info._listAddExcuteKey.Contains(current.UUIDItem))
            {
				info._listAddExcuteKey.Add(current.UUIDItem);
				WaitTryExcute valueTmp = new WaitTryExcute();
				valueTmp.current = current;
				valueTmp.from = from;
				info._listAddExcuteValue.Add(valueTmp);
            }
        }

		private bool setFuncParameters(List<EventDelegateS> listFunc, MonoBehaviour funcTarget, string methodName, params object[] parameters)
		{
			bool isHave = false;
			for (int i = 0; i < listFunc.Count; ++i)
			{
				var funcTmp = listFunc[i];
				if (funcTmp.target == funcTarget && funcTmp.methodName == methodName)
				{
					funcTmp.SetParammater(parameters);
					isHave = true;
					break;
				}
			}

			if (!isHave)
			{
				var newEvent = new EventDelegateS(funcTarget, methodName);
				if (parameters.Length > 0)
					newEvent.SetParammater(parameters);
				listFunc.Add(newEvent);
			}
			return true;
		}

		private void updateWaitExcuteMap()
		{
			//check item remove 
			for (int i = 0; i < _waitExcuteInfo._listRemoveExcute.Count; ++i)
			{
				_waitExcuteInfo._mapExcute.Remove(_waitExcuteInfo._listRemoveExcute[i]);
			}
			if (_waitExcuteInfo._listRemoveExcute.Count > 0)
				_waitExcuteInfo._listRemoveExcute.Clear();

			//check item add
			if (_waitExcuteInfo._listAddExcuteKey.Count != _waitExcuteInfo._listAddExcuteValue.Count)
			{
				Log.Error("key's size is difference of value size !");
			}
			for (int i = 0; i < _waitExcuteInfo._listAddExcuteKey.Count; ++i)
			{
				_waitExcuteInfo._mapExcute.Add(_waitExcuteInfo._listAddExcuteKey[i], _waitExcuteInfo._listAddExcuteValue[i]);
			}

			if (_waitExcuteInfo._listAddExcuteKey.Count > 0)
			{
				_waitExcuteInfo._listAddExcuteKey.Clear();
				_waitExcuteInfo._listAddExcuteValue.Clear();
			}
		}

		//刷新字典信息，因为字典本身不支持序列化，暂时以该方法处理吧
		private void UpdateMapDatas()
		{
			if (MapLinkItems.Count != 0 || ListLinkItemKey.Count == 0)
				return;

			if (ListLinkItemKey.Count != ListLinkItemValue.Count)
			{
				Log.Error("ListLinkItemKey.Count != ListLinkItemValue.Count");
				return;
			}

			for (int i = 0; i < ListLinkItemKey.Count; ++i)
			{
				MapLinkItems.Add(ListLinkItemKey[i], ListLinkItemValue[i]);
			}
		}

		private void addSafe(List<LinkID> listTarget, LinkID item)
		{
			if (!listTarget.Contains(item))
			{
				listTarget.Add(item);
			}
			else
				Log.Warning("addSafe warning: you have link it before newItem=" + item.ToString());
		}

		private void removeSafe(List<LinkID> listTarget, UUID uuidItem)
		{
			bool hasValue = false;
			for (int i = listTarget.Count - 1; i >= 0; --i)
			{
				if (listTarget[i].UUIDFrom == uuidItem || listTarget[i].UUIDTo == uuidItem)
				{
					listTarget.RemoveAt(i);
					hasValue = true;
					break;
				}
			}
			if (!hasValue)
				Log.Error("removeSafe error: dont have uuid=" + uuidItem.ToString());
		}

	    public UUID getUUID()
		{
			UUID ret = new UUID();
			ret.id = (_uuidMake++).ToString();
			return ret;
		}
	}
}
