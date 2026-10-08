using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CaseClosed.Managers;
using CaseClosed.Services;

namespace CaseClosed.UI
{
    /// <summary>
    /// Dedicated UI coordinator for the standalone Level / Case Selection scene.
    /// Displays dossier cards for Case 01, Case 02, and Case 03 with unlock/completion badges,
    /// launches the selected case, and provides navigation back to the Main Menu.
    /// </summary>
    public class LevelSelectionUI : MonoBehaviour
    {
        [Header("Case Dossier Buttons")]
        public Button case01Button;
        public Text case01TitleText;
        public Text case01StatusText;

        public Button case02Button;
        public Text case02TitleText;
        public Text case02StatusText;

        public Button case03Button;
        public Text case03TitleText;
        public Text case03StatusText;

        [Header("Navigation Buttons")]
        public Button backButton;

        private bool _referencesResolved = false;

        private void Awake()
        {
            AutoResolveReferences();
        }

        private void Start()
        {
            AutoResolveReferences();
            BindButtons();
            RefreshUI();
            UIButtonHighlightSystem.ApplyToHierarchy(gameObject);
        }

        private void OnEnable()
        {
            if (CaseProgressionService.Instance != null)
            {
                CaseProgressionService.Instance.OnProgressionChanged -= RefreshUI;
                CaseProgressionService.Instance.OnProgressionChanged += RefreshUI;
            }
            RefreshUI();
        }

        private void OnDisable()
        {
            if (CaseProgressionService.Instance != null)
            {
                CaseProgressionService.Instance.OnProgressionChanged -= RefreshUI;
            }
        }

        /// <summary>
        /// Automatically resolves UI element references in the hierarchy if unassigned.
        /// </summary>
        public void AutoResolveReferences()
        {
            if (_referencesResolved) return;

            if (case01Button == null)
            {
                Transform b = transform.Find("Button_Case01") ?? transform.Find("Container_CaseCards/Button_Case01") ?? transform.Find("Container_CaseSelect/Button_Case01");
                if (b != null) case01Button = b.GetComponent<Button>();
            }
            if (case02Button == null)
            {
                Transform b = transform.Find("Button_Case02") ?? transform.Find("Container_CaseCards/Button_Case02") ?? transform.Find("Container_CaseSelect/Button_Case02");
                if (b != null) case02Button = b.GetComponent<Button>();
            }
            if (case03Button == null)
            {
                Transform b = transform.Find("Button_Case03") ?? transform.Find("Container_CaseCards/Button_Case03") ?? transform.Find("Container_CaseSelect/Button_Case03");
                if (b != null) case03Button = b.GetComponent<Button>();
            }
            if (backButton == null)
            {
                Transform b = transform.Find("Button_Back") ?? transform.Find("Container_CaseSelect/Button_Back") ?? transform.Find("Button_BackToMenu");
                if (b != null) backButton = b.GetComponent<Button>();
            }

            ResolveCardTexts(case01Button, ref case01TitleText, ref case01StatusText);
            ResolveCardTexts(case02Button, ref case02TitleText, ref case02StatusText);
            ResolveCardTexts(case03Button, ref case03TitleText, ref case03StatusText);

            _referencesResolved = true;
        }

        private void ResolveCardTexts(Button btn, ref Text titleText, ref Text statusText)
        {
            if (btn == null) return;

            if (titleText == null)
            {
                Transform t = btn.transform.Find("Text_Title") ?? btn.transform.Find("Title") ?? btn.transform.Find("Text_CaseTitle");
                if (t != null) titleText = t.GetComponent<Text>();
            }

            if (statusText == null)
            {
                Transform s = btn.transform.Find("Text_Status") ?? btn.transform.Find("Status") ?? btn.transform.Find("Text_Badge");
                if (s != null) statusText = s.GetComponent<Text>();
            }
        }

        private void BindButtons()
        {
            if (case01Button != null)
            {
                case01Button.onClick.RemoveAllListeners();
                case01Button.onClick.AddListener(() => OnCaseClicked(1));
            }
            if (case02Button != null)
            {
                case02Button.onClick.RemoveAllListeners();
                case02Button.onClick.AddListener(() => OnCaseClicked(2));
            }
            if (case03Button != null)
            {
                case03Button.onClick.RemoveAllListeners();
                case03Button.onClick.AddListener(() => OnCaseClicked(3));
            }
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(OnBackClicked);
            }
        }

