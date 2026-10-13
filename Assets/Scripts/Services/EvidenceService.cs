using CaseClosed.Data;

namespace CaseClosed.Services
{
    public class EvidenceService
    {
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

        public bool ToggleTablePresence(CaseSessionState session, EvidenceSO evidence)
        {
            return session != null && session.ToggleEvidenceTablePresence(evidence);
        }
    }
}
