using System;

namespace CaseClosed.Data
{
    [Serializable]
    public class CaseEvaluationResult
    {
        public int totalScore;

        public int starCount;

        public string rankGrade;

        public int correctQuizAnswers;

        public int totalQuizQuestions;

        public int evidenceFoundCount;

        public int totalEvidenceCount;

        public int contradictionsCaughtCount;

        public int totalContradictionsCount;

        public float completionTimeSeconds;

        public bool isCaseSolved;
    }
}
