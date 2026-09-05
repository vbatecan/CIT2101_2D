using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Gameplay;
using CaseClosed.Managers;
using CaseClosed.UI;

namespace CaseClosed.Prototype
{
    /// <summary>
    /// Entrypoint bootstrapping MonoBehaviour that initializes all managers, attaches required camera scripts,
    /// registers selectable investigators, and handles keyboard shortcuts (Keys 1, 2, 3) for quick level switching.
    /// Can be dragged directly onto a GameBootstrap GameObject in the Unity Inspector.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        /// <summary>Whether to display the Main Menu on launch instead of auto-loading a level.</summary>
        public bool startOnMainMenu = true;

        private Case01Initializer level1;
        private Case02Initializer level2;
        private Case03Initializer level3;

        private CharacterProfileSO investigatorKyle;

        /// <summary>
        /// Ensures all singleton managers exist, sets up fixed camera, registers investigators, and registers level initializers.
        /// </summary>
        private void Awake()
        {
            Debug.Log("[Prototype:Bootstrap] GameBootstrap initializing core managers, investigators, and systems...");

            SetupFixedCamera();
            EnsureManager<AudioManager>();
            EnsureManager<CaseManager>();
            EnsureManager<EvidenceManager>();
            EnsureManager<InterrogationManager>();
            EnsureManager<DeductionBoardController>();
            EnsureManager<CaseConclusionManager>();
            EnsureManager<UIManager>();

            SetupInvestigators();

            level1 = gameObject.GetComponent<Case01Initializer>() ?? FindFirstObjectByType<Case01Initializer>();
            level2 = gameObject.GetComponent<Case02Initializer>() ?? FindFirstObjectByType<Case02Initializer>();
            level3 = gameObject.GetComponent<Case03Initializer>() ?? FindFirstObjectByType<Case03Initializer>();

            Debug.Log("[Prototype:Bootstrap] All managers and investigators initialized. Ready for level loading.");
        }

        /// <summary>
        /// Instantiates and registers the investigator character: Detective Kyle Gabriel Pastrana.
        /// </summary>
        private void SetupInvestigators()
        {
            // Investigator: Detective Kyle Gabriel Pastrana
            investigatorKyle = ScriptableObject.CreateInstance<CharacterProfileSO>();
            investigatorKyle.characterId = "CHAR_KYLE_PASTRANA";
            investigatorKyle.fullName = "Detective Kyle Gabriel Pastrana";
            investigatorKyle.age = 34;
            investigatorKyle.occupation = "Lead Field Detective";
            investigatorKyle.personalityTrait = PersonalityTrait.Observant;
            investigatorKyle.background = "Veteran lead field detective with sharp intuition for physical clues, crime scenes, and catching suspect contradictions.";

            if (CaseManager.Instance != null)
            {
                CaseManager.Instance.RegisterAvailableInvestigator(investigatorKyle);
                CaseManager.Instance.SetSelectedInvestigator(investigatorKyle);
            }
        }

        /// <summary>
        /// Displays Main Menu or default level upon start.
        /// </summary>
        private void Start()
        {
            string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Debug.Log($"[Prototype:Bootstrap] GameBootstrap Start in scene '{activeScene}', startOnMainMenu={startOnMainMenu}");

            if (activeScene == "MainMenu" || (startOnMainMenu && activeScene != "Case001" && activeScene != "Case002" && activeScene != "Case003"))
            {
                UIManager.Instance?.ShowPanel(UIPanelType.MainMenu);
                AudioManager.Instance?.PlayMenuBGM();
                return;
            }

            int targetLevel = 1;
            if (activeScene.Contains("002") || activeScene.Contains("Case2") || activeScene.Contains("Case02"))
            {
                targetLevel = 2;
            }
            else if (activeScene.Contains("003") || activeScene.Contains("Case3") || activeScene.Contains("Case03"))
            {
                targetLevel = 3;
            }

            // If a case is already loaded matching this level, keep it
            if (CaseManager.Instance != null && CaseManager.Instance.activeCase != null && CaseManager.Instance.activeCase.levelNumber == targetLevel)
            {
                UIManager.Instance?.ShowPanel(UIPanelType.InvestigationTable);
                ArmPointerController.Instance?.ForceSyncState();
                return;
            }

            LoadLevel(targetLevel);
        }

