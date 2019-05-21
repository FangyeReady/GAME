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

    private ServentInfo _info;
    public ServentInfo info {

        get { return _info; }
    }

    public void Init(ServentInfo info)
    {
        ResourcesLoader.Instance.SetSprite(StaticData.HEAD_ICON_PATH, "list" + info.ID.ToString(), sp => headPic.sprite = sp);
        this._info = info;
        level.text = info.Level.ToString();
        mask.SetActive(false);
        serID = info.ID;

        for (int i = 0; i < starContainer.childCount; i++)
        {
            Destroy(starContainer.GetChild(i).gameObject);
        }

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
