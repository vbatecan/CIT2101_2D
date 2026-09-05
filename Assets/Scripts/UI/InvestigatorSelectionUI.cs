using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;
using CaseClosed.Prototype;

namespace CaseClosed.UI
{
    /// <summary>
    /// UI View MonoBehaviour managing the case level selector.
    /// Allows the player to choose which case level (Level 1, Level 2, Level 3) to investigate.
    /// </summary>
    public class InvestigatorSelectionUI : MonoBehaviour
    {

        [Header("Level Select Buttons (Cases 1, 2, 3)")]
        public Button level1Button;
        public Button level2Button;
        public Button level3Button;
        public Text currentLevelStatusText;

        [Header("Navigation Buttons")]
        public Button closeSelectionButton;

        /// <summary>
        /// Binds UI button listeners for level selection and close actions.
        /// </summary>
        private void Start()
        {
            if (level1Button != null) level1Button.onClick.AddListener(() => OnSelectLevel(1));
            if (level2Button != null) level2Button.onClick.AddListener(() => OnSelectLevel(2));
            if (level3Button != null) level3Button.onClick.AddListener(() => OnSelectLevel(3));

            if (closeSelectionButton != null) closeSelectionButton.onClick.AddListener(OnCloseClicked);

            if (CaseManager.Instance != null)
            {
                CaseManager.Instance.OnCaseLoaded += (c) => RefreshUI();
            }

            RefreshUI();
        }

        /// <summary>
        /// Refreshes UI indicators whenever the selection panel is enabled.
        /// </summary>
        private void OnEnable()
        {
            RefreshUI();
        }

        /// <summary>
        /// Refreshes the display text showing the active level.
        /// </summary>
        public void RefreshUI()
        {
            CaseSO activeCase = CaseManager.Instance?.activeCase;

            if (currentLevelStatusText != null)
            {
                int levelNum = activeCase != null ? activeCase.levelNumber : 1;
                string title = activeCase != null ? activeCase.caseTitle : "Level 1: The Missing Necklace";
                currentLevelStatusText.text = $"CURRENT CASE: LEVEL {levelNum} - {title}";
            }
        }

        /// <summary>
        /// Handles loading a specific case level (1, 2, or 3).
        /// </summary>
        /// <param name="levelIndex">The 1-based level index.</param>
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

        /// <summary>
        /// Closes the selection modal and returns to the investigation table.
        /// </summary>
        private void OnCloseClicked()
        {
            UIManager.Instance?.ToggleInvestigatorSelectPanel();
        }
    }
}
