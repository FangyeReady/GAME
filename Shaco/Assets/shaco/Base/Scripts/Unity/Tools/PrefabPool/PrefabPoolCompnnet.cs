using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabPoolCompnnet : MonoBehaviour
{
	public void ChangeParentToPrefabPoolComponent(GameObject prefab)
	{
		prefab.transform.SetParent(this.transform);
	}
}
