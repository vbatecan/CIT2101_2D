using System;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Services;

namespace CaseClosed.Managers
{
    /// <summary>
    /// Controller MonoBehaviour managing interrogation game flow, dialogue progression, and challenge mode presentation,
    /// delegating contradiction rule matching and failure reactions to <see cref="InterrogationService"/>.
    /// Can be dragged directly onto a GameObject in the Unity Inspector.
    /// </summary>
    public class InterrogationManager : MonoBehaviour
    {
        /// <summary>Singleton instance of the InterrogationManager.</summary>
        public static InterrogationManager Instance { get; private set; }

        [Header("State")]
        /// <summary>The current suspect or witness being interrogated.</summary>
        public CharacterProfileSO currentSuspect;

        /// <summary>The active dialogue tree.</summary>
        public DialogueTreeSO currentDialogueTree;

        /// <summary>The current active dialogue statement node.</summary>
        public DialogueNode currentNode;

        /// <summary>Flag indicating whether challenge mode is active (awaiting evidence presentation).</summary>
        public bool isChallengeModeActive = false;

        /// <summary>Event raised when the interrogated suspect changes.</summary>
        public event Action<CharacterProfileSO> OnSuspectChanged;

        /// <summary>Event raised when a dialogue node is presented.</summary>
        public event Action<DialogueNode> OnDialogueNodeDisplayed;

        /// <summary>Event raised when the suspect's facial expression changes.</summary>
        public event Action<CharacterExpression> OnExpressionChanged;

        /// <summary>Event raised when challenge mode is toggled on or off.</summary>
        public event Action<bool> OnChallengeModeToggled;

        /// <summary>Event raised when a challenge attempt completes (success flag, response message).</summary>
        public event Action<bool, string> OnChallengeResult;

        /// <summary>Event raised when dialogue is dismissed or completed.</summary>
        public event Action OnDialogueClosed;

        private readonly InterrogationService interrogationService = new InterrogationService();

        /// <summary>
        /// Initializes the singleton instance.
        /// </summary>
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

        /// <summary>
        /// Sets the active suspect and dialogue tree for an interrogation session.
        /// </summary>
        /// <param name="suspect">The character profile of the suspect.</param>
        /// <param name="dialogueTree">The initial dialogue tree.</param>
        public void SetInterrogationTarget(CharacterProfileSO suspect, DialogueTreeSO dialogueTree)
        {
            currentSuspect = suspect;
            currentDialogueTree = dialogueTree;
            isChallengeModeActive = false;

            Debug.Log($"[Interrogation] Set interrogation target: '{suspect?.fullName}' (Tree: '{dialogueTree?.treeId}')");

            OnSuspectChanged?.Invoke(currentSuspect);

            if (currentDialogueTree != null && !string.IsNullOrEmpty(currentDialogueTree.startNodeId))
            {
                JumpToNode(currentDialogueTree.startNodeId);
            }
        }

        /// <summary>
        /// Navigates to a specific dialogue node by its identifier.
        /// </summary>
        /// <param name="nodeId">The target node ID to navigate to.</param>
        public void JumpToNode(string nodeId)
        {
            if (currentDialogueTree == null)
            {
                Debug.LogWarning("[Interrogation] Cannot jump to node: currentDialogueTree is null");
                return;
            }

            DialogueNode targetNode = currentDialogueTree.GetNodeById(nodeId);
            if (targetNode != null)
            {
                currentNode = targetNode;
                Debug.Log($"[Interrogation] Jumped to node '{nodeId}' (Speaker: '{targetNode.speakerName}', Expr: {targetNode.expression}, Challengeable: {targetNode.isChallengeable})");
                OnDialogueNodeDisplayed?.Invoke(currentNode);
                OnExpressionChanged?.Invoke(currentNode.expression);
            }
            else
            {
                Debug.LogWarning($"[Interrogation] Dialogue node '{nodeId}' not found in tree '{currentDialogueTree.treeId}'");
            }
        }

        /// <summary>
        /// Advances the dialogue to the default next node if no choices or active challenge block it.
        /// </summary>
        public void AdvanceDialogue()
        {
            if (currentNode == null || isChallengeModeActive) return;

            if (currentNode.choices != null && currentNode.choices.Count > 0)
            {
                // Branching choices UI handles navigation
                return;
            }

            if (!string.IsNullOrEmpty(currentNode.defaultNextNodeId))
            {
                Debug.Log($"[Interrogation] Advancing dialogue from '{currentNode.nodeId}' to default next '{currentNode.defaultNextNodeId}'");
                JumpToNode(currentNode.defaultNextNodeId);
            }
            else
            {
                Debug.Log($"[Interrogation] Reached end of dialogue branch for node '{currentNode.nodeId}'. Closing dialogue.");
                CloseDialogue();
            }
        }

        /// <summary>
        /// Closes the active dialogue window and returns to table exploration.
        /// </summary>
        public void CloseDialogue()
        {
            currentNode = null;
            isChallengeModeActive = false;
            OnChallengeModeToggled?.Invoke(false);
            OnDialogueClosed?.Invoke();
        }

        /// <summary>
        /// Toggles challenge mode on the current node if challengeable.
        /// </summary>
        /// <param name="enable">True to activate challenge mode; false to cancel.</param>
        public void ToggleChallengeMode(bool enable)
        {
            if (currentNode == null || !currentNode.isChallengeable)
            {
                Debug.Log($"[Interrogation] Current node '{(currentNode != null ? currentNode.nodeId : "NULL")}' is not challengeable.");
                OnChallengeModeToggled?.Invoke(false);
                return;
            }

            isChallengeModeActive = enable;
            Debug.Log($"[Interrogation] Challenge mode toggled: {isChallengeModeActive} for node '{currentNode.nodeId}'");
            OnChallengeModeToggled?.Invoke(isChallengeModeActive);
        }

        /// <summary>
        /// Presents a piece of evidence to challenge the current statement, evaluating contradictions via <see cref="InterrogationService"/>.
        /// </summary>
        /// <param name="presentedEvidence">The evidence item presented by the player.</param>
        public void PresentEvidenceToChallenge(EvidenceSO presentedEvidence)
        {
            if (!isChallengeModeActive || currentNode == null || presentedEvidence == null) return;

            CaseSO activeCase = CaseManager.Instance?.activeCase;
            if (activeCase == null) return;

            Debug.Log($"[Interrogation] Presenting evidence '{presentedEvidence.evidenceName}' (ID: {presentedEvidence.id}) against statement node '{currentNode.nodeId}'");

            // Contradiction verification delegated to Service
            ContradictionRuleSO matchingRule = interrogationService.FindMatchingContradiction(
                activeCase,
                currentNode.nodeId,
                presentedEvidence.id
            );

            if (matchingRule != null)
            {
                // Contradiction successfully exposed
                Debug.Log($"[Interrogation] Contradiction exposed! Rule: '{matchingRule.ruleTitle}' (Reaction: {matchingRule.reactionExpression})");
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
                CharacterExpression failExpression = interrogationService.GetFailureExpression(currentSuspect);
                string responseText = interrogationService.GetFailureResponseText(currentSuspect, presentedEvidence);

                Debug.Log($"[Interrogation] Challenge failed. Suspect reaction expression: {failExpression}. Response: \"{responseText}\"");

                OnExpressionChanged?.Invoke(failExpression);
                OnChallengeResult?.Invoke(false, responseText);
            }
        }
    }
}
