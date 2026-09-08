using System;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Services;

namespace CaseClosed.Managers
{
    /// <summary>
    /// Unity-facing adapter for interrogation flow. Inspector defaults remain serialized on this
    /// component while <see cref="InterrogationSessionState"/> owns mutable conversation state.
    /// </summary>
    public class InterrogationManager : MonoBehaviour
    {
        private static InterrogationManager _instance;

        /// <summary>Singleton instance of the InterrogationManager.</summary>
        public static InterrogationManager Instance
        {
            get
            {
                if (_instance == null) _instance = FindFirstObjectByType<InterrogationManager>(FindObjectsInactive.Include);
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Initial Interrogation Defaults")]
        [Tooltip("Optional Inspector default used to start an interrogation.")]
        [SerializeField] private CharacterProfileSO currentSuspect;
        [Tooltip("Optional Inspector default used to start an interrogation.")]
        [SerializeField] private DialogueTreeSO currentDialogueTree;
        [Tooltip("Optional Inspector node used only when restoring a preconfigured interrogation.")]
        [SerializeField] private DialogueNode currentNode;
        [SerializeField] private bool isChallengeModeActive;
        [SerializeField] private bool isShowingFailureReaction;

        private readonly InterrogationSessionState sessionState = new InterrogationSessionState();
        private readonly InterrogationService interrogationService = new InterrogationService();
        private bool hasStartedSession;

        /// <summary>The current suspect or witness being interrogated.</summary>
        public CharacterProfileSO CurrentSuspect => hasStartedSession ? sessionState.CurrentSuspect : currentSuspect;

        /// <summary>The active dialogue tree.</summary>
        public DialogueTreeSO CurrentDialogueTree => hasStartedSession ? sessionState.CurrentDialogueTree : currentDialogueTree;

        /// <summary>The current active dialogue statement node.</summary>
        public DialogueNode CurrentNode => hasStartedSession ? sessionState.CurrentNode : currentNode;

        /// <summary>Whether challenge mode is active and awaiting evidence presentation.</summary>
        public bool IsChallengeModeActive => hasStartedSession ? sessionState.IsChallengeModeActive : isChallengeModeActive;

        /// <summary>Whether the failure reaction is currently being displayed.</summary>
        public bool IsShowingFailureReaction => hasStartedSession ? sessionState.IsShowingFailureReaction : isShowingFailureReaction;

        /// <summary>The last challengeable statement node before a failed challenge.</summary>
        public DialogueNode LastChallengeableNode => hasStartedSession ? sessionState.LastChallengeableNode : null;

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

        private void Awake()
        {
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>Starts from optional Inspector defaults when both a suspect and tree are configured.</summary>
        private void Start()
        {
            if (!hasStartedSession && currentSuspect != null && currentDialogueTree != null && currentNode == null)
            {
                SetInterrogationTarget(currentSuspect, currentDialogueTree);
            }
            else if (!hasStartedSession && currentDialogueTree != null)
            {
                EnsureSessionState();
            }
        }

        /// <summary>Sets the active suspect and dialogue tree for a new interrogation session.</summary>
        public void SetInterrogationTarget(CharacterProfileSO suspect, DialogueTreeSO dialogueTree)
        {
            sessionState.Begin(suspect, dialogueTree);
            hasStartedSession = true;

            Debug.Log($"[Interrogation] Set interrogation target: '{suspect?.fullName}' (Tree: '{dialogueTree?.treeId}')");
            OnSuspectChanged?.Invoke(CurrentSuspect);

            if (CurrentDialogueTree != null && !string.IsNullOrEmpty(CurrentDialogueTree.startNodeId))
            {
                JumpToNode(CurrentDialogueTree.startNodeId);
            }
        }

        /// <summary>Navigates to a specific dialogue node by its identifier.</summary>
        public void JumpToNode(string nodeId)
        {
            DialogueTreeSO dialogueTree = CurrentDialogueTree;
            if (dialogueTree == null)
            {
                Debug.LogWarning("[Interrogation] Cannot jump to node: currentDialogueTree is null");
                return;
            }

            DialogueNode targetNode = dialogueTree.GetNodeById(nodeId);
            if (targetNode == null)
            {
                Debug.LogWarning($"[Interrogation] Dialogue node '{nodeId}' not found in tree '{dialogueTree.treeId}'");
                return;
            }

            EnsureSessionState();
            sessionState.SetCurrentNode(targetNode);
            Debug.Log($"[Interrogation] Jumped to node '{nodeId}' (Speaker: '{targetNode.speakerName}', Expr: {targetNode.expression}, Challengeable: {targetNode.isChallengeable})");
            OnDialogueNodeDisplayed?.Invoke(CurrentNode);
            OnExpressionChanged?.Invoke(CurrentNode.expression);
        }

        /// <summary>Advances the dialogue to its default next node when no challenge is active.</summary>
        public void AdvanceDialogue()
        {
            EnsureSessionState();
            DialogueNode node = CurrentNode;
            if (node == null || IsChallengeModeActive) return;

            if (IsShowingFailureReaction && sessionState.TryResumeAfterFailure(out DialogueNode challengeableNode))
            {
                Debug.Log($"[Interrogation] Looping back to challengeable node '{challengeableNode.nodeId}' after failure reaction.");
                JumpToNode(challengeableNode.nodeId);
                return;
            }

            CompleteCurrentNode(node);

            if (node.choices != null && node.choices.Count > 0)
            {
                return;
            }

            if (!string.IsNullOrEmpty(node.defaultNextNodeId))
            {
                Debug.Log($"[Interrogation] Advancing dialogue from '{node.nodeId}' to default next '{node.defaultNextNodeId}'");
                JumpToNode(node.defaultNextNodeId);
            }
            else
            {
                Debug.Log($"[Interrogation] Reached end of dialogue branch for node '{node.nodeId}'. Closing dialogue.");
                if (CurrentDialogueTree != null)
                {
                    CaseManager.Instance?.RecordDialogueCompleted(CurrentDialogueTree.treeId);
                }
                CloseDialogue();
            }
        }

        /// <summary>Applies story evidence rewards after the player finishes a dialogue node.</summary>
        private static void CompleteCurrentNode(DialogueNode node)
        {
            if (node == null || CaseManager.Instance == null || node.unlockEvidenceOnComplete == null) return;

            foreach (string evidenceId in node.unlockEvidenceOnComplete)
            {
                CaseManager.Instance.UnlockEvidence(evidenceId);
            }
        }

        /// <summary>Closes the active dialogue window and resets transient challenge state.</summary>
        public void CloseDialogue()
        {
            EnsureSessionState();
            DialogueNode node = CurrentNode;
            if (node != null && CurrentDialogueTree != null && string.IsNullOrEmpty(node.defaultNextNodeId) && (node.choices == null || node.choices.Count == 0))
            {
                CaseManager.Instance?.RecordDialogueCompleted(CurrentDialogueTree.treeId);
            }
            sessionState.Close();
            OnChallengeModeToggled?.Invoke(false);
            OnDialogueClosed?.Invoke();
        }

        /// <summary>Toggles challenge mode on the current node if that node is challengeable.</summary>
        public void ToggleChallengeMode(bool enable)
        {
            DialogueNode node = CurrentNode;
            if (node == null || !node.isChallengeable)
            {
                Debug.Log($"[Interrogation] Current node '{(node != null ? node.nodeId : "NULL")}' is not challengeable.");
                OnChallengeModeToggled?.Invoke(false);
                return;
            }

            EnsureSessionState();
            sessionState.SetChallengeMode(enable);
            Debug.Log($"[Interrogation] Challenge mode toggled: {IsChallengeModeActive} for node '{node.nodeId}'");
            OnChallengeModeToggled?.Invoke(IsChallengeModeActive);
        }

        /// <summary>Evaluates evidence presented against the current challengeable statement.</summary>
        public void PresentEvidenceToChallenge(EvidenceSO presentedEvidence)
        {
            EnsureSessionState();
            DialogueNode node = CurrentNode;
            if (node == null || presentedEvidence == null) return;

            CaseSO activeCase = CaseManager.Instance?.ActiveCase;
            if (activeCase == null) return;

            Debug.Log($"[Interrogation] Presenting evidence '{presentedEvidence.evidenceName}' (ID: {presentedEvidence.id}) against statement node '{node.nodeId}'");

            ContradictionRuleSO matchingRule = interrogationService.FindMatchingContradiction(
                activeCase,
                node.nodeId,
                presentedEvidence.id);

            if (matchingRule != null)
            {
                Debug.Log($"[Interrogation] Contradiction exposed! Rule: '{matchingRule.ruleTitle}' (Reaction: {matchingRule.reactionExpression})");
                CaseManager.Instance?.RegisterContradictionExposed(matchingRule);
                OnExpressionChanged?.Invoke(matchingRule.reactionExpression);
                OnChallengeResult?.Invoke(true, matchingRule.reactionDialogue);

                sessionState.RecordSuccessfulChallenge();
                OnChallengeModeToggled?.Invoke(false);

                if (!string.IsNullOrEmpty(matchingRule.unlockedDialogueNodeId))
                {
                    JumpToNode(matchingRule.unlockedDialogueNodeId);
                }
            }
            else
            {
                sessionState.RecordFailedChallenge();
                OnChallengeModeToggled?.Invoke(false);

                CharacterExpression failExpression = interrogationService.GetFailureExpression(CurrentSuspect);
                string responseText = interrogationService.GetFailureResponseText(CurrentSuspect, presentedEvidence);

                Debug.Log($"[Interrogation] Challenge failed. Suspect reaction expression: {failExpression}. Response: \"{responseText}\"");
                OnExpressionChanged?.Invoke(failExpression);
                OnChallengeResult?.Invoke(false, responseText);
            }
        }

        private void EnsureSessionState()
        {
            if (hasStartedSession) return;

            sessionState.Begin(currentSuspect, currentDialogueTree);
            sessionState.SetCurrentNode(currentNode);
            sessionState.SetChallengeMode(isChallengeModeActive);
            if (isShowingFailureReaction)
            {
                sessionState.RecordFailedChallenge();
            }
            hasStartedSession = true;
        }
    }
}
