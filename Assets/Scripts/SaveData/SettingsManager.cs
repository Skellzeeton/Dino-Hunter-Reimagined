using System;
using System.IO;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    private static SettingsManager _instance;
    public static SettingsManager Instance
    {
        get
        {
            if (_instance == null && Application.isPlaying)
            {
                GameObject go = new GameObject("SettingsManager");
                _instance = go.AddComponent<SettingsManager>();
                DontDestroyOnLoad(go);
                _instance.Load();
            }
            return _instance;
        }
    }

    [Serializable]
    private class SettingsData
    {
        public bool musicOn = true;
        public bool soundOn = true;
        public float musicVolume = 1f;
        public float soundVolume = 1f;
        public float mouseSensitivity = 0.5f;
        public int lastSaveSlot = 0;
    }

    private SettingsData _data = new SettingsData();
    private bool _dirty;
    private string FilePath => System.IO.Path.Combine(Application.persistentDataPath, "settings.json");
    private string TempPath => FilePath + ".tmp";
    public bool MusicOn
    {
        get => _data.musicOn;
        set
        {
            if (_data.musicOn != value)
            {
                _data.musicOn = value;
                _dirty = true;
                Save();
            }
        }
    }

    public bool SoundOn
    {
        get => _data.soundOn;
        set
        {
            if (_data.soundOn != value)
            {
                _data.soundOn = value;
                _dirty = true;
                Save();
            }
        }
    }

    public float MusicVolume
    {
        get => _data.musicVolume;
        set
        {
            float clamped = Mathf.Clamp01(value);
            if (!Mathf.Approximately(_data.musicVolume, clamped))
            {
                _data.musicVolume = clamped;
                _dirty = true;
                Save();
            }
        }
    }

    public float SoundVolume
    {
        get => _data.soundVolume;
        set
        {
            float clamped = Mathf.Clamp01(value);
            if (!Mathf.Approximately(_data.soundVolume, clamped))
            {
                _data.soundVolume = clamped;
                _dirty = true;
                Save();
            }
        }
    }

    public float MouseSensitivity
    {
        get => _data.mouseSensitivity;
        set
        {
            float clamped = Mathf.Clamp(value, 0.5f, 5f);
            if (!Mathf.Approximately(_data.mouseSensitivity, clamped))
            {
                _data.mouseSensitivity = clamped;
                _dirty = true;
                Save();
            }
        }
    }

    public int LastSaveSlot
    {
        get => _data.lastSaveSlot;
        set
        {
            if (_data.lastSaveSlot != value)
            {
                _data.lastSaveSlot = Mathf.Clamp(value, 0, 4);
                _dirty = true;
                Save();
            }
        }
    }

    private void Load()
    {
        if (!File.Exists(FilePath))
        {
            _dirty = false;
            return;
        }
        try
        {
            string json = File.ReadAllText(FilePath);
            SettingsData loaded = JsonUtility.FromJson<SettingsData>(json);
            if (loaded != null)
                _data = loaded;
        }
        catch (Exception e)
        {
            Debug.LogWarning("SettingsManager: Failed to load settings, using defaults. " + e.Message);
        }
        _dirty = false;
    }

    private void Save()
    {
        if (!_dirty) return;

        try
        {
            string json = JsonUtility.ToJson(_data, true);
            File.WriteAllText(TempPath, json);
            if (File.Exists(FilePath))
                File.Replace(TempPath, FilePath, null, true);
            else
                File.Move(TempPath, FilePath);

            _dirty = false;
        }
        catch (Exception e)
        {
            Debug.LogError("SettingsManager: Failed to save settings. " + e.Message);
        }
    }

    private void OnApplicationQuit()
    {
        if (_dirty) Save();
    }

    public void ForceSave() => Save();
}