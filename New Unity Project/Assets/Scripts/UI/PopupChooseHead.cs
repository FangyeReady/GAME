using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupChooseHead : PopupBase {

    public GameObject prefab;
    public Transform parent;

    protected override void Init()
    {
        base.Init();
        CreateItems(Player.Instance.PlayerInfos.Servent);
    }


    private void CreateItems(List<ServentInfo> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Image img = Instantiate(prefab, parent).GetComponent<Image>();
            if (img != null)
            {
                img.gameObject.SetActive(true);
                img.gameObject.name = list[i].ID.ToString();
                ResourcesLoader.Instance.SetSprite(StaticData.HEAD_ICON_PATH, "list" + img.gameObject.name, (sp) => img.sprite = sp);
            }
        }
    }

    private void OnCloseClick()
    {
        base.Close();
    }
}
