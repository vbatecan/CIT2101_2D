using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using CaseClosed.UI;

namespace CaseClosed.Editor
{
    /// <summary>
    /// Editor utility for constructing and wiring CaseTimerUI and GameOverUI components
    /// across all investigation scenes in the project.
    /// </summary>
    public static class CaseTimerSetupUtility
    {
        private static readonly string[] ScenePaths = new string[]
        {
            "Assets/Scenes/Main.unity",
            "Assets/Scenes/Case001.unity",
            "Assets/Scenes/Case002.unity",
            "Assets/Scenes/Case003.unity"
        };

        [MenuItem("Case Closed/UI/Configure Case Timer and Game Over in All Scenes", false, 15)]
        public static void ConfigureAllScenes()
        {
            int configuredCount = 0;
            string currentActiveScene = EditorSceneManager.GetActiveScene().path;

            foreach (string scenePath in ScenePaths)
            {
                if (!File.Exists(scenePath))
                {
                    Debug.LogWarning($"[CaseTimerSetup] Scene file not found at: {scenePath}");
                    continue;
                }

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                bool modified = SetupTimerAndGameOverInCurrentScene();

                if (modified)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    configuredCount++;
                    Debug.Log($"[CaseTimerSetup] Successfully configured CaseTimer and GameOver in scene: '{scenePath}'");
                }
            }

            // Return to initial scene
            if (!string.IsNullOrEmpty(currentActiveScene) && File.Exists(currentActiveScene))
            {
                EditorSceneManager.OpenScene(currentActiveScene, OpenSceneMode.Single);
            }

            Debug.Log($"[CaseTimerSetup] Setup complete! Configured {configuredCount} scene(s).");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static bool SetupTimerAndGameOverInCurrentScene()
        {
            UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
            if (uiManager == null)
            {
                Debug.LogWarning("[CaseTimerSetup] No UIManager found in active scene.");
                return false;
            }

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[CaseTimerSetup] No Canvas found in active scene.");
                return false;
            }

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 1. Locate Panel_HeaderNav
            Transform headerNav = null;
            foreach (var t in canvas.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "Panel_HeaderNav" || t.name == "PanelHeaderNav")
                {
                    headerNav = t;
                    break;
                }
            }

            GameObject timerContainerObj = null;
            if (headerNav != null)
            {
                timerContainerObj = EnsureTimerUI(headerNav, defaultFont);
                if (timerContainerObj != null)
                {
                    uiManager.timerContainer = timerContainerObj;
                }
            }
            else
            {
                Debug.LogWarning("[CaseTimerSetup] Panel_HeaderNav not found in scene.");
            }

            EnsureInGameMenuUI(canvas.transform, headerNav, uiManager, defaultFont);

            // 2. Ensure GameOver Panel under Canvas
            GameObject gameOverObj = EnsureGameOverUI(canvas.transform, defaultFont);
            if (gameOverObj != null)
            {
                uiManager.gameOverPanel = gameOverObj;
            }

