using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum SoundType
{
    Click = 0,
    UISound,
    BGM,
    Song,
}

public class SoundPlayer : ObjBase
{
    public string ID = string.Empty;
    public SoundType type;

    private AudioClip audioClip;
    public AudioClip SetAudioClip {
        get {
            return audioClip;
        }
        set {
            audioClip = value;
            source.clip = audioClip;
        }
    }
    private AudioSource source;

    public SoundPlayer(string id, SoundType type, AudioClip clip)
    {
        ID = id;
        this.type = type;
        this.audioClip = clip;

        GameObject obj = new GameObject("SoundPlayer_" + type.ToString());
        source = obj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.clip = audioClip;
    }

    public void Play()
    {
        if (IsPlaying())
        {
            source.Stop();
        }
        source.Play();
    }

    public void Pause()
    {
        source.Pause();
    }

    public void Mute()
    {
        source.mute = !source.mute;
    }

    private bool IsPlaying()
    {
        return source.isPlaying;
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
    public void PlaySound(SoundType sound, string id)
    {
        if (!AudioPlayers.ContainsKey(sound))
        {
            FetchAudioClip(sound, id, (audio)=>{
                SoundPlayer soundPlayer = new SoundPlayer(id, sound, audio);
                AudioPlayers.Add(sound, soundPlayer);
                AudioPlayers[sound].Play();
            });

        }
        else
        {

            if (AudioPlayers[sound].ID == id)
            {
                AudioPlayers[sound].Play();
            }
            else
            {
                FetchAudioClip(sound, id, (audio)=>{
                    AudioPlayers[sound].SetAudioClip = audio;
                    AudioPlayers[sound].ID = id;
                    AudioPlayers[sound].Play();
                });
              
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



    private void FetchAudioClip(SoundType sound, string id, Action<AudioClip> callback)
    {
        if (!AudioResourcesDic.ContainsKey(sound))
        {
            AudioResourcesDic[sound] = new List<AudioClip>();
        }
        AudioClip audio = AudioResourcesDic[sound].Find( clip => clip.name == id.ToString() );
        if (audio == null)
        {         
            ResourcesLoader.Instance.GetRes<AudioClip>(id.ToString(),
                (clip)=> {
                    audio = clip;
                    AudioResourcesDic[sound].Add(audio);
                    callback(audio);
                });    
        }
    }

    public override void Save()
    {
        throw new NotImplementedException();
    }
}
