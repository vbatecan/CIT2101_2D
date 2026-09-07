using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Services;
using CaseClosed.UI;

namespace CaseClosed.Tests
{
    [TestFixture]
    public class CaseFileNotebookUITests
    {
        private GameObject testRoot;
        private CaseFileNotebookUI notebookUI;
        private CaseSO testCase;
        private CharacterProfileSO testSuspect;
        private EvidenceSO testEvidence;
        private EvidenceSO testEvidence2;
        private Texture2D dummyTex;
        private Sprite dummySprite;
        private Sprite dummySpriteTop;

        [SetUp]
        public void SetUp()
        {
            testRoot = new GameObject("Test_NotebookRoot");

            dummyTex = new Texture2D(32, 32);
            dummySprite = Sprite.Create(dummyTex, new Rect(0, 0, 32, 32), Vector2.zero);
            dummySpriteTop = Sprite.Create(dummyTex, new Rect(0, 0, 16, 16), Vector2.zero);

            // Setup mock CaseSO
            testCase = ScriptableObject.CreateInstance<CaseSO>();
            testCase.caseTitle = "The Stolen Sapphire";
            testCase.levelNumber = 1;
            testCase.dateAndLocation = "Midnight at Grand Hotel";
            testCase.victimInfo = "Lord Archibald";
            testCase.incidentDescription = "A priceless sapphire was stolen from the royal suite.";
            testCase.objective = "Interrogate the suspect and recover the gem.";

            // Primary suspect
            testSuspect = ScriptableObject.CreateInstance<CharacterProfileSO>();
            testSuspect.characterId = "SUSPECT_01";
            testSuspect.fullName = "Charles Montgomery";
            testSuspect.age = 45;
            testSuspect.occupation = "Hotel Concierge";
            testSuspect.personalityTrait = PersonalityTrait.Defensive;
            testSuspect.alibi = "I was at the front desk all evening.";
            testSuspect.possibleMotives = "Crushing gambling debts.";
            testSuspect.knownConflicts = "Arguing with Lord Archibald.";
            testSuspect.defaultSittingPose = dummySprite;
            testCase.primarySuspect = testSuspect;

            // Evidence
            testEvidence = ScriptableObject.CreateInstance<EvidenceSO>();
            testEvidence.id = "EVD_01";
            testEvidence.evidenceName = "Golden Master Key";
            testEvidence.category = EvidenceCategory.PhysicalClue;
            testEvidence.baseDescription = "A master skeleton key fitting the royal suite.";
            testEvidence.detailedObservation = "Scratches on the key bit match fresh lock tumbler shavings.";
            testEvidence.normalSprite = dummySprite;
            testEvidence.zoomedSprite = dummySpriteTop;
            testEvidence.topPovSprite = dummySpriteTop;
            testEvidence.isExamined = true;

            testEvidence2 = ScriptableObject.CreateInstance<EvidenceSO>();
            testEvidence2.id = "EVD_02";
            testEvidence2.evidenceName = "Torn Ledger Page";
            testEvidence2.category = EvidenceCategory.Document;
            testEvidence2.baseDescription = "A burned page from the concierge ledger.";
            testEvidence2.normalSprite = dummySprite;
            testEvidence2.zoomedSprite = dummySpriteTop;
            testEvidence2.topPovSprite = dummySpriteTop;
            testEvidence2.isExamined = false;

            testCase.evidenceItems = new List<EvidenceSO> { testEvidence, testEvidence2 };

            // Setup UI Components
            GameObject clipboardRootGO = new GameObject("Clipboard_Root", typeof(RectTransform));
            clipboardRootGO.transform.SetParent(testRoot.transform);
            RectTransform clipboardRoot = clipboardRootGO.GetComponent<RectTransform>();

            GameObject titleGO = new GameObject("Text_Title", typeof(Text));
            titleGO.transform.SetParent(clipboardRootGO.transform);

            GameObject bodyGO = new GameObject("Text_Body", typeof(Text));
            bodyGO.transform.SetParent(clipboardRootGO.transform);

            GameObject suspectCardGO = new GameObject("Suspect_Card");
            suspectCardGO.transform.SetParent(clipboardRootGO.transform);
            GameObject portraitGO = new GameObject("Portrait_Image", typeof(Image));
            portraitGO.transform.SetParent(suspectCardGO.transform);
            GameObject suspectNameGO = new GameObject("Suspect_Name", typeof(Text));
            suspectNameGO.transform.SetParent(suspectCardGO.transform);

            GameObject evidenceCardGO = new GameObject("Evidence_Card");
            evidenceCardGO.transform.SetParent(clipboardRootGO.transform);
            GameObject evidenceImgGO = new GameObject("Evidence_Image", typeof(Image));
            evidenceImgGO.transform.SetParent(evidenceCardGO.transform);
            GameObject evidenceNameGO = new GameObject("Evidence_Name", typeof(Text));
            evidenceNameGO.transform.SetParent(evidenceCardGO.transform);

            GameObject summaryCardGO = new GameObject("Summary_Card");
            summaryCardGO.transform.SetParent(clipboardRootGO.transform);
            GameObject summaryTitleGO = new GameObject("Summary_Title", typeof(Text));
            summaryTitleGO.transform.SetParent(summaryCardGO.transform);
            GameObject summaryMetaGO = new GameObject("Summary_Meta", typeof(Text));
            summaryMetaGO.transform.SetParent(summaryCardGO.transform);

            GameObject cluesCardGO = new GameObject("Clues_Card");
            cluesCardGO.transform.SetParent(clipboardRootGO.transform);
            GameObject cluesCountGO = new GameObject("Clues_Count", typeof(Text));
            cluesCountGO.transform.SetParent(cluesCardGO.transform);

            GameObject tabSummaryGO = new GameObject("Tab_Summary", typeof(RectTransform), typeof(Button), typeof(Image));
            tabSummaryGO.transform.SetParent(clipboardRootGO.transform);
            GameObject tabSuspectsGO = new GameObject("Tab_Suspects", typeof(RectTransform), typeof(Button), typeof(Image));
            tabSuspectsGO.transform.SetParent(clipboardRootGO.transform);
            GameObject tabEvidenceGO = new GameObject("Tab_Evidence", typeof(RectTransform), typeof(Button), typeof(Image));
            tabEvidenceGO.transform.SetParent(clipboardRootGO.transform);
            GameObject tabCluesGO = new GameObject("Tab_Clues", typeof(RectTransform), typeof(Button), typeof(Image));
            tabCluesGO.transform.SetParent(clipboardRootGO.transform);
            GameObject closeBtnGO = new GameObject("Button_Close", typeof(Button));
            closeBtnGO.transform.SetParent(clipboardRootGO.transform);

            notebookUI = testRoot.AddComponent<CaseFileNotebookUI>();
            notebookUI.clipboardRoot = clipboardRoot;
            notebookUI.notebookTitleText = titleGO.GetComponent<Text>();
            notebookUI.notebookContentBody = bodyGO.GetComponent<Text>();
            notebookUI.suspectCardSection = suspectCardGO;
            notebookUI.suspectPortraitImage = portraitGO.GetComponent<Image>();
            notebookUI.suspectNameLabel = suspectNameGO.GetComponent<Text>();
            notebookUI.evidenceCardSection = evidenceCardGO;
            notebookUI.evidencePreviewImage = evidenceImgGO.GetComponent<Image>();
            notebookUI.evidenceNameLabel = evidenceNameGO.GetComponent<Text>();
            notebookUI.summaryCardSection = summaryCardGO;
            notebookUI.summaryCaseTitleLabel = summaryTitleGO.GetComponent<Text>();
            notebookUI.summaryCaseMetaLabel = summaryMetaGO.GetComponent<Text>();
            notebookUI.cluesCardSection = cluesCardGO;
            notebookUI.cluesCountLabel = cluesCountGO.GetComponent<Text>();

            notebookUI.summaryTabButton = tabSummaryGO.GetComponent<Button>();
            notebookUI.suspectsTabButton = tabSuspectsGO.GetComponent<Button>();
            notebookUI.evidenceTabButton = tabEvidenceGO.GetComponent<Button>();
            notebookUI.cluesTabButton = tabCluesGO.GetComponent<Button>();
            notebookUI.closeNotebookButton = closeBtnGO.GetComponent<Button>();

            notebookUI.activeCaseData = testCase;
            notebookUI.evidenceOverride = new HashSet<string> { "EVD_01" };
            notebookUI.cluesOverride = new Dictionary<string, string> { { "101", "Witness contradiction" } };
        }

