using System;
using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Data;

namespace CaseClosed.Managers
{
    public class CaseManager : MonoBehaviour
    {
        public static CaseManager Instance { get; private set; }

        [Header("Current Active Case")]
        public CaseSO activeCase;

        [Header("Runtime State Tracking")]
        public HashSet<string> discoveredEvidenceIds = new HashSet<string>();
        public HashSet<string> unlockedClueIds = new HashSet<string>();
        public Dictionary<string, string> unlockedCluesText = new Dictionary<string, string>();
        public HashSet<string> exposedContradictionIds = new HashSet<string>();

        public float investigationStartTime;
        public float ElapsedTime => Time.time - investigationStartTime;

        // Events
        public event Action<CaseSO> OnCaseLoaded;
        public event Action<EvidenceSO> OnEvidenceDiscovered;
        public event Action<string, string> OnClueUnlocked; // clueId, clueText
        public event Action<ContradictionRuleSO> OnContradictionExposed;

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

        public void LoadCase(CaseSO newCase)
        {
            activeCase = newCase;
            discoveredEvidenceIds.Clear();
            unlockedClueIds.Clear();
            unlockedCluesText.Clear();
            exposedContradictionIds.Clear();
            investigationStartTime = Time.time;

            if (activeCase != null)
            {
                foreach (var ev in activeCase.evidenceItems)
                {
                    ev.ResetRuntimeState();
                    if (ev.startsDiscovered)
                    {
                        RegisterDiscoveredEvidence(ev);
                    }
                }
            }

            OnCaseLoaded?.Invoke(activeCase);
        }

        public void RegisterDiscoveredEvidence(EvidenceSO evidence)
        {
            if (evidence == null) return;
            if (!discoveredEvidenceIds.Contains(evidence.id))
            {
                discoveredEvidenceIds.Add(evidence.id);
                evidence.isToggledOnTable = true;
                OnEvidenceDiscovered?.Invoke(evidence);
                AudioManager.Instance?.PlayClueDiscovered();
            }
        }

        public void UnlockClue(string clueId, string clueText)
        {
            if (string.IsNullOrEmpty(clueId)) return;

            if (!unlockedClueIds.Contains(clueId))
            {
                unlockedClueIds.Add(clueId);
                unlockedCluesText[clueId] = clueText;
                OnClueUnlocked?.Invoke(clueId, clueText);
                AudioManager.Instance?.PlayClueDiscovered();
            }
        }

        public void RegisterContradictionExposed(ContradictionRuleSO rule)
        {
            if (rule == null) return;

            if (!exposedContradictionIds.Contains(rule.ruleId))
            {
                exposedContradictionIds.Add(rule.ruleId);

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
