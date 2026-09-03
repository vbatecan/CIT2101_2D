using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    /// <summary>
    /// UI View MonoBehaviour managing the Game Over overlay presented when investigation time expires.
    /// Renders dramatic failure branding, investigation metrics (time lapsed, evidence found, contradictions exposed),
    /// and binds 'Retry Case' and 'Return to Main Menu' action buttons.
    /// Can be dragged directly onto the GameOverPanel GameObject in the Unity Inspector.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("Text Displays")]
        [Tooltip("Header text displaying 'TIME EXPIRED - CASE FAILED'.")]
        [SerializeField] private Text titleText;

        [Tooltip("Descriptive subtitle explaining the investigation failure.")]
        [SerializeField] private Text subtitleText;

        [Tooltip("Detailed metrics breakdown text (time lapsed, clues found, etc.).")]
        [SerializeField] private Text detailsBreakdownText;

        [Header("Action Buttons")]
        [Tooltip("Button to restart the active case from the beginning.")]
        [SerializeField] private Button retryButton;

        [Tooltip("Button to exit back to the main menu screen.")]
        [SerializeField] private Button returnToMainMenuButton;

        private void Awake()
        {
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(OnRetryClicked);
            }

            if (returnToMainMenuButton != null)
            {
                returnToMainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }

        private void OnEnable()
        {
            PopulateGameOverDetails();

            if (CaseManager.Instance != null)
            {
                CaseManager.Instance.OnTimeExpired += HandleTimeExpired;
            }
        }

        private void OnDisable()
        {
            if (CaseManager.Instance != null)
            {
                CaseManager.Instance.OnTimeExpired -= HandleTimeExpired;
            }
        }

        private void HandleTimeExpired()
        {
            PopulateGameOverDetails();
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowPanel(UIPanelType.GameOver);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Gathers statistics from CaseManager and formats the failure details card.
        /// </summary>
        public void PopulateGameOverDetails()
        {
            CaseSO activeCase = CaseManager.Instance?.activeCase;
            CharacterProfileSO investigator = activeCase?.leadInvestigator ?? CaseManager.Instance?.selectedInvestigator;
            string investigatorName = investigator != null ? investigator.fullName : "Unknown Detective";

            int levelNumber = activeCase != null ? activeCase.levelNumber : 1;
            string caseTitle = activeCase != null ? activeCase.caseTitle : "Unknown Case";

            int evFound = CaseManager.Instance != null ? CaseManager.Instance.discoveredEvidenceIds.Count : 0;
            int totalEv = activeCase != null ? (activeCase.totalKeyEvidenceCount > 0 ? activeCase.totalKeyEvidenceCount : activeCase.evidenceItems.Count) : 0;

            int contraFound = CaseManager.Instance != null ? CaseManager.Instance.exposedContradictionIds.Count : 0;
            int totalContra = activeCase != null ? (activeCase.totalContradictionsCount > 0 ? activeCase.totalContradictionsCount : activeCase.contradictionRules.Count) : 0;

            float timeLapsed = CaseManager.Instance != null ? CaseManager.Instance.ElapsedTime : 0f;
            string formattedTime = CaseManager.Instance?.TimerService != null
                ? CaseManager.Instance.TimerService.FormatTimeVerbose(timeLapsed)
                : $"{Mathf.FloorToInt(timeLapsed / 60)}m {Mathf.FloorToInt(timeLapsed % 60)}s";

            if (titleText != null)
            {
                titleText.text = $"LEVEL {levelNumber}: TIME EXPIRED";
                titleText.color = new Color(1f, 0.25f, 0.25f, 1f);
            }

            if (subtitleText != null)
            {
                subtitleText.text = $"Investigation Failed — The suspect slipped away before the case was closed.";
            }

            if (detailsBreakdownText != null)
            {
                detailsBreakdownText.text =
                    $"Case File: {caseTitle}\n" +
                    $"Lead Investigator: {investigatorName}\n" +
                    $"Time Lapsed: {formattedTime}\n" +
                    $"Evidence Discovered: {evFound} / {totalEv}\n" +
                    $"Contradictions Exposed: {contraFound} / {totalContra}\n\n" +
                    $"Status: UNRESOLVED";
            }
        }

        private void OnRetryClicked()
        {
            Debug.Log("[GameOverUI] Retry button clicked. Restarting investigation...");
            AudioManager.Instance?.PlayButtonClick();

            if (CaseManager.Instance != null)
            {
                CaseManager.Instance.RetryCurrentCase();
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowPanel(UIPanelType.InvestigationTable);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnMainMenuClicked()
        {
            Debug.Log("[GameOverUI] Return to Main Menu button clicked.");
            AudioManager.Instance?.PlayButtonClick();

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ReturnToMainMenu();
            }
        }
    }
}
