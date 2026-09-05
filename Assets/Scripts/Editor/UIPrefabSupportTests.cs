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
    public class UIPrefabSupportTests
    {
        private GameObject testRoot;
        private CaseSO testCase;
        private EvidenceSO testEvidence;
        private CaseManager caseManager;

        [SetUp]
        public void SetUp()
        {
            testRoot = new GameObject("Test_UIPrefabRoot");

            testEvidence = ScriptableObject.CreateInstance<EvidenceSO>();
            testEvidence.id = "EVD_TEST_PREFAB";
            testEvidence.evidenceName = "Stolen Pocket Watch";
            testEvidence.category = EvidenceCategory.PhysicalClue;
            testEvidence.baseDescription = "A gold watch found at the scene.";
            testEvidence.unlockedClueText = "The back of the watch is engraved with initials.";

            EvidenceHotspot spot = new EvidenceHotspot
            {
                hotspotId = "SPOT_ENGRAVING",
                hotspotTitle = "Engraved Initials",
                normalizedPosition = new Vector2(0.4f, 0.6f),
                isDiscovered = false,
                observationText = "Initials 'C.M.' are inscribed inside."
            };
            testEvidence.hotspots = new List<EvidenceHotspot> { spot };

            testCase = ScriptableObject.CreateInstance<CaseSO>();
            testCase.caseTitle = "Prefab Test Case";
            testCase.levelNumber = 1;
            testCase.evidenceItems = new List<EvidenceSO> { testEvidence };

            ConclusionQuestion q = new ConclusionQuestion
            {
                questionText = "Who was the owner of the watch?",
                options = new List<string> { "Charles Montgomery", "Lord Archibald", "The Butler" },
                correctOptionIndex = 0
            };
            testCase.conclusionQuestions = new List<ConclusionQuestion> { q };

            // Setup CaseManager singleton
            GameObject caseMgrObj = new GameObject("CaseManager");
            caseMgrObj.transform.SetParent(testRoot.transform);
            caseManager = caseMgrObj.AddComponent<CaseManager>();
            caseManager.activeCase = testCase;
            caseManager.discoveredEvidenceIds.Add(testEvidence.id);

            PropertyInfo instProp = typeof(CaseManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            instProp?.SetValue(null, caseManager);
        }

        [TearDown]
        public void TearDown()
        {
            PropertyInfo instProp = typeof(CaseManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            instProp?.SetValue(null, null);

            if (testRoot != null)
            {
                Object.DestroyImmediate(testRoot);
            }
            if (testEvidence != null)
            {
                Object.DestroyImmediate(testEvidence);
            }
            if (testCase != null)
            {
                Object.DestroyImmediate(testCase);
            }
        }

        #region DialogueUI Tests

        [Test]
        public void DialogueUI_PopulateEvidencePicker_UsesPrefabWhenAssigned()
        {
            GameObject dialogueObj = new GameObject("DialogueUI");
            dialogueObj.transform.SetParent(testRoot.transform);
            DialogueUI dialogueUI = dialogueObj.AddComponent<DialogueUI>();

            GameObject gridObj = new GameObject("Grid", typeof(RectTransform));
            gridObj.transform.SetParent(dialogueObj.transform);
            dialogueUI.evidencePickerGrid = gridObj.transform;

            // Create prefab with Button, Image (icon), and Text (title)
            GameObject itemPrefab = new GameObject("ItemPrefab", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObj.transform.SetParent(itemPrefab.transform);
            GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            titleObj.transform.SetParent(itemPrefab.transform);
            itemPrefab.transform.SetParent(testRoot.transform);

            dialogueUI.evidencePickerItemPrefab = itemPrefab;

            MethodInfo populateMethod = typeof(DialogueUI).GetMethod("PopulateEvidencePicker", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(populateMethod);
            populateMethod.Invoke(dialogueUI, null);

            Assert.AreEqual(1, gridObj.transform.childCount);
            Transform child = gridObj.transform.GetChild(0);
            Assert.AreEqual($"Present_{testEvidence.id}", child.name);
            Text titleText = child.GetComponentInChildren<Text>();
            Assert.IsNotNull(titleText);
            Assert.AreEqual(testEvidence.evidenceName, titleText.text);

            Button btn = child.GetComponentInChildren<Button>();
            Assert.IsNotNull(btn);
        }

        [Test]
        public void DialogueUI_PopulateEvidencePicker_FallsBackToProceduralWhenPrefabNull()
        {
            GameObject dialogueObj = new GameObject("DialogueUI");
            dialogueObj.transform.SetParent(testRoot.transform);
            DialogueUI dialogueUI = dialogueObj.AddComponent<DialogueUI>();

            GameObject gridObj = new GameObject("Grid", typeof(RectTransform));
            gridObj.transform.SetParent(dialogueObj.transform);
            dialogueUI.evidencePickerGrid = gridObj.transform;
            dialogueUI.evidencePickerItemPrefab = null;

            MethodInfo populateMethod = typeof(DialogueUI).GetMethod("PopulateEvidencePicker", BindingFlags.NonPublic | BindingFlags.Instance);
            populateMethod.Invoke(dialogueUI, null);

            Assert.AreEqual(1, gridObj.transform.childCount);
            Transform child = gridObj.transform.GetChild(0);
            Assert.AreEqual($"Present_{testEvidence.id}", child.name);
            Assert.IsNotNull(child.GetComponent<Button>());
            Assert.IsNotNull(child.GetComponent<Image>());
        }

        #endregion

        #region DeductionBoardUI Tests

        [Test]
        public void DeductionBoardUI_RenderClueCards_UsesPrefabWhenAssigned()
        {
            GameObject boardObj = new GameObject("DeductionBoardUI");
            boardObj.transform.SetParent(testRoot.transform);
            DeductionBoardUI boardUI = boardObj.AddComponent<DeductionBoardUI>();

            GameObject cluesContainer = new GameObject("CluesContainer", typeof(RectTransform));
            cluesContainer.transform.SetParent(boardObj.transform);
            boardUI.cluesContainer = cluesContainer.transform;

            // Create prefab with Button, Image, Title Text, Body Text
            GameObject cardPrefab = new GameObject("ClueCardPrefab", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            titleObj.transform.SetParent(cardPrefab.transform);
            GameObject bodyObj = new GameObject("Body", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            bodyObj.transform.SetParent(cardPrefab.transform);
            cardPrefab.transform.SetParent(testRoot.transform);

            boardUI.clueCardPrefab = cardPrefab;

            MethodInfo renderMethod = typeof(DeductionBoardUI).GetMethod("RenderClueCards", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(renderMethod);
            renderMethod.Invoke(boardUI, new object[] { testCase });

            Assert.AreEqual(1, cluesContainer.transform.childCount);
            Transform child = cluesContainer.transform.GetChild(0);
            Text[] texts = child.GetComponentsInChildren<Text>();
            Assert.AreEqual(2, texts.Length);
            Assert.IsTrue(texts[0].text == testEvidence.evidenceName || texts[1].text == testEvidence.evidenceName);
            Assert.IsNotNull(child.GetComponent<Button>());
        }

        [Test]
        public void DeductionBoardUI_RenderClueCards_FallsBackToProceduralWhenPrefabNull()
        {
            GameObject boardObj = new GameObject("DeductionBoardUI");
            boardObj.transform.SetParent(testRoot.transform);
            DeductionBoardUI boardUI = boardObj.AddComponent<DeductionBoardUI>();

            GameObject cluesContainer = new GameObject("CluesContainer", typeof(RectTransform));
            cluesContainer.transform.SetParent(boardObj.transform);
            boardUI.cluesContainer = cluesContainer.transform;
            boardUI.clueCardPrefab = null;

            MethodInfo renderMethod = typeof(DeductionBoardUI).GetMethod("RenderClueCards", BindingFlags.NonPublic | BindingFlags.Instance);
            renderMethod.Invoke(boardUI, new object[] { testCase });

            Assert.AreEqual(1, cluesContainer.transform.childCount);
            Transform child = cluesContainer.transform.GetChild(0);
            Assert.IsNotNull(child.GetComponent<Button>());
            Assert.IsNotNull(child.GetComponent<Image>());
            Text t = child.GetComponentInChildren<Text>();
            Assert.IsNotNull(t);
            Assert.IsTrue(t.text.Contains(testEvidence.evidenceName));
        }

        #endregion

        #region EvidenceInspectModal Tests

        [Test]
        public void EvidenceInspectModal_PopulateHotspots_UsesPrefabWhenAssigned()
        {
            GameObject modalObj = new GameObject("InspectModal");
            modalObj.transform.SetParent(testRoot.transform);
            EvidenceInspectModal modal = modalObj.AddComponent<EvidenceInspectModal>();

            GameObject containerObj = new GameObject("HotspotsContainer", typeof(RectTransform));
            containerObj.transform.SetParent(modalObj.transform);
            modal.hotspotsContainer = containerObj.GetComponent<RectTransform>();

            // Create hotspot marker prefab
            GameObject markerPrefab = new GameObject("HotspotMarkerPrefab", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelObj.transform.SetParent(markerPrefab.transform);
            markerPrefab.transform.SetParent(testRoot.transform);

            modal.hotspotMarkerPrefab = markerPrefab;

            MethodInfo populateMethod = typeof(EvidenceInspectModal).GetMethod("PopulateHotspots", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(populateMethod);
            populateMethod.Invoke(modal, new object[] { testEvidence });

            Assert.AreEqual(1, modal.hotspotsContainer.childCount);
            Transform spotChild = modal.hotspotsContainer.GetChild(0);
            Assert.AreEqual($"Hotspot_{testEvidence.hotspots[0].hotspotId}", spotChild.name);

            RectTransform rt = spotChild.GetComponent<RectTransform>();
            Assert.AreEqual(testEvidence.hotspots[0].normalizedPosition, rt.anchorMin);
            Assert.AreEqual(testEvidence.hotspots[0].normalizedPosition, rt.anchorMax);
            Assert.AreEqual(Vector2.zero, rt.anchoredPosition);

            Text label = spotChild.GetComponentInChildren<Text>();
            Assert.IsNotNull(label);
            Assert.AreEqual(testEvidence.hotspots[0].hotspotTitle, label.text);
        }

        [Test]
        public void EvidenceInspectModal_PopulateHotspots_FallsBackToProceduralWhenPrefabNull()
        {
            GameObject modalObj = new GameObject("InspectModal");
            modalObj.transform.SetParent(testRoot.transform);
            EvidenceInspectModal modal = modalObj.AddComponent<EvidenceInspectModal>();

            GameObject containerObj = new GameObject("HotspotsContainer", typeof(RectTransform));
            containerObj.transform.SetParent(modalObj.transform);
            modal.hotspotsContainer = containerObj.GetComponent<RectTransform>();
            modal.hotspotMarkerPrefab = null;

            MethodInfo populateMethod = typeof(EvidenceInspectModal).GetMethod("PopulateHotspots", BindingFlags.NonPublic | BindingFlags.Instance);
            populateMethod.Invoke(modal, new object[] { testEvidence });

            Assert.AreEqual(1, modal.hotspotsContainer.childCount);
            Transform spotChild = modal.hotspotsContainer.GetChild(0);
            Assert.AreEqual($"Hotspot_{testEvidence.hotspots[0].hotspotId}", spotChild.name);
            Assert.IsNotNull(spotChild.GetComponent<Button>());
            Assert.IsNotNull(spotChild.GetComponent<Image>());
        }

        #endregion

        #region ConclusionUI Tests

        [Test]
        public void ConclusionUI_RenderQuestionOptions_UsesPrefabsWhenAssigned()
        {
            GameObject conclusionObj = new GameObject("ConclusionUI");
            conclusionObj.transform.SetParent(testRoot.transform);
            ConclusionUI conclusionUI = conclusionObj.AddComponent<ConclusionUI>();

            GameObject gridObj = new GameObject("OptionsGrid", typeof(RectTransform));
            gridObj.transform.SetParent(conclusionObj.transform);
            conclusionUI.optionsGrid = gridObj.transform;

            // Create header and option prefabs
            GameObject headerPrefab = new GameObject("HeaderPrefab", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            headerPrefab.transform.SetParent(testRoot.transform);
            conclusionUI.questionHeaderPrefab = headerPrefab;

            GameObject optionPrefab = new GameObject("OptionPrefab", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Button));
            optionPrefab.transform.SetParent(testRoot.transform);
            conclusionUI.optionItemPrefab = optionPrefab;

            // Setup player answers list
            FieldInfo answersField = typeof(ConclusionUI).GetField("playerAnswers", BindingFlags.NonPublic | BindingFlags.Instance);
            List<int> answers = (List<int>)answersField.GetValue(conclusionUI);
            answers.Clear();
            answers.Add(-1);

            MethodInfo renderMethod = typeof(ConclusionUI).GetMethod("RenderQuestionOptions", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(renderMethod);
            renderMethod.Invoke(conclusionUI, new object[] { testCase });

            // 1 question with 3 options => 1 header + 3 options = 4 children
            Assert.AreEqual(4, gridObj.transform.childCount);
            Transform headerChild = gridObj.transform.GetChild(0);
            Assert.AreEqual("Header_Q0", headerChild.name);
            Text hText = headerChild.GetComponent<Text>();
            Assert.IsTrue(hText.text.Contains(testCase.conclusionQuestions[0].questionText));

            Transform opt0Child = gridObj.transform.GetChild(1);
            Assert.AreEqual("Opt_Q0_O0", opt0Child.name);
            Button btn0 = opt0Child.GetComponent<Button>();
            Assert.IsNotNull(btn0);

            // Trigger button click to test answer selection
            btn0.onClick.Invoke();
            Assert.AreEqual(0, answers[0]);
            Text opt0Text = opt0Child.GetComponent<Text>();
            Assert.IsTrue(opt0Text.text.Contains("[X]"));
        }

        [Test]
        public void ConclusionUI_RenderQuestionOptions_FallsBackToProceduralWhenPrefabsNull()
        {
            GameObject conclusionObj = new GameObject("ConclusionUI");
            conclusionObj.transform.SetParent(testRoot.transform);
            ConclusionUI conclusionUI = conclusionObj.AddComponent<ConclusionUI>();

            GameObject gridObj = new GameObject("OptionsGrid", typeof(RectTransform));
            gridObj.transform.SetParent(conclusionObj.transform);
            conclusionUI.optionsGrid = gridObj.transform;
            conclusionUI.questionHeaderPrefab = null;
            conclusionUI.optionItemPrefab = null;

            // Setup player answers list
            FieldInfo answersField = typeof(ConclusionUI).GetField("playerAnswers", BindingFlags.NonPublic | BindingFlags.Instance);
            List<int> answers = (List<int>)answersField.GetValue(conclusionUI);
            answers.Clear();
            answers.Add(-1);

            MethodInfo renderMethod = typeof(ConclusionUI).GetMethod("RenderQuestionOptions", BindingFlags.NonPublic | BindingFlags.Instance);
            renderMethod.Invoke(conclusionUI, new object[] { testCase });

            Assert.AreEqual(4, gridObj.transform.childCount);
            Transform headerChild = gridObj.transform.GetChild(0);
            Assert.AreEqual("Header_Q0", headerChild.name);
            Assert.IsTrue(headerChild.GetComponent<Text>().text.Contains(testCase.conclusionQuestions[0].questionText));

            Transform opt0Child = gridObj.transform.GetChild(1);
            Assert.AreEqual("Opt_Q0_O0", opt0Child.name);
            Button btn = opt0Child.GetComponent<Button>();
            Assert.IsNotNull(btn);
            btn.onClick.Invoke();
            Assert.AreEqual(0, answers[0]);
            Assert.IsTrue(opt0Child.GetComponent<Text>().text.Contains("[X]"));
        }

        #endregion
    }
}