        [TearDown]
        public void TearDown()
        {
            if (testRoot != null) Object.DestroyImmediate(testRoot);
            if (testCase != null) Object.DestroyImmediate(testCase);
            if (testSuspect != null) Object.DestroyImmediate(testSuspect);
            if (testEvidence != null) Object.DestroyImmediate(testEvidence);
            if (testEvidence2 != null) Object.DestroyImmediate(testEvidence2);
            if (dummySprite != null) Object.DestroyImmediate(dummySprite);
            if (dummySpriteTop != null) Object.DestroyImmediate(dummySpriteTop);
            if (dummyTex != null) Object.DestroyImmediate(dummyTex);
        }

        [Test]
        public void NotebookFormattingService_FormatCaseSummary_ContainsAllCoreFields()
        {
            NotebookFormattingService service = new NotebookFormattingService();
            string text = service.FormatCaseSummary(testCase);

            Assert.That(text, Does.Contain("CASE FILE #1"));
            Assert.That(text, Does.Contain("The Stolen Sapphire"));
            Assert.That(text, Does.Contain("Lord Archibald"));
            Assert.That(text, Does.Contain("A priceless sapphire was stolen"));
            Assert.That(text, Does.Contain("Interrogate the suspect"));
        }

        [Test]
        public void NotebookFormattingService_FormatSuspectProfiles_ContainsSuspectDetails()
        {
            NotebookFormattingService service = new NotebookFormattingService();
            string text = service.FormatSuspectProfiles(testCase);

            Assert.That(text, Does.Contain("CHARLES MONTGOMERY"));
            Assert.That(text, Does.Contain("[PRIMARY SUSPECT]"));
            Assert.That(text, Does.Contain("Hotel Concierge"));
            Assert.That(text, Does.Contain("I was at the front desk"));
            Assert.That(text, Does.Contain("Crushing gambling debts"));
        }

