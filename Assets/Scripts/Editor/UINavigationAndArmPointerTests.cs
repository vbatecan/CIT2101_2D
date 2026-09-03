using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Gameplay;
using CaseClosed.Managers;
using CaseClosed.Services;
using CaseClosed.UI;

namespace CaseClosed.Tests
{
    [TestFixture]
    public class UINavigationAndArmPointerTests
    {
        private GameObject testRoot;
        private UIManager uiManager;
        private ArmPointerController armController;
        private EvidenceManager evidenceManager;
        private CaseManager caseManager;
        private CaseConclusionManager conclusionManager;
        private DeductionBoardController deductionBoardController;

        [SetUp]
        public void SetUp()
        {
            testRoot = new GameObject("Test_UINavigationRoot");

            // Setup Managers
            caseManager = testRoot.AddComponent<CaseManager>();
            evidenceManager = testRoot.AddComponent<EvidenceManager>();
            conclusionManager = testRoot.AddComponent<CaseConclusionManager>();
            deductionBoardController = testRoot.AddComponent<DeductionBoardController>();
            uiManager = testRoot.AddComponent<UIManager>();

            // Setup Panels
            uiManager.mainTablePanel = new GameObject("Test_MainTablePanel");
            uiManager.mainTablePanel.transform.SetParent(testRoot.transform);

            uiManager.inspectModalPanel = new GameObject("Test_InspectModalPanel");
            uiManager.inspectModalPanel.transform.SetParent(testRoot.transform);

            uiManager.notebookPanel = new GameObject("Test_NotebookPanel");
            uiManager.notebookPanel.transform.SetParent(testRoot.transform);

            uiManager.deductionBoardPanel = new GameObject("Test_DeductionBoardPanel");
            uiManager.deductionBoardPanel.transform.SetParent(testRoot.transform);

            uiManager.conclusionQuizPanel = new GameObject("Test_ConclusionQuizPanel");
            uiManager.conclusionQuizPanel.transform.SetParent(testRoot.transform);

            // Setup Header Buttons
            GameObject nbBtn = new GameObject("Test_NotebookBtn", typeof(Button));
            nbBtn.transform.SetParent(testRoot.transform);
            uiManager.notebookButton = nbBtn;

            GameObject dbBtn = new GameObject("Test_DeductionBoardBtn", typeof(Button));
            dbBtn.transform.SetParent(testRoot.transform);
            uiManager.deductionBoardButton = dbBtn;

            GameObject ccBtn = new GameObject("Test_ConcludeCaseBtn", typeof(Button));
            ccBtn.transform.SetParent(testRoot.transform);
            uiManager.concludeCaseButton = ccBtn;

            // Setup ArmPointerController
            GameObject armObj = new GameObject("Test_ArmPointer");
            armObj.transform.SetParent(testRoot.transform);
            armController = armObj.AddComponent<ArmPointerController>();
            armObj.AddComponent<SpriteRenderer>();

            GameObject fp = new GameObject("Fingertip_Point");
            fp.transform.SetParent(armObj.transform);
            armController.fingertipPoint = fp.transform;

            // Route singletons to test fixtures
            typeof(UIManager).GetProperty("Instance")?.SetValue(null, uiManager);
            typeof(EvidenceManager).GetProperty("Instance")?.SetValue(null, evidenceManager);
            typeof(ArmPointerController).GetProperty("Instance")?.SetValue(null, armController);
        }

        [TearDown]
        public void TearDown()
        {
            typeof(UIManager).GetProperty("Instance")?.SetValue(null, null);
            typeof(EvidenceManager).GetProperty("Instance")?.SetValue(null, null);
            typeof(ArmPointerController).GetProperty("Instance")?.SetValue(null, null);

            if (testRoot != null)
            {
                Object.DestroyImmediate(testRoot);
            }
        }

