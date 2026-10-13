using System;
using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Services;
using CaseClosed.UI;

namespace CaseClosed.Managers
{
    public class CaseManager : MonoBehaviour
    {
        private static CaseManager _instance;

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

        public bool HasStartedSession => hasStartedSession;

        public CaseTimerService TimerService => timerService;

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

        public CaseSO ActiveCase => hasStartedSession ? sessionState.ActiveCase : activeCase;

        public CharacterProfileSO SelectedInvestigator => hasStartedSession ? sessionState.SelectedInvestigator : selectedInvestigator;

        public CharacterProfileSO EffectiveInvestigator => hasStartedSession
            ? sessionState.EffectiveInvestigator
            : selectedInvestigator ?? activeCase?.leadInvestigator;

        public IReadOnlyList<CharacterProfileSO> AvailableInvestigators => availableInvestigators;

        public IReadOnlyCollection<string> DiscoveredEvidenceIds => sessionState.DiscoveredEvidenceIds;

        public IReadOnlyCollection<string> UnlockedClueIds => sessionState.UnlockedClueIds;

        public IReadOnlyDictionary<string, string> UnlockedCluesText => sessionState.UnlockedClues;

        public IReadOnlyCollection<string> ExposedContradictionIds => sessionState.ExposedContradictionIds;

        public IReadOnlyCollection<string> CompletedDialogueTreeIds => sessionState.CompletedDialogueTreeIds;

        public float InvestigationStartTime => sessionState.InvestigationStartedAt;

        public bool HasActiveTimeLimit => ActiveCase != null && ActiveCase.hasTimeLimit && ActiveCase.timeLimitSeconds > 0f;

        public float CaseTimeLimit => ActiveCase != null ? ActiveCase.timeLimitSeconds : 0f;

        public bool IsTimerRunning => sessionState.IsTimerRunning;

        public bool HasTimeExpired => sessionState.HasTimeExpired;

        public float ElapsedTime => sessionState.GetElapsedTime(Time.time);

        public float RemainingTime => timerService.CalculateRemainingTime(CaseTimeLimit, ElapsedTime);

        public event Action<CaseSO> OnCaseLoaded;

        public event Action<CharacterProfileSO> OnInvestigatorChanged;

        public event Action<EvidenceSO> OnEvidenceDiscovered;

        public event Action<string, string> OnClueUnlocked;

        public event Action<ContradictionRuleSO> OnContradictionExposed;

        public event Action<float, float> OnTimerTick;

        public event Action OnTimeExpired;

        public event Action<string> OnDialogueTreeCompleted;

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

        private void Start()
        {
            if (!hasStartedSession && activeCase != null)
            {
                LoadCase(activeCase);
            }
        }

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

        public void PauseTimer()
        {
            if (!IsTimerRunning) return;

            sessionState.PauseTimer(Time.time);
            Debug.Log($"[CaseManager] Timer paused. Total active elapsed time: {ElapsedTime:F1}s");
        }

        public void ResumeTimer()
        {
            if (!sessionState.ResumeTimer(Time.time)) return;

            Debug.Log($"[CaseManager] Timer resumed. Remaining time: {RemainingTime:F1}s");
        }

        public void TriggerTimeExpired()
        {
            if (!sessionState.ExpireTimer()) return;

            Debug.Log($"[CaseManager] Investigation time expired for case '{(ActiveCase != null ? ActiveCase.caseTitle : "Unknown")}'. Game Over!");
            AudioManager.Instance?.PlayCaseFailed();
            OnTimeExpired?.Invoke();
            UIManager.Instance?.ShowPanel(UIPanelType.GameOver);
        }

        public void RetryCurrentCase()
        {
            CaseSO caseToRetry = ActiveCase;
            if (caseToRetry == null) return;

            Debug.Log($"[CaseManager] Retrying case '{caseToRetry.caseTitle}'...");
            LoadCase(caseToRetry);
        }

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

        public void RegisterDiscoveredEvidence(EvidenceSO evidence)
        {
            if (!sessionState.TryDiscoverEvidence(evidence)) return;

            Debug.Log($"[CaseManager] Registered new evidence discovery: '{evidence.evidenceName}' (ID: {evidence.id}). Total discovered: {sessionState.DiscoveredEvidenceCount}");
            OnEvidenceDiscovered?.Invoke(evidence);
            AudioManager.Instance?.PlayClueDiscovered();
            OnConclusionReadinessChanged?.Invoke(IsReadyForConclusion());
        }

        public bool IsEvidenceDiscovered(EvidenceSO evidence)
        {
            return sessionState.IsEvidenceDiscovered(evidence);
        }

        public bool IsEvidenceDiscovered(string evidenceId)
        {
            return sessionState.IsEvidenceDiscovered(evidenceId);
        }

        public bool IsEvidenceExamined(EvidenceSO evidence)
        {
            return sessionState.IsEvidenceExamined(evidence);
        }

        public bool TryMarkEvidenceExamined(EvidenceSO evidence)
        {
            bool marked = sessionState.TryMarkEvidenceExamined(evidence);
            if (marked)
            {
                OnConclusionReadinessChanged?.Invoke(IsReadyForConclusion());
            }
            return marked;
        }

        public bool TryDiscoverHotspot(EvidenceSO evidence, EvidenceHotspot hotspot)
        {
            return sessionState.TryDiscoverHotspot(evidence, hotspot);
        }

        public bool IsHotspotDiscovered(EvidenceSO evidence, EvidenceHotspot hotspot)
        {
            return sessionState.IsHotspotDiscovered(evidence, hotspot);
        }

        public int GetDiscoveredHotspotCount(EvidenceSO evidence)
        {
            return sessionState.GetDiscoveredHotspotCount(evidence);
        }

        public bool ToggleEvidenceTablePresence(EvidenceSO evidence)
        {
            return sessionState.ToggleEvidenceTablePresence(evidence);
        }

        public bool IsEvidenceOnTable(EvidenceSO evidence)
        {
            return sessionState.IsEvidenceOnTable(evidence);
        }

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

        public bool IsReadyForConclusion()
        {
            InterrogationManager interrogationManager = InterrogationManager.Instance;
            if (interrogationManager != null && interrogationManager.CurrentNode != null)
            {
                return false;
            }

            return AreAllEvidenceUnlocked() && AreAllDialoguesDone();
        }

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

        public void UnlockClue(string clueId, string clueText)
        {
            if (!sessionState.TryUnlockClue(clueId, clueText)) return;

            Debug.Log($"[CaseManager] Unlocked new clue: '[{clueId}]' - \"{clueText}\". Total clues: {sessionState.UnlockedClueIds.Count}");
            OnClueUnlocked?.Invoke(clueId, clueText);
            AudioManager.Instance?.PlayClueDiscovered();
        }

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

        public IReadOnlyList<int> ConclusionAnswers => sessionState.ConclusionAnswers;

        public void SetConclusionAnswer(int questionIndex, int answerIndex)
        {
            sessionState.SetConclusionAnswer(questionIndex, answerIndex);
        }

        public int GetConclusionAnswer(int questionIndex)
        {
            return sessionState.GetConclusionAnswer(questionIndex);
        }

        public void ClearConclusionAnswers()
        {
            sessionState.ClearConclusionAnswers();
        }
    }
}
