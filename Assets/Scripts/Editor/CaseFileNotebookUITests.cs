using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
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
        private Texture2D dummyTex;
        private Sprite dummySprite;

        [SetUp]
        public void SetUp()
        {
            testRoot = new GameObject("Test_NotebookRoot");

            dummyTex = new Texture2D(32, 32);
            dummySprite = Sprite.Create(dummyTex, new Rect(0, 0, 32, 32), Vector2.zero);

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
            testEvidence.isExamined = true;
            testCase.evidenceItems = new List<EvidenceSO> { testEvidence };

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
            if (dummySprite != null) Object.DestroyImmediate(dummySprite);
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
    }
}
