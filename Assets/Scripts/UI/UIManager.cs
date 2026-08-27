using UnityEngine;
using CaseClosed.Enums;
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
        public GameObject mainTablePanel;
        public GameObject inspectModalPanel;
        public GameObject notebookPanel;
        public GameObject deductionBoardPanel;
        public GameObject conclusionQuizPanel;
        public GameObject resultsScreenPanel;

        [Header("Header Navigation Buttons")]
        public GameObject notebookButton;
        public GameObject deductionBoardButton;
        public GameObject concludeCaseButton;

        private UIPanelType currentPanel = UIPanelType.InvestigationTable;

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
        /// Shows the default investigation table panel and hooks event listeners on start.
        /// </summary>
        private void Start()
        {
            ShowPanel(UIPanelType.InvestigationTable);
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
            currentPanel = panelType;

            if (mainTablePanel != null) mainTablePanel.SetActive(panelType == UIPanelType.InvestigationTable || panelType == UIPanelType.InspectModal);
            if (inspectModalPanel != null) inspectModalPanel.SetActive(panelType == UIPanelType.InspectModal);
            if (notebookPanel != null) notebookPanel.SetActive(panelType == UIPanelType.CaseFileNotebook);
            if (deductionBoardPanel != null) deductionBoardPanel.SetActive(panelType == UIPanelType.DeductionBoard);
            if (conclusionQuizPanel != null) conclusionQuizPanel.SetActive(panelType == UIPanelType.ConclusionQuiz);
            if (resultsScreenPanel != null) resultsScreenPanel.SetActive(panelType == UIPanelType.ResultsScreen);

            AudioManager.Instance?.PlayPaperFlip();
        }

        /// <summary>
        /// Toggles the case file notebook panel on and off.
        /// </summary>
        public void ToggleNotebookPanel()
        {
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
            ShowPanel(UIPanelType.ConclusionQuiz);
        }
    }
}
