using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// Master Unity Editor utility for constructing, linking, and configuring all Case Closed scenes:
    /// - MainMenu.unity (Title Screen, Case Selection, Detective Handbook, Settings, Credits)
    /// - Case001.unity (Interrogation Room Sketch Layout: Background, 2 Suspects, Table Desk, Interactive Table Items, UI)
    /// - Case002.unity (Art Gallery Case Layout)
    /// - Case003.unity (Coffee Shop Office Case Layout)
    /// Also registers all scenes into EditorBuildSettings.
    /// </summary>
    public static class CaseSceneBuilder
    {
        private static readonly Color ColorDarkBg = new Color(0.08f, 0.09f, 0.11f, 0.95f);
        private static readonly Color ColorSlatePanel = new Color(0.12f, 0.14f, 0.18f, 0.92f);
        private static readonly Color ColorHeaderGold = new Color(0.92f, 0.78f, 0.43f, 1f);
        private static readonly Color ColorButtonNormal = new Color(0.18f, 0.22f, 0.28f, 1f);
        private static readonly Color ColorButtonHighlight = new Color(0.28f, 0.34f, 0.42f, 1f);
        private static readonly Color ColorButtonPressed = new Color(0.14f, 0.16f, 0.20f, 1f);
        private static readonly Color ColorTextWhite = new Color(0.92f, 0.92f, 0.94f, 1f);
        private static readonly Color ColorTextMuted = new Color(0.65f, 0.68f, 0.74f, 1f);

        private const string SceneDir = "Assets/Scenes";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string Case001ScenePath = "Assets/Scenes/Case001.unity";
        private const string Case002ScenePath = "Assets/Scenes/Case002.unity";
        private const string Case003ScenePath = "Assets/Scenes/Case003.unity";

        [MenuItem("Case Closed/Build All Scenes (MainMenu + Case001 + Case002 + Case003)", false, 0)]
        public static void BuildAllScenes()
        {
            Debug.Log("[CaseSceneBuilder] === STARTING AUTOMATED BUILD OF ALL 4 SCENES ===");
            EnsureDirectory(SceneDir);

            BuildMainMenuScene();
            BuildCase001Scene();
            BuildCase002Scene();
            BuildCase003Scene();

            RegisterScenesInBuildSettings();

            Debug.Log("[CaseSceneBuilder] === ALL 4 SCENES BUILT & REGISTERED SUCCESSFULLY! ===");
        }

        [MenuItem("Case Closed/Build MainMenu Scene", false, 10)]
        public static void BuildMainMenuScene()
        {
            Debug.Log("[CaseSceneBuilder] Building MainMenu Scene...");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera
            Camera mainCam = CreateCamera();

            // 2. Managers
            GameObject managersObj = new GameObject("_Managers");
            EnsureComponent<AudioManager>(managersObj);
            EnsureComponent<CaseManager>(managersObj);
            GameBootstrap bootstrap = EnsureComponent<GameBootstrap>(managersObj);
            bootstrap.startOnMainMenu = true;

            // 3. Canvas & UI
            GameObject canvasObj = CreateCanvasRoot();
            UIManager uiManager = EnsureComponent<UIManager>(canvasObj);
            EnsureEventSystem();

            // Build Main Menu UI Panels
            BuildMainMenuPanelHierarchy(canvasObj, uiManager);

            // Save Scene
            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
            Debug.Log($"[CaseSceneBuilder] MainMenu Scene saved to: {MainMenuScenePath}");
        }

        [MenuItem("Case Closed/Build Case 001 Scene (Sketch Layout)", false, 11)]
        public static void BuildCase001Scene()
        {
            Debug.Log("[CaseSceneBuilder] Building Case001 Scene matching sketch...");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera
            Camera mainCam = CreateCamera();

            // 2. Load Assets
            Sprite bgSprite = LoadSprite("Assets/Assets/Interrogation Room.jpg");
            Sprite tableSprite = LoadSprite("Assets/Assets/Interrogation Table.png");
            Sprite maleSprite = LoadSprite("Assets/Assets/Case001_Male.png");
            Sprite femaleSprite = LoadSprite("Assets/Assets/Case001_Female.png");

            Sprite knifeSprite = LoadSprite("Assets/Art/Evidence/Sprite_Knife.png");
            Sprite envelopeSprite = LoadSprite("Assets/Art/Evidence/Sprite_EvidenceEnvelope.png");
            Sprite notebookSprite = LoadSprite("Assets/Art/Props/Sprite_OpenNotebook.png");
            Sprite cupSprite = LoadSprite("Assets/Art/Props/Sprite_CoffeeCup.png");
            Sprite handSprite = LoadSprite("Assets/Art/Props/Sprite_DetectiveHand.png");
            Sprite alertSprite = LoadSprite("Assets/Art/UI/Sprite_AlertIcon.png");

            // 3. Background Layer (Layer 1)
            GameObject bgObj = new GameObject("Environment_Background");
            SpriteRenderer bgRenderer = bgObj.AddComponent<SpriteRenderer>();
            bgRenderer.sprite = bgSprite;
            bgRenderer.sortingOrder = 0;
            bgObj.transform.position = new Vector3(0f, 0f, 0f);
            if (bgSprite != null)
            {
                float bgScale = 10f / (bgSprite.rect.height / bgSprite.pixelsPerUnit);
                bgObj.transform.localScale = new Vector3(bgScale, bgScale, 1f);
            }

            // Alert Reaction Icon on mirror/characters
            if (alertSprite != null)
            {
                GameObject alertObj = new GameObject("Reaction_Alert_Icon");
                SpriteRenderer alertRenderer = alertObj.AddComponent<SpriteRenderer>();
                alertRenderer.sprite = alertSprite;
                alertRenderer.sortingOrder = 15;
                alertObj.transform.position = new Vector3(2.1f, 1.8f, 0f);
                alertObj.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            }

            // 4. Characters Layer (Layer 2: Behind the table)
            // Left: Male Suspect (Vince Batecan / Primary Suspect with headphones)
            GameObject charLeft = new GameObject("Character_Suspect_Left");
            charLeft.transform.position = new Vector3(-2.2f, 0.4f, 0f);
            charLeft.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            SpriteRenderer maleRenderer = charLeft.AddComponent<SpriteRenderer>();
            maleRenderer.sprite = maleSprite;
            maleRenderer.sortingOrder = 5;

            CharacterDisplay cdLeft = charLeft.AddComponent<CharacterDisplay>();
            cdLeft.characterSlot = CharacterSlot.PrimarySuspect;
            cdLeft.characterSpriteRenderer = maleRenderer;
            cdLeft.enableIdleBreathing = true;
            cdLeft.breathingSpeed = 2f;
            cdLeft.breathingAmount = 0.025f;

            // Right: Female Suspect / Accomplice (Janine Sotto / Secondary Suspect)
            GameObject charRight = new GameObject("Character_Suspect_Right");
            charRight.transform.position = new Vector3(2.0f, 0.3f, 0f);
            charRight.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            SpriteRenderer femaleRenderer = charRight.AddComponent<SpriteRenderer>();
            femaleRenderer.sprite = femaleSprite;
            femaleRenderer.sortingOrder = 5;

            CharacterDisplay cdRight = charRight.AddComponent<CharacterDisplay>();
            cdRight.characterSlot = CharacterSlot.SecondarySuspect;
            cdRight.characterSpriteRenderer = femaleRenderer;
            cdRight.enableIdleBreathing = true;
            cdRight.breathingSpeed = 2.2f;
            cdRight.breathingAmount = 0.025f;

            // 5. Table Desk Layer (Layer 3: Foreground)
            GameObject tableObj = new GameObject("Table_Desk");
            tableObj.transform.position = new Vector3(0f, -2.4f, 0f);
            tableObj.transform.localScale = new Vector3(1.9f, 1.8f, 1f);
            SpriteRenderer tableRenderer = tableObj.AddComponent<SpriteRenderer>();
            tableRenderer.sprite = tableSprite;
            tableRenderer.sortingOrder = 10;

            // 6. Interactive Table Evidence Items (Matching Sketch!)
            // Item 1: Envelope / Photo Evidence (Left)
            GameObject itemEnvelope = new GameObject("Item_Photo_Evidence");
            itemEnvelope.transform.position = new Vector3(-3.6f, -1.8f, 0f);
            itemEnvelope.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            SpriteRenderer envRen = itemEnvelope.AddComponent<SpriteRenderer>();
            envRen.sprite = envelopeSprite;
            envRen.sortingOrder = 12;
            BoxCollider2D envCol = itemEnvelope.AddComponent<BoxCollider2D>();
            envCol.size = new Vector2(4.5f, 4.5f);
            TableEvidenceItem teiEnv = itemEnvelope.AddComponent<TableEvidenceItem>();
            teiEnv.spriteRenderer = envRen;
            teiEnv.openNotebookOnClick = false;

            // Item 2: Open Case File Notebook (Center Desk)
            GameObject itemBook = new GameObject("Item_CaseBook");
            itemBook.transform.position = new Vector3(-0.4f, -2.1f, 0f);
            itemBook.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            SpriteRenderer bookRen = itemBook.AddComponent<SpriteRenderer>();
            bookRen.sprite = notebookSprite;
            bookRen.sortingOrder = 12;
            BoxCollider2D bookCol = itemBook.AddComponent<BoxCollider2D>();
            bookCol.size = new Vector2(4.8f, 3.5f);
            TableEvidenceItem teiBook = itemBook.AddComponent<TableEvidenceItem>();
            teiBook.spriteRenderer = bookRen;
            teiBook.openNotebookOnClick = true;

            // Item 3: Knife Evidence Clue (Center-Top Desk)
            GameObject itemKnife = new GameObject("Item_Weapon_Clue");
            itemKnife.transform.position = new Vector3(0.0f, -0.9f, 0f);
            itemKnife.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
            SpriteRenderer knifeRen = itemKnife.AddComponent<SpriteRenderer>();
            knifeRen.sprite = knifeSprite;
            knifeRen.sortingOrder = 12;
            BoxCollider2D knifeCol = itemKnife.AddComponent<BoxCollider2D>();
            knifeCol.size = new Vector2(5.0f, 2.0f);
            TableEvidenceItem teiKnife = itemKnife.AddComponent<TableEvidenceItem>();
            teiKnife.spriteRenderer = knifeRen;
            teiKnife.openNotebookOnClick = false;

            // Item 4: Coffee Cup with Straw (Right Desk)
            GameObject itemCup = new GameObject("Item_Cup_Clue");
            itemCup.transform.position = new Vector3(3.6f, -1.8f, 0f);
            itemCup.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
            SpriteRenderer cupRen = itemCup.AddComponent<SpriteRenderer>();
            cupRen.sprite = cupSprite;
            cupRen.sortingOrder = 12;
            BoxCollider2D cupCol = itemCup.AddComponent<BoxCollider2D>();
            cupCol.size = new Vector2(2.5f, 4.8f);
            TableEvidenceItem teiCup = itemCup.AddComponent<TableEvidenceItem>();
            teiCup.spriteRenderer = cupRen;
            teiCup.openNotebookOnClick = false;

            // Item 5: Detective Hand Pointer (Foreground)
            if (handSprite != null)
            {
                GameObject itemHand = new GameObject("Detective_Hand_Pointer");
                itemHand.transform.position = new Vector3(2.0f, -2.4f, 0f);
                itemHand.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
                SpriteRenderer handRen = itemHand.AddComponent<SpriteRenderer>();
                handRen.sprite = handSprite;
                handRen.sortingOrder = 14;
            }

            // 7. Managers Container
            GameObject managersObj = new GameObject("_Managers");
            EnsureComponent<AudioManager>(managersObj);
            EnsureComponent<CaseManager>(managersObj);
            EnsureComponent<EvidenceManager>(managersObj);
            EnsureComponent<InterrogationManager>(managersObj);
            EnsureComponent<DeductionBoardController>(managersObj);
            EnsureComponent<CaseConclusionManager>(managersObj);

            Case01Initializer init1 = EnsureComponent<Case01Initializer>(managersObj);
            init1.initializeOnStart = true;

            GameBootstrap bootstrap = EnsureComponent<GameBootstrap>(managersObj);
            bootstrap.startOnMainMenu = false;

            // 8. Canvas & In-Game UI
            GameObject canvasObj = CreateCanvasRoot();
            UIManager uiManager = EnsureComponent<UIManager>(canvasObj);
            EnsureEventSystem();

            BuildInGameUIPanels(canvasObj, uiManager, 1);

            // Save Scene
            EditorSceneManager.SaveScene(scene, Case001ScenePath);
            Debug.Log($"[CaseSceneBuilder] Case001 Scene saved to: {Case001ScenePath}");
        }

        [MenuItem("Case Closed/Build Case 002 Scene", false, 12)]
        public static void BuildCase002Scene()
        {
            Debug.Log("[CaseSceneBuilder] Building Case002 Scene...");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();

            Sprite bgSprite = LoadSprite("Assets/Assets/Interrogation Room.jpg");
            Sprite tableSprite = LoadSprite("Assets/Assets/Interrogation Table.png");
            Sprite maleSprite = LoadSprite("Assets/Assets/Case001_Male.png");
            Sprite femaleSprite = LoadSprite("Assets/Assets/Case001_Female.png");
            Sprite envelopeSprite = LoadSprite("Assets/Art/Evidence/Sprite_EvidenceEnvelope.png");
            Sprite notebookSprite = LoadSprite("Assets/Art/Props/Sprite_OpenNotebook.png");
            Sprite cupSprite = LoadSprite("Assets/Art/Props/Sprite_CoffeeCup.png");

            // Background
            GameObject bgObj = new GameObject("Environment_Background");
            SpriteRenderer bgRenderer = bgObj.AddComponent<SpriteRenderer>();
            bgRenderer.sprite = bgSprite;
            bgRenderer.sortingOrder = 0;
            if (bgSprite != null)
            {
                float bgScale = 10f / (bgSprite.rect.height / bgSprite.pixelsPerUnit);
                bgObj.transform.localScale = new Vector3(bgScale, bgScale, 1f);
            }

            // Characters (Charl Pascual on Left, Paul Camacho on Right)
            GameObject charLeft = new GameObject("Character_Suspect_Left");
            charLeft.transform.position = new Vector3(-2.2f, 0.4f, 0f);
            charLeft.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            SpriteRenderer rLeft = charLeft.AddComponent<SpriteRenderer>();
            rLeft.sprite = maleSprite;
            rLeft.sortingOrder = 5;
            CharacterDisplay cdLeft = charLeft.AddComponent<CharacterDisplay>();
            cdLeft.characterSlot = CharacterSlot.PrimarySuspect;
            cdLeft.characterSpriteRenderer = rLeft;
            cdLeft.enableIdleBreathing = true;

            GameObject charRight = new GameObject("Character_Suspect_Right");
            charRight.transform.position = new Vector3(2.0f, 0.3f, 0f);
            charRight.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            SpriteRenderer rRight = charRight.AddComponent<SpriteRenderer>();
            rRight.sprite = femaleSprite;
            rRight.sortingOrder = 5;
            CharacterDisplay cdRight = charRight.AddComponent<CharacterDisplay>();
            cdRight.characterSlot = CharacterSlot.SecondarySuspect;
            cdRight.characterSpriteRenderer = rRight;
            cdRight.enableIdleBreathing = true;

            // Table Desk
            GameObject tableObj = new GameObject("Table_Desk");
            tableObj.transform.position = new Vector3(0f, -2.4f, 0f);
            tableObj.transform.localScale = new Vector3(1.9f, 1.8f, 1f);
            SpriteRenderer tableRenderer = tableObj.AddComponent<SpriteRenderer>();
            tableRenderer.sprite = tableSprite;
            tableRenderer.sortingOrder = 10;

            // Table Items
            GameObject itemEnvelope = new GameObject("Item_Photo_Evidence");
            itemEnvelope.transform.position = new Vector3(-3.6f, -1.8f, 0f);
            itemEnvelope.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            SpriteRenderer envRen = itemEnvelope.AddComponent<SpriteRenderer>();
            envRen.sprite = envelopeSprite;
            envRen.sortingOrder = 12;
            BoxCollider2D envCol = itemEnvelope.AddComponent<BoxCollider2D>();
            envCol.size = new Vector2(4.5f, 4.5f);
            TableEvidenceItem teiEnv = itemEnvelope.AddComponent<TableEvidenceItem>();
            teiEnv.spriteRenderer = envRen;

            GameObject itemBook = new GameObject("Item_CaseBook");
            itemBook.transform.position = new Vector3(-0.4f, -2.1f, 0f);
            itemBook.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            SpriteRenderer bookRen = itemBook.AddComponent<SpriteRenderer>();
            bookRen.sprite = notebookSprite;
            bookRen.sortingOrder = 12;
            BoxCollider2D bookCol = itemBook.AddComponent<BoxCollider2D>();
            bookCol.size = new Vector2(4.8f, 3.5f);
            TableEvidenceItem teiBook = itemBook.AddComponent<TableEvidenceItem>();
            teiBook.spriteRenderer = bookRen;
            teiBook.openNotebookOnClick = true;

            GameObject itemCup = new GameObject("Item_Cup_Clue");
            itemCup.transform.position = new Vector3(3.6f, -1.8f, 0f);
            itemCup.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
            SpriteRenderer cupRen = itemCup.AddComponent<SpriteRenderer>();
            cupRen.sprite = cupSprite;
            cupRen.sortingOrder = 12;
            BoxCollider2D cupCol = itemCup.AddComponent<BoxCollider2D>();
            cupCol.size = new Vector2(2.5f, 4.8f);
            TableEvidenceItem teiCup = itemCup.AddComponent<TableEvidenceItem>();
            teiCup.spriteRenderer = cupRen;

            // Managers
            GameObject managersObj = new GameObject("_Managers");
            EnsureComponent<AudioManager>(managersObj);
            EnsureComponent<CaseManager>(managersObj);
            EnsureComponent<EvidenceManager>(managersObj);
            EnsureComponent<InterrogationManager>(managersObj);
            EnsureComponent<DeductionBoardController>(managersObj);
            EnsureComponent<CaseConclusionManager>(managersObj);

            Case02Initializer init2 = EnsureComponent<Case02Initializer>(managersObj);
            init2.initializeOnStart = true;

            GameBootstrap bootstrap = EnsureComponent<GameBootstrap>(managersObj);
            bootstrap.startOnMainMenu = false;

            // Canvas
            GameObject canvasObj = CreateCanvasRoot();
            UIManager uiManager = EnsureComponent<UIManager>(canvasObj);
            EnsureEventSystem();
            BuildInGameUIPanels(canvasObj, uiManager, 2);

            EditorSceneManager.SaveScene(scene, Case002ScenePath);
            Debug.Log($"[CaseSceneBuilder] Case002 Scene saved to: {Case002ScenePath}");
        }

        [MenuItem("Case Closed/Build Case 003 Scene", false, 13)]
        public static void BuildCase003Scene()
        {
            Debug.Log("[CaseSceneBuilder] Building Case003 Scene...");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();

            Sprite bgSprite = LoadSprite("Assets/Assets/Interrogation Room.jpg");
            Sprite tableSprite = LoadSprite("Assets/Assets/Interrogation Table.png");
            Sprite maleSprite = LoadSprite("Assets/Assets/Case001_Male.png");
            Sprite femaleSprite = LoadSprite("Assets/Assets/Case001_Female.png");
            Sprite envelopeSprite = LoadSprite("Assets/Art/Evidence/Sprite_EvidenceEnvelope.png");
            Sprite notebookSprite = LoadSprite("Assets/Art/Props/Sprite_OpenNotebook.png");
            Sprite cupSprite = LoadSprite("Assets/Art/Props/Sprite_CoffeeCup.png");

            // Background
            GameObject bgObj = new GameObject("Environment_Background");
            SpriteRenderer bgRenderer = bgObj.AddComponent<SpriteRenderer>();
            bgRenderer.sprite = bgSprite;
            bgRenderer.sortingOrder = 0;
            if (bgSprite != null)
            {
                float bgScale = 10f / (bgSprite.rect.height / bgSprite.pixelsPerUnit);
                bgObj.transform.localScale = new Vector3(bgScale, bgScale, 1f);
            }

            // Characters (Shanaia Ortega on Left, Shan Jaraba on Right)
            GameObject charLeft = new GameObject("Character_Suspect_Left");
            charLeft.transform.position = new Vector3(-2.2f, 0.4f, 0f);
            charLeft.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            SpriteRenderer rLeft = charLeft.AddComponent<SpriteRenderer>();
            rLeft.sprite = femaleSprite;
            rLeft.sortingOrder = 5;
            CharacterDisplay cdLeft = charLeft.AddComponent<CharacterDisplay>();
            cdLeft.characterSlot = CharacterSlot.PrimarySuspect;
            cdLeft.characterSpriteRenderer = rLeft;
            cdLeft.enableIdleBreathing = true;

            GameObject charRight = new GameObject("Character_Suspect_Right");
            charRight.transform.position = new Vector3(2.0f, 0.3f, 0f);
            charRight.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            SpriteRenderer rRight = charRight.AddComponent<SpriteRenderer>();
            rRight.sprite = maleSprite;
            rRight.sortingOrder = 5;
            CharacterDisplay cdRight = charRight.AddComponent<CharacterDisplay>();
            cdRight.characterSlot = CharacterSlot.SecondarySuspect;
            cdRight.characterSpriteRenderer = rRight;
            cdRight.enableIdleBreathing = true;

            // Table Desk
            GameObject tableObj = new GameObject("Table_Desk");
            tableObj.transform.position = new Vector3(0f, -2.4f, 0f);
            tableObj.transform.localScale = new Vector3(1.9f, 1.8f, 1f);
            SpriteRenderer tableRenderer = tableObj.AddComponent<SpriteRenderer>();
            tableRenderer.sprite = tableSprite;
            tableRenderer.sortingOrder = 10;

            // Table Items
            GameObject itemEnvelope = new GameObject("Item_Photo_Evidence");
            itemEnvelope.transform.position = new Vector3(-3.6f, -1.8f, 0f);
            itemEnvelope.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            SpriteRenderer envRen = itemEnvelope.AddComponent<SpriteRenderer>();
            envRen.sprite = envelopeSprite;
            envRen.sortingOrder = 12;
            BoxCollider2D envCol = itemEnvelope.AddComponent<BoxCollider2D>();
            envCol.size = new Vector2(4.5f, 4.5f);
            TableEvidenceItem teiEnv = itemEnvelope.AddComponent<TableEvidenceItem>();
            teiEnv.spriteRenderer = envRen;

            GameObject itemBook = new GameObject("Item_CaseBook");
            itemBook.transform.position = new Vector3(-0.4f, -2.1f, 0f);
            itemBook.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            SpriteRenderer bookRen = itemBook.AddComponent<SpriteRenderer>();
            bookRen.sprite = notebookSprite;
            bookRen.sortingOrder = 12;
            BoxCollider2D bookCol = itemBook.AddComponent<BoxCollider2D>();
            bookCol.size = new Vector2(4.8f, 3.5f);
            TableEvidenceItem teiBook = itemBook.AddComponent<TableEvidenceItem>();
            teiBook.spriteRenderer = bookRen;
            teiBook.openNotebookOnClick = true;

            GameObject itemCup = new GameObject("Item_Cup_Clue");
            itemCup.transform.position = new Vector3(3.6f, -1.8f, 0f);
            itemCup.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
            SpriteRenderer cupRen = itemCup.AddComponent<SpriteRenderer>();
            cupRen.sprite = cupSprite;
            cupRen.sortingOrder = 12;
            BoxCollider2D cupCol = itemCup.AddComponent<BoxCollider2D>();
            cupCol.size = new Vector2(2.5f, 4.8f);
            TableEvidenceItem teiCup = itemCup.AddComponent<TableEvidenceItem>();
            teiCup.spriteRenderer = cupRen;

            // Managers
            GameObject managersObj = new GameObject("_Managers");
            EnsureComponent<AudioManager>(managersObj);
            EnsureComponent<CaseManager>(managersObj);
            EnsureComponent<EvidenceManager>(managersObj);
            EnsureComponent<InterrogationManager>(managersObj);
            EnsureComponent<DeductionBoardController>(managersObj);
            EnsureComponent<CaseConclusionManager>(managersObj);

            Case03Initializer init3 = EnsureComponent<Case03Initializer>(managersObj);
            init3.initializeOnStart = true;

            GameBootstrap bootstrap = EnsureComponent<GameBootstrap>(managersObj);
            bootstrap.startOnMainMenu = false;

            // Canvas
            GameObject canvasObj = CreateCanvasRoot();
            UIManager uiManager = EnsureComponent<UIManager>(canvasObj);
            EnsureEventSystem();
            BuildInGameUIPanels(canvasObj, uiManager, 3);

            EditorSceneManager.SaveScene(scene, Case003ScenePath);
            Debug.Log($"[CaseSceneBuilder] Case003 Scene saved to: {Case003ScenePath}");
        }

        #region Helper Scene Construction Methods

        public static void RegisterScenesInBuildSettings()
        {
            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(Case001ScenePath, true),
                new EditorBuildSettingsScene(Case002ScenePath, true),
                new EditorBuildSettingsScene(Case003ScenePath, true)
            };

            EditorBuildSettings.scenes = buildScenes.ToArray();
            Debug.Log($"[CaseSceneBuilder] Registered {buildScenes.Count} scenes into EditorBuildSettings:");
            foreach (var s in buildScenes)
            {
                Debug.Log($"  • {s.path}");
            }
        }

        private static Camera CreateCamera()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                camObj.tag = "MainCamera";
                mainCam = camObj.GetComponent<Camera>();
            }
            mainCam.orthographic = true;
            mainCam.orthographicSize = 5f;
            mainCam.backgroundColor = new Color(0.05f, 0.05f, 0.07f, 1f);
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.transform.position = new Vector3(0f, 0f, -10f);

            if (mainCam.GetComponent<FixedInvestigationCamera>() == null)
            {
                mainCam.gameObject.AddComponent<FixedInvestigationCamera>();
            }
            return mainCam;
        }

        private static GameObject CreateCanvasRoot()
        {
            GameObject canvasObj = GameObject.Find("Canvas_MainUI");
            if (canvasObj == null)
            {
                canvasObj = new GameObject("Canvas_MainUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            }
            Canvas canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            return canvasObj;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
        }

        private static Sprite LoadSprite(string path)
        {
            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sp != null) return sp;

            Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var obj in all)
            {
                if (obj is Sprite s) return s;
            }
            return null;
        }

        private static void EnsureDirectory(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
                AssetDatabase.Refresh();
            }
        }

        #endregion

        #region UI Panel Builders

        private static void BuildMainMenuPanelHierarchy(GameObject canvasObj, UIManager uiManager)
        {
            GameObject mainMenuPanel = EnsureChild(canvasObj, "Panel_MainMenu");
            SetupRectTransform(mainMenuPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image menuBg = EnsureComponent<Image>(mainMenuPanel);
            menuBg.color = new Color(0.06f, 0.07f, 0.09f, 1f);
            MainMenuUI mainMenuUI = EnsureComponent<MainMenuUI>(mainMenuPanel);

            // Title & Subtitle Banner
            GameObject titleObj = CreateText(mainMenuPanel, "Text_GameTitle", "CASE CLOSED", 56, FontStyle.Bold, ColorHeaderGold, TextAnchor.MiddleCenter);
            SetupRectTransform(titleObj, new Vector2(0.1f, 0.78f), new Vector2(0.9f, 0.94f), Vector2.zero, Vector2.zero);

            GameObject subtitleObj = CreateText(mainMenuPanel, "Text_GameSubtitle", "— A 2D DETECTIVE MYSTERY —", 20, FontStyle.Normal, ColorTextMuted, TextAnchor.MiddleCenter);
            SetupRectTransform(subtitleObj, new Vector2(0.1f, 0.72f), new Vector2(0.9f, 0.78f), Vector2.zero, Vector2.zero);

            // Main Buttons Container
            GameObject mainBtnsCont = EnsureChild(mainMenuPanel, "Container_MainButtons");
            SetupRectTransform(mainBtnsCont, new Vector2(0.35f, 0.10f), new Vector2(0.65f, 0.68f), Vector2.zero, Vector2.zero);
            mainMenuUI.mainButtonsContainer = mainBtnsCont;

            GameObject btnPlay = CreateButton(mainBtnsCont, "Button_Play", "▶ NEW INVESTIGATION", new Vector2(0f, 260f), new Vector2(360f, 52f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            btnPlay.GetComponent<Image>().color = new Color(0.75f, 0.58f, 0.22f, 1f);
            btnPlay.GetComponentInChildren<Text>().color = Color.black;
            btnPlay.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;
            mainMenuUI.playButton = btnPlay.GetComponent<Button>();

            GameObject btnCaseSelect = CreateButton(mainBtnsCont, "Button_CaseSelect", "📂 CASE FILES (SELECT LEVEL)", new Vector2(0f, 200f), new Vector2(360f, 48f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            mainMenuUI.caseSelectButton = btnCaseSelect.GetComponent<Button>();

            GameObject btnHowToPlay = CreateButton(mainBtnsCont, "Button_HowToPlay", "📖 DETECTIVE HANDBOOK", new Vector2(0f, 140f), new Vector2(360f, 48f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            mainMenuUI.howToPlayButton = btnHowToPlay.GetComponent<Button>();

            GameObject btnSettings = CreateButton(mainBtnsCont, "Button_Settings", "⚙ AUDIO & SETTINGS", new Vector2(0f, 80f), new Vector2(360f, 48f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            mainMenuUI.settingsButton = btnSettings.GetComponent<Button>();

            GameObject btnCredits = CreateButton(mainBtnsCont, "Button_Credits", "★ CREDITS", new Vector2(0f, 20f), new Vector2(360f, 48f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            mainMenuUI.creditsButton = btnCredits.GetComponent<Button>();

            GameObject btnQuit = CreateButton(mainBtnsCont, "Button_Quit", "✕ QUIT GAME", new Vector2(0f, -40f), new Vector2(360f, 48f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            btnQuit.GetComponent<Image>().color = new Color(0.35f, 0.12f, 0.12f, 1f);
            mainMenuUI.quitButton = btnQuit.GetComponent<Button>();

            // Case Select Container
            GameObject caseSelCont = EnsureChild(mainMenuPanel, "Container_CaseSelect");
            SetupRectTransform(caseSelCont, new Vector2(0.2f, 0.08f), new Vector2(0.8f, 0.70f), Vector2.zero, Vector2.zero);
            Image caseSelBg = EnsureComponent<Image>(caseSelCont);
            caseSelBg.color = ColorSlatePanel;
            mainMenuUI.caseSelectContainer = caseSelCont;

            GameObject caseSelHeader = CreateText(caseSelCont, "Text_Header", "SELECT INVESTIGATION DOSSIER", 24, FontStyle.Bold, ColorHeaderGold, TextAnchor.MiddleCenter);
            SetupRectTransform(caseSelHeader, new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.98f), Vector2.zero, Vector2.zero);

            GameObject btnCase1 = CreateButton(caseSelCont, "Button_Case01", "CASE 01: THE MISSING NECKLACE\n<size=13><color=#A0AAB8>Location: Manor Study | Suspect: Vince Angelo Batecan</color></size>", new Vector2(0f, 250f), new Vector2(650f, 70f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            mainMenuUI.case01Button = btnCase1.GetComponent<Button>();

            GameObject btnCase2 = CreateButton(caseSelCont, "Button_Case02", "CASE 02: THE SHATTERED MIRROR\n<size=13><color=#A0AAB8>Location: Art Gallery | Suspect: Charl Vonn Pascual</color></size>", new Vector2(0f, 160f), new Vector2(650f, 70f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            mainMenuUI.case02Button = btnCase2.GetComponent<Button>();

            GameObject btnCase3 = CreateButton(caseSelCont, "Button_Case03", "CASE 03: THE LAST CALL\n<size=13><color=#A0AAB8>Location: Coffee Shop Office | Suspect: Shanaia Ortega</color></size>", new Vector2(0f, 70f), new Vector2(650f, 70f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            mainMenuUI.case03Button = btnCase3.GetComponent<Button>();

            GameObject btnBackCaseSel = CreateButton(caseSelCont, "Button_Back", "◀ BACK TO MAIN MENU", new Vector2(0f, 10f), new Vector2(240f, 40f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            mainMenuUI.backFromCaseSelectButton = btnBackCaseSel.GetComponent<Button>();
            caseSelCont.SetActive(false);

            // Handbook Container
            GameObject htpCont = EnsureChild(mainMenuPanel, "Container_HowToPlay");
            SetupRectTransform(htpCont, new Vector2(0.15f, 0.08f), new Vector2(0.85f, 0.70f), Vector2.zero, Vector2.zero);
            Image htpBg = EnsureComponent<Image>(htpCont);
            htpBg.color = ColorSlatePanel;
            mainMenuUI.howToPlayContainer = htpCont;

            GameObject htpHeader = CreateText(htpCont, "Text_Header", "DETECTIVE HANDBOOK - INVESTIGATION RULES", 24, FontStyle.Bold, ColorHeaderGold, TextAnchor.MiddleCenter);
            SetupRectTransform(htpHeader, new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.98f), Vector2.zero, Vector2.zero);

            string htpRules =
                "<b>1. EXAMINE TABLE EVIDENCE</b>\n" +
                "Click on physical items lying on the investigation desk to inspect details, rotate 360°, and uncover hidden hotspots.\n\n" +
                "<b>2. READ THE CASE DOSSIER</b>\n" +
                "Open the detective notebook to review victim information, suspect backgrounds, known alibis, and discovered clue logs.\n\n" +
                "<b>3. INTERROGATE & EXPOSE CONTRADICTIONS</b>\n" +
                "Question suspects. When a statement contradicts physical evidence, press <b>[CHALLENGE]</b> and present the matching proof to trigger a breakthrough confession!\n\n" +
                "<b>4. SYNTHESIZE DEDUCTIONS</b>\n" +
                "Connect pairs of clues on the Deduction Board to formulate decisive deductions.\n\n" +
                "<b>5. CONCLUDE THE CASE</b>\n" +
                "Answer the multiple-choice conclusion inquiry to grade your detective performance (Rank S to D, 1 to 5 Stars)!";

            GameObject htpText = CreateText(htpCont, "Text_Rules", htpRules, 15, FontStyle.Normal, ColorTextWhite, TextAnchor.UpperLeft);
            SetupRectTransform(htpText, new Vector2(0.06f, 0.15f), new Vector2(0.94f, 0.86f), Vector2.zero, Vector2.zero);

            GameObject btnBackHtp = CreateButton(htpCont, "Button_Back", "◀ BACK TO MAIN MENU", new Vector2(0f, 15f), new Vector2(240f, 40f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            mainMenuUI.backFromHowToPlayButton = btnBackHtp.GetComponent<Button>();
            htpCont.SetActive(false);

            // Settings Container
            GameObject settingsCont = EnsureChild(mainMenuPanel, "Container_Settings");
            SetupRectTransform(settingsCont, new Vector2(0.25f, 0.12f), new Vector2(0.75f, 0.68f), Vector2.zero, Vector2.zero);
            Image setBg = EnsureComponent<Image>(settingsCont);
            setBg.color = ColorSlatePanel;
            mainMenuUI.settingsContainer = settingsCont;

            GameObject setHeader = CreateText(settingsCont, "Text_Header", "AUDIO SETTINGS", 24, FontStyle.Bold, ColorHeaderGold, TextAnchor.MiddleCenter);
            SetupRectTransform(setHeader, new Vector2(0.05f, 0.85f), new Vector2(0.95f, 0.98f), Vector2.zero, Vector2.zero);

            GameObject bgmLabel = CreateText(settingsCont, "Text_BGMLabel", "BGM Music Volume", 16, FontStyle.Bold, ColorTextWhite, TextAnchor.MiddleLeft);
            SetupRectTransform(bgmLabel, new Vector2(0.1f, 0.65f), new Vector2(0.5f, 0.75f), Vector2.zero, Vector2.zero);
            GameObject bgmSliderObj = CreateSlider(settingsCont, "Slider_BGM", new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.63f));
            mainMenuUI.bgmVolumeSlider = bgmSliderObj.GetComponent<Slider>();

            GameObject sfxLabel = CreateText(settingsCont, "Text_SFXLabel", "SFX Sound Effects Volume", 16, FontStyle.Bold, ColorTextWhite, TextAnchor.MiddleLeft);
            SetupRectTransform(sfxLabel, new Vector2(0.1f, 0.40f), new Vector2(0.5f, 0.50f), Vector2.zero, Vector2.zero);
            GameObject sfxSliderObj = CreateSlider(settingsCont, "Slider_SFX", new Vector2(0.1f, 0.30f), new Vector2(0.9f, 0.38f));
            mainMenuUI.sfxVolumeSlider = sfxSliderObj.GetComponent<Slider>();

            GameObject typeToggleObj = CreateToggle(settingsCont, "Toggle_Typewriter", "Enable Typewriter Click Sound Effects", new Vector2(0.1f, 0.18f), new Vector2(0.9f, 0.26f));
            mainMenuUI.typewriterToggle = typeToggleObj.GetComponent<Toggle>();

            GameObject btnBackSet = CreateButton(settingsCont, "Button_Back", "◀ BACK TO MAIN MENU", new Vector2(0f, 15f), new Vector2(240f, 40f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            mainMenuUI.backFromSettingsButton = btnBackSet.GetComponent<Button>();
            settingsCont.SetActive(false);

            // Credits Container
            GameObject credCont = EnsureChild(mainMenuPanel, "Container_Credits");
            SetupRectTransform(credCont, new Vector2(0.2f, 0.10f), new Vector2(0.8f, 0.70f), Vector2.zero, Vector2.zero);
            Image credBg = EnsureComponent<Image>(credCont);
            credBg.color = ColorSlatePanel;
            mainMenuUI.creditsContainer = credCont;

            GameObject credHeader = CreateText(credCont, "Text_Header", "CASE CLOSED - PRODUCTION CREDITS", 24, FontStyle.Bold, ColorHeaderGold, TextAnchor.MiddleCenter);
            SetupRectTransform(credHeader, new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.98f), Vector2.zero, Vector2.zero);

            string credTextStr =
                "<b>PROJECT:</b> CIT2101_2D / Case Closed\n\n" +
                "<b>CAST OF CHARACTERS:</b>\n" +
                "• <b>Vince Angelo Batecan</b> — Case 01 Primary Suspect\n" +
                "• <b>Kirby Raymundo</b> — Manor Owner & Aristocrat\n" +
                "• <b>Charl Vonn Pascual</b> — Night Security Guard\n" +
                "• <b>Paul Gabriel Camacho</b> — Art Gallery Owner\n" +
                "• <b>Shanaia Ortega</b> — Lead Software Developer\n" +
                "• <b>Shan Jaraba</b> — Coffee Shop Manager\n" +
                "• <b>Kurt Miguel Ancheta</b> — Startup Founder\n" +
                "• <b>Kyle Gabriel Pastrana & Miguel Borja</b> — Detective Agency\n\n" +
                "<i>Built with Unity 2D Engine (URP)</i>";

            GameObject credBody = CreateText(credCont, "Text_CreditsBody", credTextStr, 15, FontStyle.Normal, ColorTextWhite, TextAnchor.UpperLeft);
            SetupRectTransform(credBody, new Vector2(0.08f, 0.15f), new Vector2(0.92f, 0.86f), Vector2.zero, Vector2.zero);

            GameObject btnBackCred = CreateButton(credCont, "Button_Back", "◀ BACK TO MAIN MENU", new Vector2(0f, 15f), new Vector2(240f, 40f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            mainMenuUI.backFromCreditsButton = btnBackCred.GetComponent<Button>();
            credCont.SetActive(false);

            // Connect UIManager panel references
            uiManager.mainMenuPanel = mainMenuPanel;
        }

        private static void BuildInGameUIPanels(GameObject canvasObj, UIManager uiManager, int caseLevel)
        {
            // 1. In-Game Background / Table Panel Container
            GameObject mainTablePanel = EnsureChild(canvasObj, "Panel_MainTable");
            SetupRectTransform(mainTablePanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // 2. Header Navigation Bar
            GameObject headerNav = EnsureChild(canvasObj, "Panel_HeaderNav");
            SetupRectTransform(headerNav, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -60f), new Vector2(0f, 0f));
            Image headerBg = EnsureComponent<Image>(headerNav);
            headerBg.color = new Color(0.06f, 0.07f, 0.09f, 0.90f);

            GameObject btnMenu = CreateButton(headerNav, "Button_ReturnToMenu", "◀ MAIN MENU", new Vector2(10f, 10f), new Vector2(180f, 40f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            GameObject btnNotebook = CreateButton(headerNav, "Button_Notebook", "CASE NOTEBOOK", new Vector2(200f, 10f), new Vector2(180f, 40f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            GameObject btnDeduction = CreateButton(headerNav, "Button_DeductionBoard", "DEDUCTION BOARD", new Vector2(390f, 10f), new Vector2(200f, 40f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            GameObject btnConclude = CreateButton(headerNav, "Button_ConcludeCase", "CONCLUDE CASE ▶", new Vector2(-190f, 10f), new Vector2(180f, 40f), new Vector2(1f, 0f), new Vector2(1f, 0f));

            // Case Banner in center of Header
            string caseTitleHeader = $"CASE 00{caseLevel}: {(caseLevel == 1 ? "The Missing Necklace" : caseLevel == 2 ? "The Shattered Mirror" : "The Last Call")}";
            GameObject caseTitleObj = CreateText(headerNav, "Text_CaseTitleHeader", caseTitleHeader, 18, FontStyle.Bold, ColorHeaderGold, TextAnchor.MiddleCenter);
            SetupRectTransform(caseTitleObj, new Vector2(0.35f, 0f), new Vector2(0.65f, 1f), Vector2.zero, Vector2.zero);

            // 3. Dialogue Panel
            GameObject dialoguePanel = EnsureChild(canvasObj, "Panel_Dialogue");
            SetupRectTransform(dialoguePanel, new Vector2(0.08f, 0.02f), new Vector2(0.92f, 0.28f), Vector2.zero, Vector2.zero);
            Image diagBg = EnsureComponent<Image>(dialoguePanel);
            diagBg.color = ColorSlatePanel;
            DialogueUI dialogueUI = EnsureComponent<DialogueUI>(dialoguePanel);

            GameObject speakerTextObj = CreateText(dialoguePanel, "Text_SpeakerName", "Suspect Name", 22, FontStyle.Bold, ColorHeaderGold, TextAnchor.MiddleLeft);
            SetupRectTransform(speakerTextObj, new Vector2(0.03f, 0.75f), new Vector2(0.5f, 0.95f), Vector2.zero, Vector2.zero);
            dialogueUI.speakerNameText = speakerTextObj.GetComponent<Text>();

            GameObject bodyTextObj = CreateText(dialoguePanel, "Text_DialogueBody", "Dialogue statement will appear here...", 18, FontStyle.Normal, ColorTextWhite, TextAnchor.UpperLeft);
            SetupRectTransform(bodyTextObj, new Vector2(0.03f, 0.15f), new Vector2(0.78f, 0.72f), Vector2.zero, Vector2.zero);
            dialogueUI.dialogueBodyText = bodyTextObj.GetComponent<Text>();

            GameObject btnNext = CreateButton(dialoguePanel, "Button_Next", "NEXT ▶", new Vector2(-140f, 15f), new Vector2(120f, 45f), new Vector2(1f, 0f), new Vector2(1f, 0f));
            dialogueUI.nextButton = btnNext.GetComponent<Button>();

            GameObject btnChallenge = CreateButton(dialoguePanel, "Button_Challenge", "⚡ CHALLENGE", new Vector2(-140f, 70f), new Vector2(120f, 45f), new Vector2(1f, 0f), new Vector2(1f, 0f));
            btnChallenge.GetComponent<Image>().color = new Color(0.6f, 0.18f, 0.18f, 1f);
            dialogueUI.challengeButton = btnChallenge.GetComponent<Button>();

            // Evidence Picker Container inside Dialogue Panel
            GameObject evPicker = EnsureChild(dialoguePanel, "Container_EvidencePicker");
            SetupRectTransform(evPicker, new Vector2(0f, 1f), new Vector2(1f, 2.3f), Vector2.zero, Vector2.zero);
            Image pickerBg = EnsureComponent<Image>(evPicker);
            pickerBg.color = new Color(0.08f, 0.10f, 0.14f, 0.96f);
            GameObject pickerGrid = EnsureChild(evPicker, "Grid_Evidence");
            SetupRectTransform(pickerGrid, new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.85f), Vector2.zero, Vector2.zero);
            GridLayoutGroup grid = EnsureComponent<GridLayoutGroup>(pickerGrid);
            grid.cellSize = new Vector2(140f, 80f);
            grid.spacing = new Vector2(15f, 15f);
            dialogueUI.evidencePickerContainer = evPicker;
            dialogueUI.evidencePickerGrid = pickerGrid.transform;
            evPicker.SetActive(false);

            // 4. Case File Notebook Panel
            GameObject notebookPanel = EnsureChild(canvasObj, "Panel_CaseFileNotebook");
            SetupRectTransform(notebookPanel, new Vector2(0.12f, 0.08f), new Vector2(0.88f, 0.92f), Vector2.zero, Vector2.zero);
            Image noteBg = EnsureComponent<Image>(notebookPanel);
            noteBg.color = ColorDarkBg;
            CaseFileNotebookUI notebookUI = EnsureComponent<CaseFileNotebookUI>(notebookPanel);

            GameObject noteTitle = CreateText(notebookPanel, "Text_Title", "Case Dossier Notebook", 26, FontStyle.Bold, ColorHeaderGold, TextAnchor.MiddleCenter);
            SetupRectTransform(noteTitle, new Vector2(0.1f, 0.88f), new Vector2(0.9f, 0.98f), Vector2.zero, Vector2.zero);
            notebookUI.notebookTitleText = noteTitle.GetComponent<Text>();

            GameObject btnTabSum = CreateButton(notebookPanel, "Button_TabSummary", "Summary", new Vector2(20f, -80f), new Vector2(140f, 40f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            GameObject btnTabSus = CreateButton(notebookPanel, "Button_TabSuspects", "Suspects", new Vector2(170f, -80f), new Vector2(140f, 40f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            GameObject btnTabEvd = CreateButton(notebookPanel, "Button_TabEvidence", "Evidence", new Vector2(320f, -80f), new Vector2(140f, 40f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            GameObject btnTabClu = CreateButton(notebookPanel, "Button_TabClues", "Clues", new Vector2(470f, -80f), new Vector2(140f, 40f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            GameObject btnCloseNote = CreateButton(notebookPanel, "Button_CloseNotebook", "✕ CLOSE", new Vector2(-140f, -80f), new Vector2(120f, 40f), new Vector2(1f, 1f), new Vector2(1f, 1f));

            notebookUI.summaryTabButton = btnTabSum.GetComponent<Button>();
            notebookUI.suspectsTabButton = btnTabSus.GetComponent<Button>();
            notebookUI.evidenceTabButton = btnTabEvd.GetComponent<Button>();
            notebookUI.cluesTabButton = btnTabClu.GetComponent<Button>();
            notebookUI.closeNotebookButton = btnCloseNote.GetComponent<Button>();

            GameObject noteBody = CreateText(notebookPanel, "Text_ContentBody", "Notebook content...", 16, FontStyle.Normal, ColorTextWhite, TextAnchor.UpperLeft);
            SetupRectTransform(noteBody, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.78f), Vector2.zero, Vector2.zero);
            notebookUI.notebookContentBody = noteBody.GetComponent<Text>();
            notebookPanel.SetActive(false);

            // 5. Inspect Modal Panel
            GameObject inspectPanel = EnsureChild(canvasObj, "Panel_InspectModal");
            SetupRectTransform(inspectPanel, new Vector2(0.18f, 0.12f), new Vector2(0.82f, 0.88f), Vector2.zero, Vector2.zero);
            Image inspBg = EnsureComponent<Image>(inspectPanel);
            inspBg.color = ColorDarkBg;
            EvidenceInspectModal inspectUI = EnsureComponent<EvidenceInspectModal>(inspectPanel);

            GameObject inspTitle = CreateText(inspectPanel, "Text_EvidenceTitle", "Evidence Close-Up Inspection", 24, FontStyle.Bold, ColorHeaderGold, TextAnchor.MiddleCenter);
            SetupRectTransform(inspTitle, new Vector2(0.1f, 0.88f), new Vector2(0.9f, 0.98f), Vector2.zero, Vector2.zero);
            inspectUI.evidenceTitleText = inspTitle.GetComponent<Text>();

            GameObject inspImg = EnsureChild(inspectPanel, "Image_Zoomed");
            SetupRectTransform(inspImg, new Vector2(0.15f, 0.18f), new Vector2(0.85f, 0.82f), Vector2.zero, Vector2.zero);
            Image zoomImg = EnsureComponent<Image>(inspImg);
            zoomImg.preserveAspect = true;
            inspectUI.evidenceZoomImage = zoomImg;

            GameObject hotspotContainer = EnsureChild(inspImg, "Container_Hotspots");
            SetupRectTransform(hotspotContainer, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            inspectUI.hotspotsContainer = hotspotContainer.GetComponent<RectTransform>();

            GameObject btnRotL = CreateButton(inspectPanel, "Button_RotateLeft", "⟲ -90°", new Vector2(30f, 20f), new Vector2(100f, 40f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            GameObject btnRotR = CreateButton(inspectPanel, "Button_RotateRight", "⟳ +90°", new Vector2(140f, 20f), new Vector2(100f, 40f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            GameObject btnCloseInsp = CreateButton(inspectPanel, "Button_CloseInspect", "✕ CLOSE", new Vector2(-140f, 20f), new Vector2(120f, 40f), new Vector2(1f, 0f), new Vector2(1f, 0f));

            inspectUI.rotateLeftButton = btnRotL.GetComponent<Button>();
            inspectUI.rotateRightButton = btnRotR.GetComponent<Button>();
            inspectUI.closeButton = btnCloseInsp.GetComponent<Button>();
            inspectPanel.SetActive(false);

            // 6. Deduction Board Panel
            GameObject deductionPanel = EnsureChild(canvasObj, "Panel_DeductionBoard");
            SetupRectTransform(deductionPanel, new Vector2(0.1f, 0.08f), new Vector2(0.9f, 0.92f), Vector2.zero, Vector2.zero);
            Image dedBg = EnsureComponent<Image>(deductionPanel);
            dedBg.color = new Color(0.18f, 0.12f, 0.08f, 0.96f);
            deductionPanel.SetActive(false);

            // 7. Conclusion Quiz Panel
            GameObject conclusionPanel = EnsureChild(canvasObj, "Panel_ConclusionQuiz");
            SetupRectTransform(conclusionPanel, new Vector2(0.12f, 0.08f), new Vector2(0.88f, 0.92f), Vector2.zero, Vector2.zero);
            Image concBg = EnsureComponent<Image>(conclusionPanel);
            concBg.color = ColorDarkBg;
            ConclusionUI conclusionUI = EnsureComponent<ConclusionUI>(conclusionPanel);

            // Quiz Container
            GameObject quizCont = EnsureChild(conclusionPanel, "Container_Quiz");
            SetupRectTransform(quizCont, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            conclusionUI.quizContainer = quizCont;

            GameObject concTitle = CreateText(quizCont, "Text_QuizTitle", "CASE CONCLUSION INQUIRY", 26, FontStyle.Bold, ColorHeaderGold, TextAnchor.MiddleCenter);
            SetupRectTransform(concTitle, new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.98f), Vector2.zero, Vector2.zero);
            conclusionUI.questionTitleText = concTitle.GetComponent<Text>();

            GameObject optGrid = EnsureChild(quizCont, "Grid_Options");
            SetupRectTransform(optGrid, new Vector2(0.05f, 0.18f), new Vector2(0.95f, 0.85f), Vector2.zero, Vector2.zero);
            VerticalLayoutGroup vert = EnsureComponent<VerticalLayoutGroup>(optGrid);
            vert.spacing = 8f;
            vert.childControlHeight = false;
            vert.childControlWidth = true;
            conclusionUI.optionsGrid = optGrid.transform;

            GameObject btnSubmitConc = CreateButton(quizCont, "Button_SubmitConclusion", "SUBMIT VERDICT", new Vector2(0f, 20f), new Vector2(260f, 50f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            btnSubmitConc.GetComponent<Image>().color = new Color(0.65f, 0.5f, 0.15f, 1f);
            conclusionUI.submitConclusionButton = btnSubmitConc.GetComponent<Button>();

            // Results Screen Overlay
            GameObject resultsCont = EnsureChild(conclusionPanel, "Container_Results");
            SetupRectTransform(resultsCont, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image resBg = EnsureComponent<Image>(resultsCont);
            resBg.color = new Color(0.06f, 0.07f, 0.09f, 0.98f);
            conclusionUI.resultsContainer = resultsCont;

            GameObject resTitle = CreateText(resultsCont, "Text_ResultTitle", "CASE SOLVED!", 36, FontStyle.Bold, Color.green, TextAnchor.MiddleCenter);
            SetupRectTransform(resTitle, new Vector2(0.1f, 0.80f), new Vector2(0.9f, 0.92f), Vector2.zero, Vector2.zero);
            conclusionUI.resultTitleText = resTitle.GetComponent<Text>();

            GameObject resGrade = CreateText(resultsCont, "Text_ResultGrade", "GRADE: S", 28, FontStyle.Bold, ColorHeaderGold, TextAnchor.MiddleCenter);
            SetupRectTransform(resGrade, new Vector2(0.1f, 0.70f), new Vector2(0.9f, 0.80f), Vector2.zero, Vector2.zero);
            conclusionUI.resultGradeText = resGrade.GetComponent<Text>();

            GameObject resStars = CreateText(resultsCont, "Text_StarRating", "★ ★ ★ ★ ★", 28, FontStyle.Bold, Color.yellow, TextAnchor.MiddleCenter);
            SetupRectTransform(resStars, new Vector2(0.1f, 0.60f), new Vector2(0.9f, 0.70f), Vector2.zero, Vector2.zero);
            conclusionUI.starRatingText = resStars.GetComponent<Text>();

            GameObject resBreakdown = CreateText(resultsCont, "Text_Breakdown", "Total Score: 1000 pts\nCorrect Quiz Answers: 3/3\nEvidence Discovered: 3/3\nTime Taken: 2m 15s", 18, FontStyle.Normal, ColorTextWhite, TextAnchor.MiddleCenter);
            SetupRectTransform(resBreakdown, new Vector2(0.1f, 0.25f), new Vector2(0.9f, 0.58f), Vector2.zero, Vector2.zero);
            conclusionUI.scoreBreakdownText = resBreakdown.GetComponent<Text>();

            GameObject btnContinue = CreateButton(resultsCont, "Button_Continue", "REVIEW CASE", new Vector2(-220f, 40f), new Vector2(180f, 50f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            conclusionUI.continueButton = btnContinue.GetComponent<Button>();

            GameObject btnNextLvl = CreateButton(resultsCont, "Button_NextLevel", "NEXT CASE ▶", new Vector2(0f, 40f), new Vector2(200f, 50f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            btnNextLvl.GetComponent<Image>().color = new Color(0.2f, 0.5f, 0.2f, 1f);
            conclusionUI.nextLevelButton = btnNextLvl.GetComponent<Button>();
            conclusionUI.nextLevelButtonText = btnNextLvl.GetComponentInChildren<Text>();

            GameObject btnResMenu = CreateButton(resultsCont, "Button_ReturnToMenu", "MAIN MENU ◀", new Vector2(220f, 40f), new Vector2(180f, 50f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            btnResMenu.GetComponent<Image>().color = ColorHeaderGold;
            btnResMenu.GetComponentInChildren<Text>().color = Color.black;
            conclusionUI.returnToMainMenuButton = btnResMenu.GetComponent<Button>();

            resultsCont.SetActive(false);
            conclusionPanel.SetActive(false);

            // Connect UIManager panel references
            uiManager.mainTablePanel = mainTablePanel;
            uiManager.inspectModalPanel = inspectPanel;
            uiManager.notebookPanel = notebookPanel;
            uiManager.deductionBoardPanel = deductionPanel;
            uiManager.conclusionQuizPanel = conclusionPanel;
            uiManager.resultsScreenPanel = resultsCont;

            uiManager.notebookButton = btnNotebook;
            uiManager.deductionBoardButton = btnDeduction;
            uiManager.concludeCaseButton = btnConclude;
            uiManager.returnToMenuButton = btnMenu;

            // Hook UIManager buttons
            btnNotebook.GetComponent<Button>().onClick.AddListener(uiManager.ToggleNotebookPanel);
            btnDeduction.GetComponent<Button>().onClick.AddListener(uiManager.ToggleDeductionBoardPanel);
            btnConclude.GetComponent<Button>().onClick.AddListener(uiManager.OpenConclusionQuiz);
            btnMenu.GetComponent<Button>().onClick.AddListener(uiManager.ReturnToMainMenu);

            // Start in Investigation Table view
            uiManager.ShowPanel(UIPanelType.InvestigationTable);
        }

        #endregion

        #region UI Building Helpers

        private static T EnsureComponent<T>(GameObject target) where T : Component
        {
            T comp = target.GetComponent<T>();
            if (comp == null) comp = target.AddComponent<T>();
            return comp;
        }

        private static GameObject EnsureChild(GameObject parent, string childName)
        {
            Transform existing = parent.transform.Find(childName);
            if (existing != null) return existing.gameObject;

            GameObject child = new GameObject(childName, typeof(RectTransform));
            child.transform.SetParent(parent.transform, false);
            return child;
        }

        private static void SetupRectTransform(GameObject target, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            RectTransform rt = target.GetComponent<RectTransform>();
            if (rt == null) rt = target.AddComponent<RectTransform>();

            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        private static GameObject CreateText(GameObject parent, string name, string text, int fontSize, FontStyle style, Color color, TextAnchor alignment)
        {
            GameObject obj = EnsureChild(parent, name);
            EnsureComponent<CanvasRenderer>(obj);
            Text t = EnsureComponent<Text>(obj);
            t.text = text;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.color = color;
            t.alignment = alignment;
            t.supportRichText = true;
            return obj;
        }

        private static GameObject CreateButton(GameObject parent, string name, string label, Vector2 anchoredPos, Vector2 sizeDelta, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject btnObj = EnsureChild(parent, name);
            EnsureComponent<CanvasRenderer>(btnObj);

            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Image img = EnsureComponent<Image>(btnObj);
            img.color = ColorButtonNormal;

            Button btn = EnsureComponent<Button>(btnObj);
            ColorBlock cb = btn.colors;
            cb.normalColor = ColorButtonNormal;
            cb.highlightedColor = ColorButtonHighlight;
            cb.pressedColor = ColorButtonPressed;
            cb.selectedColor = ColorButtonHighlight;
            btn.colors = cb;

            GameObject labelObj = CreateText(btnObj, "Text_Label", label, 16, FontStyle.Normal, ColorTextWhite, TextAnchor.MiddleCenter);
            SetupRectTransform(labelObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            return btnObj;
        }

        private static GameObject CreateSlider(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject sliderObj = EnsureChild(parent, name);
            SetupRectTransform(sliderObj, anchorMin, anchorMax, Vector2.zero, Vector2.zero);

            GameObject bgObj = EnsureChild(sliderObj, "Background");
            SetupRectTransform(bgObj, new Vector2(0f, 0.25f), new Vector2(1f, 0.75f), Vector2.zero, Vector2.zero);
            Image bgImg = EnsureComponent<Image>(bgObj);
            bgImg.color = new Color(0.15f, 0.17f, 0.22f, 1f);

            GameObject fillArea = EnsureChild(sliderObj, "Fill Area");
            SetupRectTransform(fillArea, new Vector2(0f, 0.25f), new Vector2(1f, 0.75f), new Vector2(5f, 0f), new Vector2(-5f, 0f));
            GameObject fill = EnsureChild(fillArea, "Fill");
            SetupRectTransform(fill, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image fillImg = EnsureComponent<Image>(fill);
            fillImg.color = ColorHeaderGold;

            GameObject handleArea = EnsureChild(sliderObj, "Handle Slide Area");
            SetupRectTransform(handleArea, Vector2.zero, Vector2.one, new Vector2(10f, 0f), new Vector2(-10f, 0f));
            GameObject handle = EnsureChild(handleArea, "Handle");
            SetupRectTransform(handle, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(-10f, 0f), new Vector2(10f, 0f));
            Image handleImg = EnsureComponent<Image>(handle);
            handleImg.color = ColorTextWhite;

            Slider slider = EnsureComponent<Slider>(sliderObj);
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            return sliderObj;
        }

        private static GameObject CreateToggle(GameObject parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject toggleObj = EnsureChild(parent, name);
            SetupRectTransform(toggleObj, anchorMin, anchorMax, Vector2.zero, Vector2.zero);

            GameObject bgObj = EnsureChild(toggleObj, "Background");
            SetupRectTransform(bgObj, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, -12f), new Vector2(24f, 12f));
            Image bgImg = EnsureComponent<Image>(bgObj);
            bgImg.color = ColorButtonNormal;

            GameObject checkObj = EnsureChild(bgObj, "Checkmark");
            SetupRectTransform(checkObj, Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f));
            Image checkImg = EnsureComponent<Image>(checkObj);
            checkImg.color = ColorHeaderGold;

            GameObject labelObj = CreateText(toggleObj, "Label", label, 15, FontStyle.Normal, ColorTextWhite, TextAnchor.MiddleLeft);
            SetupRectTransform(labelObj, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(35f, 0f), Vector2.zero);

            Toggle toggle = EnsureComponent<Toggle>(toggleObj);
            toggle.graphic = checkImg;
            toggle.targetGraphic = bgImg;
            toggle.isOn = true;

            return toggleObj;
        }

        #endregion
    }
}
