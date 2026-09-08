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

        private static readonly Color HeaderGoldColor = new Color(0.95f, 0.82f, 0.45f, 1f);
        private static readonly Color ShadowBlackColor = new Color(0f, 0f, 0f, 0.85f);
        private static readonly Color ChoiceNormalColor = new Color(0.85f, 0.85f, 0.85f, 0.95f);
        private static readonly Color ChoiceSelectedColor = new Color(1f, 0.92f, 0.5f, 1f);

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

        // Runtime UI containers & components
        private GameObject _flowRoot;
        private GameObject _startScreenObj;
        private GameObject _questionScreenObj;
        private Image _questionBoxImage;
        private Text _questionBoxText;
        private Transform _choicesContainer;
        private Text _hintPromptText;
        private GameObject _nextButtonObj;
        private GameObject _confirmButtonObj;
        private Button _nextButton;
        private Button _confirmButton;
        private readonly List<Image> _choiceImages = new List<Image>();
        private readonly List<Text> _choiceTexts = new List<Text>();

        private void Awake()
        {
            EnsureAssets();
        }

        /// <summary>
        /// Binds UI button click listeners on start.
        /// </summary>
        private void Start()
        {
            EnsureAssets();

            if (submitConclusionButton != null) submitConclusionButton.onClick.AddListener(OnSubmitClicked);
            if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
            if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            if (returnToMainMenuButton != null) returnToMainMenuButton.onClick.AddListener(OnMainMenuClicked);

            if (resultsContainer != null) resultsContainer.SetActive(false);
            UIButtonHighlightSystem.ApplyToHierarchy(gameObject);
        }

        /// <summary>
        /// Ensures questionnaire and outcome sprites are loaded from assets when not assigned in Inspector.
        /// </summary>
        private void EnsureAssets()
        {
#if UNITY_EDITOR
            if (questionBoxSprite == null)
                questionBoxSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Assets/BUTTONS/QUESTION_BOX.png");
            if (startButtonSprite == null)
                startButtonSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Assets/BUTTONS/QUESTION_START_BUTTON.png");
            if (nextButtonSprite == null)
                nextButtonSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Assets/BUTTONS/QUESTION_NEXT_BUTTON.png");
            if (confirmButtonSprite == null)
                confirmButtonSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Assets/BUTTONS/QUESTION_CONFIRM_BUTTON.png");
            if (failedBackgroundSprite == null)
                failedBackgroundSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Assets/BACKGROUNDS/case1FAILED.png");
            if (solvedBackgroundSprite == null)
                solvedBackgroundSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Assets/BACKGROUNDS/CasesWIN.png");
#endif
        }

        /// <summary>
        /// Rebuilds quiz options whenever the conclusion UI panel is enabled.
        /// </summary>
        private void OnEnable()
        {
            SetupQuiz();
        }

        /// <summary>
        /// Initializes the player answer list and displays the initial Start screen.
        /// </summary>
        private void SetupQuiz()
        {
            EnsureAssets();

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
                resultBackgroundImage.color = new Color(0.06f, 0.07f, 0.09f, 0.98f);
            }

            if (resultTitleText != null)
            {
                resultTitleText.gameObject.SetActive(true);
            }

            playerAnswers.Clear();
            for (int i = 0; i < activeCase.conclusionQuestions.Count; i++)
            {
                playerAnswers.Add(-1);
            }

            _currentQuestionIndex = 0;
            EnsureFlowHierarchy();
            ShowStartScreen();
        }

        /// <summary>
        /// Builds the flow hierarchy for the Start screen, question box, choices, and navigation buttons.
        /// </summary>
        private void EnsureFlowHierarchy()
        {
            Transform parentTransform = (quizContainer != null) ? quizContainer.transform : transform;

            if (_flowRoot == null)
            {
                Transform existing = parentTransform.Find("QuizFlowRoot");
                if (existing != null)
                {
                    _flowRoot = existing.gameObject;
                }
                else
                {
                    _flowRoot = new GameObject("QuizFlowRoot", typeof(RectTransform));
                    _flowRoot.transform.SetParent(parentTransform, false);
                    RectTransform rt = _flowRoot.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.anchoredPosition = Vector2.zero;
                    rt.sizeDelta = Vector2.zero;
                }
            }

            Font standardFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (standardFont == null && questionTitleText != null)
            {
                standardFont = questionTitleText.font;
            }

            // 1. Build Start Screen if missing
            if (_startScreenObj == null)
            {
                Transform existingStart = _flowRoot.transform.Find("StartScreen");
                if (existingStart != null)
                {
                    _startScreenObj = existingStart.gameObject;
                }
                else
                {
                    _startScreenObj = new GameObject("StartScreen", typeof(RectTransform));
                    _startScreenObj.transform.SetParent(_flowRoot.transform, false);
                    RectTransform srt = _startScreenObj.GetComponent<RectTransform>();
                    srt.anchorMin = Vector2.zero;
                    srt.anchorMax = Vector2.one;
                    srt.anchoredPosition = Vector2.zero;
                    srt.sizeDelta = Vector2.zero;

                    // Title
                    GameObject titleObj = new GameObject("StartTitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                    titleObj.transform.SetParent(_startScreenObj.transform, false);
                    RectTransform trt = titleObj.GetComponent<RectTransform>();
                    trt.anchoredPosition = new Vector2(0f, 90f);
                    trt.sizeDelta = new Vector2(700f, 60f);
                    Text titleTxt = titleObj.GetComponent<Text>();
                    titleTxt.font = standardFont;
                    titleTxt.fontSize = 28;
                    titleTxt.fontStyle = FontStyle.Bold;
                    titleTxt.alignment = TextAnchor.MiddleCenter;
                    titleTxt.color = HeaderGoldColor;
                    titleTxt.text = "CASE CONCLUSION";
                    Shadow tShadow = titleObj.AddComponent<Shadow>();
                    tShadow.effectDistance = new Vector2(1.5f, -1.5f);
                    tShadow.effectColor = ShadowBlackColor;

                    // Subtitle / Prompt
                    GameObject descObj = new GameObject("StartDesc", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                    descObj.transform.SetParent(_startScreenObj.transform, false);
                    RectTransform drt = descObj.GetComponent<RectTransform>();
                    drt.anchoredPosition = new Vector2(0f, 25f);
                    drt.sizeDelta = new Vector2(650f, 60f);
                    Text descTxt = descObj.GetComponent<Text>();
                    descTxt.font = standardFont;
                    descTxt.fontSize = 17;
                    descTxt.alignment = TextAnchor.MiddleCenter;
                    descTxt.color = Color.white;
                    descTxt.text = "Answer all 5 questions based on your investigation to solve the case.\nClick Start to begin.";
                    Shadow dShadow = descObj.AddComponent<Shadow>();
                    dShadow.effectDistance = new Vector2(1.2f, -1.2f);
                    dShadow.effectColor = ShadowBlackColor;

                    // Start Button
                    GameObject btnObj = new GameObject("StartButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                    btnObj.transform.SetParent(_startScreenObj.transform, false);
                    RectTransform brt = btnObj.GetComponent<RectTransform>();
                    brt.anchoredPosition = new Vector2(0f, -65f);
                    brt.sizeDelta = new Vector2(180f, 55f);
                    Image btnImg = btnObj.GetComponent<Image>();
                    btnImg.sprite = startButtonSprite;
                    btnImg.preserveAspect = true;
                    Button btn = btnObj.GetComponent<Button>();
                    btn.onClick.AddListener(OnStartQuizClicked);
                }
            }

            // 2. Build Question Screen if missing
            if (_questionScreenObj == null)
            {
                Transform existingQ = _flowRoot.transform.Find("QuestionScreen");
                if (existingQ != null)
                {
                    _questionScreenObj = existingQ.gameObject;
                }
                else
                {
                    _questionScreenObj = new GameObject("QuestionScreen", typeof(RectTransform));
                    _questionScreenObj.transform.SetParent(_flowRoot.transform, false);
                    RectTransform qrt = _questionScreenObj.GetComponent<RectTransform>();
                    qrt.anchorMin = Vector2.zero;
                    qrt.anchorMax = Vector2.one;
                    qrt.anchoredPosition = Vector2.zero;
                    qrt.sizeDelta = Vector2.zero;

                    // Question Box
                    GameObject qBoxObj = new GameObject("QuestionBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    qBoxObj.transform.SetParent(_questionScreenObj.transform, false);
                    RectTransform qbrt = qBoxObj.GetComponent<RectTransform>();
                    qbrt.anchoredPosition = new Vector2(0f, 120f);
                    qbrt.sizeDelta = new Vector2(720f, 110f);
                    _questionBoxImage = qBoxObj.GetComponent<Image>();
                    _questionBoxImage.sprite = questionBoxSprite;
                    _questionBoxImage.type = Image.Type.Sliced;

                    GameObject qTextObj = new GameObject("QuestionText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                    qTextObj.transform.SetParent(qBoxObj.transform, false);
                    RectTransform qtrt = qTextObj.GetComponent<RectTransform>();
                    qtrt.anchorMin = Vector2.zero;
                    qtrt.anchorMax = Vector2.one;
                    qtrt.sizeDelta = new Vector2(-40f, -20f);
                    qtrt.anchoredPosition = Vector2.zero;
                    _questionBoxText = qTextObj.GetComponent<Text>();
                    _questionBoxText.font = standardFont;
                    _questionBoxText.fontSize = 17;
                    _questionBoxText.fontStyle = FontStyle.Bold;
                    _questionBoxText.alignment = TextAnchor.MiddleCenter;
                    _questionBoxText.color = Color.white;
                    Shadow qShadow = qTextObj.AddComponent<Shadow>();
                    qShadow.effectDistance = new Vector2(1.2f, -1.2f);
                    qShadow.effectColor = ShadowBlackColor;

                    // Choices Container
                    GameObject choicesObj = new GameObject("ChoicesContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
                    choicesObj.transform.SetParent(_questionScreenObj.transform, false);
                    RectTransform crt = choicesObj.GetComponent<RectTransform>();
                    crt.anchoredPosition = new Vector2(0f, -25f);
                    crt.sizeDelta = new Vector2(680f, 175f);
                    VerticalLayoutGroup vlg = choicesObj.GetComponent<VerticalLayoutGroup>();
                    vlg.spacing = 10f;
                    vlg.childAlignment = TextAnchor.UpperCenter;
                    vlg.childControlWidth = true;
                    vlg.childControlHeight = false;
                    vlg.childForceExpandWidth = true;
                    vlg.childForceExpandHeight = false;
                    _choicesContainer = choicesObj.transform;

                    // Hint / Prompt Text
                    GameObject hintObj = new GameObject("HintText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                    hintObj.transform.SetParent(_questionScreenObj.transform, false);
                    RectTransform hrt = hintObj.GetComponent<RectTransform>();
                    hrt.anchoredPosition = new Vector2(0f, -130f);
                    hrt.sizeDelta = new Vector2(600f, 30f);
                    _hintPromptText = hintObj.GetComponent<Text>();
                    _hintPromptText.font = standardFont;
                    _hintPromptText.fontSize = 15;
                    _hintPromptText.alignment = TextAnchor.MiddleCenter;
                    _hintPromptText.color = new Color(1f, 0.78f, 0.25f, 1f);
                    _hintPromptText.text = "";

                    // Navigation Footer
                    // Next Button (Arrow)
                    _nextButtonObj = new GameObject("NextButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                    _nextButtonObj.transform.SetParent(_questionScreenObj.transform, false);
                    RectTransform nrt = _nextButtonObj.GetComponent<RectTransform>();
                    nrt.anchoredPosition = new Vector2(0f, -180f);
                    nrt.sizeDelta = new Vector2(120f, 48f);
                    Image nImg = _nextButtonObj.GetComponent<Image>();
                    nImg.sprite = nextButtonSprite;
                    nImg.preserveAspect = true;
                    _nextButton = _nextButtonObj.GetComponent<Button>();
                    _nextButton.onClick.AddListener(OnNextQuestionClicked);

                    // Confirm Button (Confirm)
                    _confirmButtonObj = new GameObject("ConfirmButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                    _confirmButtonObj.transform.SetParent(_questionScreenObj.transform, false);
                    RectTransform cbrt = _confirmButtonObj.GetComponent<RectTransform>();
                    cbrt.anchoredPosition = new Vector2(0f, -180f);
                    cbrt.sizeDelta = new Vector2(160f, 50f);
                    Image cImg = _confirmButtonObj.GetComponent<Image>();
                    cImg.sprite = confirmButtonSprite;
                    cImg.preserveAspect = true;
                    _confirmButton = _confirmButtonObj.GetComponent<Button>();
                    _confirmButton.onClick.AddListener(OnConfirmCaseClicked);
                }
            }

            if (_questionBoxImage == null && _questionScreenObj != null)
            {
                _questionBoxImage = _questionScreenObj.transform.Find("QuestionBox")?.GetComponent<Image>();
                _questionBoxText = _questionScreenObj.transform.Find("QuestionBox/QuestionText")?.GetComponent<Text>();
                _choicesContainer = _questionScreenObj.transform.Find("ChoicesContainer");
                _hintPromptText = _questionScreenObj.transform.Find("HintText")?.GetComponent<Text>();
                _nextButtonObj = _questionScreenObj.transform.Find("NextButton")?.gameObject;
                _confirmButtonObj = _questionScreenObj.transform.Find("ConfirmButton")?.gameObject;
                _nextButton = _nextButtonObj?.GetComponent<Button>();
                _confirmButton = _confirmButtonObj?.GetComponent<Button>();
            }

            if (_questionBoxImage != null && _questionBoxImage.sprite == null)
            {
                _questionBoxImage.sprite = questionBoxSprite;
            }
        }

        /// <summary>
        /// Shows the initial Start prompt screen.
        /// </summary>
        private void ShowStartScreen()
        {
            if (_startScreenObj != null) _startScreenObj.SetActive(true);
            if (_questionScreenObj != null) _questionScreenObj.SetActive(false);
            UIButtonHighlightSystem.ApplyToHierarchy(gameObject);
        }

        /// <summary>
        /// Handler for Start button click: initiates the first question.
        /// </summary>
        private void OnStartQuizClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (_startScreenObj != null) _startScreenObj.SetActive(false);
            if (_questionScreenObj != null) _questionScreenObj.SetActive(true);

            _currentQuestionIndex = 0;
            RenderCurrentQuestion();
        }

        /// <summary>
        /// Renders the current question and choice option buttons using QUESTION_BOX.
        /// </summary>
        private void RenderCurrentQuestion()
        {
            CaseSO activeCase = CaseManager.Instance?.ActiveCase;
            if (activeCase == null || activeCase.conclusionQuestions == null || activeCase.conclusionQuestions.Count == 0) return;

            int totalQuestions = activeCase.conclusionQuestions.Count;
            if (_currentQuestionIndex < 0) _currentQuestionIndex = 0;
            if (_currentQuestionIndex >= totalQuestions) _currentQuestionIndex = totalQuestions - 1;

            ConclusionQuestion q = activeCase.conclusionQuestions[_currentQuestionIndex];

            // Render Question Box
            if (_questionBoxText != null)
            {
                _questionBoxText.text = $"QUESTION {_currentQuestionIndex + 1} OF {totalQuestions}\n\n{q.questionText}";
            }

            if (_hintPromptText != null)
            {
                _hintPromptText.text = "";
            }

            // Clear old choices
            _choiceImages.Clear();
            _choiceTexts.Clear();

            if (_choicesContainer != null)
            {
                foreach (Transform child in _choicesContainer)
                {
                    if (Application.isPlaying)
                        Destroy(child.gameObject);
                    else
                        DestroyImmediate(child.gameObject);
                }

                Font standardFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (standardFont == null && questionTitleText != null) standardFont = questionTitleText.font;

                // Render each choice option
                for (int optIdx = 0; optIdx < q.options.Count; optIdx++)
                {
                    int choiceIndex = optIdx;
                    GameObject choiceObj = new GameObject($"Choice_{optIdx}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                    choiceObj.transform.SetParent(_choicesContainer, false);
                    RectTransform chRt = choiceObj.GetComponent<RectTransform>();
                    chRt.sizeDelta = new Vector2(660f, 48f);

                    Image chImg = choiceObj.GetComponent<Image>();
                    chImg.sprite = questionBoxSprite;
                    chImg.type = Image.Type.Sliced;
                    _choiceImages.Add(chImg);

                    GameObject chTextObj = new GameObject("ChoiceText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                    chTextObj.transform.SetParent(choiceObj.transform, false);
                    RectTransform txtRt = chTextObj.GetComponent<RectTransform>();
                    txtRt.anchorMin = Vector2.zero;
                    txtRt.anchorMax = Vector2.one;
                    txtRt.sizeDelta = new Vector2(-40f, 0f);
                    txtRt.anchoredPosition = Vector2.zero;

                    Text chTxt = chTextObj.GetComponent<Text>();
                    chTxt.font = standardFont;
                    chTxt.fontSize = 16;
                    chTxt.alignment = TextAnchor.MiddleCenter;
                    _choiceTexts.Add(chTxt);

                    Shadow chShadow = chTextObj.AddComponent<Shadow>();
                    chShadow.effectDistance = new Vector2(1.2f, -1.2f);
                    chShadow.effectColor = ShadowBlackColor;

                    Button btn = choiceObj.GetComponent<Button>();
                    btn.onClick.AddListener(() => SelectChoice(choiceIndex));
                }

                UpdateChoiceVisuals();
            }

            // Update Navigation Buttons: Next on Q1..Q4, Confirm on Q5
            bool isLastQuestion = (_currentQuestionIndex == totalQuestions - 1);
            if (_nextButtonObj != null) _nextButtonObj.SetActive(!isLastQuestion);
            if (_confirmButtonObj != null) _confirmButtonObj.SetActive(isLastQuestion);

            UIButtonHighlightSystem.ApplyToHierarchy(_questionScreenObj);
        }

        /// <summary>
        /// Selects a choice for the current question and updates button highlight.
        /// </summary>
        /// <param name="choiceIndex">The option index chosen by the player.</param>
        private void SelectChoice(int choiceIndex)
        {
            AudioManager.Instance?.PlayButtonClick();
            playerAnswers[_currentQuestionIndex] = choiceIndex;
            if (_hintPromptText != null) _hintPromptText.text = "";
            UpdateChoiceVisuals();
        }

        /// <summary>
        /// Updates the visual highlight and text labels for choice buttons based on current selection.
        /// </summary>
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

                if (_choiceTexts[i] != null)
                {
                    _choiceTexts[i].text = isSelected ? $"<b>[✓]  {q.options[i]}</b>" : $"   [ ]  {q.options[i]}";
                    _choiceTexts[i].color = isSelected ? HeaderGoldColor : Color.white;
                }
            }
        }

        /// <summary>
        /// Advances to the next question with no turning back.
        /// </summary>
        private void OnNextQuestionClicked()
        {
            if (playerAnswers[_currentQuestionIndex] < 0)
            {
                if (_hintPromptText != null)
                {
                    _hintPromptText.text = "Please select an answer to proceed.";
                }
                AudioManager.Instance?.PlaySFX(AudioManager.Instance?.caseFailedSFX);
                return;
            }

            AudioManager.Instance?.PlayButtonClick();
            _currentQuestionIndex++;
            RenderCurrentQuestion();
        }

        /// <summary>
        /// Confirms the final question and concludes the case.
        /// </summary>
        private void OnConfirmCaseClicked()
        {
            if (playerAnswers[_currentQuestionIndex] < 0)
            {
                if (_hintPromptText != null)
                {
                    _hintPromptText.text = "Please select an answer before confirming.";
                }
                AudioManager.Instance?.PlaySFX(AudioManager.Instance?.caseFailedSFX);
                return;
            }

            AudioManager.Instance?.PlayButtonClick();
            OnSubmitClicked();
        }

        /// <summary>
        /// Handles case evaluation submission in <see cref="CaseConclusionManager"/> and displaying results.
        /// </summary>
        private void OnSubmitClicked()
        {
            Debug.Log($"[UI:Conclusion] Submit conclusion button clicked. Answers count: {playerAnswers.Count}");
            if (CaseConclusionManager.Instance == null) return;

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
            DisplayResultsCard(result);
        }

        /// <summary>
        /// Resolves the appropriate failed background sprite based on case level number.
        /// </summary>
        private Sprite GetFailedSprite(int levelNumber)
        {
            if (levelNumber == 2)
            {
#if UNITY_EDITOR
                var s2 = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Assets/BACKGROUNDS/Case2FAILED.png");
                if (s2 != null) return s2;
#endif
            }
            else if (levelNumber == 3)
            {
#if UNITY_EDITOR
                var s3 = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Assets/BACKGROUNDS/Case3FAILED.png");
                if (s3 != null) return s3;
#endif
            }

            if (failedBackgroundSprite != null) return failedBackgroundSprite;

#if UNITY_EDITOR
            var s1 = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Assets/BACKGROUNDS/case1FAILED.png");
            if (s1 != null) return s1;
#endif
            return null;
        }

        /// <summary>
        /// Populates and displays the final evaluation results scorecard.
        /// </summary>
        /// <param name="result">The evaluation result data to display.</param>
        private void DisplayResultsCard(CaseEvaluationResult result)
        {
            if (result == null) return;

            EnsureAssets();

            CaseSO activeCase = CaseManager.Instance?.ActiveCase;
            CharacterProfileSO investigator = CaseManager.Instance?.EffectiveInvestigator;
            string investigatorName = investigator != null ? investigator.fullName : "Unknown Investigator";
            int currentLevel = activeCase != null ? activeCase.levelNumber : 1;

            bool isAllCorrect = (result.totalQuizQuestions > 0 && result.correctQuizAnswers == result.totalQuizQuestions);

            Debug.Log($"[UI:Conclusion] Displaying results scorecard: AllCorrect={isAllCorrect}, Solved={result.isCaseSolved}, Score={result.totalScore}, Grade={result.rankGrade}, Stars={result.starCount}");

            if (quizContainer != null) quizContainer.SetActive(false);
            if (resultsContainer != null) resultsContainer.SetActive(true);

            // Outcome Background: CasesWIN on all correct, case1FAILED on loss
            if (resultBackgroundImage != null)
            {
                if (isAllCorrect)
                {
                    Sprite winSprite = solvedBackgroundSprite;
#if UNITY_EDITOR
                    if (winSprite == null) winSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Assets/BACKGROUNDS/CasesWIN.png");
#endif
                    resultBackgroundImage.sprite = winSprite;
                    resultBackgroundImage.color = (winSprite != null) ? Color.white : new Color(0.06f, 0.07f, 0.09f, 0.98f);
                }
                else
                {
                    Sprite failSprite = GetFailedSprite(currentLevel);
                    resultBackgroundImage.sprite = failSprite;
                    resultBackgroundImage.color = (failSprite != null) ? Color.white : new Color(0.06f, 0.07f, 0.09f, 0.98f);
                }
            }

            if (resultTitleText != null)
            {
                resultTitleText.gameObject.SetActive(true);
                resultTitleText.text = isAllCorrect ? "CASE CLOSED" : "CASE NOT CLOSED";
                resultTitleText.color = isAllCorrect ? Color.green : Color.red;
            }

            if (isAllCorrect)
            {
                CaseClosed.Services.CaseProgressionService.Instance?.SetCaseCompleted(currentLevel, true);
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
                    $"Case {currentLevel}: {activeCase?.caseTitle}\n" +
                    $"Total Score: {result.totalScore} pts\n" +
                    $"Correct Quiz Answers: {result.correctQuizAnswers}/{result.totalQuizQuestions}\n" +
                    $"Evidence Discovered: {result.evidenceFoundCount}/{result.totalEvidenceCount}\n" +
                    $"Contradictions Caught: {result.contradictionsCaughtCount}/{result.totalContradictionsCount}\n" +
                    $"Time Taken: {Mathf.FloorToInt(result.completionTimeSeconds / 60)}m {Mathf.FloorToInt(result.completionTimeSeconds % 60)}s";
            }

            if (nextLevelButton != null)
            {
                nextLevelButton.gameObject.SetActive(isAllCorrect);
                int nextLevel = currentLevel + 1;
                if (nextLevel <= 3)
                {
                    if (nextLevelButtonText != null)
                    {
                        nextLevelButtonText.text = $"Proceed to Level {nextLevel} >";
                    }
                }
                else
                {
                    if (nextLevelButtonText != null)
                    {
                        nextLevelButtonText.text = "Replay / Level Select";
                    }
                }
            }

            if (returnToMainMenuButton != null) returnToMainMenuButton.gameObject.SetActive(isAllCorrect);
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(!isAllCorrect);
                if (continueButtonText != null) continueButtonText.text = "Back to Level Start";
            }
        }

        /// <summary>
        /// Handles a failed conclusion by restarting the active level from its initial state.
        /// </summary>
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

        /// <summary>
        /// Handles click on next level button, progressing from Level 1 -> Level 2 -> Level 3 or opening selection.
        /// </summary>
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
            OnMainMenuClicked();
        }

        /// <summary>
        /// Handles click on return to main menu button, navigating back to the main menu.
        /// </summary>
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
