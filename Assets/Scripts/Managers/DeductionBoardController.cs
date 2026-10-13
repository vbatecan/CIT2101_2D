using System;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Services;

namespace CaseClosed.Managers
{
    public class DeductionBoardController : MonoBehaviour
    {
        public static DeductionBoardController Instance { get; private set; }

        [Header("Legacy Selection Defaults")]
        [Tooltip("Retained to deserialize existing scenes. Runtime selection is held in DeductionSelectionState.")]
        [SerializeField] private string selectedClueA;

        [Tooltip("Retained to deserialize existing scenes. Runtime selection is held in DeductionSelectionState.")]
        [SerializeField] private string selectedClueB;

        public event Action<string> OnClueSelectedForConnection;

        public event Action<bool, ClueConnectionSO> OnConnectionResult;

        private readonly DeductionService deductionService = new DeductionService();
        private readonly DeductionSelectionState selectionState = new DeductionSelectionState();

        public string FirstSelectedClueId => selectionState.FirstSelectedClueId;

        public string SecondSelectedClueId => selectionState.SecondSelectedClueId;

        private void Awake()
        {
            Instance = this;
            selectionState.Restore(selectedClueA, selectedClueB);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SelectClue(string clueId)
        {
            DeductionSelectionState.Transition transition = selectionState.Select(clueId);
            switch (transition)
            {
                case DeductionSelectionState.Transition.FirstClueSelected:
                    Debug.Log($"[DeductionBoard] Selected first clue for pairing: '{FirstSelectedClueId}'");
                    OnClueSelectedForConnection?.Invoke(FirstSelectedClueId);
                    return;

                case DeductionSelectionState.Transition.FirstClueDeselected:
                    Debug.Log($"[DeductionBoard] Deselected clue: '{clueId}'");
                    OnClueSelectedForConnection?.Invoke(null);
                    return;

                case DeductionSelectionState.Transition.PairReady:
                    Debug.Log($"[DeductionBoard] Selected second clue for pairing: '{SecondSelectedClueId}'. Attempting deduction connection...");
                    AttemptConnection(FirstSelectedClueId, SecondSelectedClueId);
                    return;
            }
        }

        public void ClearSelection()
        {
            Debug.Log("[DeductionBoard] Selection cleared");
            selectionState.Clear();
            OnClueSelectedForConnection?.Invoke(null);
        }

        private void AttemptConnection(string clueA, string clueB)
        {
            CaseManager caseManager = CaseManager.Instance;
            CaseSO activeCase = caseManager != null ? caseManager.ActiveCase : null;
            if (activeCase == null)
            {
                Debug.LogWarning("[DeductionBoard] Cannot attempt connection: activeCase is null");
                return;
            }

            ClueConnectionSO matchedRule = deductionService.FindMatchingConnection(activeCase, clueA, clueB);

            if (matchedRule != null)
            {
                Debug.Log($"[DeductionBoard] Successful deduction! Matched '{matchedRule.connectionTitle}' -> Unlocks '{matchedRule.resultClueTitle}' (ID: {matchedRule.resultClueId})");
                caseManager.UnlockClue(matchedRule.resultClueId, matchedRule.deductionText);
                AudioManager.Instance?.PlayDeductionLinked();
                OnConnectionResult?.Invoke(true, matchedRule);
            }
            else
            {
                Debug.Log($"[DeductionBoard] No deduction found connecting '{clueA}' and '{clueB}'");
                OnConnectionResult?.Invoke(false, null);
            }

            ClearSelection();
        }
    }
}
