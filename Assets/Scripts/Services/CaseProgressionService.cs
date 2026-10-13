using System;

namespace CaseClosed.Services
{
    public interface IProgressionStorage
    {
        bool HasKey(string key);
        int GetInt(string key, int defaultValue);
        void SetInt(string key, int value);
        void DeleteKey(string key);
        void Save();
    }

    public class PlayerPrefsProgressionStorage : IProgressionStorage
    {
        public bool HasKey(string key) => UnityEngine.PlayerPrefs.HasKey(key);
        public int GetInt(string key, int defaultValue) => UnityEngine.PlayerPrefs.GetInt(key, defaultValue);
        public void SetInt(string key, int value) => UnityEngine.PlayerPrefs.SetInt(key, value);
        public void DeleteKey(string key) => UnityEngine.PlayerPrefs.DeleteKey(key);
        public void Save() => UnityEngine.PlayerPrefs.Save();
    }

    public class CaseProgressionService
    {
        public const string PrefKeyCaseCompletedPrefix = "CaseClosed_CaseCompleted_";
        public const string PrefKeyHighestUnlocked = "CaseClosed_HighestUnlockedLevel";

        private static CaseProgressionService _instance;
        public static CaseProgressionService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CaseProgressionService();
                }
                return _instance;
            }
            set => _instance = value;
        }

        private IProgressionStorage _storage;

        public event Action OnProgressionChanged;

        public CaseProgressionService(IProgressionStorage storage = null)
        {
            _storage = storage ?? new PlayerPrefsProgressionStorage();
        }

        public void SetStorage(IProgressionStorage storage)
        {
            _storage = storage ?? new PlayerPrefsProgressionStorage();
            OnProgressionChanged?.Invoke();
        }

        public bool IsCaseUnlocked(int levelNumber)
        {
            if (levelNumber <= 1) return true;

            return IsCaseCompleted(levelNumber - 1);
        }

        public bool IsCaseCompleted(int levelNumber)
        {
            if (levelNumber < 1) return false;
            string key = $"{PrefKeyCaseCompletedPrefix}{levelNumber}";
            return _storage.GetInt(key, 0) == 1;
        }

        public void SetCaseCompleted(int levelNumber, bool completed = true)
        {
            if (levelNumber < 1) return;

            string key = $"{PrefKeyCaseCompletedPrefix}{levelNumber}";
            _storage.SetInt(key, completed ? 1 : 0);

            if (completed)
            {
                int currentHighest = GetHighestUnlockedLevel();
                int nextLevel = levelNumber + 1;
                if (nextLevel > currentHighest)
                {
                    _storage.SetInt(PrefKeyHighestUnlocked, nextLevel);
                }
            }

            _storage.Save();
            OnProgressionChanged?.Invoke();
        }

        public int GetHighestUnlockedLevel()
        {
            int highest = 1;
            for (int i = 1; i <= 10; i++)
            {
                if (IsCaseUnlocked(i)) highest = i;
                else break;
            }
            return highest;
        }

        public void ResetProgression(int maxCasesToClear = 10)
        {
            for (int i = 1; i <= maxCasesToClear; i++)
            {
                string key = $"{PrefKeyCaseCompletedPrefix}{i}";
                _storage.DeleteKey(key);
            }
            _storage.DeleteKey(PrefKeyHighestUnlocked);
            _storage.Save();
            OnProgressionChanged?.Invoke();
        }
    }
}
