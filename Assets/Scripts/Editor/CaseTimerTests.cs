using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;
using CaseClosed.Services;
using CaseClosed.UI;

namespace CaseClosed.Tests
{
    [TestFixture]
    public class CaseTimerTests
    {
        private CaseTimerService timerService;
        private GameObject testRoot;
        private CaseManager caseManager;

        [SetUp]
        public void SetUp()
        {
            timerService = new CaseTimerService();
            testRoot = new GameObject("Test_CaseTimerRoot");
            caseManager = testRoot.AddComponent<CaseManager>();
            typeof(CaseManager).GetProperty("Instance")?.SetValue(null, caseManager);
        }

        [TearDown]
        public void TearDown()
        {
            typeof(CaseManager).GetProperty("Instance")?.SetValue(null, null);

            if (testRoot != null)
            {
                Object.DestroyImmediate(testRoot);
            }
        }

        [Test]
        public void CaseTimerService_CalculatesRemainingTime_Correctly()
        {
            float limit = 300f; // 5 mins

            Assert.AreEqual(300f, timerService.CalculateRemainingTime(limit, 0f), 0.01f);
            Assert.AreEqual(200f, timerService.CalculateRemainingTime(limit, 100f), 0.01f);
            Assert.AreEqual(0f, timerService.CalculateRemainingTime(limit, 300f), 0.01f);
            Assert.AreEqual(0f, timerService.CalculateRemainingTime(limit, 350f), 0.01f); // Over limit clamps to 0
            Assert.AreEqual(0f, timerService.CalculateRemainingTime(0f, 50f), 0.01f); // Untimed
        }

        [Test]
        public void CaseTimerService_UrgencyThresholds_EvaluateCorrectly()
        {
            Assert.AreEqual(TimerUrgencyState.Normal, timerService.GetUrgencyState(180f));
            Assert.AreEqual(TimerUrgencyState.Normal, timerService.GetUrgencyState(120.5f));

            Assert.AreEqual(TimerUrgencyState.Warning, timerService.GetUrgencyState(120f));
            Assert.AreEqual(TimerUrgencyState.Warning, timerService.GetUrgencyState(60f));
            Assert.AreEqual(TimerUrgencyState.Warning, timerService.GetUrgencyState(30.5f));

            Assert.AreEqual(TimerUrgencyState.Urgent, timerService.GetUrgencyState(30f));
            Assert.AreEqual(TimerUrgencyState.Urgent, timerService.GetUrgencyState(10f));
            Assert.AreEqual(TimerUrgencyState.Urgent, timerService.GetUrgencyState(0f));
        }

        [Test]
        public void CaseTimerService_FormatsTime_MinutesSeconds_Correctly()
        {
            Assert.AreEqual("05:00", timerService.FormatTimeMinutesSeconds(300f));
            Assert.AreEqual("01:23", timerService.FormatTimeMinutesSeconds(83f));
            Assert.AreEqual("00:09", timerService.FormatTimeMinutesSeconds(9f));
            Assert.AreEqual("00:00", timerService.FormatTimeMinutesSeconds(0f));
            Assert.AreEqual("00:00", timerService.FormatTimeMinutesSeconds(-5f));
        }

        [Test]
        public void CaseTimerService_FormatsTime_Verbose_Correctly()
        {
            Assert.AreEqual("5m 00s", timerService.FormatTimeVerbose(300f));
            Assert.AreEqual("1m 23s", timerService.FormatTimeVerbose(83f));
            Assert.AreEqual("0m 09s", timerService.FormatTimeVerbose(9f));
            Assert.AreEqual("0m 00s", timerService.FormatTimeVerbose(0f));
        }

        [Test]
        public void CaseManager_LoadsTimedCase_InitializesCountdown()
        {
            CaseSO testCase = ScriptableObject.CreateInstance<CaseSO>();
            testCase.caseTitle = "Test Timed Case";
            testCase.hasTimeLimit = true;
            testCase.timeLimitSeconds = 180f;

            caseManager.LoadCase(testCase);

            Assert.IsTrue(caseManager.HasActiveTimeLimit);
            Assert.AreEqual(180f, caseManager.CaseTimeLimit);
            Assert.IsTrue(caseManager.IsTimerRunning);
            Assert.IsFalse(caseManager.HasTimeExpired);
            Assert.GreaterOrEqual(caseManager.RemainingTime, 179f);
        }

        [Test]
        public void CaseManager_LoadsUntimedCase_DoesNotRunTimer()
        {
            CaseSO testCase = ScriptableObject.CreateInstance<CaseSO>();
            testCase.caseTitle = "Test Untimed Case";
            testCase.hasTimeLimit = false;
            testCase.timeLimitSeconds = 300f;

            caseManager.LoadCase(testCase);

            Assert.IsFalse(caseManager.HasActiveTimeLimit);
            Assert.IsFalse(caseManager.IsTimerRunning);
            Assert.IsFalse(caseManager.HasTimeExpired);
        }

        [Test]
        public void CaseManager_PauseAndResumeTimer_OperatesCorrectly()
        {
            CaseSO testCase = ScriptableObject.CreateInstance<CaseSO>();
            testCase.hasTimeLimit = true;
            testCase.timeLimitSeconds = 200f;

            caseManager.LoadCase(testCase);
            Assert.IsTrue(caseManager.IsTimerRunning);

            caseManager.PauseTimer();
            Assert.IsFalse(caseManager.IsTimerRunning);

            caseManager.ResumeTimer();
            Assert.IsTrue(caseManager.IsTimerRunning);
        }

