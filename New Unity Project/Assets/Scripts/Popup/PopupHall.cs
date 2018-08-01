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

    private int CurrenSongIndex = StaticData.Song[0];

    protected override void Init()
    {
        base.Init();

        muteMusic.onValueChanged.AddListener(MuteMusic);
        playMusic.onValueChanged.AddListener(PlayOrPauseMusic);       
        SoundManager.Instance.PlaySound(m_soundType, CurrenSongIndex);
    }


    private void OnSettingGridClicked(GameObject obj = null)
    {
        SettingGrid.SetActive(!SettingGrid.activeSelf);
    }


    private void PreMusic()
    {
        if (CurrenSongIndex - 1 >= StaticData.Song[0])
        {
            SoundManager.Instance.PauseMusic(m_soundType);
            SoundManager.Instance.PlaySound(m_soundType, --CurrenSongIndex);
        }
    }

    private void NextMusic()
    {
        if (CurrenSongIndex + 1 <= StaticData.Song[StaticData.Song.Length - 1])
        {
            SoundManager.Instance.PauseMusic(m_soundType);
            SoundManager.Instance.PlaySound(m_soundType, ++CurrenSongIndex);
        }
    }

    private void PlayOrPauseMusic(bool isActive)
    {
        if (isActive)
        {
            SoundManager.Instance.PlaySound(m_soundType, CurrenSongIndex);
            playImg.overrideSprite = ResourcesLoader.Instance.LoadResources<Sprite>(StaticData.UI_PIC, "Pause");
        }
        else
        {
            SoundManager.Instance.PauseMusic(m_soundType);
            playImg.overrideSprite = ResourcesLoader.Instance.LoadResources<Sprite>(StaticData.UI_PIC, "Play");
        }
    }

    private void MuteMusic(bool isActive)
    {
        SoundManager.Instance.MuteMusic(m_soundType);
        if (isActive)
        {        
            muteImg.overrideSprite = ResourcesLoader.Instance.LoadResources<Sprite>(StaticData.UI_PIC, "SoundOn"); 
        }
        else
        {
            muteImg.overrideSprite = ResourcesLoader.Instance.LoadResources<Sprite>(StaticData.UI_PIC, "SoundOff"); 
        }
    }
}
