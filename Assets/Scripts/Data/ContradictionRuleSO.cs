using UnityEngine;
using CaseClosed.Enums;

namespace CaseClosed.Data
{
    /// <summary>
    /// ScriptableObject defining a contradiction rule, mapping statement nodes to required evidence items
    /// and character reaction states.
    /// </summary>
    [CreateAssetMenu(fileName = "NewContradictionRule", menuName = "Case Closed/Contradiction Rule")]
    public class ContradictionRuleSO : ScriptableObject
    {
        [Header("Rule Identification")]
        public string ruleId;
        public string ruleTitle;

        [Header("Trigger Requirements")]
        public string targetStatementNodeId;
        public string requiredEvidenceId;
        public string requiredClueId; // Optional specific clue requirement

        [Header("Character Reaction")]
        public CharacterExpression reactionExpression = CharacterExpression.Shocked;
        [TextArea(3, 5)]
        public string reactionDialogue;

        [Header("State Changes & Rewards")]
        public string unlockedDialogueNodeId;
        public string unlockedClueId;
        public string unlockedClueText;
        public int scoreBonus = 100;
    }
}
