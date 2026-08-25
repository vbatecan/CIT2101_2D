using UnityEngine;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    public enum UIPanelType
    {
        InvestigationTable,
        InspectModal,
        CaseFileNotebook,
        DeductionBoard,
        ConclusionQuiz,
        ResultsScreen
    }

    public class UIManager : MonoBehaviour
    {
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

        private void Start()
        {
            ShowPanel(UIPanelType.InvestigationTable);
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.OnInspectModalOpened += (ev) => ShowPanel(UIPanelType.InspectModal);
                EvidenceManager.Instance.OnInspectModalClosed += () => ShowPanel(UIPanelType.InvestigationTable);
            }
        }

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

        public void ToggleNotebookPanel()
        {
            if (currentPanel == UIPanelType.CaseFileNotebook)
                ShowPanel(UIPanelType.InvestigationTable);
            else
                ShowPanel(UIPanelType.CaseFileNotebook);
        }

        public void ToggleDeductionBoardPanel()
        {
            if (currentPanel == UIPanelType.DeductionBoard)
                ShowPanel(UIPanelType.InvestigationTable);
            else
                ShowPanel(UIPanelType.DeductionBoard);
        }

        public void OpenConclusionQuiz()
        {
            ShowPanel(UIPanelType.ConclusionQuiz);
        }
    }
}
