using CaseClosed.Data;
using CaseClosed.Enums;

namespace CaseClosed.Services
{
    public class InterrogationService
    {
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

        public string GetFailureResponseText(CharacterProfileSO profile, EvidenceSO evidence)
        {
            string suspectName = profile != null ? profile.fullName : "The suspect";
            string evidenceName = evidence != null ? evidence.evidenceName : "item";
            return $"{suspectName} looks at the {evidenceName} unimpressed: \"That proves nothing about what I just said.\"";
        }
    }
}
