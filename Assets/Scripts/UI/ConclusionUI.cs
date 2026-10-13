using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    public class ConclusionUI : MonoBehaviour
    {
        [Header("Quiz Elements")]
        public GameObject quizContainer;
        public Text questionTitleText;
        public Transform optionsGrid;
        [Tooltip("Optional prefab for conclusion question header.")]
        [SerializeField] public GameObject questionHeaderPrefab;
        [Tooltip("Optional prefab for conclusion option clickable item.")]
        [SerializeField] public GameObject optionItemPrefab;
        public Button submitConclusionButton;

        [Header("Questionnaire Sprites")]
        [Tooltip("Background asset used for question title box and choice buttons.")]
        [SerializeField] private Sprite questionBoxSprite;

        [Tooltip("Button asset to start the question sequence.")]
        [SerializeField] private Sprite startButtonSprite;

        [Tooltip("Button asset to advance to the next question.")]
        [SerializeField] private Sprite nextButtonSprite;

        [Tooltip("Button asset displayed on the final question to confirm and conclude.")]
        [SerializeField] private Sprite confirmButtonSprite;

        [Tooltip("Asset for failed conclusion outcome background.")]
        [SerializeField] private Sprite failedBackgroundSprite;

        [Tooltip("Button asset for returning to main menu on the win screen.")]
        [SerializeField] private Sprite menuGameButtonSprite;

        [Tooltip("Button asset for advancing to next case on the win screen.")]
        [SerializeField] private Sprite nextGameButtonSprite;

        private static readonly Color HeaderDarkColor = DetectiveUITheme.Ink;
        private static readonly Color SubtitleDarkColor = DetectiveUITheme.MutedInk;
        private static readonly Color PaperColor = DetectiveUITheme.Paper;
        private static readonly Color ChoiceNormalColor = new Color(0.98f, 0.97f, 0.93f, 1f);
        private static readonly Color ChoiceBorderNormalColor = new Color(0.78f, 0.72f, 0.62f, 0.55f);
        private static readonly Color ChoiceSelectedColor = new Color(0.96f, 0.88f, 0.66f, 1f);
        private static readonly Color ChoiceBorderSelectedColor = DetectiveUITheme.WoodDark;
        private static readonly Color ChoiceTextNormalColor = HeaderDarkColor;
        private static readonly Color ChoiceTextSelectedColor = HeaderDarkColor;

        [Header("Results Screen Overlay")]
        public GameObject resultsContainer;
        public Text resultTitleText;
        public Text resultGradeText;
        public Text starRatingText;
        public Text scoreBreakdownText;
        public Button continueButton;
        public Text continueButtonText;
        public Button nextLevelButton;
        public Text nextLevelButtonText;
        public Button returnToMainMenuButton;

        [Header("Outcome Branding")]
        [Tooltip("Full-screen background image used when the case conclusion completes.")]
        [SerializeField] private Image resultBackgroundImage;

        [Tooltip("Case Closed / You Win artwork displayed for a successful conclusion.")]
        [SerializeField] private Sprite solvedBackgroundSprite;

        private readonly List<int> playerAnswers = new List<int>();
        private int _currentQuestionIndex = 0;
        private bool _hasStartedQuiz = false;
        private CaseSO _loadedCase = null;

        [SerializeField] private GameObject _flowRoot;
        [SerializeField] private GameObject _startScreenObj;
        [SerializeField] private GameObject _questionScreenObj;
        [SerializeField] private Image _questionBoxImage;
        [SerializeField] private Text _questionBoxText;
        [SerializeField] private Transform _choicesContainer;
        [SerializeField] private Text _hintPromptText;
        [SerializeField] private GameObject _prevButtonObj;
        [SerializeField] private Button _prevButton;
        [SerializeField] private GameObject _nextButtonObj;
        [SerializeField] private GameObject _confirmButtonObj;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button returnToInvestigationButton;
        [SerializeField] private ScrollRect _choicesScroll;
        [SerializeField] private Text startDescriptionText;
        [SerializeField] private Button startQuizButton;
        [SerializeField] private Button choiceButtonTemplate;
        private bool flowButtonsBound;
        private readonly List<Button> choiceButtons = new List<Button>();
        private readonly List<Image> _choiceImages = new List<Image>();
        private readonly List<Text> _choiceTexts = new List<Text>();
        private readonly List<Outline> _choiceOutlines = new List<Outline>();

        private void Start()
        {
            BindFlowButtons();

            if (submitConclusionButton != null) submitConclusionButton.onClick.AddListener(OnSubmitClicked);
            if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
            if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            if (returnToMainMenuButton != null) returnToMainMenuButton.onClick.AddListener(OnMainMenuClicked);

            if (resultsContainer != null) resultsContainer.SetActive(false);
        }

        private void OnEnable()
        {
            SetupQuiz();
        }

        private void SetupQuiz()
        {

            CaseSO activeCase = CaseManager.Instance?.ActiveCase;
            if (activeCase == null || activeCase.conclusionQuestions == null) return;

            Debug.Log($"[UI:Conclusion] Setting up conclusion quiz for '{activeCase.caseTitle}' with {activeCase.conclusionQuestions.Count} questions");

            if (quizContainer != null) quizContainer.SetActive(true);
            if (resultsContainer != null) resultsContainer.SetActive(false);
            if (submitConclusionButton != null) submitConclusionButton.gameObject.SetActive(false);
            if (questionTitleText != null) questionTitleText.gameObject.SetActive(false);
            if (optionsGrid != null && optionsGrid.parent != null && optionsGrid.parent.name.Contains("Scroll"))
            {
                optionsGrid.parent.gameObject.SetActive(false);
            }

            if (resultBackgroundImage != null)
            {
                resultBackgroundImage.sprite = null;
                resultBackgroundImage.color = DetectiveUITheme.Paper;
            }

            if (resultTitleText != null)
            {
                resultTitleText.gameObject.SetActive(false);
            }

            CaseManager manager = CaseManager.Instance;
            bool sessionWasReset = manager != null && manager.ConclusionAnswers.Count == 0 &&
                playerAnswers.Exists(answer => answer >= 0);
            if (_loadedCase != activeCase || sessionWasReset)
            {
                _loadedCase = activeCase;
                _hasStartedQuiz = false;
                _currentQuestionIndex = 0;
                playerAnswers.Clear();
            }

            int totalQ = activeCase.conclusionQuestions.Count;
            while (playerAnswers.Count < totalQ)
            {
                playerAnswers.Add(-1);
            }
            if (playerAnswers.Count > totalQ)
            {
                playerAnswers.RemoveRange(totalQ, playerAnswers.Count - totalQ);
            }

            if (manager != null)
            {
                for (int i = 0; i < totalQ; i++)
                {
                    playerAnswers[i] = manager.GetConclusionAnswer(i);
                }
            }

            bool hasAnySavedAnswer = false;
            for (int i = 0; i < playerAnswers.Count; i++)
            {
                if (playerAnswers[i] >= 0)
                {
                    hasAnySavedAnswer = true;
                    break;
                }
            }

            BindFlowButtons();
            UpdateStartContent(activeCase);
            if (_hasStartedQuiz || hasAnySavedAnswer)
            {
                _hasStartedQuiz = true;
                if (_startScreenObj != null) _startScreenObj.SetActive(false);
                if (_questionScreenObj != null) _questionScreenObj.SetActive(true);
                if (returnToInvestigationButton != null) returnToInvestigationButton.gameObject.SetActive(true);
                if (_currentQuestionIndex < 0 || _currentQuestionIndex >= totalQ) _currentQuestionIndex = 0;
                RenderCurrentQuestion();
            }
            else
            {
                _currentQuestionIndex = 0;
                ShowStartScreen();
            }
        }

        private void UpdateStartContent(CaseSO activeCase)
        {
            if (startDescriptionText == null) return;
            startDescriptionText.text = $"{activeCase.caseTitle}\n\nAnswer {playerAnswers.Count} questions using the evidence you gathered.\nYou can review and change answers before submitting.";
        }

        private void OnReturnToInvestigationClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (UIManager.Instance != null)
                UIManager.Instance.ShowPanel(UIPanelType.InvestigationTable);
        }

        private void BindFlowButtons()
        {
            if (flowButtonsBound) return;
            if (startQuizButton != null) startQuizButton.onClick.AddListener(OnStartQuizClicked);
            if (_prevButton != null) _prevButton.onClick.AddListener(OnPrevQuestionClicked);
            if (_nextButton != null) _nextButton.onClick.AddListener(OnNextQuestionClicked);
            if (_confirmButton != null) _confirmButton.onClick.AddListener(OnConfirmCaseClicked);
            if (returnToInvestigationButton != null)
                returnToInvestigationButton.onClick.AddListener(OnReturnToInvestigationClicked);
            flowButtonsBound = true;
        }

        private void ShowStartScreen()
        {
            if (_startScreenObj != null)
            {
                _startScreenObj.SetActive(true);
                Button start = _startScreenObj.transform.Find("StartButton")?.GetComponent<Button>();
                if (start != null)
                {
                    bool hasAnswers = false;
                    for (int i = 0; i < playerAnswers.Count; i++)
                    {
                        if (playerAnswers[i] >= 0) { hasAnswers = true; break; }
                    }
                    DetectiveUITheme.Action(start, hasAnswers ? "Resume final report" : "Begin final report", HeaderDarkColor);
                }
            }
            if (_questionScreenObj != null) _questionScreenObj.SetActive(false);
            if (_prevButtonObj != null) _prevButtonObj.SetActive(false);
            if (returnToInvestigationButton != null) returnToInvestigationButton.gameObject.SetActive(true);
        }

        private void OnStartQuizClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            _hasStartedQuiz = true;
            if (_startScreenObj != null) _startScreenObj.SetActive(false);
            if (_questionScreenObj != null) _questionScreenObj.SetActive(true);
            if (returnToInvestigationButton != null) returnToInvestigationButton.gameObject.SetActive(true);

            if (_currentQuestionIndex < 0 || _currentQuestionIndex >= playerAnswers.Count)
            {
                _currentQuestionIndex = 0;
            }
            RenderCurrentQuestion();
        }

        private void RenderCurrentQuestion()
        {
            CaseSO activeCase = CaseManager.Instance?.ActiveCase;
            if (activeCase == null || activeCase.conclusionQuestions == null || activeCase.conclusionQuestions.Count == 0) return;

            int totalQuestions = activeCase.conclusionQuestions.Count;
            if (_currentQuestionIndex < 0) _currentQuestionIndex = 0;
            if (_currentQuestionIndex >= totalQuestions) _currentQuestionIndex = totalQuestions - 1;

            ConclusionQuestion q = activeCase.conclusionQuestions[_currentQuestionIndex];

            if (_questionBoxText != null)
            {
                _questionBoxText.text = $"<size=14>FINAL REPORT   /   QUESTION {_currentQuestionIndex + 1} OF {totalQuestions}</size>\n\n{q.questionText}";
                _questionBoxText.color = HeaderDarkColor;
            }

            if (_hintPromptText != null)
            {
                if (playerAnswers[_currentQuestionIndex] >= 0)
                {
                    _hintPromptText.text = "Answer recorded. Continue or change your selection.";
                }
                else
                {
                    _hintPromptText.text = "Choose the answer supported by your evidence.";
                }
                _hintPromptText.color = SubtitleDarkColor;
            }

            if (_choicesContainer != null && choiceButtonTemplate != null)
            {
                while (choiceButtons.Count < q.options.Count)
                {
                    int choiceIndex = choiceButtons.Count;
                    Button button = Instantiate(choiceButtonTemplate, _choicesContainer, false);
                    button.name = $"Choice_{choiceIndex}";
                    button.onClick.AddListener(() => SelectChoice(choiceIndex));
                    choiceButtons.Add(button);
                    _choiceImages.Add(button.GetComponent<Image>());
                    _choiceTexts.Add(button.GetComponentInChildren<Text>(true));
                    _choiceOutlines.Add(button.GetComponent<Outline>());
                }

                float availableWidth = Mathf.Max(120f, ((RectTransform)_choicesContainer).rect.width - 80f);
                for (int i = 0; i < choiceButtons.Count; i++)
                {
                    bool isActive = i < q.options.Count;
                    choiceButtons[i].gameObject.SetActive(isActive);
                    if (!isActive) continue;

                    Text label = _choiceTexts[i];
                    LayoutElement layout = choiceButtons[i].GetComponent<LayoutElement>();
                    TextGenerationSettings settings = label.GetGenerationSettings(new Vector2(availableWidth, 0f));
                    settings.fontStyle = FontStyle.Bold;
                    string measuredText = $"[X]  {i + 1:00}   {q.options[i]}";
                    float textHeight = label.cachedTextGeneratorForLayout.GetPreferredHeight(measuredText, settings) / label.pixelsPerUnit;
                    layout.preferredHeight = Mathf.Max(64f, textHeight + 28f);
                }

                UpdateChoiceVisuals();
                if (_choicesScroll != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_choicesContainer);
                    _choicesScroll.StopMovement();
                    _choicesScroll.verticalNormalizedPosition = 1f;
                }
            }

            bool isLastQuestion = (_currentQuestionIndex == totalQuestions - 1);
            if (_nextButtonObj != null) _nextButtonObj.SetActive(!isLastQuestion);
            if (_confirmButtonObj != null) _confirmButtonObj.SetActive(isLastQuestion);
            if (_prevButtonObj != null) _prevButtonObj.SetActive(_currentQuestionIndex > 0);
        }

        private void OnPrevQuestionClicked()
        {
            if (_currentQuestionIndex <= 0) return;
            AudioManager.Instance?.PlayButtonClick();
            _currentQuestionIndex--;
            RenderCurrentQuestion();
        }

        private void SelectChoice(int choiceIndex)
        {
            AudioManager.Instance?.PlayButtonClick();
            playerAnswers[_currentQuestionIndex] = choiceIndex;
            CaseManager.Instance?.SetConclusionAnswer(_currentQuestionIndex, choiceIndex);
            if (_hintPromptText != null)
            {
                _hintPromptText.text = "Answer recorded. Continue when ready.";
                _hintPromptText.color = SubtitleDarkColor;
            }
            UpdateChoiceVisuals();
        }

        private void UpdateChoiceVisuals()
        {
            CaseSO activeCase = CaseManager.Instance?.ActiveCase;
            if (activeCase == null || _currentQuestionIndex >= activeCase.conclusionQuestions.Count) return;

            ConclusionQuestion q = activeCase.conclusionQuestions[_currentQuestionIndex];
            int selectedIndex = playerAnswers[_currentQuestionIndex];

            for (int i = 0; i < _choiceImages.Count; i++)
            {
                if (i >= q.options.Count) break;
                bool isSelected = (i == selectedIndex);

                if (_choiceImages[i] != null)
                {
                    _choiceImages[i].color = isSelected ? ChoiceSelectedColor : ChoiceNormalColor;
                }

                if (i < _choiceOutlines.Count && _choiceOutlines[i] != null)
                {
                    _choiceOutlines[i].effectColor = isSelected ? ChoiceBorderSelectedColor : ChoiceBorderNormalColor;
                    _choiceOutlines[i].effectDistance = isSelected ? new Vector2(2.5f, -2.5f) : new Vector2(1.5f, -1.5f);
                }

                if (_choiceTexts[i] != null)
                {
                    string marker = isSelected ? "[X]" : "[ ]";
                    _choiceTexts[i].text = $"{marker}  {i + 1:00}   {q.options[i]}";
                    _choiceTexts[i].fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal;
                    _choiceTexts[i].color = isSelected ? ChoiceTextSelectedColor : ChoiceTextNormalColor;
                }
            }
        }

        private void OnNextQuestionClicked()
        {
            if (playerAnswers[_currentQuestionIndex] < 0)
            {
                if (_hintPromptText != null)
                {
                    _hintPromptText.text = "Please select an answer to proceed.";
                    _hintPromptText.color = new Color(0.65f, 0.19f, 0.12f, 1f);
                }
                AudioManager.Instance?.PlaySFX(AudioManager.Instance?.caseFailedSFX);
                return;
            }

            AudioManager.Instance?.PlayButtonClick();
            _currentQuestionIndex++;
            RenderCurrentQuestion();
        }

        private void OnConfirmCaseClicked()
        {
            if (playerAnswers[_currentQuestionIndex] < 0)
            {
                if (_hintPromptText != null)
                {
                    _hintPromptText.text = "Please select an answer before confirming.";
                    _hintPromptText.color = new Color(0.65f, 0.19f, 0.12f, 1f);
                }
                AudioManager.Instance?.PlaySFX(AudioManager.Instance?.caseFailedSFX);
                return;
            }

            AudioManager.Instance?.PlayButtonClick();
            OnSubmitClicked();
        }

        private void OnSubmitClicked()
        {
            Debug.Log($"[UI:Conclusion] Submit conclusion button clicked. Answers count: {playerAnswers.Count}");
            if (CaseConclusionManager.Instance == null)
            {
                ShowSubmissionError("Conclusion unavailable. Return to investigation and try again.");
                return;
            }

            for (int i = 0; i < playerAnswers.Count; i++)
            {
                if (playerAnswers[i] < 0)
                {
                    Debug.LogWarning($"[UI:Conclusion] Cannot submit: question {i + 1} has not been answered.");
                    if (_hintPromptText != null)
                    {
                        _hintPromptText.text = $"Question {i + 1} has not been answered.";
                    }
                    return;
                }
            }

            CaseEvaluationResult result = CaseConclusionManager.Instance.EvaluateCase(playerAnswers);
            if (result == null)
            {
                string message = "Conclusion unavailable. Return to investigation and try again.";
                CaseManager caseManager = CaseManager.Instance;
                if (caseManager != null && caseManager.ActiveCase != null)
                {
                    bool evidenceComplete = caseManager.AreAllEvidenceUnlocked();
                    bool dialoguesComplete = caseManager.AreAllDialoguesDone();
                    if (!evidenceComplete && !dialoguesComplete)
                        message = "Find all evidence and finish all interrogations before submitting.";
                    else if (!evidenceComplete)
                        message = "Find all evidence before submitting your conclusion.";
                    else if (!dialoguesComplete)
                        message = "Finish all interrogations before submitting your conclusion.";
                }
                ShowSubmissionError(message);
                return;
            }
            if (UIManager.Instance != null)
                UIManager.Instance.ShowResults(result);
        }

        private void ShowSubmissionError(string message)
        {
            if (_hintPromptText != null)
            {
                _hintPromptText.text = message;
                _hintPromptText.color = DetectiveUITheme.StampRed;
            }
        }

        private void OnContinueClicked()
        {
            CaseSO activeCase = CaseManager.Instance?.ActiveCase;
            int currentLevel = activeCase != null ? activeCase.levelNumber : 1;
            Debug.Log($"[UI:Conclusion] Restarting failed Level {currentLevel} from the beginning");

            CaseClosed.Prototype.GameBootstrap bootstrap = Object.FindFirstObjectByType<CaseClosed.Prototype.GameBootstrap>();
            if (bootstrap != null)
            {
                bootstrap.LoadLevel(currentLevel);
                return;
            }

            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        private void OnNextLevelClicked()
        {
            CaseSO activeCase = CaseManager.Instance?.ActiveCase;
            int currentLevel = activeCase != null ? activeCase.levelNumber : 1;
            int nextLevel = currentLevel + 1;

            if (nextLevel <= 3)
            {
                string targetScene = $"Case00{nextLevel}";
                if (Application.CanStreamedLevelBeLoaded(targetScene))
                {
                    Debug.Log($"[UI:Conclusion] Loading scene '{targetScene}' via SceneManager...");
                    UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
                    return;
                }

                var bootstrap = Object.FindFirstObjectByType<CaseClosed.Prototype.GameBootstrap>();
                if (bootstrap != null)
                {
                    Debug.Log($"[UI:Conclusion] Advancing to Level {nextLevel} via bootstrap...");
                    bootstrap.LoadLevel(nextLevel);
                    return;
                }
            }

            Debug.Log("[UI:Conclusion] Reached final level or returning to Level Select");
            if (Application.CanStreamedLevelBeLoaded("LevelSelect"))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("LevelSelect");
                return;
            }
            OnMainMenuClicked();
        }

        private void OnMainMenuClicked()
        {
            Debug.Log("[UI:Conclusion] Return to Main Menu button clicked");
            if (Application.CanStreamedLevelBeLoaded("MainMenu"))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
            else
            {
                UIManager.Instance?.ReturnToMainMenu();
            }
        }
    }
}
