using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Gameplay;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Panels")]
        public GameObject mainMenuPanel;
        public GameObject mainTablePanel;
        public GameObject inspectModalPanel;
        public GameObject notebookPanel;
        public GameObject suspectFolderPanel;
        public GameObject deductionBoardPanel;
        public GameObject conclusionQuizPanel;
        public GameObject resultsScreenPanel;
        public GameObject investigatorSelectPanel;
        public GameObject gameOverPanel;
        public GameObject inGameMenuPanel;
        public GameObject mainMenuConfirmPanel;

        [Header("Header Navigation Elements")]
        public GameObject timerContainer;
        public GameObject notebookButton;
        public GameObject suspectFolderButton;
        public GameObject deductionBoardButton;
        public GameObject concludeCaseButton;
        public GameObject investigatorSelectButton;
        public GameObject returnToMenuButton;

        [Header("In-Game Menu Buttons")]
        public Button resumeGameButton;
        public Button inGameMainMenuButton;
        public Button confirmMainMenuYesButton;
        public Button confirmMainMenuNoButton;

        private UIPanelType _currentPanel = UIPanelType.MainMenu;
        private UIPanelType _panelBeforeInGameMenu = UIPanelType.InvestigationTable;
        private EvidenceManager _subscribedEvidenceManager;
        private CaseManager _subscribedCaseManager;
        private InterrogationManager _subscribedInterrogationManager;
        private Button concludeNavigationButton;
        private CaseEvaluationResult pendingResult;
        private bool syncingOutcomeScenes;
        private const string ResultsSceneName = "ResultScreen";
        private const string GameOverSceneName = "GameOver";

        public UIPanelType currentPanel => _currentPanel;

        private void Awake()
        {
            Instance = this;

            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.pixelPerfect = true;
            }

            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler != null && scaler.dynamicPixelsPerUnit < 3.0f)
            {
                scaler.dynamicPixelsPerUnit = 3.0f;
            }
        }

        private void OnDestroy()
        {
            UnregisterEvents();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            if (returnToMenuButton != null)
            {
                Button btn = returnToMenuButton.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(ToggleInGameMenu);
            }

            if (resumeGameButton != null)
            {
                resumeGameButton.onClick.AddListener(CloseInGameMenu);
            }

            if (inGameMainMenuButton != null)
            {
                inGameMainMenuButton.onClick.AddListener(OpenMainMenuConfirmation);
            }

            if (confirmMainMenuYesButton != null)
            {
                confirmMainMenuYesButton.onClick.AddListener(ConfirmReturnToMainMenu);
            }

            if (confirmMainMenuNoButton != null)
            {
                confirmMainMenuNoButton.onClick.AddListener(CloseMainMenuConfirmation);
            }

            if (notebookButton != null)
            {
                Button btn = notebookButton.GetComponentInChildren<Button>(true);
                if (btn != null) btn.onClick.AddListener(ToggleNotebookPanel);
            }

            if (suspectFolderButton == null)
            {
                suspectFolderButton = GameObject.Find("SuspectFolderButton")
                                   ?? GameObject.Find("ButtonFOLDER_0")
                                   ?? GameObject.Find("ButtonFOLDER");
            }

            if (suspectFolderButton != null && suspectFolderButton.GetComponent<SuspectFolderButton>() == null)
            {
                Button btn = suspectFolderButton.GetComponentInChildren<Button>(true);
                if (btn != null)
                {
                    btn.onClick.RemoveListener(ToggleSuspectFolderPanel);
                    btn.onClick.AddListener(ToggleSuspectFolderPanel);
                }
            }

            if (suspectFolderPanel == null)
            {
                var found = Object.FindFirstObjectByType<SuspectFolderUI>(FindObjectsInactive.Include);
                if (found != null)
                {
                    suspectFolderPanel = found.gameObject;
                }
                else
                {
#if UNITY_EDITOR
                    var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Panels/Panel_SuspectFolder.prefab");
                    if (prefab != null)
                    {
                        suspectFolderPanel = Instantiate(prefab, transform);
                        suspectFolderPanel.name = "Panel_SuspectFolder";
                        suspectFolderPanel.SetActive(false);
                    }
#endif
                }
            }

            if (deductionBoardButton != null)
            {
                Button btn = deductionBoardButton.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(ToggleDeductionBoardPanel);
            }

            if (concludeCaseButton == null)
            {
                concludeCaseButton = GameObject.Find("ConcludeCaseButton")
                                  ?? GameObject.Find("Button_ConcludeCase")
                                  ?? GameObject.Find("ButtonCONCLUDE_0")
                                  ?? GameObject.Find("ButtonCONCLUDE");

                if (concludeCaseButton == null)
                {
                    foreach (Transform child in GetComponentsInChildren<Transform>(true))
                    {
                        if (child.name.IndexOf("Conclude", System.StringComparison.OrdinalIgnoreCase) >= 0 &&
                            !child.name.Contains("Quiz") && !child.name.Contains("Panel"))
                        {
                            concludeCaseButton = child.gameObject;
                            break;
                        }
                    }
                }
            }

            if (concludeCaseButton != null)
            {
                if (concludeCaseButton.GetComponent<DetectiveButton>() != null)
                {
                    Button btn = concludeCaseButton.GetComponent<Button>();
                    btn.onClick.RemoveListener(HandleConcludeNavigationClicked);
                    btn.onClick.AddListener(HandleConcludeNavigationClicked);
                }
                else
                {
                    var concludeComp = concludeCaseButton.GetComponent<ConcludeCaseButton>() ?? concludeCaseButton.AddComponent<ConcludeCaseButton>();
                    concludeComp.EnsureClickable();

                    Button btn = concludeCaseButton.GetComponentInChildren<Button>(true);
                    if (btn != null)
                    {
                        btn.onClick.RemoveListener(OpenConclusionQuiz);
                        btn.onClick.AddListener(OpenConclusionQuiz);
                    }
                }
            }

            BindNavigationButtons();

            UIPanelType initialPanel = (mainMenuPanel != null) ? UIPanelType.MainMenu : UIPanelType.InvestigationTable;
            ShowPanel(initialPanel);
            RegisterEvents();
            UpdateConclusionButtonState();
            UIButtonHighlightSystem.ApplyToAllButtonsInScene();
        }

        private void BindNavigationButtons()
        {
            returnToMenuButton = BindNavigationControl(returnToMenuButton, ToggleInGameMenu);
            notebookButton = BindNavigationControl(notebookButton, ToggleNotebookPanel);
            concludeCaseButton = BindNavigationControl(concludeCaseButton, HandleConcludeNavigationClicked);
            if (concludeCaseButton != null)
                concludeNavigationButton = concludeCaseButton.GetComponent<Button>();
            deductionBoardButton = BindNavigationControl(deductionBoardButton, ToggleDeductionBoardPanel);

            if (resumeGameButton != null)
                resumeGameButton = BindNavigationControl(resumeGameButton.gameObject, CloseInGameMenu).GetComponent<Button>();
            if (inGameMainMenuButton != null)
                inGameMainMenuButton = BindNavigationControl(inGameMainMenuButton.gameObject, OpenMainMenuConfirmation).GetComponent<Button>();
        }

        private GameObject BindNavigationControl(GameObject original, UnityAction onClick)
        {
            if (original == null) return null;
            Button button = original.GetComponent<Button>();
            if (button == null) return original;
            button.onClick.RemoveListener(onClick);
            button.onClick.AddListener(onClick);
            return original;
        }

        private void HandleConcludeNavigationClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            OpenConclusionQuiz();
        }

        private void OnEnable()
        {
            RegisterEvents();
        }

        private void OnDisable()
        {
            UnregisterEvents();
        }

        private void RegisterEvents()
        {
            EvidenceManager evidenceManager = EvidenceManager.Instance;
            if (_subscribedEvidenceManager != evidenceManager)
            {
                if (_subscribedEvidenceManager != null)
                {
                    _subscribedEvidenceManager.OnInspectModalOpened -= HandleInspectModalOpened;
                    _subscribedEvidenceManager.OnInspectModalClosed -= HandleInspectModalClosed;
                }

                _subscribedEvidenceManager = evidenceManager;
                if (_subscribedEvidenceManager != null)
                {
                    _subscribedEvidenceManager.OnInspectModalOpened += HandleInspectModalOpened;
                    _subscribedEvidenceManager.OnInspectModalClosed += HandleInspectModalClosed;
                }
            }

            CaseManager caseManager = CaseManager.Instance;
            if (_subscribedCaseManager != caseManager)
            {
                if (_subscribedCaseManager != null)
                {
                    _subscribedCaseManager.OnTimeExpired -= HandleTimeExpired;
                    _subscribedCaseManager.OnEvidenceDiscovered -= HandleEvidenceOrDialogueChanged;
                    _subscribedCaseManager.OnDialogueTreeCompleted -= HandleDialogueTreeCompleted;
                    _subscribedCaseManager.OnConclusionReadinessChanged -= HandleConclusionReadinessChanged;
                    _subscribedCaseManager.OnCaseLoaded -= HandleCaseLoaded;
                }

                _subscribedCaseManager = caseManager;
                if (_subscribedCaseManager != null)
                {
                    _subscribedCaseManager.OnTimeExpired += HandleTimeExpired;
                    _subscribedCaseManager.OnEvidenceDiscovered += HandleEvidenceOrDialogueChanged;
                    _subscribedCaseManager.OnDialogueTreeCompleted += HandleDialogueTreeCompleted;
                    _subscribedCaseManager.OnConclusionReadinessChanged += HandleConclusionReadinessChanged;
                    _subscribedCaseManager.OnCaseLoaded += HandleCaseLoaded;
                }
            }

            InterrogationManager interrogationManager = InterrogationManager.Instance;
            if (_subscribedInterrogationManager != interrogationManager)
            {
                if (_subscribedInterrogationManager != null)
                {
                    _subscribedInterrogationManager.OnDialogueClosed -= HandleEvidenceOrDialogueChanged;
                    _subscribedInterrogationManager.OnDialogueNodeDisplayed -= HandleDialogueNodeDisplayed;
                }

                _subscribedInterrogationManager = interrogationManager;
                if (_subscribedInterrogationManager != null)
                {
                    _subscribedInterrogationManager.OnDialogueClosed += HandleEvidenceOrDialogueChanged;
                    _subscribedInterrogationManager.OnDialogueNodeDisplayed += HandleDialogueNodeDisplayed;
                }
            }
        }

        private void UnregisterEvents()
        {
            if (_subscribedEvidenceManager != null)
            {
                _subscribedEvidenceManager.OnInspectModalOpened -= HandleInspectModalOpened;
                _subscribedEvidenceManager.OnInspectModalClosed -= HandleInspectModalClosed;
                _subscribedEvidenceManager = null;
            }

            if (_subscribedCaseManager != null)
            {
                _subscribedCaseManager.OnTimeExpired -= HandleTimeExpired;
                _subscribedCaseManager.OnEvidenceDiscovered -= HandleEvidenceOrDialogueChanged;
                _subscribedCaseManager.OnDialogueTreeCompleted -= HandleDialogueTreeCompleted;
                _subscribedCaseManager.OnConclusionReadinessChanged -= HandleConclusionReadinessChanged;
                _subscribedCaseManager.OnCaseLoaded -= HandleCaseLoaded;
                _subscribedCaseManager = null;
            }

            if (_subscribedInterrogationManager != null)
            {
                _subscribedInterrogationManager.OnDialogueClosed -= HandleEvidenceOrDialogueChanged;
                _subscribedInterrogationManager.OnDialogueNodeDisplayed -= HandleDialogueNodeDisplayed;
                _subscribedInterrogationManager = null;
            }
        }

        private void HandleEvidenceOrDialogueChanged(EvidenceSO evidence)
        {
            UpdateConclusionButtonState();
        }

        private void HandleEvidenceOrDialogueChanged()
        {
            UpdateConclusionButtonState();
        }

        private void HandleDialogueNodeDisplayed(DialogueNode node)
        {
            UpdateConclusionButtonState();
        }

        private void HandleDialogueTreeCompleted(string treeId)
        {
            UpdateConclusionButtonState();
        }

        private void HandleConclusionReadinessChanged(bool isReady)
        {
            UpdateConclusionButtonState();
        }

        private void HandleCaseLoaded(CaseSO caseData)
        {
            UpdateConclusionButtonState();
        }

        private void HandleInspectModalOpened(EvidenceSO evidence)
        {
            ShowPanel(UIPanelType.InspectModal);
        }

        private void HandleInspectModalClosed()
        {
            ShowPanel(UIPanelType.InvestigationTable);
            UpdateConclusionButtonState();
        }

        private void UpdateConclusionButtonState()
        {
            if (concludeCaseButton == null)
            {
                concludeCaseButton = GameObject.Find("ConcludeCaseButton")
                                  ?? GameObject.Find("Button_ConcludeCase")
                                  ?? GameObject.Find("ButtonCONCLUDE_0")
                                  ?? GameObject.Find("ButtonCONCLUDE");

                if (concludeCaseButton == null)
                {
                    foreach (Transform child in GetComponentsInChildren<Transform>(true))
                    {
                        if (child.name.IndexOf("Conclude", System.StringComparison.OrdinalIgnoreCase) >= 0 &&
                            !child.name.Contains("Quiz") && !child.name.Contains("Panel"))
                        {
                            concludeCaseButton = child.gameObject;
                            break;
                        }
                    }
                }
            }

            if (concludeCaseButton == null) return;

            bool isReady = CaseManager.Instance != null && CaseManager.Instance.IsReadyForConclusion();
            var concludeComp = concludeCaseButton.GetComponent<ConcludeCaseButton>();
            if (concludeComp != null)
            {
                concludeComp.UpdateReadinessState();
            }

            Button button = concludeCaseButton.GetComponentInChildren<Button>(true);
            if (button != null)
            {
                button.interactable = isReady;
            }

            if (concludeNavigationButton != null)
            {
                Color color = DetectiveUITheme.MutedInk;
                if (isReady)
                    color = DetectiveUITheme.Ink;
                DetectiveUITheme.Action(concludeNavigationButton, "Conclude case", color);
                concludeNavigationButton.interactable = isReady;
            }

            RegisterEvents();
        }

        private void HandleTimeExpired()
        {
            Debug.Log("[UI:Manager] Received OnTimeExpired event from CaseManager. Showing GameOver overlay.");
            ShowPanel(UIPanelType.GameOver);
        }

        public void ShowPanel(UIPanelType panelType)
        {
            RegisterEvents();

            if (panelType == UIPanelType.InvestigatorSelect)
                panelType = UIPanelType.InvestigationTable;

            Debug.Log($"[UI:Manager] Transitioning panel from '{_currentPanel}' to '{panelType}'");
            _currentPanel = panelType;

            bool isMainMenu = (panelType == UIPanelType.MainMenu);
            bool isInspect = (panelType == UIPanelType.InspectModal);
            bool isGameOver = (panelType == UIPanelType.GameOver);
            bool isResults = (panelType == UIPanelType.ResultsScreen);
            bool isInGameMenu = (panelType == UIPanelType.InGameMenu);

            if (suspectFolderPanel == null)
            {
                var foundGO = Object.FindFirstObjectByType<SuspectFolderUI>(FindObjectsInactive.Include);
                if (foundGO != null)
                {
                    suspectFolderPanel = foundGO.gameObject;
                }
                else if (panelType == UIPanelType.SuspectFolder)
                {
#if UNITY_EDITOR
                    var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Panels/Panel_SuspectFolder.prefab");
                    if (prefab != null)
                    {
                        suspectFolderPanel = Instantiate(prefab, transform);
                        suspectFolderPanel.name = "Panel_SuspectFolder";
                        suspectFolderPanel.SetActive(false);
                    }
#endif
                }
            }

            if (conclusionQuizPanel == null)
            {
                var foundGO = Object.FindFirstObjectByType<ConclusionUI>(FindObjectsInactive.Include);
                if (foundGO != null)
                {
                    conclusionQuizPanel = foundGO.gameObject;
                }
                else if (panelType == UIPanelType.ConclusionQuiz)
                {
#if UNITY_EDITOR
                    var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Panels/Panel_ConclusionQuiz.prefab");
                    if (prefab != null)
                    {
                        conclusionQuizPanel = Instantiate(prefab, transform);
                        conclusionQuizPanel.name = "Panel_ConclusionQuiz";
                        conclusionQuizPanel.SetActive(false);
                    }
#endif
                }
            }

            if (mainMenuPanel != null) mainMenuPanel.SetActive(isMainMenu);
            if (mainTablePanel != null) mainTablePanel.SetActive(!isMainMenu && (panelType == UIPanelType.InvestigationTable || isInGameMenu));
            if (inspectModalPanel != null) inspectModalPanel.SetActive(isInspect);
            if (notebookPanel != null) notebookPanel.SetActive(panelType == UIPanelType.CaseFileNotebook);
            if (suspectFolderPanel != null) suspectFolderPanel.SetActive(panelType == UIPanelType.SuspectFolder);
            if (deductionBoardPanel != null) deductionBoardPanel.SetActive(panelType == UIPanelType.DeductionBoard);
            if (conclusionQuizPanel != null) conclusionQuizPanel.SetActive(panelType == UIPanelType.ConclusionQuiz);
            if (resultsScreenPanel != null) resultsScreenPanel.SetActive(false);
            if (investigatorSelectPanel != null) investigatorSelectPanel.SetActive(false);
            if (inGameMenuPanel != null) inGameMenuPanel.SetActive(isInGameMenu);
            if (mainMenuConfirmPanel != null && !isInGameMenu) mainMenuConfirmPanel.SetActive(false);
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            bool showHeaderNav = !isMainMenu && !isInspect && !isGameOver && !isResults && !isInGameMenu;
            if (timerContainer != null) timerContainer.SetActive(showHeaderNav);
            if (notebookButton != null) notebookButton.SetActive(showHeaderNav);
            if (suspectFolderButton != null) suspectFolderButton.SetActive(showHeaderNav);
            if (deductionBoardButton != null) deductionBoardButton.SetActive(showHeaderNav);
            if (concludeCaseButton != null) concludeCaseButton.SetActive(showHeaderNav);
            if (investigatorSelectButton != null) investigatorSelectButton.SetActive(false);
            if (returnToMenuButton != null) returnToMenuButton.SetActive(showHeaderNav);

            if (isMainMenu || isResults || isGameOver || isInGameMenu)
            {
                CaseManager.Instance?.PauseTimer();
            }
            else
            {
                CaseManager.Instance?.ResumeTimer();
            }

            ArmPointerController.Instance?.ForceSyncState();
            AudioManager.Instance?.PlayPaperFlip();
            UIButtonHighlightSystem.ApplyToHierarchy(gameObject);
            UpdateOutcomeScenes();
        }

        public void ShowResults(CaseEvaluationResult result)
        {
            if (result == null) return;
            pendingResult = result;
            if (result.totalQuizQuestions > 0 && result.correctQuizAnswers == result.totalQuizQuestions)
            {
                CaseManager caseManager = CaseManager.Instance;
                if (caseManager != null && caseManager.ActiveCase != null)
                    CaseClosed.Services.CaseProgressionService.Instance?.SetCaseCompleted(caseManager.ActiveCase.levelNumber, true);
            }
            ShowPanel(UIPanelType.ResultsScreen);
        }

        private string GetRequestedOutcomeScene()
        {
            if (_currentPanel == UIPanelType.ResultsScreen) return ResultsSceneName;
            if (_currentPanel == UIPanelType.GameOver) return GameOverSceneName;
            return null;
        }

        private void UpdateOutcomeScenes()
        {
            string requestedScene = GetRequestedOutcomeScene();
            SetOutcomeSceneVisible(ResultsSceneName, requestedScene == ResultsSceneName);
            SetOutcomeSceneVisible(GameOverSceneName, requestedScene == GameOverSceneName);
            if (!syncingOutcomeScenes)
                StartCoroutine(SyncOutcomeScenes());
        }

        private IEnumerator SyncOutcomeScenes()
        {
            syncingOutcomeScenes = true;
            while (true)
            {
                string requestedScene = GetRequestedOutcomeScene();
                Scene resultsScene = SceneManager.GetSceneByName(ResultsSceneName);
                Scene gameOverScene = SceneManager.GetSceneByName(GameOverSceneName);
                if (resultsScene.isLoaded && requestedScene != ResultsSceneName)
                {
                    yield return SceneManager.UnloadSceneAsync(resultsScene);
                    continue;
                }
                if (gameOverScene.isLoaded && requestedScene != GameOverSceneName)
                {
                    yield return SceneManager.UnloadSceneAsync(gameOverScene);
                    continue;
                }
                if (requestedScene == null) break;
                if (!SceneManager.GetSceneByName(requestedScene).isLoaded)
                {
                    if (!Application.CanStreamedLevelBeLoaded(requestedScene))
                    {
                        Debug.LogError($"[UI:Manager] Outcome scene '{requestedScene}' is missing from the build settings.");
                        break;
                    }
                    yield return SceneManager.LoadSceneAsync(requestedScene, LoadSceneMode.Additive);
                    continue;
                }
                SetOutcomeSceneVisible(requestedScene, true);
                break;
            }
            syncingOutcomeScenes = false;
        }

        private void SetOutcomeSceneVisible(string sceneName, bool visible)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded) return;
            Canvas gameplayCanvas = GetComponent<Canvas>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Canvas canvas = root.GetComponent<Canvas>();
                if (canvas == null) continue;
                if (gameplayCanvas != null) canvas.worldCamera = gameplayCanvas.worldCamera;
                if (canvas.worldCamera == null) canvas.worldCamera = Camera.main;
                if (root.transform.childCount > 0)
                    root.transform.GetChild(0).gameObject.SetActive(visible);
                ResultsScreenUI results = root.GetComponent<ResultsScreenUI>();
                if (visible && results != null) results.Display(pendingResult);
            }
        }

        public void ReturnToMainMenu()
        {
            Debug.Log("[UI:Manager] Returning to Main Menu");
            string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (activeScene != "MainMenu" && activeScene != "Main" && Application.CanStreamedLevelBeLoaded("MainMenu"))
            {
                Debug.Log("[UI:Manager] Loading 'MainMenu' scene...");
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
            else
            {
                ShowPanel(UIPanelType.MainMenu);
                AudioManager.Instance?.PlayMenuBGM();
            }
        }

        public void ToggleInGameMenu()
        {
            if (currentPanel == UIPanelType.InGameMenu)
            {
                CloseInGameMenu();
                return;
            }

            _panelBeforeInGameMenu = currentPanel == UIPanelType.MainMenu
                ? UIPanelType.InvestigationTable
                : currentPanel;
            ShowPanel(UIPanelType.InGameMenu);
        }

        public void CloseInGameMenu()
        {
            if (currentPanel != UIPanelType.InGameMenu) return;
            ShowPanel(_panelBeforeInGameMenu);
        }

        public void OpenMainMenuConfirmation()
        {
            if (currentPanel != UIPanelType.InGameMenu) return;
            if (mainMenuConfirmPanel != null)
            {
                mainMenuConfirmPanel.SetActive(true);
                mainMenuConfirmPanel.transform.SetAsLastSibling();
            }
        }

        public void CloseMainMenuConfirmation()
        {
            if (mainMenuConfirmPanel != null) mainMenuConfirmPanel.SetActive(false);
        }

        public void ConfirmReturnToMainMenu()
        {
            CloseMainMenuConfirmation();
            ReturnToMainMenu();
        }

        public void ToggleInvestigatorSelectPanel()
        {
            ShowPanel(UIPanelType.InvestigationTable);
        }

        public void ToggleNotebookPanel()
        {
            Debug.Log("[UI:Manager] Toggle notebook panel clicked");
            if (currentPanel == UIPanelType.CaseFileNotebook)
                ShowPanel(UIPanelType.InvestigationTable);
            else
                ShowPanel(UIPanelType.CaseFileNotebook);
        }

        public void ToggleSuspectFolderPanel()
        {
            Debug.Log("[UI:Manager] Toggle suspect folder panel clicked");
            if (currentPanel == UIPanelType.SuspectFolder)
                ShowPanel(UIPanelType.InvestigationTable);
            else
                ShowPanel(UIPanelType.SuspectFolder);
        }

        public void ToggleDeductionBoardPanel()
        {
            Debug.Log("[UI:Manager] Toggle deduction board panel clicked");
            if (currentPanel == UIPanelType.DeductionBoard)
                ShowPanel(UIPanelType.InvestigationTable);
            else
                ShowPanel(UIPanelType.DeductionBoard);
        }

        public void OpenConclusionQuiz()
        {
            if (CaseManager.Instance == null || !CaseManager.Instance.IsReadyForConclusion()) return;

            if (conclusionQuizPanel == null)
            {
                var found = Object.FindFirstObjectByType<ConclusionUI>(FindObjectsInactive.Include);
                if (found != null)
                {
                    conclusionQuizPanel = found.gameObject;
                }
                else
                {
#if UNITY_EDITOR
                    var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Panels/Panel_ConclusionQuiz.prefab");
                    if (prefab != null)
                    {
                        conclusionQuizPanel = Instantiate(prefab, transform);
                        conclusionQuizPanel.name = "Panel_ConclusionQuiz";
                    }
#endif
                }
            }

            Debug.Log("[UI:Manager] Open conclusion quiz button clicked — showing ConclusionQuiz panel");
            ShowPanel(UIPanelType.ConclusionQuiz);
        }
    }
}
