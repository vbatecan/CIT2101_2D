using System;

namespace CaseClosed.Data
{
    /// <summary>
    /// Holds the evaluation results and final score breakdown for a concluded case investigation.
    /// </summary>
    [Serializable]
    public class CaseEvaluationResult
    {
        /// <summary>Total composite score achieved across quiz, evidence, contradictions, and time.</summary>
        public int totalScore;

        /// <summary>Star rating from 1 to 5 stars.</summary>
        public int starCount;

        /// <summary>Letter rank grade (S, A, B, C, D).</summary>
        public string rankGrade;

        /// <summary>Number of quiz questions answered correctly.</summary>
        public int correctQuizAnswers;

        /// <summary>Total number of questions in the conclusion quiz.</summary>
        public int totalQuizQuestions;

        /// <summary>Number of evidence items discovered by the player.</summary>
        public int evidenceFoundCount;

        /// <summary>Total number of required/available evidence items.</summary>
        public int totalEvidenceCount;

        /// <summary>Number of contradiction challenges successfully exposed.</summary>
        public int contradictionsCaughtCount;

        /// <summary>Total number of contradiction rules in the case.</summary>
        public int totalContradictionsCount;

        /// <summary>Total elapsed time in seconds from start to submission.</summary>
        public float completionTimeSeconds;

        /// <summary>Whether the primary suspect was correctly identified to solve the case.</summary>
        public bool isCaseSolved;
    }
}
