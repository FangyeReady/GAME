using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PopupHall : PopupBase {

    public GameObject SettingGrid;
    public Toggle muteMusic;
    public Toggle playMusic;
    public Image muteImg;
    public Image playImg;

    private SoundType m_soundType = SoundType.Song;

    private int CurrenSongIndex = 0;

    protected override void Init()
    {
        base.Init();

        muteMusic.onValueChanged.AddListener(MuteMusic);
        playMusic.onValueChanged.AddListener(PlayOrPauseMusic);       
        //SoundManager.Instance.PlaySound(m_soundType, CurrenSongIndex);
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
            ResourcesLoader.Instance.SetSprite("Pause", (sp) =>
            {   
                playImg.overrideSprite = sp;
            });
        }
        else
        {
            SoundManager.Instance.PauseMusic(m_soundType);
            ResourcesLoader.Instance.SetSprite("Play", (sp) =>
            {
                playImg.overrideSprite = sp;
            });
        }
    }

    private void MuteMusic(bool isActive)
    {
        SoundManager.Instance.MuteMusic(m_soundType);
        if (isActive)
        {
            ResourcesLoader.Instance.SetSprite("SoundOn", (sp) =>
            {
                muteImg.overrideSprite = sp;
            }); 
        }
        else
        {
            ResourcesLoader.Instance.SetSprite("SoundOff", (sp) =>
            {
                muteImg.overrideSprite = sp;
            }); 
        }
    }
    #endregion

    #region UI

    #endregion
}