        /// <summary>
        /// Updates button interactability and status badges based on case progression.
        /// </summary>
        public void RefreshUI()
        {
            var progression = CaseProgressionService.Instance;

            UpdateCard(1, case01Button, case01TitleText, case01StatusText, progression);
            UpdateCard(2, case02Button, case02TitleText, case02StatusText, progression);
            UpdateCard(3, case03Button, case03TitleText, case03StatusText, progression);
        }

        private void UpdateCard(int levelIndex, Button btn, Text titleText, Text statusText, CaseProgressionService progression)
        {
            if (btn == null) return;

            bool isUnlocked = (progression != null) ? progression.IsCaseUnlocked(levelIndex) : (levelIndex == 1);
            bool isCompleted = (progression != null) && progression.IsCaseCompleted(levelIndex);

            btn.interactable = isUnlocked;

            if (titleText != null)
            {
                titleText.fontStyle = FontStyle.Bold;
                if (!string.IsNullOrEmpty(titleText.text))
                {
                    titleText.text = titleText.text.ToUpperInvariant();
                }
                titleText.color = isUnlocked ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.75f);
            }

            if (statusText != null)
            {
                statusText.fontStyle = FontStyle.Bold;
                if (isCompleted)
                {
                    statusText.text = "[ COMPLETED \u2605 ]";
                    statusText.color = new Color(0.25f, 0.85f, 0.45f, 1f); // Vibrant Green
                }
                else if (isUnlocked)
                {
                    statusText.text = "[ AVAILABLE ]";
                    statusText.color = new Color(1f, 0.85f, 0.45f, 1f); // Warm Gold
                }
                else
                {
                    int requiredLevel = levelIndex - 1;
                    statusText.text = $"[ LOCKED \uD83D\uDD12 (BEAT CASE 0{requiredLevel}) ]";
                    statusText.color = new Color(0.75f, 0.35f, 0.35f, 0.85f); // Dim Red
                }
            }

            Image btnImg = btn.GetComponent<Image>();
            if (btnImg != null)
            {
                btnImg.color = isUnlocked ? Color.white : new Color(0.2f, 0.2f, 0.2f, 0.95f);
            }

            Transform shadeChild = btn.transform.Find("BlackShade") ?? btn.transform.Find("Image_BlackShade") ?? btn.transform.Find("Shade");
            if (shadeChild != null)
            {
                shadeChild.gameObject.SetActive(!isUnlocked);
            }
        }

        private void OnCaseClicked(int levelIndex)
        {
            var progression = CaseProgressionService.Instance;
            if (progression != null && !progression.IsCaseUnlocked(levelIndex))
            {
                Debug.LogWarning($"[UI:LevelSelection] Case 0{levelIndex} is locked. Complete previous case first.");
                if (AudioManager.Instance != null && AudioManager.Instance.caseFailedSFX != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.caseFailedSFX);
                }
                return;
            }

            LaunchCase(levelIndex);
        }

        /// <summary>
        /// Stores the chosen level into PlayerPrefs and loads the gameplay scene.
        /// </summary>
        /// <param name="levelIndex">1-based case index (1, 2, or 3).</param>
        public void LaunchCase(int levelIndex)
        {
            Debug.Log($"[UI:LevelSelection] Launching Case {levelIndex}...");
            AudioManager.Instance?.PlayButtonClick();

            PlayerPrefs.SetInt("CaseClosed_SelectedLevel", levelIndex);
            PlayerPrefs.Save();

            string sceneName = $"Case00{levelIndex}";

            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.Log($"[UI:LevelSelection] Loading scene '{sceneName}' via SceneManager...");
                SceneManager.LoadScene(sceneName);
                return;
            }

            // Fallback: If dedicated scene isn't built but Case001 is available, load Case001
            if (Application.CanStreamedLevelBeLoaded("Case001"))
            {
                Debug.Log($"[UI:LevelSelection] '{sceneName}' not streamable. Loading Case001 scene fallback...");
                SceneManager.LoadScene("Case001");
                return;
            }

            Debug.LogError($"[UI:LevelSelection] Neither '{sceneName}' nor 'Case001' can be loaded!");
        }

        /// <summary>
        /// Returns back to the Main Menu scene.
        /// </summary>
        public void OnBackClicked()
        {
            Debug.Log("[UI:LevelSelection] Returning to Main Menu...");
            AudioManager.Instance?.PlayButtonClick();

            if (Application.CanStreamedLevelBeLoaded("MainMenu"))
            {
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                Debug.LogError("[UI:LevelSelection] Cannot stream MainMenu scene from build settings!");
            }
        }
    }
}
