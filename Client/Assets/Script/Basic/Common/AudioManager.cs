using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class AudioManager : SingletonObject<AudioManager>
{
        public Dictionary<string, AudioClip> m_MusicDict= new Dictionary<string, AudioClip>();
        public Dictionary<string, AudioClip> m_ClipDict = new Dictionary<string, AudioClip>();
        Camera belongCamera;

        public void PlayMusic(string name)
        {
                return;
                if (!m_MusicDict.ContainsKey(name))
                {
                        AudioClip ac = ResourceManager.GetInst().Load("Audio/" + name, AssetResidentType.Always) as AudioClip;
                        m_MusicDict.Add(name, ac);
                        AudioController.AddToCategory(AudioController.GetCategory("Music"), ac, name);
                }
                AudioController.PlayMusic(name);
                Debug.Log("PlaySE  " + name);

        }
        public void PlaySE(string name, float vol = 1f, float delay = 0f)
        {
                if (string.IsNullOrEmpty(name))
                        return;
                if (!m_ClipDict.ContainsKey(name))
                {
                        AudioClip ac = ResourceManager.GetInst().Load("Audio/" + name, AssetResidentType.Temporary) as AudioClip;
                        m_ClipDict.Add(name, ac);
                        AudioController.AddToCategory(AudioController.GetCategory("SE"), ac, name); 
                }
                AudioController.Play(name, vol, delay);
                Debug.Log("PlaySE  " + name);
        }

        public void ReleaseAll()
        {
                foreach (var param in m_ClipDict)
                {
                        AudioController.RemoveAudioItem(param.Key);
                        Object.Destroy(param.Value);
                }
                m_ClipDict.Clear();
        }
}
