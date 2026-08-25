using System;
using UnityEngine;
using CaseClosed.Data;

namespace CaseClosed.Managers
{
    public class EvidenceManager : MonoBehaviour
    {
        public static EvidenceManager Instance { get; private set; }

        public EvidenceSO currentlySelectedEvidence;
        public bool isInspectingModalOpen = false;

        public event Action<EvidenceSO> OnEvidenceSelected;
        public event Action<EvidenceSO> OnInspectModalOpened;
        public event Action OnInspectModalClosed;
        public event Action<EvidenceHotspot> OnHotspotDiscovered;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SelectEvidence(EvidenceSO evidence)
        {
            currentlySelectedEvidence = evidence;
            OnEvidenceSelected?.Invoke(evidence);
        }

        public void OpenInspectModal(EvidenceSO evidence)
        {
            if (evidence == null) return;
            currentlySelectedEvidence = evidence;
            evidence.isExamined = true;
            isInspectingModalOpen = true;

            // Auto-unlock base clue if specified
            if (!string.IsNullOrEmpty(evidence.unlockedClueText))
            {
                CaseManager.Instance?.UnlockClue(evidence.id + "_BASE_CLUE", evidence.unlockedClueText);
            }

            OnInspectModalOpened?.Invoke(evidence);
            AudioManager.Instance?.PlayExamineZoom();
        }

        public void CloseInspectModal()
        {
            isInspectingModalOpen = false;
            OnInspectModalClosed?.Invoke();
        }

        public void DiscoverHotspot(EvidenceHotspot hotspot)
        {
            if (hotspot == null || hotspot.isDiscovered) return;
            hotspot.isDiscovered = true;

            if (!string.IsNullOrEmpty(hotspot.clueUnlockedId))
            {
                CaseManager.Instance?.UnlockClue(hotspot.clueUnlockedId, hotspot.observationText);
            }

            OnHotspotDiscovered?.Invoke(hotspot);
        }

        public void ToggleEvidenceOnTable(EvidenceSO evidence)
        {
            if (evidence == null) return;
            evidence.isToggledOnTable = !evidence.isToggledOnTable;
        }
    }
}
