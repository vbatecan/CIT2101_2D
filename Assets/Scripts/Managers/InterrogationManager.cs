using System;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Services;

namespace CaseClosed.Managers
{
    public class InterrogationManager : MonoBehaviour
    {
        private static InterrogationManager _instance;

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

        public CharacterProfileSO CurrentSuspect => hasStartedSession ? sessionState.CurrentSuspect : currentSuspect;

        public DialogueTreeSO CurrentDialogueTree => hasStartedSession ? sessionState.CurrentDialogueTree : currentDialogueTree;

        public DialogueNode CurrentNode => hasStartedSession ? sessionState.CurrentNode : currentNode;

        public bool IsChallengeModeActive => hasStartedSession ? sessionState.IsChallengeModeActive : isChallengeModeActive;

        public bool IsShowingFailureReaction => hasStartedSession ? sessionState.IsShowingFailureReaction : isShowingFailureReaction;

        public DialogueNode LastChallengeableNode => hasStartedSession ? sessionState.LastChallengeableNode : null;

        public event Action<CharacterProfileSO> OnSuspectChanged;

        public event Action<DialogueNode> OnDialogueNodeDisplayed;

        public event Action<CharacterExpression> OnExpressionChanged;

        public event Action<bool> OnChallengeModeToggled;

        public event Action<bool, string> OnChallengeResult;

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

        public void QueueInspectionDialogue(string nodeId)
        {
            EnsureSessionState();
            if (!string.IsNullOrEmpty(nodeId) && CurrentDialogueTree != null && CurrentDialogueTree.GetNodeById(nodeId) == null)
            {
                Debug.LogWarning($"[Interrogation] Cannot queue inspection dialogue: node '{nodeId}' not found in current tree '{CurrentDialogueTree.treeId}'");
                return;
            }
            sessionState.QueueInspectionDialogue(nodeId);
        }

        public void QueueInspectionEvidence(EvidenceSO evidence)
        {
            EnsureSessionState();
            sessionState.QueueInspectionEvidence(evidence);
        }

        public void PlayPendingInspectionDialogue()
        {
            EnsureSessionState();
            string nodeId = sessionState.TakeInspectionDialogue();
            EvidenceSO evidence = sessionState.TakeInspectionEvidence();
            if (evidence != null)
            {
                PresentEvidenceToChallenge(evidence, true);
                return;
            }
            if (!string.IsNullOrEmpty(nodeId)) JumpToNode(nodeId);
        }

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

        private static void CompleteCurrentNode(DialogueNode node)
        {
            if (node == null || CaseManager.Instance == null || node.unlockEvidenceOnComplete == null) return;

            foreach (string evidenceId in node.unlockEvidenceOnComplete)
            {
                CaseManager.Instance.UnlockEvidence(evidenceId);
            }
        }

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

        public void PresentEvidenceToChallenge(EvidenceSO presentedEvidence)
        {
            PresentEvidenceToChallenge(presentedEvidence, false);
        }

        private void PresentEvidenceToChallenge(EvidenceSO presentedEvidence, bool inspectionCompleted)
        {
            EnsureSessionState();
            DialogueNode node = CurrentNode;
            if (node == null || !node.isChallengeable || presentedEvidence == null) return;

            CaseManager caseManager = CaseManager.Instance;
            CaseSO activeCase = caseManager != null ? caseManager.ActiveCase : null;
            if (activeCase == null) return;

            EvidenceManager evidenceManager = EvidenceManager.Instance;
            if (evidenceManager != null && (evidenceManager.isInspectingModalOpen ||
                (!inspectionCompleted && evidenceManager.LastInspectionClosedFrame == Time.frameCount))) return;

            if (!caseManager.IsEvidenceExamined(presentedEvidence))
            {
                if (evidenceManager != null) evidenceManager.OpenInspectModal(presentedEvidence, null);
                return;
            }

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
