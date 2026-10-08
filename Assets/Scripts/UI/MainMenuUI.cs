using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Enums;
using CaseClosed.Managers;
using CaseClosed.Prototype;

namespace CaseClosed.UI
{
    /// <summary>
    /// Master UI View coordinator for the Main Menu screen.
    /// Coordinates sub-panels (Title Buttons, Case Select dossier cards, Audio & Gameplay Settings,
    /// How to Play handbook, Credits, and Exit Confirmation Modal) and launches cases.
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
        public GameObject exitConfirmContainer;

        [Header("Main Navigation Buttons")]
        public Button playButton;
        public Button caseSelectButton;
        public Button howToPlayButton;
        public Button settingsButton;
        public Button creditsButton;
        public Button quitButton;

        [Header("Case Select UI")]
        public Button case01Button;
        public Text case01TitleText;
        public Text case01StatusText;

        public Button case02Button;
        public Text case02TitleText;
        public Text case02StatusText;

        public Button case03Button;
        public Text case03TitleText;
        public Text case03StatusText;

        public Button backFromCaseSelectButton;

        [Header("Sub-Panel Back Buttons")]
        public Button backFromHowToPlayButton;
        public Button backFromSettingsButton;
        public Button backFromCreditsButton;

        [Header("Audio Settings - Music")]
        public Slider bgmVolumeSlider;
        public Text bgmPercentText;
        public Button bgmMuteButton;
        public Text bgmMuteText;

        [Header("Audio Settings - SFX")]
        public Slider sfxVolumeSlider;
        public Text sfxPercentText;
        public Button sfxMuteButton;
        public Text sfxMuteText;

        [Header("Audio Settings - Dialogue")]
        public Slider dialogVolumeSlider;
        public Text dialogPercentText;
        public Button dialogMuteButton;
        public Text dialogMuteText;

        [Header("Display & Gameplay Settings")]
        public Toggle fullscreenToggle;
        public Text fullscreenStatusText;
        public Slider textSpeedSlider;
        public Text textSpeedText;
        public Toggle typewriterToggle;
        public Button resetSettingsButton;

        [Header("Exit Confirmation Modal")]
        public Button confirmExitYesButton;
        public Button confirmExitNoButton;

        private bool _referencesResolved = false;

        private void Awake()
        {
            AutoResolveReferences();
        }

        private void Start()
        {
            AutoResolveReferences();
            BindMainButtons();
            BindCaseSelectButtons();
            BindSettingsButtons();
            BindExitConfirmButtons();
            BindBackButtons();
            UIButtonHighlightSystem.ApplyToHierarchy(gameObject);
        }

        private void OnEnable()
        {
            AutoResolveReferences();
            ReturnToMainView(false);
            RefreshCaseSelectUI();
            AudioManager.Instance?.PlayMenuBGM();
            UIButtonHighlightSystem.ApplyToHierarchy(gameObject);

            if (CaseClosed.Services.CaseProgressionService.Instance != null)
            {
                CaseClosed.Services.CaseProgressionService.Instance.OnProgressionChanged -= RefreshCaseSelectUI;
                CaseClosed.Services.CaseProgressionService.Instance.OnProgressionChanged += RefreshCaseSelectUI;
            }
        }

        private void OnDisable()
        {
            if (CaseClosed.Services.CaseProgressionService.Instance != null)
            {
                CaseClosed.Services.CaseProgressionService.Instance.OnProgressionChanged -= RefreshCaseSelectUI;
            }
        }

        private void OnDestroy()
        {
            if (CaseClosed.Services.CaseProgressionService.Instance != null)
            {
                CaseClosed.Services.CaseProgressionService.Instance.OnProgressionChanged -= RefreshCaseSelectUI;
            }
        }

