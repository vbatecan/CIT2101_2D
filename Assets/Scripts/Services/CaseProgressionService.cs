using System;
using System.Collections.Generic;

namespace CaseClosed.Services
{
    /// <summary>
    /// Storage abstraction interface for saving and retrieving case progression data.
    /// Allows pure in-memory mocking in unit tests without touching PlayerPrefs.
    /// </summary>
    public interface IProgressionStorage
    {
        bool HasKey(string key);
        int GetInt(string key, int defaultValue);
        void SetInt(string key, int value);
        void DeleteKey(string key);
        void Save();
    }

    /// <summary>
    /// Unity PlayerPrefs-backed progression storage implementation.
    /// </summary>
    public class PlayerPrefsProgressionStorage : IProgressionStorage
    {
        public bool HasKey(string key) => UnityEngine.PlayerPrefs.HasKey(key);
        public int GetInt(string key, int defaultValue) => UnityEngine.PlayerPrefs.GetInt(key, defaultValue);
        public void SetInt(string key, int value) => UnityEngine.PlayerPrefs.SetInt(key, value);
        public void DeleteKey(string key) => UnityEngine.PlayerPrefs.DeleteKey(key);
        public void Save() => UnityEngine.PlayerPrefs.Save();
    }

    /// <summary>
    /// In-memory progression storage implementation for automated NUnit test suites.
    /// </summary>
    public class InMemoryProgressionStorage : IProgressionStorage
    {
        private readonly Dictionary<string, int> _data = new Dictionary<string, int>();

        public bool HasKey(string key) => _data.ContainsKey(key);
        public int GetInt(string key, int defaultValue) => _data.TryGetValue(key, out int val) ? val : defaultValue;
        public void SetInt(string key, int value) => _data[key] = value;
        public void DeleteKey(string key) => _data.Remove(key);
        public void Save() { }
        public void Clear() => _data.Clear();
    }

    /// <summary>
    /// Pure C# domain service tracking unlocked and completed cases across the detective campaign.
    /// Zero MonoBehaviour dependencies; 100% unit-testable.
    /// </summary>
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

        /// <summary>Event raised whenever case completion or unlock status changes.</summary>
        public event Action OnProgressionChanged;

        /// <summary>
        /// Initializes the service with a specified storage backend (defaults to PlayerPrefs).
        /// </summary>
        /// <param name="storage">Custom storage implementation or null for PlayerPrefs.</param>
        public CaseProgressionService(IProgressionStorage storage = null)
        {
            _storage = storage ?? new PlayerPrefsProgressionStorage();
        }

        /// <summary>
        /// Configures a new storage backend (e.g. for unit testing).
        /// </summary>
        public void SetStorage(IProgressionStorage storage)
        {
            _storage = storage ?? new PlayerPrefsProgressionStorage();
            OnProgressionChanged?.Invoke();
        }

        /// <summary>
        /// Determines whether the specified case level is unlocked and available to play.
        /// Case 01 is always unlocked by default. Case N is unlocked if Case N-1 is completed.
        /// </summary>
        /// <param name="levelNumber">1-based level index (1, 2, 3, etc.).</param>
        /// <returns>True if the case is unlocked; otherwise false.</returns>
        public bool IsCaseUnlocked(int levelNumber)
        {
            if (levelNumber <= 1) return true;

            // Level N is unlocked if level N - 1 has been completed
            return IsCaseCompleted(levelNumber - 1);
        }

        /// <summary>
        /// Checks whether the specified case level has been solved and completed.
        /// </summary>
        /// <param name="levelNumber">1-based level index.</param>
        /// <returns>True if completed; otherwise false.</returns>
        public bool IsCaseCompleted(int levelNumber)
        {
            if (levelNumber < 1) return false;
            string key = $"{PrefKeyCaseCompletedPrefix}{levelNumber}";
            return _storage.GetInt(key, 0) == 1;
        }

        /// <summary>
        /// Marks a case as completed (or incomplete) and persists the state.
        /// </summary>
        /// <param name="levelNumber">1-based level index.</param>
        /// <param name="completed">Whether the case was completed.</param>
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

        /// <summary>
        /// Returns the highest level number unlocked so far.
        /// </summary>
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

        /// <summary>
        /// Resets all campaign progress, locking all cases except Case 01.
        /// </summary>
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