        [Test]
        public void NotebookFormattingService_FormatDiscoveredEvidence_ShowsExaminedDetails()
        {
            NotebookFormattingService service = new NotebookFormattingService();
            HashSet<string> discovered = new HashSet<string> { "EVD_01" };
            string text = service.FormatDiscoveredEvidence(testCase, discovered);

            Assert.That(text, Does.Contain("Golden Master Key"));
            Assert.That(text, Does.Contain("[PhysicalClue]"));
            Assert.That(text, Does.Contain("[EXAMINED]"));
            Assert.That(text, Does.Contain("Scratches on the key bit"));
        }

        [Test]
        public void NotebookFormattingService_FormatDiscoveredEvidence_EmptyShowsPlaceholder()
        {
            NotebookFormattingService service = new NotebookFormattingService();
            HashSet<string> discovered = new HashSet<string>();
            string text = service.FormatDiscoveredEvidence(testCase, discovered);

            Assert.That(text, Does.Contain("No physical evidence logged yet"));
        }

        [Test]
        public void NotebookFormattingService_FormatUnlockedClues_FormatsDictProperly()
        {
            NotebookFormattingService service = new NotebookFormattingService();
            Dictionary<string, string> clues = new Dictionary<string, string>
            {
                { "101", "Alibi contradiction: Witness saw suspect at 11:30 PM near the balcony." }
            };
            string text = service.FormatUnlockedClues(clues);

            Assert.That(text, Does.Contain("[CLUE #101]"));
            Assert.That(text, Does.Contain("Alibi contradiction"));
        }

        [Test]
        public void NotebookFormattingService_FormatUnlockedClues_EmptyShowsPrompt()
        {
            NotebookFormattingService service = new NotebookFormattingService();
            string text = service.FormatUnlockedClues(new Dictionary<string, string>());

            Assert.That(text, Does.Contain("No deduction clues unlocked yet"));
        }

