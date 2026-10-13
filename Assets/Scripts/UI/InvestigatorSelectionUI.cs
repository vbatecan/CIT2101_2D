using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;
using CaseClosed.Prototype;

namespace CaseClosed.UI
{
    public class InvestigatorSelectionUI : MonoBehaviour
    {

        [Header("Level Select Buttons (Cases 1, 2, 3)")]
        public Button level1Button;
        public Button level2Button;
        public Button level3Button;
        public Text currentLevelStatusText;

        [Header("Navigation Buttons")]
        public Button closeSelectionButton;

        private void Start()
        {
            if (level1Button != null)
            {
                level1Button.onClick.AddListener(() => OnSelectLevel(1));
                DetectiveUITheme.Action(level1Button, "CASE 01", DetectiveUITheme.Ink);
            }
            if (level2Button != null)
            {
                level2Button.onClick.AddListener(() => OnSelectLevel(2));
                DetectiveUITheme.Action(level2Button, "CASE 02", DetectiveUITheme.Ink);
            }
            if (level3Button != null)
            {
                level3Button.onClick.AddListener(() => OnSelectLevel(3));
                DetectiveUITheme.Action(level3Button, "CASE 03", DetectiveUITheme.Ink);
            }

            if (closeSelectionButton != null)
            {
                closeSelectionButton.onClick.AddListener(OnCloseClicked);
                DetectiveUITheme.Action(closeSelectionButton, "CLOSE", DetectiveUITheme.Ink);
            }

            if (CaseManager.Instance != null)
            {
                CaseManager.Instance.OnCaseLoaded += (c) => RefreshUI();
            }

            RefreshUI();
            UIButtonHighlightSystem.ApplyToHierarchy(gameObject);
        }

        private void OnEnable()
        {
            RefreshUI();
        }

        public void RefreshUI()
        {
            CaseSO activeCase = CaseManager.Instance?.ActiveCase;

            if (currentLevelStatusText != null)
            {
                int levelNum = activeCase != null ? activeCase.levelNumber : 1;
                string title = activeCase != null ? activeCase.caseTitle : "Level 1: The Missing Necklace";
                currentLevelStatusText.text = $"CURRENT CASE: LEVEL {levelNum} - {title}";
            }
        }

        private void OnSelectLevel(int levelIndex)
        {
            GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
            if (bootstrap != null)
            {
                bootstrap.SwitchToCaseSceneOrLevel(levelIndex);
            }
            else
            {
                string sceneName = $"Case00{levelIndex}";
                if (Application.CanStreamedLevelBeLoaded(sceneName))
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
                }
            }
            RefreshUI();
            UIManager.Instance?.ShowPanel(UIPanelType.InvestigationTable);
        }

        private void OnCloseClicked()
        {
            UIManager.Instance?.ToggleInvestigatorSelectPanel();
        }
    }
}
