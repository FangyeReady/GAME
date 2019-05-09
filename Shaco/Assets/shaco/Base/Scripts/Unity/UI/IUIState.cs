using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
	public interface IUIState
    {
        string key { get; }
        GameObject parent { get; }
        UIRootComponent uiRoot { get; }
        System.Collections.ObjectModel.ReadOnlyCollection<UIPrefab> prefabs { get; }
        UIEvent uiEvent { get; }
    }
}