        [Test]
        public void CaseFileNotebookUI_SwitchTab_ActivatesCorrespondingCards()
        {
            // Switch to Suspects
            notebookUI.SwitchTab(NotebookTab.Suspects);
            Assert.That(notebookUI.notebookTitleText.text, Is.EqualTo("SUSPECT DOSSIER"));
            Assert.That(notebookUI.suspectCardSection.activeSelf, Is.True);
            Assert.That(notebookUI.summaryCardSection.activeSelf, Is.False);
            Assert.That(notebookUI.cluesCardSection.activeSelf, Is.False);

            // Switch to Evidence
            notebookUI.SwitchTab(NotebookTab.Evidence);
            Assert.That(notebookUI.notebookTitleText.text, Is.EqualTo("EVIDENCE REPOSITORY"));
            Assert.That(notebookUI.evidenceCardSection.activeSelf, Is.True);
            Assert.That(notebookUI.suspectCardSection.activeSelf, Is.False);

            // Switch to Clues
            notebookUI.SwitchTab(NotebookTab.Clues);
            Assert.That(notebookUI.notebookTitleText.text, Is.EqualTo("DEDUCTION JOURNAL"));
            Assert.That(notebookUI.cluesCardSection.activeSelf, Is.True);
            Assert.That(notebookUI.suspectCardSection.activeSelf, Is.False);
            Assert.That(notebookUI.evidenceCardSection.activeSelf, Is.False);

            // Switch to CaseSummary
            notebookUI.SwitchTab(NotebookTab.CaseSummary);
            Assert.That(notebookUI.summaryCardSection.activeSelf, Is.True);
            Assert.That(notebookUI.cluesCardSection.activeSelf, Is.False);
            Assert.That(notebookUI.suspectCardSection.activeSelf, Is.False);
            Assert.That(notebookUI.evidenceCardSection.activeSelf, Is.False);
        }

        [Test]
        public void CaseFileNotebookUI_LandscapeDimensions_AreProperlyProportioned()
        {
            // Verify default hidden and visible Y positions for landscape layout
            Assert.That(notebookUI.hiddenPosY, Is.EqualTo(-1100f));
            Assert.That(notebookUI.visiblePosY, Is.EqualTo(0f));
            Assert.That(notebookUI.slideDuration, Is.EqualTo(0.35f));
        }

        [Test]
        public void CaseFileNotebookUI_TabButtonsClick_SwitchesTabsSuccessfully()
        {
            notebookUI.SetupTabButtons();

            // Simulate clicking Suspects tab button
            notebookUI.suspectsTabButton.onClick.Invoke();
            Assert.That(notebookUI.notebookTitleText.text, Is.EqualTo("SUSPECT DOSSIER"));
            Assert.That(notebookUI.suspectCardSection.activeSelf, Is.True);

            // Simulate clicking Evidence tab button
            notebookUI.evidenceTabButton.onClick.Invoke();
            Assert.That(notebookUI.notebookTitleText.text, Is.EqualTo("EVIDENCE REPOSITORY"));
            Assert.That(notebookUI.evidenceCardSection.activeSelf, Is.True);

            // Simulate clicking Clues tab button
            notebookUI.cluesTabButton.onClick.Invoke();
            Assert.That(notebookUI.notebookTitleText.text, Is.EqualTo("DEDUCTION JOURNAL"));
            Assert.That(notebookUI.cluesCardSection.activeSelf, Is.True);

            // Simulate clicking Summary tab button
            notebookUI.summaryTabButton.onClick.Invoke();
            Assert.That(notebookUI.notebookTitleText.text, Is.EqualTo(testCase.caseTitle));
            Assert.That(notebookUI.summaryCardSection.activeSelf, Is.True);
        }

        [Test]
        public void NotebookFormattingService_FormatEvidenceDossier_DiscoveredShowsDetails()
        {
            NotebookFormattingService service = new NotebookFormattingService();
            string text = service.FormatEvidenceDossier(testEvidence, true, 1, 2);

            Assert.That(text, Does.Contain("[EVIDENCE 1 OF 2]"));
            Assert.That(text, Does.Contain("GOLDEN MASTER KEY"));
            Assert.That(text, Does.Contain("LOGGED & VERIFIED"));
            Assert.That(text, Does.Contain("Scratches on the key bit match"));
        }

