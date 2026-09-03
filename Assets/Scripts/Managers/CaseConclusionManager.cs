using System;
using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Services;

namespace CaseClosed.Managers
{
    /// <summary>
    /// Controller MonoBehaviour managing the case conclusion evaluation workflow,
    /// delegating scoring logic to <see cref="CaseEvaluationService"/> and broadcasting results.
    /// Can be dragged directly onto a GameObject in the Unity Inspector.
    /// </summary>
    public class CaseConclusionManager : MonoBehaviour
    {
        /// <summary>Singleton instance of the CaseConclusionManager.</summary>
        public static CaseConclusionManager Instance { get; private set; }

        /// <summary>Event raised when a case evaluation has completed.</summary>
        public event Action<CaseEvaluationResult> OnCaseEvaluated;

        private readonly CaseEvaluationService evaluationService = new CaseEvaluationService();

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
        /// Evaluates player quiz answers against the active case, calculates composite score using <see cref="CaseEvaluationService"/>,
        /// plays appropriate audio feedback, and notifies subscribers.
        /// </summary>
        /// <param name="playerSelectedOptionIndices">List of option indices selected for each conclusion question.</param>
        /// <returns>A populated <see cref="CaseEvaluationResult"/>, or null if no case is active.</returns>
        public CaseEvaluationResult EvaluateCase(List<int> playerSelectedOptionIndices)
        {
            CaseSO activeCase = CaseManager.Instance?.activeCase;
            if (activeCase == null)
            {
                Debug.LogWarning("[CaseConclusion] Cannot evaluate case: activeCase is null");
                return null;
            }

            if (!CaseManager.Instance.IsReadyForConclusion())
            {
                Debug.LogWarning("[CaseConclusion] Cannot evaluate case: examine all evidence and expose every contradiction first.");
                return null;
            }

            int evidenceFoundCount = CaseManager.Instance.discoveredEvidenceIds.Count;
            int contradictionsCaughtCount = CaseManager.Instance.exposedContradictionIds.Count;
            float elapsedTime = CaseManager.Instance.ElapsedTime;

            Debug.Log($"[CaseConclusion] Evaluating case '{activeCase.caseTitle}' (DiscoveredEv: {evidenceFoundCount}, Contradictions: {contradictionsCaughtCount}, ElapsedTime: {elapsedTime:F1}s)");

            // Business logic calculation delegated to pure Service
            CaseEvaluationResult result = evaluationService.EvaluateCase(
                activeCase,
                playerSelectedOptionIndices,
                evidenceFoundCount,
                contradictionsCaughtCount,
                elapsedTime
            );

            if (result != null)
            {
                Debug.Log($"[CaseConclusion] Evaluation complete: Solved={result.isCaseSolved}, Score={result.totalScore}, Grade={result.rankGrade}, Stars={result.starCount}, CorrectQuiz={result.correctQuizAnswers}/{result.totalQuizQuestions}");

                // Play audio cues based on result
                if (result.isCaseSolved)
                {
                    AudioManager.Instance?.PlaySFX(AudioManager.Instance.caseSolvedSFX);
                }
                else
                {
                    AudioManager.Instance?.PlaySFX(AudioManager.Instance.caseFailedSFX);
                }

                OnCaseEvaluated?.Invoke(result);
            }

            return result;
        }
    }
}
