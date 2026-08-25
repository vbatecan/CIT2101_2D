using System;
using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Data;

namespace CaseClosed.Managers
{
    [Serializable]
    public class CaseEvaluationResult
    {
        public int totalScore;
        public int starCount; // 1 to 5
        public string rankGrade; // S, A, B, C, D
        public int correctQuizAnswers;
        public int totalQuizQuestions;
        public int evidenceFoundCount;
        public int totalEvidenceCount;
        public int contradictionsCaughtCount;
        public int totalContradictionsCount;
        public float completionTimeSeconds;
        public bool isCaseSolved;
    }

    public class CaseConclusionManager : MonoBehaviour
    {
        public static CaseConclusionManager Instance { get; private set; }

        public event Action<CaseEvaluationResult> OnCaseEvaluated;

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

        public CaseEvaluationResult EvaluateCase(List<int> playerSelectedOptionIndices)
        {
            CaseSO activeCase = CaseManager.Instance?.activeCase;
            if (activeCase == null) return null;

            CaseEvaluationResult result = new CaseEvaluationResult();
            result.completionTimeSeconds = CaseManager.Instance.ElapsedTime;

            // 1. Quiz Evaluation
            int quizScore = 0;
            int correctCount = 0;
            int totalQuestions = activeCase.conclusionQuestions.Count;

            for (int i = 0; i < totalQuestions; i++)
            {
                var q = activeCase.conclusionQuestions[i];
                int selectedIdx = (i < playerSelectedOptionIndices.Count) ? playerSelectedOptionIndices[i] : -1;
                if (selectedIdx == q.correctOptionIndex)
                {
                    quizScore += q.pointValue;
                    correctCount++;
                }
            }

            result.correctQuizAnswers = correctCount;
            result.totalQuizQuestions = totalQuestions;

            // Primary suspect question (Question 0) must be correct to solve case
            result.isCaseSolved = (correctCount >= 1 && playerSelectedOptionIndices.Count > 0 &&
                                  playerSelectedOptionIndices[0] == activeCase.conclusionQuestions[0].correctOptionIndex);

            // 2. Evidence Bonus
            int evidenceFound = CaseManager.Instance.discoveredEvidenceIds.Count;
            int totalEv = activeCase.totalKeyEvidenceCount > 0 ? activeCase.totalKeyEvidenceCount : activeCase.evidenceItems.Count;
            result.evidenceFoundCount = evidenceFound;
            result.totalEvidenceCount = totalEv;
            int evidenceScore = Mathf.RoundToInt(((float)evidenceFound / Mathf.Max(1, totalEv)) * 300f);

            // 3. Contradictions Bonus
            int contradictionsCaught = CaseManager.Instance.exposedContradictionIds.Count;
            int totalContra = activeCase.totalContradictionsCount > 0 ? activeCase.totalContradictionsCount : activeCase.contradictionRules.Count;
            result.contradictionsCaughtCount = contradictionsCaught;
            result.totalContradictionsCount = totalContra;
            int contradictionScore = Mathf.RoundToInt(((float)contradictionsCaught / Mathf.Max(1, totalContra)) * 300f);

            // 4. Time Bonus
            int timeBonus = 0;
            if (result.completionTimeSeconds <= activeCase.parCompletionTimeSeconds)
            {
                timeBonus = Mathf.RoundToInt((1f - (result.completionTimeSeconds / activeCase.parCompletionTimeSeconds)) * 200f);
            }

            // Total Calculation
            result.totalScore = quizScore + evidenceScore + contradictionScore + timeBonus;

            // Stars & Grade
            if (!result.isCaseSolved)
            {
                result.starCount = 1;
                result.rankGrade = "D";
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.caseFailedSFX);
            }
            else
            {
                if (result.totalScore >= 1500) { result.starCount = 5; result.rankGrade = "S"; }
                else if (result.totalScore >= 1200) { result.starCount = 4; result.rankGrade = "A"; }
                else if (result.totalScore >= 900) { result.starCount = 3; result.rankGrade = "B"; }
                else { result.starCount = 2; result.rankGrade = "C"; }

                AudioManager.Instance?.PlaySFX(AudioManager.Instance.caseSolvedSFX);
            }

            OnCaseEvaluated?.Invoke(result);
            return result;
        }
    }
}