        [Test]
        public void NotebookFormattingService_FormatEvidenceDossier_LockedShowsPlaceholder()
        {
            NotebookFormattingService service = new NotebookFormattingService();
            string text = service.FormatEvidenceDossier(testEvidence2, false, 2, 2);

            Assert.That(text, Does.Contain("[EVIDENCE 2 OF 2]"));
            Assert.That(text, Does.Contain("[ ??? UNDISCOVERED EVIDENCE ]"));
            Assert.That(text, Does.Contain("NOT YET RECOVERED"));
            Assert.That(text, Does.Contain("has not yet been discovered by the detective"));
        }

        [Test]
        public void CaseFileNotebookUI_EvidenceRepository_UsesTopPovSprite()
        {
            notebookUI.SwitchTab(NotebookTab.Evidence);

            Assert.That(notebookUI.CurrentEvidenceIndex, Is.EqualTo(0));
            Assert.That(notebookUI.evidencePreviewImage.gameObject.activeSelf, Is.True);
            Assert.That(notebookUI.evidencePreviewImage.sprite, Is.EqualTo(dummySpriteTop));
            Assert.That(notebookUI.evidenceNameLabel.text, Is.EqualTo("Golden Master Key"));
        }

        [Test]
        public void CaseFileNotebookUI_EvidenceRepository_CyclesMultipleEvidenceWithNextAndPrev()
        {
            notebookUI.SwitchTab(NotebookTab.Evidence);
            Assert.That(notebookUI.CurrentEvidenceIndex, Is.EqualTo(0));

            // Cycle Next
            notebookUI.NextEvidence();
            Assert.That(notebookUI.CurrentEvidenceIndex, Is.EqualTo(1));
            // Item 2 is undiscovered in override
            Assert.That(notebookUI.evidenceNameLabel.text, Is.EqualTo("[ ??? LOCKED EVIDENCE ]"));
            Assert.That(notebookUI.evidencePreviewImage.gameObject.activeSelf, Is.False);
            Assert.That(notebookUI.evidenceIndexLabel.text, Does.Contain("2 / 2"));

            // Cycle Prev (back to Item 1)
            notebookUI.PreviousEvidence();
            Assert.That(notebookUI.CurrentEvidenceIndex, Is.EqualTo(0));
            Assert.That(notebookUI.evidenceNameLabel.text, Is.EqualTo("Golden Master Key"));
            Assert.That(notebookUI.evidencePreviewImage.gameObject.activeSelf, Is.True);
            Assert.That(notebookUI.evidenceIndexLabel.text, Does.Contain("1 / 2"));
        }

        [Test]
        public void CaseFileNotebookUI_FocusEvidence_NavigatesDirectlyToTargetEvidence()
        {
            notebookUI.FocusEvidence("EVD_02");

            Assert.That(notebookUI.CurrentEvidenceIndex, Is.EqualTo(1));
            Assert.That(notebookUI.notebookTitleText.text, Is.EqualTo("EVIDENCE REPOSITORY"));
            Assert.That(notebookUI.evidenceCardSection.activeSelf, Is.True);
        }

        [Test]
        public void CaseFileNotebookUI_EvidenceRepository_SmartFocus_SelectsFirstDiscoveredWhenIndexZeroUndiscovered()
        {
            // EVD_01 (index 0) is undiscovered, EVD_02 (index 1) is discovered
            notebookUI.evidenceOverride = new HashSet<string> { "EVD_02" };

            notebookUI.SwitchTab(NotebookTab.Evidence);

            // Smart focus should advance to index 1 (EVD_02)
            Assert.That(notebookUI.CurrentEvidenceIndex, Is.EqualTo(1));
            Assert.That(notebookUI.evidenceNameLabel.text, Is.EqualTo("Torn Ledger Page"));
            Assert.That(notebookUI.evidencePreviewImage.gameObject.activeSelf, Is.True);
            Assert.That(notebookUI.evidencePreviewImage.preserveAspect, Is.True);
            Assert.That(notebookUI.evidenceLockedPlaceholder.activeSelf, Is.False);
            Assert.That(notebookUI.evidenceIndexLabel.text, Does.Contain("2 / 2"));
        }