        /// <summary>
        /// Listens for number key presses (1, 2, 3) to dynamically switch active cases / levels,
        /// and (Esc / M) to return to Main Menu.
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToCaseSceneOrLevel(1);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToCaseSceneOrLevel(2);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToCaseSceneOrLevel(3);
            else if (Input.GetKeyDown(KeyCode.M))
            {
                UIManager.Instance?.ToggleInGameMenu();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                // If Evidence Inspect Modal is currently open, let EvidenceInspectModal handle closing (do NOT return to Main Menu)
                if (EvidenceManager.Instance != null && EvidenceManager.Instance.isInspectingModalOpen)
                {
                    // Handled by EvidenceInspectModal
                }
                else if (UIManager.Instance != null && UIManager.Instance.currentPanel != UIPanelType.InvestigationTable && UIManager.Instance.currentPanel != UIPanelType.MainMenu)
                {
                    // Return to investigation table from sub-panels (Notebook, Deduction Board, etc.)
                    UIManager.Instance.ShowPanel(UIPanelType.InvestigationTable);
                }
                else
                {
                    UIManager.Instance?.ToggleInGameMenu();
                }
            }
        }

        /// <summary>
        /// Switches to the dedicated scene for the given case index if loaded/available, or calls <see cref="LoadLevel"/>.
        /// </summary>
        /// <param name="caseIndex">The 1-based case index (1, 2, 3).</param>
        public void SwitchToCaseSceneOrLevel(int caseIndex)
        {
            string sceneName = $"Case00{caseIndex}";
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.Log($"[Prototype:Bootstrap] Loading scene '{sceneName}'...");
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
            else
            {
                LoadLevel(caseIndex);
            }
        }

        /// <summary>
        /// Loads the data corresponding to a specified case index and initializes managers.
        /// </summary>
        /// <param name="levelIndex">The 1-based level index (1, 2, or 3).</param>
        public void LoadLevel(int levelIndex)
        {
            Debug.Log($"[Prototype:Bootstrap] Shortcut triggered: Loading Level {levelIndex}...");

            CaseClosed.Data.CaseSO caseData = null;

            switch (levelIndex)
            {
                case 1:
                    if (level1 == null) level1 = gameObject.GetComponent<Case01Initializer>() ?? FindFirstObjectByType<Case01Initializer>() ?? gameObject.AddComponent<Case01Initializer>();
                    caseData = level1.CreateCase01Data();
                    break;
                case 2:
                    if (level2 == null) level2 = gameObject.GetComponent<Case02Initializer>() ?? FindFirstObjectByType<Case02Initializer>() ?? gameObject.AddComponent<Case02Initializer>();
                    caseData = level2.CreateCase02Data();
                    break;
                case 3:
                    if (level3 == null) level3 = gameObject.GetComponent<Case03Initializer>() ?? FindFirstObjectByType<Case03Initializer>() ?? gameObject.AddComponent<Case03Initializer>();
                    caseData = level3.CreateCase03Data();
                    break;
            }

            if (caseData != null)
            {
                CaseManager.Instance?.LoadCase(caseData);
                if (InterrogationManager.Instance != null && caseData.primarySuspect != null && caseData.dialogueTrees.Count > 0)
                {
                    InterrogationManager.Instance.SetInterrogationTarget(caseData.primarySuspect, caseData.dialogueTrees[0]);
                }
                UIManager.Instance?.ShowPanel(UIPanelType.InvestigationTable);
                ArmPointerController.Instance?.ForceSyncState();
            }
            else
            {
                Debug.LogWarning($"[Prototype:Bootstrap] Failed to generate case data for level {levelIndex}");
            }
        }

        /// <summary>
        /// Locates or instantiates the Main Camera and ensures the <see cref="FixedInvestigationCamera"/> script is attached.
        /// </summary>
        private void SetupFixedCamera()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                camObj.tag = "MainCamera";
                mainCam = camObj.GetComponent<Camera>();
            }

            if (mainCam.GetComponent<FixedInvestigationCamera>() == null)
            {
                mainCam.gameObject.AddComponent<FixedInvestigationCamera>();
            }

        }

        /// <summary>
        /// Locates an existing manager MonoBehaviour in the scene or creates a new GameObject with the component.
        /// </summary>
        /// <typeparam name="T">The MonoBehaviour manager type to ensure.</typeparam>
        /// <returns>The existing or newly instantiated manager component.</returns>
        private T EnsureManager<T>() where T : MonoBehaviour
        {
            T manager = FindFirstObjectByType<T>();
            if (manager == null)
            {
                GameObject managerObj = new GameObject(typeof(T).Name);
                manager = managerObj.AddComponent<T>();
            }
            return manager;
        }
    }
}
