using System;
using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Data;

namespace CaseClosed.Managers
{
    public class DeductionBoardController : MonoBehaviour
    {
        public static DeductionBoardController Instance { get; private set; }

        public string selectedClueA;
        public string selectedClueB;

        public event Action<string> OnClueSelectedForConnection;
        public event Action<bool, ClueConnectionSO> OnConnectionResult;

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

        public void SelectClue(string clueId)
        {
            if (string.IsNullOrEmpty(clueId)) return;

            if (string.IsNullOrEmpty(selectedClueA))
            {
                selectedClueA = clueId;
                OnClueSelectedForConnection?.Invoke(selectedClueA);
            }
            else if (selectedClueA == clueId)
            {
                // Deselect
                selectedClueA = null;
                OnClueSelectedForConnection?.Invoke(null);
            }
            else
            {
                selectedClueB = clueId;
                AttemptConnection(selectedClueA, selectedClueB);
            }
        }

        public void ClearSelection()
        {
            selectedClueA = null;
            selectedClueB = null;
            OnClueSelectedForConnection?.Invoke(null);
        }

        private void AttemptConnection(string clueA, string clueB)
        {
            CaseSO activeCase = CaseManager.Instance?.activeCase;
            if (activeCase == null) return;

            ClueConnectionSO matchedRule = null;
            foreach (var rule in activeCase.clueConnections)
            {
                if (rule.Matches(clueA, clueB))
                {
                    matchedRule = rule;
                    break;
                }
            }

            if (matchedRule != null)
            {
                CaseManager.Instance?.UnlockClue(matchedRule.resultClueId, matchedRule.deductionText);
                AudioManager.Instance?.PlayDeductionLinked();
                OnConnectionResult?.Invoke(true, matchedRule);
            }
            else
            {
                OnConnectionResult?.Invoke(false, null);
            }

            ClearSelection();
        }
    }
}