        [Test]
        public void CaseFileNotebookUI_EvidenceRepository_SmartFocus_FallsBackToZeroWhenNoEvidenceDiscovered()
        {
            // No evidence discovered yet
            notebookUI.evidenceOverride = new HashSet<string>();

            notebookUI.SwitchTab(NotebookTab.Evidence);

            // Smart focus stays at 0
            Assert.That(notebookUI.CurrentEvidenceIndex, Is.EqualTo(0));
            Assert.That(notebookUI.evidencePreviewImage.gameObject.activeSelf, Is.False);
            Assert.That(notebookUI.evidenceLockedPlaceholder.activeSelf, Is.True);
            Assert.That(notebookUI.evidenceLockedLabel.text, Is.EqualTo("[ EVIDENCE LOCKED ]"));
            Assert.That(notebookUI.evidenceNameLabel.text, Is.EqualTo("[ ??? LOCKED EVIDENCE ]"));
            Assert.That(notebookUI.notebookContentBody.text, Does.Contain("[ ??? UNDISCOVERED EVIDENCE ]"));
            Assert.That(notebookUI.evidenceIndexLabel.text, Does.Contain("1 / 2"));
        }

        [Test]
        public void CaseFileNotebookUI_EvidenceRepository_SmartFocus_PreservesCurrentlyFocusedIfAlreadyDiscovered()
        {
            // Both items discovered
            notebookUI.evidenceOverride = new HashSet<string> { "EVD_01", "EVD_02" };
            notebookUI.FocusEvidence("EVD_02");
            Assert.That(notebookUI.CurrentEvidenceIndex, Is.EqualTo(1));

            // Switch to Suspects and back to Evidence
            notebookUI.SwitchTab(NotebookTab.Suspects);
            notebookUI.SwitchTab(NotebookTab.Evidence);

            // Stays on index 1 because it's already discovered
            Assert.That(notebookUI.CurrentEvidenceIndex, Is.EqualTo(1));
            Assert.That(notebookUI.evidenceNameLabel.text, Is.EqualTo("Torn Ledger Page"));
            Assert.That(notebookUI.evidencePreviewImage.gameObject.activeSelf, Is.True);
        }

        [Test]
        public void CaseFileNotebookUI_EvidenceRepository_SmartFocus_RefocusesDiscoveredAfterNavigatingToLockedItem()
        {
            // Only EVD_01 (index 0) is discovered
            notebookUI.evidenceOverride = new HashSet<string> { "EVD_01" };
            notebookUI.SwitchTab(NotebookTab.Evidence);
            Assert.That(notebookUI.CurrentEvidenceIndex, Is.EqualTo(0));

            // User manually cycles to locked item (index 1)
            notebookUI.NextEvidence();
            Assert.That(notebookUI.CurrentEvidenceIndex, Is.EqualTo(1));
            Assert.That(notebookUI.evidenceNameLabel.text, Is.EqualTo("[ ??? LOCKED EVIDENCE ]"));
            Assert.That(notebookUI.evidenceLockedPlaceholder.activeSelf, Is.True);
            Assert.That(notebookUI.evidenceLockedLabel.text, Is.EqualTo("[ EVIDENCE LOCKED ]"));
            Assert.That(notebookUI.evidencePreviewImage.gameObject.activeSelf, Is.False);

            // User navigates away and returns to Evidence tab
            notebookUI.SwitchTab(NotebookTab.Clues);
            notebookUI.SwitchTab(NotebookTab.Evidence);

            // Smart focus detects index 1 is undiscovered, refocuses index 0 (EVD_01)
            Assert.That(notebookUI.CurrentEvidenceIndex, Is.EqualTo(0));
            Assert.That(notebookUI.evidenceNameLabel.text, Is.EqualTo("Golden Master Key"));
            Assert.That(notebookUI.evidencePreviewImage.gameObject.activeSelf, Is.True);
            Assert.That(notebookUI.evidenceLockedPlaceholder.activeSelf, Is.False);
        }

