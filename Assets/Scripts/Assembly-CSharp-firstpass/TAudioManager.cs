using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TAudioManager : MonoBehaviour
{
    private class AudioInfo
    {
        public ITAudioEvent audioEvt;
        public bool loop;
        public bool sfx;
        public float volume;
    }

    private Dictionary<AudioSource, AudioInfo> m_playAudios = new Dictionary<AudioSource, AudioInfo>();
    private Dictionary<string, List<ITAudioRule>> m_audio_rules = new Dictionary<string, List<ITAudioRule>>();
    private bool m_isMusicOn = true;
    private bool m_isSoundOn = true;
    private float m_musicVolume = 1f;
    private float m_soundVolume = 1f;
    private AudioListener audioListener;
    private static TAudioManager s_instance;

    private readonly List<AudioSource> m_removeCache = new List<AudioSource>(32);

    public static TAudioManager instance
    {
        get
        {
            if (s_instance == null && Application.isPlaying)
            {
                GameObject target = new GameObject("TAudioManager", typeof(TAudioManager));
                s_instance = target.GetComponent<TAudioManager>();
                DontDestroyOnLoad(target);
            }
            return s_instance;
        }
    }

    public static bool checkInstance => s_instance != null;

    public bool isMusicOn
    {
        get => m_isMusicOn;
        set
        {
            if (m_isMusicOn == value) return;
            m_isMusicOn = value;
            SettingsManager.Instance.MusicOn = value;
            foreach (var pair in m_playAudios)
            {
                if (pair.Key == null) continue;
                if (pair.Value.sfx) continue;
                if (m_isMusicOn) pair.Key.Play();
                else pair.Key.Pause();
            }
        }
    }

    public bool isSoundOn
    {
        get => m_isSoundOn;
        set
        {
            if (m_isSoundOn == value) return;
            m_isSoundOn = value;
            SettingsManager.Instance.SoundOn = value;
            foreach (var pair in m_playAudios)
            {
                if (pair.Key == null) continue;
                var info = pair.Value;
                if (!info.sfx) continue;
                if (m_isSoundOn)
                {
                    if (info.loop && info.audioEvt != null)
                        info.audioEvt.Trigger();
                }
                else
                {
                    pair.Key.Stop();
                }
            }
        }
    }

    public float musicVolume
    {
        get => m_musicVolume;
        set
        {
            m_musicVolume = Mathf.Clamp01(value);
            SettingsManager.Instance.MusicVolume = m_musicVolume;
            foreach (var pair in m_playAudios)
            {
                if (pair.Key == null || pair.Value.sfx) continue;
                pair.Key.volume = pair.Value.volume * m_musicVolume;
            }
        }
    }

    public float soundVolume
    {
        get => m_soundVolume;
        set
        {
            m_soundVolume = Mathf.Clamp01(value);
            SettingsManager.Instance.SoundVolume = m_soundVolume;
            foreach (var pair in m_playAudios)
            {
                if (pair.Key == null || !pair.Value.sfx) continue;
                pair.Key.volume = pair.Value.volume * m_soundVolume;
            }
        }
    }

    private void Awake()
    {
        if (s_instance != null && s_instance != this)
        {
            Destroy(s_instance.gameObject);
        }
        m_isMusicOn   = SettingsManager.Instance.MusicOn;
        m_isSoundOn   = SettingsManager.Instance.SoundOn;
        m_musicVolume = SettingsManager.Instance.MusicVolume;
        m_soundVolume = SettingsManager.Instance.SoundVolume;
        var listener = FindObjectOfType<AudioListener>();
        if (listener == null)
        {
            var go = new GameObject("AudioListener", typeof(AudioListener));
            DontDestroyOnLoad(go);
            listener = go.GetComponent<AudioListener>();
        }
        audioListener = listener;
        s_instance = this;
    }

    private void OnDestroy()
    {
        if (s_instance == this)
            s_instance = null;
    }

    private void Update()
    {
        m_removeCache.Clear();
        foreach (var pair in m_playAudios)
        {
            AudioSource source = pair.Key;
            AudioInfo info = pair.Value;
            if (source == null)
            {
                m_removeCache.Add(pair.Key);
                continue;
            }
            if (info.loop)
                continue;
            if (info.audioEvt != null && info.audioEvt.isPlaying)
                continue;
            if (source.isPlaying)
                continue;
            m_removeCache.Add(pair.Key);
        }
        for (int i = 0; i < m_removeCache.Count; i++)
            m_playAudios.Remove(m_removeCache[i]);
    }

    public void Pause(AudioSource audio)
    {
        if (audio != null) audio.Pause();
    }

    public void PlaySound(AudioSource audio, AudioClip clip, bool loop) => PlaySound(audio, clip, loop, false);

    public void PlaySound(AudioSource audio, AudioClip clip, bool loop, bool cutoff)
    {
        if (audio == null || clip == null) return;
        float pitch = audio.pitch > 0.0001f ? audio.pitch : 1f;
        if (!TryPlay(audio.gameObject, clip.length / pitch)) return;
        AudioInfo info;
        if (!m_playAudios.TryGetValue(audio, out info))
        {
            info = new AudioInfo
            {
                audioEvt = audio.GetComponent<ITAudioEvent>(),
                loop     = loop,
                sfx      = true,
                volume   = audio.volume,
            };
            m_playAudios.Add(audio, info);
        }
        else
        {
            info.loop = loop;
            info.sfx  = true;
        }
        audio.volume = info.volume * m_soundVolume;
        audio.loop   = loop;
        audio.clip   = clip;
        if (!m_isSoundOn) return;
        if (loop || cutoff) audio.Play();
        else                audio.PlayOneShot(clip);
    }

    public void StopSound(AudioSource audio)
    {
        if (audio == null) return;
        audio.Stop();
        TryStop(audio.gameObject);
        m_playAudios.Remove(audio);
    }

    public void PlayMusic(AudioSource audio, AudioClip clip, bool loop) => PlayMusic(audio, clip, loop, false);

    public void PlayMusic(AudioSource audio, AudioClip clip, bool loop, bool cutoff)
    {
        if (audio == null || clip == null) return;
        float pitch = audio.pitch > 0.0001f ? audio.pitch : 1f;
        if (!TryPlay(audio.gameObject, clip.length / pitch)) return;
        AudioInfo info;
        if (!m_playAudios.TryGetValue(audio, out info))
        {
            info = new AudioInfo
            {
                audioEvt = audio.GetComponent<ITAudioEvent>(),
                loop     = loop,
                sfx      = false,
                volume   = audio.volume,
            };
            m_playAudios.Add(audio, info);
        }
        else
        {
            info.loop = loop;
            info.sfx  = false;
        }
        audio.volume = info.volume * m_musicVolume;
        audio.loop   = loop;
        audio.clip   = clip;
        if (!m_isMusicOn) return;
        if (loop || cutoff) audio.Play();
        else                audio.PlayOneShot(clip);
    }

    public void StopMusic(AudioSource audio)
    {
        if (audio == null) return;
        audio.Stop();
        TryStop(audio.gameObject);
        m_playAudios.Remove(audio);
    }

    public void StopAll()
    {
        foreach (var pair in m_playAudios)
        {
            if (pair.Key != null) pair.Key.Stop();
        }
        m_playAudios.Clear();
    }

    private static string TrimClone(string name)
    {
        int idx = name.IndexOf("(Clone)");
        return idx >= 0 ? name.Substring(0, idx) : name;
    }

    private bool TryPlay(GameObject go, float length)
    {
        if (go == null) return false;
        string name = TrimClone(go.name);
        if (!m_audio_rules.TryGetValue(name, out var rules)) return true;
        float overTime = Time.realtimeSinceStartup + length;
        for (int i = 0; i < rules.Count; i++)
            if (!rules[i].Try(name, go, overTime)) return false;
        return true;
    }

    private void TryStop(GameObject go)
    {
        if (go == null) return;
        string name = TrimClone(go.name);
        if (!m_audio_rules.TryGetValue(name, out var rules)) return;
        for (int i = 0; i < rules.Count; i++)
            rules[i].Stop(go);
    }

    public void RegistRule(string name, ITAudioRule rule)
    {
        if (!m_audio_rules.TryGetValue(name, out var rules))
        {
            rules = new List<ITAudioRule>();
            m_audio_rules.Add(name, rules);
        }
        rules.Add(rule);
    }

    public void ClearRegistRule() => m_audio_rules.Clear();

    public void AdjustMusicVolume(float delta) => musicVolume = Mathf.Clamp01(musicVolume + delta);

    public void AdjustSoundVolume(float delta) => soundVolume = Mathf.Clamp01(soundVolume + delta);

    public void ApplySavedVolumesToAllSources()
    {
        foreach (var pair in m_playAudios)
        {
            if (pair.Key == null) continue;
            var info = pair.Value;
            pair.Key.volume = info.volume * (info.sfx ? m_soundVolume : m_musicVolume);
        }
    }

    private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;

    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ApplySavedVolumesToAllSources();
}