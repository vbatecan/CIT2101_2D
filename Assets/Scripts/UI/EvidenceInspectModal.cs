using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    /// <summary>
    /// UI View MonoBehaviour managing the zoomed evidence inspection modal, sprite rotation, and interactive hotspot buttons.
    /// Can be dragged directly onto the InspectModal GameObject in the Unity Inspector.
    /// </summary>
    public class EvidenceInspectModal : MonoBehaviour
    {
        [Header("UI Controls")]
        public Text evidenceTitleText;
        public Image evidenceZoomImage;
        public Text descriptionText;
        public Text clueUnlockedNotificationText;
        public Button closeButton;
        public Button rotateLeftButton;
        public Button rotateRightButton;

        [Header("Hotspots Container")]
        public RectTransform hotspotsContainer;

        private EvidenceSO currentEvidence;

        /// <summary>
        /// Binds UI button click listeners and subscribes to evidence manager events.
        /// </summary>
        private void Start()
        {
            if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
            if (rotateLeftButton != null) rotateLeftButton.onClick.AddListener(() => RotateSprite(-90f));
            if (rotateRightButton != null) rotateRightButton.onClick.AddListener(() => RotateSprite(90f));

            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.OnInspectModalOpened += DisplayEvidence;
                EvidenceManager.Instance.OnHotspotDiscovered += HandleHotspotDiscovered;
            }
        }

        /// <summary>
        /// Populates the inspect modal with zoomed sprite, title, observation descriptions, and hotspot overlays.
        /// </summary>
        /// <param name="evidence">The evidence item being inspected.</param>
        public void DisplayEvidence(EvidenceSO evidence)
        {
            currentEvidence = evidence;
            if (evidence == null) return;

            if (evidenceTitleText != null) evidenceTitleText.text = evidence.evidenceName;

            if (evidenceZoomImage != null)
            {
                evidenceZoomImage.sprite = evidence.zoomedSprite != null ? evidence.zoomedSprite : evidence.normalSprite;
                evidenceZoomImage.rectTransform.localRotation = Quaternion.identity;
            }

            if (descriptionText != null)
            {
                descriptionText.text = string.IsNullOrEmpty(evidence.detailedObservation)
                    ? evidence.baseDescription
                    : $"{evidence.baseDescription}\n\n[Observation]\n{evidence.detailedObservation}";
            }

            if (clueUnlockedNotificationText != null) clueUnlockedNotificationText.gameObject.SetActive(false);

            PopulateHotspots(evidence);
        }

        /// <summary>
        /// Dynamically creates interactive hotspot buttons positioned over normalized coordinates of the evidence sprite.
        /// </summary>
        /// <param name="evidence">The evidence item containing hotspot data.</param>
        private void PopulateHotspots(EvidenceSO evidence)
        {
            if (hotspotsContainer == null) return;

            foreach (Transform child in hotspotsContainer)
            {
                Destroy(child.gameObject);
            }

            if (evidence.hotspots == null || evidence.hotspots.Count == 0) return;

            foreach (var spot in evidence.hotspots)
            {
                GameObject spotObj = new GameObject($"Hotspot_{spot.hotspotId}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                spotObj.transform.SetParent(hotspotsContainer, false);

                RectTransform rt = spotObj.GetComponent<RectTransform>();
                rt.anchorMin = spot.normalizedPosition;
                rt.anchorMax = spot.normalizedPosition;
                rt.sizeDelta = new Vector2(40f, 40f);

                Image img = spotObj.GetComponent<Image>();
                img.color = spot.isDiscovered ? new Color(0.2f, 0.8f, 0.2f, 0.6f) : new Color(0.9f, 0.7f, 0.1f, 0.8f);

                EvidenceHotspot currentSpot = spot;
                spotObj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    EvidenceManager.Instance?.DiscoverHotspot(currentSpot);
                });
            }
        }

        /// <summary>
        /// Updates the notification banner and re-renders hotspot markers when a hotspot is discovered.
        /// </summary>
        /// <param name="hotspot">The hotspot that was discovered.</param>
        private void HandleHotspotDiscovered(EvidenceHotspot hotspot)
        {
            if (clueUnlockedNotificationText != null)
            {
                clueUnlockedNotificationText.gameObject.SetActive(true);
                clueUnlockedNotificationText.text = $"[NEW CLUE DISCOVERED]\n{hotspot.observationText}";
            }

            if (currentEvidence != null)
            {
                PopulateHotspots(currentEvidence);
            }
        }

        /// <summary>
        /// Rotates the zoomed evidence sprite by a specified angle in degrees.
        /// </summary>
        /// <param name="angle">Rotation delta in degrees (e.g. 90f or -90f).</param>
        private void RotateSprite(float angle)
        {
            if (evidenceZoomImage != null)
            {
                evidenceZoomImage.rectTransform.Rotate(0, 0, angle);
            }
        }

        /// <summary>
        /// Handles close button click, closing the inspect modal via <see cref="EvidenceManager"/>.
        /// </summary>
        private void OnCloseClicked()
        {
            EvidenceManager.Instance?.CloseInspectModal();
        }
    }
}
