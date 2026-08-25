using System;
using UnityEngine;
using CaseClosed.Data;

namespace CaseClosed.Managers
{
    public class InterrogationManager : MonoBehaviour
    {
        public static InterrogationManager Instance { get; private set; }

        [Header("State")]
        public CharacterProfileSO currentSuspect;
        public DialogueTreeSO currentDialogueTree;
        public DialogueNode currentNode;
        public bool isChallengeModeActive = false;

        public event Action<CharacterProfileSO> OnSuspectChanged;
        public event Action<DialogueNode> OnDialogueNodeDisplayed;
        public event Action<CharacterExpression> OnExpressionChanged;
        public event Action<bool> OnChallengeModeToggled;
        public event Action<bool, string> OnChallengeResult; // success, message

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

        public void SetInterrogationTarget(CharacterProfileSO suspect, DialogueTreeSO dialogueTree)
        {
            currentSuspect = suspect;
            currentDialogueTree = dialogueTree;
            isChallengeModeActive = false;

            OnSuspectChanged?.Invoke(currentSuspect);

            if (currentDialogueTree != null && !string.IsNullOrEmpty(currentDialogueTree.startNodeId))
            {
                JumpToNode(currentDialogueTree.startNodeId);
            }
        }

        public void JumpToNode(string nodeId)
        {
            if (currentDialogueTree == null) return;
            DialogueNode targetNode = currentDialogueTree.GetNodeById(nodeId);
            if (targetNode != null)
            {
                currentNode = targetNode;
                OnDialogueNodeDisplayed?.Invoke(currentNode);
                OnExpressionChanged?.Invoke(currentNode.expression);
            }
        }

        public void AdvanceDialogue()
        {
            if (currentNode == null || isChallengeModeActive) return;

            if (currentNode.choices != null && currentNode.choices.Count > 0)
            {
                // Choices UI will handle navigation
                return;
            }

            if (!string.IsNullOrEmpty(currentNode.defaultNextNodeId))
            {
                JumpToNode(currentNode.defaultNextNodeId);
            }
        }

        public void ToggleChallengeMode(bool enable)
        {
            if (currentNode == null || !currentNode.isChallengeable)
            {
                OnChallengeModeToggled?.Invoke(false);
                return;
            }

            isChallengeModeActive = enable;
            OnChallengeModeToggled?.Invoke(isChallengeModeActive);
        }

        public void PresentEvidenceToChallenge(EvidenceSO presentedEvidence)
        {
            if (!isChallengeModeActive || currentNode == null || presentedEvidence == null) return;

            CaseSO activeCase = CaseManager.Instance?.activeCase;
            if (activeCase == null) return;

            ContradictionRuleSO matchingRule = null;
            foreach (var rule in activeCase.contradictionRules)
            {
                if (rule.targetStatementNodeId == currentNode.nodeId &&
                    rule.requiredEvidenceId == presentedEvidence.id)
                {
                    matchingRule = rule;
                    break;
                }
            }

            if (matchingRule != null)
            {
                // Success! Contradiction found
                CaseManager.Instance?.RegisterContradictionExposed(matchingRule);
                OnExpressionChanged?.Invoke(matchingRule.reactionExpression);
                OnChallengeResult?.Invoke(true, matchingRule.reactionDialogue);

                isChallengeModeActive = false;
                OnChallengeModeToggled?.Invoke(false);

                if (!string.IsNullOrEmpty(matchingRule.unlockedDialogueNodeId))
                {
                    JumpToNode(matchingRule.unlockedDialogueNodeId);
                }
            }
            else
            {
                // Challenge failed / No contradiction with this evidence
                CharacterExpression failExpression = GetFailureExpression(currentSuspect);
                OnExpressionChanged?.Invoke(failExpression);

                string responseText = GetFailureResponseText(currentSuspect, presentedEvidence);
                OnChallengeResult?.Invoke(false, responseText);
            }
        }

        private CharacterExpression GetFailureExpression(CharacterProfileSO profile)
        {
            if (profile == null) return CharacterExpression.Neutral;
            switch (profile.personalityTrait)
            {
                case PersonalityTrait.Defensive: return CharacterExpression.Defensive;
                case PersonalityTrait.Nervous: return CharacterExpression.Nervous;
                case PersonalityTrait.Calm: return CharacterExpression.Smug;
                case PersonalityTrait.Aggressive: return CharacterExpression.Angry;
                default: return CharacterExpression.Neutral;
            }
        }

        private string GetFailureResponseText(CharacterProfileSO profile, EvidenceSO evidence)
        {
            string sName = profile != null ? profile.fullName : "The suspect";
            return $"{sName} looks at the {evidence.evidenceName} unimpressed: \"That proves nothing about what I just said.\"";
        }
    }
}
