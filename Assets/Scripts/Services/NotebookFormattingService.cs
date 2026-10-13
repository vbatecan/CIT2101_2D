using System.Collections.Generic;
using System.Text;
using CaseClosed.Data;

namespace CaseClosed.Services
{
    public class NotebookFormattingService
    {
        private const string PageDivider = "<color=#8C7B6B>────────────────────────────────────────────</color>";
        private const string SubtleDivider = "<color=#A69B8F>────────────────────────────</color>";

        public string FormatCaseSummary(CaseSO activeCase)
        {
            return FormatCaseSummary(activeCase, activeCase != null ? activeCase.leadInvestigator : null);
        }

        public string FormatCaseSummary(CaseSO activeCase, CharacterProfileSO effectiveInvestigator)
        {
            if (activeCase == null) return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<size=22><b>CASE FILE #{activeCase.levelNumber:00} : {activeCase.caseTitle.ToUpper()}</b></size>");
            sb.AppendLine(PageDivider);
            if (effectiveInvestigator != null)
            {
                sb.AppendLine($"<b>LEAD INVESTIGATOR:</b> {effectiveInvestigator.fullName.ToUpper()} <color=#5C4033>[{effectiveInvestigator.occupation.ToUpper()}]</color>");
            }
            sb.AppendLine($"<b>DATE & LOCATION:</b> {activeCase.dateAndLocation}");
            sb.AppendLine($"<b>VICTIM:</b> {activeCase.victimInfo}");
            sb.AppendLine(PageDivider + "\n");
            sb.AppendLine($"<size=16><b>[ INCIDENT REPORT ]</b></size>\n{activeCase.incidentDescription}\n");
            sb.AppendLine($"<size=16><b>[ INVESTIGATION OBJECTIVE ]</b></size>\n{activeCase.objective}\n");
            sb.AppendLine(PageDivider);
            return sb.ToString();
        }

        public string FormatSuspectProfiles(CaseSO activeCase)
        {
            if (activeCase == null) return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<size=20><b>PERSONS OF INTEREST & DOSSIERS</b></size>");
            sb.AppendLine(PageDivider + "\n");

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

        public string FormatSuspectProfile(CharacterProfileSO profile, bool isPrimary)
        {
            if (profile == null) return string.Empty;

            StringBuilder sb = new StringBuilder();
            string roleBadge = isPrimary
                ? "<color=#8B0000><b>[★ PRIMARY SUSPECT]</b></color>"
                : "<color=#4A3B32><b>[PERSON OF INTEREST]</b></color>";

            sb.AppendLine($"<size=19><b>{profile.fullName.ToUpper()}</b></size> {roleBadge}");
            sb.AppendLine($"<b>AGE:</b> {profile.age}   |   <b>OCCUPATION:</b> {profile.occupation}");
            sb.AppendLine($"<b>BEHAVIORAL TRAIT:</b> {profile.personalityTrait}");
            sb.AppendLine($"<b>RELATION TO VICTIM:</b> {profile.relationshipToVictim}\n");

            sb.AppendLine("<size=15><b>[ RECORDED ALIBI ]</b></size>");
            sb.AppendLine($"<i>\"{profile.alibi}\"</i>\n");

            sb.AppendLine("<size=15><b>[ SUSPECT MOTIVE ]</b></size>");
            sb.AppendLine($"{profile.possibleMotives}\n");

            sb.AppendLine("<size=15><b>[ KNOWN CONFLICTS ]</b></size>");
            sb.AppendLine($"{profile.knownConflicts}\n");

            sb.AppendLine(PageDivider + "\n");
            return sb.ToString();
        }

        public string FormatDiscoveredEvidence(CaseSO activeCase, IReadOnlyCollection<string> discoveredIds)
        {
            return FormatDiscoveredEvidence(activeCase, discoveredIds, null);
        }

        public string FormatDiscoveredEvidence(
            CaseSO activeCase,
            IReadOnlyCollection<string> discoveredIds,
            System.Func<EvidenceSO, bool> isEvidenceExamined)
        {
            if (activeCase == null || activeCase.evidenceItems == null) return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<size=20><b>PHYSICAL EVIDENCE CATALOG</b></size>");
            sb.AppendLine(PageDivider + "\n");

            bool any = false;
            foreach (var ev in activeCase.evidenceItems)
            {
                if (ev != null && Contains(discoveredIds, ev.id))
                {
                    any = true;
                    bool examined = isEvidenceExamined != null ? isEvidenceExamined(ev) : ev.isExamined;
                    string statusTag = examined
                        ? "<color=#1B4D3E><b>[✓ EXAMINED]</b></color>"
                        : "<color=#8B5A2B><b>[● DISCOVERED]</b></color>";

                    sb.AppendLine($"<size=18><b>• {ev.evidenceName}</b></size> <color=#5C4033>[{ev.category}]</color> {statusTag}");
                    sb.AppendLine($"  {ev.baseDescription}");
                    if (examined && !string.IsNullOrEmpty(ev.detailedObservation))
                    {
                        sb.AppendLine($"  <color=#1B4D3E><b>[FORENSIC FINDINGS]:</b> {ev.detailedObservation}</color>");
                    }
                    sb.AppendLine(SubtleDivider + "\n");
                }
            }

            if (!any)
            {
                sb.AppendLine("<i>No physical evidence logged yet.\nInspect the crime scene and interrogation desk to discover evidence.</i>");
            }

            return sb.ToString();
        }

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
                return "<i>No evidence item selected.</i>";
            }

