using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CaseClosed.Data;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    /// <summary>
    /// UI View MonoBehaviour managing the zoomed evidence inspection modal, sprite rotation,
    /// smooth zoom magnification, pan/drag navigation, and interactive hotspot buttons.
    /// Can be dragged directly onto the InspectModal GameObject in the Unity Inspector.
    /// </summary>
    public class EvidenceInspectModal : MonoBehaviour, IScrollHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [Header("UI Controls - Header & Info")]
        public Text evidenceTitleText;
        public Image evidenceZoomImage;
        public Text descriptionText;
        public Text clueUnlockedNotificationText;
        public Button closeButton;

        [Header("UI Controls - Rotation")]
        public Button rotateLeftButton;
        public Button rotateRightButton;

        [Header("UI Controls - Zoom & Pan")]
        public Button zoomInButton;
        public Button zoomOutButton;
        public Button resetZoomButton;
        public Slider zoomSlider;
        public Text zoomLevelText;
        public RectTransform viewportRectTransform;

        [Header("Hotspots Container")]
        public RectTransform hotspotsContainer;

        [Header("Zoom & Pan Settings")]
        [Range(1f, 2f)] public float minZoom = 1.0f;
        [Range(2f, 6f)] public float maxZoom = 3.5f;
        public float zoomStep = 0.25f;
        public float scrollSensitivity = 0.15f;
        public bool smoothZoom = true;
        public float zoomLerpSpeed = 15f;

        private EvidenceSO currentEvidence;
        private float currentZoom = 1.0f;
        private float targetZoom = 1.0f;
        private Vector2 currentPanPosition = Vector2.zero;
        private Vector2 targetPanPosition = Vector2.zero;
        private Vector2 lastDragLocalPos;
        private bool isDragging = false;

        /// <summary>Current rendered zoom magnification factor.</summary>
        public float CurrentZoom => currentZoom;

        /// <summary>Target zoom magnification factor.</summary>
        public float TargetZoom => targetZoom;

        /// <summary>Current rendered pan translation offset.</summary>
        public Vector2 CurrentPanPosition => currentPanPosition;

        /// <summary>Target pan translation offset.</summary>
        public Vector2 TargetPanPosition => targetPanPosition;

        /// <summary>
        /// Binds UI button click listeners, initializes slider, and subscribes to evidence manager events.
        /// </summary>
        private void Start()
        {
            if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
            if (rotateLeftButton != null) rotateLeftButton.onClick.AddListener(() => RotateSprite(-90f));
            if (rotateRightButton != null) rotateRightButton.onClick.AddListener(() => RotateSprite(90f));
            if (zoomInButton != null) zoomInButton.onClick.AddListener(ZoomIn);
            if (zoomOutButton != null) zoomOutButton.onClick.AddListener(ZoomOut);
            if (resetZoomButton != null) resetZoomButton.onClick.AddListener(ResetView);

            if (zoomSlider != null)
            {
                zoomSlider.minValue = minZoom;
                zoomSlider.maxValue = maxZoom;
                zoomSlider.value = targetZoom;
                zoomSlider.onValueChanged.AddListener(OnSliderZoomChanged);
            }

            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.OnInspectModalOpened += DisplayEvidence;
                EvidenceManager.Instance.OnHotspotDiscovered += HandleHotspotDiscovered;
            }

            UpdateZoomUI();
        }

        /// <summary>
        /// Unsubscribes from manager events on destroy to prevent memory leaks.
        /// </summary>
        private void OnDestroy()
        {
            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.OnInspectModalOpened -= DisplayEvidence;
                EvidenceManager.Instance.OnHotspotDiscovered -= HandleHotspotDiscovered;
            }
        }

        /// <summary>
        /// Updates smooth lerping for zoom magnification and pan position.
        /// </summary>
        private void Update()
        {
            if (evidenceZoomImage == null) return;

            if (smoothZoom)
            {
                currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.unscaledDeltaTime * zoomLerpSpeed);
                currentPanPosition = Vector2.Lerp(currentPanPosition, targetPanPosition, Time.unscaledDeltaTime * zoomLerpSpeed);
            }
            else
            {
                currentZoom = targetZoom;
                currentPanPosition = targetPanPosition;
            }

            ApplyTransform();
        }

        /// <summary>
        /// Applies current zoom scale and pan translation to the evidence image RectTransform.
        /// </summary>
        private void ApplyTransform()
        {
            if (evidenceZoomImage != null)
            {
                evidenceZoomImage.rectTransform.localScale = new Vector3(currentZoom, currentZoom, 1f);
                evidenceZoomImage.rectTransform.anchoredPosition = currentPanPosition;
            }
        }

        /// <summary>
        /// Populates the inspect modal with zoomed sprite, title, observation descriptions, and hotspot overlays.
        /// Resets zoom, pan, and rotation to default initial values.
        /// </summary>
        /// <param name="evidence">The evidence item being inspected.</param>
        public void DisplayEvidence(EvidenceSO evidence)
        {
            currentEvidence = evidence;
            if (evidence == null) return;

            Debug.Log($"[UI:InspectModal] Displaying evidence '{evidence.evidenceName}' (ID: {evidence.id}, Hotspots: {evidence.hotspots?.Count ?? 0})");

            if (evidenceTitleText != null) evidenceTitleText.text = evidence.evidenceName;

            if (evidenceZoomImage != null)
            {
                evidenceZoomImage.sprite = evidence.zoomedSprite != null ? evidence.zoomedSprite : evidence.normalSprite;
            }

            // Reset view state upon opening evidence
            ResetView();

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
        /// Increments target zoom by <see cref="zoomStep"/> and updates UI.
        /// </summary>
        public void ZoomIn()
        {
            SetTargetZoom(targetZoom + zoomStep);
            AudioManager.Instance?.PlayButtonClick();
        }

        /// <summary>
        /// Decrements target zoom by <see cref="zoomStep"/> and updates UI.
        /// </summary>
        public void ZoomOut()
        {
            SetTargetZoom(targetZoom - zoomStep);
            AudioManager.Instance?.PlayButtonClick();
        }

        /// <summary>
        /// Sets the target zoom magnification, clamping between <see cref="minZoom"/> and <see cref="maxZoom"/>,
        /// recalculates allowable pan bounds, and updates UI indicators.
        /// </summary>
        /// <param name="zoom">The new target zoom scale factor.</param>
        public void SetTargetZoom(float zoom)
        {
            float oldZoom = targetZoom;
            targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);

            ClampPan();
            UpdateZoomUI();

            if (!smoothZoom)
            {
                currentZoom = targetZoom;
                currentPanPosition = targetPanPosition;
                ApplyTransform();
            }

            if (Mathf.Abs(oldZoom - targetZoom) > 0.01f)
            {
                Debug.Log($"[UI:InspectModal] Zoom changed: {targetZoom:F2}x (Current: {currentZoom:F2}x)");
            }
        }

        /// <summary>
        /// Resets target zoom to <see cref="minZoom"/> (1.0x) and centers pan translation.
        /// </summary>
        public void ResetZoom()
        {
            targetZoom = minZoom;
            targetPanPosition = Vector2.zero;
            ClampPan();
            UpdateZoomUI();

            if (!smoothZoom)
            {
                currentZoom = targetZoom;
                currentPanPosition = targetPanPosition;
                ApplyTransform();
            }

            Debug.Log("[UI:InspectModal] Zoom reset to 1.0x");
        }

        /// <summary>
        /// Resets target zoom to default (1.0x), centers pan offset to (0,0), and resets sprite rotation to 0°.
        /// </summary>
        public void ResetView()
        {
            ResetZoom();
            if (evidenceZoomImage != null)
            {
                evidenceZoomImage.rectTransform.localRotation = Quaternion.identity;
            }
            Debug.Log("[UI:InspectModal] View fully reset (Zoom: 1.0x, Pan: (0,0), Rotation: 0°)");
        }

        /// <summary>
        /// Updates zoom level text, slider position, and button interactability states.
        /// </summary>
        private void UpdateZoomUI()
        {
            if (zoomLevelText != null)
            {
                zoomLevelText.text = $"{Mathf.RoundToInt(targetZoom * 100f)}%";
            }

            if (zoomSlider != null)
            {
                zoomSlider.SetValueWithoutNotify(targetZoom);
            }

            if (zoomInButton != null)
            {
                zoomInButton.interactable = targetZoom < maxZoom - 0.001f;
            }

            if (zoomOutButton != null)
            {
                zoomOutButton.interactable = targetZoom > minZoom + 0.001f;
            }
        }

        /// <summary>
        /// Handles zoom slider value change event.
        /// </summary>
        /// <param name="value">The slider zoom value.</param>
        private void OnSliderZoomChanged(float value)
        {
            SetTargetZoom(value);
        }

        /// <summary>
        /// Handles mouse scroll wheel events over the modal/viewport, zooming towards cursor position.
        /// </summary>
        /// <param name="eventData">Pointer event data from Unity EventSystem.</param>
        public void OnScroll(PointerEventData eventData)
        {
            if (Mathf.Abs(eventData.scrollDelta.y) < 0.001f) return;

            float deltaZoom = eventData.scrollDelta.y * scrollSensitivity;
            float newZoom = Mathf.Clamp(targetZoom + deltaZoom, minZoom, maxZoom);

            if (Mathf.Abs(newZoom - targetZoom) < 0.001f) return;

            // Zoom centered towards pointer local position
            RectTransform panParent = GetPanParentRect();
            if (panParent != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(panParent, eventData.position, eventData.pressEventCamera, out Vector2 cursorLocalPoint))
            {
                float zoomRatio = newZoom / targetZoom;
                targetPanPosition = (targetPanPosition - cursorLocalPoint) * zoomRatio + cursorLocalPoint;
            }

            SetTargetZoom(newZoom);
            AudioManager.Instance?.PlayExamineZoom();
        }

        /// <summary>
        /// Handles begin drag pointer event for panning magnified evidence.
        /// </summary>
        /// <param name="eventData">Pointer event data from Unity EventSystem.</param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            RectTransform panParent = GetPanParentRect();
            if (panParent != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(panParent, eventData.position, eventData.pressEventCamera, out lastDragLocalPos))
            {
                isDragging = true;
            }
        }

        /// <summary>
        /// Handles drag pointer event, translating pan offset and clamping within viewport bounds.
        /// </summary>
        /// <param name="eventData">Pointer event data from Unity EventSystem.</param>
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            RectTransform panParent = GetPanParentRect();
            if (panParent != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(panParent, eventData.position, eventData.pressEventCamera, out Vector2 currentLocalPos))
            {
                Vector2 dragDelta = currentLocalPos - lastDragLocalPos;
                targetPanPosition += dragDelta;
                ClampPan();
                lastDragLocalPos = currentLocalPos;

                if (!smoothZoom)
                {
                    currentPanPosition = targetPanPosition;
                    ApplyTransform();
                }
            }
        }

        /// <summary>
        /// Handles end drag pointer event.
        /// </summary>
        /// <param name="eventData">Pointer event data from Unity EventSystem.</param>
        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
        }

        /// <summary>
        /// Clamps pan position based on current zoom magnification and viewport bounds.
        /// When zoom is 1.0x, pan is locked to (0,0).
        /// </summary>
        public void ClampPan()
        {
            if (targetZoom <= minZoom + 0.001f)
            {
                targetPanPosition = Vector2.zero;
                return;
            }

            RectTransform viewport = GetPanParentRect();
            if (viewport != null && evidenceZoomImage != null)
            {
                Vector2 viewportSize = viewport.rect.size;
                Vector2 imageSize = evidenceZoomImage.rectTransform.rect.size;

                // When image is larger than viewport at target zoom, allowable pan extends to edge
                float maxPanX = Mathf.Max(0f, (imageSize.x * targetZoom - viewportSize.x) * 0.5f);
                float maxPanY = Mathf.Max(0f, (imageSize.y * targetZoom - viewportSize.y) * 0.5f);

                targetPanPosition.x = Mathf.Clamp(targetPanPosition.x, -maxPanX, maxPanX);
                targetPanPosition.y = Mathf.Clamp(targetPanPosition.y, -maxPanY, maxPanY);
            }
            else
            {
                targetPanPosition = Vector2.zero;
            }
        }

        /// <summary>
        /// Retrieves the parent RectTransform acting as the viewport boundary for panning calculations.
        /// </summary>
        /// <returns>The viewport RectTransform or parent RectTransform.</returns>
        private RectTransform GetPanParentRect()
        {
            if (viewportRectTransform != null) return viewportRectTransform;
            if (evidenceZoomImage != null && evidenceZoomImage.rectTransform.parent != null)
            {
                return evidenceZoomImage.rectTransform.parent as RectTransform;
            }
            return transform as RectTransform;
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
                    Debug.Log($"[UI:InspectModal] Hotspot clicked '{currentSpot.hotspotTitle}' (ID: {currentSpot.hotspotId}) on evidence '{evidence.evidenceName}'");
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
            Debug.Log($"[UI:InspectModal] Hotspot discovered notification shown for '{hotspot.hotspotTitle}'");

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
        public void RotateSprite(float angle)
        {
            Debug.Log($"[UI:InspectModal] Rotate button clicked (Angle delta: {angle}°)");
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
            Debug.Log("[UI:InspectModal] Close inspect modal button clicked");
            EvidenceManager.Instance?.CloseInspectModal();
        }
    }
}
