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
    public class EvidenceInspectModal : MonoBehaviour, IScrollHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [Header("Evidence Visuals")]
        public Image evidenceZoomImage;
        public RectTransform viewportRectTransform;
        public RectTransform hotspotsContainer;
        [Tooltip("Optional prefab for interactive hotspot markers on the inspected evidence.")]
        [SerializeField] public GameObject hotspotMarkerPrefab;
        public Text clueUnlockedNotificationText;

        [Header("Background Configuration")]
        [Tooltip("Background Image displaying the top-down view of the table during evidence inspection.")]
        public Image backgroundImage;
        [Tooltip("Sprite for the top-down view of the table (TableTOPVIEW).")]
        public Sprite tableTopViewSprite;

        private static readonly Color DiscoveredHotspotColor = new Color(0.2f, 0.8f, 0.2f, 0.6f);
        private static readonly Color UndiscoveredHotspotColor = new Color(0.9f, 0.7f, 0.1f, 0.8f);

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

        public float CurrentZoom => currentZoom;

        public float TargetZoom => targetZoom;

        public float CurrentRotationAngle => currentRotationAngle;

        public float TargetRotationAngle => targetRotationAngle;

        public Vector2 CurrentPanPosition => currentPanPosition;

        public Vector2 TargetPanPosition => targetPanPosition;

        public bool IsInspecting => isInspecting;

        public IReadOnlyList<GameObject> HiddenSceneObjects => hiddenSceneObjects;

        private void Awake()
        {
            EnsureBackgroundSetup();

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

        public void EnsureBackgroundSetup()
        {
            if (backgroundImage == null)
            {
                var bgChild = transform.Find("Image_Background") ?? transform.Find("Background");
                if (bgChild != null)
                {
                    backgroundImage = bgChild.GetComponent<Image>();
                }
                if (backgroundImage == null)
                {
                    backgroundImage = GetComponent<Image>();
                }
            }

#if UNITY_EDITOR
            if (tableTopViewSprite == null)
            {
                tableTopViewSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Assets/BACKGROUNDS/TableTOPVIEW.png");
                if (tableTopViewSprite == null)
                {
                    var allSubAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Assets/BACKGROUNDS/TableTOPVIEW.png");
                    foreach (var sub in allSubAssets)
                    {
                        if (sub is Sprite s)
                        {
                            tableTopViewSprite = s;
                            break;
                        }
                    }
                }
            }
#endif

            if (backgroundImage != null)
            {
                if (backgroundImage.sprite == null && tableTopViewSprite != null)
                {
                    backgroundImage.sprite = tableTopViewSprite;
                }
                backgroundImage.color = (backgroundImage.sprite != null) ? Color.white : Color.clear;
                backgroundImage.raycastTarget = true;
            }
        }

        private void OnEnable()
        {
            RegisterManagerEvents();

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

            bool needsTransformUpdate = false;

            if (Mathf.Abs(currentZoom - targetZoom) > 0.0005f || (currentPanPosition - targetPanPosition).sqrMagnitude > 0.001f)
            {
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
                needsTransformUpdate = true;
            }
            else if (currentZoom != targetZoom || currentPanPosition != targetPanPosition)
            {
                currentZoom = targetZoom;
                currentPanPosition = targetPanPosition;
                needsTransformUpdate = true;
            }

            if (Mathf.Abs(currentRotationAngle - targetRotationAngle) > 0.05f)
            {
                if (smoothRotation)
                {
                    currentRotationAngle = Mathf.Lerp(currentRotationAngle, targetRotationAngle, Time.unscaledDeltaTime * rotationLerpSpeed);
                }
                else
                {
                    currentRotationAngle = targetRotationAngle;
                }
                needsTransformUpdate = true;
            }
            else if (currentRotationAngle != targetRotationAngle)
            {
                currentRotationAngle = targetRotationAngle;
                needsTransformUpdate = true;
            }

            if (needsTransformUpdate)
            {
                ApplyTransform();
            }
        }

        public void ApplyTransform()
        {
            if (evidenceZoomImage != null)
            {
                evidenceZoomImage.rectTransform.localScale = new Vector3(currentZoom, currentZoom, 1f);
                evidenceZoomImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, currentRotationAngle);
                evidenceZoomImage.rectTransform.anchoredPosition = currentPanPosition;
            }
        }

        public void DisplayEvidence(EvidenceSO evidence)
        {
            currentEvidence = evidence;
            if (evidence == null) return;

            Debug.Log($"[UI:InspectModal] Entering isolated inspection for evidence '{evidence.evidenceName}' (ID: {evidence.id})");
            isInspecting = true;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            Sprite targetSprite = evidence.zoomedSprite != null ? evidence.zoomedSprite : evidence.normalSprite;
            if (evidenceZoomImage != null)
            {
                evidenceZoomImage.sprite = targetSprite;
                evidenceZoomImage.preserveAspect = true;
                evidenceZoomImage.enabled = (targetSprite != null);
                evidenceZoomImage.color = (targetSprite != null) ? Color.white : Color.clear;
            }

            EnsureBackgroundSetup();
            if (backgroundImage != null)
            {
                Sprite bgSprite = (evidence.customInspectBackground != null)
                    ? evidence.customInspectBackground
                    : tableTopViewSprite;

                if (bgSprite != null)
                {
                    backgroundImage.sprite = bgSprite;
                    backgroundImage.color = Color.white;
                    backgroundImage.enabled = true;
                }
            }

            ResetView();

            HideSceneObjects();

            if (clueUnlockedNotificationText != null)
            {
                clueUnlockedNotificationText.gameObject.SetActive(false);
            }

            PopulateHotspots(evidence);
        }

        public void HideSceneObjects()
        {
            RestoreSceneObjects();

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

        public void RestoreSceneObjects()
        {
            if (hiddenSceneObjects.Count > 0)
            {
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

            if (ArmPointerController.Instance != null)
            {
                if (!ArmPointerController.Instance.gameObject.activeSelf)
                {
                    ArmPointerController.Instance.gameObject.SetActive(true);
                }
                ArmPointerController.Instance.ResetTapState();
            }
        }

        public void RotateWithCursor(float deltaAngle)
        {
            targetRotationAngle += deltaAngle;
            if (!smoothRotation)
            {
                currentRotationAngle = targetRotationAngle;
                ApplyTransform();
            }
        }

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

        public void OnScroll(PointerEventData eventData)
        {
            if (Mathf.Abs(eventData.scrollDelta.y) < 0.001f) return;

            float deltaZoom = eventData.scrollDelta.y * scrollSensitivity;
            float newZoom = Mathf.Clamp(targetZoom + deltaZoom, minZoom, maxZoom);

            if (Mathf.Abs(newZoom - targetZoom) < 0.001f) return;

            RectTransform panParent = GetPanParentRect();
            if (panParent != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(panParent, eventData.position, eventData.pressEventCamera, out Vector2 cursorLocalPoint))
            {
                float zoomRatio = newZoom / targetZoom;
                targetPanPosition = (targetPanPosition - cursorLocalPoint) * zoomRatio + cursorLocalPoint;
            }

            SetTargetZoom(newZoom);
            AudioManager.Instance?.PlayExamineZoom();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            RectTransform panParent = GetPanParentRect();
            if (panParent != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(panParent, eventData.position, eventData.pressEventCamera, out lastDragLocalPos))
            {
                isDragging = true;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                float deltaAngle = -eventData.delta.x * rotationSensitivity;
                RotateWithCursor(deltaAngle);
            }
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

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                CloseInspect();
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Left && !eventData.dragging)
            {
                GameObject clicked = eventData.pointerCurrentRaycast.gameObject;
                if (clicked == gameObject ||
                    (viewportRectTransform != null && clicked == viewportRectTransform.gameObject) ||
                    (backgroundImage != null && clicked == backgroundImage.gameObject))
                {
                    Debug.Log("[UI:InspectModal] Background clicked outside evidence - closing inspect mode");
                    CloseInspect();
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
        }

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

        private RectTransform GetPanParentRect()
        {
            if (viewportRectTransform != null) return viewportRectTransform;
            if (evidenceZoomImage != null && evidenceZoomImage.rectTransform.parent != null)
            {
                return evidenceZoomImage.rectTransform.parent as RectTransform;
            }
            return transform as RectTransform;
        }

        private void PopulateHotspots(EvidenceSO evidence)
        {
            if (hotspotsContainer == null) return;

            foreach (Transform child in hotspotsContainer)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }

            if (evidence.hotspots == null || evidence.hotspots.Count == 0) return;

            foreach (var spot in evidence.hotspots)
            {
                if (spot == null) continue;

                EvidenceHotspot currentSpot = spot;

                if (hotspotMarkerPrefab != null)
                {
                    GameObject spotObj = Instantiate(hotspotMarkerPrefab, hotspotsContainer, false);
                    spotObj.name = $"Hotspot_{currentSpot.hotspotId}";

                    RectTransform rt = spotObj.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchorMin = currentSpot.normalizedPosition;
                        rt.anchorMax = currentSpot.normalizedPosition;
                        rt.anchoredPosition = Vector2.zero;
                    }

                    Image img = spotObj.GetComponent<Image>() ?? spotObj.GetComponentInChildren<Image>();
                    if (img != null)
                    {
                        img.color = IsHotspotDiscovered(evidence, currentSpot) ? DiscoveredHotspotColor : UndiscoveredHotspotColor;
                    }

                    Text labelText = spotObj.GetComponentInChildren<Text>();
                    if (labelText != null)
                    {
                        labelText.fontStyle = FontStyle.Bold;
                        labelText.text = !string.IsNullOrEmpty(currentSpot.hotspotTitle) ? currentSpot.hotspotTitle.ToUpperInvariant() : "";
                    }

                    Button btn = spotObj.GetComponent<Button>() ?? spotObj.GetComponentInChildren<Button>();
                    if (btn == null) btn = spotObj.AddComponent<Button>();

                    btn.onClick.AddListener(() =>
                    {
                        Debug.Log($"[UI:InspectModal] Hotspot clicked '{currentSpot.hotspotTitle}' (ID: {currentSpot.hotspotId}) on evidence '{evidence.evidenceName}'");
                        EvidenceManager.Instance?.DiscoverHotspot(evidence, currentSpot);
                    });
                }
                else
                {
                    GameObject spotObj = new GameObject($"Hotspot_{currentSpot.hotspotId}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                    spotObj.transform.SetParent(hotspotsContainer, false);

                    RectTransform rt = spotObj.GetComponent<RectTransform>();
                    rt.anchorMin = currentSpot.normalizedPosition;
                    rt.anchorMax = currentSpot.normalizedPosition;
                    rt.sizeDelta = new Vector2(40f, 40f);

                    Image img = spotObj.GetComponent<Image>();
                    img.color = IsHotspotDiscovered(evidence, currentSpot) ? DiscoveredHotspotColor : UndiscoveredHotspotColor;

                    spotObj.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        Debug.Log($"[UI:InspectModal] Hotspot clicked '{currentSpot.hotspotTitle}' (ID: {currentSpot.hotspotId}) on evidence '{evidence.evidenceName}'");
                        EvidenceManager.Instance?.DiscoverHotspot(evidence, currentSpot);
                    });
                }
            }

            if (hotspotsContainer != null)
            {
                UIButtonHighlightSystem.ApplyToHierarchy(hotspotsContainer.gameObject);
            }
        }

        private void HandleHotspotDiscovered(EvidenceHotspot hotspot)
        {
            Debug.Log($"[UI:InspectModal] Hotspot discovered notification shown for '{hotspot.hotspotTitle}'");

            if (clueUnlockedNotificationText != null)
            {
                clueUnlockedNotificationText.gameObject.SetActive(true);
                clueUnlockedNotificationText.color = DetectiveUITheme.Ink;
                clueUnlockedNotificationText.fontStyle = FontStyle.Bold;
                clueUnlockedNotificationText.text = $"[NEW CLUE DISCOVERED]\n{hotspot.observationText}";

                Image bannerBg = clueUnlockedNotificationText.GetComponentInParent<Image>();
                if (bannerBg != null && bannerBg != backgroundImage)
                {
                    DetectiveUITheme.Surface(bannerBg, DetectiveUITheme.Paper, true);
                }
            }

            if (currentEvidence != null)
            {
                PopulateHotspots(currentEvidence);
            }
        }

        private static bool IsHotspotDiscovered(EvidenceSO evidence, EvidenceHotspot hotspot)
        {
            CaseManager caseManager = CaseManager.Instance;
            return caseManager != null && caseManager.IsHotspotDiscovered(evidence, hotspot);
        }

        public void CloseInspect()
        {
            Debug.Log("[UI:InspectModal] Closing isolated evidence inspection mode");
            isInspecting = false;
            RestoreSceneObjects();
            ResetView();
            EvidenceManager.Instance?.CloseInspectModal();
            ArmPointerController.Instance?.ResetTapState();
            ArmPointerController.Instance?.ForceSyncState();
        }

        private void HandleInspectClosed()
        {
            isInspecting = false;
            RestoreSceneObjects();
            ResetView();
            ArmPointerController.Instance?.ResetTapState();
            ArmPointerController.Instance?.ForceSyncState();
        }
    }
}
