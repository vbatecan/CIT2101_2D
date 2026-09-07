using CaseClosed.Data;

namespace CaseClosed.Services
{
    /// <summary>
    /// Owns the mutable flow state of one interrogation. Dialogue trees and profiles remain
    /// authored data; this type records only the player's current position and challenge state.
    /// </summary>
    public sealed class InterrogationSessionState
    {
        public CharacterProfileSO CurrentSuspect { get; private set; }
        public DialogueTreeSO CurrentDialogueTree { get; private set; }
        public DialogueNode CurrentNode { get; private set; }
        public DialogueNode LastChallengeableNode { get; private set; }
        public bool IsChallengeModeActive { get; private set; }
        public bool IsShowingFailureReaction { get; private set; }

        public void Begin(CharacterProfileSO suspect, DialogueTreeSO dialogueTree)
        {
            CurrentSuspect = suspect;
            CurrentDialogueTree = dialogueTree;
            CurrentNode = null;
            LastChallengeableNode = null;
            IsChallengeModeActive = false;
            IsShowingFailureReaction = false;
        }

        public void SetCurrentNode(DialogueNode node)
        {
            CurrentNode = node;
        }

        public void SetChallengeMode(bool enabled)
        {
            IsChallengeModeActive = enabled;
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
            CurrentNode = null;
            LastChallengeableNode = null;
            IsChallengeModeActive = false;
            IsShowingFailureReaction = false;
        }
    }
}
