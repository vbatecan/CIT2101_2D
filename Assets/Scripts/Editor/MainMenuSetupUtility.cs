using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using CaseClosed.UI;

namespace CaseClosed.Editor
{
    /// <summary>
    /// Editor utility for constructing and updating a symmetric, production-grade Main Menu UI hierarchy
    /// across scenes in the project. Eliminates duplicate overlapping texts, sets high-DPI canvas parameters,
    /// configures crisp button color blocks, and adds sharp drop shadows for maximum legibility.
    /// </summary>
    public static class MainMenuSetupUtility
    {
        private static readonly string[] ScenePaths = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Main.unity"
        };

        private static Font defaultFont;

        private static Font GetFont()
        {
            if (defaultFont == null)
            {
                defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            return defaultFont;
        }

        [MenuItem("Case Closed/UI/Rebuild Main Menu UI (Symmetric & Production-Grade)", false, 20)]
        public static void RebuildMainMenuInAllScenes()
        {
            int configured = 0;
            string initialScene = EditorSceneManager.GetActiveScene().path;

            foreach (string scenePath in ScenePaths)
            {
                if (!File.Exists(scenePath)) continue;

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                bool success = RebuildMainMenuInCurrentScene();

                if (success)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    configured++;
                    Debug.Log($"[MainMenuSetup] Successfully rebuilt Main Menu in: '{scenePath}'");
                }
            }

            if (!string.IsNullOrEmpty(initialScene) && File.Exists(initialScene))
            {
                EditorSceneManager.OpenScene(initialScene, OpenSceneMode.Single);
            }

            Debug.Log($"[MainMenuSetup] Rebuild complete! Configured {configured} scene(s).");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static bool RebuildMainMenuInCurrentScene()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas_MainUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObj.GetComponent<Canvas>();
            }

            Camera mainCam = Camera.main;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = mainCam;
            canvas.planeDistance = 5f;
            canvas.pixelPerfect = true;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.dynamicPixelsPerUnit = 3.0f; // 3x high-DPI dynamic font atlas rasterization

            Transform panelTransform = canvas.transform.Find("Panel_MainMenu");
            GameObject panelObj;
            if (panelTransform == null)
            {
                panelObj = new GameObject("Panel_MainMenu", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(MainMenuUI));
                panelObj.transform.SetParent(canvas.transform, false);
            }
            else
            {
                panelObj = panelTransform.gameObject;
            }

            RectTransform panelRT = panelObj.GetComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            Image panelImg = panelObj.GetComponent<Image>();
            panelImg.color = new Color(0.06f, 0.07f, 0.09f, 1f);

            MainMenuUI menuUI = panelObj.GetComponent<MainMenuUI>();
            if (menuUI == null) menuUI = panelObj.AddComponent<MainMenuUI>();

            // 1. Title & Subtitle
            EnsureHeader(panelObj.transform);

            // 2. Main Navigation Buttons Container
            GameObject mainButtons = EnsureMainButtonsContainer(panelObj.transform, menuUI);

            // 3. Case Select Container (Symmetric 3-Card Dossier)
            GameObject caseSelect = EnsureCaseSelectContainer(panelObj.transform, menuUI);

            // 4. Audio & Gameplay Settings Container (Symmetric 2-Column)
            GameObject settings = EnsureSettingsContainer(panelObj.transform, menuUI);

            // 5. Exit Confirmation Modal
            GameObject exitConfirm = EnsureExitConfirmationModal(panelObj.transform, menuUI);

            // Preserve and sanitize HowToPlay and Credits containers
            Transform howToPlay = panelObj.transform.Find("Container_HowToPlay");
            if (howToPlay != null)
            {
                menuUI.howToPlayContainer = howToPlay.gameObject;
                Transform htBack = howToPlay.Find("Button_Back");
                if (htBack != null) CleanButtonTexts(htBack.gameObject, "BACK TO MENU", 16);
            }

            Transform credits = panelObj.transform.Find("Container_Credits");
            if (credits != null)
            {
                menuUI.creditsContainer = credits.gameObject;
                Transform crBack = credits.Find("Button_Back");
                if (crBack != null) CleanButtonTexts(crBack.gameObject, "BACK TO MENU", 16);
            }