        [Test]
        public void ArmPointerController_DetermineUIMode_TrueWhenInModalOrInspection()
        {
            // When in Investigation Table mode and modal closed
            uiManager.ShowPanel(UIPanelType.InvestigationTable);
            evidenceManager.isInspectingModalOpen = false;
            Assert.IsFalse(armController.DetermineUIMode());

            // When in Notebook mode
            uiManager.ShowPanel(UIPanelType.CaseFileNotebook);
            Assert.IsTrue(armController.DetermineUIMode());

            // When in Deduction Board mode
            uiManager.ShowPanel(UIPanelType.DeductionBoard);
            Assert.IsTrue(armController.DetermineUIMode());

            // When in Conclusion Quiz mode
            uiManager.ShowPanel(UIPanelType.ConclusionQuiz);
            Assert.IsTrue(armController.DetermineUIMode());

            // When in Inspect Modal
            uiManager.ShowPanel(UIPanelType.InvestigationTable);
            evidenceManager.isInspectingModalOpen = true;
            Assert.IsTrue(armController.DetermineUIMode());
        }

        [Test]
        public void ArmPointerController_ForceSyncState_SetsArmActiveCorrectly()
        {
            uiManager.ShowPanel(UIPanelType.InvestigationTable);
            evidenceManager.isInspectingModalOpen = false;

            armController.ForceSyncState();
            Assert.IsTrue(armController.isArmActive);
            Assert.IsFalse(armController.isDialogueOrUIActive);

            // Switch to notebook
            uiManager.ShowPanel(UIPanelType.CaseFileNotebook);
            Assert.IsFalse(armController.isArmActive);
            Assert.IsTrue(armController.isDialogueOrUIActive);

            // Return to investigation table
            uiManager.ShowPanel(UIPanelType.InvestigationTable);
            Assert.IsTrue(armController.isArmActive);
            Assert.IsFalse(armController.isDialogueOrUIActive);
        }

        [Test]
        public void UIManager_TogglePanels_SwitchesPanelsCorrectly()
        {
            uiManager.ShowPanel(UIPanelType.InvestigationTable);
            Assert.AreEqual(UIPanelType.InvestigationTable, uiManager.currentPanel);

            // Toggle Notebook
            uiManager.ToggleNotebookPanel();
            Assert.AreEqual(UIPanelType.CaseFileNotebook, uiManager.currentPanel);
            uiManager.ToggleNotebookPanel();
            Assert.AreEqual(UIPanelType.InvestigationTable, uiManager.currentPanel);

            // Toggle Deduction Board
            uiManager.ToggleDeductionBoardPanel();
            Assert.AreEqual(UIPanelType.DeductionBoard, uiManager.currentPanel);
            uiManager.ToggleDeductionBoardPanel();
            Assert.AreEqual(UIPanelType.InvestigationTable, uiManager.currentPanel);

            // Open Conclusion Quiz
            caseManager.activeCase.totalContradictionsCount = 0;
            uiManager.OpenConclusionQuiz();
            Assert.AreEqual(UIPanelType.ConclusionQuiz, uiManager.currentPanel);
        }

        [Test]
        public void DeductionService_FindMatchingConnection_ValidatesPairings()
        {
            DeductionService service = new DeductionService();
            CaseSO caseSO = ScriptableObject.CreateInstance<CaseSO>();

            ClueConnectionSO conn = ScriptableObject.CreateInstance<ClueConnectionSO>();
            conn.connectionId = "CONN_TEST";
            conn.clueA_Id = "CLUE_A";
            conn.clueB_Id = "CLUE_B";
            conn.resultClueId = "CLUE_RESULT";
            conn.connectionTitle = "Test Connection";
            conn.deductionText = "A connected with B";
            caseSO.clueConnections.Add(conn);

            // In order
            Assert.IsNotNull(service.FindMatchingConnection(caseSO, "CLUE_A", "CLUE_B"));
            // Reversed order
            Assert.IsNotNull(service.FindMatchingConnection(caseSO, "CLUE_B", "CLUE_A"));
            // Invalid pairing
            Assert.IsNull(service.FindMatchingConnection(caseSO, "CLUE_A", "CLUE_INVALID"));

            Object.DestroyImmediate(caseSO);
            Object.DestroyImmediate(conn);
        }