        [Test]
        public void CaseFileNotebookUI_SetTargetZoom_ClampsAndAppliesScale()
        {
            notebookUI.smoothZoom = false;
            notebookUI.minZoom = 1.0f;
            notebookUI.maxZoom = 3.5f;

            notebookUI.SetTargetZoom(2.5f);
            Assert.AreEqual(2.5f, notebookUI.TargetZoom, 0.001f);
            Assert.AreEqual(2.5f, notebookUI.CurrentZoom, 0.001f);
            Assert.AreEqual(2.5f, notebookUI.clipboardRoot.localScale.x, 0.001f);

            notebookUI.SetTargetZoom(5.0f);
            Assert.AreEqual(3.5f, notebookUI.TargetZoom, 0.001f);
            Assert.AreEqual(3.5f, notebookUI.clipboardRoot.localScale.x, 0.001f);

            notebookUI.SetTargetZoom(0.2f);
            Assert.AreEqual(1.0f, notebookUI.TargetZoom, 0.001f);
            Assert.AreEqual(1.0f, notebookUI.clipboardRoot.localScale.x, 0.001f);
        }

        [Test]
        public void CaseFileNotebookUI_SetTargetPosition_ClampsAndAppliesPosition()
        {
            notebookUI.smoothZoom = false;
            notebookUI.SetTargetPosition(new Vector2(60f, -40f));
            Assert.AreEqual(new Vector2(60f, -40f), notebookUI.TargetPosition);
            Assert.AreEqual(new Vector2(60f, -40f), notebookUI.clipboardRoot.anchoredPosition);
        }

        [Test]
        public void CaseFileNotebookUI_ResetView_RestoresDefaultZoomAndCenter()
        {
            notebookUI.smoothZoom = false;
            notebookUI.SetTargetZoom(3.0f);
            notebookUI.SetTargetPosition(new Vector2(200f, 150f));

            notebookUI.ResetView();

            Assert.AreEqual(1.0f, notebookUI.TargetZoom, 0.001f);
            Assert.AreEqual(1.0f, notebookUI.CurrentZoom, 0.001f);
            Assert.AreEqual(Vector2.zero, notebookUI.TargetPosition);
            Assert.AreEqual(Vector2.zero, notebookUI.clipboardRoot.anchoredPosition);
            Assert.AreEqual(Vector3.one, notebookUI.clipboardRoot.localScale);
        }

        [Test]
        public void CaseFileNotebookUI_OnPointerClick_RightClickResetsView()
        {
            notebookUI.smoothZoom = false;
            notebookUI.SetTargetZoom(2.5f);
            notebookUI.SetTargetPosition(new Vector2(100f, 80f));

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Right
            };

            notebookUI.OnPointerClick(eventData);

            Assert.AreEqual(1.0f, notebookUI.TargetZoom, 0.001f);
            Assert.AreEqual(Vector2.zero, notebookUI.TargetPosition);
            Assert.AreEqual(Vector3.one, notebookUI.clipboardRoot.localScale);
        }

