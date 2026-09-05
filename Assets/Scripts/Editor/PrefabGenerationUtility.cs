using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Gameplay;
using CaseClosed.Managers;
using CaseClosed.Prototype;
using CaseClosed.UI;

namespace CaseClosed.Editor
{
    /// <summary>
    /// Master Editor utility script for programmatically generating, configuring, and saving all planned
    /// Prefabs under Assets/Prefabs/ and wiring them seamlessly across all investigation scenes and the main menu.
    /// </summary>
    public static class PrefabGenerationUtility
    {
        // Directory paths
        public const string PrefabsRoot = "Assets/Prefabs";
        public const string CoreDir = "Assets/Prefabs/Core";
        public const string GameplayDir = "Assets/Prefabs/Gameplay";
        public const string EvidenceDir = "Assets/Prefabs/Gameplay/Evidence";
        public const string SuspectsDir = "Assets/Prefabs/Gameplay/Suspects";
        public const string UIDir = "Assets/Prefabs/UI";
        public const string PanelsDir = "Assets/Prefabs/UI/Panels";
        public const string ElementsDir = "Assets/Prefabs/UI/Elements";
        public const string VFXDir = "Assets/Prefabs/VFX";

        private static readonly string[] TargetScenes = new string[]
        {
            "Assets/Scenes/Case001.unity",
            "Assets/Scenes/Case002.unity",
            "Assets/Scenes/Case003.unity",
            "Assets/Scenes/Main.unity",
            "Assets/Scenes/MainMenu.unity"
        };

        private static Font s_defaultFont;

        private static Font GetDefaultFont()
        {
            if (s_defaultFont == null)
            {
                s_defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            return s_defaultFont;
        }

        #region Menu Items

        [MenuItem("Case Closed/Prefabs/Generate All Prefabs", false, 30)]
        public static void GenerateAllPrefabsMenu()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Generating Prefabs", "Creating directories...", 0.05f);
                EnsureDirectoryStructure();

                EditorUtility.DisplayProgressBar("Generating Prefabs", "Building UI Elements...", 0.15f);
                GenerateUIElements();

                EditorUtility.DisplayProgressBar("Generating Prefabs", "Building UI Panels...", 0.40f);
                GenerateUIPanels();

                EditorUtility.DisplayProgressBar("Generating Prefabs", "Building Master Canvas...", 0.60f);
                GenerateMasterCanvasPrefab();

                EditorUtility.DisplayProgressBar("Generating Prefabs", "Building Gameplay Actors...", 0.75f);
                GenerateWorldGameplayActors();

                EditorUtility.DisplayProgressBar("Generating Prefabs", "Building Core Rigs...", 0.85f);
                GenerateCoreRigs();

                EditorUtility.DisplayProgressBar("Generating Prefabs", "Building VFX...", 0.95f);
                GenerateVFXPrefabs();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("<color=green><b>[PrefabGen] All planned Prefabs generated and saved successfully!</b></color>");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PrefabGen] Failed to generate all prefabs: {ex}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        [MenuItem("Case Closed/Prefabs/Apply Prefabs To All Scenes", false, 31)]
        public static void ApplyPrefabsToAllScenesMenu()
        {
            string initialScene = EditorSceneManager.GetActiveScene().path;
            int processedCount = 0;

            try
            {
                EnsureDirectoryStructure();
                GenerateAllPrefabsMenu();

                for (int i = 0; i < TargetScenes.Length; i++)
                {
                    string scenePath = TargetScenes[i];
                    if (!File.Exists(scenePath))
                    {
                        Debug.LogWarning($"[PrefabGen] Scene file not found at: '{scenePath}', skipping.");
                        continue;
                    }

                    EditorUtility.DisplayProgressBar("Wiring Prefabs To Scenes", $"Configuring '{scenePath}'...", (float)i / TargetScenes.Length);
                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                    bool modified = ApplyPrefabsToCurrentScene(scenePath);
                    if (modified)
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                        processedCount++;
                        Debug.Log($"[PrefabGen] Successfully wired prefabs to scene: '{scenePath}'");
                    }
                }

                if (!string.IsNullOrEmpty(initialScene) && File.Exists(initialScene))
                {
                    EditorSceneManager.OpenScene(initialScene, OpenSceneMode.Single);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"<color=green><b>[PrefabGen] Successfully applied and wired prefabs across {processedCount} scene(s)!</b></color>");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PrefabGen] Error applying prefabs to scenes: {ex}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        #endregion

        #region Directory Structure

        public static void EnsureDirectoryStructure()
        {
            EnsureDirectory(PrefabsRoot);
            EnsureDirectory(CoreDir);
            EnsureDirectory(GameplayDir);
            EnsureDirectory(EvidenceDir);
            EnsureDirectory(SuspectsDir);
            EnsureDirectory(UIDir);
            EnsureDirectory(PanelsDir);
            EnsureDirectory(ElementsDir);
            EnsureDirectory(VFXDir);
            AssetDatabase.Refresh();
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        #endregion

        #region A. Dynamic UI Elements

        public static void GenerateUIElements()
        {
            GenerateClueCardPrefab();
            GenerateEvidencePickerItemPrefab();
            GenerateHotspotMarkerPrefab();
            GenerateConclusionQuestionHeaderPrefab();
            GenerateConclusionOptionItemPrefab();
            GenerateCaseDossierCardPrefab();
        }

        private static void GenerateClueCardPrefab()
        {
            string path = $"{ElementsDir}/UI_ClueCard.prefab";
            GameObject root = new GameObject("UI_ClueCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(280f, 60f);

            Image img = root.GetComponent<Image>();
            img.color = new Color(0.20f, 0.22f, 0.28f, 0.90f);

            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Shadow));
            textObj.transform.SetParent(root.transform, false);

            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(10f, 5f);
            textRt.offsetMax = new Vector2(-10f, -5f);

            Text text = textObj.GetComponent<Text>();
            text.font = GetDefaultFont();
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            text.text = "<b>Clue Title</b>\nClue description details...";

            Shadow shadow = textObj.GetComponent<Shadow>();
            shadow.effectDistance = new Vector2(1.2f, -1.2f);
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);

            SaveAndDestroy(root, path);
        }

        private static void GenerateEvidencePickerItemPrefab()
        {
            string path = $"{ElementsDir}/UI_EvidencePickerItem.prefab";
            GameObject root = new GameObject("UI_EvidencePickerItem", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80f, 80f);

            Image frameImg = root.GetComponent<Image>();
            frameImg.color = new Color(0.18f, 0.20f, 0.25f, 0.95f);

            // Child Icon Image
            GameObject iconObj = new GameObject("Image_Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObj.transform.SetParent(root.transform, false);
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.5f, 0.5f);
            iconRt.anchorMax = new Vector2(0.5f, 0.5f);
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.anchoredPosition = new Vector2(0f, 8f);
            iconRt.sizeDelta = new Vector2(56f, 56f);

            Image iconImg = iconObj.GetComponent<Image>();
            iconImg.color = Color.white;
            iconImg.raycastTarget = false;

            // Child Text Label
            GameObject labelObj = new GameObject("Text_Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Shadow));
            labelObj.transform.SetParent(root.transform, false);
            RectTransform labelRt = labelObj.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0f, 0f);
            labelRt.anchorMax = new Vector2(1f, 0f);
            labelRt.pivot = new Vector2(0.5f, 0f);
            labelRt.anchoredPosition = new Vector2(0f, 2f);
            labelRt.sizeDelta = new Vector2(0f, 18f);

            Text labelText = labelObj.GetComponent<Text>();
            labelText.font = GetDefaultFont();
            labelText.fontSize = 12;
            labelText.fontStyle = FontStyle.Bold;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = Color.white;
            labelText.text = "Evidence";
            labelText.raycastTarget = false;

            Shadow shadow = labelObj.GetComponent<Shadow>();
            shadow.effectDistance = new Vector2(1f, -1f);
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);

