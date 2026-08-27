using CaseClosed.Data;
using CaseClosed.Enums;

namespace CaseClosed.Services
{
    /// <summary>
    /// Service responsible for validating contradiction challenges during interrogations,
    /// calculating emotional reaction expressions, and generating contextual character dialogue.
    /// </summary>
    public class InterrogationService
    {
        /// <summary>
        /// Searches the active case for a contradiction rule that matches a presented evidence against the current statement node.
        /// </summary>
        /// <param name="activeCase">The active case containing contradiction rules.</param>
        /// <param name="statementNodeId">The ID of the statement node being challenged.</param>
        /// <param name="evidenceId">The ID of the evidence presented by the player.</param>
        /// <returns>The matching <see cref="ContradictionRuleSO"/> if a contradiction exists; otherwise, null.</returns>
        public ContradictionRuleSO FindMatchingContradiction(CaseSO activeCase, string statementNodeId, string evidenceId)
        {
            if (activeCase == null || activeCase.contradictionRules == null) return null;
            if (string.IsNullOrEmpty(statementNodeId) || string.IsNullOrEmpty(evidenceId)) return null;

            foreach (var rule in activeCase.contradictionRules)
            {
                if (rule != null &&
                    rule.targetStatementNodeId == statementNodeId &&
                    rule.requiredEvidenceId == evidenceId)
                {
                    return rule;
                }
            }

            return null;
        }

        /// <summary>
        /// Determines the emotional expression a suspect displays when a player presents irrelevant evidence or fails a challenge.
        /// </summary>
        /// <param name="profile">The character profile of the interrogated suspect.</param>
        /// <returns>The corresponding <see cref="CharacterExpression"/> suited to the character's personality.</returns>
        public CharacterExpression GetFailureExpression(CharacterProfileSO profile)
        {
            if (profile == null) return CharacterExpression.Neutral;

            switch (profile.personalityTrait)
            {
                case PersonalityTrait.Defensive:
                    return CharacterExpression.Defensive;
                case PersonalityTrait.Nervous:
                    return CharacterExpression.Nervous;
                case PersonalityTrait.Calm:
                case PersonalityTrait.Confident:
                    return CharacterExpression.Smug;
                case PersonalityTrait.Aggressive:
                    return CharacterExpression.Angry;
                case PersonalityTrait.Secretive:
                    return CharacterExpression.Thinking;
                default:
                    return CharacterExpression.Neutral;
            }
        }

        /// <summary>
        /// Generates dialogue text spoken by a suspect when a challenge fails.
        /// </summary>
        /// <param name="profile">The character profile of the interrogated suspect.</param>
        /// <param name="evidence">The irrelevant evidence presented by the player.</param>
        /// <returns>A formatted dialogue string representing the suspect's dismissive reaction.</returns>
        public string GetFailureResponseText(CharacterProfileSO profile, EvidenceSO evidence)
        {
            string suspectName = profile != null ? profile.fullName : "The suspect";
            string evidenceName = evidence != null ? evidence.evidenceName : "item";
            return $"{suspectName} looks at the {evidenceName} unimpressed: \"That proves nothing about what I just said.\"";
        }
    }
}
