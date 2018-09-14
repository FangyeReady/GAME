using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupGachaResult : PopupBase {

    public GameObject itemPrefab;
    public Transform parent;



    protected override void Init()
    {
        base.Init();
    }



    public void CreateItems(List<ServentInfo> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            GachaItem item = Instantiate(itemPrefab, parent).GetComponent<GachaItem>();
            item.Init(list[i]);
            item.gameObject.SetActive(true);
            Player.Instance.PlayerInfos.Servent.Add(list[i]);//暂时这样做，后续改成只能选择一个加入队伍？
        }  
    }



    private void OnConfirmClick()
    {
        int childCount = parent.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
        base.Close();
    }
}