            SaveAndDestroy(root, path);
        }

        private static void GenerateHotspotMarkerPrefab()
        {
            string path = $"{ElementsDir}/UI_HotspotMarker.prefab";
            GameObject root = new GameObject("UI_HotspotMarker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(40f, 40f);

            Image ringImg = root.GetComponent<Image>();
            ringImg.color = new Color(0.95f, 0.75f, 0.15f, 0.85f);

            // Child center target
            GameObject innerObj = new GameObject("Image_TargetCenter", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            innerObj.transform.SetParent(root.transform, false);
            RectTransform innerRt = innerObj.GetComponent<RectTransform>();
            innerRt.anchorMin = new Vector2(0.5f, 0.5f);
            innerRt.anchorMax = new Vector2(0.5f, 0.5f);
            innerRt.sizeDelta = new Vector2(16f, 16f);

            Image innerImg = innerObj.GetComponent<Image>();
            innerImg.color = new Color(1f, 0.92f, 0.3f, 0.95f);
            innerImg.raycastTarget = false;

            SaveAndDestroy(root, path);
        }

        private static void GenerateConclusionQuestionHeaderPrefab()
        {
            string path = $"{ElementsDir}/UI_ConclusionQuestionHeader.prefab";
            GameObject root = new GameObject("UI_ConclusionQuestionHeader", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Shadow));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500f, 40f);

            Text text = root.GetComponent<Text>();
            text.font = GetDefaultFont();
            text.fontSize = 18;
            text.fontStyle = FontStyle.Bold;
            text.color = new Color(0.95f, 0.82f, 0.45f, 1f); // Gold
            text.alignment = TextAnchor.MiddleLeft;
            text.text = "1. Question Title Header";

            Shadow shadow = root.GetComponent<Shadow>();
            shadow.effectDistance = new Vector2(1.2f, -1.2f);
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);

            SaveAndDestroy(root, path);
        }

        private static void GenerateConclusionOptionItemPrefab()
        {
            string path = $"{ElementsDir}/UI_ConclusionOptionItem.prefab";
            GameObject root = new GameObject("UI_ConclusionOptionItem", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(480f, 36f);

            Image bg = root.GetComponent<Image>();
            bg.color = new Color(0.15f, 0.17f, 0.22f, 0.85f);

            GameObject textObj = new GameObject("Text_Option", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Shadow));
            textObj.transform.SetParent(root.transform, false);

            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(12f, 0f);
            textRt.offsetMax = new Vector2(-12f, 0f);

            Text text = textObj.GetComponent<Text>();
            text.font = GetDefaultFont();
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            text.text = "[ ] Selectable conclusion option choice";

            Shadow shadow = textObj.GetComponent<Shadow>();
            shadow.effectDistance = new Vector2(1.2f, -1.2f);
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);

            SaveAndDestroy(root, path);
        }

        private static void GenerateCaseDossierCardPrefab()
        {
            string path = $"{ElementsDir}/UI_CaseDossierCard.prefab";
            GameObject root = new GameObject("UI_CaseDossierCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(220f, 260f);

            Image cardBg = root.GetComponent<Image>();
            cardBg.color = new Color(0.22f, 0.18f, 0.15f, 0.95f);

            // Inset art placeholder
            GameObject thumbObj = new GameObject("Image_Thumbnail", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            thumbObj.transform.SetParent(root.transform, false);
            RectTransform thumbRt = thumbObj.GetComponent<RectTransform>();
            thumbRt.anchorMin = new Vector2(0.5f, 1f);
            thumbRt.anchorMax = new Vector2(0.5f, 1f);
            thumbRt.pivot = new Vector2(0.5f, 1f);
            thumbRt.anchoredPosition = new Vector2(0f, -16f);
            thumbRt.sizeDelta = new Vector2(188f, 120f);
            Image thumbImg = thumbObj.GetComponent<Image>();
            thumbImg.color = new Color(0.12f, 0.10f, 0.08f, 0.90f);
            thumbImg.raycastTarget = false;

            // Title
            AddTextChild(root.transform, "Text_Title", "CASE 01: THE MISSING NECKLACE", 15, FontStyle.Bold,
                TextAnchor.MiddleCenter, new Color(0.95f, 0.82f, 0.45f, 1f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -15f), new Vector2(195f, 48f), true);

            // Status
            AddTextChild(root.transform, "Text_Status", "STATUS: UNLOCKED", 13, FontStyle.Normal,
                TextAnchor.MiddleCenter, new Color(0.80f, 0.88f, 0.80f, 1f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 22f), new Vector2(190f, 28f), true);

            SaveAndDestroy(root, path);
        }

        #endregion

        #region B. Major UI Panels

        public static void GenerateUIPanels()
        {
            GenerateHeaderNavPanelPrefab();
            GenerateDialoguePanelPrefab();
            GenerateInspectModalPanelPrefab();
            GenerateDeductionBoardPanelPrefab();
            GenerateConclusionQuizPanelPrefab();
            GenerateResultsScreenPanelPrefab();
            GenerateGameOverPanelPrefab();
            GenerateInGameMenuPanelPrefab();
            GenerateInvestigatorSelectPanelPrefab();
            EnsureCaseFileNotebookPanelPrefab();
        }

        private static void GenerateHeaderNavPanelPrefab()
        {
            string path = $"{PanelsDir}/Panel_HeaderNav.prefab";
            GameObject root = new GameObject("Panel_HeaderNav", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0f, 64f);

            Image bg = root.GetComponent<Image>();
            bg.color = new Color(0.08f, 0.10f, 0.14f, 0.94f);
            bg.raycastTarget = true;

            // 1. Timer Container
            GameObject timerObj = new GameObject("Container_Timer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CaseTimerUI));
            timerObj.transform.SetParent(root.transform, false);
            RectTransform timerRt = timerObj.GetComponent<RectTransform>();
            timerRt.anchorMin = new Vector2(0.5f, 0.5f);
            timerRt.anchorMax = new Vector2(0.5f, 0.5f);
            timerRt.pivot = new Vector2(0.5f, 0.5f);
            timerRt.anchoredPosition = Vector2.zero;
            timerRt.sizeDelta = new Vector2(160f, 44f);

            Image timerBg = timerObj.GetComponent<Image>();
            timerBg.color = new Color(0.10f, 0.12f, 0.16f, 0.95f);
            timerBg.raycastTarget = false;

            Text timerText = AddTextChild(timerObj.transform, "Text_Timer", "05:00", 22, FontStyle.Bold,
                TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, true);

            CaseTimerUI timerUI = timerObj.GetComponent<CaseTimerUI>();
            var sTimer = new SerializedObject(timerUI);
            sTimer.FindProperty("timerText").objectReferenceValue = timerText;
            sTimer.FindProperty("backgroundBadge").objectReferenceValue = timerBg;
            sTimer.ApplyModifiedProperties();

            // 2. Buttons Left
            AddButtonChild(root.transform, "Button_Notebook", "Notebook", 14, new Color(0.18f, 0.32f, 0.45f, 0.95f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-380f, 0f), new Vector2(120f, 42f));

            AddButtonChild(root.transform, "Button_DeductionBoard", "Deductions", 14, new Color(0.24f, 0.34f, 0.48f, 0.95f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-240f, 0f), new Vector2(136f, 42f));

            // 3. Buttons Right
            AddButtonChild(root.transform, "Button_Conclude", "Conclude", 14, new Color(0.48f, 0.32f, 0.18f, 0.95f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(240f, 0f), new Vector2(130f, 42f));

            AddButtonChild(root.transform, "Button_InvestigatorSelect", "Detectives", 14, new Color(0.28f, 0.28f, 0.36f, 0.95f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(380f, 0f), new Vector2(130f, 42f));

            AddButtonChild(root.transform, "Button_Menu", "Menu", 14, new Color(0.38f, 0.22f, 0.22f, 0.95f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(510f, 0f), new Vector2(100f, 42f));

            SaveAndDestroy(root, path);
        }

        private static void GenerateDialoguePanelPrefab()
        {
            string path = $"{PanelsDir}/Panel_Dialogue.prefab";
            GameObject root = new GameObject("Panel_Dialogue", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(DialogueUI));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 25f);
            rt.sizeDelta = new Vector2(1120f, 220f);

            Image bg = root.GetComponent<Image>();
            bg.color = new Color(0.06f, 0.08f, 0.12f, 0.96f);
            bg.raycastTarget = true;

            DialogueUI dialogueUI = root.GetComponent<DialogueUI>();

            // Speaker Name
            Text speakerText = AddTextChild(root.transform, "Text_SpeakerName", "Speaker Name", 18, FontStyle.Bold,
                TextAnchor.MiddleLeft, new Color(0.95f, 0.82f, 0.45f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(30f, -14f), new Vector2(380f, 32f), true);

            // Dialogue Body
            Text bodyText = AddTextChild(root.transform, "Text_DialogueBody", "Dialogue statement line appears here...", 16, FontStyle.Normal,
                TextAnchor.UpperLeft, Color.white,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                new Vector2(0f, 0f), new Vector2(-60f, -110f), true);
            bodyText.rectTransform.offsetMin = new Vector2(30f, 60f);
            bodyText.rectTransform.offsetMax = new Vector2(-30f, -50f);

            // Action Buttons
            Button nextBtn = AddButtonChild(root.transform, "Button_Next", "Next", 15, new Color(0.18f, 0.45f, 0.25f, 0.95f),
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-30f, 15f), new Vector2(130f, 40f));

            Button challengeBtn = AddButtonChild(root.transform, "Button_Challenge", "Challenge", 15, new Color(0.72f, 0.24f, 0.18f, 0.95f),
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-175f, 15f), new Vector2(140f, 40f));

            Button closeBtn = AddButtonChild(root.transform, "Button_Close", "X", 14, new Color(0.35f, 0.35f, 0.38f, 0.95f),
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-15f, -15f), new Vector2(36f, 36f));

            // Challenge Highlight border
            GameObject highlightObj = new GameObject("Highlight_Challenge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            highlightObj.transform.SetParent(root.transform, false);
            RectTransform hlRt = highlightObj.GetComponent<RectTransform>();
            hlRt.anchorMin = Vector2.zero;
            hlRt.anchorMax = Vector2.one;
            hlRt.offsetMin = new Vector2(-4f, -4f);
            hlRt.offsetMax = new Vector2(4f, 4f);
            Image hlImg = highlightObj.GetComponent<Image>();
            hlImg.color = new Color(1f, 0.2f, 0.2f, 0.85f);
            hlImg.raycastTarget = false;
            highlightObj.SetActive(false);

            // Evidence Picker Drawer
            GameObject drawerObj = new GameObject("Drawer_EvidencePicker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            drawerObj.transform.SetParent(root.transform, false);
            RectTransform drawerRt = drawerObj.GetComponent<RectTransform>();
            drawerRt.anchorMin = new Vector2(0.5f, 1f);
            drawerRt.anchorMax = new Vector2(0.5f, 1f);
            drawerRt.pivot = new Vector2(0.5f, 0f);
            drawerRt.anchoredPosition = new Vector2(0f, 10f);
            drawerRt.sizeDelta = new Vector2(820f, 120f);

            Image drawerBg = drawerObj.GetComponent<Image>();
            drawerBg.color = new Color(0.10f, 0.12f, 0.16f, 0.98f);
            drawerBg.raycastTarget = true;

            GameObject gridObj = new GameObject("Grid_Evidence", typeof(RectTransform), typeof(GridLayoutGroup));
            gridObj.transform.SetParent(drawerObj.transform, false);
            RectTransform gridRt = gridObj.GetComponent<RectTransform>();
            gridRt.anchorMin = Vector2.zero;
            gridRt.anchorMax = Vector2.one;
            gridRt.offsetMin = new Vector2(12f, 10f);
            gridRt.offsetMax = new Vector2(-12f, -10f);

            GridLayoutGroup glg = gridObj.GetComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(80f, 80f);
            glg.spacing = new Vector2(10f, 10f);
            glg.childAlignment = TextAnchor.MiddleLeft;

            drawerObj.SetActive(false);

            // Wire DialogueUI
            dialogueUI.speakerNameText = speakerText;
            dialogueUI.dialogueBodyText = bodyText;
            dialogueUI.nextButton = nextBtn;
            dialogueUI.challengeButton = challengeBtn;
            dialogueUI.closeDialogueButton = closeBtn;
            dialogueUI.challengeHighlight = highlightObj;
            dialogueUI.evidencePickerContainer = drawerObj;
            dialogueUI.evidencePickerGrid = gridObj.transform;
            dialogueUI.evidencePickerItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ElementsDir}/UI_EvidencePickerItem.prefab");

            SaveAndDestroy(root, path);
        }

        private static void GenerateInspectModalPanelPrefab()
        {
            string path = $"{PanelsDir}/Panel_InspectModal.prefab";
            GameObject root = new GameObject("Panel_InspectModal", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(EvidenceInspectModal));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = root.GetComponent<Image>();
            bg.color = Color.clear;
            bg.raycastTarget = true;

            EvidenceInspectModal modal = root.GetComponent<EvidenceInspectModal>();

            // Viewport
            GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform));
            viewportObj.transform.SetParent(root.transform, false);
            RectTransform vpRt = viewportObj.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = Vector2.zero;
            vpRt.offsetMax = Vector2.zero;

            // Zoom image
            GameObject zoomObj = new GameObject("Image_Zoom", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            zoomObj.transform.SetParent(viewportObj.transform, false);
            RectTransform zoomRt = zoomObj.GetComponent<RectTransform>();
            zoomRt.anchorMin = new Vector2(0.5f, 0.5f);
            zoomRt.anchorMax = new Vector2(0.5f, 0.5f);
            zoomRt.pivot = new Vector2(0.5f, 0.5f);
            zoomRt.anchoredPosition = Vector2.zero;
            zoomRt.sizeDelta = new Vector2(600f, 600f);

            Image zoomImg = zoomObj.GetComponent<Image>();
            zoomImg.color = Color.white;
            zoomImg.raycastTarget = true;

            // Hotspots container
            GameObject hotspotsObj = new GameObject("Container_Hotspots", typeof(RectTransform));
            hotspotsObj.transform.SetParent(zoomObj.transform, false);
            RectTransform hsRt = hotspotsObj.GetComponent<RectTransform>();
            hsRt.anchorMin = Vector2.zero;
            hsRt.anchorMax = Vector2.one;
            hsRt.offsetMin = Vector2.zero;
            hsRt.offsetMax = Vector2.zero;

            // Notification text
            Text notifText = AddTextChild(root.transform, "Text_ClueNotification", "[NEW CLUE DISCOVERED]\nObservation text goes here...",
                16, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.95f, 0.85f, 0.35f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -80f), new Vector2(640f, 60f), true);
            notifText.gameObject.SetActive(false);

            modal.evidenceZoomImage = zoomImg;
            modal.viewportRectTransform = vpRt;
            modal.hotspotsContainer = hsRt;
            modal.clueUnlockedNotificationText = notifText;
            modal.hotspotMarkerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ElementsDir}/UI_HotspotMarker.prefab");

            SaveAndDestroy(root, path);
        }

        private static void GenerateDeductionBoardPanelPrefab()
        {
            string path = $"{PanelsDir}/Panel_DeductionBoard.prefab";
            GameObject root = new GameObject("Panel_DeductionBoard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(DeductionBoardUI));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = root.GetComponent<Image>();
            bg.color = new Color(0.10f, 0.08f, 0.08f, 0.96f);
            bg.raycastTarget = true;

            DeductionBoardUI deductionUI = root.GetComponent<DeductionBoardUI>();

            // Header Title
            Text title = AddTextChild(root.transform, "Text_Title", "MIND PALACE - DEDUCTION BOARD", 24, FontStyle.Bold,
                TextAnchor.MiddleCenter, new Color(0.95f, 0.82f, 0.45f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -24f), new Vector2(600f, 40f), true);

            // Close Button
            Button closeBtn = AddButtonChild(root.transform, "Button_Close", "Return to Desk", 14, new Color(0.35f, 0.35f, 0.38f, 0.95f),
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-30f, -22f), new Vector2(150f, 40f));

            // Selection Status Text
            Text statusText = AddTextChild(root.transform, "Text_SelectionStatus", "Select two clues to form a deduction connection.", 15, FontStyle.Italic,
                TextAnchor.MiddleCenter, new Color(0.85f, 0.85f, 0.85f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -70f), new Vector2(700f, 30f), true);

            // Scroll container for clues
            GameObject scrollObj = new GameObject("Container_CluesScroll", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect));
            scrollObj.transform.SetParent(root.transform, false);
            RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0.04f, 0.06f);
            scrollRt.anchorMax = new Vector2(0.62f, 0.88f);
            scrollRt.offsetMin = Vector2.zero;
            scrollRt.offsetMax = Vector2.zero;

            Image scrollBg = scrollObj.GetComponent<Image>();
            scrollBg.color = new Color(0.06f, 0.05f, 0.05f, 0.70f);

            ScrollRect sr = scrollObj.GetComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;

            GameObject cluesContent = new GameObject("Content_Clues", typeof(RectTransform), typeof(GridLayoutGroup));
            cluesContent.transform.SetParent(scrollObj.transform, false);
            RectTransform cluesRt = cluesContent.GetComponent<RectTransform>();
            cluesRt.anchorMin = new Vector2(0f, 1f);
            cluesRt.anchorMax = new Vector2(1f, 1f);
            cluesRt.pivot = new Vector2(0.5f, 1f);
            cluesRt.anchoredPosition = Vector2.zero;
            cluesRt.sizeDelta = new Vector2(0f, 600f);

            GridLayoutGroup glg = cluesContent.GetComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(280f, 60f);
            glg.spacing = new Vector2(14f, 14f);
            glg.padding = new RectOffset(16, 16, 16, 16);
            sr.content = cluesRt;

            // Feedback Banner
            GameObject bannerObj = new GameObject("Banner_Feedback", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bannerObj.transform.SetParent(root.transform, false);
            RectTransform bannerRt = bannerObj.GetComponent<RectTransform>();
            bannerRt.anchorMin = new Vector2(0.5f, 0.5f);
            bannerRt.anchorMax = new Vector2(0.5f, 0.5f);
            bannerRt.sizeDelta = new Vector2(520f, 70f);
            Image bannerImg = bannerObj.GetComponent<Image>();
            bannerImg.color = new Color(0.18f, 0.45f, 0.25f, 0.95f);

            Text feedbackText = AddTextChild(bannerObj.transform, "Text_Feedback", "Valid Deduction Established!", 16, FontStyle.Bold,
                TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, true);
            bannerObj.SetActive(false);

            // Deductions panel on right
            GameObject deductionsObj = new GameObject("Container_Deductions", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            deductionsObj.transform.SetParent(root.transform, false);
            RectTransform dedRt = deductionsObj.GetComponent<RectTransform>();
            dedRt.anchorMin = new Vector2(0.64f, 0.06f);
            dedRt.anchorMax = new Vector2(0.96f, 0.88f);
            dedRt.offsetMin = Vector2.zero;
            dedRt.offsetMax = Vector2.zero;

            Image dedBg = deductionsObj.GetComponent<Image>();
            dedBg.color = new Color(0.14f, 0.11f, 0.09f, 0.90f);

            Text completedBody = AddTextChild(deductionsObj.transform, "Text_CompletedDeductionsBody", "<b>COMPLETED DEDUCTIONS:</b>\n\nNone yet established.",
                15, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.9f, 0.9f, 0.9f, 1f),
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, true);
            completedBody.rectTransform.offsetMin = new Vector2(16f, 16f);
            completedBody.rectTransform.offsetMax = new Vector2(-16f, -16f);

            deductionUI.boardTitleText = title;
            deductionUI.closeBoardButton = closeBtn;
            deductionUI.cluesContainer = cluesContent.transform;
            deductionUI.clueCardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ElementsDir}/UI_ClueCard.prefab");
            deductionUI.selectionStatusText = statusText;
            deductionUI.feedbackBanner = bannerObj;
            deductionUI.feedbackText = feedbackText;
            deductionUI.deductionsContainer = deductionsObj.transform;
            deductionUI.completedDeductionsBody = completedBody;

            SaveAndDestroy(root, path);
        }

        private static void GenerateConclusionQuizPanelPrefab()
        {
            string path = $"{PanelsDir}/Panel_ConclusionQuiz.prefab";
            GameObject root = new GameObject("Panel_ConclusionQuiz", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ConclusionUI));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = root.GetComponent<Image>();
            bg.color = new Color(0.06f, 0.07f, 0.09f, 0.98f);
            bg.raycastTarget = true;

            ConclusionUI conclusionUI = root.GetComponent<ConclusionUI>();

            // Quiz Container
            GameObject quizContainer = new GameObject("Container_Quiz", typeof(RectTransform));
            quizContainer.transform.SetParent(root.transform, false);
            RectTransform qcRt = quizContainer.GetComponent<RectTransform>();
            qcRt.anchorMin = Vector2.zero;
            qcRt.anchorMax = Vector2.one;
            qcRt.offsetMin = Vector2.zero;
            qcRt.offsetMax = Vector2.zero;

            Text qTitle = AddTextChild(quizContainer.transform, "Text_QuestionTitle", "CASE CONCLUSION: FINAL HYPOTHESIS", 22, FontStyle.Bold,
                TextAnchor.MiddleCenter, new Color(0.95f, 0.82f, 0.45f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -35f), new Vector2(750f, 40f), true);

            // Options Grid
            GameObject scrollObj = new GameObject("Scroll_Options", typeof(RectTransform), typeof(ScrollRect));
            scrollObj.transform.SetParent(quizContainer.transform, false);
            RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0.1f, 0.16f);
            scrollRt.anchorMax = new Vector2(0.9f, 0.90f);
            scrollRt.offsetMin = Vector2.zero;
            scrollRt.offsetMax = Vector2.zero;

            GameObject gridObj = new GameObject("Grid_Options", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            gridObj.transform.SetParent(scrollObj.transform, false);
            RectTransform gridRt = gridObj.GetComponent<RectTransform>();
            gridRt.anchorMin = new Vector2(0f, 1f);
            gridRt.anchorMax = new Vector2(1f, 1f);
            gridRt.pivot = new Vector2(0.5f, 1f);
            gridRt.anchoredPosition = Vector2.zero;
            gridRt.sizeDelta = new Vector2(0f, 400f);

            VerticalLayoutGroup vlg = gridObj.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = gridObj.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect sr = scrollObj.GetComponent<ScrollRect>();
            sr.content = gridRt;
            sr.horizontal = false;
            sr.vertical = true;

            Button submitBtn = AddButtonChild(quizContainer.transform, "Button_SubmitConclusion", "Submit Hypothesis", 18, new Color(0.18f, 0.45f, 0.25f, 0.95f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 35f), new Vector2(280f, 50f));

            // Results Container
            GameObject resultsContainer = new GameObject("Container_Results", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            resultsContainer.transform.SetParent(root.transform, false);
            RectTransform rcRt = resultsContainer.GetComponent<RectTransform>();
            rcRt.anchorMin = Vector2.zero;
            rcRt.anchorMax = Vector2.one;
            rcRt.offsetMin = Vector2.zero;
            rcRt.offsetMax = Vector2.zero;
            Image rcBg = resultsContainer.GetComponent<Image>();
            rcBg.color = new Color(0.06f, 0.07f, 0.09f, 0.98f);

            Text resTitle = AddTextChild(resultsContainer.transform, "Text_ResultTitle", "CASE RESOLUTION", 28, FontStyle.Bold,
                TextAnchor.MiddleCenter, new Color(0.95f, 0.82f, 0.45f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -60f), new Vector2(600f, 50f), true);

            Text resGrade = AddTextChild(resultsContainer.transform, "Text_Grade", "GRADE: S", 36, FontStyle.Bold,
                TextAnchor.MiddleCenter, new Color(0.2f, 0.85f, 0.3f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -115f), new Vector2(300f, 50f), true);

            Text resStars = AddTextChild(resultsContainer.transform, "Text_StarRating", "★ ★ ★", 28, FontStyle.Bold,
                TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.2f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -165f), new Vector2(300f, 40f), true);

            Text resBreakdown = AddTextChild(resultsContainer.transform, "Text_ScoreBreakdown", "Accuracy: 100%\nTime Bonus: +500\nTotal Score: 1500", 17, FontStyle.Normal,
                TextAnchor.MiddleCenter, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -15f), new Vector2(500f, 120f), true);

            Button contBtn = AddButtonChild(resultsContainer.transform, "Button_Continue", "Review Desk", 16, new Color(0.18f, 0.32f, 0.45f, 0.95f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-160f, 60f), new Vector2(180f, 48f));

            Button nextLevelBtn = AddButtonChild(resultsContainer.transform, "Button_NextLevel", "Next Case", 16, new Color(0.18f, 0.45f, 0.25f, 0.95f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(160f, 60f), new Vector2(180f, 48f));

            Button menuBtn = AddButtonChild(resultsContainer.transform, "Button_MainMenu", "Main Menu", 16, new Color(0.35f, 0.35f, 0.38f, 0.95f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(160f, 48f));

            resultsContainer.SetActive(false);

            conclusionUI.quizContainer = quizContainer;
            conclusionUI.questionTitleText = qTitle;
            conclusionUI.optionsGrid = gridObj.transform;
            conclusionUI.questionHeaderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ElementsDir}/UI_ConclusionQuestionHeader.prefab");
            conclusionUI.optionItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ElementsDir}/UI_ConclusionOptionItem.prefab");
            conclusionUI.submitConclusionButton = submitBtn;
            conclusionUI.resultsContainer = resultsContainer;
            conclusionUI.resultTitleText = resTitle;
            conclusionUI.resultGradeText = resGrade;
            conclusionUI.starRatingText = resStars;
            conclusionUI.scoreBreakdownText = resBreakdown;
            conclusionUI.continueButton = contBtn;
            conclusionUI.nextLevelButton = nextLevelBtn;
            conclusionUI.returnToMainMenuButton = menuBtn;

            SaveAndDestroy(root, path);
        }

        private static void GenerateResultsScreenPanelPrefab()
        {
            string path = $"{PanelsDir}/Panel_ResultsScreen.prefab";
            GameObject root = new GameObject("Panel_ResultsScreen", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = root.GetComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.07f, 0.95f);
            bg.raycastTarget = true;

            // Background artwork
            GameObject solvedBg = new GameObject("Image_SolvedBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            solvedBg.transform.SetParent(root.transform, false);
            RectTransform sbgRt = solvedBg.GetComponent<RectTransform>();
            sbgRt.anchorMin = Vector2.zero;
            sbgRt.anchorMax = Vector2.one;
            sbgRt.offsetMin = Vector2.zero;
            sbgRt.offsetMax = Vector2.zero;
            Image sbgImg = solvedBg.GetComponent<Image>();
            sbgImg.color = new Color(1f, 1f, 1f, 0.12f);
            sbgImg.raycastTarget = false;

            // Card Results
            GameObject cardObj = new GameObject("Card_Results", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            cardObj.transform.SetParent(root.transform, false);
            RectTransform cardRt = cardObj.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(650f, 520f);
            Image cardBg = cardObj.GetComponent<Image>();
            cardBg.color = new Color(0.12f, 0.14f, 0.18f, 0.98f);

            AddTextChild(cardObj.transform, "Text_Title", "CASE CLOSED", 28, FontStyle.Bold,
                TextAnchor.MiddleCenter, new Color(0.95f, 0.82f, 0.45f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -40f), new Vector2(500f, 45f), true);

            AddTextChild(cardObj.transform, "Text_Grade", "GRADE: S", 36, FontStyle.Bold,
                TextAnchor.MiddleCenter, new Color(0.2f, 0.85f, 0.3f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -90f), new Vector2(300f, 45f), true);

            AddTextChild(cardObj.transform, "Text_StarRating", "★ ★ ★", 28, FontStyle.Bold,
                TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.2f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -135f), new Vector2(300f, 40f), true);

            AddTextChild(cardObj.transform, "Text_ScoreBreakdown", "Evidence Examined: 100%\nContradictions Exposed: 100%\nInvestigation Time: 3m 45s\nFinal Score: 1000",
                16, FontStyle.Normal, TextAnchor.MiddleCenter, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 0f), new Vector2(550f, 120f), true);

            AddButtonChild(cardObj.transform, "Button_Continue", "Review Desk", 15, new Color(0.18f, 0.32f, 0.45f, 0.95f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-160f, 45f), new Vector2(170f, 46f));

            AddButtonChild(cardObj.transform, "Button_NextLevel", "Next Case", 15, new Color(0.18f, 0.45f, 0.25f, 0.95f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), new Vector2(0f, 45f), new Vector2(170f, 46f));

            AddButtonChild(cardObj.transform, "Button_MainMenu", "Main Menu", 15, new Color(0.35f, 0.35f, 0.38f, 0.95f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(160f, 45f), new Vector2(170f, 46f));

            SaveAndDestroy(root, path);
        }

        private static void GenerateGameOverPanelPrefab()
        {
            string path = $"{PanelsDir}/Panel_GameOver.prefab";
            GameObject root = new GameObject("Panel_GameOver", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(GameOverUI));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = root.GetComponent<Image>();
            bg.color = new Color(0.06f, 0.02f, 0.02f, 0.96f);
            bg.raycastTarget = true;

            GameOverUI gameOverUI = root.GetComponent<GameOverUI>();

            GameObject cardObj = new GameObject("Card_GameOver", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            cardObj.transform.SetParent(root.transform, false);
            RectTransform cardRt = cardObj.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(600f, 440f);
            Image cardBg = cardObj.GetComponent<Image>();
            cardBg.color = new Color(0.12f, 0.07f, 0.07f, 1f);

            Text titleText = AddTextChild(cardObj.transform, "Text_Title", "LEVEL 1: TIME EXPIRED", 28, FontStyle.Bold,
                TextAnchor.MiddleCenter, new Color(1f, 0.25f, 0.25f, 1f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 160f), new Vector2(560f, 50f), true);

            Text subtitleText = AddTextChild(cardObj.transform, "Text_Subtitle", "Investigation Failed — The suspect slipped away before the case was closed.", 15, FontStyle.Italic,
                TextAnchor.MiddleCenter, new Color(0.82f, 0.82f, 0.82f, 1f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 115f), new Vector2(560f, 35f), true);

            Text detailsText = AddTextChild(cardObj.transform, "Text_Details", "Lead Investigator: Detective\nTime Lapsed: 5m 00s\nEvidence Discovered: 0 / 5\nContradictions Exposed: 0 / 3\n\nStatus: UNRESOLVED", 17, FontStyle.Normal,
                TextAnchor.UpperLeft, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 0f), new Vector2(500f, 170f), true);

            Button retryBtn = AddButtonChild(cardObj.transform, "Button_Retry", "Retry Case", 16, new Color(0.18f, 0.45f, 0.25f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-120f, -160f), new Vector2(200f, 48f));

            Button menuBtn = AddButtonChild(cardObj.transform, "Button_MainMenu", "Main Menu", 16, new Color(0.35f, 0.35f, 0.38f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(120f, -160f), new Vector2(200f, 48f));

            var sGo = new SerializedObject(gameOverUI);
            sGo.FindProperty("titleText").objectReferenceValue = titleText;
            sGo.FindProperty("subtitleText").objectReferenceValue = subtitleText;
            sGo.FindProperty("detailsBreakdownText").objectReferenceValue = detailsText;
            sGo.FindProperty("retryButton").objectReferenceValue = retryBtn;
            sGo.FindProperty("returnToMainMenuButton").objectReferenceValue = menuBtn;
            sGo.ApplyModifiedProperties();

            root.SetActive(false);
            SaveAndDestroy(root, path);
        }

        private static void GenerateInGameMenuPanelPrefab()
        {
            string path = $"{PanelsDir}/Panel_InGameMenu.prefab";
            GameObject root = new GameObject("Panel_InGameMenu", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = root.GetComponent<Image>();
            bg.color = new Color(0.04f, 0.05f, 0.08f, 0.88f);
            bg.raycastTarget = true;

            GameObject cardObj = new GameObject("Card_InGameMenu", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            cardObj.transform.SetParent(root.transform, false);
            RectTransform cardRt = cardObj.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(420f, 320f);
            Image cardBg = cardObj.GetComponent<Image>();
            cardBg.color = new Color(0.12f, 0.14f, 0.18f, 0.98f);

            AddTextChild(cardObj.transform, "Text_Title", "GAME PAUSED", 24, FontStyle.Bold,
                TextAnchor.MiddleCenter, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 95f), new Vector2(380f, 40f), true);

            AddButtonChild(cardObj.transform, "Button_Resume", "Resume Game", 16, new Color(0.18f, 0.45f, 0.25f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 25f), new Vector2(260f, 48f));

            AddButtonChild(cardObj.transform, "Button_MainMenu", "Main Menu", 16, new Color(0.55f, 0.22f, 0.22f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -40f), new Vector2(260f, 48f));

            // Nested Confirm Panel
            GameObject confirmObj = new GameObject("Panel_MainMenuConfirm", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            confirmObj.transform.SetParent(root.transform, false);
            RectTransform cRt = confirmObj.GetComponent<RectTransform>();
            cRt.anchorMin = Vector2.zero;
            cRt.anchorMax = Vector2.one;
            cRt.offsetMin = Vector2.zero;
            cRt.offsetMax = Vector2.zero;
            Image cBg = confirmObj.GetComponent<Image>();
            cBg.color = new Color(0.02f, 0.02f, 0.02f, 0.90f);
            cBg.raycastTarget = true;

            GameObject cCard = new GameObject("Card_Confirm", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            cCard.transform.SetParent(confirmObj.transform, false);
            RectTransform ccRt = cCard.GetComponent<RectTransform>();
            ccRt.anchorMin = new Vector2(0.5f, 0.5f);
            ccRt.anchorMax = new Vector2(0.5f, 0.5f);
            ccRt.sizeDelta = new Vector2(500f, 260f);
            Image ccBg = cCard.GetComponent<Image>();
            ccBg.color = new Color(0.14f, 0.10f, 0.10f, 0.98f);

            AddTextChild(cCard.transform, "Text_Title", "RETURN TO MAIN MENU?", 22, FontStyle.Bold,
                TextAnchor.MiddleCenter, new Color(1f, 0.35f, 0.35f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 70f), new Vector2(460f, 40f), true);

            AddTextChild(cCard.transform, "Text_Message", "Any unsaved investigation progress in this case will be lost.\nAre you sure you want to return to the main menu?",
                15, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.85f, 0.85f, 0.85f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 15f), new Vector2(440f, 50f), true);

            AddButtonChild(cCard.transform, "Button_ConfirmYes", "Yes, Return", 15, new Color(0.65f, 0.20f, 0.20f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-110f, -60f), new Vector2(180f, 46f));

            AddButtonChild(cCard.transform, "Button_ConfirmNo", "Cancel", 15, new Color(0.30f, 0.32f, 0.38f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(110f, -60f), new Vector2(180f, 46f));

            confirmObj.SetActive(false);
            root.SetActive(false);

            SaveAndDestroy(root, path);
        }

        private static void GenerateInvestigatorSelectPanelPrefab()
        {
            string path = $"{PanelsDir}/Panel_InvestigatorSelect.prefab";
            GameObject root = new GameObject("Panel_InvestigatorSelect", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(InvestigatorSelectionUI));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = root.GetComponent<Image>();
            bg.color = new Color(0.06f, 0.07f, 0.10f, 0.96f);
            bg.raycastTarget = true;

            InvestigatorSelectionUI ui = root.GetComponent<InvestigatorSelectionUI>();

            AddTextChild(root.transform, "Text_Title", "SELECT CASE LEVEL", 24, FontStyle.Bold,
                TextAnchor.MiddleCenter, new Color(0.95f, 0.82f, 0.45f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -60f), new Vector2(600f, 40f), true);

            // Level Selection Sub-Container
            GameObject levelSelectObj = new GameObject("Container_LevelSelect", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            levelSelectObj.transform.SetParent(root.transform, false);
            RectTransform lsRt = levelSelectObj.GetComponent<RectTransform>();
            lsRt.anchorMin = new Vector2(0.5f, 0.5f);
            lsRt.anchorMax = new Vector2(0.5f, 0.5f);
            lsRt.pivot = new Vector2(0.5f, 0.5f);
            lsRt.anchoredPosition = new Vector2(0f, 20f);
            lsRt.sizeDelta = new Vector2(620f, 160f);
            Image lsBg = levelSelectObj.GetComponent<Image>();
            lsBg.color = new Color(0.10f, 0.12f, 0.15f, 0.90f);

            Text levelStatus = AddTextChild(levelSelectObj.transform, "Text_LevelStatus", "CURRENT CASE: LEVEL 1", 16, FontStyle.Bold,
                TextAnchor.MiddleCenter, new Color(0.95f, 0.82f, 0.45f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -28f), new Vector2(500f, 30f), true);

            Button l1Btn = AddButtonChild(levelSelectObj.transform, "Button_Level1", "Case 01", 14, new Color(0.24f, 0.34f, 0.48f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-190f, 36f), new Vector2(175f, 44f));

            Button l2Btn = AddButtonChild(levelSelectObj.transform, "Button_Level2", "Case 02", 14, new Color(0.24f, 0.34f, 0.48f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(175f, 44f));

            Button l3Btn = AddButtonChild(levelSelectObj.transform, "Button_Level3", "Case 03", 14, new Color(0.24f, 0.34f, 0.48f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(190f, 36f), new Vector2(175f, 44f));

            Button closeSelectionBtn = AddButtonChild(root.transform, "Button_CloseSelection", "Return to Desk", 15, new Color(0.35f, 0.35f, 0.38f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(220f, 44f));

            ui.level1Button = l1Btn;
            ui.level2Button = l2Btn;
            ui.level3Button = l3Btn;
            ui.currentLevelStatusText = levelStatus;
            ui.closeSelectionButton = closeSelectionBtn;

            SaveAndDestroy(root, path);
        }

        private static void EnsureCaseFileNotebookPanelPrefab()
        {
            string destPath = $"{PanelsDir}/Panel_CaseFileNotebook.prefab";
            string srcPath = "Assets/Prefabs/Panel_CaseFileNotebook.prefab";

            if (!File.Exists(destPath) && File.Exists(srcPath))
            {
                AssetDatabase.CopyAsset(srcPath, destPath);
                AssetDatabase.Refresh();
            }
        }

        #endregion

        #region C. Master Canvas

        public static void GenerateMasterCanvasPrefab()
        {
            string path = $"{UIDir}/Canvas_MainUI.prefab";
            GameObject canvasRoot = new GameObject("Canvas_MainUI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(UIManager));

            Canvas canvas = canvasRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;

            CanvasScaler scaler = canvasRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.dynamicPixelsPerUnit = 3.0f;

            UIManager uiManager = canvasRoot.GetComponent<UIManager>();

            // 1. Table Panel placeholder
            GameObject mainTable = new GameObject("Panel_MainTable", typeof(RectTransform));
            mainTable.transform.SetParent(canvasRoot.transform, false);
            RectTransform mtRt = mainTable.GetComponent<RectTransform>();
            mtRt.anchorMin = Vector2.zero;
            mtRt.anchorMax = Vector2.one;
            mtRt.offsetMin = Vector2.zero;
            mtRt.offsetMax = Vector2.zero;
            uiManager.mainTablePanel = mainTable;

            // 2. Instantiate and attach child panels from prefabs
            GameObject headerNavPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PanelsDir}/Panel_HeaderNav.prefab");
            if (headerNavPrefab != null)
            {
                GameObject headerNav = PrefabUtility.InstantiatePrefab(headerNavPrefab, canvasRoot.transform) as GameObject;
                if (headerNav != null)
                {
                    Transform tContainer = headerNav.transform.Find("Container_Timer");
                    if (tContainer != null) uiManager.timerContainer = tContainer.gameObject;

                    Transform nbBtn = headerNav.transform.Find("Button_Notebook");
                    if (nbBtn != null) uiManager.notebookButton = nbBtn.gameObject;

                    Transform dedBtn = headerNav.transform.Find("Button_DeductionBoard");
                    if (dedBtn != null) uiManager.deductionBoardButton = dedBtn.gameObject;

                    Transform concBtn = headerNav.transform.Find("Button_Conclude");
                    if (concBtn != null) uiManager.concludeCaseButton = concBtn.gameObject;

                    Transform invBtn = headerNav.transform.Find("Button_InvestigatorSelect");
                    if (invBtn != null) uiManager.investigatorSelectButton = invBtn.gameObject;

                    Transform menuBtn = headerNav.transform.Find("Button_Menu");
                    if (menuBtn != null) uiManager.returnToMenuButton = menuBtn.gameObject;
                }
            }

            GameObject dialoguePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PanelsDir}/Panel_Dialogue.prefab");
            if (dialoguePrefab != null)
            {
                PrefabUtility.InstantiatePrefab(dialoguePrefab, canvasRoot.transform);
            }

            GameObject inspectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PanelsDir}/Panel_InspectModal.prefab");
            if (inspectPrefab != null)
            {
                GameObject inspectObj = PrefabUtility.InstantiatePrefab(inspectPrefab, canvasRoot.transform) as GameObject;
                uiManager.inspectModalPanel = inspectObj;
            }

            GameObject notebookPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PanelsDir}/Panel_CaseFileNotebook.prefab")
                                    ?? AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Panel_CaseFileNotebook.prefab");
            if (notebookPrefab != null)
            {
                GameObject nbObj = PrefabUtility.InstantiatePrefab(notebookPrefab, canvasRoot.transform) as GameObject;
                uiManager.notebookPanel = nbObj;
            }

            GameObject deductionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PanelsDir}/Panel_DeductionBoard.prefab");
            if (deductionPrefab != null)
            {
                GameObject dedObj = PrefabUtility.InstantiatePrefab(deductionPrefab, canvasRoot.transform) as GameObject;
                uiManager.deductionBoardPanel = dedObj;
            }

            GameObject conclusionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PanelsDir}/Panel_ConclusionQuiz.prefab");
            if (conclusionPrefab != null)
            {
                GameObject concObj = PrefabUtility.InstantiatePrefab(conclusionPrefab, canvasRoot.transform) as GameObject;
                uiManager.conclusionQuizPanel = concObj;
            }

            GameObject resultsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PanelsDir}/Panel_ResultsScreen.prefab");
            if (resultsPrefab != null)
            {
                GameObject resObj = PrefabUtility.InstantiatePrefab(resultsPrefab, canvasRoot.transform) as GameObject;
                uiManager.resultsScreenPanel = resObj;
            }

            GameObject invSelectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PanelsDir}/Panel_InvestigatorSelect.prefab");
            if (invSelectPrefab != null)
            {
                GameObject invObj = PrefabUtility.InstantiatePrefab(invSelectPrefab, canvasRoot.transform) as GameObject;
                uiManager.investigatorSelectPanel = invObj;
            }

            GameObject gameOverPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PanelsDir}/Panel_GameOver.prefab");
            if (gameOverPrefab != null)
            {
                GameObject goObj = PrefabUtility.InstantiatePrefab(gameOverPrefab, canvasRoot.transform) as GameObject;
                uiManager.gameOverPanel = goObj;
            }

            GameObject inGameMenuPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PanelsDir}/Panel_InGameMenu.prefab");
            if (inGameMenuPrefab != null)
            {
                GameObject igmObj = PrefabUtility.InstantiatePrefab(inGameMenuPrefab, canvasRoot.transform) as GameObject;
                uiManager.inGameMenuPanel = igmObj;

                if (igmObj != null)
                {
                    Transform resumeBtnTrans = igmObj.transform.Find("Card_InGameMenu/Button_Resume");
                    if (resumeBtnTrans != null) uiManager.resumeGameButton = resumeBtnTrans.GetComponent<Button>();

                    Transform menuBtnTrans = igmObj.transform.Find("Card_InGameMenu/Button_MainMenu");
                    if (menuBtnTrans != null) uiManager.inGameMainMenuButton = menuBtnTrans.GetComponent<Button>();

                    Transform confirmTrans = igmObj.transform.Find("Panel_MainMenuConfirm");
                    if (confirmTrans != null)
                    {
                        uiManager.mainMenuConfirmPanel = confirmTrans.gameObject;
                        Transform yesBtnTrans = confirmTrans.Find("Card_Confirm/Button_ConfirmYes");
                        if (yesBtnTrans != null) uiManager.confirmMainMenuYesButton = yesBtnTrans.GetComponent<Button>();
                        Transform noBtnTrans = confirmTrans.Find("Card_Confirm/Button_ConfirmNo");
                        if (noBtnTrans != null) uiManager.confirmMainMenuNoButton = noBtnTrans.GetComponent<Button>();
                    }
                }
            }

            SaveAndDestroy(canvasRoot, path);
        }

        #endregion

        #region D. World-Space Gameplay Actors

        public static void GenerateWorldGameplayActors()
        {
            GenerateTableEvidenceBasePrefab();
            GenerateTableEvidenceVariants();
            GenerateSuspectSlotBasePrefab();
            GenerateSuspectSlotVariants();
        }

        private static void GenerateTableEvidenceBasePrefab()
        {
            string path = $"{GameplayDir}/Actor_TableEvidence_Base.prefab";
            GameObject root = new GameObject("Actor_TableEvidence_Base", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(TableEvidenceItem));
            
            SpriteRenderer sr = root.GetComponent<SpriteRenderer>();
            sr.sortingOrder = 10;

            BoxCollider2D col = root.GetComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.5f, 1.5f);

            // Child glow halo
            GameObject haloObj = new GameObject("Glow_Halo", typeof(SpriteRenderer));
            haloObj.transform.SetParent(root.transform, false);
            haloObj.transform.localPosition = new Vector3(0f, 0f, 0.01f);
            SpriteRenderer haloSr = haloObj.GetComponent<SpriteRenderer>();
            haloSr.sortingOrder = 9;
            haloSr.color = new Color(1f, 0.88f, 0.25f, 0f);
            haloObj.SetActive(false);

            TableEvidenceItem item = root.GetComponent<TableEvidenceItem>();
            item.spriteRenderer = sr;
            item.highlightGlow = haloObj;
            item.glowColor = new Color(1.0f, 0.88f, 0.25f, 0.95f);
            item.maxGlowIntensity = 1.0f;
            item.haloBaseScale = 1.08f;
            item.glowFadeSpeed = 12f;
            item.pulseGlow = true;
            item.pulseSpeed = 4.0f;
            item.pulseAmplitude = 0.04f;
            item.scaleOnHover = true;
            item.hoverScaleMultiplier = 1.035f;
            item.hoverColor = new Color(1f, 1f, 0.9f, 1f);

            SaveAndDestroy(root, path);
        }

        private static void GenerateTableEvidenceVariants()
        {
            // 1. Photograph
            CreateEvidenceVariant("TableEvidence_Photograph", "EVD_FAMILY_PHOTO", new Vector2(1.2f, 1.4f), 1.05f, false,
                "Assets/EVIDENCES/TablePOV/WindowPOV.png");

            // 2. Document
            CreateEvidenceVariant("TableEvidence_Document", "EVD_FORENSIC_REPORT", new Vector2(1.4f, 1.8f), 1.04f, false,
                "Assets/EVIDENCES/TablePOV/SecuritylogPOV.png");

            // 3. Physical Clue
            CreateEvidenceVariant("TableEvidence_PhysicalClue", "EVD_CRIME_KNIFE", new Vector2(1.5f, 1.2f), 1.06f, false,
                "Assets/EVIDENCES/TablePOV/TeacupPOv.png");

            // 4. Open Notebook
            CreateEvidenceVariant("TableEvidence_OpenNotebook", "", new Vector2(2.0f, 1.5f), 1.03f, true,
                "Assets/UI/DetectiveClipboard.png");
        }

        private static void CreateEvidenceVariant(string prefabName, string evidenceId, Vector2 colliderSize, float hoverScale, bool openNotebook, string spritePath)
        {
            string targetPath = $"{EvidenceDir}/{prefabName}.prefab";
            GameObject root = new GameObject(prefabName, typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(TableEvidenceItem));

            SpriteRenderer sr = root.GetComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            if (!string.IsNullOrEmpty(spritePath))
            {
                sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            }

            BoxCollider2D col = root.GetComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = colliderSize;

            GameObject haloObj = new GameObject("Glow_Halo", typeof(SpriteRenderer));
            haloObj.transform.SetParent(root.transform, false);
            haloObj.transform.localPosition = new Vector3(0f, 0f, 0.01f);
            SpriteRenderer haloSr = haloObj.GetComponent<SpriteRenderer>();
            haloSr.sortingOrder = 9;
            haloSr.color = new Color(1f, 0.88f, 0.25f, 0f);
            if (sr.sprite != null) haloSr.sprite = sr.sprite;
            haloObj.SetActive(false);

            TableEvidenceItem item = root.GetComponent<TableEvidenceItem>();
            item.spriteRenderer = sr;
            item.highlightGlow = haloObj;
            item.evidenceId = evidenceId;
            item.openNotebookOnClick = openNotebook;
            item.glowColor = new Color(1.0f, 0.88f, 0.25f, 0.95f);
            item.maxGlowIntensity = 1.0f;
            item.haloBaseScale = 1.08f;
            item.glowFadeSpeed = 12f;
            item.pulseGlow = true;
            item.pulseSpeed = 4.0f;
            item.pulseAmplitude = 0.04f;
            item.scaleOnHover = true;
            item.hoverScaleMultiplier = hoverScale;
            item.hoverColor = new Color(1f, 1f, 0.9f, 1f);

            SaveAndDestroy(root, targetPath);
        }

        private static void GenerateSuspectSlotBasePrefab()
        {
            string path = $"{GameplayDir}/Actor_SuspectSlot_Base.prefab";
            GameObject root = new GameObject("Actor_SuspectSlot_Base", typeof(SpriteRenderer), typeof(CharacterDisplay));

            SpriteRenderer sr = root.GetComponent<SpriteRenderer>();
            sr.sortingOrder = 5;

            CharacterDisplay cd = root.GetComponent<CharacterDisplay>();
            cd.characterSpriteRenderer = sr;
            cd.characterSlot = CharacterSlot.AutoDetect;

            SaveAndDestroy(root, path);
        }

        private static void GenerateSuspectSlotVariants()
        {
            // SuspectSlot_Left (Primary Suspect)
            CreateSuspectVariant("SuspectSlot_Left", CharacterSlot.PrimarySuspect, new Vector3(-3.2f, 1.2f, 0f), "Assets/CHARACTERS/Vince.png");

            // SuspectSlot_Right (Secondary Suspect)
            CreateSuspectVariant("SuspectSlot_Right", CharacterSlot.SecondarySuspect, new Vector3(3.2f, 1.2f, 0f), "Assets/CHARACTERS/Paul.png");
        }

        private static void CreateSuspectVariant(string prefabName, CharacterSlot slot, Vector3 pos, string spritePath)
        {
            string targetPath = $"{SuspectsDir}/{prefabName}.prefab";
            GameObject root = new GameObject(prefabName, typeof(SpriteRenderer), typeof(CharacterDisplay));
            root.transform.position = pos;

            SpriteRenderer sr = root.GetComponent<SpriteRenderer>();
            sr.sortingOrder = 5;
            if (!string.IsNullOrEmpty(spritePath))
            {
                sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            }

            CharacterDisplay cd = root.GetComponent<CharacterDisplay>();
            cd.characterSpriteRenderer = sr;
            cd.characterSlot = slot;

            SaveAndDestroy(root, targetPath);
        }

        #endregion

        #region E. Core Rigs

        public static void GenerateCoreRigs()
        {
            GenerateCoreManagersPrefab();
            GenerateCoreCameraPrefab();
            GenerateCoreDetectiveArmPointerPrefab();
        }

        private static void GenerateCoreManagersPrefab()
        {
            string path = $"{CoreDir}/Core_Managers.prefab";
            GameObject root = new GameObject("_Managers",
                typeof(AudioManager),
                typeof(CaseManager),
                typeof(EvidenceManager),
                typeof(InterrogationManager),
                typeof(DeductionBoardController),
                typeof(CaseConclusionManager),
                typeof(GameBootstrap));

            AudioManager audioMgr = root.GetComponent<AudioManager>();
            AudioManagerSetupUtility.AssignClipsAndSources(audioMgr);

            AudioSource[] sources = root.GetComponents<AudioSource>();
            while (sources.Length > 3)
            {
                UnityEngine.Object.DestroyImmediate(sources[sources.Length - 1]);
                sources = root.GetComponents<AudioSource>();
            }

            SaveAndDestroy(root, path);
        }

        private static void GenerateCoreCameraPrefab()
        {
            string path = $"{CoreDir}/Core_InvestigationCamera.prefab";
            GameObject root = new GameObject("Main Camera",
                typeof(Camera),
                typeof(AudioListener),
                typeof(FixedInvestigationCamera),
                typeof(Physics2DRaycaster));

            root.tag = "MainCamera";
            root.transform.position = new Vector3(0f, 0f, -10f);

            Camera cam = root.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.11f, 1f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;

            FixedInvestigationCamera fixCam = root.GetComponent<FixedInvestigationCamera>();
            fixCam.fixedPosition = new Vector3(0f, 0f, -10f);
            fixCam.orthographicSize = 5f;
            fixCam.lockCameraTransform = true;

            SaveAndDestroy(root, path);
        }

        private static void GenerateCoreDetectiveArmPointerPrefab()
        {
            string path = $"{CoreDir}/Core_DetectiveArmPointer.prefab";
            GameObject root = new GameObject("Detective_Arm_Pointer", typeof(SpriteRenderer), typeof(ArmPointerController));
            root.transform.position = new Vector3(0f, -2.5f, 0f);

            SpriteRenderer sr = root.GetComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ArmPointer.png");
            sr.sortingOrder = 20;

            GameObject fingertip = new GameObject("FingertipPoint");
            fingertip.transform.SetParent(root.transform, false);
            fingertip.transform.localPosition = new Vector3(0f, 2.2f, 0f);

            ArmPointerController apc = root.GetComponent<ArmPointerController>();
            apc.armRenderer = sr;
            apc.fingertipPoint = fingertip.transform;
            apc.interactionRadius = 0.45f;
            apc.horizontalBounds = new Vector2(-6.5f, 6.5f);
            apc.verticalBounds = new Vector2(-3.8f, -0.8f);
            apc.followSpeed = 18f;
            apc.maxTiltAngle = 10f;

            SaveAndDestroy(root, path);
        }

        #endregion

        #region F. VFX Prefabs

        public static void GenerateVFXPrefabs()
        {
            GenerateClueDiscoveredBannerPrefab();
            GenerateContradictionBurstPrefab();
        }

        private static void GenerateClueDiscoveredBannerPrefab()
        {
            string path = $"{VFXDir}/FX_ClueDiscoveredBanner.prefab";
            GameObject root = new GameObject("FX_ClueDiscoveredBanner", typeof(RectTransform), typeof(CanvasRenderer), typeof(CanvasGroup), typeof(Image));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.85f);
            rt.anchorMax = new Vector2(0.5f, 0.85f);
            rt.sizeDelta = new Vector2(460f, 80f);

            Image bg = root.GetComponent<Image>();
            bg.color = new Color(0.12f, 0.16f, 0.22f, 0.96f);

            // Border
            GameObject borderObj = new GameObject("Image_Border", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            borderObj.transform.SetParent(root.transform, false);
            RectTransform bRt = borderObj.GetComponent<RectTransform>();
            bRt.anchorMin = Vector2.zero;
            bRt.anchorMax = Vector2.one;
            bRt.offsetMin = new Vector2(-2f, -2f);
            bRt.offsetMax = new Vector2(2f, 2f);
            Image bImg = borderObj.GetComponent<Image>();
            bImg.color = new Color(0.95f, 0.82f, 0.45f, 0.9f);
            bImg.raycastTarget = false;

            AddTextChild(root.transform, "Text_Banner", "★ CLUE DISCOVERED ★", 20, FontStyle.Bold,
                TextAnchor.MiddleCenter, new Color(0.98f, 0.92f, 0.5f, 1f),
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, true);

            SaveAndDestroy(root, path);
        }

        private static void GenerateContradictionBurstPrefab()
        {
            string path = $"{VFXDir}/FX_ContradictionBurst.prefab";
            GameObject root = new GameObject("FX_ContradictionBurst", typeof(RectTransform), typeof(CanvasRenderer), typeof(CanvasGroup), typeof(Image));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image flash = root.GetComponent<Image>();
            flash.color = new Color(0.95f, 0.15f, 0.15f, 0.85f);
            flash.raycastTarget = false;

            AddTextChild(root.transform, "Text_Burst", "CONTRADICTION EXPOSED!", 34, FontStyle.Bold,
                TextAnchor.MiddleCenter, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(800f, 100f), true);

            SaveAndDestroy(root, path);
        }

        #endregion

        #region Scene Wiring Utility

        public static bool ApplyPrefabsToCurrentScene(string scenePath)
        {
            bool isMenu = scenePath.Contains("MainMenu");

            // 1. Configure Main Camera
            WireMainCamera();

            // 2. Configure _Managers (Preserve scene-specific initializers)
            WireManagers();

            // 3. Configure Canvas_MainUI
            WireCanvasMainUI(isMenu);

            if (!isMenu)
            {
                // 4. Configure Detective Arm Pointer
                WireDetectiveArmPointer();

                // 5. Configure Characters
                WireCharacters();

                // 6. Configure Items / Table Evidence
                WireTableItems();

                // 7. Configure Case Timer & Game Over
                CaseTimerSetupUtility.SetupTimerAndGameOverInCurrentScene();
            }
            else
            {
                MainMenuSetupUtility.RebuildMainMenuInCurrentScene();
            }

            return true;
        }

        private static void WireMainCamera()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                var found = UnityEngine.Object.FindFirstObjectByType<Camera>();
                if (found != null) mainCam = found;
            }

            if (mainCam == null)
            {
                GameObject camPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{CoreDir}/Core_InvestigationCamera.prefab");
                if (camPrefab != null)
                {
                    PrefabUtility.InstantiatePrefab(camPrefab);
                }
                return;
            }

            mainCam.tag = "MainCamera";
            mainCam.orthographic = true;
            mainCam.orthographicSize = 5f;

            if (mainCam.GetComponent<AudioListener>() == null)
            {
                mainCam.gameObject.AddComponent<AudioListener>();
            }

            if (mainCam.GetComponent<FixedInvestigationCamera>() == null)
            {
                var fixCam = mainCam.gameObject.AddComponent<FixedInvestigationCamera>();
                fixCam.fixedPosition = new Vector3(0f, 0f, -10f);
                fixCam.orthographicSize = 5f;
                fixCam.lockCameraTransform = true;
            }

            if (mainCam.GetComponent<Physics2DRaycaster>() == null)
            {
                mainCam.gameObject.AddComponent<Physics2DRaycaster>();
            }

            EditorUtility.SetDirty(mainCam);
        }

        private static void WireManagers()
        {
            GameObject managersRoot = GameObject.Find("_Managers");
            if (managersRoot == null)
            {
                managersRoot = new GameObject("_Managers");
            }

            EnsureComponent<AudioManager>(managersRoot);
            EnsureComponent<CaseManager>(managersRoot);
            EnsureComponent<EvidenceManager>(managersRoot);
            EnsureComponent<InterrogationManager>(managersRoot);
            EnsureComponent<DeductionBoardController>(managersRoot);
            EnsureComponent<CaseConclusionManager>(managersRoot);
            EnsureComponent<GameBootstrap>(managersRoot);

            AudioManager audioMgr = managersRoot.GetComponent<AudioManager>();
            if (audioMgr != null)
            {
                AudioManagerSetupUtility.AssignClipsAndSources(audioMgr);
                EditorUtility.SetDirty(audioMgr);
            }

            EditorUtility.SetDirty(managersRoot);
        }

        private static void WireCanvasMainUI(bool isMenuScene)
        {
            Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{UIDir}/Canvas_MainUI.prefab");
                if (canvasPrefab != null)
                {
                    canvas = (PrefabUtility.InstantiatePrefab(canvasPrefab) as GameObject)?.GetComponent<Canvas>();
                }
            }

            if (canvas == null) return;

            canvas.pixelPerfect = true;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 5f;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>() ?? canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.dynamicPixelsPerUnit = 3.0f;

            UIManager uiManager = canvas.GetComponent<UIManager>() ?? canvas.gameObject.AddComponent<UIManager>();

            // Wire Panel references
            EnsurePanelInstance(canvas.transform, "Panel_HeaderNav", $"{PanelsDir}/Panel_HeaderNav.prefab");
            EnsurePanelInstance(canvas.transform, "Panel_Dialogue", $"{PanelsDir}/Panel_Dialogue.prefab");
            GameObject inspectPanel = EnsurePanelInstance(canvas.transform, "Panel_InspectModal", $"{PanelsDir}/Panel_InspectModal.prefab");
            GameObject nbPanel = EnsurePanelInstance(canvas.transform, "Panel_CaseFileNotebook", $"{PanelsDir}/Panel_CaseFileNotebook.prefab")
                             ?? EnsurePanelInstance(canvas.transform, "Panel_CaseFileNotebook", "Assets/Prefabs/Panel_CaseFileNotebook.prefab");
            GameObject dedPanel = EnsurePanelInstance(canvas.transform, "Panel_DeductionBoard", $"{PanelsDir}/Panel_DeductionBoard.prefab");
            GameObject concPanel = EnsurePanelInstance(canvas.transform, "Panel_ConclusionQuiz", $"{PanelsDir}/Panel_ConclusionQuiz.prefab");
            GameObject resPanel = EnsurePanelInstance(canvas.transform, "Panel_ResultsScreen", $"{PanelsDir}/Panel_ResultsScreen.prefab");
            GameObject invPanel = EnsurePanelInstance(canvas.transform, "Panel_InvestigatorSelect", $"{PanelsDir}/Panel_InvestigatorSelect.prefab");

            if (inspectPanel != null) uiManager.inspectModalPanel = inspectPanel;
            if (nbPanel != null) uiManager.notebookPanel = nbPanel;
            if (dedPanel != null) uiManager.deductionBoardPanel = dedPanel;
            if (concPanel != null) uiManager.conclusionQuizPanel = concPanel;
            if (resPanel != null) uiManager.resultsScreenPanel = resPanel;
            if (invPanel != null) uiManager.investigatorSelectPanel = invPanel;

            Transform headerNav = canvas.transform.Find("Panel_HeaderNav");
            if (headerNav != null)
            {
                Transform tContainer = headerNav.Find("Container_Timer");
                if (tContainer != null) uiManager.timerContainer = tContainer.gameObject;

                Transform nbBtn = headerNav.Find("Button_Notebook");
                if (nbBtn != null) uiManager.notebookButton = nbBtn.gameObject;

                Transform dedBtn = headerNav.Find("Button_DeductionBoard");
                if (dedBtn != null) uiManager.deductionBoardButton = dedBtn.gameObject;

                Transform concBtn = headerNav.Find("Button_Conclude");
                if (concBtn != null) uiManager.concludeCaseButton = concBtn.gameObject;

                Transform invBtn = headerNav.Find("Button_InvestigatorSelect");
                if (invBtn != null) uiManager.investigatorSelectButton = invBtn.gameObject;

                Transform menuBtn = headerNav.Find("Button_Menu") ?? headerNav.Find("Button_ReturnToMenu");
                if (menuBtn != null) uiManager.returnToMenuButton = menuBtn.gameObject;
            }

            EditorUtility.SetDirty(uiManager);
            EditorUtility.SetDirty(canvas);
        }

        private static GameObject EnsurePanelInstance(Transform canvasTrans, string panelName, string prefabPath)
        {
            Transform existing = canvasTrans.Find(panelName);
            if (existing != null) return existing.gameObject;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab, canvasTrans) as GameObject;
                if (instance != null)
                {
                    instance.name = panelName;
                    return instance;
                }
            }
            return null;
        }

        private static void WireDetectiveArmPointer()
        {
            GameObject armObj = GameObject.Find("Detective_Arm_Pointer");
            if (armObj == null)
            {
                GameObject armPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{CoreDir}/Core_DetectiveArmPointer.prefab");
                if (armPrefab != null)
                {
                    armObj = PrefabUtility.InstantiatePrefab(armPrefab) as GameObject;
                }
            }

            if (armObj != null)
            {
                SpriteRenderer sr = armObj.GetComponent<SpriteRenderer>() ?? armObj.AddComponent<SpriteRenderer>();
                if (sr.sprite == null)
                {
                    sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ArmPointer.png");
                }
                sr.sortingOrder = 20;

                ArmPointerController apc = armObj.GetComponent<ArmPointerController>() ?? armObj.AddComponent<ArmPointerController>();
                apc.armRenderer = sr;
                apc.targetCamera = Camera.main;

                Transform fingertip = armObj.transform.Find("FingertipPoint");
                if (fingertip == null)
                {
                    GameObject ftObj = new GameObject("FingertipPoint");
                    ftObj.transform.SetParent(armObj.transform, false);
                    ftObj.transform.localPosition = new Vector3(0f, 2.2f, 0f);
                    fingertip = ftObj.transform;
                }
                apc.fingertipPoint = fingertip;
                EditorUtility.SetDirty(apc);
                EditorUtility.SetDirty(armObj);
            }
        }

        private static void WireCharacters()
        {
            GameObject charactersRoot = GameObject.Find("Characters");
            if (charactersRoot == null) return;

            CharacterDisplay[] displays = charactersRoot.GetComponentsInChildren<CharacterDisplay>(true);
            foreach (var display in displays)
            {
                if (display.characterSpriteRenderer == null)
                {
                    display.characterSpriteRenderer = display.GetComponent<SpriteRenderer>();
                }
                EditorUtility.SetDirty(display);
            }
        }

        private static void WireTableItems()
        {
            GameObject itemsRoot = GameObject.Find("Items");
            if (itemsRoot == null) return;

            TableEvidenceItem[] evidenceItems = itemsRoot.GetComponentsInChildren<TableEvidenceItem>(true);
            foreach (var item in evidenceItems)
            {
                if (item.spriteRenderer == null)
                {
                    item.spriteRenderer = item.GetComponent<SpriteRenderer>();
                }
                if (item.spriteRenderer != null)
                {
                    item.spriteRenderer.sortingOrder = 10;
                }

                BoxCollider2D col = item.GetComponent<BoxCollider2D>() ?? item.gameObject.AddComponent<BoxCollider2D>();
                col.isTrigger = true;

                item.glowColor = new Color(1.0f, 0.88f, 0.25f, 0.95f);
                item.maxGlowIntensity = 1.0f;
                item.haloBaseScale = 1.08f;
                item.glowFadeSpeed = 12f;
                item.pulseGlow = true;
                item.pulseSpeed = 4.0f;
                item.scaleOnHover = true;
                item.hoverScaleMultiplier = 1.035f;

                EditorUtility.SetDirty(item);
            }
        }

        #endregion

        #region Helpers

        private static GameObject SaveAndDestroy(GameObject tempObj, string prefabPath)
        {
            EnsureDirectory(Path.GetDirectoryName(prefabPath));
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(tempObj, prefabPath);
            UnityEngine.Object.DestroyImmediate(tempObj);
            return prefab;
        }

        private static Text AddTextChild(Transform parent, string name, string text, int fontSize, FontStyle style,
            TextAnchor alignment, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 sizeDelta, bool addShadow = true)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Text t = go.GetComponent<Text>();
            t.font = GetDefaultFont();
            t.text = text;
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.alignment = alignment;
            t.color = color;
            t.raycastTarget = false;

            if (addShadow)
            {
                Shadow s = go.AddComponent<Shadow>();
                s.effectDistance = new Vector2(1.2f, -1.2f);
                s.effectColor = new Color(0f, 0f, 0f, 0.85f);
                s.useGraphicAlpha = true;
            }

            return t;
        }

        private static Button AddButtonChild(Transform parent, string name, string label, int fontSize,
            Color btnColor, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Image img = go.GetComponent<Image>();
            img.color = btnColor;
            img.raycastTarget = true;

            Button btn = go.GetComponent<Button>();

            AddTextChild(go.transform, "Text_Label", label, fontSize, FontStyle.Bold,
                TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one,
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, true);

            return btn;
        }

        private static T EnsureComponent<T>(GameObject target) where T : Component
        {
            T comp = target.GetComponent<T>();
            if (comp == null)
            {
                comp = target.AddComponent<T>();
            }
            return comp;
        }

        #endregion
    }
}
