using UnityEngine;
using UnityEngine.UI;
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
            if (CaseManager.Instance != null)
            {
                CaseManager.Instance.OnTimeExpired -= HandleTimeExpired;
            }

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

            if (investigatorSelectButton != null)
            {
                Button btn = investigatorSelectButton.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(ToggleInvestigatorSelectPanel);
            }

            if (notebookButton != null)
            {
                Button btn = notebookButton.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(ToggleNotebookPanel);
            }

            if (deductionBoardButton != null)
            {
                Button btn = deductionBoardButton.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(ToggleDeductionBoardPanel);
            }

            if (concludeCaseButton != null)
            {
                Button btn = concludeCaseButton.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(OpenConclusionQuiz);
            }

            UIPanelType initialPanel = (mainMenuPanel != null) ? UIPanelType.MainMenu : UIPanelType.InvestigationTable;
            ShowPanel(initialPanel);
            RegisterEvents();
            UpdateConclusionButtonState();
        }

        /// <summary>
        /// Registers event handlers for modal transitions and case expiration notifications.
        /// </summary>
        private void RegisterEvents()
        {
            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.OnInspectModalOpened += (ev) => ShowPanel(UIPanelType.InspectModal);
                EvidenceManager.Instance.OnInspectModalClosed += HandleInspectModalClosed;
            }
        }

        private void HandleInspectModalClosed()
        {
            ShowPanel(UIPanelType.InvestigationTable);
            UpdateConclusionButtonState();
        }

        private void UpdateConclusionButtonState()
        {
            if (concludeCaseButton == null) return;

            Button button = concludeCaseButton.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = CaseManager.Instance != null && CaseManager.Instance.IsReadyForConclusion();
            }

            if (CaseManager.Instance != null)
            {
                CaseManager.Instance.OnTimeExpired += HandleTimeExpired;
            }
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

            if (mainMenuPanel != null) mainMenuPanel.SetActive(isMainMenu);
            if (mainTablePanel != null) mainTablePanel.SetActive(!isMainMenu && (panelType == UIPanelType.InvestigationTable || isInGameMenu));
            if (inspectModalPanel != null) inspectModalPanel.SetActive(isInspect);
            if (notebookPanel != null) notebookPanel.SetActive(panelType == UIPanelType.CaseFileNotebook);
            if (deductionBoardPanel != null) deductionBoardPanel.SetActive(panelType == UIPanelType.DeductionBoard);
            if (conclusionQuizPanel != null) conclusionQuizPanel.SetActive(panelType == UIPanelType.ConclusionQuiz);
            if (resultsScreenPanel != null) resultsScreenPanel.SetActive(isResults);
            if (investigatorSelectPanel != null) investigatorSelectPanel.SetActive(panelType == UIPanelType.InvestigatorSelect);
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
            if (deductionBoardButton != null) deductionBoardButton.SetActive(showHeaderNav);
            if (concludeCaseButton != null) concludeCaseButton.SetActive(showHeaderNav);
            if (investigatorSelectButton != null) investigatorSelectButton.SetActive(showHeaderNav);
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
        /// Toggles the investigator selection panel on and off.
        /// </summary>
        public void ToggleInvestigatorSelectPanel()
        {
            Debug.Log("[UI:Manager] Toggle investigator select panel clicked");
            if (currentPanel == UIPanelType.InvestigatorSelect)
                ShowPanel(UIPanelType.InvestigationTable);
            else
                ShowPanel(UIPanelType.InvestigatorSelect);
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
                Debug.LogWarning("[UI:Manager] Conclusion locked: examine all evidence and expose every contradiction first.");
                UpdateConclusionButtonState();
                return;
            }

            Debug.Log("[UI:Manager] Open conclusion quiz button clicked");
            ShowPanel(UIPanelType.ConclusionQuiz);
        }
    }
}
