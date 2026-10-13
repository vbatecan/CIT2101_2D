using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    public class ResultsScreenUI : MonoBehaviour
    {
        [Header("Outcome Artwork")]
        [SerializeField] private Sprite solvedBackgroundSprite;
        [Tooltip("Failure artwork for cases 1, 2, and 3, in that order.")]
        [SerializeField] private Sprite[] failedBackgroundSprites;

        [Header("Scene UI References")]
        [SerializeField] private GameObject resultsContainer;
        [SerializeField] private Image resultBackgroundImage;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button returnToMainMenuButton;
        private CaseEvaluationResult displayedResult;

        private void Awake()
        {
            if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
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
            }
            if (returnToMainMenuButton != null) returnToMainMenuButton.gameObject.SetActive(true);
            if (nextLevelButton != null) nextLevelButton.gameObject.SetActive(allCorrect);
            if (continueButton != null) continueButton.gameObject.SetActive(!allCorrect);
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
