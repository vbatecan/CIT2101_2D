using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;
using CaseClosed.UI;

namespace CaseClosed.Tests
{
    [TestFixture]
    public class InterrogationAndDialogueUITests
    {
        private GameObject _testRoot;
        private DialogueUI _dialogueUI;
        private InterrogationManager _interrogationManager;
        private CaseManager _caseManager;
        private CharacterProfileSO _suspectProfile;
        private CharacterProfileSO _investigatorProfile;
        private DialogueTreeSO _dialogueTree;
        private EvidenceSO _testEvidence;

        [SetUp]
        public void SetUp()
        {
            _testRoot = new GameObject("TestRoot_Interrogation");

            // Setup CaseManager
            GameObject caseMgrObj = new GameObject("CaseManager");
            caseMgrObj.transform.SetParent(_testRoot.transform);
            _caseManager = caseMgrObj.AddComponent<CaseManager>();

            _investigatorProfile = ScriptableObject.CreateInstance<CharacterProfileSO>();
            _investigatorProfile.characterId = "CHAR_VALENTINE";
            _investigatorProfile.fullName = "Detective Valentine";
            _caseManager.selectedInvestigator = _investigatorProfile;

            PropertyInfo caseManagerInst = typeof(CaseManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            caseManagerInst?.SetValue(null, _caseManager);

            // Setup InterrogationManager
            GameObject interrogationObj = new GameObject("InterrogationManager");
            interrogationObj.transform.SetParent(_testRoot.transform);
            _interrogationManager = interrogationObj.AddComponent<InterrogationManager>();

            PropertyInfo interrogationInst = typeof(InterrogationManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            interrogationInst?.SetValue(null, _interrogationManager);

            // Setup Suspect Profile
            _suspectProfile = ScriptableObject.CreateInstance<CharacterProfileSO>();
            _suspectProfile.characterId = "CHAR_SUSPECT";
            _suspectProfile.fullName = "Vincent Price";
            _suspectProfile.personalityTrait = PersonalityTrait.Defensive;

            // Setup Evidence
            _testEvidence = ScriptableObject.CreateInstance<EvidenceSO>();
            _testEvidence.id = "EVD_IRRELEVANT";
            _testEvidence.evidenceName = "Random Paper";

            // Setup CaseSO
            CaseSO testCase = ScriptableObject.CreateInstance<CaseSO>();
            testCase.caseTitle = "Test Case";
            testCase.contradictionRules = new List<ContradictionRuleSO>();
            _caseManager.activeCase = testCase;

            // Setup Dialogue Tree
            _dialogueTree = ScriptableObject.CreateInstance<DialogueTreeSO>();
            _dialogueTree.treeId = "TREE_TEST";
            _dialogueTree.characterId = _suspectProfile.characterId;
            _dialogueTree.startNodeId = "NODE_CHALLENGEABLE";

            DialogueNode challengeableNode = new DialogueNode
            {
                nodeId = "NODE_CHALLENGEABLE",
                speakerId = _suspectProfile.characterId,
                speakerName = _suspectProfile.fullName,
                statementText = "I was at the library all evening!",
                isChallengeable = true,
                defaultNextNodeId = "NODE_END"
            };

            DialogueNode endNode = new DialogueNode
            {
                nodeId = "NODE_END",
                speakerId = _suspectProfile.characterId,
                speakerName = _suspectProfile.fullName,
                statementText = "That is all I have to say.",
                isChallengeable = false
            };

            _dialogueTree.nodes.Add(challengeableNode);
            _dialogueTree.nodes.Add(endNode);

            // Setup DialogueUI
            GameObject uiObj = new GameObject("DialogueUI");
            uiObj.transform.SetParent(_testRoot.transform);
            _dialogueUI = uiObj.AddComponent<DialogueUI>();

            GameObject speakerObj = new GameObject("SpeakerText", typeof(RectTransform), typeof(Text));
            speakerObj.transform.SetParent(uiObj.transform);
            _dialogueUI.speakerNameText = speakerObj.GetComponent<Text>();

            GameObject bodyObj = new GameObject("BodyText", typeof(RectTransform), typeof(Text));
            bodyObj.transform.SetParent(uiObj.transform);
            _dialogueUI.dialogueBodyText = bodyObj.GetComponent<Text>();

            GameObject challengeBtnObj = new GameObject("ChallengeButton", typeof(RectTransform), typeof(Image), typeof(Button));
            challengeBtnObj.transform.SetParent(uiObj.transform);
            _dialogueUI.challengeButton = challengeBtnObj.GetComponent<Button>();

            GameObject nextBtnObj = new GameObject("NextButton", typeof(RectTransform), typeof(Image), typeof(Button));
            nextBtnObj.transform.SetParent(uiObj.transform);
            _dialogueUI.nextButton = nextBtnObj.GetComponent<Button>();

            PropertyInfo dialogueInst = typeof(DialogueUI).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            dialogueInst?.SetValue(null, _dialogueUI);
        }

        [TearDown]
        public void TearDown()
        {
            PropertyInfo caseManagerInst = typeof(CaseManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            caseManagerInst?.SetValue(null, null);

            PropertyInfo interrogationInst = typeof(InterrogationManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            interrogationInst?.SetValue(null, null);

            PropertyInfo dialogueInst = typeof(DialogueUI).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            dialogueInst?.SetValue(null, null);

            if (_testRoot != null) Object.DestroyImmediate(_testRoot);
            if (_suspectProfile != null) Object.DestroyImmediate(_suspectProfile);
            if (_investigatorProfile != null) Object.DestroyImmediate(_investigatorProfile);
            if (_testEvidence != null) Object.DestroyImmediate(_testEvidence);
            if (_dialogueTree != null) Object.DestroyImmediate(_dialogueTree);
        }

        [Test]
        public void DialogueUI_ChallengeButton_BecomesActive_WhenNodeIsChallengeableAndTypingFinishes()
        {
            DialogueNode challengeableNode = _dialogueTree.GetNodeById("NODE_CHALLENGEABLE");
            Assert.IsTrue(challengeableNode.isChallengeable);

            _dialogueUI.DisplayNode(challengeableNode);

            // Initially while typing, challenge button must be inactive
            Assert.IsFalse(_dialogueUI.challengeButton.gameObject.activeSelf, "Challenge button must be inactive while typing");

            // Complete typing
            _dialogueUI.CompleteTypingImmediately();

            // Once typing finishes, challenge button must become active
            Assert.IsTrue(_dialogueUI.challengeButton.gameObject.activeSelf, "Challenge button must become active once typing finishes");
            Assert.IsTrue(_dialogueUI.isCurrentNodeChallengeable);
        }

        [Test]
        public void DialogueUI_ChallengeButton_RemainsInactive_WhenNodeIsNotChallengeable()
        {
            DialogueNode regularNode = _dialogueTree.GetNodeById("NODE_END");
            Assert.IsFalse(regularNode.isChallengeable);

            _dialogueUI.DisplayNode(regularNode);
            _dialogueUI.CompleteTypingImmediately();

            Assert.IsFalse(_dialogueUI.challengeButton.gameObject.activeSelf, "Challenge button must stay inactive for non-challengeable nodes");
            Assert.IsFalse(_dialogueUI.isCurrentNodeChallengeable);
        }

        [Test]
        public void DialogueUI_ButtonlessChallengeableLine_EnablesChallengeModeWhenTypingCompletes()
        {
            _interrogationManager.SetInterrogationTarget(_suspectProfile, _dialogueTree);
            _dialogueUI.challengeButton = null;

            _dialogueUI.DisplayNode(_interrogationManager.currentNode);
            _dialogueUI.CompleteTypingImmediately();

            Assert.IsTrue(_interrogationManager.isChallengeModeActive,
                "Buttonless character dialogue must enable evidence selection after a challengeable line finishes.");
        }

        [Test]
        public void DialogueUI_DynamicDetectiveName_ResolvesSelectedInvestigator()
        {
            DialogueNode detectiveNode = new DialogueNode
            {
                nodeId = "NODE_DET",
                speakerId = "PLAYER",
                speakerName = "Detective",
                statementText = "Where were you at 9 PM?"
            };

            _dialogueUI.DisplayNode(detectiveNode);
            Assert.AreEqual("Detective Valentine", _dialogueUI.speakerNameText.text);

            // When selected investigator is null, fallback to Detective
            _caseManager.selectedInvestigator = null;
            _dialogueUI.DisplayNode(detectiveNode);
            Assert.AreEqual("Detective", _dialogueUI.speakerNameText.text);
        }

        [Test]
        public void DialogueUI_TracksChallengeFailureReaction()
        {
            _interrogationManager.currentSuspect = _suspectProfile;

            MethodInfo handleChallengeResultMethod = typeof(DialogueUI).GetMethod("HandleChallengeResult", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(handleChallengeResultMethod);

            // Simulate failed challenge response
            handleChallengeResultMethod.Invoke(_dialogueUI, new object[] { false, "That proves nothing!" });

            Assert.IsTrue(_dialogueUI.isShowingFailureReaction, "DialogueUI should track that failure reaction is active");
            Assert.IsFalse(_dialogueUI.challengeButton.gameObject.activeSelf, "Challenge button must be inactive during failure reaction");

            _dialogueUI.CompleteTypingImmediately();
            Assert.IsFalse(_dialogueUI.challengeButton.gameObject.activeSelf, "Challenge button must remain inactive even after typing failure reaction");

            // Displaying a new node clears the failure reaction state
            DialogueNode challengeableNode = _dialogueTree.GetNodeById("NODE_CHALLENGEABLE");
            _dialogueUI.DisplayNode(challengeableNode);
            Assert.IsFalse(_dialogueUI.isShowingFailureReaction, "Displaying a node should clear failure reaction flag");
        }

        [Test]
        public void InterrogationManager_FailedChallenge_SetsLoopbackState()
        {
            _interrogationManager.SetInterrogationTarget(_suspectProfile, _dialogueTree);

            Assert.AreEqual("NODE_CHALLENGEABLE", _interrogationManager.currentNode.nodeId);
            Assert.IsFalse(_interrogationManager.isShowingFailureReaction);

            // Present irrelevant evidence -> mismatch failure
            _interrogationManager.PresentEvidenceToChallenge(_testEvidence);

            Assert.IsTrue(_interrogationManager.isShowingFailureReaction, "isShowingFailureReaction must be true on challenge mismatch");
            Assert.IsNotNull(_interrogationManager.LastChallengeableNode, "_lastChallengeableNode must remember the statement node");
            Assert.AreEqual("NODE_CHALLENGEABLE", _interrogationManager.LastChallengeableNode.nodeId);
            Assert.IsFalse(_interrogationManager.isChallengeModeActive, "Challenge mode must be deactivated when evidence is presented");
        }

        [Test]
        public void InterrogationManager_AdvanceDialogue_LoopsBackToChallengeableNode_OnFailureReaction()
        {
            _interrogationManager.SetInterrogationTarget(_suspectProfile, _dialogueTree);

            // Fail challenge
            _interrogationManager.PresentEvidenceToChallenge(_testEvidence);
            Assert.IsTrue(_interrogationManager.isShowingFailureReaction);

            // Calling AdvanceDialogue while showing failure reaction should loop back to challengeable node
            _interrogationManager.AdvanceDialogue();

            Assert.IsFalse(_interrogationManager.isShowingFailureReaction, "AdvanceDialogue must clear isShowingFailureReaction");
            Assert.IsNotNull(_interrogationManager.currentNode, "currentNode must not be null");
            Assert.AreEqual("NODE_CHALLENGEABLE", _interrogationManager.currentNode.nodeId, "Must loop back to the challengeable statement node instead of advancing past it");
        }
    }
}
