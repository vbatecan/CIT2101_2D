using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaseClosed.Services
{
    /// <summary>
    /// Storage abstraction interface for saving and retrieving game settings.
    /// Allows pure in-memory mocking in unit tests without touching PlayerPrefs.
    /// </summary>
    public interface ISettingsStorage
    {
        bool HasKey(string key);
        float GetFloat(string key, float defaultValue);
        void SetFloat(string key, float value);
        int GetInt(string key, int defaultValue);
        void SetInt(string key, int value);
        void Save();
    }

    /// <summary>
    /// Unity PlayerPrefs-backed settings storage implementation.
    /// </summary>
    public class PlayerPrefsSettingsStorage : ISettingsStorage
    {
        public bool HasKey(string key) => PlayerPrefs.HasKey(key);
        public float GetFloat(string key, float defaultValue) => PlayerPrefs.GetFloat(key, defaultValue);
        public void SetFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);
        public int GetInt(string key, int defaultValue) => PlayerPrefs.GetInt(key, defaultValue);
        public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);
        public void Save() => PlayerPrefs.Save();
    }

    /// <summary>
    /// In-memory settings storage implementation for automated NUnit test suites.
    /// </summary>
    public class InMemorySettingsStorage : ISettingsStorage
    {
        private readonly Dictionary<string, float> _floats = new Dictionary<string, float>();
        private readonly Dictionary<string, int> _ints = new Dictionary<string, int>();

        public bool HasKey(string key) => _floats.ContainsKey(key) || _ints.ContainsKey(key);
        public float GetFloat(string key, float defaultValue) => _floats.TryGetValue(key, out float val) ? val : defaultValue;
        public void SetFloat(string key, float value) => _floats[key] = value;
        public int GetInt(string key, int defaultValue) => _ints.TryGetValue(key, out int val) ? val : defaultValue;
        public void SetInt(string key, int value) => _ints[key] = value;
        public void Save() { }
        public void Clear()
        {
            _floats.Clear();
            _ints.Clear();
        }
    }

    /// <summary>
    /// Pure C# domain service managing game audio levels, display presentation, and dialogue text pacing.
    /// Zero MonoBehaviour dependencies; 100% unit-testable.
    /// </summary>
    public class GameSettingsService
    {
        public const string PrefKeyBgmVolume = "CaseClosed_BGM_Volume";
        public const string PrefKeyBgmMuted = "CaseClosed_BGM_Muted";
        public const string PrefKeySfxVolume = "CaseClosed_SFX_Volume";
        public const string PrefKeySfxMuted = "CaseClosed_SFX_Muted";
        public const string PrefKeyDialogVolume = "CaseClosed_Dialog_Volume";
        public const string PrefKeyDialogMuted = "CaseClosed_Dialog_Muted";
        public const string PrefKeyFullscreen = "CaseClosed_Fullscreen";
        public const string PrefKeyTextSpeed = "CaseClosed_TextSpeed";
        public const string PrefKeyTypewriterEnabled = "CaseClosed_Typewriter_Enabled";

        public const float DefaultBgmVolume = 0.8f;
        public const float DefaultSfxVolume = 1.0f;
        public const float DefaultDialogVolume = 1.0f;
        public const bool DefaultFullscreen = true;
        public const float DefaultTextSpeed = 35f;
        public const bool DefaultTypewriterEnabled = true;

        private static GameSettingsService _instance;
        public static GameSettingsService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameSettingsService();
                }
                return _instance;
            }
            set => _instance = value;
        }

        private ISettingsStorage _storage;

        public float BgmVolume { get; private set; } = DefaultBgmVolume;
        public bool IsBgmMuted { get; private set; } = false;
        public float SfxVolume { get; private set; } = DefaultSfxVolume;
        public bool IsSfxMuted { get; private set; } = false;
        public float DialogVolume { get; private set; } = DefaultDialogVolume;
        public bool IsDialogMuted { get; private set; } = false;
        public bool IsFullscreen { get; private set; } = DefaultFullscreen;
        public float TextSpeed { get; private set; } = DefaultTextSpeed;
        public bool IsTypewriterEnabled { get; private set; } = DefaultTypewriterEnabled;

        public event Action OnSettingsChanged;

        public GameSettingsService(ISettingsStorage storage = null)
        {
            _storage = storage ?? new PlayerPrefsSettingsStorage();
            LoadSettings();
        }

        public void SetStorage(ISettingsStorage storage)
        {
            _storage = storage ?? new PlayerPrefsSettingsStorage();
            LoadSettings();
        }

        public void LoadSettings()
        {
            BgmVolume = Mathf.Clamp01(_storage.GetFloat(PrefKeyBgmVolume, DefaultBgmVolume));
            IsBgmMuted = _storage.GetInt(PrefKeyBgmMuted, 0) == 1;

            SfxVolume = Mathf.Clamp01(_storage.GetFloat(PrefKeySfxVolume, DefaultSfxVolume));
            IsSfxMuted = _storage.GetInt(PrefKeySfxMuted, 0) == 1;

            DialogVolume = Mathf.Clamp01(_storage.GetFloat(PrefKeyDialogVolume, DefaultDialogVolume));
            IsDialogMuted = _storage.GetInt(PrefKeyDialogMuted, 0) == 1;

            IsFullscreen = _storage.GetInt(PrefKeyFullscreen, DefaultFullscreen ? 1 : 0) == 1;
            TextSpeed = Mathf.Clamp(_storage.GetFloat(PrefKeyTextSpeed, DefaultTextSpeed), 15f, 100f);
            IsTypewriterEnabled = _storage.GetInt(PrefKeyTypewriterEnabled, DefaultTypewriterEnabled ? 1 : 0) == 1;

            OnSettingsChanged?.Invoke();
        }

        public void SaveSettings()
        {
            _storage.SetFloat(PrefKeyBgmVolume, BgmVolume);
            _storage.SetInt(PrefKeyBgmMuted, IsBgmMuted ? 1 : 0);

            _storage.SetFloat(PrefKeySfxVolume, SfxVolume);
            _storage.SetInt(PrefKeySfxMuted, IsSfxMuted ? 1 : 0);

            _storage.SetFloat(PrefKeyDialogVolume, DialogVolume);
            _storage.SetInt(PrefKeyDialogMuted, IsDialogMuted ? 1 : 0);

            _storage.SetInt(PrefKeyFullscreen, IsFullscreen ? 1 : 0);
            _storage.SetFloat(PrefKeyTextSpeed, TextSpeed);
            _storage.SetInt(PrefKeyTypewriterEnabled, IsTypewriterEnabled ? 1 : 0);

            _storage.Save();
            OnSettingsChanged?.Invoke();
        }

        public void SetBgmVolume(float volume)
        {
            BgmVolume = Mathf.Clamp01(volume);
            SaveSettings();
        }

        public void SetBgmMuted(bool muted)
        {
            IsBgmMuted = muted;
            SaveSettings();
        }

        public void ToggleBgmMute() => SetBgmMuted(!IsBgmMuted);

        public void SetSfxVolume(float volume)
        {
            SfxVolume = Mathf.Clamp01(volume);
            SaveSettings();
        }

        public void SetSfxMuted(bool muted)
        {
            IsSfxMuted = muted;
            SaveSettings();
        }

        public void ToggleSfxMute() => SetSfxMuted(!IsSfxMuted);

        public void SetDialogVolume(float volume)
        {
            DialogVolume = Mathf.Clamp01(volume);
            SaveSettings();
        }

        public void SetDialogMuted(bool muted)
        {
            IsDialogMuted = muted;
            SaveSettings();
        }

        public void ToggleDialogMute() => SetDialogMuted(!IsDialogMuted);

        public void SetFullscreen(bool fullscreen)
        {
            IsFullscreen = fullscreen;
            Screen.fullScreen = fullscreen;
            SaveSettings();
        }

        public void ToggleFullscreen() => SetFullscreen(!IsFullscreen);

        public void SetTextSpeed(float speed)
        {
            TextSpeed = Mathf.Clamp(speed, 15f, 100f);
            SaveSettings();
        }

        public void SetTypewriterEnabled(bool enabled)
        {
            IsTypewriterEnabled = enabled;
            SaveSettings();
        }

        public void ResetToDefaults()
        {
            BgmVolume = DefaultBgmVolume;
            IsBgmMuted = false;
            SfxVolume = DefaultSfxVolume;
            IsSfxMuted = false;
            DialogVolume = DefaultDialogVolume;
            IsDialogMuted = false;
            IsFullscreen = DefaultFullscreen;
            Screen.fullScreen = DefaultFullscreen;
            TextSpeed = DefaultTextSpeed;
            IsTypewriterEnabled = DefaultTypewriterEnabled;

            SaveSettings();
        }

        public float GetEffectiveBgmVolume() => IsBgmMuted ? 0f : BgmVolume;
        public float GetEffectiveSfxVolume() => IsSfxMuted ? 0f : SfxVolume;
        public float GetEffectiveDialogVolume() => IsDialogMuted ? 0f : DialogVolume;

        public static string GetTextSpeedLabel(float speed)
        {
            if (speed <= 25f) return $"Slow ({Mathf.RoundToInt(speed)} cps)";
            if (speed <= 45f) return $"Normal ({Mathf.RoundToInt(speed)} cps)";
            return $"Fast ({Mathf.RoundToInt(speed)} cps)";
        }
    }
}
