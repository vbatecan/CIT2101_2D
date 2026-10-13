using System;
using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Services;

namespace CaseClosed.Managers
{
    public class CaseConclusionManager : MonoBehaviour
    {
        public static CaseConclusionManager Instance { get; private set; }

        public event Action<CaseEvaluationResult> OnCaseEvaluated;

        private readonly CaseEvaluationService evaluationService = new CaseEvaluationService();

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public CaseEvaluationResult EvaluateCase(List<int> playerSelectedOptionIndices)
        {
            CaseSO activeCase = CaseManager.Instance?.ActiveCase;
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

            int evidenceFoundCount = CaseManager.Instance.DiscoveredEvidenceIds.Count;
            int contradictionsCaughtCount = CaseManager.Instance.ExposedContradictionIds.Count;
            float elapsedTime = CaseManager.Instance.ElapsedTime;

            Debug.Log($"[CaseConclusion] Evaluating case '{activeCase.caseTitle}' (DiscoveredEv: {evidenceFoundCount}, Contradictions: {contradictionsCaughtCount}, ElapsedTime: {elapsedTime:F1}s)");

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
