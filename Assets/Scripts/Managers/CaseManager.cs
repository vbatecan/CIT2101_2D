using System;
using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Services;
using CaseClosed.UI;

namespace CaseClosed.Managers
{
    /// <summary>
    /// Central state coordinator MonoBehaviour tracking the active case, discovered evidence,
    /// unlocked clue logs, exposed contradictions, and investigation countdown timer.
    /// Can be dragged directly onto a GameObject in the Unity Inspector.
    /// </summary>
    public class CaseManager : MonoBehaviour
    {
        /// <summary>Singleton instance of the CaseManager.</summary>
        public static CaseManager Instance { get; private set; }

        private readonly CaseTimerService timerService = new CaseTimerService();

        /// <summary>Pure domain service for timer and urgency calculations.</summary>
        public CaseTimerService TimerService => timerService;

        [Header("Current Active Case")]
        /// <summary>The currently loaded case ScriptableObject.</summary>
        public CaseSO activeCase;

        [Header("Active Player Investigator")]
        /// <summary>The investigator character selected by the player to solve the case.</summary>
        public CharacterProfileSO selectedInvestigator;

        /// <summary>Registered investigator characters available for player selection.</summary>
        public List<CharacterProfileSO> availableInvestigators = new List<CharacterProfileSO>();

        [Header("Runtime State Tracking")]
        /// <summary>Set of evidence IDs that have been discovered during the current investigation.</summary>
        public HashSet<string> discoveredEvidenceIds = new HashSet<string>();

        /// <summary>Set of clue IDs that have been unlocked during the current investigation.</summary>
        public HashSet<string> unlockedClueIds = new HashSet<string>();

        /// <summary>Dictionary mapping unlocked clue IDs to their descriptive observation text.</summary>
        public Dictionary<string, string> unlockedCluesText = new Dictionary<string, string>();

        /// <summary>Set of contradiction rule IDs that have been successfully challenged.</summary>
        public HashSet<string> exposedContradictionIds = new HashSet<string>();

        /// <summary>The timestamp when the investigation began.</summary>
        public float investigationStartTime;

        private float _accumulatedElapsedTime = 0f;
        private float _lastResumeTimestamp = 0f;

        /// <summary>Whether the active case has a countdown time limit configured.</summary>
        public bool HasActiveTimeLimit => activeCase != null && activeCase.hasTimeLimit && activeCase.timeLimitSeconds > 0f;

        /// <summary>Configured time limit in seconds for the current case.</summary>
        public float CaseTimeLimit => activeCase != null ? activeCase.timeLimitSeconds : 0f;

        /// <summary>Whether the investigation timer is actively running.</summary>
        public bool IsTimerRunning { get; private set; }

        /// <summary>Whether the case countdown time has expired (Game Over).</summary>
        public bool HasTimeExpired { get; private set; }

        /// <summary>Total elapsed active investigation time in seconds.</summary>
        public float ElapsedTime => _accumulatedElapsedTime + (IsTimerRunning ? (Time.time - _lastResumeTimestamp) : 0f);

        /// <summary>Remaining investigation time in seconds before game over.</summary>
        public float RemainingTime => timerService.CalculateRemainingTime(CaseTimeLimit, ElapsedTime);

        /// <summary>Event raised when a new case file is loaded into runtime.</summary>
        public event Action<CaseSO> OnCaseLoaded;

        /// <summary>Event raised when the player selects or switches their investigator character.</summary>
        public event Action<CharacterProfileSO> OnInvestigatorChanged;

        /// <summary>Event raised when a new piece of evidence is discovered.</summary>
        public event Action<EvidenceSO> OnEvidenceDiscovered;

        /// <summary>Event raised when a new clue or deduction is unlocked (clueId, clueText).</summary>
        public event Action<string, string> OnClueUnlocked;

        /// <summary>Event raised when a contradiction is successfully exposed.</summary>
        public event Action<ContradictionRuleSO> OnContradictionExposed;

        /// <summary>Event raised on every timer tick with remainingSeconds and elapsedSeconds.</summary>
        public event Action<float, float> OnTimerTick;

        /// <summary>Event raised when the case investigation time expires (Game Over).</summary>
        public event Action OnTimeExpired;

