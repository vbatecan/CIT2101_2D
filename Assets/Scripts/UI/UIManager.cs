using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Gameplay;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    /// <summary>
    /// Master UI View coordinator MonoBehaviour managing canvas panel states, modal dialogs, and navigation buttons.
    /// Can be dragged directly onto the Canvas/UIManager GameObject in the Unity Inspector.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        /// <summary>Singleton instance of the UIManager.</summary>
        public static UIManager Instance { get; private set; }

        [Header("UI Panels")]
        public GameObject mainMenuPanel;
        public GameObject mainTablePanel;
        public GameObject inspectModalPanel;
        public GameObject notebookPanel;
        public GameObject suspectFolderPanel;
        public GameObject deductionBoardPanel;
        public GameObject conclusionQuizPanel;
        public GameObject resultsScreenPanel;
        public GameObject investigatorSelectPanel;
        public GameObject gameOverPanel;
        public GameObject inGameMenuPanel;
        public GameObject mainMenuConfirmPanel;

        [Header("Header Navigation Elements")]
        public GameObject timerContainer;
        public GameObject notebookButton;
        public GameObject suspectFolderButton;
        public GameObject deductionBoardButton;
        public GameObject concludeCaseButton;
        public GameObject investigatorSelectButton;
        public GameObject returnToMenuButton;

        [Header("In-Game Menu Buttons")]
        public Button resumeGameButton;
        public Button inGameMainMenuButton;
        public Button confirmMainMenuYesButton;
        public Button confirmMainMenuNoButton;

        private UIPanelType _currentPanel = UIPanelType.MainMenu;
        private UIPanelType _panelBeforeInGameMenu = UIPanelType.InvestigationTable;
        private EvidenceManager _subscribedEvidenceManager;
        private CaseManager _subscribedCaseManager;
        private InterrogationManager _subscribedInterrogationManager;

        /// <summary>The currently active UI panel type.</summary>
        public UIPanelType currentPanel => _currentPanel;

        /// <summary>
        /// Initializes the singleton instance.
        /// </summary>
        private void Awake()
        {
            Instance = this;

            // Enforce sharp font rasterization and pixel-perfect canvas alignment at runtime
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.pixelPerfect = true;
            }

            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler != null && scaler.dynamicPixelsPerUnit < 3.0f)
            {
                scaler.dynamicPixelsPerUnit = 3.0f;
            }
        }

        private void OnDestroy()
        {
            UnregisterEvents();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Shows the initial UI panel and hooks event listeners on start.
        /// </summary>
        private void Start()
        {
            if (returnToMenuButton != null)
            {
                Button btn = returnToMenuButton.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(ToggleInGameMenu);
            }

            if (resumeGameButton != null)
            {
                resumeGameButton.onClick.AddListener(CloseInGameMenu);
            }

            if (inGameMainMenuButton != null)
            {
                inGameMainMenuButton.onClick.AddListener(OpenMainMenuConfirmation);
            }

            if (confirmMainMenuYesButton != null)
            {
                confirmMainMenuYesButton.onClick.AddListener(ConfirmReturnToMainMenu);
            }

            if (confirmMainMenuNoButton != null)
            {
                confirmMainMenuNoButton.onClick.AddListener(CloseMainMenuConfirmation);
            }

            if (notebookButton != null)
            {
                Button btn = notebookButton.GetComponentInChildren<Button>(true);
                if (btn != null) btn.onClick.AddListener(ToggleNotebookPanel);
            }

            if (suspectFolderButton == null)
            {
                suspectFolderButton = GameObject.Find("SuspectFolderButton")
                                   ?? GameObject.Find("ButtonFOLDER_0")
                                   ?? GameObject.Find("ButtonFOLDER");
            }

            if (suspectFolderButton != null && suspectFolderButton.GetComponent<SuspectFolderButton>() == null)
            {
                Button btn = suspectFolderButton.GetComponentInChildren<Button>(true);
                if (btn != null)
                {
                    btn.onClick.RemoveListener(ToggleSuspectFolderPanel);
                    btn.onClick.AddListener(ToggleSuspectFolderPanel);
                }
            }

            if (suspectFolderPanel == null)
            {
                var found = Object.FindFirstObjectByType<SuspectFolderUI>(FindObjectsInactive.Include);
                if (found != null)
                {
                    suspectFolderPanel = found.gameObject;
                }
                else
                {
#if UNITY_EDITOR
                    var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Panels/Panel_SuspectFolder.prefab");
                    if (prefab != null)
                    {
                        suspectFolderPanel = Instantiate(prefab, transform);
                        suspectFolderPanel.name = "Panel_SuspectFolder";
                        suspectFolderPanel.SetActive(false);
                    }
#endif
                }
            }

            if (deductionBoardButton != null)
            {
                Button btn = deductionBoardButton.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(ToggleDeductionBoardPanel);
            }

            if (concludeCaseButton == null)
            {
                concludeCaseButton = GameObject.Find("ConcludeCaseButton")
                                  ?? GameObject.Find("Button_ConcludeCase")
                                  ?? GameObject.Find("ButtonCONCLUDE_0")
                                  ?? GameObject.Find("ButtonCONCLUDE");

                if (concludeCaseButton == null)
                {
                    foreach (Transform child in GetComponentsInChildren<Transform>(true))
                    {
                        if (child.name.IndexOf("Conclude", System.StringComparison.OrdinalIgnoreCase) >= 0 &&
                            !child.name.Contains("Quiz") && !child.name.Contains("Panel"))
                        {
                            concludeCaseButton = child.gameObject;
                            break;
                        }
                    }
                }
            }

            if (concludeCaseButton != null)
            {
                var concludeComp = concludeCaseButton.GetComponent<ConcludeCaseButton>() ?? concludeCaseButton.AddComponent<ConcludeCaseButton>();
                concludeComp.EnsureClickable();

                Button btn = concludeCaseButton.GetComponentInChildren<Button>(true);
                if (btn != null)
                {
                    btn.onClick.RemoveListener(OpenConclusionQuiz);
                    btn.onClick.AddListener(OpenConclusionQuiz);
                }
            }

            UIPanelType initialPanel = (mainMenuPanel != null) ? UIPanelType.MainMenu : UIPanelType.InvestigationTable;
            ShowPanel(initialPanel);
            RegisterEvents();
            UpdateConclusionButtonState();
            UIButtonHighlightSystem.ApplyToAllButtonsInScene();
        }

        private void OnEnable()
        {
            RegisterEvents();
        }

        private void OnDisable()
        {
            UnregisterEvents();
        }

        /// <summary>
        /// Registers event handlers for modal transitions and case expiration notifications.
        /// </summary>
        private void RegisterEvents()
        {
            EvidenceManager evidenceManager = EvidenceManager.Instance;
            if (_subscribedEvidenceManager != evidenceManager)
            {
                if (_subscribedEvidenceManager != null)
                {
                    _subscribedEvidenceManager.OnInspectModalOpened -= HandleInspectModalOpened;
                    _subscribedEvidenceManager.OnInspectModalClosed -= HandleInspectModalClosed;
                }

                _subscribedEvidenceManager = evidenceManager;
                if (_subscribedEvidenceManager != null)
                {
                    _subscribedEvidenceManager.OnInspectModalOpened += HandleInspectModalOpened;
                    _subscribedEvidenceManager.OnInspectModalClosed += HandleInspectModalClosed;
                }
            }

            CaseManager caseManager = CaseManager.Instance;
            if (_subscribedCaseManager != caseManager)
            {
                if (_subscribedCaseManager != null)
                {
                    _subscribedCaseManager.OnTimeExpired -= HandleTimeExpired;
                    _subscribedCaseManager.OnEvidenceDiscovered -= HandleEvidenceOrDialogueChanged;
                    _subscribedCaseManager.OnDialogueTreeCompleted -= HandleDialogueTreeCompleted;
                    _subscribedCaseManager.OnConclusionReadinessChanged -= HandleConclusionReadinessChanged;
                    _subscribedCaseManager.OnCaseLoaded -= HandleCaseLoaded;
                }

                _subscribedCaseManager = caseManager;
                if (_subscribedCaseManager != null)
                {
                    _subscribedCaseManager.OnTimeExpired += HandleTimeExpired;
                    _subscribedCaseManager.OnEvidenceDiscovered += HandleEvidenceOrDialogueChanged;
                    _subscribedCaseManager.OnDialogueTreeCompleted += HandleDialogueTreeCompleted;
                    _subscribedCaseManager.OnConclusionReadinessChanged += HandleConclusionReadinessChanged;
                    _subscribedCaseManager.OnCaseLoaded += HandleCaseLoaded;
                }
            }

            InterrogationManager interrogationManager = InterrogationManager.Instance;
            if (_subscribedInterrogationManager != interrogationManager)
            {
                if (_subscribedInterrogationManager != null)
                {
                    _subscribedInterrogationManager.OnDialogueClosed -= HandleEvidenceOrDialogueChanged;
                }

                _subscribedInterrogationManager = interrogationManager;
                if (_subscribedInterrogationManager != null)
                {
                    _subscribedInterrogationManager.OnDialogueClosed += HandleEvidenceOrDialogueChanged;
                }
            }
        }

        private void UnregisterEvents()
        {
            if (_subscribedEvidenceManager != null)
            {
                _subscribedEvidenceManager.OnInspectModalOpened -= HandleInspectModalOpened;
                _subscribedEvidenceManager.OnInspectModalClosed -= HandleInspectModalClosed;
                _subscribedEvidenceManager = null;
            }

            if (_subscribedCaseManager != null)
            {
                _subscribedCaseManager.OnTimeExpired -= HandleTimeExpired;
                _subscribedCaseManager.OnEvidenceDiscovered -= HandleEvidenceOrDialogueChanged;
                _subscribedCaseManager.OnDialogueTreeCompleted -= HandleDialogueTreeCompleted;
                _subscribedCaseManager.OnConclusionReadinessChanged -= HandleConclusionReadinessChanged;
                _subscribedCaseManager.OnCaseLoaded -= HandleCaseLoaded;
                _subscribedCaseManager = null;
            }

            if (_subscribedInterrogationManager != null)
            {
                _subscribedInterrogationManager.OnDialogueClosed -= HandleEvidenceOrDialogueChanged;
                _subscribedInterrogationManager = null;
            }
        }

        private void HandleEvidenceOrDialogueChanged(EvidenceSO evidence)
        {
            UpdateConclusionButtonState();
        }

        private void HandleEvidenceOrDialogueChanged()
        {
            UpdateConclusionButtonState();
        }

        private void HandleDialogueTreeCompleted(string treeId)
        {
            UpdateConclusionButtonState();
        }

        private void HandleConclusionReadinessChanged(bool isReady)
        {
            UpdateConclusionButtonState();
        }

        private void HandleCaseLoaded(CaseSO caseData)
        {
            UpdateConclusionButtonState();
        }

        private void HandleInspectModalOpened(EvidenceSO evidence)
        {
            ShowPanel(UIPanelType.InspectModal);
        }

        private void HandleInspectModalClosed()
        {
            ShowPanel(UIPanelType.InvestigationTable);
            UpdateConclusionButtonState();
        }

        private void UpdateConclusionButtonState()
        {
            if (concludeCaseButton == null)
            {
                concludeCaseButton = GameObject.Find("ConcludeCaseButton")
                                  ?? GameObject.Find("Button_ConcludeCase")
                                  ?? GameObject.Find("ButtonCONCLUDE_0")
                                  ?? GameObject.Find("ButtonCONCLUDE");

                if (concludeCaseButton == null)
                {
                    foreach (Transform child in GetComponentsInChildren<Transform>(true))
                    {
                        if (child.name.IndexOf("Conclude", System.StringComparison.OrdinalIgnoreCase) >= 0 &&
                            !child.name.Contains("Quiz") && !child.name.Contains("Panel"))
                        {
                            concludeCaseButton = child.gameObject;
                            break;
                        }
                    }
                }
            }

            if (concludeCaseButton == null) return;

            bool isReady = CaseManager.Instance != null && CaseManager.Instance.IsReadyForConclusion();

            var concludeComp = concludeCaseButton.GetComponent<ConcludeCaseButton>();
            if (concludeComp != null)
            {
                concludeComp.UpdateReadinessState();
            }

            Button button = concludeCaseButton.GetComponentInChildren<Button>(true);
            if (button != null)
            {
                button.interactable = isReady;
            }

            RegisterEvents();
        }

        private void HandleTimeExpired()
        {
            Debug.Log("[UI:Manager] Received OnTimeExpired event from CaseManager. Showing GameOver overlay.");
            ShowPanel(UIPanelType.GameOver);
        }

        /// <summary>
        /// Activates the requested UI panel and deactivates all other mutually exclusive panels.
        /// </summary>
        /// <param name="panelType">The target <see cref="UIPanelType"/> to activate.</param>
        public void ShowPanel(UIPanelType panelType)
        {
            RegisterEvents();

            // Keep old serialized navigation callbacks compatible with the retired selector.
            if (panelType == UIPanelType.InvestigatorSelect)
                panelType = UIPanelType.InvestigationTable;

            Debug.Log($"[UI:Manager] Transitioning panel from '{_currentPanel}' to '{panelType}'");
            _currentPanel = panelType;

            bool isMainMenu = (panelType == UIPanelType.MainMenu);
            bool isInspect = (panelType == UIPanelType.InspectModal);
            bool isGameOver = (panelType == UIPanelType.GameOver);
            bool isResults = (panelType == UIPanelType.ResultsScreen);
            bool isInGameMenu = (panelType == UIPanelType.InGameMenu);

            if (gameOverPanel == null)
            {
                var foundGO = Object.FindFirstObjectByType<GameOverUI>(FindObjectsInactive.Include);
                if (foundGO != null) gameOverPanel = foundGO.gameObject;
            }

            if (suspectFolderPanel == null)
            {
                var foundGO = Object.FindFirstObjectByType<SuspectFolderUI>(FindObjectsInactive.Include);
                if (foundGO != null)
                {
                    suspectFolderPanel = foundGO.gameObject;
                }
                else if (panelType == UIPanelType.SuspectFolder)
                {
#if UNITY_EDITOR
                    var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Panels/Panel_SuspectFolder.prefab");
                    if (prefab != null)
                    {
                        suspectFolderPanel = Instantiate(prefab, transform);
                        suspectFolderPanel.name = "Panel_SuspectFolder";
                        suspectFolderPanel.SetActive(false);
                    }
#endif
                }
            }

            if (mainMenuPanel != null) mainMenuPanel.SetActive(isMainMenu);
            if (mainTablePanel != null) mainTablePanel.SetActive(!isMainMenu && (panelType == UIPanelType.InvestigationTable || isInGameMenu));
            if (inspectModalPanel != null) inspectModalPanel.SetActive(isInspect);
            if (notebookPanel != null) notebookPanel.SetActive(panelType == UIPanelType.CaseFileNotebook);
            if (suspectFolderPanel != null) suspectFolderPanel.SetActive(panelType == UIPanelType.SuspectFolder);
            if (deductionBoardPanel != null) deductionBoardPanel.SetActive(panelType == UIPanelType.DeductionBoard);
            if (conclusionQuizPanel != null) conclusionQuizPanel.SetActive(panelType == UIPanelType.ConclusionQuiz);
            if (resultsScreenPanel != null) resultsScreenPanel.SetActive(isResults);
            if (investigatorSelectPanel != null) investigatorSelectPanel.SetActive(false);
            if (inGameMenuPanel != null) inGameMenuPanel.SetActive(isInGameMenu);
            if (mainMenuConfirmPanel != null && !isInGameMenu) mainMenuConfirmPanel.SetActive(false);
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(isGameOver);
                if (isGameOver)
                {
                    gameOverPanel.transform.SetAsLastSibling();
                }
            }

            // Toggle in-game header navigation visibility (hidden during MainMenu, Evidence Inspection, Results, and GameOver)
            bool showHeaderNav = !isMainMenu && !isInspect && !isGameOver && !isResults && !isInGameMenu;
            if (timerContainer != null) timerContainer.SetActive(showHeaderNav);
            if (notebookButton != null) notebookButton.SetActive(showHeaderNav);
            if (suspectFolderButton != null) suspectFolderButton.SetActive(showHeaderNav);
            if (deductionBoardButton != null) deductionBoardButton.SetActive(showHeaderNav);
            if (concludeCaseButton != null) concludeCaseButton.SetActive(showHeaderNav);
            if (investigatorSelectButton != null) investigatorSelectButton.SetActive(false);
            if (returnToMenuButton != null) returnToMenuButton.SetActive(showHeaderNav);

            // Control countdown timer state across panels
            if (isMainMenu || isResults || isGameOver || isInGameMenu)
            {
                CaseManager.Instance?.PauseTimer();
            }
            else
            {
                CaseManager.Instance?.ResumeTimer();
            }

            ArmPointerController.Instance?.ForceSyncState();
            AudioManager.Instance?.PlayPaperFlip();
            UIButtonHighlightSystem.ApplyToHierarchy(gameObject);
        }

        /// <summary>
        /// Navigates back to the main menu screen.
        /// </summary>
        public void ReturnToMainMenu()
        {
            Debug.Log("[UI:Manager] Returning to Main Menu");
            string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (activeScene != "MainMenu" && activeScene != "Main" && Application.CanStreamedLevelBeLoaded("MainMenu"))
            {
                Debug.Log("[UI:Manager] Loading 'MainMenu' scene...");
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
            else
            {
                ShowPanel(UIPanelType.MainMenu);
                AudioManager.Instance?.PlayMenuBGM();
            }
        }

        /// <summary>Opens or closes the in-game menu without unloading the active case.</summary>
        public void ToggleInGameMenu()
        {
            if (currentPanel == UIPanelType.InGameMenu)
            {
                CloseInGameMenu();
                return;
            }

            _panelBeforeInGameMenu = currentPanel == UIPanelType.MainMenu
                ? UIPanelType.InvestigationTable
                : currentPanel;
            ShowPanel(UIPanelType.InGameMenu);
        }

        /// <summary>Returns from the in-game menu to the panel that was open before it.</summary>
        public void CloseInGameMenu()
        {
            if (currentPanel != UIPanelType.InGameMenu) return;
            ShowPanel(_panelBeforeInGameMenu);
        }

        /// <summary>Displays the confirmation prompt before leaving the active case.</summary>
        public void OpenMainMenuConfirmation()
        {
            if (currentPanel != UIPanelType.InGameMenu) return;
            if (mainMenuConfirmPanel != null)
            {
                mainMenuConfirmPanel.SetActive(true);
                mainMenuConfirmPanel.transform.SetAsLastSibling();
            }
        }

        /// <summary>Closes the leave-case confirmation and keeps the in-game menu open.</summary>
        public void CloseMainMenuConfirmation()
        {
            if (mainMenuConfirmPanel != null) mainMenuConfirmPanel.SetActive(false);
        }

        /// <summary>Leaves the active case after the player confirms the main-menu navigation.</summary>
        public void ConfirmReturnToMainMenu()
        {
            CloseMainMenuConfirmation();
            ReturnToMainMenu();
        }

        /// <summary>
        /// Compatibility callback for old investigator selection button bindings.
        /// </summary>
        public void ToggleInvestigatorSelectPanel()
        {
            ShowPanel(UIPanelType.InvestigationTable);
        }

        /// <summary>
        /// Toggles the case file notebook panel on and off.
        /// </summary>
        public void ToggleNotebookPanel()
        {
            Debug.Log("[UI:Manager] Toggle notebook panel clicked");
            if (currentPanel == UIPanelType.CaseFileNotebook)
                ShowPanel(UIPanelType.InvestigationTable);
            else
                ShowPanel(UIPanelType.CaseFileNotebook);
        }

        /// <summary>
        /// Toggles the suspect dossier folder panel on and off.
        /// </summary>
        public void ToggleSuspectFolderPanel()
        {
            Debug.Log("[UI:Manager] Toggle suspect folder panel clicked");
            if (currentPanel == UIPanelType.SuspectFolder)
                ShowPanel(UIPanelType.InvestigationTable);
            else
                ShowPanel(UIPanelType.SuspectFolder);
        }

        /// <summary>
        /// Toggles the deduction board panel on and off.
        /// </summary>
        public void ToggleDeductionBoardPanel()
        {
            Debug.Log("[UI:Manager] Toggle deduction board panel clicked");
            if (currentPanel == UIPanelType.DeductionBoard)
                ShowPanel(UIPanelType.InvestigationTable);
            else
                ShowPanel(UIPanelType.DeductionBoard);
        }

        /// <summary>
        /// Opens the final case conclusion quiz panel.
        /// </summary>
        public void OpenConclusionQuiz()
        {
            if (CaseManager.Instance == null || !CaseManager.Instance.IsReadyForConclusion())
            {
                bool evDone = CaseManager.Instance != null && CaseManager.Instance.AreAllEvidenceUnlocked();
                bool diagDone = CaseManager.Instance != null && CaseManager.Instance.AreAllDialoguesDone();
                Debug.LogWarning($"[UI:Manager] Conclusion locked: all dialogues must be done and all evidence must be unlocked first! (EvidenceUnlocked: {evDone}, DialoguesDone: {diagDone})");
                UpdateConclusionButtonState();
                return;
            }

            if (conclusionQuizPanel == null)
            {
                var found = Object.FindFirstObjectByType<ConclusionUI>(FindObjectsInactive.Include);
                if (found != null)
                {
                    conclusionQuizPanel = found.gameObject;
                }
                else
                {
#if UNITY_EDITOR
                    var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Panels/Panel_ConclusionQuiz.prefab");
                    if (prefab != null)
                    {
                        conclusionQuizPanel = Instantiate(prefab, transform);
                        conclusionQuizPanel.name = "Panel_ConclusionQuiz";
                    }
#endif
                }
            }

            Debug.Log("[UI:Manager] Open conclusion quiz button clicked — showing ConclusionQuiz panel");
            ShowPanel(UIPanelType.ConclusionQuiz);
        }
    }
}
