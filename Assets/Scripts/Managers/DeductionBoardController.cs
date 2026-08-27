using System;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Services;

namespace CaseClosed.Managers
{
    /// <summary>
    /// Controller MonoBehaviour managing deduction board user interaction and clue selection state,
    /// delegating connection matching logic to <see cref="DeductionService"/>.
    /// Can be dragged directly onto a GameObject in the Unity Inspector.
    /// </summary>
    public class DeductionBoardController : MonoBehaviour
    {
        /// <summary>Singleton instance of the DeductionBoardController.</summary>
        public static DeductionBoardController Instance { get; private set; }

        /// <summary>The first selected clue ID on the board.</summary>
        public string selectedClueA;

        /// <summary>The second selected clue ID on the board.</summary>
        public string selectedClueB;

        /// <summary>Event raised when a clue is selected or deselected for connection.</summary>
        public event Action<string> OnClueSelectedForConnection;

        /// <summary>Event raised when a clue connection attempt completes (success flag and matched rule).</summary>
        public event Action<bool, ClueConnectionSO> OnConnectionResult;

        private readonly DeductionService deductionService = new DeductionService();

        /// <summary>
        /// Initializes the singleton instance.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Handles the selection of a clue on the deduction board, managing pair selection and triggering connection attempts.
        /// </summary>
        /// <param name="clueId">The unique identifier of the clicked clue.</param>
        public void SelectClue(string clueId)
        {
            if (string.IsNullOrEmpty(clueId)) return;

            if (string.IsNullOrEmpty(selectedClueA))
            {
                selectedClueA = clueId;
                Debug.Log($"[DeductionBoard] Selected first clue for pairing: '{selectedClueA}'");
                OnClueSelectedForConnection?.Invoke(selectedClueA);
            }
            else if (selectedClueA == clueId)
            {
                // Deselect if clicking the already selected clue
                Debug.Log($"[DeductionBoard] Deselected clue: '{selectedClueA}'");
                selectedClueA = null;
                OnClueSelectedForConnection?.Invoke(null);
            }
            else
            {
                selectedClueB = clueId;
                Debug.Log($"[DeductionBoard] Selected second clue for pairing: '{selectedClueB}'. Attempting deduction connection...");
                AttemptConnection(selectedClueA, selectedClueB);
            }
        }

        /// <summary>
        /// Clears all currently selected clues on the board.
        /// </summary>
        public void ClearSelection()
        {
            Debug.Log("[DeductionBoard] Selection cleared");
            selectedClueA = null;
            selectedClueB = null;
            OnClueSelectedForConnection?.Invoke(null);
        }

        /// <summary>
        /// Attempts to connect two clues, verifying against the active case rules via <see cref="DeductionService"/>.
        /// </summary>
        /// <param name="clueA">First clue ID in the connection.</param>
        /// <param name="clueB">Second clue ID in the connection.</param>
        private void AttemptConnection(string clueA, string clueB)
        {
            CaseSO activeCase = CaseManager.Instance?.activeCase;
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
                CaseManager.Instance?.UnlockClue(matchedRule.resultClueId, matchedRule.deductionText);
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
