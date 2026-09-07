using NUnit.Framework;
using CaseClosed.Data;
using CaseClosed.Services;

namespace CaseClosed.Tests
{
    public class InterrogationSessionStateTests
    {
        [Test]
        public void FailedChallenge_PreservesNodeForOneLoopback_AndDisablesChallengeMode()
        {
            InterrogationSessionState state = new InterrogationSessionState();
            DialogueNode node = new DialogueNode { nodeId = "NODE_CHALLENGE" };

            state.Begin(null, null);
            state.SetCurrentNode(node);
            state.SetChallengeMode(true);
            state.RecordFailedChallenge();

            Assert.IsTrue(state.IsShowingFailureReaction);
            Assert.IsFalse(state.IsChallengeModeActive);
            Assert.AreSame(node, state.LastChallengeableNode);
            Assert.IsTrue(state.TryResumeAfterFailure(out DialogueNode loopbackNode));
            Assert.AreSame(node, loopbackNode);
            Assert.IsFalse(state.IsShowingFailureReaction);
            Assert.IsFalse(state.TryResumeAfterFailure(out _));
        }

        [Test]
        public void SuccessfulChallengeAndClose_ClearTransientChallengeState()
        {
            InterrogationSessionState state = new InterrogationSessionState();
            DialogueNode node = new DialogueNode { nodeId = "NODE_CHALLENGE" };

            state.Begin(null, null);
            state.SetCurrentNode(node);
            state.RecordFailedChallenge();
            state.RecordSuccessfulChallenge();

            Assert.IsFalse(state.IsShowingFailureReaction);
            Assert.IsFalse(state.IsChallengeModeActive);
            Assert.IsNull(state.LastChallengeableNode);

            state.SetChallengeMode(true);
            state.Close();

            Assert.IsNull(state.CurrentNode);
            Assert.IsFalse(state.IsChallengeModeActive);
            Assert.IsFalse(state.IsShowingFailureReaction);
        }
    }
}