        [Test]
        public void ArmPointerController_ResetTapState_RestoresCursorTrackingAfterDeactivation()
        {
            uiManager.ShowPanel(UIPanelType.InvestigationTable);
            armController.ForceSyncState();

            // Simulate clicking on an evidence item: tap starts and isTapping becomes true
            armController.isTapping = true;
            Assert.IsTrue(armController.isTapping);

            // Directly call ForceSyncState (as done by RestoreSceneObjects and CloseInspect upon reactivation)
            armController.ForceSyncState();

            // Tap state should be cleanly reset to allow cursor follow
            Assert.IsFalse(armController.isTapping, "isTapping must be false after ForceSyncState to allow cursor follow");
            Assert.IsTrue(armController.isArmActive, "isArmActive must be true after returning to InvestigationTable");
        }

        [Test]
        public void EvidenceInspectModal_CloseInspect_RestoresAndReactivatesArmPointer()
        {
            uiManager.ShowPanel(UIPanelType.InvestigationTable);
            evidenceManager.isInspectingModalOpen = false;

            // Create inspect modal on inspectModalPanel
            EvidenceInspectModal inspectModal = uiManager.inspectModalPanel.AddComponent<EvidenceInspectModal>();
            inspectModal.sceneObjectsToHide = new string[] { "Test_ArmPointer" };

            // Start in tapping state
            armController.isTapping = true;

            // Create test evidence
            EvidenceSO testEvidence = ScriptableObject.CreateInstance<EvidenceSO>();
            testEvidence.id = "EVD_TEST_CLOSE";
            testEvidence.evidenceName = "Test Broken Clock";

            // Enter inspection
            inspectModal.DisplayEvidence(testEvidence);
            Assert.IsFalse(armController.gameObject.activeSelf, "Arm should be hidden during evidence inspection");

            // Exit inspection
            inspectModal.CloseInspect();

            // Assert arm is restored, active, and tap state is reset
            Assert.IsTrue(armController.gameObject.activeSelf, "Arm GameObject must be reactivated after CloseInspect");
            Assert.IsFalse(armController.isTapping, "Arm isTapping must be reset so arm follows cursor");
            Assert.IsTrue(armController.isArmActive, "Arm must be marked active after CloseInspect");
            Assert.IsFalse(inspectModal.IsInspecting, "Inspect modal must be marked closed");

            Object.DestroyImmediate(testEvidence);
            Object.DestroyImmediate(inspectModal);
        }

        [Test]
        public void CaseEvaluationService_EvaluateCase_CalculatesScoreAndGrade()
        {
            CaseEvaluationService service = new CaseEvaluationService();
            CaseSO caseSO = ScriptableObject.CreateInstance<CaseSO>();
            caseSO.totalKeyEvidenceCount = 1;
            caseSO.totalContradictionsCount = 1;
            caseSO.parCompletionTimeSeconds = 300f;

            ConclusionQuestion q1 = new ConclusionQuestion
            {
                questionId = "Q1",
                questionText = "Question 1",
                options = new List<string> { "Correct", "Wrong" },
                correctOptionIndex = 0,
                pointValue = 1000
            };
            caseSO.conclusionQuestions.Add(q1);

            ContradictionRuleSO rule = ScriptableObject.CreateInstance<ContradictionRuleSO>();
            rule.ruleId = "RULE_1";
            caseSO.contradictionRules.Add(rule);

            EvidenceSO ev = ScriptableObject.CreateInstance<EvidenceSO>();
            ev.id = "EV_1";
            caseSO.evidenceItems.Add(ev);

            // 100% correct answers
            CaseEvaluationResult result = service.EvaluateCase(
                caseSO,
                new List<int> { 0 },
                evidenceFoundCount: 1,
                contradictionsCaughtCount: 1,
                elapsedTimeSeconds: 60f
            );

            Assert.IsNotNull(result);
            Assert.IsTrue(result.isCaseSolved);
            Assert.AreEqual(1, result.correctQuizAnswers);
            Assert.AreEqual("S", result.rankGrade);
            Assert.AreEqual(5, result.starCount);

            Object.DestroyImmediate(caseSO);
            Object.DestroyImmediate(rule);
            Object.DestroyImmediate(ev);
        }
    }
}