            EditorUtility.SetDirty(uiManager);
            return true;
        }

        private static GameObject EnsureTimerUI(Transform headerNav, Font font)
        {
            Transform existingTimer = headerNav.Find("Container_Timer");
            GameObject timerObj;

            if (existingTimer != null)
            {
                timerObj = existingTimer.gameObject;
            }
            else
            {
                timerObj = new GameObject("Container_Timer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                timerObj.transform.SetParent(headerNav, false);

                RectTransform rt = timerObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, 0f);
                rt.sizeDelta = new Vector2(160f, 44f);

                Image bg = timerObj.GetComponent<Image>();
                bg.color = new Color(0.10f, 0.12f, 0.16f, 0.95f);
                bg.raycastTarget = false;
            }

            CaseTimerUI timerUI = timerObj.GetComponent<CaseTimerUI>();
            if (timerUI == null)
            {
                timerUI = timerObj.AddComponent<CaseTimerUI>();
            }

            // Child Text_Timer
            Transform textTrans = timerObj.transform.Find("Text_Timer");
            Text textComp;
            if (textTrans != null)
            {
                textComp = textTrans.GetComponent<Text>();
            }
            else
            {
                GameObject textObj = new GameObject("Text_Timer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                textObj.transform.SetParent(timerObj.transform, false);

                RectTransform textRt = textObj.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = Vector2.zero;
                textRt.offsetMax = Vector2.zero;

                textComp = textObj.GetComponent<Text>();
                textComp.font = font;
                textComp.fontSize = 22;
                textComp.fontStyle = FontStyle.Bold;
                textComp.alignment = TextAnchor.MiddleCenter;
                textComp.color = Color.white;
                textComp.text = "05:00";
                textComp.raycastTarget = false;
            }

            var timerSerialized = new SerializedObject(timerUI);
            timerSerialized.FindProperty("timerText").objectReferenceValue = textComp;
            timerSerialized.FindProperty("backgroundBadge").objectReferenceValue = timerObj.GetComponent<Image>();
            timerSerialized.ApplyModifiedProperties();

            EditorUtility.SetDirty(timerUI);
            return timerObj;
        }

        private static GameObject EnsureGameOverUI(Transform canvasTrans, Font font)
        {
            Transform existingPanel = canvasTrans.Find("Panel_GameOver");
            GameObject panelObj;

            if (existingPanel != null)
            {
                panelObj = existingPanel.gameObject;
            }
            else
            {
                panelObj = new GameObject("Panel_GameOver", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                panelObj.transform.SetParent(canvasTrans, false);

                RectTransform rt = panelObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                Image bg = panelObj.GetComponent<Image>();
                bg.color = new Color(0.06f, 0.02f, 0.02f, 0.96f);
                bg.raycastTarget = true;
            }

            GameOverUI gameOverUI = panelObj.GetComponent<GameOverUI>();
            if (gameOverUI == null)
            {
                gameOverUI = panelObj.AddComponent<GameOverUI>();
            }

            // Card container
            Transform cardTrans = panelObj.transform.Find("Card_GameOver");
            GameObject cardObj;
            if (cardTrans != null)
            {
                cardObj = cardTrans.gameObject;
            }
            else
            {
                cardObj = new GameObject("Card_GameOver", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                cardObj.transform.SetParent(panelObj.transform, false);

                RectTransform cardRt = cardObj.GetComponent<RectTransform>();
                cardRt.anchorMin = new Vector2(0.5f, 0.5f);
                cardRt.anchorMax = new Vector2(0.5f, 0.5f);
                cardRt.pivot = new Vector2(0.5f, 0.5f);
                cardRt.anchoredPosition = Vector2.zero;
                cardRt.sizeDelta = new Vector2(600f, 440f);

                Image cardBg = cardObj.GetComponent<Image>();
                cardBg.color = new Color(0.12f, 0.07f, 0.07f, 1f);
            }

            // Title Text
            Text titleText = EnsureTextChild(cardObj.transform, "Text_Title", font, 28, FontStyle.Bold,
                new Vector2(0f, 160f), new Vector2(560f, 50f), TextAnchor.MiddleCenter, new Color(1f, 0.25f, 0.25f));
            titleText.text = "LEVEL 1: TIME EXPIRED";

            // Subtitle Text
            Text subtitleText = EnsureTextChild(cardObj.transform, "Text_Subtitle", font, 15, FontStyle.Italic,
                new Vector2(0f, 115f), new Vector2(560f, 35f), TextAnchor.MiddleCenter, new Color(0.82f, 0.82f, 0.82f));
            subtitleText.text = "Investigation Failed — The suspect slipped away before the case was closed.";

            // Details Breakdown Text
            Text detailsText = EnsureTextChild(cardObj.transform, "Text_Details", font, 17, FontStyle.Normal,
                new Vector2(0f, 0f), new Vector2(500f, 170f), TextAnchor.UpperLeft, Color.white);
            detailsText.text = "Lead Investigator: Detective\nTime Lapsed: 5m 00s\nEvidence Discovered: 0 / 5\nContradictions Exposed: 0 / 3\n\nStatus: UNRESOLVED";

            // Button Retry
            Button retryBtn = EnsureButtonChild(cardObj.transform, "Button_Retry", font, "Retry Case",
                new Vector2(-120f, -160f), new Vector2(200f, 48f), new Color(0.18f, 0.45f, 0.25f));

            // Button Main Menu
            Button menuBtn = EnsureButtonChild(cardObj.transform, "Button_MainMenu", font, "Main Menu",
                new Vector2(120f, -160f), new Vector2(200f, 48f), new Color(0.35f, 0.35f, 0.38f));

            var goSerialized = new SerializedObject(gameOverUI);
            goSerialized.FindProperty("titleText").objectReferenceValue = titleText;
            goSerialized.FindProperty("subtitleText").objectReferenceValue = subtitleText;
            goSerialized.FindProperty("detailsBreakdownText").objectReferenceValue = detailsText;
            goSerialized.FindProperty("retryButton").objectReferenceValue = retryBtn;
            goSerialized.FindProperty("returnToMainMenuButton").objectReferenceValue = menuBtn;
            goSerialized.ApplyModifiedProperties();

            panelObj.SetActive(false);
            EditorUtility.SetDirty(gameOverUI);
            return panelObj;
        }

        private static void EnsureInGameMenuUI(Transform canvasTrans, Transform headerNav, UIManager uiManager, Font font)
        {
            // 1. Link header return button if unassigned
            if (uiManager.returnToMenuButton == null && headerNav != null)
            {
                Transform returnBtnTrans = headerNav.Find("Button_ReturnToMenu");
                if (returnBtnTrans == null)
                {
                    returnBtnTrans = headerNav.Find("Button_Menu");
                }
                if (returnBtnTrans != null)
                {
                    uiManager.returnToMenuButton = returnBtnTrans.gameObject;
                }
            }

            // 2. Ensure In-Game Menu Panel
            Transform existingMenu = canvasTrans.Find("Panel_InGameMenu");
            GameObject inGameMenuObj;

            if (existingMenu != null)
            {
                inGameMenuObj = existingMenu.gameObject;
            }
            else
            {
                inGameMenuObj = new GameObject("Panel_InGameMenu", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                inGameMenuObj.transform.SetParent(canvasTrans, false);

                RectTransform rt = inGameMenuObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                Image bg = inGameMenuObj.GetComponent<Image>();
                bg.color = new Color(0.04f, 0.05f, 0.08f, 0.88f);
                bg.raycastTarget = true;
            }

            Transform menuCardTrans = inGameMenuObj.transform.Find("Card_InGameMenu");
            GameObject menuCardObj;
            if (menuCardTrans != null)
            {
                menuCardObj = menuCardTrans.gameObject;
            }
            else
            {
                menuCardObj = new GameObject("Card_InGameMenu", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                menuCardObj.transform.SetParent(inGameMenuObj.transform, false);

                RectTransform cardRt = menuCardObj.GetComponent<RectTransform>();
                cardRt.anchorMin = new Vector2(0.5f, 0.5f);
                cardRt.anchorMax = new Vector2(0.5f, 0.5f);
                cardRt.pivot = new Vector2(0.5f, 0.5f);
                cardRt.anchoredPosition = Vector2.zero;
                cardRt.sizeDelta = new Vector2(420f, 320f);

                Image cardBg = menuCardObj.GetComponent<Image>();
                cardBg.color = new Color(0.12f, 0.14f, 0.18f, 0.98f);
            }

            // Title
            Text titleText = EnsureTextChild(menuCardObj.transform, "Text_Title", font, 24, FontStyle.Bold,
                new Vector2(0f, 95f), new Vector2(380f, 40f), TextAnchor.MiddleCenter, Color.white);
            titleText.text = "GAME PAUSED";

            // Buttons
            Button resumeBtn = EnsureButtonChild(menuCardObj.transform, "Button_Resume", font, "Resume Game",
                new Vector2(0f, 25f), new Vector2(260f, 48f), new Color(0.18f, 0.45f, 0.25f));

            Button mainMenuBtn = EnsureButtonChild(menuCardObj.transform, "Button_MainMenu", font, "Main Menu",
                new Vector2(0f, -40f), new Vector2(260f, 48f), new Color(0.55f, 0.22f, 0.22f));

            inGameMenuObj.SetActive(false);
            uiManager.inGameMenuPanel = inGameMenuObj;
            uiManager.resumeGameButton = resumeBtn;
            uiManager.inGameMainMenuButton = mainMenuBtn;

            // 3. Ensure Main Menu Confirmation Dialog Panel
            Transform existingConfirm = canvasTrans.Find("Panel_MainMenuConfirm");
            GameObject confirmPanelObj;

            if (existingConfirm != null)
            {
                confirmPanelObj = existingConfirm.gameObject;
            }
            else
            {
                confirmPanelObj = new GameObject("Panel_MainMenuConfirm", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                confirmPanelObj.transform.SetParent(canvasTrans, false);

                RectTransform rt = confirmPanelObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                Image bg = confirmPanelObj.GetComponent<Image>();
                bg.color = new Color(0.02f, 0.02f, 0.02f, 0.90f);
                bg.raycastTarget = true;
            }

            Transform confirmCardTrans = confirmPanelObj.transform.Find("Card_Confirm");
            GameObject confirmCardObj;
            if (confirmCardTrans != null)
            {
                confirmCardObj = confirmCardTrans.gameObject;
            }
            else
            {
                confirmCardObj = new GameObject("Card_Confirm", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                confirmCardObj.transform.SetParent(confirmPanelObj.transform, false);

                RectTransform cardRt = confirmCardObj.GetComponent<RectTransform>();
                cardRt.anchorMin = new Vector2(0.5f, 0.5f);
                cardRt.anchorMax = new Vector2(0.5f, 0.5f);
                cardRt.pivot = new Vector2(0.5f, 0.5f);
                cardRt.anchoredPosition = Vector2.zero;
                cardRt.sizeDelta = new Vector2(500f, 260f);

                Image cardBg = confirmCardObj.GetComponent<Image>();
                cardBg.color = new Color(0.14f, 0.10f, 0.10f, 0.98f);
            }

            // Confirm Title & Message
            Text confirmTitle = EnsureTextChild(confirmCardObj.transform, "Text_Title", font, 22, FontStyle.Bold,
                new Vector2(0f, 70f), new Vector2(460f, 40f), TextAnchor.MiddleCenter, new Color(1f, 0.35f, 0.35f));
            confirmTitle.text = "RETURN TO MAIN MENU?";

            Text confirmMessage = EnsureTextChild(confirmCardObj.transform, "Text_Message", font, 15, FontStyle.Normal,
                new Vector2(0f, 15f), new Vector2(440f, 50f), TextAnchor.MiddleCenter, new Color(0.85f, 0.85f, 0.85f));
            confirmMessage.text = "Any unsaved investigation progress in this case will be lost.\nAre you sure you want to return to the main menu?";

            // Confirm Buttons
            Button yesBtn = EnsureButtonChild(confirmCardObj.transform, "Button_ConfirmYes", font, "Yes, Return",
                new Vector2(-110f, -60f), new Vector2(180f, 46f), new Color(0.65f, 0.20f, 0.20f));

            Button noBtn = EnsureButtonChild(confirmCardObj.transform, "Button_ConfirmNo", font, "Cancel",
                new Vector2(110f, -60f), new Vector2(180f, 46f), new Color(0.30f, 0.32f, 0.38f));

            confirmPanelObj.SetActive(false);
            uiManager.mainMenuConfirmPanel = confirmPanelObj;
            uiManager.confirmMainMenuYesButton = yesBtn;
            uiManager.confirmMainMenuNoButton = noBtn;

            var uiSerialized = new SerializedObject(uiManager);
            uiSerialized.FindProperty("inGameMenuPanel").objectReferenceValue = inGameMenuObj;
            uiSerialized.FindProperty("mainMenuConfirmPanel").objectReferenceValue = confirmPanelObj;
            uiSerialized.FindProperty("resumeGameButton").objectReferenceValue = resumeBtn;
            uiSerialized.FindProperty("inGameMainMenuButton").objectReferenceValue = mainMenuBtn;
            uiSerialized.FindProperty("confirmMainMenuYesButton").objectReferenceValue = yesBtn;
            uiSerialized.FindProperty("confirmMainMenuNoButton").objectReferenceValue = noBtn;
            if (uiManager.returnToMenuButton != null)
            {
                uiSerialized.FindProperty("returnToMenuButton").objectReferenceValue = uiManager.returnToMenuButton;
            }
            uiSerialized.ApplyModifiedProperties();
        }

        private static Text EnsureTextChild(Transform parent, string name, Font font, int size, FontStyle style,
            Vector2 pos, Vector2 sizeDelta, TextAnchor alignment, Color color)
        {
            Transform t = parent.Find(name);
            GameObject go;
            if (t != null)
            {
                go = t.gameObject;
            }
            else
            {
                go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                go.transform.SetParent(parent, false);
            }

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;

            Text text = go.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        private static Button EnsureButtonChild(Transform parent, string name, Font font, string label,
            Vector2 pos, Vector2 sizeDelta, Color btnColor)
        {
            Transform t = parent.Find(name);
            GameObject go;
            if (t != null)
            {
                go = t.gameObject;
            }
            else
            {
                go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                go.transform.SetParent(parent, false);
            }

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;

            Image img = go.GetComponent<Image>();
            img.color = btnColor;
            img.raycastTarget = true;

            Button btn = go.GetComponent<Button>();

            // Child text label
            EnsureTextChild(go.transform, "Text_Label", font, 16, FontStyle.Bold,
                Vector2.zero, sizeDelta, TextAnchor.MiddleCenter, Color.white).text = label;

            return btn;
        }
    }
}
