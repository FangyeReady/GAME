using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServentHeadItem : MonoBehaviour {

    public Image headPic;
    public GameObject mask;

    public Transform starContainer;
    public GameObject starPre;

    public Text level;

    private int serID = 0;

    public void Init(ServentInfo info)
    {
        ResourcesLoader.Instance.SetSprite(StaticData.HEAD_ICON_PATH, "list" + info.ID.ToString(), sp => headPic.sprite = sp);
        level.text = info.Level.ToString();
        mask.SetActive(false);
        serID = info.ID;
        for (int i = 0; i < info.star; i++)
        {
            GameObject obj = Instantiate(starPre, starContainer);
            obj.SetActive(true);
        }
    }

    void OnChoosed()
    {
        PopupServentHome home = UIManager.Instance.GetWindow<PopupServentHome>();
        home.RefreshChooseState(int.Parse(this.gameObject.name), serID);
    }
}
