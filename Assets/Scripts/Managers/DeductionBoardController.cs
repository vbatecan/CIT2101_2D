using System;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Services;

namespace CaseClosed.Managers
{
    /// <summary>
    /// Unity adapter for deduction board interaction. Pair-selection state lives in
    /// <see cref="DeductionSelectionState"/> and connection matching lives in <see cref="DeductionService"/>.
    /// Can be dragged directly onto a GameObject in the Unity Inspector.
    /// </summary>
    public class DeductionBoardController : MonoBehaviour
    {
        /// <summary>Singleton instance of the DeductionBoardController.</summary>
        public static DeductionBoardController Instance { get; private set; }

        [Header("Legacy Selection Defaults")]
        [Tooltip("Retained to deserialize existing scenes. Runtime selection is held in DeductionSelectionState.")]
        [SerializeField] private string selectedClueA;

        [Tooltip("Retained to deserialize existing scenes. Runtime selection is held in DeductionSelectionState.")]
        [SerializeField] private string selectedClueB;

        /// <summary>Event raised when a clue is selected or deselected for connection.</summary>
        public event Action<string> OnClueSelectedForConnection;

        /// <summary>Event raised when a clue connection attempt completes (success flag and matched rule).</summary>
        public event Action<bool, ClueConnectionSO> OnConnectionResult;

        private readonly DeductionService deductionService = new DeductionService();
        private readonly DeductionSelectionState selectionState = new DeductionSelectionState();

        /// <summary>The first clue selected for the current runtime connection attempt.</summary>
        public string FirstSelectedClueId => selectionState.FirstSelectedClueId;

        /// <summary>The second clue selected for the current runtime connection attempt.</summary>
        public string SecondSelectedClueId => selectionState.SecondSelectedClueId;

        /// <summary>
        /// Initializes the singleton instance.
        /// </summary>
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

        /// <summary>
        /// Handles the selection of a clue on the deduction board, managing pair selection and triggering connection attempts.
        /// </summary>
        /// <param name="clueId">The unique identifier of the clicked clue.</param>
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

        /// <summary>
        /// Clears all currently selected clues on the board.
        /// </summary>
        public void ClearSelection()
        {
            Debug.Log("[DeductionBoard] Selection cleared");
            selectionState.Clear();
            OnClueSelectedForConnection?.Invoke(null);
        }

        /// <summary>
        /// Attempts to connect two clues, verifying against the active case rules via <see cref="DeductionService"/>.
        /// </summary>
        /// <param name="clueA">First clue ID in the connection.</param>
        /// <param name="clueB">Second clue ID in the connection.</param>
        private void AttemptConnection(string clueA, string clueB)
        {
            CaseManager caseManager = CaseManager.Instance;
            CaseSO activeCase = caseManager != null ? caseManager.ActiveCase : null;
            if (activeCase == null)
            {
                Debug.LogWarning("[DeductionBoard] Cannot attempt connection: activeCase is null");
                return;
            }

            // Connection matching logic delegated to Service
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
