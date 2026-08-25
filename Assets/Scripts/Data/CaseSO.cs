using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaseClosed.Data
{
    [Serializable]
    public class ConclusionQuestion
    {
        public string questionId;
        public string questionText;
        public List<string> options = new List<string>();
        public int correctOptionIndex;
        public int pointValue = 250;
    }

    [CreateAssetMenu(fileName = "NewCase", menuName = "Case Closed/Case File")]
    public class CaseSO : ScriptableObject
    {
        [Header("Case Overview")]
        public string caseId;
        public string caseTitle;
        public string dateAndLocation;
        [TextArea(3, 5)]
        public string incidentDescription;
        [TextArea(2, 4)]
        public string objective;
        public string victimInfo;

        [Header("Characters & Suspects")]
        public CharacterProfileSO primarySuspect;
        public List<CharacterProfileSO> additionalSuspects = new List<CharacterProfileSO>();

        [Header("Evidence & Clues")]
        public List<EvidenceSO> evidenceItems = new List<EvidenceSO>();
        public List<ClueConnectionSO> clueConnections = new List<ClueConnectionSO>();

        [Header("Interrogation & Contradictions")]
        public List<DialogueTreeSO> dialogueTrees = new List<DialogueTreeSO>();
        public List<ContradictionRuleSO> contradictionRules = new List<ContradictionRuleSO>();

        [Header("Case Conclusion Quiz")]
        public List<ConclusionQuestion> conclusionQuestions = new List<ConclusionQuestion>();

        [Header("Scoring Criteria")]
        public int totalKeyEvidenceCount = 5;
        public int totalContradictionsCount = 3;
        public float parCompletionTimeSeconds = 300f; // 5 mins
    }
}
