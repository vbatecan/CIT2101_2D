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
    /// Unity-facing coordinator for a case session. Authored ScriptableObjects provide immutable
    /// case configuration; <see cref="CaseSessionState"/> owns all mutable play-session state.
    /// </summary>
    public class CaseManager : MonoBehaviour
    {
        private static CaseManager _instance;

        /// <summary>Singleton instance of the CaseManager.</summary>
        public static CaseManager Instance
        {
            get
            {
                if (_instance == null) _instance = FindFirstObjectByType<CaseManager>(FindObjectsInactive.Include);
                return _instance;
            }
            private set => _instance = value;
        }

        private readonly CaseTimerService timerService = new CaseTimerService();
        private readonly CaseSessionState sessionState = new CaseSessionState();
        private bool hasStartedSession;

        /// <summary>Whether runtime case initialization has completed.</summary>
        public bool HasStartedSession => hasStartedSession;

        /// <summary>Pure domain service for timer and urgency calculations.</summary>
        public CaseTimerService TimerService => timerService;

        /// <summary>Internal runtime-state seam used by pure domain services in the gameplay assembly.</summary>
        internal CaseSessionState SessionState => sessionState;

        [Header("Current Active Case")]
        [Tooltip("Initial authored case configuration. Runtime loading is held in CaseSessionState.")]
        [SerializeField] private CaseSO activeCase;

        [Header("Active Player Investigator")]
        [Tooltip("Initial investigator selection. Runtime selection is held in CaseSessionState.")]
        [SerializeField] private CharacterProfileSO selectedInvestigator;

        [Tooltip("Registered investigator characters available for player selection.")]
        [SerializeField] private List<CharacterProfileSO> availableInvestigators = new List<CharacterProfileSO>();

        [SerializeField, HideInInspector]
        [Tooltip("Legacy serialized timestamp retained for existing scenes. Session timing now lives in CaseSessionState.")]
        private float investigationStartTime;

        /// <summary>Current case configuration for this running session.</summary>
        public CaseSO ActiveCase => hasStartedSession ? sessionState.ActiveCase : activeCase;

        /// <summary>Player-selected investigator for this session, if one is selected.</summary>
        public CharacterProfileSO SelectedInvestigator => hasStartedSession ? sessionState.SelectedInvestigator : selectedInvestigator;

        /// <summary>Selected investigator or the case's authored default investigator.</summary>
        public CharacterProfileSO EffectiveInvestigator => hasStartedSession
            ? sessionState.EffectiveInvestigator
            : selectedInvestigator ?? activeCase?.leadInvestigator;

        /// <summary>Read-only investigator choices registered with the manager.</summary>
        public IReadOnlyList<CharacterProfileSO> AvailableInvestigators => availableInvestigators;

        /// <summary>Evidence discovered during this session. The collection cannot be mutated externally.</summary>
        public IReadOnlyCollection<string> DiscoveredEvidenceIds => sessionState.DiscoveredEvidenceIds;

        /// <summary>Clue identifiers unlocked during this session. The collection cannot be mutated externally.</summary>
        public IReadOnlyCollection<string> UnlockedClueIds => sessionState.UnlockedClueIds;

        /// <summary>Read-only clue text indexed by clue identifier for the current session.</summary>
        public IReadOnlyDictionary<string, string> UnlockedCluesText => sessionState.UnlockedClues;

        /// <summary>Contradictions exposed during this session. The collection cannot be mutated externally.</summary>
        public IReadOnlyCollection<string> ExposedContradictionIds => sessionState.ExposedContradictionIds;

        /// <summary>Completed dialogue tree identifiers during this session.</summary>
        public IReadOnlyCollection<string> CompletedDialogueTreeIds => sessionState.CompletedDialogueTreeIds;

        /// <summary>The timestamp when the active session began.</summary>
        public float InvestigationStartTime => sessionState.InvestigationStartedAt;

        /// <summary>Whether the active case has a countdown time limit configured.</summary>
        public bool HasActiveTimeLimit => ActiveCase != null && ActiveCase.hasTimeLimit && ActiveCase.timeLimitSeconds > 0f;

        /// <summary>Configured time limit in seconds for the current case.</summary>
        public float CaseTimeLimit => ActiveCase != null ? ActiveCase.timeLimitSeconds : 0f;

        /// <summary>Whether the investigation timer is actively running.</summary>
        public bool IsTimerRunning => sessionState.IsTimerRunning;

        /// <summary>Whether the case countdown time has expired (Game Over).</summary>
        public bool HasTimeExpired => sessionState.HasTimeExpired;

        /// <summary>Total elapsed active investigation time in seconds.</summary>
        public float ElapsedTime => sessionState.GetElapsedTime(Time.time);

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

        /// <summary>Event raised when a dialogue tree is completed.</summary>
        public event Action<string> OnDialogueTreeCompleted;

        /// <summary>Event raised when conclusion readiness state changes.</summary>
        public event Action<bool> OnConclusionReadinessChanged;

        private void Awake()
        {
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>Automatically loads the inspector-configured case once at startup.</summary>
        private void Start()
        {
            if (!hasStartedSession && activeCase != null)
            {
                LoadCase(activeCase);
            }
        }

        /// <summary>Drives countdown ticks and checks for timer expiry.</summary>
        private void Update()
        {
            if (!IsTimerRunning || !HasActiveTimeLimit || HasTimeExpired) return;

            float remaining = RemainingTime;
            float elapsed = ElapsedTime;
            OnTimerTick?.Invoke(remaining, elapsed);

            if (remaining <= 0f || timerService.IsTimeExpired(CaseTimeLimit, elapsed))
            {
                TriggerTimeExpired();
            }
        }

        /// <summary>
        /// Loads a new case into an isolated runtime session without changing its authored assets.
        /// </summary>
        /// <param name="newCase">The case ScriptableObject to load.</param>
        public void LoadCase(CaseSO newCase)
        {
            CharacterProfileSO investigator = SelectedInvestigator;
            sessionState.Begin(newCase, investigator, Time.time);
            hasStartedSession = true;

            Debug.Log($"[CaseManager] Loading case: '{(newCase != null ? newCase.caseTitle : "NULL")}' (Level: {newCase?.levelNumber}, ID: {newCase?.caseId}, TimeLimit: {(HasActiveTimeLimit ? $"{CaseTimeLimit}s" : "Untimed")}, Investigator: '{EffectiveInvestigator?.fullName ?? "Unassigned"}')");

            if (ActiveCase != null && ActiveCase.evidenceItems != null)
            {
                foreach (EvidenceSO evidence in ActiveCase.evidenceItems)
                {
                    if (evidence != null && evidence.startsDiscovered)
                    {
                        RegisterDiscoveredEvidence(evidence);
                    }
                }
            }

            OnCaseLoaded?.Invoke(ActiveCase);
            OnTimerTick?.Invoke(RemainingTime, 0f);
        }

        /// <summary>Pauses the investigation countdown timer.</summary>
        public void PauseTimer()
        {
            if (!IsTimerRunning) return;

            sessionState.PauseTimer(Time.time);
            Debug.Log($"[CaseManager] Timer paused. Total active elapsed time: {ElapsedTime:F1}s");
        }

        /// <summary>Resumes the countdown timer if the active case has not expired.</summary>
        public void ResumeTimer()
        {
            if (!sessionState.ResumeTimer(Time.time)) return;

            Debug.Log($"[CaseManager] Timer resumed. Remaining time: {RemainingTime:F1}s");
        }

        /// <summary>Triggers investigation failure due to time expiration (Game Over).</summary>
        public void TriggerTimeExpired()
        {
            if (!sessionState.ExpireTimer()) return;

            Debug.Log($"[CaseManager] Investigation time expired for case '{(ActiveCase != null ? ActiveCase.caseTitle : "Unknown")}'. Game Over!");
            AudioManager.Instance?.PlayCaseFailed();
            OnTimeExpired?.Invoke();
            UIManager.Instance?.ShowPanel(UIPanelType.GameOver);
        }

        /// <summary>Restarts the current case investigation with a fresh session state.</summary>
        public void RetryCurrentCase()
        {
            CaseSO caseToRetry = ActiveCase;
            if (caseToRetry == null) return;

            Debug.Log($"[CaseManager] Retrying case '{caseToRetry.caseTitle}'...");
            LoadCase(caseToRetry);
        }

        /// <summary>Sets the active investigator without mutating the authored case asset.</summary>
        /// <param name="investigator">The character profile chosen by the player.</param>
        public void SetSelectedInvestigator(CharacterProfileSO investigator)
        {
            if (investigator == null) return;

            if (hasStartedSession)
            {
                sessionState.SetSelectedInvestigator(investigator);
            }
            else
            {
                selectedInvestigator = investigator;
            }

            Debug.Log($"[CaseManager] Active investigator changed to: '{investigator.fullName}' ({investigator.occupation})");
            OnInvestigatorChanged?.Invoke(investigator);
        }

        /// <summary>Clears the runtime investigator selection and falls back to the authored case default.</summary>
        public void ClearSelectedInvestigator()
        {
            if (hasStartedSession)
            {
                sessionState.SetSelectedInvestigator(null);
            }
            else
            {
                selectedInvestigator = null;
            }

            OnInvestigatorChanged?.Invoke(null);
        }

        /// <summary>Registers a selectable investigator character if not already in the available list.</summary>
        public void RegisterAvailableInvestigator(CharacterProfileSO investigator)
        {
            if (investigator == null) return;

            if (!availableInvestigators.Contains(investigator))
            {
                availableInvestigators.Add(investigator);
            }

            if (SelectedInvestigator == null)
            {
                SetSelectedInvestigator(investigator);
            }
        }

        /// <summary>Registers a piece of evidence as discovered and notifies existing listeners once.</summary>
        public void RegisterDiscoveredEvidence(EvidenceSO evidence)
        {
            if (!sessionState.TryDiscoverEvidence(evidence)) return;

            Debug.Log($"[CaseManager] Registered new evidence discovery: '{evidence.evidenceName}' (ID: {evidence.id}). Total discovered: {sessionState.DiscoveredEvidenceCount}");
            OnEvidenceDiscovered?.Invoke(evidence);
            AudioManager.Instance?.PlayClueDiscovered();
            OnConclusionReadinessChanged?.Invoke(IsReadyForConclusion());
        }

        /// <summary>Returns whether evidence has been discovered during this session.</summary>
        public bool IsEvidenceDiscovered(EvidenceSO evidence)
        {
            return sessionState.IsEvidenceDiscovered(evidence);
        }

        /// <summary>Returns whether an evidence ID has been discovered during this session.</summary>
        public bool IsEvidenceDiscovered(string evidenceId)
        {
            return sessionState.IsEvidenceDiscovered(evidenceId);
        }

        /// <summary>Returns whether evidence has been examined during this session.</summary>
        public bool IsEvidenceExamined(EvidenceSO evidence)
        {
            return sessionState.IsEvidenceExamined(evidence);
        }

        /// <summary>Records evidence examination, returning false when it was already examined.</summary>
        public bool TryMarkEvidenceExamined(EvidenceSO evidence)
        {
            bool marked = sessionState.TryMarkEvidenceExamined(evidence);
            if (marked)
            {
                OnConclusionReadinessChanged?.Invoke(IsReadyForConclusion());
            }
            return marked;
        }

        /// <summary>Records a hotspot discovery, returning false when it was already discovered.</summary>
        public bool TryDiscoverHotspot(EvidenceSO evidence, EvidenceHotspot hotspot)
        {
            return sessionState.TryDiscoverHotspot(evidence, hotspot);
        }

        /// <summary>Returns whether a hotspot has been discovered during this session.</summary>
        public bool IsHotspotDiscovered(EvidenceSO evidence, EvidenceHotspot hotspot)
        {
            return sessionState.IsHotspotDiscovered(evidence, hotspot);
        }

        /// <summary>Returns how many hotspots of an evidence item were discovered in this session.</summary>
        public int GetDiscoveredHotspotCount(EvidenceSO evidence)
        {
            return sessionState.GetDiscoveredHotspotCount(evidence);
        }

        /// <summary>Toggles runtime table presence and returns the new state.</summary>
        public bool ToggleEvidenceTablePresence(EvidenceSO evidence)
        {
            return sessionState.ToggleEvidenceTablePresence(evidence);
        }

        /// <summary>Returns whether evidence is currently present on the investigation table.</summary>
        public bool IsEvidenceOnTable(EvidenceSO evidence)
        {
            return sessionState.IsEvidenceOnTable(evidence);
        }

        /// <summary>Returns true only after every active-case evidence item has been examined.</summary>
        public bool AreAllEvidenceExamined()
        {
            if (ActiveCase == null || ActiveCase.evidenceItems == null || ActiveCase.evidenceItems.Count < 3)
            {
                return false;
            }

            foreach (EvidenceSO evidence in ActiveCase.evidenceItems)
            {
                if (evidence == null || !sessionState.IsEvidenceExamined(evidence))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Returns true if all evidence items in the active case have been discovered/unlocked.</summary>
        public bool AreAllEvidenceUnlocked()
        {
            if (ActiveCase == null || ActiveCase.evidenceItems == null || ActiveCase.evidenceItems.Count == 0)
            {
                return false;
            }

            foreach (EvidenceSO evidence in ActiveCase.evidenceItems)
            {
                if (evidence != null && !sessionState.IsEvidenceDiscovered(evidence))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Returns true if all dialogue trees in the active case have completed.</summary>
        public bool AreAllDialoguesDone()
        {
            if (ActiveCase == null || ActiveCase.dialogueTrees == null || ActiveCase.dialogueTrees.Count == 0)
            {
                return true;
            }

            foreach (DialogueTreeSO tree in ActiveCase.dialogueTrees)
            {
                if (tree != null && !string.IsNullOrEmpty(tree.treeId) && !sessionState.IsDialogueCompleted(tree.treeId))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Records that a dialogue tree has reached completion.</summary>
        public void RecordDialogueCompleted(string treeId)
        {
            if (string.IsNullOrEmpty(treeId)) return;
            if (sessionState.TryMarkDialogueCompleted(treeId))
            {
                Debug.Log($"[CaseManager] Dialogue tree '{treeId}' marked completed. Total completed trees: {sessionState.CompletedDialogueCount}");
                OnDialogueTreeCompleted?.Invoke(treeId);
                OnConclusionReadinessChanged?.Invoke(IsReadyForConclusion());
            }
        }

        /// <summary>Returns true when the player can confront the culprit (all dialogs done and all evidence unlocked).</summary>
        public bool IsReadyForConclusion()
        {
            return AreAllEvidenceUnlocked() && AreAllDialoguesDone();
        }

        /// <summary>Unlocks an authored case evidence item by identifier.</summary>
        public void UnlockEvidence(string evidenceId)
        {
            if (string.IsNullOrEmpty(evidenceId) || ActiveCase == null || ActiveCase.evidenceItems == null) return;

            foreach (EvidenceSO evidence in ActiveCase.evidenceItems)
            {
                if (evidence != null && evidence.id == evidenceId)
                {
                    RegisterDiscoveredEvidence(evidence);
                    return;
                }
            }
        }

        /// <summary>Unlocks a clue and notifies existing listeners once.</summary>
        public void UnlockClue(string clueId, string clueText)
        {
            if (!sessionState.TryUnlockClue(clueId, clueText)) return;

            Debug.Log($"[CaseManager] Unlocked new clue: '[{clueId}]' - \"{clueText}\". Total clues: {sessionState.UnlockedClueIds.Count}");
            OnClueUnlocked?.Invoke(clueId, clueText);
            AudioManager.Instance?.PlayClueDiscovered();
        }

        /// <summary>Registers an exposed contradiction and any associated reward clue.</summary>
        public void RegisterContradictionExposed(ContradictionRuleSO rule)
        {
            if (!sessionState.TryExposeContradiction(rule)) return;

            Debug.Log($"[CaseManager] Registered contradiction exposed: '{rule.ruleTitle}' (ID: {rule.ruleId}). Total contradictions caught: {sessionState.ExposedContradictionCount}");

            if (!string.IsNullOrEmpty(rule.unlockedClueId))
            {
                UnlockClue(rule.unlockedClueId, rule.unlockedClueText);
            }

            OnContradictionExposed?.Invoke(rule);
            AudioManager.Instance?.PlayContradictionFound();
        }
    }
}
