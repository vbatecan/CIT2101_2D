using CaseClosed.Data;

namespace CaseClosed.Services
{
    /// <summary>
    /// Service responsible for evidence inspection business logic, interactive hotspot validation,
    /// and clue derivation from physical evidence.
    /// </summary>
    public class EvidenceService
    {
        /// <summary>
        /// Processes the examination of an evidence item and extracts its base clue if available.
        /// </summary>
        /// <param name="evidence">The evidence item being inspected.</param>
        /// <param name="baseClueId">Output identifier for the unlocked base clue.</param>
        /// <param name="baseClueText">Output descriptive observation text for the unlocked clue.</param>
        /// <returns>True if a valid base clue was extracted upon examination; otherwise, false.</returns>
        public bool InspectEvidence(CaseSessionState session, EvidenceSO evidence, out string baseClueId, out string baseClueText)
        {
            baseClueId = null;
            baseClueText = null;

            if (session == null || evidence == null) return false;

            session.TryMarkEvidenceExamined(evidence);

            if (!string.IsNullOrEmpty(evidence.unlockedClueText))
            {
                baseClueId = evidence.id + "_BASE_CLUE";
                baseClueText = evidence.unlockedClueText;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Validates and records discovery of an evidence hotspot, extracting associated clue information.
        /// </summary>
        /// <param name="hotspot">The hotspot being clicked or inspected.</param>
        /// <param name="clueId">Output identifier for the unlocked clue.</param>
        /// <param name="clueText">Output observation text for the unlocked clue.</param>
        /// <returns>True if a new hotspot was successfully discovered; otherwise, false.</returns>
        public bool DiscoverHotspot(
            CaseSessionState session,
            EvidenceSO evidence,
            EvidenceHotspot hotspot,
            out string clueId,
            out string clueText)
        {
            clueId = null;
            clueText = null;

            if (session == null || evidence == null || hotspot == null || !session.TryDiscoverHotspot(evidence, hotspot)) return false;

            if (!string.IsNullOrEmpty(hotspot.clueUnlockedId))
            {
                clueId = hotspot.clueUnlockedId;
                clueText = hotspot.observationText;
                return true;
            }

            return true;
        }

        /// <summary>
        /// Toggles the table display visibility state for an evidence item.
        /// </summary>
        /// <param name="evidence">The evidence item to toggle.</param>
        /// <returns>The new visibility state on the investigation table.</returns>
        public bool ToggleTablePresence(CaseSessionState session, EvidenceSO evidence)
        {
            return session != null && session.ToggleEvidenceTablePresence(evidence);
        }
    }
}
