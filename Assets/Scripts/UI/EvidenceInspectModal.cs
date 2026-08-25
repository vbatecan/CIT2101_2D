using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
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

        private void PopulateHotspots(EvidenceSO evidence)
        {
            if (hotspotsContainer == null) return;

            foreach (Transform child in hotspotsContainer)
            {
                Destroy(child.gameObject);
            }

            if (evidence.hotspots == null || evidence.hotspots.Count == 0) return;

            Vector2 containerSize = hotspotsContainer.rect.size;

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

        private void RotateSprite(float angle)
        {
            if (evidenceZoomImage != null)
            {
                evidenceZoomImage.rectTransform.Rotate(0, 0, angle);
            }
        }

        private void OnCloseClicked()
        {
            EvidenceManager.Instance?.CloseInspectModal();
        }
    }
}