        private void Update()
        {
            // Keyboard shortcuts & Escape navigation
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (exitConfirmContainer != null && exitConfirmContainer.activeSelf)
                {
                    CloseExitConfirmation();
                }
                else if (IsAnySubViewOpen())
                {
                    ReturnToMainView();
                }
                else
                {
                    OpenExitConfirmation();
                }
            }
        }

        private bool IsAnySubViewOpen()
        {
            return (caseSelectContainer != null && caseSelectContainer.activeSelf) ||
                   (howToPlayContainer != null && howToPlayContainer.activeSelf) ||
                   (settingsContainer != null && settingsContainer.activeSelf) ||
                   (creditsContainer != null && creditsContainer.activeSelf);
        }

        /// <summary>
        /// Attempts to locate missing button or panel references in the hierarchy if unassigned in Inspector.
        /// </summary>
        private void AutoResolveReferences(bool force = false)
        {
            if (_referencesResolved && !force) return;
            if (mainButtonsContainer == null)
            {
                Transform containerTr = transform.Find("Container_MainButtons") ?? transform.Find("MainButtons") ?? transform.Find("Buttons");
                if (containerTr != null) mainButtonsContainer = containerTr.gameObject;
            }

            if (caseSelectContainer == null)
            {
                Transform c = transform.Find("Panel_CaseSelect") ?? transform.Find("CaseSelectContainer");
                if (c != null) caseSelectContainer = c.gameObject;
            }
            if (settingsContainer == null)
            {
                Transform c = transform.Find("Panel_Settings") ?? transform.Find("SettingsContainer");
                if (c != null) settingsContainer = c.gameObject;
            }
            if (howToPlayContainer == null)
            {
                Transform c = transform.Find("Panel_HowToPlay") ?? transform.Find("HowToPlayContainer");
                if (c != null) howToPlayContainer = c.gameObject;
            }
            if (creditsContainer == null)
            {
                Transform c = transform.Find("Panel_Credits") ?? transform.Find("CreditsContainer");
                if (c != null) creditsContainer = c.gameObject;
            }
            if (exitConfirmContainer == null)
            {
                Transform c = transform.Find("Panel_ExitConfirmation") ?? transform.Find("ExitConfirmContainer");
                if (c != null) exitConfirmContainer = c.gameObject;
            }

            if (playButton == null)
            {
                playButton = transform.Find("Button_Play")?.GetComponent<Button>() ?? transform.Find("Container_MainButtons/Button_Play")?.GetComponent<Button>();
            }
            if (caseSelectButton == null)
            {
                caseSelectButton = transform.Find("Button_CaseSelect")?.GetComponent<Button>() ?? transform.Find("Container_MainButtons/Button_CaseSelect")?.GetComponent<Button>();
            }
            if (settingsButton == null)
            {
                settingsButton = transform.Find("Button_Settings")?.GetComponent<Button>() ?? transform.Find("Container_MainButtons/Button_Settings")?.GetComponent<Button>();
            }
            if (howToPlayButton == null)
            {
                howToPlayButton = transform.Find("Button_HowToPlay")?.GetComponent<Button>() ?? transform.Find("Container_MainButtons/Button_HowToPlay")?.GetComponent<Button>();
            }
            if (creditsButton == null)
            {
                creditsButton = transform.Find("Button_Credits")?.GetComponent<Button>() ?? transform.Find("Container_MainButtons/Button_Credits")?.GetComponent<Button>();
            }
            if (quitButton == null)
            {
                quitButton = transform.Find("Button_Quit")?.GetComponent<Button>() ?? transform.Find("Container_MainButtons/Button_Quit")?.GetComponent<Button>();
            }

            _referencesResolved = true;
        }

        /// <summary>
        /// Toggles the main navigation buttons container or individual buttons if no container object exists.
        /// </summary>
        private void SetMainButtonsActive(bool active)
        {
            if (mainButtonsContainer != null)
            {
                mainButtonsContainer.SetActive(active);
            }
            else
            {
                if (playButton != null) playButton.gameObject.SetActive(active);
                if (caseSelectButton != null) caseSelectButton.gameObject.SetActive(active);
                if (howToPlayButton != null) howToPlayButton.gameObject.SetActive(active);
                if (settingsButton != null) settingsButton.gameObject.SetActive(active);
                if (creditsButton != null) creditsButton.gameObject.SetActive(active);
                if (quitButton != null) quitButton.gameObject.SetActive(active);
            }
        }

        private void BindMainButtons()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            if (caseSelectButton != null) caseSelectButton.onClick.AddListener(OpenCaseSelect);
            if (howToPlayButton != null) howToPlayButton.onClick.AddListener(() => OpenSubView(howToPlayContainer));
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (creditsButton != null) creditsButton.onClick.AddListener(() => OpenSubView(creditsContainer));
            if (quitButton != null) quitButton.onClick.AddListener(OpenExitConfirmation);
        }

        private void BindCaseSelectButtons()
        {
            if (case01Button != null) case01Button.onClick.AddListener(() => OnCaseButtonClicked(1));
            if (case02Button != null) case02Button.onClick.AddListener(() => OnCaseButtonClicked(2));
            if (case03Button != null) case03Button.onClick.AddListener(() => OnCaseButtonClicked(3));
            if (backFromCaseSelectButton != null) backFromCaseSelectButton.onClick.AddListener(ReturnToMainView);
        }

        private void BindSettingsButtons()
        {
            // Music Volume & Mute
            if (bgmVolumeSlider != null)
                bgmVolumeSlider.onValueChanged.AddListener(OnBgmVolumeSliderChanged);
            if (bgmMuteButton != null)
                bgmMuteButton.onClick.AddListener(OnBgmMuteButtonClicked);

            // SFX Volume & Mute
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeSliderChanged);
            if (sfxMuteButton != null)
                sfxMuteButton.onClick.AddListener(OnSfxMuteButtonClicked);

            // Dialogue Volume & Mute
            if (dialogVolumeSlider != null)
                dialogVolumeSlider.onValueChanged.AddListener(OnDialogVolumeSliderChanged);
            if (dialogMuteButton != null)
                dialogMuteButton.onClick.AddListener(OnDialogMuteButtonClicked);

            // Display Mode
            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);

            // Text Speed
            if (textSpeedSlider != null)
                textSpeedSlider.onValueChanged.AddListener(OnTextSpeedSliderChanged);

            // Typewriter SFX
            if (typewriterToggle != null)
                typewriterToggle.onValueChanged.AddListener(OnTypewriterToggleChanged);

            // Reset Settings
            if (resetSettingsButton != null)
                resetSettingsButton.onClick.AddListener(OnResetSettingsClicked);
        }

        private void BindExitConfirmButtons()
        {
            if (confirmExitYesButton != null) confirmExitYesButton.onClick.AddListener(OnExitConfirmed);
            if (confirmExitNoButton != null) confirmExitNoButton.onClick.AddListener(CloseExitConfirmation);
        }

        private void BindBackButtons()
        {
            if (backFromHowToPlayButton != null) backFromHowToPlayButton.onClick.AddListener(ReturnToMainView);
            if (backFromSettingsButton != null) backFromSettingsButton.onClick.AddListener(ReturnToMainView);
            if (backFromCreditsButton != null) backFromCreditsButton.onClick.AddListener(ReturnToMainView);
        }

        /// <summary>
        /// Displays the default primary buttons view and hides all sub-view overlays.
        /// </summary>
        public void ReturnToMainView() => ReturnToMainView(true);

        /// <summary>
        /// Displays the default primary buttons view and hides all sub-view overlays, optionally playing button click SFX.
        /// </summary>
        public void ReturnToMainView(bool playAudio)
        {
            SetMainButtonsActive(true);
            if (caseSelectContainer != null) caseSelectContainer.SetActive(false);
            if (howToPlayContainer != null) howToPlayContainer.SetActive(false);
            if (settingsContainer != null) settingsContainer.SetActive(false);
            if (creditsContainer != null) creditsContainer.SetActive(false);
            if (exitConfirmContainer != null) exitConfirmContainer.SetActive(false);

            if (playAudio)
            {
                AudioManager.Instance?.PlayButtonClick();
            }
        }

        /// <summary>
        /// Activates the specified sub-view overlay and hides the main buttons view.
        /// </summary>
        public void OpenSubView(GameObject targetSubView)
        {
            SetMainButtonsActive(false);
            if (caseSelectContainer != null) caseSelectContainer.SetActive(targetSubView == caseSelectContainer);
            if (howToPlayContainer != null) howToPlayContainer.SetActive(targetSubView == howToPlayContainer);
            if (settingsContainer != null) settingsContainer.SetActive(targetSubView == settingsContainer);
            if (creditsContainer != null) creditsContainer.SetActive(targetSubView == creditsContainer);
            if (exitConfirmContainer != null) exitConfirmContainer.SetActive(false);

            AudioManager.Instance?.PlayButtonClick();
        }

        /// <summary>
        /// Opens the Case Selection screen and refreshes locked/unlocked statuses.
        /// If a dedicated LevelSelect scene is registered, loads that scene; otherwise opens the in-menu subview.
        /// </summary>
        public void OpenCaseSelect()
        {
            if (UnityEngine.Application.CanStreamedLevelBeLoaded("LevelSelect"))
            {
                AudioManager.Instance?.PlayButtonClick();
                UnityEngine.SceneManagement.SceneManager.LoadScene("LevelSelect");
                return;
            }

            RefreshCaseSelectUI();
            OpenSubView(caseSelectContainer);
        }

        /// <summary>
        /// Updates button interactability and status badge texts based on CaseProgressionService.
        /// </summary>
        public void RefreshCaseSelectUI()
        {
            var progression = CaseClosed.Services.CaseProgressionService.Instance;

            UpdateCaseCard(1, case01Button, case01TitleText, case01StatusText, progression);
            UpdateCaseCard(2, case02Button, case02TitleText, case02StatusText, progression);
            UpdateCaseCard(3, case03Button, case03TitleText, case03StatusText, progression);
        }

        private void UpdateCaseCard(int levelIndex, Button btn, Text titleText, Text statusText, CaseClosed.Services.CaseProgressionService progression)
        {
            if (btn == null) return;

            bool isUnlocked = (progression != null) ? progression.IsCaseUnlocked(levelIndex) : (levelIndex == 1);
            bool isCompleted = (progression != null) && progression.IsCaseCompleted(levelIndex);

            btn.interactable = isUnlocked;

            if (titleText != null)
            {
                titleText.color = isUnlocked ? DetectiveUITheme.Ink : DetectiveUITheme.MutedInk;
            }

            if (statusText != null)
            {
                if (isCompleted)
                {
                    statusText.text = "[ SOLVED \u2605 ]";
                    statusText.color = DetectiveUITheme.Brass;
                }
                else if (isUnlocked)
                {
                    statusText.text = "[ AVAILABLE ]";
                    statusText.color = new Color(0.18f, 0.40f, 0.24f, 1f);
                }
                else
                {
                    int requiredLevel = levelIndex - 1;
                    statusText.text = $"[ LOCKED \uD83D\uDD12 (Beat Case 0{requiredLevel}) ]";
                    statusText.color = new Color(0.7f, 0.35f, 0.35f, 0.85f); // Dim Red
                }
            }

            // Keep folder artwork light; labels and interactability convey the lock state.
            Image btnImg = btn.GetComponent<Image>();
            if (btnImg != null)
            {
                if (!isUnlocked)
                {
                    btnImg.color = DetectiveUITheme.Paper;
                }
                else
                {
                    // Unlocked / Completed: vivid full color folder artwork
                    btnImg.color = Color.white;
                }
            }

            // Legacy dark overlays are not part of the light theme.
            Transform shadeChild = btn.transform.Find("BlackShade") ?? btn.transform.Find("Image_BlackShade") ?? btn.transform.Find("Shade");
            if (shadeChild != null)
            {
                shadeChild.gameObject.SetActive(false);
            }
        }

        private void OnCaseButtonClicked(int levelIndex)
        {
            var progression = CaseClosed.Services.CaseProgressionService.Instance;
            if (progression != null && !progression.IsCaseUnlocked(levelIndex))
            {
                Debug.LogWarning($"[UI:MainMenu] Case 0{levelIndex} is locked. Complete previous case first.");
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.caseFailedSFX);
                return;
            }

            LaunchCase(levelIndex);
        }

        /// <summary>
        /// Handles Play button click: loads the next uncompleted available case or Case 1.
        /// </summary>
        private void OnPlayClicked()
        {
            int targetLevel = 1;
            var progression = CaseClosed.Services.CaseProgressionService.Instance;
            if (progression != null)
            {
                if (!progression.IsCaseCompleted(1)) targetLevel = 1;
                else if (!progression.IsCaseCompleted(2)) targetLevel = 2;
                else if (!progression.IsCaseCompleted(3)) targetLevel = 3;
                else targetLevel = 1; // All completed, replay Case 1
            }

            Debug.Log($"[UI:MainMenu] Play clicked -> Launching Case 0{targetLevel}");
            AudioManager.Instance?.PlayButtonClick();
            LaunchCase(targetLevel);
        }

        /// <summary>
        /// Opens the settings sub-view and synchronizes all controls with GameSettingsService.
        /// </summary>
        public void OpenSettings()
        {
            RefreshSettingsUI();
            OpenSubView(settingsContainer);
        }

        /// <summary>
        /// Synchronizes all settings sliders, mute labels, toggles, and text speeds with runtime data.
        /// </summary>
        public void RefreshSettingsUI()
        {
            var settings = CaseClosed.Services.GameSettingsService.Instance;
            var audio = AudioManager.Instance;

            // BGM
            float bgmVal = audio != null ? audio.bgmVolume : (settings?.BgmVolume ?? 0.8f);
            bool bgmMuted = audio != null ? audio.isBgmMuted : (settings?.IsBgmMuted ?? false);
            if (bgmVolumeSlider != null) bgmVolumeSlider.value = bgmVal;
            if (bgmPercentText != null) bgmPercentText.text = $"{Mathf.RoundToInt(bgmVal * 100)}%";
            if (bgmMuteText != null) bgmMuteText.text = bgmMuted ? "MUTED" : "MUTE";
            UpdateMuteButtonPresentation(bgmMuteButton, bgmMuted);

            // SFX
            float sfxVal = audio != null ? audio.sfxVolume : (settings?.SfxVolume ?? 1.0f);
            bool sfxMuted = audio != null ? audio.isSfxMuted : (settings?.IsSfxMuted ?? false);
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVal;
            if (sfxPercentText != null) sfxPercentText.text = $"{Mathf.RoundToInt(sfxVal * 100)}%";
            if (sfxMuteText != null) sfxMuteText.text = sfxMuted ? "MUTED" : "MUTE";
            UpdateMuteButtonPresentation(sfxMuteButton, sfxMuted);

            // Dialogue
            float dlgVal = audio != null ? audio.dialogVolume : (settings?.DialogVolume ?? 1.0f);
            bool dlgMuted = audio != null ? audio.isDialogMuted : (settings?.IsDialogMuted ?? false);
            if (dialogVolumeSlider != null) dialogVolumeSlider.value = dlgVal;
            if (dialogPercentText != null) dialogPercentText.text = $"{Mathf.RoundToInt(dlgVal * 100)}%";
            if (dialogMuteText != null) dialogMuteText.text = dlgMuted ? "MUTED" : "MUTE";
            UpdateMuteButtonPresentation(dialogMuteButton, dlgMuted);

            // Display Mode
            bool isFull = Screen.fullScreen;
            if (fullscreenToggle != null) fullscreenToggle.isOn = isFull;
            if (fullscreenStatusText != null) fullscreenStatusText.text = isFull ? "FULLSCREEN (Borderless)" : "WINDOWED";

            // Text Speed
            float speedVal = settings != null ? settings.TextSpeed : 35f;
            if (textSpeedSlider != null) textSpeedSlider.value = speedVal;
            if (textSpeedText != null) textSpeedText.text = CaseClosed.Services.GameSettingsService.GetTextSpeedLabel(speedVal);

            // Typewriter SFX
            bool typewriterOn = audio != null ? audio.isTypewriterEnabled : (settings?.IsTypewriterEnabled ?? true);
            if (typewriterToggle != null) typewriterToggle.isOn = typewriterOn;
        }

        private void OnBgmVolumeSliderChanged(float value)
        {
            AudioManager.Instance?.SetBGMVolume(value);
            CaseClosed.Services.GameSettingsService.Instance?.SetBgmVolume(value);
            if (bgmPercentText != null) bgmPercentText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }

        private void OnBgmMuteButtonClicked()
        {
            bool newMute = !(AudioManager.Instance?.isBgmMuted ?? false);
            AudioManager.Instance?.SetBgmMuted(newMute);
            CaseClosed.Services.GameSettingsService.Instance?.SetBgmMuted(newMute);
            if (bgmMuteText != null) bgmMuteText.text = newMute ? "MUTED" : "MUTE";
            UpdateMuteButtonPresentation(bgmMuteButton, newMute);
            AudioManager.Instance?.PlayButtonClick();
        }

        private void OnSfxVolumeSliderChanged(float value)
        {
            AudioManager.Instance?.SetSFXVolume(value);
            CaseClosed.Services.GameSettingsService.Instance?.SetSfxVolume(value);
            if (sfxPercentText != null) sfxPercentText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }

        private void OnSfxMuteButtonClicked()
        {
            bool newMute = !(AudioManager.Instance?.isSfxMuted ?? false);
            AudioManager.Instance?.SetSfxMuted(newMute);
            CaseClosed.Services.GameSettingsService.Instance?.SetSfxMuted(newMute);
            if (sfxMuteText != null) sfxMuteText.text = newMute ? "MUTED" : "MUTE";
            UpdateMuteButtonPresentation(sfxMuteButton, newMute);
            AudioManager.Instance?.PlayButtonClick();
        }

        private void OnDialogVolumeSliderChanged(float value)
        {
            AudioManager.Instance?.SetDialogVolume(value);
            CaseClosed.Services.GameSettingsService.Instance?.SetDialogVolume(value);
            if (dialogPercentText != null) dialogPercentText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }

        private void OnDialogMuteButtonClicked()
        {
            bool newMute = !(AudioManager.Instance?.isDialogMuted ?? false);
            AudioManager.Instance?.SetDialogMuted(newMute);
            CaseClosed.Services.GameSettingsService.Instance?.SetDialogMuted(newMute);
            if (dialogMuteText != null) dialogMuteText.text = newMute ? "MUTED" : "MUTE";
            UpdateMuteButtonPresentation(dialogMuteButton, newMute);
            AudioManager.Instance?.PlayButtonClick();
        }

        private static void UpdateMuteButtonPresentation(Button button, bool muted)
        {
            string caption = "Mute";
            Color color = DetectiveUITheme.Ink;
            if (muted)
            {
                caption = "Muted";
                color = DetectiveUITheme.StampRed;
            }
            DetectiveUITheme.Action(button, caption, color);
        }

        private void OnFullscreenToggleChanged(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            CaseClosed.Services.GameSettingsService.Instance?.SetFullscreen(isFullscreen);
            if (fullscreenStatusText != null)
            {
                fullscreenStatusText.text = isFullscreen ? "FULLSCREEN (Borderless)" : "WINDOWED";
            }
            AudioManager.Instance?.PlayButtonClick();
        }

        private void OnTextSpeedSliderChanged(float speed)
        {
            CaseClosed.Services.GameSettingsService.Instance?.SetTextSpeed(speed);
            DialogueUI.Instance?.SetTextSpeed(speed);
            if (textSpeedText != null)
            {
                textSpeedText.text = CaseClosed.Services.GameSettingsService.GetTextSpeedLabel(speed);
            }
        }

        private void OnTypewriterToggleChanged(bool enabled)
        {
            AudioManager.Instance?.SetTypewriterEnabled(enabled);
            CaseClosed.Services.GameSettingsService.Instance?.SetTypewriterEnabled(enabled);
            AudioManager.Instance?.PlayButtonClick();
        }

        /// <summary>
        /// Restores all audio, display, and dialogue settings to default values.
        /// </summary>
        public void OnResetSettingsClicked()
        {
            Debug.Log("[UI:MainMenu] Resetting all settings to defaults...");
            AudioManager.Instance?.ResetAudioSettingsToDefault();
            CaseClosed.Services.GameSettingsService.Instance?.ResetToDefaults();
            DialogueUI.Instance?.SetTextSpeed(35f);

            RefreshSettingsUI();
            AudioManager.Instance?.PlayButtonClick();
        }

        /// <summary>
        /// Displays the Exit Confirmation Modal prompt.
        /// </summary>
        public void OpenExitConfirmation()
        {
            Debug.Log("[UI:MainMenu] Opening exit confirmation prompt...");
            if (exitConfirmContainer != null)
            {
                exitConfirmContainer.SetActive(true);
                exitConfirmContainer.transform.SetAsLastSibling();
            }
            AudioManager.Instance?.PlayButtonClick();
        }

        /// <summary>
        /// Closes the Exit Confirmation Modal prompt, returning focus to menu.
        /// </summary>
        public void CloseExitConfirmation()
        {
            Debug.Log("[UI:MainMenu] Exit cancelled. Returning to menu.");
            if (exitConfirmContainer != null)
            {
                exitConfirmContainer.SetActive(false);
            }
            AudioManager.Instance?.PlayButtonClick();
        }

        /// <summary>
        /// Exits the game application or stops editor play mode.
        /// </summary>
        public void OnExitConfirmed()
        {
            Debug.Log("[UI:MainMenu] Exit confirmed. Closing application...");
            AudioManager.Instance?.PlayButtonClick();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Loads the chosen case index via SceneManager or falls back to <see cref="GameBootstrap"/>.
        /// </summary>
        /// <param name="caseIndex">The 1-based case index (1, 2, or 3).</param>
        public void LaunchCase(int caseIndex)
        {
            Debug.Log($"[UI:MainMenu] Launching Case {caseIndex}...");
            AudioManager.Instance?.PlayButtonClick();

            PlayerPrefs.SetInt("CaseClosed_SelectedLevel", caseIndex);
            PlayerPrefs.Save();

            string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string sceneName = $"Case00{caseIndex}";

            if (activeScene == sceneName)
            {
                GameBootstrap currentBootstrap = FindFirstObjectByType<GameBootstrap>();
                if (currentBootstrap != null)
                {
                    currentBootstrap.LoadLevel(caseIndex);
                    return;
                }
            }

            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.Log($"[UI:MainMenu] Loading scene '{sceneName}' via SceneManager...");
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
                return;
            }

            // Fallback: If dedicated scene isn't built but Case001 is available, load Case001
            if (activeScene != "Case001" && Application.CanStreamedLevelBeLoaded("Case001"))
            {
                Debug.Log($"[UI:MainMenu] '{sceneName}' not streamable. Loading Case001 scene fallback...");
                UnityEngine.SceneManagement.SceneManager.LoadScene("Case001");
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
    }
}
