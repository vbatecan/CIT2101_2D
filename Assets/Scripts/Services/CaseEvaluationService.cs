using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Data;

namespace CaseClosed.Services
{
    public class CaseEvaluationService
    {
        public CaseEvaluationResult EvaluateCase(
            CaseSO activeCase,
            List<int> playerSelectedOptionIndices,
            int evidenceFoundCount,
            int contradictionsCaughtCount,
            float elapsedTimeSeconds)
        {
            if (activeCase == null) return null;

            CaseEvaluationResult result = new CaseEvaluationResult
            {
                completionTimeSeconds = elapsedTimeSeconds
            };

            EvaluateQuiz(activeCase, playerSelectedOptionIndices, out int quizScore, out int correctCount, out bool isPrimarySuspectCorrect);
            result.correctQuizAnswers = correctCount;
            result.totalQuizQuestions = activeCase.conclusionQuestions.Count;
            result.isCaseSolved = activeCase.conclusionQuestions.Count == correctCount;

            int totalEv = activeCase.totalKeyEvidenceCount > 0 ? activeCase.totalKeyEvidenceCount : activeCase.evidenceItems.Count;
            result.evidenceFoundCount = evidenceFoundCount;
            result.totalEvidenceCount = totalEv;
            int evidenceScore = CalculateEvidenceScore(evidenceFoundCount, totalEv);

            int totalContra = activeCase.totalContradictionsCount > 0 ? activeCase.totalContradictionsCount : activeCase.contradictionRules.Count;
            result.contradictionsCaughtCount = contradictionsCaughtCount;
            result.totalContradictionsCount = totalContra;
            int contradictionScore = CalculateContradictionScore(contradictionsCaughtCount, totalContra);

            int timeBonus = CalculateTimeBonus(elapsedTimeSeconds, activeCase.parCompletionTimeSeconds);

            result.totalScore = quizScore + evidenceScore + contradictionScore + timeBonus;
            CalculateStarsAndGrade(result.totalScore, result.isCaseSolved, out int starCount, out string rankGrade);
            result.starCount = starCount;
            result.rankGrade = rankGrade;

            return result;
        }

        public void EvaluateQuiz(
            CaseSO activeCase,
            List<int> playerAnswers,
            out int quizScore,
            out int correctCount,
            out bool isPrimarySuspectCorrect)
        {
            quizScore = 0;
            correctCount = 0;
            isPrimarySuspectCorrect = false;

            if (activeCase == null || activeCase.conclusionQuestions == null) return;

            int totalQuestions = activeCase.conclusionQuestions.Count;
            for (int i = 0; i < totalQuestions; i++)
            {
                var q = activeCase.conclusionQuestions[i];
                int selectedIdx = (playerAnswers != null && i < playerAnswers.Count) ? playerAnswers[i] : -1;
                if (selectedIdx == q.correctOptionIndex)
                {
                    quizScore += q.pointValue;
                    correctCount++;
                }
            }

            isPrimarySuspectCorrect = (correctCount >= 1 && playerAnswers != null && playerAnswers.Count > 0 &&
                                      playerAnswers[0] == activeCase.conclusionQuestions[0].correctOptionIndex);
        }

        public int CalculateEvidenceScore(int evidenceFound, int totalEvidence)
        {
            return Mathf.RoundToInt(((float)evidenceFound / Mathf.Max(1, totalEvidence)) * 300f);
        }

        public int CalculateContradictionScore(int contradictionsCaught, int totalContradictions)
        {
            return Mathf.RoundToInt(((float)contradictionsCaught / Mathf.Max(1, totalContradictions)) * 300f);
        }

        public int CalculateTimeBonus(float completionTimeSeconds, float parTimeSeconds)
        {
            if (parTimeSeconds > 0f && completionTimeSeconds <= parTimeSeconds)
            {
                return Mathf.RoundToInt((1f - (completionTimeSeconds / parTimeSeconds)) * 200f);
            }
            return 0;
        }

        public void CalculateStarsAndGrade(int totalScore, bool isCaseSolved, out int starCount, out string rankGrade)
        {
            if (!isCaseSolved)
            {
                starCount = 1;
                rankGrade = "D";
                return;
            }

            if (totalScore >= 1500)
            {
                starCount = 5;
                rankGrade = "S";
            }
            else if (totalScore >= 1200)
            {
                starCount = 4;
                rankGrade = "A";
            }
            else if (totalScore >= 900)
            {
                starCount = 3;
                rankGrade = "B";
            }
            else
            {
                starCount = 2;
                rankGrade = "C";
            }
        }
    }
}
