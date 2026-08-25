using System.Text;
using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    public class CaseFileNotebookUI : MonoBehaviour
    {
        public enum NotebookTab { CaseSummary, Suspects, Evidence, Clues }

        [Header("Tab Buttons")]
        public Button summaryTabButton;
        public Button suspectsTabButton;
        public Button evidenceTabButton;
        public Button cluesTabButton;
        public Button closeNotebookButton;

        [Header("Content Container Text")]
        public Text notebookTitleText;
        public Text notebookContentBody;

        private NotebookTab currentTab = NotebookTab.CaseSummary;

        private void Start()
        {
            if (summaryTabButton != null) summaryTabButton.onClick.AddListener(() => SwitchTab(NotebookTab.CaseSummary));
            if (suspectsTabButton != null) suspectsTabButton.onClick.AddListener(() => SwitchTab(NotebookTab.Suspects));
            if (evidenceTabButton != null) evidenceTabButton.onClick.AddListener(() => SwitchTab(NotebookTab.Evidence));
            if (cluesTabButton != null) cluesTabButton.onClick.AddListener(() => SwitchTab(NotebookTab.Clues));
            if (closeNotebookButton != null) closeNotebookButton.onClick.AddListener(OnCloseClicked);
        }

        private void OnEnable()
        {
            SwitchTab(currentTab);
        }

        public void SwitchTab(NotebookTab tab)
        {
            currentTab = tab;
            CaseSO activeCase = CaseManager.Instance?.activeCase;
            if (activeCase == null) return;

            StringBuilder sb = new StringBuilder();

            switch (tab)
            {
                case NotebookTab.CaseSummary:
                    if (notebookTitleText != null) notebookTitleText.text = activeCase.caseTitle;
                    sb.AppendLine($"Date & Location: {activeCase.dateAndLocation}");
                    sb.AppendLine($"Victim: {activeCase.victimInfo}\n");
                    sb.AppendLine($"[INCIDENT SUMMARY]\n{activeCase.incidentDescription}\n");
                    sb.AppendLine($"[OBJECTIVE]\n{activeCase.objective}");
                    break;

                case NotebookTab.Suspects:
                    if (notebookTitleText != null) notebookTitleText.text = "Suspect Profiles";
                    AppendSuspectProfile(sb, activeCase.primarySuspect, true);
                    if (activeCase.additionalSuspects != null)
                    {
                        foreach (var suspect in activeCase.additionalSuspects)
                        {
                            AppendSuspectProfile(sb, suspect, false);
                        }
                    }
                    break;

                case NotebookTab.Evidence:
                    if (notebookTitleText != null) notebookTitleText.text = "Discovered Evidence";
                    var discovered = CaseManager.Instance?.discoveredEvidenceIds;
                    foreach (var ev in activeCase.evidenceItems)
                    {
                        if (discovered != null && discovered.Contains(ev.id))
                        {
                            sb.AppendLine($"• {ev.evidenceName} [{ev.category}]");
                            sb.AppendLine($"  {ev.baseDescription}");
                            if (ev.isExamined) sb.AppendLine($"  [EXAMINED]: {ev.detailedObservation}");
                            sb.AppendLine();
                        }
                    }
                    break;

                case NotebookTab.Clues:
                    if (notebookTitleText != null) notebookTitleText.text = "Unlocked Clues & Deductions";
                    var cluesDict = CaseManager.Instance?.unlockedCluesText;
                    if (cluesDict != null && cluesDict.Count > 0)
                    {
                        foreach (var kvp in cluesDict)
                        {
                            sb.AppendLine($"[CLUE #{kvp.Key}]");
                            sb.AppendLine($"{kvp.Value}\n");
                        }
                    }
                    else
                    {
                        sb.AppendLine("No clues unlocked yet. Examine evidence and interrogate suspects.");
                    }
                    break;
            }

            if (notebookContentBody != null) notebookContentBody.text = sb.ToString();
            AudioManager.Instance?.PlayPaperFlip();
        }

        private void AppendSuspectProfile(StringBuilder sb, CharacterProfileSO profile, bool isPrimary)
        {
            if (profile == null) return;
            sb.AppendLine($"=== {profile.fullName.ToUpper()} {(isPrimary ? "(PRIMARY SUSPECT)" : "")} ===");
            sb.AppendLine($"Age: {profile.age} | Occupation: {profile.occupation}");
            sb.AppendLine($"Personality: {profile.personalityTrait}");
            sb.AppendLine($"Relationship to Victim: {profile.relationshipToVictim}");
            sb.AppendLine($"Alibi: {profile.alibi}");
            sb.AppendLine($"Possible Motive: {profile.possibleMotives}");
            sb.AppendLine($"Known Conflicts: {profile.knownConflicts}\n");
        }

        private void OnCloseClicked()
        {
            UIManager.Instance?.ToggleNotebookPanel();
        }
    }
}
