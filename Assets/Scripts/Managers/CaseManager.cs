using System;
using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Data;

namespace CaseClosed.Managers
{
    /// <summary>
    /// Central state coordinator MonoBehaviour tracking the active case, discovered evidence,
    /// unlocked clue logs, and exposed contradictions.
    /// Can be dragged directly onto a GameObject in the Unity Inspector.
    /// </summary>
    public class CaseManager : MonoBehaviour
    {
        /// <summary>Singleton instance of the CaseManager.</summary>
        public static CaseManager Instance { get; private set; }

        [Header("Current Active Case")]
        /// <summary>The currently loaded case ScriptableObject.</summary>
        public CaseSO activeCase;

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

        /// <summary>Elapsed investigation time in seconds since case load.</summary>
        public float ElapsedTime => Time.time - investigationStartTime;

        /// <summary>Event raised when a new case file is loaded into runtime.</summary>
        public event Action<CaseSO> OnCaseLoaded;

        /// <summary>Event raised when a new piece of evidence is discovered.</summary>
        public event Action<EvidenceSO> OnEvidenceDiscovered;

        /// <summary>Event raised when a new clue or deduction is unlocked (clueId, clueText).</summary>
        public event Action<string, string> OnClueUnlocked;

        /// <summary>Event raised when a contradiction is successfully exposed.</summary>
        public event Action<ContradictionRuleSO> OnContradictionExposed;

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
        /// Loads a new case into runtime, resetting discovery state sets, starting the investigation timer,
        /// and registering initially discovered evidence items.
        /// </summary>
        /// <param name="newCase">The case ScriptableObject to load.</param>
        public void LoadCase(CaseSO newCase)
        {
            activeCase = newCase;
            discoveredEvidenceIds.Clear();
            unlockedClueIds.Clear();
            unlockedCluesText.Clear();
            exposedContradictionIds.Clear();
            investigationStartTime = Time.time;

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
                OnEvidenceDiscovered?.Invoke(evidence);
                AudioManager.Instance?.PlayClueDiscovered();
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
