using System;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Services;

namespace CaseClosed.Managers
{
    public class EvidenceManager : MonoBehaviour
    {
        public static EvidenceManager Instance { get; private set; }

        public EvidenceSO currentlySelectedEvidence;

        public bool isInspectingModalOpen = false;

        public int LastInspectionClosedFrame { get; private set; } = -1;

        public event Action<EvidenceSO> OnEvidenceSelected;

        public event Action<EvidenceSO> OnInspectModalOpened;

        public event Action OnInspectModalClosed;

        public event Action<EvidenceHotspot> OnHotspotDiscovered;

        private readonly EvidenceService evidenceService = new EvidenceService();

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SelectEvidence(EvidenceSO evidence)
        {
            currentlySelectedEvidence = evidence;
            Debug.Log($"[EvidenceManager] Selected evidence item: '{(evidence != null ? evidence.evidenceName : "NULL")}' (ID: {evidence?.id})");
            OnEvidenceSelected?.Invoke(evidence);
        }

        public void OpenInspectModal(EvidenceSO evidence)
        {
            OpenInspectModal(evidence, evidence != null ? evidence.dialogueNodeToTriggerOnInspect : null);
        }

        public void OpenInspectModal(EvidenceSO evidence, string dialogueNodeId)
        {
            if (evidence == null || isInspectingModalOpen) return;

            InterrogationManager.Instance?.QueueInspectionDialogue(dialogueNodeId);
            InterrogationManager.Instance?.QueueInspectionEvidence(evidence);
            currentlySelectedEvidence = evidence;
            isInspectingModalOpen = true;

            Debug.Log($"[EvidenceManager] Opened inspect modal for: '{evidence.evidenceName}' (ID: {evidence.id})");

            CaseManager caseManager = CaseManager.Instance;
            caseManager?.RegisterDiscoveredEvidence(evidence);

            if (caseManager != null && evidenceService.InspectEvidence(caseManager.SessionState, evidence, out string baseClueId, out string baseClueText))
            {
                Debug.Log($"[EvidenceManager] Extracted base clue from '{evidence.evidenceName}': '[{baseClueId}]' - \"{baseClueText}\"");
                caseManager.UnlockClue(baseClueId, baseClueText);
            }

            OnInspectModalOpened?.Invoke(evidence);
            AudioManager.Instance?.PlayExamineZoom();
        }

        public void CloseInspectModal()
        {
            if (!isInspectingModalOpen) return;

            Debug.Log("[EvidenceManager] Closed inspect modal");
            LastInspectionClosedFrame = Time.frameCount;
            isInspectingModalOpen = false;
            OnInspectModalClosed?.Invoke();
            InterrogationManager.Instance?.PlayPendingInspectionDialogue();
        }

        public void DiscoverHotspot(EvidenceHotspot hotspot)
        {
            DiscoverHotspot(ResolveEvidenceForHotspot(hotspot), hotspot);
        }

        public void DiscoverHotspot(EvidenceSO evidence, EvidenceHotspot hotspot)
        {
            if (evidence == null || hotspot == null) return;

            CaseManager caseManager = CaseManager.Instance;
            if (caseManager == null || caseManager.ActiveCase == null) return;

            Debug.Log($"[EvidenceManager] Discovering hotspot: '{hotspot.hotspotTitle}' (ID: {hotspot.hotspotId})");

            if (!evidenceService.DiscoverHotspot(caseManager.SessionState, evidence, hotspot, out string clueId, out string clueText)) return;

            if (!string.IsNullOrEmpty(clueId))
            {
                Debug.Log($"[EvidenceManager] Hotspot unlocked clue: '[{clueId}]' - \"{clueText}\"");
                caseManager.UnlockClue(clueId, clueText);
            }

            OnHotspotDiscovered?.Invoke(hotspot);
        }

        public void ToggleEvidenceOnTable(EvidenceSO evidence)
        {
            CaseManager caseManager = CaseManager.Instance;
            bool newState = caseManager != null && evidenceService.ToggleTablePresence(caseManager.SessionState, evidence);
            Debug.Log($"[EvidenceManager] Toggled table presence for '{(evidence != null ? evidence.evidenceName : "NULL")}': {newState}");
        }

        private EvidenceSO ResolveEvidenceForHotspot(EvidenceHotspot hotspot)
        {
            if (hotspot == null) return null;

            if (currentlySelectedEvidence != null && currentlySelectedEvidence.hotspots != null && currentlySelectedEvidence.hotspots.Contains(hotspot))
            {
                return currentlySelectedEvidence;
            }

            CaseSO activeCase = CaseManager.Instance?.ActiveCase;
            if (activeCase == null || activeCase.evidenceItems == null) return null;

            foreach (EvidenceSO evidence in activeCase.evidenceItems)
            {
                if (evidence != null && evidence.hotspots != null && evidence.hotspots.Contains(hotspot))
                {
                    return evidence;
                }
            }

            return null;
        }
    }
}