            StringBuilder sb = new StringBuilder();
            string countHeader = totalCount > 0 ? $"<color=#5C4033><b>[ EVIDENCE {index:00} OF {totalCount:00} ]</b></color>\n" : string.Empty;

            if (isDiscovered)
            {
                sb.AppendLine(countHeader + $"<size=20><b>{evidence.evidenceName.ToUpper()}</b></size>");
                sb.AppendLine($"<b>CATEGORY:</b> <color=#5C4033>[{evidence.category}]</color>");
                string statusText = isExamined
                    ? "<color=#1B4D3E><b>✓ FULLY EXAMINED & CATALOGED</b></color>"
                    : "<color=#8B5A2B><b>● LOGGED IN EVIDENCE — REQUIRES FORENSIC INSPECTION</b></color>";
                sb.AppendLine($"<b>STATUS:</b> {statusText}");
                sb.AppendLine(PageDivider + "\n");

                sb.AppendLine("<size=15><b>[ PHYSICAL DESCRIPTION ]</b></size>");
                sb.AppendLine(evidence.baseDescription + "\n");

                sb.AppendLine("<size=15><b>[ FORENSIC FINDINGS ]</b></size>");
                if (isExamined && !string.IsNullOrEmpty(evidence.detailedObservation))
                {
                    sb.AppendLine($"<color=#1B4D3E>{evidence.detailedObservation}</color>\n");
                }
                else
                {
                    sb.AppendLine("<i>Item has not been examined on the inspection table yet. Inspect this evidence to reveal forensic details and find hidden hotspots.</i>\n");
                }

                if (!string.IsNullOrEmpty(evidence.unlockedClueText))
                {
                    sb.AppendLine("<size=15><b>[ UNLOCKED CLUE & DEDUCTION ]</b></size>");
                    sb.AppendLine($"<color=#8B5A2B><b>★ {evidence.unlockedClueText}</b></color>\n");
                }

                if (evidence.hotspots != null && evidence.hotspots.Count > 0)
                {
                    sb.AppendLine($"<b>Forensic Hotspots Examined:</b> <b>{discoveredHotspotCount} / {evidence.hotspots.Count}</b>\n");
                }
            }
            else
            {
                sb.AppendLine(countHeader + "<size=20><b>[ ??? UNDISCOVERED EVIDENCE ]</b></size>");
                sb.AppendLine("<b>CATEGORY:</b> <color=#5C4033>[CLASSIFIED]</color>");
                sb.AppendLine("<b>STATUS:</b> <color=#8B0000><b>NOT YET RECOVERED AT CRIME SCENE</b></color>");
                sb.AppendLine(PageDivider + "\n");

                sb.AppendLine("<size=15><b>[ EVIDENCE STATUS ]</b></size>");
                sb.AppendLine("<i>This piece of evidence has not yet been discovered by the investigator.\n\nSearch the crime scene, interrogate suspects, and follow leads to bring this evidence to light.</i>\n");
            }

            sb.AppendLine(PageDivider);
            return sb.ToString();
        }

        public string FormatUnlockedClues(IReadOnlyDictionary<string, string> unlockedClues)
        {
            if (unlockedClues == null || unlockedClues.Count == 0)
            {
                return "<i>No deduction clues synthesized yet.\n\nExamine physical evidence on the inspection table and cross-examine suspects to synthesize breakthrough clues.</i>";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<size=20><b>DEDUCTION JOURNAL ({unlockedClues.Count} CLUES)</b></size>");
            sb.AppendLine(PageDivider + "\n");

            int idx = 1;
            foreach (var kvp in unlockedClues)
            {
                string title = CleanClueTitle(kvp.Key);
                sb.AppendLine($"<size=17><b>★ CLUE {idx:00}: {title}</b></size>");
                sb.AppendLine($"  {kvp.Value}\n");
                sb.AppendLine(SubtleDivider + "\n");
                idx++;
            }

            return sb.ToString();
        }

        public static string CleanClueTitle(string key)
        {
            if (string.IsNullOrEmpty(key)) return "RECORD";
            string title = key.Replace("evidence_", "").Replace("clue_", "").Replace("_", " ").Trim().ToUpperInvariant();
            return !string.IsNullOrEmpty(title) ? title : key.ToUpperInvariant();
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