        /// <summary>
        /// Initializes the singleton instance.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(gameObject);
                else
                    Destroy(gameObject);
#else
                Destroy(gameObject);
#endif
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// <summary>
        /// Automatically loads activeCase on Start if assigned in the Inspector.
        /// </summary>
        private void Start()
        {
            if (activeCase != null && discoveredEvidenceIds.Count == 0)
            {
                LoadCase(activeCase);
            }
        }

        /// <summary>
        /// Frame update driving countdown ticks and checking for timer expiry.
        /// </summary>
        private void Update()
        {
            if (IsTimerRunning && HasActiveTimeLimit && !HasTimeExpired)
            {
                float remaining = RemainingTime;
                float elapsed = ElapsedTime;
                OnTimerTick?.Invoke(remaining, elapsed);

                if (remaining <= 0f || timerService.IsTimeExpired(activeCase.timeLimitSeconds, elapsed))
                {
                    TriggerTimeExpired();
                }
            }
        }

        /// <summary>
        /// Loads a new case into runtime, resetting discovery state sets, starting the investigation timer,
        /// assigning the active investigator, and registering initially discovered evidence items.
        /// </summary>
        /// <param name="newCase">The case ScriptableObject to load.</param>
        public void LoadCase(CaseSO newCase)
        {
            activeCase = newCase;
            discoveredEvidenceIds.Clear();
            unlockedClueIds.Clear();
            unlockedCluesText.Clear();
            exposedContradictionIds.Clear();
            _accumulatedElapsedTime = 0f;
            _lastResumeTimestamp = Time.time;
            investigationStartTime = Time.time;
            HasTimeExpired = false;
            IsTimerRunning = HasActiveTimeLimit;

            if (activeCase != null && selectedInvestigator != null)
            {
                activeCase.leadInvestigator = selectedInvestigator;
            }

            Debug.Log($"[CaseManager] Loading case: '{(newCase != null ? newCase.caseTitle : "NULL")}' (Level: {newCase?.levelNumber}, ID: {newCase?.caseId}, TimeLimit: {(HasActiveTimeLimit ? $"{CaseTimeLimit}s" : "Untimed")}, Investigator: '{selectedInvestigator?.fullName ?? "Unassigned"}')");

            if (activeCase != null && activeCase.evidenceItems != null)
            {
                foreach (var ev in activeCase.evidenceItems)
                {
                    if (ev != null)
                    {
                        ev.ResetRuntimeState();
                        if (ev.startsDiscovered)
                        {
                            RegisterDiscoveredEvidence(ev);
                        }
                    }
                }
            }

            OnCaseLoaded?.Invoke(activeCase);
            OnTimerTick?.Invoke(RemainingTime, 0f);
        }

        /// <summary>
        /// Pauses the investigation countdown timer (e.g. during Main Menu or Results screen).
        /// </summary>
        public void PauseTimer()
        {
            if (IsTimerRunning)
            {
                _accumulatedElapsedTime += Time.time - _lastResumeTimestamp;
                IsTimerRunning = false;
                Debug.Log($"[CaseManager] Timer paused. Total active elapsed time: {_accumulatedElapsedTime:F1}s");
            }
        }

        /// <summary>
        /// Resumes the countdown timer if the case is active and has not expired.
        /// </summary>
        public void ResumeTimer()
        {
            if (!IsTimerRunning && !HasTimeExpired && HasActiveTimeLimit)
            {
                _lastResumeTimestamp = Time.time;
                IsTimerRunning = true;
                Debug.Log($"[CaseManager] Timer resumed. Remaining time: {RemainingTime:F1}s");
            }
        }

        /// <summary>
        /// Triggers investigation failure due to time expiration (Game Over).
        /// </summary>
        public void TriggerTimeExpired()
        {
            if (HasTimeExpired) return;

            HasTimeExpired = true;
            IsTimerRunning = false;
            _accumulatedElapsedTime = CaseTimeLimit;

            Debug.Log($"[CaseManager] Investigation time expired for case '{(activeCase != null ? activeCase.caseTitle : "Unknown")}'. Game Over!");
            AudioManager.Instance?.PlayCaseFailed();
            OnTimeExpired?.Invoke();
            UIManager.Instance?.ShowPanel(UIPanelType.GameOver);
        }

        /// <summary>
        /// Restarts the current case investigation from the beginning with a fresh countdown.
        /// </summary>
        public void RetryCurrentCase()
        {
            if (activeCase != null)
            {
                Debug.Log($"[CaseManager] Retrying case '{activeCase.caseTitle}'...");
                LoadCase(activeCase);
            }
        }

