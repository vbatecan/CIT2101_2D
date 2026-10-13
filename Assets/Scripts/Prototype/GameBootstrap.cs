using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Gameplay;
using CaseClosed.Managers;
using CaseClosed.UI;

namespace CaseClosed.Prototype
{
    public class GameBootstrap : MonoBehaviour
    {
        public bool startOnMainMenu = true;

        [Header("Authored Case Data Assets (Optional)")]
        [Tooltip("Pre-configured Case 01 ScriptableObject asset (e.g. Case01_Data.asset). If assigned, loaded directly.")]
        [SerializeField] private CaseSO _case01Asset;
        [Tooltip("Pre-configured Case 02 ScriptableObject asset (e.g. Case02_Data.asset). If assigned, loaded directly.")]
        [SerializeField] private CaseSO _case02Asset;
        [Tooltip("Pre-configured Case 03 ScriptableObject asset. If assigned, loaded directly.")]
        [SerializeField] private CaseSO _case03Asset;

        public CaseSO Case01Asset { get => _case01Asset; set => _case01Asset = value; }
        public CaseSO Case02Asset { get => _case02Asset; set => _case02Asset = value; }
        public CaseSO Case03Asset { get => _case03Asset; set => _case03Asset = value; }

        private Case01Initializer level1;
        private Case02Initializer level2;
        private Case03Initializer level3;

        private CharacterProfileSO investigatorKyle;

        private void Awake()
        {
            Debug.Log("[Prototype:Bootstrap] GameBootstrap initializing core managers, investigators, and systems...");

            SetupFixedCamera();
            EnsureManager<AudioManager>();
            EnsureManager<CaseManager>();
            EnsureManager<EvidenceManager>();
            EnsureManager<InterrogationManager>();
            EnsureManager<DeductionBoardController>();
            EnsureManager<CaseConclusionManager>();
            EnsureManager<UIManager>();

            SetupInvestigators();

            level1 = gameObject.GetComponent<Case01Initializer>() ?? FindFirstObjectByType<Case01Initializer>();
            level2 = gameObject.GetComponent<Case02Initializer>() ?? FindFirstObjectByType<Case02Initializer>();
            level3 = gameObject.GetComponent<Case03Initializer>() ?? FindFirstObjectByType<Case03Initializer>();

            Debug.Log("[Prototype:Bootstrap] All managers and investigators initialized. Ready for level loading.");
        }

        private void SetupInvestigators()
        {
            investigatorKyle = ScriptableObject.CreateInstance<CharacterProfileSO>();
            investigatorKyle.characterId = "CHAR_KYLE_PASTRANA";
            investigatorKyle.fullName = "Detective Kyle Gabriel Pastrana";
            investigatorKyle.age = 34;
            investigatorKyle.occupation = "Lead Field Detective";
            investigatorKyle.personalityTrait = PersonalityTrait.Observant;
            investigatorKyle.background = "Veteran lead field detective with sharp intuition for physical clues, crime scenes, and catching suspect contradictions.";

            if (CaseManager.Instance != null)
            {
                CaseManager.Instance.RegisterAvailableInvestigator(investigatorKyle);
                CaseManager.Instance.SetSelectedInvestigator(investigatorKyle);
            }
        }

        private void Start()
        {
            string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Debug.Log($"[Prototype:Bootstrap] GameBootstrap Start in scene '{activeScene}', startOnMainMenu={startOnMainMenu}");

            if (activeScene == "MainMenu" || (startOnMainMenu && activeScene != "Case001" && activeScene != "Case002" && activeScene != "Case003"))
            {
                UIManager.Instance?.ShowPanel(UIPanelType.MainMenu);
                AudioManager.Instance?.PlayMenuBGM();
                return;
            }

            int targetLevel = 1;
            if (activeScene.Contains("002") || activeScene.Contains("Case2") || activeScene.Contains("Case02"))
            {
                targetLevel = 2;
            }
            else if (activeScene.Contains("003") || activeScene.Contains("Case3") || activeScene.Contains("Case03"))
            {
                targetLevel = 3;
            }
            else
            {
                targetLevel = PlayerPrefs.GetInt("CaseClosed_SelectedLevel", 1);
            }

            if (CaseManager.Instance != null && CaseManager.Instance.ActiveCase != null && CaseManager.Instance.ActiveCase.levelNumber == targetLevel)
            {
                CaseSO activeCase = CaseManager.Instance.ActiveCase;
                if (!CaseManager.Instance.HasStartedSession)
                {
                    CaseManager.Instance.LoadCase(activeCase);
                }
                EnsureInterrogationStarted(activeCase);
                UIManager.Instance?.ShowPanel(UIPanelType.InvestigationTable);
                ArmPointerController.Instance?.ForceSyncState();
                return;
            }

            LoadLevel(targetLevel);
        }

