using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Data;

namespace CaseClosed.Services
{
    /// <summary>
    /// Service responsible for calculating scoring, quiz accuracy, contradiction and evidence bonuses,
    /// par time calculations, letter ranks, and star ratings for case conclusions.
    /// </summary>
    public class CaseEvaluationService
    {
        /// <summary>
        /// Evaluates a completed investigation based on player quiz answers, discovered evidence,
        /// exposed contradictions, and elapsed time.
        /// </summary>
        /// <param name="activeCase">The active case data being evaluated.</param>
        /// <param name="playerSelectedOptionIndices">List of option indices selected by the player for each conclusion question.</param>
        /// <param name="evidenceFoundCount">The count of evidence items discovered during the investigation.</param>
        /// <param name="contradictionsCaughtCount">The count of contradiction rules successfully triggered.</param>
        /// <param name="elapsedTimeSeconds">Total elapsed time in seconds from case start to conclusion submission.</param>
        /// <returns>A populated <see cref="CaseEvaluationResult"/> containing scores, stars, rank grade, and solved status.</returns>
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

            // 1. Quiz Evaluation
            EvaluateQuiz(activeCase, playerSelectedOptionIndices, out int quizScore, out int correctCount, out bool isPrimarySuspectCorrect);
            result.correctQuizAnswers = correctCount;
            result.totalQuizQuestions = activeCase.conclusionQuestions.Count;
            result.isCaseSolved = isPrimarySuspectCorrect;

            // 2. Evidence Scoring
            int totalEv = activeCase.totalKeyEvidenceCount > 0 ? activeCase.totalKeyEvidenceCount : activeCase.evidenceItems.Count;
            result.evidenceFoundCount = evidenceFoundCount;
            result.totalEvidenceCount = totalEv;
            int evidenceScore = CalculateEvidenceScore(evidenceFoundCount, totalEv);

            // 3. Contradiction Scoring
            int totalContra = activeCase.totalContradictionsCount > 0 ? activeCase.totalContradictionsCount : activeCase.contradictionRules.Count;
            result.contradictionsCaughtCount = contradictionsCaughtCount;
            result.totalContradictionsCount = totalContra;
            int contradictionScore = CalculateContradictionScore(contradictionsCaughtCount, totalContra);

            // 4. Time Bonus
            int timeBonus = CalculateTimeBonus(elapsedTimeSeconds, activeCase.parCompletionTimeSeconds);

            // 5. Aggregate Score and Rank
            result.totalScore = quizScore + evidenceScore + contradictionScore + timeBonus;
            CalculateStarsAndGrade(result.totalScore, result.isCaseSolved, out int starCount, out string rankGrade);
            result.starCount = starCount;
            result.rankGrade = rankGrade;

            return result;
        }

        /// <summary>
        /// Grades the conclusion quiz questions against player answers.
        /// </summary>
        /// <param name="activeCase">The case containing conclusion questions.</param>
        /// <param name="playerAnswers">The player's selected option indices.</param>
        /// <param name="quizScore">Output accumulated point value from correct answers.</param>
        /// <param name="correctCount">Output total number of questions answered correctly.</param>
        /// <param name="isPrimarySuspectCorrect">Output indicating whether the required primary suspect question was answered correctly.</param>
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

            // Primary suspect question (Question 0) must be correct to solve case
            isPrimarySuspectCorrect = (correctCount >= 1 && playerAnswers != null && playerAnswers.Count > 0 &&
                                      playerAnswers[0] == activeCase.conclusionQuestions[0].correctOptionIndex);
        }

        /// <summary>
        /// Computes the evidence discovery score bonus based on discovered vs total items.
        /// </summary>
        /// <param name="evidenceFound">Number of evidence items found by player.</param>
        /// <param name="totalEvidence">Total required/available evidence count.</param>
        /// <returns>Calculated evidence score out of 300 points.</returns>
        public int CalculateEvidenceScore(int evidenceFound, int totalEvidence)
        {
            return Mathf.RoundToInt(((float)evidenceFound / Mathf.Max(1, totalEvidence)) * 300f);
        }

        /// <summary>
        /// Computes the contradiction score bonus based on exposed vs total contradictions.
        /// </summary>
        /// <param name="contradictionsCaught">Number of contradictions successfully challenged.</param>
        /// <param name="totalContradictions">Total contradiction rules in the case.</param>
        /// <returns>Calculated contradiction score out of 300 points.</returns>
        public int CalculateContradictionScore(int contradictionsCaught, int totalContradictions)
        {
            return Mathf.RoundToInt(((float)contradictionsCaught / Mathf.Max(1, totalContradictions)) * 300f);
        }

        /// <summary>
        /// Computes a time efficiency bonus if completed under par time.
        /// </summary>
        /// <param name="completionTimeSeconds">Time taken in seconds.</param>
        /// <param name="parTimeSeconds">Target par completion time in seconds.</param>
        /// <returns>Calculated time bonus up to 200 points.</returns>
        public int CalculateTimeBonus(float completionTimeSeconds, float parTimeSeconds)
        {
            if (parTimeSeconds > 0f && completionTimeSeconds <= parTimeSeconds)
            {
                return Mathf.RoundToInt((1f - (completionTimeSeconds / parTimeSeconds)) * 200f);
            }
            return 0;
        }

        /// <summary>
        /// Determines the star rating (1 to 5) and letter rank grade based on total score and solve status.
        /// </summary>
        /// <param name="totalScore">Composite score from quiz, evidence, contradictions, and time.</param>
        /// <param name="isCaseSolved">Whether the required suspect was correctly identified.</param>
        /// <param name="starCount">Output star rating from 1 to 5.</param>
        /// <param name="rankGrade">Output letter grade string (S, A, B, C, D).</param>
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
