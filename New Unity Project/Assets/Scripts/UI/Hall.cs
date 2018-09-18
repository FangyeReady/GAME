using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Hall : PopupBase {

    public GameObject SettingGrid;
    public Toggle muteMusic;
    public Toggle playMusic;
    public Image muteImg;
    public Image playImg;

    private SoundType m_soundType = SoundType.Song;

    private int CurrenSongIndex = 0;

    protected override void OnEnabled()
    {
        base.OnEnabled();
        //DontDestroyOnLoad(this);
    }

    protected override void Init()
    {
        base.Init();

        muteMusic.onValueChanged.AddListener(MuteMusic);
        playMusic.onValueChanged.AddListener(PlayOrPauseMusic);       
    }


    private void OnSettingGridClicked(GameObject obj = null)
    {
        SettingGrid.SetActive(!SettingGrid.activeSelf);
    }

    #region PlayMusic
    private void PreMusic()
    {
        if (CurrenSongIndex - 1 >= 0)
        {
            SoundManager.Instance.PauseMusic(m_soundType);
            SoundManager.Instance.PlaySound(m_soundType, StaticData.Song[--CurrenSongIndex]);
        }
    }

    private void NextMusic()
    {
        if (CurrenSongIndex + 1 <= StaticData.Song.Length - 1)
        {
            SoundManager.Instance.PauseMusic(m_soundType);
            SoundManager.Instance.PlaySound(m_soundType, StaticData.Song[++CurrenSongIndex]);
        }
    }

    private void PlayOrPauseMusic(bool isActive)
    {
        if (isActive)
        {
            SoundManager.Instance.PlaySound(m_soundType, StaticData.Song[CurrenSongIndex]);
            ResourcesLoader.Instance.SetSprite(StaticData.UI_PIC, "Pause", (sp) =>
            {   
                playImg.sprite = sp;
            });
        }
        else
        {
            SoundManager.Instance.PauseMusic(m_soundType);
            ResourcesLoader.Instance.SetSprite(StaticData.UI_PIC, "Play", (sp) =>
            {
                playImg.sprite = sp;
            });
        }
    }

    private void MuteMusic(bool isActive)
    {
        SoundManager.Instance.MuteMusic(m_soundType);
        if (isActive)
        {
            ResourcesLoader.Instance.SetSprite(StaticData.UI_PIC, "SoundOn", (sp) =>
            {
                muteImg.sprite = sp;
            }); 
        }
        else
        {
            ResourcesLoader.Instance.SetSprite(StaticData.UI_PIC, "SoundOff", (sp) =>
            {
                muteImg.sprite = sp;
            }); 
        }
    }
    #endregion

    #region UI
    private void OnInfoClick()
    {
        UIManager.Instance.OpenWindow<PopupPlayerInfo>();

    }

    private void OnGachaClick()
    {
        UIManager.Instance.OpenWindow<PopupGacha>();

    }

    private void OnManageClick()
    {
        UIManager.Instance.OpenWindow<PopupServentHome>();
    }

    private void OnPlayClick()
    {
        LoggerM.Log("play~!");
    }

    #endregion

}
