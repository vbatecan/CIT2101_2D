using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    /// <summary>
    /// UI View MonoBehaviour managing the conclusion quiz presentation, option selection,
    /// and final results scorecard rendering.
    /// Can be dragged directly onto the ConclusionPanel GameObject in the Unity Inspector.
    /// </summary>
    public class ConclusionUI : MonoBehaviour
    {
        [Header("Quiz Elements")]
        public GameObject quizContainer;
        public Text questionTitleText;
        public Transform optionsGrid;
        public Button submitConclusionButton;

        [Header("Results Screen Overlay")]
        public GameObject resultsContainer;
        public Text resultTitleText;
        public Text resultGradeText;
        public Text starRatingText;
        public Text scoreBreakdownText;
        public Button continueButton;
        public Button nextLevelButton;
        public Text nextLevelButtonText;
        public Button returnToMainMenuButton;

        private List<int> playerAnswers = new List<int>();

        /// <summary>
        /// Binds UI button click listeners on start.
        /// </summary>
        private void Start()
        {
            if (submitConclusionButton != null) submitConclusionButton.onClick.AddListener(OnSubmitClicked);
            if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
            if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            if (returnToMainMenuButton != null) returnToMainMenuButton.onClick.AddListener(OnMainMenuClicked);

            if (resultsContainer != null) resultsContainer.SetActive(false);
        }

        /// <summary>
        /// Rebuilds quiz options whenever the conclusion UI panel is enabled.
        /// </summary>
        private void OnEnable()
        {
            SetupQuiz();
        }

        /// <summary>
        /// Initializes the player answer list and dynamically renders quiz questions and selectable option items.
        /// </summary>
        private void SetupQuiz()
        {
            CaseSO activeCase = CaseManager.Instance?.activeCase;
            if (activeCase == null || activeCase.conclusionQuestions == null) return;

            Debug.Log($"[UI:Conclusion] Setting up conclusion quiz for '{activeCase.caseTitle}' with {activeCase.conclusionQuestions.Count} questions");

            if (quizContainer != null) quizContainer.SetActive(true);
            if (resultsContainer != null) resultsContainer.SetActive(false);

            playerAnswers.Clear();
            for (int i = 0; i < activeCase.conclusionQuestions.Count; i++)
            {
                playerAnswers.Add(0); // Default to first option
            }

            RenderQuestionOptions(activeCase);
        }

        /// <summary>
        /// Dynamically builds the UI hierarchy for question headers and clickable option choices.
        /// </summary>
        /// <param name="activeCase">The active case containing conclusion questions.</param>
        private void RenderQuestionOptions(CaseSO activeCase)
        {
            if (optionsGrid == null) return;

            foreach (Transform child in optionsGrid)
            {
                Destroy(child.gameObject);
            }

            for (int qIdx = 0; qIdx < activeCase.conclusionQuestions.Count; qIdx++)
            {
                var q = activeCase.conclusionQuestions[qIdx];

                GameObject headerObj = new GameObject($"Header_Q{qIdx}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                headerObj.transform.SetParent(optionsGrid, false);
                Text hText = headerObj.GetComponent<Text>();
                hText.text = $"\n{qIdx + 1}. {q.questionText}";
                hText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                hText.fontSize = 16;
                hText.fontStyle = FontStyle.Bold;
                hText.color = Color.yellow;

                int questionIndex = qIdx;
                for (int optIdx = 0; optIdx < q.options.Count; optIdx++)
                {
                    int optionIndex = optIdx;
                    GameObject optObj = new GameObject($"Opt_Q{qIdx}_O{optIdx}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Button));
                    optObj.transform.SetParent(optionsGrid, false);

                    Text optText = optObj.GetComponent<Text>();
                    optText.text = $"   [ ] {q.options[optIdx]}";
                    optText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    optText.fontSize = 14;
                    optText.color = Color.white;

                    optObj.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        Debug.Log($"[UI:Conclusion] Selected option {optionIndex} ('{q.options[optionIndex]}') for Question {questionIndex + 1} ('{q.questionText}')");
                        playerAnswers[questionIndex] = optionIndex;
                        optText.text = $"   [X] {q.options[optionIndex]}";
                        AudioManager.Instance?.PlayButtonClick();
                    });
                }
            }
        }

        /// <summary>
        /// Handles click on the submit button, triggering case evaluation in <see cref="CaseConclusionManager"/> and displaying results.
        /// </summary>
        private void OnSubmitClicked()
        {
            Debug.Log($"[UI:Conclusion] Submit conclusion button clicked. Answers count: {playerAnswers.Count}");
            if (CaseConclusionManager.Instance == null) return;

            CaseEvaluationResult result = CaseConclusionManager.Instance.EvaluateCase(playerAnswers);
            DisplayResultsCard(result);
        }

        /// <summary>
        /// Populates and displays the final evaluation results scorecard.
        /// </summary>
        /// <param name="result">The evaluation result data to display.</param>
        private void DisplayResultsCard(CaseEvaluationResult result)
        {
            if (result == null) return;

            Debug.Log($"[UI:Conclusion] Displaying results scorecard: Solved={result.isCaseSolved}, Score={result.totalScore}, Grade={result.rankGrade}, Stars={result.starCount}");

            if (quizContainer != null) quizContainer.SetActive(false);
            if (resultsContainer != null) resultsContainer.SetActive(true);

            CaseSO activeCase = CaseManager.Instance?.activeCase;
            CharacterProfileSO investigator = activeCase?.leadInvestigator ?? CaseManager.Instance?.selectedInvestigator;
            string investigatorName = investigator != null ? investigator.fullName : "Unknown Investigator";
            int currentLevel = activeCase != null ? activeCase.levelNumber : 1;

            if (resultTitleText != null)
            {
                resultTitleText.text = result.isCaseSolved ? $"LEVEL {currentLevel} SOLVED!" : $"LEVEL {currentLevel} FAILED";
                resultTitleText.color = result.isCaseSolved ? Color.green : Color.red;
            }

            if (resultGradeText != null) resultGradeText.text = $"GRADE: {result.rankGrade}";

            if (starRatingText != null)
            {
                string stars = "";
                for (int i = 0; i < 5; i++)
                {
                    stars += (i < result.starCount) ? "★ " : "☆ ";
                }
                starRatingText.text = stars;
            }

            if (scoreBreakdownText != null)
            {
                scoreBreakdownText.text =
                    $"Lead Investigator: {investigatorName}\n" +
                    $"Total Score: {result.totalScore} pts\n" +
                    $"Correct Quiz Answers: {result.correctQuizAnswers}/{result.totalQuizQuestions}\n" +
                    $"Evidence Discovered: {result.evidenceFoundCount}/{result.totalEvidenceCount}\n" +
                    $"Contradictions Caught: {result.contradictionsCaughtCount}/{result.totalContradictionsCount}\n" +
                    $"Time Taken: {Mathf.FloorToInt(result.completionTimeSeconds / 60)}m {Mathf.FloorToInt(result.completionTimeSeconds % 60)}s";
            }

            if (nextLevelButton != null)
            {
                int nextLevel = currentLevel + 1;
                if (nextLevel <= 3)
                {
                    nextLevelButton.gameObject.SetActive(true);
                    if (nextLevelButtonText != null)
                    {
                        nextLevelButtonText.text = $"Proceed to Level {nextLevel} >";
                    }
                }
                else
                {
                    nextLevelButton.gameObject.SetActive(true);
                    if (nextLevelButtonText != null)
                    {
                        nextLevelButtonText.text = "Replay / Level Select";
                    }
                }
            }
        }

        /// <summary>
        /// Handles click on continue button, returning to the investigation table via <see cref="UIManager"/>.
        /// </summary>
        private void OnContinueClicked()
        {
            Debug.Log("[UI:Conclusion] Continue button clicked, navigating back to InvestigationTable");
            UIManager.Instance?.ShowPanel(UIPanelType.InvestigationTable);
        }

        /// <summary>
        /// Handles click on next level button, progressing from Level 1 -> Level 2 -> Level 3 or opening selection.
        /// </summary>
        private void OnNextLevelClicked()
        {
            CaseSO activeCase = CaseManager.Instance?.activeCase;
            int currentLevel = activeCase != null ? activeCase.levelNumber : 1;
            int nextLevel = currentLevel + 1;

            var bootstrap = Object.FindFirstObjectByType<CaseClosed.Prototype.GameBootstrap>();
            if (nextLevel <= 3 && bootstrap != null)
            {
                Debug.Log($"[UI:Conclusion] Advancing to Level {nextLevel}...");
                bootstrap.LoadLevel(nextLevel);
            }
            else
            {
                Debug.Log("[UI:Conclusion] Reached final level or returning to Level Select");
                UIManager.Instance?.ReturnToMainMenu();
            }
        }

        /// <summary>
        /// Handles click on return to main menu button, navigating back to the main menu.
        /// </summary>
        private void OnMainMenuClicked()
        {
            Debug.Log("[UI:Conclusion] Return to Main Menu button clicked");
            UIManager.Instance?.ReturnToMainMenu();
        }
    }
}