        private void EnsureInterrogationStarted(CaseSO caseData)
        {
            InterrogationManager interrogation = InterrogationManager.Instance;
            if (interrogation == null || caseData == null || caseData.primarySuspect == null ||
                caseData.dialogueTrees == null || caseData.dialogueTrees.Count == 0)
            {
                return;
            }

            if (interrogation.CurrentSuspect == null || interrogation.CurrentDialogueTree == null)
            {
                interrogation.SetInterrogationTarget(caseData.primarySuspect, caseData.dialogueTrees[0]);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToCaseSceneOrLevel(1);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToCaseSceneOrLevel(2);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToCaseSceneOrLevel(3);
            else if (Input.GetKeyDown(KeyCode.M))
            {
                UIManager.Instance?.ToggleInGameMenu();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (EvidenceManager.Instance != null && EvidenceManager.Instance.isInspectingModalOpen)
                {
                }
                else if (UIManager.Instance != null && UIManager.Instance.currentPanel != UIPanelType.InvestigationTable && UIManager.Instance.currentPanel != UIPanelType.MainMenu)
                {
                    UIManager.Instance.ShowPanel(UIPanelType.InvestigationTable);
                }
                else
                {
                    UIManager.Instance?.ToggleInGameMenu();
                }
            }
        }

        public void SwitchToCaseSceneOrLevel(int caseIndex)
        {
            string sceneName = $"Case00{caseIndex}";
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.Log($"[Prototype:Bootstrap] Loading scene '{sceneName}'...");
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
            else
            {
                LoadLevel(caseIndex);
            }
        }

        public void LoadLevel(int levelIndex)
        {
            Debug.Log($"[Prototype:Bootstrap] Shortcut triggered: Loading Level {levelIndex}...");

            CaseClosed.Data.CaseSO caseData = null;

            switch (levelIndex)
            {
                case 1:
                    if (_case01Asset != null)
                    {
                        caseData = _case01Asset;
                    }
                    else
                    {
                        if (level1 == null) level1 = gameObject.GetComponent<Case01Initializer>() ?? FindFirstObjectByType<Case01Initializer>() ?? gameObject.AddComponent<Case01Initializer>();
                        caseData = level1 != null ? (level1.CaseDataAsset ?? level1.CreateCase01Data()) : null;
                    }
                    break;
                case 2:
                    if (_case02Asset != null)
                    {
                        caseData = _case02Asset;
                    }
                    else
                    {
#if UNITY_EDITOR
                        _case02Asset = UnityEditor.AssetDatabase.LoadAssetAtPath<CaseSO>("Assets/Data/Case002/Case02_Data.asset");
                        if (_case02Asset != null)
                        {
                            caseData = _case02Asset;
                            break;
                        }
#endif
                        if (level2 == null) level2 = gameObject.GetComponent<Case02Initializer>() ?? FindFirstObjectByType<Case02Initializer>() ?? gameObject.AddComponent<Case02Initializer>();
                        caseData = level2 != null ? (level2.CaseDataAsset ?? level2.CreateCase02Data()) : null;
                    }
                    break;
                case 3:
                    if (_case03Asset != null)
                    {
                        caseData = _case03Asset;
                    }
                    else
                    {
#if UNITY_EDITOR
                        _case03Asset = UnityEditor.AssetDatabase.LoadAssetAtPath<CaseSO>("Assets/Data/Case003/Case03_Data.asset");
                        if (_case03Asset != null)
                        {
                            caseData = _case03Asset;
                            break;
                        }
#endif
                        if (level3 == null) level3 = gameObject.GetComponent<Case03Initializer>() ?? FindFirstObjectByType<Case03Initializer>() ?? gameObject.AddComponent<Case03Initializer>();
                        caseData = level3 != null ? (level3.CaseDataAsset ?? level3.CreateCase03Data()) : null;
                    }
                    break;
            }

            if (caseData != null)
            {
                CaseManager.Instance?.LoadCase(caseData);
                if (InterrogationManager.Instance != null && caseData.primarySuspect != null && caseData.dialogueTrees != null && caseData.dialogueTrees.Count > 0)
                {
                    InterrogationManager.Instance.SetInterrogationTarget(caseData.primarySuspect, caseData.dialogueTrees[0]);
                }
                UIManager.Instance?.ShowPanel(UIPanelType.InvestigationTable);
                ArmPointerController.Instance?.ForceSyncState();
            }
            else
            {
                Debug.LogWarning($"[Prototype:Bootstrap] Failed to generate case data for level {levelIndex}");
            }
        }

        private void SetupFixedCamera()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                camObj.tag = "MainCamera";
                mainCam = camObj.GetComponent<Camera>();
            }

            if (mainCam.GetComponent<FixedInvestigationCamera>() == null)
            {
                mainCam.gameObject.AddComponent<FixedInvestigationCamera>();
            }

        }

        private T EnsureManager<T>() where T : MonoBehaviour
        {
            T manager = FindFirstObjectByType<T>();
            if (manager == null)
            {
                GameObject managerObj = new GameObject(typeof(T).Name);
                manager = managerObj.AddComponent<T>();
            }
            return manager;
        }
    }
}
