using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
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

        private List<int> playerAnswers = new List<int>();

        private void Start()
        {
            if (submitConclusionButton != null) submitConclusionButton.onClick.AddListener(OnSubmitClicked);
            if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);

            if (resultsContainer != null) resultsContainer.SetActive(false);
        }

        private void OnEnable()
        {
            SetupQuiz();
        }

        private void SetupQuiz()
        {
            CaseSO activeCase = CaseManager.Instance?.activeCase;
            if (activeCase == null || activeCase.conclusionQuestions == null) return;

            if (quizContainer != null) quizContainer.SetActive(true);
            if (resultsContainer != null) resultsContainer.SetActive(false);

            playerAnswers.Clear();
            for (int i = 0; i < activeCase.conclusionQuestions.Count; i++)
            {
                playerAnswers.Add(0); // Default to first option
            }

            RenderQuestionOptions(activeCase);
        }

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
                        playerAnswers[questionIndex] = optionIndex;
                        optText.text = $"   [X] {q.options[optionIndex]}";
                        AudioManager.Instance?.PlayButtonClick();
                    });
                }
            }
        }

        private void OnSubmitClicked()
        {
            if (CaseConclusionManager.Instance == null) return;

            CaseEvaluationResult result = CaseConclusionManager.Instance.EvaluateCase(playerAnswers);
            DisplayResultsCard(result);
        }

        private void DisplayResultsCard(CaseEvaluationResult result)
        {
            if (result == null) return;

            if (quizContainer != null) quizContainer.SetActive(false);
            if (resultsContainer != null) resultsContainer.SetActive(true);

            if (resultTitleText != null)
            {
                resultTitleText.text = result.isCaseSolved ? "CASE SOLVED!" : "INVESTIGATION FAILED";
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
                    $"Total Score: {result.totalScore} pts\n" +
                    $"Correct Quiz Answers: {result.correctQuizAnswers}/{result.totalQuizQuestions}\n" +
                    $"Evidence Discovered: {result.evidenceFoundCount}/{result.totalEvidenceCount}\n" +
                    $"Contradictions Caught: {result.contradictionsCaughtCount}/{result.totalContradictionsCount}\n" +
                    $"Time Taken: {Mathf.FloorToInt(result.completionTimeSeconds / 60)}m {Mathf.FloorToInt(result.completionTimeSeconds % 60)}s";
            }
        }

        private void OnContinueClicked()
        {
            UIManager.Instance?.ShowPanel(UIPanelType.InvestigationTable);
        }
    }
}
