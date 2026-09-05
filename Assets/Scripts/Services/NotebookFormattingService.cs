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
            sb.AppendLine($"<size=22><b>CASE FILE #{activeCase.levelNumber}</b></size>");
            sb.AppendLine($"<size=18><b>{activeCase.caseTitle}</b></size>\n");
            if (activeCase.leadInvestigator != null)
            {
                sb.AppendLine($"<b>Lead Investigator:</b> {activeCase.leadInvestigator.fullName} ({activeCase.leadInvestigator.occupation})");
            }
            sb.AppendLine($"<b>Date & Location:</b> {activeCase.dateAndLocation}");
            sb.AppendLine($"<b>Victim:</b> {activeCase.victimInfo}\n");
            sb.AppendLine($"<size=16><b>[ INCIDENT SUMMARY ]</b></size>\n{activeCase.incidentDescription}\n");
            sb.AppendLine($"<size=16><b>[ CURRENT OBJECTIVE ]</b></size>\n{activeCase.objective}");
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
            sb.AppendLine($"<size=20><b>{profile.fullName.ToUpper()}</b></size> {(isPrimary ? "<color=#991B1B><b>[PRIMARY SUSPECT]</b></color>" : "<color=#475569>[PERSON OF INTEREST]</color>")}");
            sb.AppendLine($"<b>Age:</b> {profile.age}  |  <b>Occupation:</b> {profile.occupation}");
            sb.AppendLine($"<b>Personality:</b> {profile.personalityTrait}");
            sb.AppendLine($"<b>Relationship to Victim:</b> {profile.relationshipToVictim}\n");
            sb.AppendLine($"<b>Alibi:</b>\n<i>\"{profile.alibi}\"</i>\n");
            sb.AppendLine($"<b>Possible Motive:</b>\n{profile.possibleMotives}\n");
            sb.AppendLine($"<b>Known Conflicts:</b>\n{profile.knownConflicts}\n");
            sb.AppendLine("<color=#64748B>────────────────────────────</color>\n");
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
            bool any = false;
            foreach (var ev in activeCase.evidenceItems)
            {
                if (ev != null && discoveredIds != null && discoveredIds.Contains(ev.id))
                {
                    any = true;
                    sb.AppendLine($"<size=18><b>• {ev.evidenceName}</b></size> <color=#475569>[{ev.category}]</color>");
                    sb.AppendLine($"  {ev.baseDescription}");
                    if (ev.isExamined && !string.IsNullOrEmpty(ev.detailedObservation))
                    {
                        sb.AppendLine($"  <color=#0F766E><b>[EXAMINED]:</b> {ev.detailedObservation}</color>");
                    }
                    sb.AppendLine("<color=#94A3B8>────────────────────────────</color>\n");
                }
            }

            if (!any)
            {
                sb.AppendLine("<i>No physical evidence logged yet.\nInspect the crime scene and interrogation desk to discover evidence.</i>");
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
                return "<i>No deduction clues unlocked yet.\nExamine evidence hotspots and confront suspects with contradictions.</i>";
            }

            StringBuilder sb = new StringBuilder();
            foreach (var kvp in unlockedClues)
            {
                sb.AppendLine($"<size=18><b>[CLUE #{kvp.Key}]</b></size>");
                sb.AppendLine($"  {kvp.Value}\n");
                sb.AppendLine("<color=#94A3B8>────────────────────────────</color>\n");
            }

            return sb.ToString();
        }
    }
}