        [Test]
        public void CaseFileNotebookUI_OnScroll_AdjustsZoom()
        {
            notebookUI.smoothZoom = false;
            notebookUI.ResetView();
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                scrollDelta = new Vector2(0f, 2.0f),
                position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)
            };

            notebookUI.OnScroll(eventData);

            Assert.Greater(notebookUI.TargetZoom, 1.0f);
        }

        [Test]
        public void CaseFileNotebookUI_EnsureRaycastSetup_AttachesDragTargetAndZeroesScrollSensitivity()
        {
            GameObject scrollGO = new GameObject("Test_ScrollRect", typeof(ScrollRect));
            scrollGO.transform.SetParent(notebookUI.clipboardRoot);
            ScrollRect scrollRect = scrollGO.GetComponent<ScrollRect>();
            scrollRect.scrollSensitivity = 15f;
            notebookUI.contentScrollRect = scrollRect;

            notebookUI.EnsureRaycastSetup();

            var dragTarget = notebookUI.clipboardRoot.GetComponent<CaseFileNotebookDragTarget>();
            Assert.IsNotNull(dragTarget);
            Assert.AreEqual(0f, notebookUI.contentScrollRect.scrollSensitivity, 0.001f);
        }

        [Test]
        public void CaseFileNotebookUI_LeftClickOnNotebook_DoesNotClose()
        {
            notebookUI.EnsureRaycastSetup();

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                pointerCurrentRaycast = new RaycastResult
                {
                    gameObject = notebookUI.clipboardRoot.gameObject
                }
            };

            notebookUI.OnPointerClick(eventData);
            Assert.IsFalse(notebookUI.IsClosing, "Left-clicking on clipboardRoot should not trigger close.");

            var dragTarget = notebookUI.clipboardRoot.GetComponent<CaseFileNotebookDragTarget>();
            dragTarget.OnPointerClick(eventData);
            Assert.IsFalse(notebookUI.IsClosing, "CaseFileNotebookDragTarget should ignore left clicks.");
        }

        [Test]
        public void CaseFileNotebookDragTarget_RightClick_ResetsView()
        {
            notebookUI.smoothZoom = false;
            notebookUI.EnsureRaycastSetup();
            notebookUI.SetTargetZoom(2.8f);

            var dragTarget = notebookUI.clipboardRoot.GetComponent<CaseFileNotebookDragTarget>();
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Right
            };

            dragTarget.OnPointerClick(eventData);
            Assert.AreEqual(1.0f, notebookUI.TargetZoom, 0.001f);
        }

        [Test]
        public void CaseFileNotebookUI_SwitchTab_PreservesZoomAndPosition()
        {
            notebookUI.smoothZoom = false;
            notebookUI.SetTargetZoom(2.4f);
            notebookUI.SetTargetPosition(new Vector2(150f, 75f));

            Assert.AreEqual(2.4f, notebookUI.CurrentZoom, 0.001f);
            Assert.AreEqual(new Vector2(150f, 75f), notebookUI.CurrentPosition);

            // Switch to Suspects
            notebookUI.SwitchTab(NotebookTab.Suspects);
            Assert.AreEqual(2.4f, notebookUI.CurrentZoom, 0.001f);
            Assert.AreEqual(2.4f, notebookUI.TargetZoom, 0.001f);
            Assert.AreEqual(new Vector2(150f, 75f), notebookUI.CurrentPosition);
            Assert.AreEqual(new Vector2(150f, 75f), notebookUI.TargetPosition);

            // Switch to Evidence
            notebookUI.SwitchTab(NotebookTab.Evidence);
            Assert.AreEqual(2.4f, notebookUI.CurrentZoom, 0.001f);
            Assert.AreEqual(2.4f, notebookUI.TargetZoom, 0.001f);
            Assert.AreEqual(new Vector2(150f, 75f), notebookUI.CurrentPosition);
            Assert.AreEqual(new Vector2(150f, 75f), notebookUI.TargetPosition);

            // Switch to Clues
            notebookUI.SwitchTab(NotebookTab.Clues);
            Assert.AreEqual(2.4f, notebookUI.CurrentZoom, 0.001f);
            Assert.AreEqual(2.4f, notebookUI.TargetZoom, 0.001f);
            Assert.AreEqual(new Vector2(150f, 75f), notebookUI.CurrentPosition);
            Assert.AreEqual(new Vector2(150f, 75f), notebookUI.TargetPosition);
        }

        [Test]
        public void CaseFileNotebookUI_Drag_UpdatesTargetPosition()
        {
            notebookUI.smoothZoom = false;
            notebookUI.ResetView();

            PointerEventData beginData = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = new Vector2(500f, 500f)
            };
            notebookUI.OnBeginDrag(beginData);
            Assert.IsTrue(notebookUI.IsDragging);

            PointerEventData dragData = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = new Vector2(550f, 530f)
            };
            notebookUI.OnDrag(dragData);
            Assert.AreEqual(new Vector2(50f, 30f), notebookUI.TargetPosition);

            PointerEventData endData = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left
            };
            notebookUI.OnEndDrag(endData);
            Assert.IsFalse(notebookUI.IsDragging);
        }
    }
}
