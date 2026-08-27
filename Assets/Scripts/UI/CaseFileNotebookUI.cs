using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;
using CaseClosed.Services;

namespace CaseClosed.UI
{
    /// <summary>
    /// UI View MonoBehaviour managing the detective's case file notebook tabs and display rendering,
    /// delegating text compilation and formatting to <see cref="NotebookFormattingService"/>.
    /// Can be dragged directly onto the NotebookPanel GameObject in the Unity Inspector.
    /// </summary>
    public class CaseFileNotebookUI : MonoBehaviour
    {
        [Header("Tab Buttons")]
        public Button summaryTabButton;
        public Button suspectsTabButton;
        public Button evidenceTabButton;
        public Button cluesTabButton;
        public Button closeNotebookButton;

        [Header("Content Container Text")]
        public Text notebookTitleText;
        public Text notebookContentBody;

        private NotebookTab currentTab = NotebookTab.CaseSummary;
        private readonly NotebookFormattingService formattingService = new NotebookFormattingService();

        /// <summary>
        /// Binds UI button click listeners for tab switching and close actions.
        /// </summary>
        private void Start()
        {
            if (summaryTabButton != null) summaryTabButton.onClick.AddListener(() => SwitchTab(NotebookTab.CaseSummary));
            if (suspectsTabButton != null) suspectsTabButton.onClick.AddListener(() => SwitchTab(NotebookTab.Suspects));
            if (evidenceTabButton != null) evidenceTabButton.onClick.AddListener(() => SwitchTab(NotebookTab.Evidence));
            if (cluesTabButton != null) cluesTabButton.onClick.AddListener(() => SwitchTab(NotebookTab.Clues));
            if (closeNotebookButton != null) closeNotebookButton.onClick.AddListener(OnCloseClicked);
        }

        /// <summary>
        /// Refreshes the active tab content when the notebook panel is enabled.
        /// </summary>
        private void OnEnable()
        {
            SwitchTab(currentTab);
        }

        /// <summary>
        /// Switches the active notebook tab, formats the corresponding data using <see cref="NotebookFormattingService"/>,
        /// and updates the text display while playing an audio effect.
        /// </summary>
        /// <param name="tab">The target <see cref="NotebookTab"/> to display.</param>
        public void SwitchTab(NotebookTab tab)
        {
            currentTab = tab;
            CaseSO activeCase = CaseManager.Instance?.activeCase;
            if (activeCase == null) return;

            Debug.Log($"[UI:Notebook] Switched to tab '{tab}' for case '{activeCase.caseTitle}'");

            string contentText = string.Empty;

            switch (tab)
            {
                case NotebookTab.CaseSummary:
                    if (notebookTitleText != null) notebookTitleText.text = activeCase.caseTitle;
                    contentText = formattingService.FormatCaseSummary(activeCase);
                    break;

                case NotebookTab.Suspects:
                    if (notebookTitleText != null) notebookTitleText.text = "Suspect Profiles";
                    contentText = formattingService.FormatSuspectProfiles(activeCase);
                    break;

                case NotebookTab.Evidence:
                    if (notebookTitleText != null) notebookTitleText.text = "Discovered Evidence";
                    var discovered = CaseManager.Instance?.discoveredEvidenceIds;
                    contentText = formattingService.FormatDiscoveredEvidence(activeCase, discovered);
                    break;

                case NotebookTab.Clues:
                    if (notebookTitleText != null) notebookTitleText.text = "Unlocked Clues & Deductions";
                    var cluesDict = CaseManager.Instance?.unlockedCluesText;
                    contentText = formattingService.FormatUnlockedClues(cluesDict);
                    break;
            }

            if (notebookContentBody != null) notebookContentBody.text = contentText;
            AudioManager.Instance?.PlayPaperFlip();
        }

        /// <summary>
        /// Handles click on the close notebook button, closing the panel via <see cref="UIManager"/>.
        /// </summary>
        private void OnCloseClicked()
        {
            Debug.Log("[UI:Notebook] Close notebook button clicked");
            UIManager.Instance?.ToggleNotebookPanel();
        }
    }
}