        /// <summary>
        /// Sets the active investigator character for the player and updates the current case.
        /// </summary>
        /// <param name="investigator">The character profile of the investigator chosen by the player.</param>
        public void SetSelectedInvestigator(CharacterProfileSO investigator)
        {
            if (investigator == null) return;
            selectedInvestigator = investigator;

            if (activeCase != null)
            {
                activeCase.leadInvestigator = investigator;
            }

            Debug.Log($"[CaseManager] Active investigator changed to: '{investigator.fullName}' ({investigator.occupation})");
            OnInvestigatorChanged?.Invoke(investigator);
        }

        /// <summary>
        /// Registers a selectable investigator character if not already in the available list.
        /// </summary>
        /// <param name="investigator">The investigator profile to register.</param>
        public void RegisterAvailableInvestigator(CharacterProfileSO investigator)
        {
            if (investigator == null) return;
            if (!availableInvestigators.Contains(investigator))
            {
                availableInvestigators.Add(investigator);
            }
            if (selectedInvestigator == null)
            {
                SetSelectedInvestigator(investigator);
            }
        }

        /// <summary>
        /// Registers a piece of evidence as discovered if not already recorded, toggling its table presence and playing an audio cue.
        /// </summary>
        /// <param name="evidence">The discovered evidence item.</param>
        public void RegisterDiscoveredEvidence(EvidenceSO evidence)
        {
            if (evidence == null) return;
            if (!discoveredEvidenceIds.Contains(evidence.id))
            {
                discoveredEvidenceIds.Add(evidence.id);
                evidence.isToggledOnTable = true;
                Debug.Log($"[CaseManager] Registered new evidence discovery: '{evidence.evidenceName}' (ID: {evidence.id}). Total discovered: {discoveredEvidenceIds.Count}");
                OnEvidenceDiscovered?.Invoke(evidence);
                AudioManager.Instance?.PlayClueDiscovered();
            }
        }

        /// <summary>
        /// Unlocks a case evidence item by ID after a story or dialogue requirement is completed.
        /// </summary>
        /// <param name="evidenceId">The evidence ID configured on the active case.</param>
        public void UnlockEvidence(string evidenceId)
        {
            if (string.IsNullOrEmpty(evidenceId) || activeCase == null || activeCase.evidenceItems == null) return;

            foreach (EvidenceSO evidence in activeCase.evidenceItems)
            {
                if (evidence != null && evidence.id == evidenceId)
                {
                    RegisterDiscoveredEvidence(evidence);
                    return;
                }
            }
        }

        /// <summary>
        /// Unlocks a clue and registers its description in the notebook dictionary, notifying listeners.
        /// </summary>
        /// <param name="clueId">The unique clue identifier.</param>
        /// <param name="clueText">The descriptive text of the clue.</param>
        public void UnlockClue(string clueId, string clueText)
        {
            if (string.IsNullOrEmpty(clueId)) return;

            if (!unlockedClueIds.Contains(clueId))
            {
                unlockedClueIds.Add(clueId);
                unlockedCluesText[clueId] = clueText;
                Debug.Log($"[CaseManager] Unlocked new clue: '[{clueId}]' - \"{clueText}\". Total clues: {unlockedClueIds.Count}");
                OnClueUnlocked?.Invoke(clueId, clueText);
                AudioManager.Instance?.PlayClueDiscovered();
            }
        }

        /// <summary>
        /// Registers an exposed contradiction rule, unlocking any associated reward clues and playing an audio cue.
        /// </summary>
        /// <param name="rule">The contradiction rule that was exposed.</param>
        public void RegisterContradictionExposed(ContradictionRuleSO rule)
        {
            if (rule == null) return;

            if (!exposedContradictionIds.Contains(rule.ruleId))
            {
                exposedContradictionIds.Add(rule.ruleId);
                Debug.Log($"[CaseManager] Registered contradiction exposed: '{rule.ruleTitle}' (ID: {rule.ruleId}). Total contradictions caught: {exposedContradictionIds.Count}");

                if (!string.IsNullOrEmpty(rule.unlockedClueId))
                {
                    UnlockClue(rule.unlockedClueId, rule.unlockedClueText);
                }

                OnContradictionExposed?.Invoke(rule);
                AudioManager.Instance?.PlayContradictionFound();
            }
        }
    }
}
