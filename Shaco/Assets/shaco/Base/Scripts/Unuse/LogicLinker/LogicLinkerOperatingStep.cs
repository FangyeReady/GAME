using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
	public class LogicLinkerOperatingStep 
	{
		public int MaxOperationStep = 100;

		private LogicLinker _target = null;
		private List<Dictionary<LogicLinker.UUID, LogicLinker.LinkItem>> _listOperations = new List<Dictionary<LogicLinker.UUID, LogicLinker.LinkItem>>();
		private int _currentOperatingIndex = 0;
		private bool _isRecording = false;
		private int _autoOffsetCount = -1;

		public void setTargetLogicLinker(LogicLinker target)
		{
			_target = target;
			_currentOperatingIndex = 0;
			_listOperations.Clear();
			_isRecording = false;
		}

		public void recordOperation()
		{
			if (_target == null)
			{
				return;
			}
			if (_isRecording)
				return;

			_isRecording = true;

			_recordOperation();

			int removeIndex = _currentOperatingIndex + 1;
			if (removeIndex < _listOperations.Count - 1)
			{
				_listOperations.RemoveRange(removeIndex, _listOperations.Count - 1 - removeIndex);
			}	

			_currentOperatingIndex = _listOperations.Count;

			_isRecording = false;
		}

		public void nextStep()
		{
			if (_currentOperatingIndex < _listOperations.Count - 1)
			{
				++_currentOperatingIndex;
				replaceAllLinkItems();
//				Log.Info("nextStep index=" + _currentOperatingIndex + " count=" + _listOperations.Count);
			}
		}

		public void prevStep()
		{
			if (_currentOperatingIndex > 0)
			{
				--_currentOperatingIndex;
				replaceAllLinkItems();
//				Log.Info("prevStep index=" + _currentOperatingIndex + " count=" + _listOperations.Count);
			}
		}

		private void replaceAllLinkItems()
		{
			if (_currentOperatingIndex < 0 || _currentOperatingIndex > _listOperations.Count - 1)
			{
				Log.Error("_currentOperatingIndex out of range _currentOperatingIndex=" + _currentOperatingIndex + " count=" + _listOperations.Count);
				return;
			}

			if (_target == null)
			{
				return;
			}
			_isRecording = true;

			var mapTmp = _listOperations[_currentOperatingIndex];

			if (_currentOperatingIndex == _listOperations.Count - 1 && _autoOffsetCount != _listOperations.Count)
			{
				_recordOperation();
				_autoOffsetCount = _listOperations.Count;
			}

			_target.clear();

			foreach (var key in mapTmp.Keys)
			{
				var itemTmp = mapTmp[key];
				_target.addLinkItem(itemTmp.Clone());
			}

			_isRecording = false;
		}

		private void _recordOperation()
		{
			if (_target == null)
			{
				return;
			}

			if (_currentOperatingIndex > MaxOperationStep && _listOperations.Count > 0)
			{
				_listOperations.RemoveAt(0);
			}

			_listOperations.Add(new Dictionary<LogicLinker.UUID, LogicLinker.LinkItem>(new LogicLinker.UUIDCompare()));

			_target.foreachLinkItems((LogicLinker.LinkItem item) => {

				var newItem = item.Clone();
				_listOperations[_listOperations.Count - 1].Add(newItem.UUIDItem, newItem);
			});
		}
	}
}