        [Test]
        public void CaseManager_TriggerTimeExpired_FiresEventAndSetsGameOver()
        {
            CaseSO testCase = ScriptableObject.CreateInstance<CaseSO>();
            testCase.hasTimeLimit = true;
            testCase.timeLimitSeconds = 60f;

            caseManager.LoadCase(testCase);

            bool eventFired = false;
            caseManager.OnTimeExpired += () => eventFired = true;

            caseManager.TriggerTimeExpired();

            Assert.IsTrue(eventFired);
            Assert.IsTrue(caseManager.HasTimeExpired);
            Assert.IsFalse(caseManager.IsTimerRunning);
            Assert.AreEqual(0f, caseManager.RemainingTime, 0.01f);
        }

        [Test]
        public void CaseManager_RetryCurrentCase_ResetsTimerAndClearsExpiration()
        {
            CaseSO testCase = ScriptableObject.CreateInstance<CaseSO>();
            testCase.caseTitle = "Retry Case Test";
            testCase.hasTimeLimit = true;
            testCase.timeLimitSeconds = 120f;

            caseManager.LoadCase(testCase);
            caseManager.TriggerTimeExpired();
            Assert.IsTrue(caseManager.HasTimeExpired);

            caseManager.RetryCurrentCase();

            Assert.IsFalse(caseManager.HasTimeExpired);
            Assert.IsTrue(caseManager.IsTimerRunning);
            Assert.GreaterOrEqual(caseManager.RemainingTime, 119f);
        }

        [Test]
        public void GameOverUI_PopulatesFailureDetailsCard()
        {
            CaseSO testCase = ScriptableObject.CreateInstance<CaseSO>();
            testCase.levelNumber = 2;
            testCase.caseTitle = "The Shattered Mirror";
            testCase.totalKeyEvidenceCount = 4;
            testCase.totalContradictionsCount = 2;
            testCase.hasTimeLimit = true;
            testCase.timeLimitSeconds = 180f;

            CharacterProfileSO detective = ScriptableObject.CreateInstance<CharacterProfileSO>();
            detective.fullName = "Kyle Pastrana";
            caseManager.selectedInvestigator = detective;

            caseManager.LoadCase(testCase);

            GameObject goObj = new GameObject("Test_GameOverPanel");
            goObj.transform.SetParent(testRoot.transform);
            GameOverUI gameOverUI = goObj.AddComponent<GameOverUI>();

            GameObject titleObj = new GameObject("Title", typeof(Text));
            titleObj.transform.SetParent(goObj.transform);
            Text titleText = titleObj.GetComponent<Text>();

            GameObject detailsObj = new GameObject("Details", typeof(Text));
            detailsObj.transform.SetParent(goObj.transform);
            Text detailsText = detailsObj.GetComponent<Text>();

            var serialized = new UnityEditor.SerializedObject(gameOverUI);
            serialized.FindProperty("titleText").objectReferenceValue = titleText;
            serialized.FindProperty("detailsBreakdownText").objectReferenceValue = detailsText;
            serialized.ApplyModifiedProperties();

            gameOverUI.PopulateGameOverDetails();

            StringAssert.Contains("LEVEL 2: TIME EXPIRED", titleText.text);
            StringAssert.Contains("Kyle Pastrana", detailsText.text);
            StringAssert.Contains("The Shattered Mirror", detailsText.text);
            StringAssert.Contains("Evidence Discovered: 0 / 4", detailsText.text);
            StringAssert.Contains("Contradictions Exposed: 0 / 2", detailsText.text);
            StringAssert.Contains("Status: UNRESOLVED", detailsText.text);
        }

        [Test]
        public void CaseManager_WhenTimeExpires_UIManagerTransitionsToGameOverPanel()
        {
            UIManager uiManager = testRoot.AddComponent<UIManager>();
            typeof(UIManager).GetProperty("Instance")?.SetValue(null, uiManager);

            GameObject tablePanel = new GameObject("TablePanel");
            tablePanel.transform.SetParent(testRoot.transform);
            uiManager.mainTablePanel = tablePanel;

            GameObject gameOverPanel = new GameObject("GameOverPanel");
            gameOverPanel.transform.SetParent(testRoot.transform);
            gameOverPanel.SetActive(false);
            uiManager.gameOverPanel = gameOverPanel;

            uiManager.ShowPanel(UIPanelType.InvestigationTable);
            Assert.AreEqual(UIPanelType.InvestigationTable, uiManager.currentPanel);
            Assert.IsTrue(tablePanel.activeSelf);
            Assert.IsFalse(gameOverPanel.activeSelf);

            CaseSO testCase = ScriptableObject.CreateInstance<CaseSO>();
            testCase.hasTimeLimit = true;
            testCase.timeLimitSeconds = 30f;
            caseManager.LoadCase(testCase);

            caseManager.TriggerTimeExpired();

            Assert.AreEqual(UIPanelType.GameOver, uiManager.currentPanel);
            Assert.IsTrue(gameOverPanel.activeSelf);
            Assert.IsFalse(tablePanel.activeSelf);

            typeof(UIManager).GetProperty("Instance")?.SetValue(null, null);
        }
    }
}
