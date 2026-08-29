using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Enums;
using CaseClosed.Managers;
using CaseClosed.Prototype;

namespace CaseClosed.UI
{
    /// <summary>
    /// Master UI View coordinator for the Main Menu screen.
    /// Manages sub-panels (Title Buttons, Case Select, How to Play handbook, Audio Settings, Credits)
    /// and launches cases via <see cref="GameBootstrap"/>.
    /// Can be dragged directly onto the MainMenuPanel GameObject in the Unity Inspector.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Sub-View Containers")]
        public GameObject mainButtonsContainer;
        public GameObject caseSelectContainer;
        public GameObject howToPlayContainer;
        public GameObject settingsContainer;
        public GameObject creditsContainer;

        [Header("Main Navigation Buttons")]
        public Button playButton;
        public Button caseSelectButton;
        public Button howToPlayButton;
        public Button settingsButton;
        public Button creditsButton;
        public Button quitButton;

        [Header("Case Select Buttons")]
        public Button case01Button;
        public Button case02Button;
        public Button case03Button;
        public Button backFromCaseSelectButton;

        [Header("Sub-Panel Back Buttons")]
        public Button backFromHowToPlayButton;
        public Button backFromSettingsButton;
        public Button backFromCreditsButton;

        [Header("Audio Settings Controls")]
        public Slider bgmVolumeSlider;
        public Slider sfxVolumeSlider;
        public Toggle typewriterToggle;

        /// <summary>
        /// Binds all button listeners and initializes settings controls.
        /// </summary>
        private void Start()
        {
            // Main menu action buttons
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            if (caseSelectButton != null) caseSelectButton.onClick.AddListener(() => OpenSubView(caseSelectContainer));
            if (howToPlayButton != null) howToPlayButton.onClick.AddListener(() => OpenSubView(howToPlayContainer));
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (creditsButton != null) creditsButton.onClick.AddListener(() => OpenSubView(creditsContainer));
            if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);

            // Case select buttons
            if (case01Button != null) case01Button.onClick.AddListener(() => LaunchCase(1));
            if (case02Button != null) case02Button.onClick.AddListener(() => LaunchCase(2));
            if (case03Button != null) case03Button.onClick.AddListener(() => LaunchCase(3));
            if (backFromCaseSelectButton != null) backFromCaseSelectButton.onClick.AddListener(ReturnToMainView);

            // Back buttons
            if (backFromHowToPlayButton != null) backFromHowToPlayButton.onClick.AddListener(ReturnToMainView);
            if (backFromSettingsButton != null) backFromSettingsButton.onClick.AddListener(ReturnToMainView);
            if (backFromCreditsButton != null) backFromCreditsButton.onClick.AddListener(ReturnToMainView);

            // Settings listeners
            if (bgmVolumeSlider != null)
            {
                bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
            if (typewriterToggle != null)
            {
                typewriterToggle.onValueChanged.AddListener(OnTypewriterToggleChanged);
            }
        }

        /// <summary>
        /// Resets to the primary menu view and starts background music when enabled.
        /// </summary>
        private void OnEnable()
        {
            ReturnToMainView();
            AudioManager.Instance?.PlayMenuBGM();
        }

        /// <summary>
        /// Displays the default primary buttons view and hides all sub-view overlays.
        /// </summary>
        public void ReturnToMainView()
        {
            if (mainButtonsContainer != null) mainButtonsContainer.SetActive(true);
            if (caseSelectContainer != null) caseSelectContainer.SetActive(false);
            if (howToPlayContainer != null) howToPlayContainer.SetActive(false);
            if (settingsContainer != null) settingsContainer.SetActive(false);
            if (creditsContainer != null) creditsContainer.SetActive(false);

            AudioManager.Instance?.PlayButtonClick();
        }

        /// <summary>
        /// Activates the specified sub-view overlay and hides the main buttons view.
        /// </summary>
        /// <param name="targetSubView">The sub-view GameObject container to display.</param>
        public void OpenSubView(GameObject targetSubView)
        {
            if (mainButtonsContainer != null) mainButtonsContainer.SetActive(false);
            if (caseSelectContainer != null) caseSelectContainer.SetActive(targetSubView == caseSelectContainer);
            if (howToPlayContainer != null) howToPlayContainer.SetActive(targetSubView == howToPlayContainer);
            if (settingsContainer != null) settingsContainer.SetActive(targetSubView == settingsContainer);
            if (creditsContainer != null) creditsContainer.SetActive(targetSubView == creditsContainer);

            AudioManager.Instance?.PlayButtonClick();
        }

        /// <summary>
        /// Handles Play button click: loads the first case or resumes investigation.
        /// </summary>
        private void OnPlayClicked()
        {
            Debug.Log("[UI:MainMenu] Play button clicked -> Starting Case 01");
            AudioManager.Instance?.PlayButtonClick();
            LaunchCase(1);
        }

        /// <summary>
        /// Opens the settings sub-view and synchronizes UI controls with current AudioManager volume values.
        /// </summary>
        private void OpenSettings()
        {
            if (AudioManager.Instance != null)
            {
                if (bgmVolumeSlider != null) bgmVolumeSlider.value = AudioManager.Instance.bgmVolume;
                if (sfxVolumeSlider != null) sfxVolumeSlider.value = AudioManager.Instance.sfxVolume;
                if (typewriterToggle != null) typewriterToggle.isOn = AudioManager.Instance.isTypewriterEnabled;
            }
            OpenSubView(settingsContainer);
        }

        /// <summary>
        /// Loads the chosen case index via SceneManager or falls back to <see cref="GameBootstrap"/>.
        /// </summary>
        /// <param name="caseIndex">The 1-based case index (1, 2, or 3).</param>
        public void LaunchCase(int caseIndex)
        {
            Debug.Log($"[UI:MainMenu] Launching Case {caseIndex}...");
            AudioManager.Instance?.PlayButtonClick();

            string sceneName = $"Case00{caseIndex}";
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.Log($"[UI:MainMenu] Loading scene '{sceneName}' via SceneManager...");
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
                return;
            }

            GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
            if (bootstrap != null)
            {
                bootstrap.LoadLevel(caseIndex);
            }
            else
            {
                Debug.LogWarning("[UI:MainMenu] GameBootstrap not found in scene. Loading default case via CaseManager.");
                UIManager.Instance?.ShowPanel(UIPanelType.InvestigationTable);
            }
        }

        /// <summary>
        /// Handles BGM volume slider changes.
        /// </summary>
        private void OnBGMVolumeChanged(float value)
        {
            AudioManager.Instance?.SetBGMVolume(value);
        }

        /// <summary>
        /// Handles SFX volume slider changes.
        /// </summary>
        private void OnSFXVolumeChanged(float value)
        {
            AudioManager.Instance?.SetSFXVolume(value);
        }

        /// <summary>
        /// Handles typewriter sound toggle changes.
        /// </summary>
        private void OnTypewriterToggleChanged(bool value)
        {
            AudioManager.Instance?.SetTypewriterEnabled(value);
        }

        /// <summary>
        /// Exits the game application or stops editor play mode.
        /// </summary>
        private void OnQuitClicked()
        {
            Debug.Log("[UI:MainMenu] Quitting game application...");
            AudioManager.Instance?.PlayButtonClick();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
