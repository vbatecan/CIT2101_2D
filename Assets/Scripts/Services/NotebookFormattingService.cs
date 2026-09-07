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
            return FormatCaseSummary(activeCase, activeCase != null ? activeCase.leadInvestigator : null);
        }

        /// <summary>
        /// Formats a case summary using the investigator effective for the current session without
        /// changing the investigator reference authored on the case asset.
        /// </summary>
        public string FormatCaseSummary(CaseSO activeCase, CharacterProfileSO effectiveInvestigator)
        {
            if (activeCase == null) return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<size=22><b>CASE FILE #{activeCase.levelNumber}</b></size>");
            sb.AppendLine($"<size=18><b>{activeCase.caseTitle}</b></size>\n");
            if (effectiveInvestigator != null)
            {
                sb.AppendLine($"<b>Lead Investigator:</b> {effectiveInvestigator.fullName} ({effectiveInvestigator.occupation})");
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
        public string FormatDiscoveredEvidence(CaseSO activeCase, IReadOnlyCollection<string> discoveredIds)
        {
            return FormatDiscoveredEvidence(activeCase, discoveredIds, null);
        }

        /// <summary>
        /// Formats discovered evidence from an explicit session-state examination predicate.
        /// Supplying the predicate prevents this presentation service from reading legacy runtime
        /// flags retained on authored evidence assets for serialized-data compatibility.
        /// </summary>
        public string FormatDiscoveredEvidence(
            CaseSO activeCase,
            IReadOnlyCollection<string> discoveredIds,
            System.Func<EvidenceSO, bool> isEvidenceExamined)
        {
            if (activeCase == null || activeCase.evidenceItems == null) return string.Empty;

            StringBuilder sb = new StringBuilder();
            bool any = false;
            foreach (var ev in activeCase.evidenceItems)
            {
                if (ev != null && Contains(discoveredIds, ev.id))
                {
                    any = true;
                    sb.AppendLine($"<size=18><b>• {ev.evidenceName}</b></size> <color=#475569>[{ev.category}]</color>");
                    sb.AppendLine($"  {ev.baseDescription}");
                    bool examined = isEvidenceExamined != null ? isEvidenceExamined(ev) : ev.isExamined;
                    if (examined && !string.IsNullOrEmpty(ev.detailedObservation))
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
        /// Formats a complete single evidence dossier entry for the right-hand lined page of the notebook.
        /// If discovered, reveals full category, description, examination findings, and unlocked clues.
        /// If undiscovered / locked, presents a redacted placeholder with guidance to keep investigating.
        /// </summary>
        /// <param name="evidence">The evidence item to format.</param>
        /// <param name="isDiscovered">Whether the player has discovered this evidence item.</param>
        /// <param name="index">Current 1-based index (e.g. 1).</param>
        /// <param name="totalCount">Total evidence count (e.g. 3).</param>
        /// <returns>Rich text formatted dossier entry.</returns>
        public string FormatEvidenceDossier(EvidenceSO evidence, bool isDiscovered, int index = 0, int totalCount = 0)
        {
            int legacyDiscoveredHotspotCount = 0;
            if (evidence != null && evidence.hotspots != null)
            {
                foreach (EvidenceHotspot hotspot in evidence.hotspots)
                {
                    if (hotspot != null && hotspot.isDiscovered) legacyDiscoveredHotspotCount++;
                }
            }

            return FormatEvidenceDossier(
                evidence,
                isDiscovered,
                evidence != null && evidence.isExamined,
                legacyDiscoveredHotspotCount,
                index,
                totalCount);
        }

        /// <summary>
        /// Formats an evidence dossier from explicit runtime session state rather than mutable
        /// fields on the authored evidence object.
        /// </summary>
        public string FormatEvidenceDossier(
            EvidenceSO evidence,
            bool isDiscovered,
            bool isExamined,
            int discoveredHotspotCount,
            int index,
            int totalCount)
        {
            if (evidence == null)
            {
                return "<i>No evidence selected.</i>";
            }

            StringBuilder sb = new StringBuilder();
            string countHeader = totalCount > 0 ? $"<color=#64748B>[EVIDENCE {index} OF {totalCount}]</color>\n" : string.Empty;

            if (isDiscovered)
            {
                sb.AppendLine(countHeader + $"<size=20><b>{evidence.evidenceName.ToUpper()}</b></size>");
                sb.AppendLine($"<color=#1E293B><b>Category:</b></color> <color=#475569>[{evidence.category}]</color>");
                sb.AppendLine($"<color=#1E293B><b>Status:</b></color> <color=#166534><b>LOGGED & VERIFIED</b></color>\n");

                sb.AppendLine("<size=15><b>[ PHYSICAL DESCRIPTION ]</b></size>");
                sb.AppendLine(evidence.baseDescription + "\n");

                if (isExamined && !string.IsNullOrEmpty(evidence.detailedObservation))
                {
                    sb.AppendLine("<size=15><b>[ EXAMINATION FINDINGS ]</b></size>");
                    sb.AppendLine($"<color=#0F766E>{evidence.detailedObservation}</color>\n");
                }
                else
                {
                    sb.AppendLine("<size=15><b>[ EXAMINATION FINDINGS ]</b></size>");
                    sb.AppendLine("<i>Item has not been closely examined on the table yet. Double-click or inspect to reveal forensic details.</i>\n");
                }

                if (!string.IsNullOrEmpty(evidence.unlockedClueText))
                {
                    sb.AppendLine("<size=15><b>[ UNLOCKED CLUE & DEDUCTION ]</b></size>");
                    sb.AppendLine($"<color=#B45309>{evidence.unlockedClueText}</color>\n");
                }

                if (evidence.hotspots != null && evidence.hotspots.Count > 0)
                {
                    sb.AppendLine($"<b>Forensic Hotspots Discovered:</b> {discoveredHotspotCount} / {evidence.hotspots.Count}");
                }
            }
            else
            {
                sb.AppendLine(countHeader + "<size=20><b>[ ??? UNDISCOVERED EVIDENCE ]</b></size>");
                sb.AppendLine("<color=#1E293B><b>Category:</b></color> <color=#64748B>[CLASSIFIED]</color>");
                sb.AppendLine("<color=#1E293B><b>Status:</b></color> <color=#991B1B><b>NOT YET RECOVERED</b></color>\n");

                sb.AppendLine("<size=15><b>[ EVIDENCE STATUS ]</b></size>");
                sb.AppendLine("<i>This piece of evidence has not yet been discovered by the detective.\n\nExplore the crime scene, interrogate the suspects, and inspect contradictory statements to uncover this item.</i>\n");
            }

            sb.AppendLine("<color=#94A3B8>────────────────────────────</color>");
            return sb.ToString();
        }

        /// <summary>
        /// Formats unlocked clues and synthesized deduction notes into a readable log.
        /// </summary>
        /// <param name="unlockedClues">Dictionary of clue IDs and their unlocked description text.</param>
        /// <returns>A formatted string of all unlocked clues, or an empty prompt message.</returns>
        public string FormatUnlockedClues(IReadOnlyDictionary<string, string> unlockedClues)
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

        private static bool Contains(IReadOnlyCollection<string> values, string value)
        {
            if (values == null) return false;

            foreach (string candidate in values)
            {
                if (candidate == value) return true;
            }
            return false;
        }
    }
}
