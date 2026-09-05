using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CaseClosed.Editor;
using CaseClosed.Enums;
using CaseClosed.Gameplay;
using CaseClosed.Managers;
using CaseClosed.Prototype;
using CaseClosed.UI;

namespace CaseClosed.Tests
{
    [TestFixture]
    public class PrefabGenerationUtilityTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            PrefabGenerationUtility.EnsureDirectoryStructure();
            PrefabGenerationUtility.GenerateAllPrefabsMenu();
        }

        #region Directory Tests

        [Test]
        public void DirectoryStructure_AllRequiredDirectoriesExist()
        {
            Assert.IsTrue(Directory.Exists(PrefabGenerationUtility.CoreDir), "Core directory must exist.");
            Assert.IsTrue(Directory.Exists(PrefabGenerationUtility.GameplayDir), "Gameplay directory must exist.");
            Assert.IsTrue(Directory.Exists(PrefabGenerationUtility.EvidenceDir), "Evidence directory must exist.");
            Assert.IsTrue(Directory.Exists(PrefabGenerationUtility.SuspectsDir), "Suspects directory must exist.");
            Assert.IsTrue(Directory.Exists(PrefabGenerationUtility.UIDir), "UI directory must exist.");
            Assert.IsTrue(Directory.Exists(PrefabGenerationUtility.PanelsDir), "Panels directory must exist.");
            Assert.IsTrue(Directory.Exists(PrefabGenerationUtility.ElementsDir), "Elements directory must exist.");
            Assert.IsTrue(Directory.Exists(PrefabGenerationUtility.VFXDir), "VFX directory must exist.");
        }

        #endregion

        #region UI Elements Tests

        [Test]
        public void UI_ClueCard_ConfiguredCorrectly()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.ElementsDir}/UI_ClueCard.prefab");
            Assert.IsNotNull(prefab, "UI_ClueCard.prefab must exist.");

            RectTransform rt = prefab.GetComponent<RectTransform>();
            Assert.IsNotNull(rt);
            Assert.AreEqual(new Vector2(280f, 60f), rt.sizeDelta);
            Assert.IsNotNull(prefab.GetComponent<Image>());
            Assert.IsNotNull(prefab.GetComponent<Button>());

            Transform textChild = prefab.transform.Find("Text");
            Assert.IsNotNull(textChild, "UI_ClueCard must have child Text.");
            Text t = textChild.GetComponent<Text>();
            Assert.IsNotNull(t);
            Assert.AreEqual(14, t.fontSize);
            Assert.IsNotNull(textChild.GetComponent<Shadow>(), "UI_ClueCard text must have Shadow.");
        }

        [Test]
        public void UI_EvidencePickerItem_ConfiguredCorrectly()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.ElementsDir}/UI_EvidencePickerItem.prefab");
            Assert.IsNotNull(prefab, "UI_EvidencePickerItem.prefab must exist.");

            RectTransform rt = prefab.GetComponent<RectTransform>();
            Assert.AreEqual(new Vector2(80f, 80f), rt.sizeDelta);
            Assert.IsNotNull(prefab.GetComponent<Image>());
            Assert.IsNotNull(prefab.GetComponent<Button>());

            Transform icon = prefab.transform.Find("Image_Icon");
            Assert.IsNotNull(icon, "Must have Image_Icon child.");
            Assert.IsNotNull(icon.GetComponent<Image>());

            Transform label = prefab.transform.Find("Text_Label");
            Assert.IsNotNull(label, "Must have Text_Label child.");
            Text t = label.GetComponent<Text>();
            Assert.IsNotNull(t);
            Assert.AreEqual(12, t.fontSize);
        }

        [Test]
        public void UI_HotspotMarker_ConfiguredCorrectly()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.ElementsDir}/UI_HotspotMarker.prefab");
            Assert.IsNotNull(prefab, "UI_HotspotMarker.prefab must exist.");

            RectTransform rt = prefab.GetComponent<RectTransform>();
            Assert.AreEqual(new Vector2(40f, 40f), rt.sizeDelta);
            Assert.IsNotNull(prefab.GetComponent<Image>());
            Assert.IsNotNull(prefab.GetComponent<Button>());
        }

        [Test]
        public void UI_ConclusionQuestionHeader_ConfiguredCorrectly()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.ElementsDir}/UI_ConclusionQuestionHeader.prefab");
            Assert.IsNotNull(prefab, "UI_ConclusionQuestionHeader.prefab must exist.");

            RectTransform rt = prefab.GetComponent<RectTransform>();
            Assert.AreEqual(new Vector2(500f, 40f), rt.sizeDelta);

            Text text = prefab.GetComponent<Text>();
            Assert.IsNotNull(text);
            Assert.AreEqual(18, text.fontSize);
            Assert.AreEqual(FontStyle.Bold, text.fontStyle);
            Assert.IsNotNull(prefab.GetComponent<Shadow>());
        }

        [Test]
        public void UI_ConclusionOptionItem_ConfiguredCorrectly()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.ElementsDir}/UI_ConclusionOptionItem.prefab");
            Assert.IsNotNull(prefab, "UI_ConclusionOptionItem.prefab must exist.");

            RectTransform rt = prefab.GetComponent<RectTransform>();
            Assert.AreEqual(new Vector2(480f, 36f), rt.sizeDelta);
            Assert.IsNotNull(prefab.GetComponent<Image>());
            Assert.IsNotNull(prefab.GetComponent<Button>());

            Transform textChild = prefab.transform.Find("Text_Option");
            Assert.IsNotNull(textChild);
            Text t = textChild.GetComponent<Text>();
            Assert.IsNotNull(t);
            Assert.AreEqual(16, t.fontSize);
            Assert.IsNotNull(textChild.GetComponent<Shadow>());
        }

        [Test]
        public void UI_CaseDossierCard_ConfiguredCorrectly()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.ElementsDir}/UI_CaseDossierCard.prefab");
            Assert.IsNotNull(prefab, "UI_CaseDossierCard.prefab must exist.");

            RectTransform rt = prefab.GetComponent<RectTransform>();
            Assert.AreEqual(new Vector2(220f, 260f), rt.sizeDelta);
            Assert.IsNotNull(prefab.GetComponent<Image>());
            Assert.IsNotNull(prefab.GetComponent<Button>());

            Assert.IsNotNull(prefab.transform.Find("Text_Title"));
            Assert.IsNotNull(prefab.transform.Find("Text_Status"));
        }

        #endregion

        #region UI Panels Tests

        [Test]
        public void Panel_HeaderNav_ConfiguredCorrectly()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.PanelsDir}/Panel_HeaderNav.prefab");
            Assert.IsNotNull(prefab);

            Transform timer = prefab.transform.Find("Container_Timer");
            Assert.IsNotNull(timer);
            Assert.IsNotNull(timer.GetComponent<CaseTimerUI>());

            Assert.IsNotNull(prefab.transform.Find("Button_Notebook"));
            Assert.IsNotNull(prefab.transform.Find("Button_DeductionBoard"));
            Assert.IsNotNull(prefab.transform.Find("Button_Conclude"));
            Assert.IsNotNull(prefab.transform.Find("Button_InvestigatorSelect"));
            Assert.IsNotNull(prefab.transform.Find("Button_Menu"));
        }

        [Test]
        public void Panel_Dialogue_ConfiguredCorrectly()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.PanelsDir}/Panel_Dialogue.prefab");
            Assert.IsNotNull(prefab);

            DialogueUI ui = prefab.GetComponent<DialogueUI>();
            Assert.IsNotNull(ui);
            Assert.IsNotNull(ui.speakerNameText);
            Assert.IsNotNull(ui.dialogueBodyText);
            Assert.IsNotNull(ui.nextButton);
            Assert.IsNotNull(ui.challengeButton);
            Assert.IsNotNull(ui.challengeHighlight);
            Assert.IsNotNull(ui.evidencePickerContainer);
            Assert.IsNotNull(ui.evidencePickerGrid);
            Assert.IsNotNull(ui.evidencePickerItemPrefab);
        }

        [Test]
        public void Panel_InspectModal_ConfiguredCorrectly()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.PanelsDir}/Panel_InspectModal.prefab");
            Assert.IsNotNull(prefab);

            EvidenceInspectModal modal = prefab.GetComponent<EvidenceInspectModal>();
            Assert.IsNotNull(modal);
            Assert.IsNotNull(modal.evidenceZoomImage);
            Assert.IsNotNull(modal.viewportRectTransform);
            Assert.IsNotNull(modal.hotspotsContainer);
            Assert.IsNotNull(modal.clueUnlockedNotificationText);
            Assert.IsNotNull(modal.hotspotMarkerPrefab);
        }

        [Test]
        public void Panel_DeductionBoard_ConfiguredCorrectly()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.PanelsDir}/Panel_DeductionBoard.prefab");
            Assert.IsNotNull(prefab);

            DeductionBoardUI ui = prefab.GetComponent<DeductionBoardUI>();
            Assert.IsNotNull(ui);
            Assert.IsNotNull(ui.boardTitleText);
            Assert.IsNotNull(ui.closeBoardButton);
            Assert.IsNotNull(ui.cluesContainer);
            Assert.IsNotNull(ui.clueCardPrefab);
            Assert.IsNotNull(ui.feedbackBanner);
            Assert.IsNotNull(ui.deductionsContainer);
        }

        [Test]
        public void Panel_ConclusionQuiz_ConfiguredCorrectly()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.PanelsDir}/Panel_ConclusionQuiz.prefab");
            Assert.IsNotNull(prefab);

            ConclusionUI ui = prefab.GetComponent<ConclusionUI>();
            Assert.IsNotNull(ui);
            Assert.IsNotNull(ui.quizContainer);
            Assert.IsNotNull(ui.questionTitleText);
            Assert.IsNotNull(ui.optionsGrid);
            Assert.IsNotNull(ui.questionHeaderPrefab);
            Assert.IsNotNull(ui.optionItemPrefab);
            Assert.IsNotNull(ui.submitConclusionButton);
            Assert.IsNotNull(ui.resultsContainer);
        }

        [Test]
        public void Panel_ResultsScreen_ConfiguredCorrectly()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.PanelsDir}/Panel_ResultsScreen.prefab");
            Assert.IsNotNull(prefab);

            Assert.IsNotNull(prefab.transform.Find("Card_Results/Text_Title"));
            Assert.IsNotNull(prefab.transform.Find("Card_Results/Text_Grade"));
            Assert.IsNotNull(prefab.transform.Find("Card_Results/Text_StarRating"));
            Assert.IsNotNull(prefab.transform.Find("Card_Results/Text_ScoreBreakdown"));
            Assert.IsNotNull(prefab.transform.Find("Card_Results/Button_Continue"));
            Assert.IsNotNull(prefab.transform.Find("Card_Results/Button_NextLevel"));
            Assert.IsNotNull(prefab.transform.Find("Card_Results/Button_MainMenu"));
        }

        [Test]
        public void Panel_GameOver_ConfiguredCorrectly()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.PanelsDir}/Panel_GameOver.prefab");
            Assert.IsNotNull(prefab);

            GameOverUI ui = prefab.GetComponent<GameOverUI>();
            Assert.IsNotNull(ui);

            var sGo = new SerializedObject(ui);
            Assert.IsNotNull(sGo.FindProperty("titleText").objectReferenceValue);
            Assert.IsNotNull(sGo.FindProperty("subtitleText").objectReferenceValue);
            Assert.IsNotNull(sGo.FindProperty("detailsBreakdownText").objectReferenceValue);
            Assert.IsNotNull(sGo.FindProperty("retryButton").objectReferenceValue);
            Assert.IsNotNull(sGo.FindProperty("returnToMainMenuButton").objectReferenceValue);
        }

        [Test]
        public void Panel_InGameMenu_ConfiguredCorrectly()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.PanelsDir}/Panel_InGameMenu.prefab");
            Assert.IsNotNull(prefab);

            Assert.IsNotNull(prefab.transform.Find("Card_InGameMenu/Button_Resume"));
            Assert.IsNotNull(prefab.transform.Find("Card_InGameMenu/Button_MainMenu"));
            Assert.IsNotNull(prefab.transform.Find("Panel_MainMenuConfirm/Card_Confirm/Button_ConfirmYes"));
            Assert.IsNotNull(prefab.transform.Find("Panel_MainMenuConfirm/Card_Confirm/Button_ConfirmNo"));
        }

        [Test]
        public void Panel_InvestigatorSelect_ConfiguredCorrectly()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.PanelsDir}/Panel_InvestigatorSelect.prefab");
            Assert.IsNotNull(prefab);

            InvestigatorSelectionUI ui = prefab.GetComponent<InvestigatorSelectionUI>();
            Assert.IsNotNull(ui);
            Assert.IsNotNull(ui.selectKyleButton);
            Assert.IsNotNull(ui.selectMiguelButton);
            Assert.IsNotNull(ui.level1Button);
            Assert.IsNotNull(ui.level2Button);
            Assert.IsNotNull(ui.level3Button);
            Assert.IsNotNull(ui.closeSelectionButton);
        }

        #endregion

        #region Master Canvas Tests

        [Test]
        public void Canvas_MainUI_ConfiguredCorrectly()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.UIDir}/Canvas_MainUI.prefab");
            Assert.IsNotNull(prefab);

            Canvas canvas = prefab.GetComponent<Canvas>();
            Assert.IsNotNull(canvas);
            Assert.IsTrue(canvas.pixelPerfect);

            CanvasScaler scaler = prefab.GetComponent<CanvasScaler>();
            Assert.IsNotNull(scaler);
            Assert.AreEqual(new Vector2(1920f, 1080f), scaler.referenceResolution);

            UIManager ui = prefab.GetComponent<UIManager>();
            Assert.IsNotNull(ui);
            Assert.IsNotNull(ui.mainTablePanel);
            Assert.IsNotNull(ui.inspectModalPanel);
            Assert.IsNotNull(ui.notebookPanel);
            Assert.IsNotNull(ui.deductionBoardPanel);
            Assert.IsNotNull(ui.conclusionQuizPanel);
            Assert.IsNotNull(ui.resultsScreenPanel);
            Assert.IsNotNull(ui.investigatorSelectPanel);
            Assert.IsNotNull(ui.gameOverPanel);
            Assert.IsNotNull(ui.inGameMenuPanel);
            Assert.IsNotNull(ui.timerContainer);
            Assert.IsNotNull(ui.notebookButton);
            Assert.IsNotNull(ui.deductionBoardButton);
            Assert.IsNotNull(ui.concludeCaseButton);
            Assert.IsNotNull(ui.investigatorSelectButton);
            Assert.IsNotNull(ui.returnToMenuButton);
        }

        #endregion

        #region Gameplay Actors Tests

        [Test]
        public void TableEvidence_BaseAndVariants_ConfiguredCorrectly()
        {
            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.GameplayDir}/Actor_TableEvidence_Base.prefab");
            Assert.IsNotNull(basePrefab);
            Assert.IsNotNull(basePrefab.GetComponent<SpriteRenderer>());
            Assert.IsNotNull(basePrefab.GetComponent<BoxCollider2D>());
            Assert.IsNotNull(basePrefab.GetComponent<TableEvidenceItem>());
            Assert.IsNotNull(basePrefab.transform.Find("Glow_Halo"));

            string[] variants = new string[]
            {
                "TableEvidence_Photograph",
                "TableEvidence_Document",
                "TableEvidence_PhysicalClue",
                "TableEvidence_OpenNotebook"
            };

            foreach (var v in variants)
            {
                GameObject vPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.EvidenceDir}/{v}.prefab");
                Assert.IsNotNull(vPrefab, $"Variant {v} must exist in EvidenceDir.");
                Assert.IsNotNull(vPrefab.GetComponent<TableEvidenceItem>());
            }
        }

        [Test]
        public void SuspectSlot_BaseAndVariants_ConfiguredCorrectly()
        {
            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.GameplayDir}/Actor_SuspectSlot_Base.prefab");
            Assert.IsNotNull(basePrefab);
            Assert.IsNotNull(basePrefab.GetComponent<SpriteRenderer>());
            Assert.IsNotNull(basePrefab.GetComponent<CharacterDisplay>());

            GameObject left = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.SuspectsDir}/SuspectSlot_Left.prefab");
            Assert.IsNotNull(left);
            Assert.AreEqual(CharacterSlot.PrimarySuspect, left.GetComponent<CharacterDisplay>().characterSlot);

            GameObject right = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.SuspectsDir}/SuspectSlot_Right.prefab");
            Assert.IsNotNull(right);
            Assert.AreEqual(CharacterSlot.SecondarySuspect, right.GetComponent<CharacterDisplay>().characterSlot);
        }

        #endregion

        #region Core Rigs Tests

        [Test]
        public void Core_Managers_ConfiguredCorrectly()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.CoreDir}/Core_Managers.prefab");
            Assert.IsNotNull(prefab);
            Assert.IsNotNull(prefab.GetComponent<AudioManager>());
            Assert.IsNotNull(prefab.GetComponent<CaseManager>());
            Assert.IsNotNull(prefab.GetComponent<EvidenceManager>());
            Assert.IsNotNull(prefab.GetComponent<InterrogationManager>());
            Assert.IsNotNull(prefab.GetComponent<DeductionBoardController>());
            Assert.IsNotNull(prefab.GetComponent<CaseConclusionManager>());
            Assert.IsNotNull(prefab.GetComponent<GameBootstrap>());
            Assert.AreEqual(3, prefab.GetComponents<AudioSource>().Length);
        }

        [Test]
        public void Core_InvestigationCamera_ConfiguredCorrectly()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.CoreDir}/Core_InvestigationCamera.prefab");
            Assert.IsNotNull(prefab);
            Assert.AreEqual("MainCamera", prefab.tag);
            Assert.IsNotNull(prefab.GetComponent<Camera>());
            Assert.IsNotNull(prefab.GetComponent<AudioListener>());
            Assert.IsNotNull(prefab.GetComponent<FixedInvestigationCamera>());
            Assert.IsNotNull(prefab.GetComponent<Physics2DRaycaster>());
        }

        [Test]
        public void Core_DetectiveArmPointer_ConfiguredCorrectly()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.CoreDir}/Core_DetectiveArmPointer.prefab");
            Assert.IsNotNull(prefab);
            Assert.IsNotNull(prefab.GetComponent<SpriteRenderer>());

            ArmPointerController apc = prefab.GetComponent<ArmPointerController>();
            Assert.IsNotNull(apc);
            Assert.IsNotNull(apc.fingertipPoint);
        }

        #endregion

        #region VFX Tests

        [Test]
        public void VFX_Prefabs_ConfiguredCorrectly()
        {
            GameObject banner = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.VFXDir}/FX_ClueDiscoveredBanner.prefab");
            Assert.IsNotNull(banner);
            Assert.IsNotNull(banner.GetComponent<CanvasGroup>());

            GameObject burst = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerationUtility.VFXDir}/FX_ContradictionBurst.prefab");
            Assert.IsNotNull(burst);
            Assert.IsNotNull(burst.GetComponent<CanvasGroup>());
        }

        #endregion
    }
}
