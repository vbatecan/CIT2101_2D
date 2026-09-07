using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CaseClosed.Data;
using CaseClosed.Managers;
using CaseClosed.UI;

namespace CaseClosed.Tests
{
    [TestFixture]
    public class CharacterDialogueBoxTests
    {
        private GameObject _testRoot;
        private DialogueUI _dialogueUI;
        private GameObject _vinceBox;
        private GameObject _janeBox;
        private GameObject _paulBox;
        private GameObject _vonnBox;
        private GameObject _shanBox;
        private GameObject _shaniaBox;
        private GameObject _detectiveBox;

        private TextMeshProUGUI _vinceTmp;
        private TextMeshProUGUI _janeTmp;

        [SetUp]
        public void SetUp()
        {
            _testRoot = new GameObject("TestRoot");

            GameObject uiObj = new GameObject("DialogueUI");
            uiObj.transform.SetParent(_testRoot.transform);
            _dialogueUI = uiObj.AddComponent<DialogueUI>();

            // Setup mock button references
            GameObject nextObj = new GameObject("NextButton", typeof(Button));
            nextObj.transform.SetParent(uiObj.transform);
            _dialogueUI.nextButton = nextObj.GetComponent<Button>();

            GameObject challengeObj = new GameObject("ChallengeButton", typeof(Button));
            challengeObj.transform.SetParent(uiObj.transform);
            _dialogueUI.challengeButton = challengeObj.GetComponent<Button>();

            // Setup mock character boxes
            _vinceBox = CreateMockDialogBox("VinceDialog", out _vinceTmp);
            _janeBox = CreateMockDialogBox("JaneDialog", out _janeTmp);
            _paulBox = CreateMockDialogBox("PaulDialog", out _);
            _vonnBox = CreateMockDialogBox("VonnDialog", out _);
            _shanBox = CreateMockDialogBox("ShanDialog", out _);
            _shaniaBox = CreateMockDialogBox("ShaniaDialog", out _);
            _detectiveBox = CreateMockDialogBox("DetectiveDialogMessage", out _);

            _dialogueUI.vinceDialogBox = _vinceBox;
            _dialogueUI.janeDialogBox = _janeBox;
            _dialogueUI.paulDialogBox = _paulBox;
            _dialogueUI.vonnDialogBox = _vonnBox;
            _dialogueUI.shanDialogBox = _shanBox;
            _dialogueUI.shaniaDialogBox = _shaniaBox;
            _dialogueUI.detectiveDialogBox = _detectiveBox;
        }

        [TearDown]
        public void TearDown()
        {
            if (_testRoot != null)
            {
                Object.DestroyImmediate(_testRoot);
            }
        }

        private GameObject CreateMockDialogBox(string name, out TextMeshProUGUI tmp)
        {
            GameObject box = new GameObject(name);
            box.transform.SetParent(_testRoot.transform);

            GameObject msgChild = new GameObject("DialogMessage", typeof(RectTransform), typeof(CanvasRenderer));
            msgChild.transform.SetParent(box.transform);
            tmp = msgChild.AddComponent<TextMeshProUGUI>();

            box.SetActive(false);
            return box;
        }

        [Test]
        public void DialogueUI_CharacterKeyResolution_MatchesAllCharactersCorrectly()
        {
            Assert.AreEqual("Vince", _dialogueUI.ResolveCharacterKey("CHAR_VINCE_BATECAN", "Vince Angelo Batecan", false));
            Assert.AreEqual("Jane", _dialogueUI.ResolveCharacterKey("CHAR_CASE1_FEMALE", "Jane Reyes", false));
            Assert.AreEqual("Paul", _dialogueUI.ResolveCharacterKey("CHAR_PAUL_CAMACHO", "Paul Gabriel Camacho", false));
            Assert.AreEqual("Vonn", _dialogueUI.ResolveCharacterKey("CHAR_CHARL_PASCUAL", "Charl Vonn Pascual", false));
            Assert.AreEqual("Shan", _dialogueUI.ResolveCharacterKey("CHAR_SHAN", "Shan Jaraba", false));
            Assert.AreEqual("Shania", _dialogueUI.ResolveCharacterKey("CHAR_SHANIA", "Shania", false));
            Assert.AreEqual("Detective", _dialogueUI.ResolveCharacterKey("", "Detective", true));
            Assert.AreEqual("Detective", _dialogueUI.ResolveCharacterKey("PLAYER", "", false));
        }

        [Test]
        public void DialogueUI_CharacterKeyResolution_ShaniaDoesNotMatchShan()
        {
            // Verify that Shania is prioritized and never confused with Shan prefix
            string resolvedKey = _dialogueUI.ResolveCharacterKey("CHAR_SHANIA", "Shania", false);
            Assert.AreEqual("Shania", resolvedKey);
            Assert.AreNotEqual("Shan", resolvedKey);
        }

