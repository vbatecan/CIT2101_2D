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

        [Header("Header Navigation Buttons")]
        public GameObject notebookButton;
        public GameObject deductionBoardButton;
        public GameObject concludeCaseButton;
        public GameObject investigatorSelectButton;
        public GameObject returnToMenuButton;

        private UIPanelType _currentPanel = UIPanelType.MainMenu;

        /// <summary>The currently active UI panel type.</summary>
        public UIPanelType currentPanel => _currentPanel;

        /// <summary>
        /// Initializes the singleton instance.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
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
                if (btn != null) btn.onClick.AddListener(ReturnToMainMenu);
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
        }

        /// <summary>
        /// Registers event handlers for evidence inspect modal open and close notifications.
        /// </summary>
        private void RegisterEvents()
        {
            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.OnInspectModalOpened += (ev) => ShowPanel(UIPanelType.InspectModal);
                EvidenceManager.Instance.OnInspectModalClosed += () => ShowPanel(UIPanelType.InvestigationTable);
            }
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

            if (mainMenuPanel != null) mainMenuPanel.SetActive(isMainMenu);
            if (mainTablePanel != null) mainTablePanel.SetActive(!isMainMenu && panelType == UIPanelType.InvestigationTable);
            if (inspectModalPanel != null) inspectModalPanel.SetActive(isInspect);
            if (notebookPanel != null) notebookPanel.SetActive(panelType == UIPanelType.CaseFileNotebook);
            if (deductionBoardPanel != null) deductionBoardPanel.SetActive(panelType == UIPanelType.DeductionBoard);
            if (conclusionQuizPanel != null) conclusionQuizPanel.SetActive(panelType == UIPanelType.ConclusionQuiz);
            if (resultsScreenPanel != null) resultsScreenPanel.SetActive(panelType == UIPanelType.ResultsScreen);
            if (investigatorSelectPanel != null) investigatorSelectPanel.SetActive(panelType == UIPanelType.InvestigatorSelect);

            // Toggle in-game header navigation visibility (hidden during MainMenu and during isolated Evidence Inspection)
            bool showHeaderNav = !isMainMenu && !isInspect;
            if (notebookButton != null) notebookButton.SetActive(showHeaderNav);
            if (deductionBoardButton != null) deductionBoardButton.SetActive(showHeaderNav);
            if (concludeCaseButton != null) concludeCaseButton.SetActive(showHeaderNav);
            if (investigatorSelectButton != null) investigatorSelectButton.SetActive(showHeaderNav);
            if (returnToMenuButton != null) returnToMenuButton.SetActive(showHeaderNav);

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
            Debug.Log("[UI:Manager] Open conclusion quiz button clicked");
            ShowPanel(UIPanelType.ConclusionQuiz);
        }
    }
}