            menuUI.mainButtonsContainer = mainButtons;
            menuUI.caseSelectContainer = caseSelect;
            menuUI.settingsContainer = settings;
            menuUI.exitConfirmContainer = exitConfirm;

            // Initially set active states
            mainButtons.SetActive(true);
            caseSelect.SetActive(false);
            settings.SetActive(false);
            exitConfirm.SetActive(false);
            if (howToPlay != null) howToPlay.gameObject.SetActive(false);
            if (credits != null) credits.gameObject.SetActive(false);

            EditorUtility.SetDirty(menuUI);
            return true;
        }

        private static void EnsureHeader(Transform parent)
        {
            Transform titleT = parent.Find("Text_GameTitle");
            GameObject titleObj = titleT != null ? titleT.gameObject : new GameObject("Text_GameTitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            titleObj.transform.SetParent(parent, false);
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.78f);
            titleRT.anchorMax = new Vector2(0.5f, 0.90f);
            titleRT.sizeDelta = new Vector2(900, 80);
            titleRT.anchoredPosition = Vector2.zero;

            Text titleText = titleObj.GetComponent<Text>();
            titleText.text = "CASE CLOSED";
            titleText.font = GetFont();
            titleText.fontSize = 58;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(0.96f, 0.84f, 0.45f, 1f); // Rich Warm Gold
            AddShadow(titleObj, new Vector2(2f, -2f), new Color(0f, 0f, 0f, 0.95f));

            Transform subT = parent.Find("Text_GameSubtitle");
            GameObject subObj = subT != null ? subT.gameObject : new GameObject("Text_GameSubtitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            subObj.transform.SetParent(parent, false);
            RectTransform subRT = subObj.GetComponent<RectTransform>();
            subRT.anchorMin = new Vector2(0.5f, 0.72f);
            subRT.anchorMax = new Vector2(0.5f, 0.78f);
            subRT.sizeDelta = new Vector2(900, 40);
            subRT.anchoredPosition = Vector2.zero;

            Text subText = subObj.GetComponent<Text>();
            subText.text = "DETECTIVE INVESTIGATION & INTERROGATION SIMULATION";
            subText.font = GetFont();
            subText.fontSize = 16;
            subText.fontStyle = FontStyle.Bold;
            subText.alignment = TextAnchor.MiddleCenter;
            subText.color = new Color(0.78f, 0.83f, 0.90f, 0.95f);
            AddShadow(subObj, new Vector2(1.2f, -1.2f), new Color(0f, 0f, 0f, 0.9f));
        }

        private static GameObject EnsureMainButtonsContainer(Transform parent, MainMenuUI menuUI)
        {
            Transform contT = parent.Find("Container_MainButtons");
            GameObject cont = contT != null ? contT.gameObject : new GameObject("Container_MainButtons", typeof(RectTransform));
            cont.transform.SetParent(parent, false);

            RectTransform rt = cont.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, -60);
            rt.sizeDelta = new Vector2(400, 400);

            // 6 Symmetric Buttons with comfortable vertical breathing room (14px gap)
            menuUI.playButton = CreateMenuButton(cont.transform, "Button_Play", "START INVESTIGATION", new Vector2(0, 145), new Vector2(380, 52), new Color(0.92f, 0.76f, 0.32f, 1f), new Color(0.08f, 0.08f, 0.12f, 1f), 18);
            menuUI.caseSelectButton = CreateMenuButton(cont.transform, "Button_CaseSelect", "CASE FILES", new Vector2(0, 85), new Vector2(380, 48), new Color(0.18f, 0.23f, 0.31f, 1f), Color.white, 16);
            menuUI.howToPlayButton = CreateMenuButton(cont.transform, "Button_HowToPlay", "CASE HANDBOOK", new Vector2(0, 25), new Vector2(380, 48), new Color(0.18f, 0.23f, 0.31f, 1f), Color.white, 16);
            menuUI.settingsButton = CreateMenuButton(cont.transform, "Button_Settings", "AUDIO & SETTINGS", new Vector2(0, -35), new Vector2(380, 48), new Color(0.18f, 0.23f, 0.31f, 1f), Color.white, 16);
            menuUI.creditsButton = CreateMenuButton(cont.transform, "Button_Credits", "CREDITS", new Vector2(0, -95), new Vector2(380, 48), new Color(0.18f, 0.23f, 0.31f, 1f), Color.white, 16);
            menuUI.quitButton = CreateMenuButton(cont.transform, "Button_Quit", "EXIT GAME", new Vector2(0, -155), new Vector2(380, 48), new Color(0.55f, 0.18f, 0.18f, 1f), new Color(1f, 0.94f, 0.94f, 1f), 16);

            return cont;
        }

        private static GameObject EnsureCaseSelectContainer(Transform parent, MainMenuUI menuUI)
        {
            Transform contT = parent.Find("Container_CaseSelect");
            GameObject cont = contT != null ? contT.gameObject : new GameObject("Container_CaseSelect", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            cont.transform.SetParent(parent, false);

            RectTransform rt = cont.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, -30);
            rt.sizeDelta = new Vector2(760, 540);

            Image img = cont.GetComponent<Image>();
            img.color = new Color(0.10f, 0.12f, 0.16f, 0.97f);

            // Header Title & Subtitle
            CreateLabel(cont.transform, "Text_Header", "INVESTIGATION DOSSIERS", new Vector2(0, 225), new Vector2(720, 38), 24, FontStyle.Bold, new Color(0.96f, 0.84f, 0.45f, 1f), TextAnchor.MiddleCenter);
            CreateLabel(cont.transform, "Text_SubHeader", "Select an active or completed case file to launch investigation", new Vector2(0, 195), new Vector2(720, 24), 14, FontStyle.Normal, new Color(0.78f, 0.83f, 0.90f, 0.95f), TextAnchor.MiddleCenter);

            // 3 Dossier Cards
            CreateCaseCard(cont.transform, 1, "CASE 01: THE STOLEN NECKLACE", "Suspect: Victoria Sterling | Crime Scene: Grand Manor", new Vector2(0, 100), out menuUI.case01Button, out menuUI.case01TitleText, out menuUI.case01StatusText);
            CreateCaseCard(cont.transform, 2, "CASE 02: THE SHATTERED MIRROR", "Suspects: Museum Guard & Owner | Crime Scene: Art Gallery", new Vector2(0, 0), out menuUI.case02Button, out menuUI.case02TitleText, out menuUI.case02StatusText);
            CreateCaseCard(cont.transform, 3, "CASE 03: THE LAST CALL", "Suspect: Shanaia Ortega | Crime Scene: Downtown Cafe Office", new Vector2(0, -100), out menuUI.case03Button, out menuUI.case03TitleText, out menuUI.case03StatusText);

            // Back button
            menuUI.backFromCaseSelectButton = CreateMenuButton(cont.transform, "Button_Back", "BACK TO MENU", new Vector2(0, -220), new Vector2(280, 46), new Color(0.20f, 0.25f, 0.34f, 1f), Color.white, 16);

            return cont;
        }

        private static void CreateCaseCard(Transform parent, int caseNum, string defaultTitle, string subtitle, Vector2 pos, out Button button, out Text titleText, out Text statusText)
        {
            string name = $"Button_Case0{caseNum}";
            Transform btnT = parent.Find(name);
            GameObject btnObj = btnT != null ? btnT.gameObject : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(690, 82);

            Image img = btnObj.GetComponent<Image>();
            img.color = new Color(0.16f, 0.20f, 0.28f, 1f);

            button = btnObj.GetComponent<Button>();
            ColorBlock cb = button.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            cb.selectedColor = Color.white;
            button.colors = cb;

            // CRITICAL: Clean up any old generic button text (Text_Label or Text) that might overlap with card labels
            Transform oldLabel = btnObj.transform.Find("Text_Label");
            if (oldLabel != null) Object.DestroyImmediate(oldLabel.gameObject);
            Transform oldText = btnObj.transform.Find("Text");
            if (oldText != null) Object.DestroyImmediate(oldText.gameObject);

            // Case Number Tag
            CreateLabel(btnObj.transform, "Text_Tag", $"[0{caseNum}]", new Vector2(-295, 0), new Vector2(64, 44), 18, FontStyle.Bold, new Color(0.96f, 0.84f, 0.45f, 1f), TextAnchor.MiddleCenter);

            // Title
            Transform titleT = btnObj.transform.Find("Text_Title");
            GameObject titleObj = titleT != null ? titleT.gameObject : new GameObject("Text_Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            titleObj.transform.SetParent(btnObj.transform, false);
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchoredPosition = new Vector2(-55, 14);
            titleRT.sizeDelta = new Vector2(390, 28);
            titleText = titleObj.GetComponent<Text>();
            titleText.text = defaultTitle;
            titleText.font = GetFont();
            titleText.fontSize = 17;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleLeft;
            titleText.color = Color.white;
            AddShadow(titleObj, new Vector2(1.2f, -1.2f), new Color(0f, 0f, 0f, 0.85f));

            // Subtitle
            CreateLabel(btnObj.transform, "Text_Subtitle", subtitle, new Vector2(-55, -14), new Vector2(390, 24), 13, FontStyle.Normal, new Color(0.78f, 0.83f, 0.90f, 0.95f), TextAnchor.MiddleLeft);

            // Status Badge
            Transform statT = btnObj.transform.Find("Text_Status");
            GameObject statObj = statT != null ? statT.gameObject : new GameObject("Text_Status", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            statObj.transform.SetParent(btnObj.transform, false);
            RectTransform statRT = statObj.GetComponent<RectTransform>();
            statRT.anchoredPosition = new Vector2(215, 0);
            statRT.sizeDelta = new Vector2(210, 38);
            statusText = statObj.GetComponent<Text>();
            statusText.text = caseNum == 1 ? "[ AVAILABLE ]" : "[ LOCKED \uD83D\uDD12 ]";
            statusText.font = GetFont();
            statusText.fontSize = 14;
            statusText.fontStyle = FontStyle.Bold;
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.color = caseNum == 1 ? new Color(0.45f, 0.95f, 0.45f, 1f) : new Color(0.90f, 0.45f, 0.45f, 0.95f);
            AddShadow(statObj, new Vector2(1.2f, -1.2f), new Color(0f, 0f, 0f, 0.85f));
        }

        private static GameObject EnsureSettingsContainer(Transform parent, MainMenuUI menuUI)
        {
            Transform contT = parent.Find("Container_Settings");
            GameObject cont = contT != null ? contT.gameObject : new GameObject("Container_Settings", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            cont.transform.SetParent(parent, false);

            // Clean up old legacy labels if present
            Transform oldBgm = cont.transform.Find("Text_BGMLabel");
            if (oldBgm != null) Object.DestroyImmediate(oldBgm.gameObject);
            Transform oldSfx = cont.transform.Find("Text_SFXLabel");
            if (oldSfx != null) Object.DestroyImmediate(oldSfx.gameObject);

            RectTransform rt = cont.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, -30);
            rt.sizeDelta = new Vector2(780, 520);

            Image img = cont.GetComponent<Image>();
            img.color = new Color(0.10f, 0.12f, 0.16f, 0.97f);

            // Header
            CreateLabel(cont.transform, "Text_Header", "AUDIO & GAMEPLAY SETTINGS", new Vector2(0, 220), new Vector2(720, 38), 24, FontStyle.Bold, new Color(0.96f, 0.84f, 0.45f, 1f), TextAnchor.MiddleCenter);
            CreateLabel(cont.transform, "Text_SubHeader", "Adjust volume channels, display presentation, and dialogue text pacing", new Vector2(0, 190), new Vector2(720, 24), 14, FontStyle.Normal, new Color(0.78f, 0.83f, 0.90f, 0.95f), TextAnchor.MiddleCenter);

            // Left Column: Audio (Center X = -190)
            CreateLabel(cont.transform, "Header_Audio", "- AUDIO CHANNELS -", new Vector2(-190, 145), new Vector2(340, 28), 16, FontStyle.Bold, new Color(0.96f, 0.84f, 0.45f, 1f), TextAnchor.MiddleCenter);

            CreateAudioChannelRow(cont.transform, "BGM", "Music Volume", new Vector2(-190, 85), out menuUI.bgmVolumeSlider, out menuUI.bgmPercentText, out menuUI.bgmMuteButton, out menuUI.bgmMuteText);
            CreateAudioChannelRow(cont.transform, "SFX", "Sound Effects Volume", new Vector2(-190, 10), out menuUI.sfxVolumeSlider, out menuUI.sfxPercentText, out menuUI.sfxMuteButton, out menuUI.sfxMuteText);
            CreateAudioChannelRow(cont.transform, "Dialog", "Dialogue Volume", new Vector2(-190, -65), out menuUI.dialogVolumeSlider, out menuUI.dialogPercentText, out menuUI.dialogMuteButton, out menuUI.dialogMuteText);

            // Right Column: Display & Gameplay (Center X = +190)
            CreateLabel(cont.transform, "Header_Display", "- DISPLAY & PACING -", new Vector2(190, 145), new Vector2(340, 28), 16, FontStyle.Bold, new Color(0.96f, 0.84f, 0.45f, 1f), TextAnchor.MiddleCenter);

            // Fullscreen / Windowed Mode
            CreateLabel(cont.transform, "Label_DisplayMode", "Display Mode", new Vector2(190, 105), new Vector2(320, 22), 14, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
            menuUI.fullscreenToggle = CreateToggle(cont.transform, "Toggle_Fullscreen", "Fullscreen Mode", new Vector2(190, 75), new Vector2(320, 30));
            
            Transform fsStatusT = cont.transform.Find("Text_FullscreenStatus");
            GameObject fsStatusObj = fsStatusT != null ? fsStatusT.gameObject : new GameObject("Text_FullscreenStatus", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            fsStatusObj.transform.SetParent(cont.transform, false);
            RectTransform fsStatusRT = fsStatusObj.GetComponent<RectTransform>();
            fsStatusRT.anchoredPosition = new Vector2(190, 48);
            fsStatusRT.sizeDelta = new Vector2(320, 20);
            menuUI.fullscreenStatusText = fsStatusObj.GetComponent<Text>();
            menuUI.fullscreenStatusText.text = "FULLSCREEN (Borderless)";
            menuUI.fullscreenStatusText.font = GetFont();
            menuUI.fullscreenStatusText.fontSize = 13;
            menuUI.fullscreenStatusText.color = new Color(0.78f, 0.83f, 0.90f, 0.95f);
            AddShadow(fsStatusObj, new Vector2(1.2f, -1.2f), new Color(0f, 0f, 0f, 0.85f));

            // Text Speed
            CreateLabel(cont.transform, "Label_TextSpeed", "Dialogue Text Speed", new Vector2(190, 10), new Vector2(320, 22), 14, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
            Transform speedLblT = cont.transform.Find("Text_TextSpeedValue");
            GameObject speedLblObj = speedLblT != null ? speedLblT.gameObject : new GameObject("Text_TextSpeedValue", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            speedLblObj.transform.SetParent(cont.transform, false);
            RectTransform speedLblRT = speedLblObj.GetComponent<RectTransform>();
            speedLblRT.anchoredPosition = new Vector2(270, 10);
            speedLblRT.sizeDelta = new Vector2(160, 22);
            menuUI.textSpeedText = speedLblObj.GetComponent<Text>();
            menuUI.textSpeedText.text = "Normal (35 cps)";
            menuUI.textSpeedText.font = GetFont();
            menuUI.textSpeedText.fontSize = 14;
            menuUI.textSpeedText.fontStyle = FontStyle.Bold;
            menuUI.textSpeedText.alignment = TextAnchor.MiddleRight;
            menuUI.textSpeedText.color = new Color(0.96f, 0.84f, 0.45f, 1f);
            AddShadow(speedLblObj, new Vector2(1.2f, -1.2f), new Color(0f, 0f, 0f, 0.85f));

            menuUI.textSpeedSlider = CreateSlider(cont.transform, "Slider_TextSpeed", new Vector2(190, -18), new Vector2(320, 20), 15f, 80f, 35f);

            // Typewriter Sound Toggle
            CreateLabel(cont.transform, "Label_Typewriter", "Typewriter Sound Effects", new Vector2(190, -55), new Vector2(320, 22), 14, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
            menuUI.typewriterToggle = CreateToggle(cont.transform, "Toggle_Typewriter", "Enable Typing Click SFX", new Vector2(190, -82), new Vector2(320, 30));

            // Symmetrical Bottom Actions
            menuUI.resetSettingsButton = CreateMenuButton(cont.transform, "Button_ResetSettings", "RESET DEFAULTS", new Vector2(-150, -210), new Vector2(250, 46), new Color(0.45f, 0.18f, 0.18f, 1f), new Color(1f, 0.92f, 0.92f, 1f), 15);
            menuUI.backFromSettingsButton = CreateMenuButton(cont.transform, "Button_Back", "BACK TO MENU", new Vector2(150, -210), new Vector2(250, 46), new Color(0.20f, 0.25f, 0.34f, 1f), Color.white, 16);

            return cont;
        }

        private static void CreateAudioChannelRow(Transform parent, string id, string labelText, Vector2 pos, out Slider slider, out Text percentText, out Button muteBtn, out Text muteText)
        {
            // Title & Readout Row
            CreateLabel(parent, $"Label_{id}", labelText, new Vector2(pos.x - 30, pos.y + 20), new Vector2(260, 22), 14, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);

            Transform pctT = parent.Find($"Text_{id}Percent");
            GameObject pctObj = pctT != null ? pctT.gameObject : new GameObject($"Text_{id}Percent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            pctObj.transform.SetParent(parent, false);
            RectTransform pctRT = pctObj.GetComponent<RectTransform>();
            pctRT.anchoredPosition = new Vector2(pos.x + 130, pos.y + 20);
            pctRT.sizeDelta = new Vector2(60, 22);
            percentText = pctObj.GetComponent<Text>();
            percentText.text = "100%";
            percentText.font = GetFont();
            percentText.fontSize = 15;
            percentText.fontStyle = FontStyle.Bold;
            percentText.alignment = TextAnchor.MiddleRight;
            percentText.color = new Color(0.96f, 0.84f, 0.45f, 1f);
            AddShadow(pctObj, new Vector2(1.2f, -1.2f), new Color(0f, 0f, 0f, 0.85f));

            // Slider
            slider = CreateSlider(parent, $"Slider_{id}", new Vector2(pos.x - 45, pos.y - 8), new Vector2(230, 20), 0f, 1f, 1f);

            // Mute Button
            muteBtn = CreateMenuButton(parent, $"Button_Mute_{id}", "MUTE", new Vector2(pos.x + 120, pos.y - 8), new Vector2(82, 30), new Color(0.26f, 0.31f, 0.40f, 1f), Color.white, 12);
            muteText = muteBtn.GetComponentInChildren<Text>();
        }

        private static GameObject EnsureExitConfirmationModal(Transform parent, MainMenuUI menuUI)
        {
            Transform contT = parent.Find("Container_ExitConfirm");
            GameObject cont = contT != null ? contT.gameObject : new GameObject("Container_ExitConfirm", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            cont.transform.SetParent(parent, false);

            RectTransform rt = cont.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Semi-transparent backdrop scrim
            Image img = cont.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.88f);
            img.raycastTarget = true;

            // Centered Modal Dialog Window
            Transform boxT = cont.transform.Find("Dialog_Window");
            GameObject box = boxT != null ? boxT.gameObject : new GameObject("Dialog_Window", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            box.transform.SetParent(cont.transform, false);

            RectTransform boxRT = box.GetComponent<RectTransform>();
            boxRT.anchorMin = new Vector2(0.5f, 0.5f);
            boxRT.anchorMax = new Vector2(0.5f, 0.5f);
            boxRT.anchoredPosition = Vector2.zero;
            boxRT.sizeDelta = new Vector2(540, 250);

            Image boxImg = box.GetComponent<Image>();
            boxImg.color = new Color(0.12f, 0.14f, 0.18f, 1f);

            // Title
            CreateLabel(box.transform, "Text_ExitTitle", "EXIT INVESTIGATION", new Vector2(0, 78), new Vector2(480, 38), 24, FontStyle.Bold, new Color(0.95f, 0.42f, 0.42f, 1f), TextAnchor.MiddleCenter);

            // Body
            CreateLabel(box.transform, "Text_ExitBody", "Are you sure you want to close Case Closed\nand exit to the desktop?", new Vector2(0, 16), new Vector2(480, 52), 16, FontStyle.Normal, new Color(0.88f, 0.90f, 0.94f, 1f), TextAnchor.MiddleCenter);

            // Twin Buttons
            menuUI.confirmExitNoButton = CreateMenuButton(box.transform, "Button_ExitNo", "NO, RETURN", new Vector2(-125, -65), new Vector2(200, 46), new Color(0.22f, 0.27f, 0.35f, 1f), Color.white, 15);
            menuUI.confirmExitYesButton = CreateMenuButton(box.transform, "Button_ExitYes", "YES, EXIT", new Vector2(125, -65), new Vector2(200, 46), new Color(0.60f, 0.18f, 0.18f, 1f), Color.white, 15);

            return cont;
        }

        private static Text CreateLabel(Transform parent, string name, string text, Vector2 pos, Vector2 size, int fontSize, FontStyle style, Color color, TextAnchor align, bool addShadow = true)
        {
            Transform t = parent.Find(name);
            GameObject obj = t != null ? t.gameObject : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Text txt = obj.GetComponent<Text>();
            txt.text = text;
            txt.font = GetFont();
            txt.fontSize = fontSize;
            txt.fontStyle = style;
            txt.alignment = align;
            txt.color = color;

            if (addShadow)
            {
                AddShadow(obj, new Vector2(1.2f, -1.2f), new Color(0f, 0f, 0f, 0.85f));
            }
            return txt;
        }

        private static void CleanButtonTexts(GameObject btnObj, string label, int fontSize, Color? textColor = null)
        {
            // 1. Remove legacy Text_Label if present
            Transform oldLabel = btnObj.transform.Find("Text_Label");
            if (oldLabel != null) Object.DestroyImmediate(oldLabel.gameObject);

            // 2. Ensure single "Text" child exists
            Transform lblT = btnObj.transform.Find("Text");
            GameObject lblObj = lblT != null ? lblT.gameObject : new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            lblObj.transform.SetParent(btnObj.transform, false);

            // 3. Remove any other orphaned text children to guarantee no ghosting/double layer
            for (int i = btnObj.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = btnObj.transform.GetChild(i);
                if (child != lblObj.transform && child.name.StartsWith("Text"))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            RectTransform lblRT = lblObj.GetComponent<RectTransform>();
            lblRT.anchorMin = Vector2.zero;
            lblRT.anchorMax = Vector2.one;
            lblRT.offsetMin = Vector2.zero;
            lblRT.offsetMax = Vector2.zero;

            Text txt = lblObj.GetComponent<Text>();
            txt.text = label;
            txt.font = GetFont();
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = textColor ?? Color.white;

            // Only add shadow if text is not dark charcoal
            Color actualColor = textColor ?? Color.white;
            if (actualColor.grayscale > 0.3f)
            {
                AddShadow(lblObj, new Vector2(1.2f, -1.2f), new Color(0f, 0f, 0f, 0.85f));
            }
            else
            {
                Shadow s = lblObj.GetComponent<Shadow>();
                if (s != null) Object.DestroyImmediate(s);
            }
        }

        private static Button CreateMenuButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Color bgColor, Color textColor, int fontSize = 16)
        {
            Transform btnT = parent.Find(name);
            GameObject btnObj = btnT != null ? btnT.gameObject : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = btnObj.GetComponent<Image>();
            img.color = bgColor;

            Button btn = btnObj.GetComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.18f, 1.18f, 1.18f, 1f);
            cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            cb.selectedColor = Color.white;
            cb.colorMultiplier = 1f;
            btn.colors = cb;

            CleanButtonTexts(btnObj, label, fontSize, textColor);

            return btn;
        }

        private static void AddShadow(GameObject obj, Vector2 offset, Color color)
        {
            Shadow shadow = obj.GetComponent<Shadow>();
            if (shadow == null) shadow = obj.AddComponent<Shadow>();
            shadow.effectDistance = offset;
            shadow.effectColor = color;
            shadow.useGraphicAlpha = true;
        }

        private static Slider CreateSlider(Transform parent, string name, Vector2 pos, Vector2 size, float minVal, float maxVal, float defaultVal)
        {
            Transform sldT = parent.Find(name);
            GameObject sldObj = sldT != null ? sldT.gameObject : new GameObject(name, typeof(RectTransform), typeof(Slider));
            sldObj.transform.SetParent(parent, false);

            RectTransform rt = sldObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Slider slider = sldObj.GetComponent<Slider>();
            slider.minValue = minVal;
            slider.maxValue = maxVal;
            slider.value = defaultVal;

            // Background track
            Transform bgT = sldObj.transform.Find("Background");
            GameObject bgObj = bgT != null ? bgT.gameObject : new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgObj.transform.SetParent(sldObj.transform, false);
            RectTransform bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0, 0.25f);
            bgRT.anchorMax = new Vector2(1, 0.75f);
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            bgObj.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.24f, 1f);

            // Fill Area & Fill
            Transform fillAreaT = sldObj.transform.Find("Fill Area");
            GameObject fillAreaObj = fillAreaT != null ? fillAreaT.gameObject : new GameObject("Fill Area", typeof(RectTransform));
            fillAreaObj.transform.SetParent(sldObj.transform, false);
            RectTransform fillAreaRT = fillAreaObj.GetComponent<RectTransform>();
            fillAreaRT.anchorMin = new Vector2(0, 0.25f);
            fillAreaRT.anchorMax = new Vector2(1, 0.75f);
            fillAreaRT.offsetMin = new Vector2(5, 0);
            fillAreaRT.offsetMax = new Vector2(-5, 0);

            Transform fillT = fillAreaObj.transform.Find("Fill");
            GameObject fillObj = fillT != null ? fillT.gameObject : new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            RectTransform fillRT = fillObj.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;
            Image fillImg = fillObj.GetComponent<Image>();
            fillImg.color = new Color(0.96f, 0.84f, 0.45f, 1f);

            slider.fillRect = fillRT;

            // Handle Area & Handle
            Transform handleAreaT = sldObj.transform.Find("Handle Slide Area");
            GameObject handleAreaObj = handleAreaT != null ? handleAreaT.gameObject : new GameObject("Handle Slide Area", typeof(RectTransform));
            handleAreaObj.transform.SetParent(sldObj.transform, false);
            RectTransform handleAreaRT = handleAreaObj.GetComponent<RectTransform>();
            handleAreaRT.anchorMin = Vector2.zero;
            handleAreaRT.anchorMax = Vector2.one;
            handleAreaRT.offsetMin = new Vector2(10, 0);
            handleAreaRT.offsetMax = new Vector2(-10, 0);

            Transform handleT = handleAreaObj.transform.Find("Handle");
            GameObject handleObj = handleT != null ? handleT.gameObject : new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            handleObj.transform.SetParent(handleAreaObj.transform, false);
            RectTransform handleRT = handleObj.GetComponent<RectTransform>();
            handleRT.sizeDelta = new Vector2(18, 18);
            handleObj.GetComponent<Image>().color = new Color(0.95f, 0.95f, 0.95f, 1f);

            slider.handleRect = handleRT;
            slider.targetGraphic = handleObj.GetComponent<Image>();

            return slider;
        }

        private static Toggle CreateToggle(Transform parent, string name, string label, Vector2 pos, Vector2 size)
        {
            Transform togT = parent.Find(name);
            GameObject togObj = togT != null ? togT.gameObject : new GameObject(name, typeof(RectTransform), typeof(Toggle));
            togObj.transform.SetParent(parent, false);

            RectTransform rt = togObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Toggle toggle = togObj.GetComponent<Toggle>();

            // Box Background
            Transform bgT = togObj.transform.Find("Background");
            GameObject bgObj = bgT != null ? bgT.gameObject : new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgObj.transform.SetParent(togObj.transform, false);
            RectTransform bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0, 0.5f);
            bgRT.anchorMax = new Vector2(0, 0.5f);
            bgRT.anchoredPosition = new Vector2(12, 0);
            bgRT.sizeDelta = new Vector2(22, 22);
            Image bgImg = bgObj.GetComponent<Image>();
            bgImg.color = new Color(0.18f, 0.23f, 0.31f, 1f);

            // Checkmark
            Transform chkT = bgObj.transform.Find("Checkmark");
            GameObject chkObj = chkT != null ? chkT.gameObject : new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            chkObj.transform.SetParent(bgObj.transform, false);
            RectTransform chkRT = chkObj.GetComponent<RectTransform>();
            chkRT.anchorMin = new Vector2(0.5f, 0.5f);
            chkRT.anchorMax = new Vector2(0.5f, 0.5f);
            chkRT.anchoredPosition = Vector2.zero;
            chkRT.sizeDelta = new Vector2(14, 14);
            Image chkImg = chkObj.GetComponent<Image>();
            chkImg.color = new Color(0.96f, 0.84f, 0.45f, 1f);

            toggle.graphic = chkImg;
            toggle.targetGraphic = bgImg;

            // Label
            CreateLabel(togObj.transform, "Label", label, new Vector2(28, 0), new Vector2(size.x - 30, size.y), 14, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft);

            return toggle;
        }
    }
}
