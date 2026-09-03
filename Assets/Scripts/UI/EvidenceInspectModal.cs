using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CaseClosed.Data;
using CaseClosed.Gameplay;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    /// <summary>
    /// UI View MonoBehaviour managing the isolated evidence inspection mode:
    /// - Hides all background, characters, items, arm pointer, and HUD elements so only the inspected evidence is visible.
    /// - Provides continuous 360-degree rotation controlled via mouse cursor drag.
    /// - Preserves smooth mouse scroll wheel zoom and pan navigation.
    /// - Closes when clicking the background outside the evidence, right-clicking, or pressing Esc/Space.
    /// </summary>
    public class EvidenceInspectModal : MonoBehaviour, IScrollHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [Header("Evidence Visuals")]
        public Image evidenceZoomImage;
        public RectTransform viewportRectTransform;
        public RectTransform hotspotsContainer;
        public Text clueUnlockedNotificationText;

        [Header("Rotation Settings")]
        public float rotationSensitivity = 0.45f;
        public bool smoothRotation = true;
        public float rotationLerpSpeed = 16f;

        [Header("Zoom & Pan Settings")]
        [Range(0.8f, 2f)] public float minZoom = 1.0f;
        [Range(2f, 6f)] public float maxZoom = 3.5f;
        public float zoomStep = 0.25f;
        public float scrollSensitivity = 0.15f;
        public bool smoothZoom = true;
        public float zoomLerpSpeed = 15f;

        [Header("Scene Isolation Configuration")]
        [Tooltip("Names of root GameObjects to hide when entering evidence inspection.")]
        public string[] sceneObjectsToHide = new string[]
        {
            "Environments",
            "Characters",
            "Items",
            "Detective_Arm_Pointer",
            "Panel_HeaderNav",
            "Panel_Dialogue"
        };

        private EvidenceSO currentEvidence;
        private float currentZoom = 1.0f;
        private float targetZoom = 1.0f;
        private float currentRotationAngle = 0f;
        private float targetRotationAngle = 0f;
        private Vector2 currentPanPosition = Vector2.zero;
        private Vector2 targetPanPosition = Vector2.zero;
        private Vector2 lastDragLocalPos;
        private bool isDragging = false;
        private bool isInspecting = false;

        private readonly List<GameObject> hiddenSceneObjects = new List<GameObject>();

        /// <summary>Current rendered zoom magnification factor.</summary>
        public float CurrentZoom => currentZoom;

        /// <summary>Target zoom magnification factor.</summary>
        public float TargetZoom => targetZoom;

        /// <summary>Current rendered rotation angle in degrees.</summary>
        public float CurrentRotationAngle => currentRotationAngle;

        /// <summary>Target rotation angle in degrees.</summary>
        public float TargetRotationAngle => targetRotationAngle;

        /// <summary>Current rendered pan translation offset.</summary>
        public Vector2 CurrentPanPosition => currentPanPosition;

        /// <summary>Target pan translation offset.</summary>
        public Vector2 TargetPanPosition => targetPanPosition;

        /// <summary>Whether evidence inspection mode is currently active.</summary>
        public bool IsInspecting => isInspecting;

        /// <summary>List of GameObjects temporarily hidden for scene isolation.</summary>
        public IReadOnlyList<GameObject> HiddenSceneObjects => hiddenSceneObjects;

        private void Awake()
        {
            // Ensure no dark background image is rendered on the inspect panel container, but allow transparent clicks
            Image panelBg = GetComponent<Image>();
            if (panelBg != null)
            {
                panelBg.color = Color.clear;
                panelBg.raycastTarget = true;
            }

            if (evidenceZoomImage != null && evidenceZoomImage.sprite == null)
            {
                evidenceZoomImage.enabled = false;
                evidenceZoomImage.color = Color.clear;
            }

            if (clueUnlockedNotificationText != null)
            {
                clueUnlockedNotificationText.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            RegisterManagerEvents();

            // Auto-display currently selected evidence if already set when opening
            if (EvidenceManager.Instance != null && EvidenceManager.Instance.currentlySelectedEvidence != null)
            {
                DisplayEvidence(EvidenceManager.Instance.currentlySelectedEvidence);
            }
        }

        private void OnDisable()
        {
            UnregisterManagerEvents();
            RestoreSceneObjects();
        }

        private void Start()
        {
            RegisterManagerEvents();

            if (EvidenceManager.Instance != null && EvidenceManager.Instance.currentlySelectedEvidence != null && currentEvidence == null)
            {
                DisplayEvidence(EvidenceManager.Instance.currentlySelectedEvidence);
            }
        }

        private void OnDestroy()
        {
            UnregisterManagerEvents();
            RestoreSceneObjects();
        }

        private bool eventsRegistered = false;

        private void RegisterManagerEvents()
        {
            if (eventsRegistered || EvidenceManager.Instance == null) return;
            EvidenceManager.Instance.OnInspectModalOpened += DisplayEvidence;
            EvidenceManager.Instance.OnHotspotDiscovered += HandleHotspotDiscovered;
            EvidenceManager.Instance.OnInspectModalClosed += HandleInspectClosed;
            eventsRegistered = true;
        }

        private void UnregisterManagerEvents()
        {
            if (!eventsRegistered || EvidenceManager.Instance == null) return;
            EvidenceManager.Instance.OnInspectModalOpened -= DisplayEvidence;
            EvidenceManager.Instance.OnHotspotDiscovered -= HandleHotspotDiscovered;
            EvidenceManager.Instance.OnInspectModalClosed -= HandleInspectClosed;
            eventsRegistered = false;
        }

        private void Update()
        {
            if (!isInspecting) return;

            // Handle Exit Inputs: Right-Click or Escape / Space / Backspace / Enter / E / Q keys
            if (Input.GetMouseButtonDown(1) ||
                Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Backspace) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.E) ||
                Input.GetKeyDown(KeyCode.Q))
            {
                CloseInspect();
                return;
            }

            if (evidenceZoomImage == null) return;

            // Smooth Interpolation for Zoom, Pan, and Rotation
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

            if (smoothRotation)
            {
                currentRotationAngle = Mathf.Lerp(currentRotationAngle, targetRotationAngle, Time.unscaledDeltaTime * rotationLerpSpeed);
            }
            else
            {
                currentRotationAngle = targetRotationAngle;
            }

            ApplyTransform();
        }

        /// <summary>
        /// Applies current zoom scale, rotation angle, and pan translation to the evidence image RectTransform.
        /// </summary>
        public void ApplyTransform()
        {
            if (evidenceZoomImage != null)
            {
                evidenceZoomImage.rectTransform.localScale = new Vector3(currentZoom, currentZoom, 1f);
                evidenceZoomImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, currentRotationAngle);
                evidenceZoomImage.rectTransform.anchoredPosition = currentPanPosition;
            }
        }

        /// <summary>
        /// Populates the isolated inspection view with the evidence sprite, resets zoom/rotation,
        /// and hides all other scene objects so only this evidence is visible.
        /// </summary>
        /// <param name="evidence">The evidence item being inspected.</param>
        public void DisplayEvidence(EvidenceSO evidence)
        {
            currentEvidence = evidence;
            if (evidence == null) return;

            Debug.Log($"[UI:InspectModal] Entering isolated inspection for evidence '{evidence.evidenceName}' (ID: {evidence.id})");
            isInspecting = true;

            // Ensure cursor is visible and free during inspection
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            Sprite targetSprite = evidence.zoomedSprite != null ? evidence.zoomedSprite : evidence.normalSprite;
            if (evidenceZoomImage != null)
            {
                evidenceZoomImage.sprite = targetSprite;
                evidenceZoomImage.enabled = (targetSprite != null);
                evidenceZoomImage.color = (targetSprite != null) ? Color.white : Color.clear;
            }

            // Reset view state upon opening evidence
            ResetView();

            // Hide background, characters, table items, arm pointer, and header HUD
            HideSceneObjects();

            if (clueUnlockedNotificationText != null)
            {
                clueUnlockedNotificationText.gameObject.SetActive(false);
            }

            PopulateHotspots(evidence);
        }

        /// <summary>
        /// Hides background environments, characters, table items, arm pointer, and UI panels.
        /// </summary>
        public void HideSceneObjects()
        {
            RestoreSceneObjects(); // Clean any previous cache

            // 1. Hide explicit named objects
            if (sceneObjectsToHide != null)
            {
                foreach (string objName in sceneObjectsToHide)
                {
                    if (string.IsNullOrEmpty(objName)) continue;
                    GameObject go = GameObject.Find(objName);
                    if (go != null && go.activeSelf)
                    {
                        go.SetActive(false);
                        hiddenSceneObjects.Add(go);
                    }
                }
            }

            // 2. Hide other active root scene objects (excluding cameras, managers, and canvas)
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (activeScene.isLoaded)
            {
                var roots = activeScene.GetRootGameObjects();
                foreach (var root in roots)
                {
                    if (root == null || !root.activeSelf) continue;
                    if (root.name == "Main Camera" || root.name == "Canvas_MainUI" || root.name == "_Managers" || root.name == "EventSystem" || root.name.StartsWith("Test_")) continue;
                    if (!hiddenSceneObjects.Contains(root))
                    {
                        root.SetActive(false);
                        hiddenSceneObjects.Add(root);
                    }
                }
            }

            Debug.Log($"[UI:InspectModal] Isolated scene: hidden {hiddenSceneObjects.Count} game objects.");
        }

        /// <summary>
        /// Restores all previously hidden scene objects to their active state.
        /// </summary>
        public void RestoreSceneObjects()
        {
            if (hiddenSceneObjects.Count == 0) return;

            Debug.Log($"[UI:InspectModal] Restoring {hiddenSceneObjects.Count} hidden scene objects.");
            foreach (var go in hiddenSceneObjects)
            {
                if (go != null)
                {
                    go.SetActive(true);
                }
            }
            hiddenSceneObjects.Clear();
        }

        /// <summary>
        /// Rotates the evidence object using mouse cursor drag delta.
        /// </summary>
        /// <param name="deltaAngle">The rotation angle delta in degrees.</param>
        public void RotateWithCursor(float deltaAngle)
        {
            targetRotationAngle += deltaAngle;
            if (!smoothRotation)
            {
                currentRotationAngle = targetRotationAngle;
                ApplyTransform();
            }
        }

        /// <summary>
        /// Sets the target zoom magnification factor, clamping between <see cref="minZoom"/> and <see cref="maxZoom"/>.
        /// </summary>
        /// <param name="zoom">The new target zoom scale factor.</param>
        public void SetTargetZoom(float zoom)
        {
            float oldZoom = targetZoom;
            targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);

            ClampPan();

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

            if (!smoothZoom)
            {
                currentZoom = targetZoom;
                currentPanPosition = targetPanPosition;
                ApplyTransform();
            }
        }

        /// <summary>
        /// Resets target zoom to 1.0x, centers pan offset to (0,0), and resets sprite rotation to 0°.
        /// </summary>
        public void ResetView()
        {
            ResetZoom();
            targetRotationAngle = 0f;
            currentRotationAngle = 0f;

            if (evidenceZoomImage != null)
            {
                evidenceZoomImage.rectTransform.localRotation = Quaternion.identity;
            }

            ApplyTransform();
            Debug.Log("[UI:InspectModal] View fully reset (Zoom: 1.0x, Pan: (0,0), Rotation: 0°)");
        }

        /// <summary>
        /// Handles mouse scroll wheel events over the viewport, smoothly zooming towards cursor position.
        /// </summary>
        /// <param name="eventData">Pointer event data from Unity EventSystem.</param>
        public void OnScroll(PointerEventData eventData)
        {
            if (Mathf.Abs(eventData.scrollDelta.y) < 0.001f) return;

            float deltaZoom = eventData.scrollDelta.y * scrollSensitivity;
            float newZoom = Mathf.Clamp(targetZoom + deltaZoom, minZoom, maxZoom);

            if (Mathf.Abs(newZoom - targetZoom) < 0.001f) return;

            // Center zoom towards pointer local position
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
        /// Handles begin drag pointer event.
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
        /// Handles drag pointer event:
        /// - Left Mouse Button: controls rotation of the evidence via horizontal/tangential cursor movement.
        /// - Middle Mouse Button or zoomed dragging: translates pan offset.
        /// </summary>
        /// <param name="eventData">Pointer event data from Unity EventSystem.</param>
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            // 1. Left Mouse Button Drag: Rotate Evidence with cursor
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                float deltaAngle = -eventData.delta.x * rotationSensitivity;
                RotateWithCursor(deltaAngle);
            }
            // 2. Middle Mouse Button Drag: Pan when zoomed
            else if (eventData.button == PointerEventData.InputButton.Middle && targetZoom > minZoom + 0.001f)
            {
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
        }

        /// <summary>
        /// Handles pointer clicks on the modal viewport / background:
        /// - Right-Click anywhere closes inspection.
        /// - Left-Click directly on the transparent background outside the evidence image closes inspection.
        /// </summary>
        /// <param name="eventData">Pointer event data from EventSystem.</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                CloseInspect();
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Left && !eventData.dragging)
            {
                // If the clicked object is this background panel/viewport (outside the evidence image itself)
                if (eventData.pointerCurrentRaycast.gameObject == gameObject || 
                    (viewportRectTransform != null && eventData.pointerCurrentRaycast.gameObject == viewportRectTransform.gameObject))
                {
                    Debug.Log("[UI:InspectModal] Background clicked outside evidence - closing inspect mode");
                    CloseInspect();
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
        /// Closes the evidence inspection mode and notifies <see cref="EvidenceManager"/>.
        /// </summary>
        public void CloseInspect()
        {
            Debug.Log("[UI:InspectModal] Closing isolated evidence inspection mode");
            isInspecting = false;
            EvidenceManager.Instance?.CloseInspectModal();
            RestoreSceneObjects();
            ResetView();
            ArmPointerController.Instance?.ForceSyncState();
        }

        /// <summary>
        /// Handler for external close notifications from <see cref="EvidenceManager"/>.
        /// </summary>
        private void HandleInspectClosed()
        {
            isInspecting = false;
            RestoreSceneObjects();
            ResetView();
            ArmPointerController.Instance?.ForceSyncState();
        }
    }
}
