using System.Collections.Generic;
using CaseClosed.Data;

namespace CaseClosed.Services
{
    /// <summary>
    /// Owns the mutable flow state of one interrogation. Dialogue trees and profiles remain
    /// authored data; this type records only the player's current position and challenge state.
    /// </summary>
    public sealed class InterrogationSessionState
    {
        private readonly HashSet<string> displayedNodeIds = new HashSet<string>();
        private string pendingInspectionNodeId;
        private EvidenceSO pendingInspectionEvidence;

        public CharacterProfileSO CurrentSuspect { get; private set; }
        public DialogueTreeSO CurrentDialogueTree { get; private set; }
        public DialogueNode CurrentNode { get; private set; }
        public DialogueNode LastChallengeableNode { get; private set; }
        public bool IsChallengeModeActive { get; private set; }
        public bool IsShowingFailureReaction { get; private set; }

        public void Begin(CharacterProfileSO suspect, DialogueTreeSO dialogueTree)
        {
            displayedNodeIds.Clear();
            pendingInspectionNodeId = null;
            pendingInspectionEvidence = null;
            CurrentSuspect = suspect;
            CurrentDialogueTree = dialogueTree;
            CurrentNode = null;
            LastChallengeableNode = null;
            IsChallengeModeActive = false;
            IsShowingFailureReaction = false;
        }

        public void SetCurrentNode(DialogueNode node)
        {
            pendingInspectionEvidence = null;
            CurrentNode = node;
            if (node != null) displayedNodeIds.Add(node.nodeId);
        }

        public void QueueInspectionDialogue(string nodeId)
        {
            pendingInspectionNodeId = !string.IsNullOrEmpty(nodeId) && !displayedNodeIds.Contains(nodeId)
                ? nodeId
                : null;
        }

        public string TakeInspectionDialogue()
        {
            string nodeId = pendingInspectionNodeId;
            pendingInspectionNodeId = null;
            return nodeId != null && !displayedNodeIds.Contains(nodeId) ? nodeId : null;
        }

        public void SetChallengeMode(bool enabled)
        {
            IsChallengeModeActive = enabled;
            if (!enabled) pendingInspectionEvidence = null;
        }

        public void QueueInspectionEvidence(EvidenceSO evidence)
        {
            pendingInspectionEvidence = IsChallengeModeActive ? evidence : null;
        }

        public EvidenceSO TakeInspectionEvidence()
        {
            EvidenceSO evidence = pendingInspectionEvidence;
            pendingInspectionEvidence = null;
            return IsChallengeModeActive ? evidence : null;
        }

        public void RecordFailedChallenge()
        {
            LastChallengeableNode = CurrentNode;
            IsShowingFailureReaction = true;
            IsChallengeModeActive = false;
        }

        public void RecordSuccessfulChallenge()
        {
            LastChallengeableNode = null;
            IsShowingFailureReaction = false;
            IsChallengeModeActive = false;
        }

        public bool TryResumeAfterFailure(out DialogueNode challengeableNode)
        {
            challengeableNode = null;
            if (!IsShowingFailureReaction) return false;

            IsShowingFailureReaction = false;
            challengeableNode = LastChallengeableNode;
            return challengeableNode != null;
        }

        public void Close()
        {
            pendingInspectionEvidence = null;
            CurrentNode = null;
            LastChallengeableNode = null;
            IsChallengeModeActive = false;
            IsShowingFailureReaction = false;
        }
    }
}