        [Test]
        public void DialogueUI_DisplayNode_ActivatesCorrespondingBox_AndDeactivatesOthers()
        {
            DialogueNode vinceNode = new DialogueNode
            {
                nodeId = "V_01",
                speakerId = "CHAR_VINCE_BATECAN",
                speakerName = "Vince Angelo Batecan",
                statementText = "I stayed in the kitchen!"
            };

            _dialogueUI.DisplayNode(vinceNode);
            _dialogueUI.CompleteTypingImmediately();

            Assert.IsTrue(_vinceBox.activeSelf, "VinceDialog must be active when Vince speaks");
            Assert.IsFalse(_janeBox.activeSelf, "JaneDialog must be inactive when Vince speaks");
            Assert.AreEqual("I stayed in the kitchen!", _vinceTmp.text);

            // Now display Jane statement
            DialogueNode janeNode = new DialogueNode
            {
                nodeId = "J_01",
                speakerId = "CHAR_CASE1_FEMALE",
                speakerName = "Jane Reyes",
                statementText = "That is not true, Vince."
            };

            _dialogueUI.DisplayNode(janeNode);
            _dialogueUI.CompleteTypingImmediately();

            Assert.IsFalse(_vinceBox.activeSelf, "VinceDialog must be deactivated when Jane speaks");
            Assert.IsTrue(_janeBox.activeSelf, "JaneDialog must be activated when Jane speaks");
            Assert.AreEqual("That is not true, Vince.", _janeTmp.text);
        }

        [Test]
        public void DialogueUI_DisplayNode_DetectiveActivatesDetectiveBox()
        {
            DialogueNode detNode = new DialogueNode
            {
                nodeId = "D_01",
                speakerId = "PLAYER",
                speakerName = "Detective",
                statementText = "Explain your presence outside the study."
            };

            _dialogueUI.DisplayNode(detNode);
            _dialogueUI.CompleteTypingImmediately();

            Assert.IsTrue(_detectiveBox.activeSelf, "DetectiveDialogMessage must be active when Detective speaks");
            Assert.IsFalse(_vinceBox.activeSelf, "VinceDialog must be inactive when Detective speaks");
            Assert.IsFalse(_janeBox.activeSelf, "JaneDialog must be inactive when Detective speaks");
        }

        [Test]
        public void DialogueUI_SupportsTextMeshProUGUI_ForDialogueAndSpeaker()
        {
            GameObject tmpBodyObj = new GameObject("TMP_Body", typeof(RectTransform), typeof(CanvasRenderer));
            tmpBodyObj.transform.SetParent(_dialogueUI.transform);
            TextMeshProUGUI tmpBody = tmpBodyObj.AddComponent<TextMeshProUGUI>();
            _dialogueUI.tmpDialogueText = tmpBody;

            GameObject tmpSpeakerObj = new GameObject("TMP_Speaker", typeof(RectTransform), typeof(CanvasRenderer));
            tmpSpeakerObj.transform.SetParent(_dialogueUI.transform);
            TextMeshProUGUI tmpSpeaker = tmpSpeakerObj.AddComponent<TextMeshProUGUI>();
            _dialogueUI.tmpSpeakerNameText = tmpSpeaker;

            DialogueNode node = new DialogueNode
            {
                nodeId = "TEST_TMP",
                speakerId = "CHAR_VINCE",
                speakerName = "Vince Batecan",
                statementText = "Testing TextMeshPro integration."
            };

            _dialogueUI.DisplayNode(node);
            _dialogueUI.CompleteTypingImmediately();

            Assert.AreEqual("Vince Batecan", tmpSpeaker.text);
            Assert.AreEqual("Testing TextMeshPro integration.", tmpBody.text);
        }

        [Test]
        public void DialogueUI_HandleDialogueAdvanceInput_CompletesTypingIfActive()
        {
            DialogueNode node = new DialogueNode
            {
                nodeId = "TYPING_TEST",
                speakerId = "CHAR_VINCE",
                speakerName = "Vince",
                statementText = "A long animated statement for typing test."
            };

            _dialogueUI.DisplayNode(node);
            Assert.IsTrue(_dialogueUI.IsTyping, "Must be typing initially");

            _dialogueUI.HandleDialogueAdvanceInput();

            Assert.IsFalse(_dialogueUI.IsTyping, "HandleDialogueAdvanceInput must complete typing immediately if typing was in progress");
            Assert.AreEqual("A long animated statement for typing test.", _vinceTmp.text);
        }

        [Test]
        public void DialogueUI_HideDialoguePanel_DeactivatesAllCharacterBoxes()
        {
            DialogueNode vinceNode = new DialogueNode
            {
                nodeId = "V_02",
                speakerId = "CHAR_VINCE",
                speakerName = "Vince",
                statementText = "Statement."
            };

            _dialogueUI.DisplayNode(vinceNode);
            Assert.IsTrue(_vinceBox.activeSelf);
            Assert.IsTrue(DialogueUI.IsDialogueOpen);

            _dialogueUI.HideDialoguePanel();

            Assert.IsFalse(_vinceBox.activeSelf, "VinceDialog must be deactivated after HideDialoguePanel");
            Assert.IsFalse(_janeBox.activeSelf, "JaneDialog must be deactivated after HideDialoguePanel");
            Assert.IsFalse(_detectiveBox.activeSelf, "DetectiveDialogMessage must be deactivated after HideDialoguePanel");
            Assert.IsFalse(DialogueUI.IsDialogueOpen, "IsDialogueOpen must be false after HideDialoguePanel");
        }
    }
}
