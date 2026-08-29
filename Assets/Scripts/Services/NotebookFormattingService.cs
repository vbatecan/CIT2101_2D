using System.Collections.Generic;
using System.Text;
using CaseClosed.Data;

namespace CaseClosed.Services
{
    /// <summary>
    /// Service responsible for compiling and formatting case data, suspect dossiers,
    /// evidence records, and deduction logs into clean text strings for the notebook UI.
    /// </summary>
    public class NotebookFormattingService
    {
        /// <summary>
        /// Formats the overview, level designation, assigned investigator, victim, summary, and objective of a case.
        /// </summary>
        /// <param name="activeCase">The case data to format.</param>
        /// <returns>A formatted synopsis string.</returns>
        public string FormatCaseSummary(CaseSO activeCase)
        {
            if (activeCase == null) return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"[LEVEL {activeCase.levelNumber}] - {activeCase.caseTitle}");
            if (activeCase.leadInvestigator != null)
            {
                sb.AppendLine($"Lead Investigator: {activeCase.leadInvestigator.fullName} ({activeCase.leadInvestigator.occupation})");
            }
            sb.AppendLine($"Date & Location: {activeCase.dateAndLocation}");
            sb.AppendLine($"Victim: {activeCase.victimInfo}\n");
            sb.AppendLine($"[INCIDENT SUMMARY]\n{activeCase.incidentDescription}\n");
            sb.AppendLine($"[OBJECTIVE]\n{activeCase.objective}");
            return sb.ToString();
        }

        /// <summary>
        /// Formats the dossier profiles of the primary suspect and all secondary suspects in a case.
        /// </summary>
        /// <param name="activeCase">The case containing suspect profiles.</param>
        /// <returns>A formatted dossier string detailing all suspects.</returns>
        public string FormatSuspectProfiles(CaseSO activeCase)
        {
            if (activeCase == null) return string.Empty;

            StringBuilder sb = new StringBuilder();
            if (activeCase.primarySuspect != null)
            {
                sb.Append(FormatSuspectProfile(activeCase.primarySuspect, true));
            }

            if (activeCase.additionalSuspects != null)
            {
                foreach (var suspect in activeCase.additionalSuspects)
                {
                    if (suspect != null)
                    {
                        sb.Append(FormatSuspectProfile(suspect, false));
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats an individual suspect's background, personality, alibi, and motives.
        /// </summary>
        /// <param name="profile">The character profile to format.</param>
        /// <param name="isPrimary">Whether the character is the primary suspect in the investigation.</param>
        /// <returns>A formatted dossier entry string.</returns>
        public string FormatSuspectProfile(CharacterProfileSO profile, bool isPrimary)
        {
            if (profile == null) return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"=== {profile.fullName.ToUpper()} {(isPrimary ? "(PRIMARY SUSPECT)" : "")} ===");
            sb.AppendLine($"Age: {profile.age} | Occupation: {profile.occupation}");
            sb.AppendLine($"Personality: {profile.personalityTrait}");
            sb.AppendLine($"Relationship to Victim: {profile.relationshipToVictim}");
            sb.AppendLine($"Alibi: {profile.alibi}");
            sb.AppendLine($"Possible Motive: {profile.possibleMotives}");
            sb.AppendLine($"Known Conflicts: {profile.knownConflicts}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Formats the list of evidence discovered by the player along with examination notes.
        /// </summary>
        /// <param name="activeCase">The active case containing evidence definitions.</param>
        /// <param name="discoveredIds">Set of evidence IDs that have been found by the player.</param>
        /// <returns>A formatted list of discovered evidence items.</returns>
        public string FormatDiscoveredEvidence(CaseSO activeCase, HashSet<string> discoveredIds)
        {
            if (activeCase == null || activeCase.evidenceItems == null) return string.Empty;

            StringBuilder sb = new StringBuilder();
            foreach (var ev in activeCase.evidenceItems)
            {
                if (ev != null && discoveredIds != null && discoveredIds.Contains(ev.id))
                {
                    sb.AppendLine($"• {ev.evidenceName} [{ev.category}]");
                    sb.AppendLine($"  {ev.baseDescription}");
                    if (ev.isExamined && !string.IsNullOrEmpty(ev.detailedObservation))
                    {
                        sb.AppendLine($"  [EXAMINED]: {ev.detailedObservation}");
                    }
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats unlocked clues and synthesized deduction notes into a readable log.
        /// </summary>
        /// <param name="unlockedClues">Dictionary of clue IDs and their unlocked description text.</param>
        /// <returns>A formatted string of all unlocked clues, or an empty prompt message.</returns>
        public string FormatUnlockedClues(Dictionary<string, string> unlockedClues)
        {
            if (unlockedClues == null || unlockedClues.Count == 0)
            {
                return "No clues unlocked yet. Examine evidence and interrogate suspects.";
            }

            StringBuilder sb = new StringBuilder();
            foreach (var kvp in unlockedClues)
            {
                sb.AppendLine($"[CLUE #{kvp.Key}]");
                sb.AppendLine($"{kvp.Value}\n");
            }

            return sb.ToString();
        }
    }
}
