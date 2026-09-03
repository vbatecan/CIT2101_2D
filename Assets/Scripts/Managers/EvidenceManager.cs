using System;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Services;

namespace CaseClosed.Managers
{
    /// <summary>
    /// Controller MonoBehaviour managing evidence selection, examination modal states, and interactive hotspot triggers,
    /// delegating inspection processing to <see cref="EvidenceService"/>.
    /// Can be dragged directly onto a GameObject in the Unity Inspector.
    /// </summary>
    public class EvidenceManager : MonoBehaviour
    {
        /// <summary>Singleton instance of the EvidenceManager.</summary>
        public static EvidenceManager Instance { get; private set; }

        /// <summary>The currently focused or selected evidence item.</summary>
        public EvidenceSO currentlySelectedEvidence;

        /// <summary>Flag indicating whether the close-up inspect modal is currently open.</summary>
        public bool isInspectingModalOpen = false;

        /// <summary>Event raised when an evidence item is selected on the table or list.</summary>
        public event Action<EvidenceSO> OnEvidenceSelected;

        /// <summary>Event raised when the close-up inspect modal is opened for an evidence item.</summary>
        public event Action<EvidenceSO> OnInspectModalOpened;

        /// <summary>Event raised when the close-up inspect modal is closed.</summary>
        public event Action OnInspectModalClosed;

        /// <summary>Event raised when a hotspot on an evidence item is discovered.</summary>
        public event Action<EvidenceHotspot> OnHotspotDiscovered;

        private readonly EvidenceService evidenceService = new EvidenceService();

        /// <summary>
        /// Initializes the singleton instance.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(gameObject);
                else
                    Destroy(gameObject);
#else
                Destroy(gameObject);
#endif
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Selects an evidence item and notifies listeners.
        /// </summary>
        /// <param name="evidence">The evidence item to select.</param>
        public void SelectEvidence(EvidenceSO evidence)
        {
            currentlySelectedEvidence = evidence;
            Debug.Log($"[EvidenceManager] Selected evidence item: '{(evidence != null ? evidence.evidenceName : "NULL")}' (ID: {evidence?.id})");
            OnEvidenceSelected?.Invoke(evidence);
        }

        /// <summary>
        /// Opens the close-up inspection modal for an evidence item, extracts any base clue via <see cref="EvidenceService"/>,
        /// and plays an examination audio effect.
        /// </summary>
        /// <param name="evidence">The evidence item to examine.</param>
        public void OpenInspectModal(EvidenceSO evidence)
        {
            if (evidence == null) return;

            currentlySelectedEvidence = evidence;
            isInspectingModalOpen = true;

            Debug.Log($"[EvidenceManager] Opened inspect modal for: '{evidence.evidenceName}' (ID: {evidence.id})");

            // Inspection and clue extraction processed via Service
            if (evidenceService.InspectEvidence(evidence, out string baseClueId, out string baseClueText))
            {
                Debug.Log($"[EvidenceManager] Extracted base clue from '{evidence.evidenceName}': '[{baseClueId}]' - \"{baseClueText}\"");
                CaseManager.Instance?.UnlockClue(baseClueId, baseClueText);
            }

            OnInspectModalOpened?.Invoke(evidence);
            AudioManager.Instance?.PlayExamineZoom();
        }

        /// <summary>
        /// Closes the close-up inspect modal and returns to the normal table view.
        /// </summary>
        public void CloseInspectModal()
        {
            Debug.Log("[EvidenceManager] Closed inspect modal");
            isInspectingModalOpen = false;
            OnInspectModalClosed?.Invoke();
        }

        /// <summary>
        /// Processes the discovery of an interactive hotspot on an examined evidence item via <see cref="EvidenceService"/>.
        /// </summary>
        /// <param name="hotspot">The hotspot being discovered.</param>
        public void DiscoverHotspot(EvidenceHotspot hotspot)
        {
            if (hotspot == null || hotspot.isDiscovered) return;

            Debug.Log($"[EvidenceManager] Discovering hotspot: '{hotspot.hotspotTitle}' (ID: {hotspot.hotspotId})");

            // Hotspot validation and clue derivation processed via Service
            if (evidenceService.DiscoverHotspot(hotspot, out string clueId, out string clueText))
            {
                if (!string.IsNullOrEmpty(clueId))
                {
                    Debug.Log($"[EvidenceManager] Hotspot unlocked clue: '[{clueId}]' - \"{clueText}\"");
                    CaseManager.Instance?.UnlockClue(clueId, clueText);
                }
            }

            OnHotspotDiscovered?.Invoke(hotspot);
        }

        /// <summary>
        /// Toggles whether an evidence item is placed on the investigation desk via <see cref="EvidenceService"/>.
        /// </summary>
        /// <param name="evidence">The evidence item to toggle.</param>
        public void ToggleEvidenceOnTable(EvidenceSO evidence)
        {
            bool newState = evidenceService.ToggleTablePresence(evidence);
            Debug.Log($"[EvidenceManager] Toggled table presence for '{(evidence != null ? evidence.evidenceName : "NULL")}': {newState}");
        }
    }
}
