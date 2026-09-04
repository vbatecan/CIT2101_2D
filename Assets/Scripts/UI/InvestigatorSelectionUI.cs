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
    /// UI View MonoBehaviour managing the detective investigator selection screen and level selector.
    /// Allows the player to choose between the 2 investigator characters (Detective Kyle Gabriel Pastrana and Detective Miguel Borja)
    /// and choose which case level (Level 1, Level 2, Level 3) to investigate.
    /// </summary>
    public class InvestigatorSelectionUI : MonoBehaviour
    {
        [Header("Investigator Selection Buttons")]
        public Button selectKyleButton;
        public Button selectMiguelButton;
        public Text currentInvestigatorStatusText;

        [Header("Investigator Cards Display Text")]
        public Text kyleCardDetailsText;
        public Text miguelCardDetailsText;

        [Header("Level Select Buttons (Cases 1, 2, 3)")]
        public Button level1Button;
        public Button level2Button;
        public Button level3Button;
        public Text currentLevelStatusText;

        [Header("Navigation Buttons")]
        public Button closeSelectionButton;

        /// <summary>
        /// Binds UI button listeners for character picking, level selection, and close actions.
        /// </summary>
        private void Start()
        {
            if (selectKyleButton != null) selectKyleButton.onClick.AddListener(() => OnSelectInvestigatorByIndex(0));
            if (selectMiguelButton != null) selectMiguelButton.onClick.AddListener(() => OnSelectInvestigatorByIndex(1));

            if (level1Button != null) level1Button.onClick.AddListener(() => OnSelectLevel(1));
            if (level2Button != null) level2Button.onClick.AddListener(() => OnSelectLevel(2));
            if (level3Button != null) level3Button.onClick.AddListener(() => OnSelectLevel(3));

            if (closeSelectionButton != null) closeSelectionButton.onClick.AddListener(OnCloseClicked);

            if (CaseManager.Instance != null)
            {
                CaseManager.Instance.OnInvestigatorChanged += (inv) => RefreshUI();
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
        /// Refreshes the display text, highlighting the selected investigator and active level.
        /// </summary>
        public void RefreshUI()
        {
            CharacterProfileSO selected = CaseManager.Instance?.selectedInvestigator;
            CaseSO activeCase = CaseManager.Instance?.activeCase;

            if (currentInvestigatorStatusText != null)
            {
                string invName = selected != null ? selected.fullName : "None";
                string invRole = selected != null ? selected.occupation : "";
                currentInvestigatorStatusText.text = $"ACTIVE INVESTIGATOR: {invName.ToUpper()} ({invRole})";
            }

            if (kyleCardDetailsText != null)
            {
                bool isKyleActive = selected != null && selected.characterId == "CHAR_KYLE_PASTRANA";
                kyleCardDetailsText.text =
                    $"<b>DETECTIVE KYLE GABRIEL PASTRANA</b> {(isKyleActive ? "<color=yellow>[ACTIVE]</color>" : "")}\n" +
                    $"Role: Lead Field Detective | Age: 34\n" +
                    $"Trait: Observant & Direct\n" +
                    $"Specialty: Scene reconstruction, physical evidence analysis, and disproving physical alibis.";
            }

            if (miguelCardDetailsText != null)
            {
                bool isMiguelActive = selected != null && selected.characterId == "CHAR_MIGUEL_BORJA";
                miguelCardDetailsText.text =
                    $"<b>DETECTIVE MIGUEL BORJA</b> {(isMiguelActive ? "<color=yellow>[ACTIVE]</color>" : "")}\n" +
                    $"Role: Lead Digital Forensics Detective | Age: 36\n" +
                    $"Trait: Methodical & Analytical\n" +
                    $"Specialty: Digital logs, cyber forensics, encrypted timestamps, and data trail deductions.";
            }

            if (currentLevelStatusText != null)
            {
                int levelNum = activeCase != null ? activeCase.levelNumber : 1;
                string title = activeCase != null ? activeCase.caseTitle : "Level 1: The Missing Necklace";
                currentLevelStatusText.text = $"CURRENT CASE: LEVEL {levelNum} - {title}";
            }
        }

        /// <summary>
        /// Handles selecting an investigator by index (0 for Kyle, 1 for Miguel).
        /// </summary>
        /// <param name="index">The 0-based index of the investigator.</param>
        private void OnSelectInvestigatorByIndex(int index)
        {
            var list = CaseManager.Instance?.availableInvestigators;
            if (list != null && index >= 0 && index < list.Count)
            {
                CaseManager.Instance.SetSelectedInvestigator(list[index]);
            }
            else
            {
                GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
                if (bootstrap != null)
                {
                    bootstrap.SelectInvestigator(index);
                }
            }

            AudioManager.Instance?.PlayButtonClick();
            RefreshUI();
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
