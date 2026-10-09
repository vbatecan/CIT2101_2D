using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    /// <summary>Renders case outcomes in the additive ResultScreen scene.</summary>
    public class ResultsScreenUI : MonoBehaviour
    {
        [Header("Outcome Artwork")]
        [SerializeField] private Sprite solvedBackgroundSprite;
        [Tooltip("Failure artwork for cases 1, 2, and 3, in that order.")]
        [SerializeField] private Sprite[] failedBackgroundSprites;

        private GameObject resultsContainer;
        private Image resultBackgroundImage;
        private Button continueButton;
        private Button nextLevelButton;
        private Button returnToMainMenuButton;
        private CaseEvaluationResult displayedResult;

        private void Awake()
        {
            if (transform.childCount == 0) return;
            resultsContainer = transform.GetChild(0).gameObject;
            Transform background = resultsContainer.transform.Find("Image_SolvedBackground");
            if (background != null) resultBackgroundImage = background.GetComponent<Image>();
            foreach (Button button in resultsContainer.GetComponentsInChildren<Button>(true))
            {
                if (button.name == "Button_MainMenu") returnToMainMenuButton = button;
                else if (button.name == "Button_NextLevel") nextLevelButton = button;
            }
            GameObject retry = DetectiveUITheme.CreateButton(resultsContainer.transform, "Button_Continue");
            if (retry != null)
            {
                continueButton = retry.GetComponent<Button>();
                continueButton.onClick.AddListener(OnContinueClicked);
            }
            if (returnToMainMenuButton != null) returnToMainMenuButton.onClick.AddListener(OnMainMenuClicked);
            if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        }

        public void Display(CaseEvaluationResult result)
        {
            if (result == null || displayedResult == result || resultsContainer == null) return;
            displayedResult = result;
            CaseSO activeCase = CaseManager.Instance?.ActiveCase;
            int currentLevel = 1;
            if (activeCase != null) currentLevel = activeCase.levelNumber;
            bool allCorrect = result.totalQuizQuestions > 0 && result.correctQuizAnswers == result.totalQuizQuestions;
            Sprite artwork = solvedBackgroundSprite;
            if (!allCorrect)
            {
                artwork = null;
                if (failedBackgroundSprites != null && failedBackgroundSprites.Length > 0)
                {
                    int index = Mathf.Clamp(currentLevel - 1, 0, failedBackgroundSprites.Length - 1);
                    artwork = failedBackgroundSprites[index];
                }
            }
            if (resultBackgroundImage != null)
            {
                resultBackgroundImage.sprite = artwork;
                resultBackgroundImage.color = DetectiveUITheme.Paper;
                if (artwork != null) resultBackgroundImage.color = Color.white;
                resultBackgroundImage.preserveAspect = false;
                resultBackgroundImage.transform.SetAsFirstSibling();
            }
            foreach (Text text in resultsContainer.GetComponentsInChildren<Text>(true))
            {
                if (text.GetComponentInParent<Button>() == null)
                {
                    text.text = string.Empty;
                    text.gameObject.SetActive(false);
                }
            }
            if (allCorrect)
            {
                ConfigureButton(returnToMainMenuButton, "Main menu", new Vector2(-150f, 90f), new Vector2(240f, 65f), DetectiveUITheme.MutedInk);
                ConfigureButton(nextLevelButton, "Next case", new Vector2(150f, 90f), new Vector2(240f, 65f), DetectiveUITheme.Ink);
                if (continueButton != null) continueButton.gameObject.SetActive(false);
            }
            else
            {
                ConfigureButton(returnToMainMenuButton, "Main menu", new Vector2(120f, 40f), new Vector2(200f, 50f), DetectiveUITheme.MutedInk);
                ConfigureButton(continueButton, "Reopen case", new Vector2(-120f, 40f), new Vector2(200f, 50f), DetectiveUITheme.Ink);
                if (nextLevelButton != null) nextLevelButton.gameObject.SetActive(false);
            }
            UIButtonHighlightSystem.ApplyToHierarchy(resultsContainer);
        }

        private static void ConfigureButton(Button button, string caption, Vector2 position, Vector2 size, Color color)
        {
            if (button == null) return;
            button.gameObject.SetActive(true);
            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = rect.anchorMin;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = position;
                rect.sizeDelta = size;
            }
            DetectiveUITheme.Action(button, caption, color);
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
            if (Application.CanStreamedLevelBeLoaded("LevelSelect"))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("LevelSelect");
                return;
            }
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
