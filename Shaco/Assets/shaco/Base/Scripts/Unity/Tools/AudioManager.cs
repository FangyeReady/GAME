/*
 * @Author: shaco 
 * @Date: 2018-05-14 15:52:50 
 * @Last Modified by: shaco
 * @Last Modified time: 2018-05-14 18:52:33
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{

    public class AudioManager : MonoBehaviour
    {
        private class AudioInfo
        {
            public string audioName = string.Empty;
            public AudioSource audio = null;
            
            public bool isPaused = false;
            public float startVolume = 0;
            public float endVolume = 0;
            public float duration = 0;
            public float currentDuration = 0;
            public System.Action completedCallBack = null;
            public System.Action fadeCompletedCallBack = null;
        }

        //所有音频文件
        private Dictionary<string, List<AudioInfo>> _audios = new Dictionary<string, List<AudioInfo>>();

        //被主动暂停的音频文件
        private Dictionary<string, int> _pausedAudios = new Dictionary<string, int>();

        //音频模板
        private AudioSource _audioModel = null;

        //全局音量大小(范围：0 ~ 1)
        private float _volume = 1.0f;

        //已将删除音频列表
        private Dictionary<string, int> _willRemoveKeys = new Dictionary<string, int>();

        void Update()
        {
            if (_audios.Count == 0)
                return;

            foreach (var iter in _audios)
            {
                var infoList = iter.Value;
                for (int i = infoList.Count - 1; i >= 0; --i)
                {
                    var infoTmp = infoList[i];
                    UpdateVolumeFade(infoTmp);

                    var isCompleted = IsAutoCompleted(infoTmp);
                    if (isCompleted)
                    {
                        if (null != infoTmp.completedCallBack)
                        {
                            infoTmp.completedCallBack();
                        }
                        infoTmp.audio.gameObject.RecyclingWithPool();
                        infoList.RemoveAt(i);
                    }
                }

                if (infoList.Count == 0)
                {
                    _willRemoveKeys.Add(iter.Key, 0);
                }
            }

            if (_willRemoveKeys.Count > 0)
            {
                foreach (var iter in _willRemoveKeys)
                {
                    _audios.Remove(iter.Key);
                }
                _willRemoveKeys.Clear();
            }
        }

        //播放音频，如果之前有暂停过音频，则恢复音频播放
        static public void Play(string audioName, bool loop = false, float crossDuration = 0)
        {
            Play(audioName, string.Empty, loop, crossDuration);
        }

        static public void Play(string audioName, string multiVersionControlRelativePath, bool loop = false, float crossDuration = 0)
        {
            var instanceTmp = GameEntry.GetComponentInstance<AudioManager>();

            List<AudioInfo> infoList = null;
            AudioInfo newAudioInfo = null;
            AudioSource audioSource = null;

            //仅恢复播放
            if (instanceTmp._pausedAudios.ContainsKey(audioName))
            {
                Resume(audioName, crossDuration);
                instanceTmp._pausedAudios.Remove(audioName);
                return;
            }
                
            //播放新音频
            newAudioInfo = new AudioInfo();
            audioSource = GetAudioSourceFromCache();
            
            if (!instanceTmp._audios.ContainsKey(audioName))
            {
                infoList = new List<AudioInfo>();
                instanceTmp._audios.Add(audioName, infoList);
            }
            else 
            {
                infoList = instanceTmp._audios[audioName];
            }
            infoList.Add(newAudioInfo);

            audioSource.clip = ResourcesEx.LoadResourcesOrLocal<AudioClip>(audioName, multiVersionControlRelativePath);
            audioSource.loop = loop;
            audioSource.volume = instanceTmp._volume;
            audioSource.transform.parent = instanceTmp.transform;

            if (null == audioSource.clip)
            {
                shaco.Log.Error("AudioManager Play error: not found clip by path=" + audioName);
            }

            newAudioInfo.audioName = audioName;
            newAudioInfo.audio = audioSource;
            newAudioInfo.fadeCompletedCallBack = null;

            if (crossDuration > 0)
            {
                newAudioInfo.startVolume = 0;
                newAudioInfo.endVolume = instanceTmp._volume;
                newAudioInfo.duration = crossDuration;
                newAudioInfo.currentDuration = 0;
            }
            else 
            {
                newAudioInfo.currentDuration = -1;
            }

            audioSource.Play();
        }

        //暂停
        static public void Pause(string audioName, float crossDuration = 0)
        {
            var instanceTmp = GameEntry.GetComponentInstance<AudioManager>();
            if (!instanceTmp._pausedAudios.ContainsKey(audioName))
                instanceTmp._pausedAudios.Add(audioName, 0);

            if (instanceTmp._audios.ContainsKey(audioName))
            {
                var infoList = instanceTmp._audios[audioName];
                for (int i = 0; i < infoList.Count; ++i)
                {
                    infoList[i].isPaused = true;
                }
            }

            DoWhenFadeCompleted(audioName, crossDuration, (AudioInfo info) =>
            {
                info.audio.Pause();
            });
        }

        static public void Stop(string audioName, float crossDuration = 0)
        {
            DoWhenFadeCompleted(audioName, crossDuration, (AudioInfo info) =>
            {
                var instanceTmp = GameEntry.GetComponentInstance<AudioManager>();
                if (!instanceTmp._willRemoveKeys.ContainsKey(audioName))
                {
                    info.audio.Stop();
                    info.audio.gameObject.RecyclingWithPool();
                    instanceTmp._willRemoveKeys.Add(audioName, 0);
                    instanceTmp._pausedAudios.Remove(audioName);
                }
            });
        }

        static public void PauseAll(float crossDuration = 0)
        {
            var instanceTmp = GameEntry.GetComponentInstance<AudioManager>();
            
            foreach (var iter in instanceTmp._audios)
            {
                Pause(iter.Key, crossDuration);
            }
        }

        //关闭所有声音
        static public void StopAll(float crossDuration = 0)
        {
            var instanceTmp = GameEntry.GetComponentInstance<AudioManager>();

            foreach (var iter in instanceTmp._audios)
            {
                Stop(iter.Key, crossDuration);
            }
        }

        //设置全局音量大小
        static public void SetVolume(float percent)
        {
            if (percent < 0) percent = 0;
            if (percent > 1) percent = 1;

            GameEntry.GetComponentInstance<AudioManager>()._volume = percent;
        }

        //获取音频信息
        static public AudioSource GetAudioSource(string audioName, int index = 0)
        {
            AudioSource retValue = null;
            var instanceTmp = GameEntry.GetComponentInstance<AudioManager>();
            if (!instanceTmp._audios.ContainsKey(audioName))
            {
                shaco.Log.Error("AudioManager GetAudioSource error: not found audio by name=" + audioName);
            }
            else 
            {
                var infoList = instanceTmp._audios[audioName];
                if (index < 0 || index > infoList.Count - 1)
                {
                    shaco.Log.Error("AudioManager GetAudioSource error: out of range index=" + index + " count=" + infoList.Count);
                }
                else 
                {
                    retValue = infoList[index].audio;
                }
            }
            return retValue;
        }

        static private void Resume(string audioName, float crossDuration = 0)
        {
            var instanceTmp = GameEntry.GetComponentInstance<AudioManager>();

            if (!instanceTmp._audios.ContainsKey(audioName))
            {
                shaco.Log.Error("AudioManager Resume error: not found audio by name=" + audioName);
            }
            else 
            {
                var infoList = instanceTmp._audios[audioName];
                for (int i = 0; i < infoList.Count; ++i)
                {
                    var audioInfoTmp = infoList[i];

                    audioInfoTmp.startVolume = audioInfoTmp.audio.volume;
                    audioInfoTmp.endVolume = instanceTmp._volume;
                    audioInfoTmp.duration = crossDuration;
                    audioInfoTmp.currentDuration = 0;
                    audioInfoTmp.fadeCompletedCallBack = null;

                    audioInfoTmp.audio.Play();
                    audioInfoTmp.isPaused = false;
                }
            }
        }

        //更新渐变音量大小
        static private void UpdateVolumeFade(AudioInfo info)
        {
            //音量渐变结束
            if (info.currentDuration < 0)
                return;

            info.currentDuration += Time.deltaTime;
            float durationPercent = 0;

            if (info.duration <= 0)
            {
                //立即完成
                durationPercent = 1.0f;
            }
            else
            {
                //渐变中
                durationPercent = info.currentDuration / info.duration;
            }

            if (durationPercent >= 1.0f)
            {
                //完成了
                durationPercent = 1.0f;
                info.currentDuration = -1;

                if (null != info.fadeCompletedCallBack)
                {
                    info.fadeCompletedCallBack();
                }
            }
            info.audio.volume = info.startVolume + (info.endVolume - info.startVolume) * durationPercent;
        }

        //音频是否自动播放完毕 
        static bool IsAutoCompleted(AudioInfo info)
        {
            //循环音乐不自动播放完毕
            if (info.audio.loop || info.isPaused)
                return false;

            return !info.audio.isPlaying;
        }

        static private void DoWhenFadeCompleted(string audioName, float duration, System.Action<AudioInfo> callback)
        {
            var instanceTmp = GameEntry.GetComponentInstance<AudioManager>();
            if (!instanceTmp._audios.ContainsKey(audioName))
            {
                shaco.Log.Error("AudioManager DoWhenFadeCompleted error: not found audio by name=" + audioName);
            }
            else
            {
                var infoList = instanceTmp._audios[audioName];

                for (int i = 0; i < infoList.Count; ++i)
                {
                    var audioInfoTmp = infoList[i];

                    audioInfoTmp.startVolume = audioInfoTmp.audio.volume;
                    audioInfoTmp.endVolume = 0;
                    audioInfoTmp.duration = duration;
                    audioInfoTmp.currentDuration = 0;

                    if (duration > 0)
                    {
                        audioInfoTmp.fadeCompletedCallBack = () =>
                        {
                            callback(audioInfoTmp);
                        };
                    }
                    else
                    {
                        callback(audioInfoTmp);
                    }
                }
            }
        }

        static private AudioSource GetAudioSourceFromCache()
        {
            var instanceTmp = GameEntry.GetComponentInstance<AudioManager>();
            if (instanceTmp._audioModel.IsNull())
            {
                var objTmp = new GameObject();
                objTmp.transform.parent = instanceTmp.transform;
                instanceTmp._audioModel = objTmp.AddComponent<AudioSource>();
                instanceTmp._audioModel.name = "AudioModel";
            }
            return instanceTmp._audioModel.gameObject.InstantiateWithPool().GetComponent<AudioSource>();
        }
    }
}