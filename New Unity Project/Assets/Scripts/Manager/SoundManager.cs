using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum SoundType
{
    Click = 0,
    UISound,
    BGM,
    Song,
}

public class SoundPlayer : ObjBase
{
    public int ID = 0;
    public SoundType type;

    private AudioClip audioClip;
    public AudioClip AudioClipSet {
        get {
            return audioClip;
        }
        set {
            audioClip = value;
            m_AudioSource.clip = audioClip;
        }
    }
    private AudioSource source;
    private AudioSource m_AudioSource
    {
        get
        {
            if (source == null)
            {
                GameObject obj = new GameObject("SoundPlayer_" + type.ToString());
                source = obj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.clip = audioClip;
            }
            return source;
        }
    }

    public SoundPlayer(int id, SoundType type, AudioClip clip)
    {
        ID = id;
        this.type = type;
        this.audioClip = clip;
    }

    public void Play()
    {
        if (IsPlaying())
        {
            m_AudioSource.Stop();
        }
        m_AudioSource.Play();
    }

    public void Pause()
    {
        m_AudioSource.Pause();
    }

    public void Mute()
    {
        m_AudioSource.mute = !m_AudioSource.mute;
    }

    private bool IsPlaying()
    {
        return m_AudioSource.isPlaying;
    }

    protected override void OnDestoryed()
    {
        source = null;
        base.OnDestoryed();
    }



}

public class SoundManager : AutoStaticInstance<SoundManager> {

    //播放器集合，根据音频类型或应用范围来区分
    private Dictionary<SoundType, SoundPlayer> AudioPlayers = new Dictionary<SoundType, SoundPlayer>();
    //音频的缓存
    private Dictionary<SoundType, List<AudioClip>> AudioResourcesDic = new Dictionary<SoundType, List<AudioClip>>();


    /// <summary>
    /// 播放声音
    /// </summary>
    /// <param name="sound"></param>
    /// <param name="id"> </param>
    public void PlaySound(SoundType sound, int id)
    {
        if (!AudioPlayers.ContainsKey(sound))
        {
            AudioClip audio = FetchAudioClip(sound, id);
            SoundPlayer soundPlayer = new SoundPlayer(id, sound, audio);
            AudioPlayers.Add(sound, soundPlayer);
            AudioPlayers[sound].Play();
        }
        else
        {

            if (AudioPlayers[sound].ID == id)
            {
                AudioPlayers[sound].Play();
            }
            else
            {
                AudioClip audio = FetchAudioClip(sound, id);
                AudioPlayers[sound].AudioClipSet = audio;
                AudioPlayers[sound].ID = id;
                AudioPlayers[sound].Play();
            }
        }
    }

    /// <summary>
    /// 暂停音乐
    /// </summary>
    /// <param name="sound"></param>
    /// <param name="id"></param>
    public void PauseMusic(SoundType sound)
    {
        if (!AudioPlayers.ContainsKey(sound))
        {
            return;
        }

        AudioPlayers[sound].Pause();
    }

    /// <summary>
    /// 静音
    /// </summary>
    /// <param name="sound"></param>
    public void MuteMusic(SoundType sound)
    {
        if (!AudioPlayers.ContainsKey(sound))
        {
            return;
        }

        AudioPlayers[sound].Mute();
    }



    private AudioClip FetchAudioClip(SoundType sound, int id)
    {
        if (!AudioResourcesDic.ContainsKey(sound))
        {
            AudioResourcesDic[sound] = new List<AudioClip>();
        }
        AudioClip audio = AudioResourcesDic[sound].Find( clip => clip.name == id.ToString() );
        if (audio == null)
        {
            audio = ResourcesLoader.Instance.LoadResources<AudioClip>(StaticData.AUDIO_CLIP_PATH + sound.ToString() + "/" , id.ToString());
            AudioResourcesDic[sound].Add(audio);
        }
        return audio; 
    }
}
