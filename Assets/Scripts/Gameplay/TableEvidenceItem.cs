using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;
using CaseClosed.UI;

namespace CaseClosed.Gameplay
{
    /// <summary>
    /// Gameplay MonoBehaviour attached to physical evidence and interactive items placed on the investigation desk,
    /// providing luminous glowing aura feedback when hovered by the detective's arm pointer, hover highlights,
    /// single-click selection/explanation, double-click zoom inspection, and case file notebook opening triggers.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class TableEvidenceItem : MonoBehaviour
    {
        [Header("Evidence Data Link")]
        [Tooltip("The ScriptableObject defining this evidence item. Leave empty if this is a general book/tool prop.")]
        public EvidenceSO evidenceData;

        [Tooltip("Identifier matching an EvidenceSO.id (e.g. 'EVD_FAMILY_PHOTO', 'EVD_CRIME_KNIFE', 'EVD_COFFEE_CUP') to automatically bind at runtime.")]
        public string evidenceId;

        [Header("Interactive Behavior Mode")]
        [Tooltip("If true, clicking this item on the table directly opens the Case File Notebook (e.g. for the open case book on desk).")]
        public bool openNotebookOnClick = false;

        [Tooltip("Optional dialogue node ID to jump to in InterrogationManager when this item is clicked (suspect explains item).")]
        public string dialogueNodeToTriggerOnInspect;

        [Header("Glow Aura & Visual Feedback")]
        public SpriteRenderer spriteRenderer;

        [ColorUsage(true, true)]
        [Tooltip("HDR/RGB color of the glowing silhouette aura surrounding the evidence item when hovered.")]
        public Color glowColor = new Color(1.0f, 0.88f, 0.25f, 0.95f);

        [Tooltip("Maximum glow intensity multiplier when hovered.")]
        public float maxGlowIntensity = 1.0f;

        [Tooltip("Base scale expansion factor for the glowing silhouette halo.")]
        public float haloBaseScale = 1.08f;

        [Tooltip("Speed of fading the glow in and out.")]
        public float glowFadeSpeed = 12f;

        [Tooltip("If true, the glow aura gently breathes and pulses while hovered.")]
        public bool pulseGlow = true;

        [Tooltip("Speed of the breathing pulse.")]
        public float pulseSpeed = 4.0f;

        [Tooltip("Amplitude of the breathing pulse expansion & alpha oscillation.")]
        public float pulseAmplitude = 0.04f;

        [Header("Tactile Hover Response")]
        [Tooltip("If true, slightly lifts and scales the item up when hovered.")]
        public bool scaleOnHover = true;

        [Tooltip("Scale multiplier applied during hover.")]
        public float hoverScaleMultiplier = 1.035f;

        [Tooltip("Subtle brightness/tint boost applied to the item sprite during hover.")]
        public Color hoverColor = new Color(1f, 1f, 0.9f, 1f);

        [Tooltip("Optional custom external glow GameObject (e.g. inspector light or particle aura).")]
        public GameObject highlightGlow;

        [Header("Discovery Flash & Pulse Cue")]
        [Tooltip("Duration in seconds of the discovery glow cue when an item is revealed on the desk.")]
        public float discoveryCueDuration = 2.5f;

        [Tooltip("Peak glow intensity multiplier during discovery cue.")]
        public float discoveryGlowIntensity = 1.6f;

        [ColorUsage(true, true)]
        [Tooltip("Luminous HDR glow tint for the discovery celebration cue.")]
        public Color discoveryGlowColor = new Color(1.0f, 0.95f, 0.35f, 1.0f);

        // Runtime animation state
        private bool isHovered = false;
        private float currentGlowIntensity = 0f;
        private float discoveryCueTimer = 0f;
        [SerializeField, HideInInspector] private Vector3 baseScale = Vector3.one;
        private bool hasCachedBaseScale = false;
        private Color originalColor = Color.white;

        // Auto-generated glow halo child
        private GameObject haloObj;
        private SpriteRenderer haloRenderer;
        private Collider2D itemCollider;
        private CaseManager subscribedCaseManager;

        public bool IsHovered => isHovered;
        public float CurrentGlowIntensity => currentGlowIntensity;
        public bool IsDiscovered => CheckIsDiscovered();
        public bool IsItemVisible => openNotebookOnClick || (spriteRenderer != null && spriteRenderer.enabled);
        public bool IsDiscoveryCueActive => discoveryCueTimer > 0f;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (itemCollider == null) itemCollider = GetComponent<Collider2D>();

            EnsureBaseScaleCached();

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }

            SetupGlowHalo();
            ResolveEvidenceData();
        }

        private void Start()
        {
            EnsureBaseScaleCached();

            SubscribeToCaseManager();

            ResolveEvidenceData();
            BindEvidenceData();
            AdjustColliderToSprite();

            if (highlightGlow != null) highlightGlow.SetActive(false);
            if (haloObj != null) haloObj.SetActive(false);

            UpdateVisibilityState();
        }

        private void OnEnable()
        {
            EnsureBaseScaleCached();
        }

        private void OnDestroy()
        {
            UnsubscribeFromCaseManager();
        }

        public void SubscribeToCaseManager()
        {
            if (CaseManager.Instance != null && subscribedCaseManager != CaseManager.Instance)
            {
                UnsubscribeFromCaseManager();

                subscribedCaseManager = CaseManager.Instance;
                subscribedCaseManager.OnCaseLoaded += HandleCaseLoaded;
                subscribedCaseManager.OnEvidenceDiscovered += HandleEvidenceDiscovered;

                if (subscribedCaseManager.activeCase != null)
                {
                    HandleCaseLoaded(subscribedCaseManager.activeCase);
                }
            }
        }

        public void UnsubscribeFromCaseManager()
        {
            if (subscribedCaseManager != null)
            {
                subscribedCaseManager.OnCaseLoaded -= HandleCaseLoaded;
                subscribedCaseManager.OnEvidenceDiscovered -= HandleEvidenceDiscovered;
                subscribedCaseManager = null;
            }
            else if (CaseManager.Instance != null)
            {
                CaseManager.Instance.OnCaseLoaded -= HandleCaseLoaded;
                CaseManager.Instance.OnEvidenceDiscovered -= HandleEvidenceDiscovered;
            }
        }

        private void EnsureBaseScaleCached()
        {
            if (!hasCachedBaseScale || baseScale.sqrMagnitude < 0.00001f)
            {
                if (transform.localScale.sqrMagnitude > 0.00001f)
                {
                    baseScale = transform.localScale;
                    hasCachedBaseScale = true;
                }
            }
        }

        private void Update()
        {
            if (subscribedCaseManager == null && CaseManager.Instance != null)
            {
                SubscribeToCaseManager();
                UpdateVisibilityState();
            }

            UpdateGlowAnimation();
            CheckDirectMouseInteraction();
        }

        private void CheckDirectMouseInteraction()
        {
            if (!openNotebookOnClick && spriteRenderer != null && !spriteRenderer.enabled) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            // Disallow desk interaction if UI modal or inspection modal is currently active
            if (UIManager.Instance != null && UIManager.Instance.currentPanel != UIPanelType.InvestigationTable) return;
            if (EvidenceManager.Instance != null && EvidenceManager.Instance.isInspectingModalOpen) return;

            Vector3 mouseScreen = Input.mousePosition;
            Vector3 mouseWorld3D = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, -cam.transform.position.z));
            Vector2 mouseWorld = new Vector2(mouseWorld3D.x, mouseWorld3D.y);

            if (itemCollider == null) itemCollider = GetComponent<Collider2D>();
            if (itemCollider == null || !itemCollider.enabled) return;

            bool isOver = itemCollider.OverlapPoint(mouseWorld);

            // If arm pointer is inactive or absent, provide direct cursor hover highlights
            if (ArmPointerController.Instance == null || !ArmPointerController.Instance.isArmActive)
            {
                if (isOver && !isHovered) SetHoverState(true);
                else if (!isOver && isHovered) SetHoverState(false);
            }

            if (isOver)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    TriggerClick(false);
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    TriggerClick(true);
                }
            }
        }

        /// <summary>
        /// Creates and configures the dedicated glowing silhouette child object behind the evidence item.
        /// </summary>
        private void SetupGlowHalo()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;

            Transform child = transform.Find("Glow_Halo");
            if (child != null)
            {
                haloObj = child.gameObject;
                haloRenderer = haloObj.GetComponent<SpriteRenderer>();
            }
            else
            {
                haloObj = new GameObject("Glow_Halo");
                haloObj.transform.SetParent(transform, false);
                haloObj.transform.localPosition = new Vector3(0f, 0f, 0.01f);
                haloRenderer = haloObj.AddComponent<SpriteRenderer>();
            }

            if (haloRenderer != null)
            {
                haloRenderer.sprite = spriteRenderer.sprite;
                haloRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
                haloRenderer.sortingOrder = Mathf.Max(0, spriteRenderer.sortingOrder - 1);

                Shader unlitShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default");
                if (unlitShader != null)
                {
                    haloRenderer.material = new Material(unlitShader);
                }

                haloRenderer.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0f);
            }

            haloObj.SetActive(false);
        }

        /// <summary>
        /// Smoothly animates glow intensity, breathing pulse oscillation, discovery cue, and subtle scale lift.
        /// </summary>
        private void UpdateGlowAnimation()
        {
            EnsureBaseScaleCached();

            if (haloObj == null)
            {
                SetupGlowHalo();
            }

            bool isCueActive = discoveryCueTimer > 0f;
            if (isCueActive)
            {
                discoveryCueTimer -= Time.deltaTime;
                if (discoveryCueTimer < 0f)
                {
                    discoveryCueTimer = 0f;
                    isCueActive = false;
                }
            }

            float targetIntensity = 0f;
            if (isHovered)
            {
                targetIntensity = maxGlowIntensity;
            }
            else if (isCueActive)
            {
                float cueProgress = Mathf.Clamp01(discoveryCueTimer / discoveryCueDuration);
                targetIntensity = discoveryGlowIntensity * cueProgress;
            }

            currentGlowIntensity = Mathf.MoveTowards(currentGlowIntensity, targetIntensity, Time.deltaTime * glowFadeSpeed);

            if (currentGlowIntensity > 0.001f || isCueActive)
            {
                if (haloObj != null && !haloObj.activeSelf)
                {
                    haloObj.SetActive(true);
                }

                if (haloRenderer != null)
                {
                    if (spriteRenderer != null && haloRenderer.sprite != spriteRenderer.sprite)
                    {
                        haloRenderer.sprite = spriteRenderer.sprite;
                    }

                    Color activeColor = glowColor;
                    float pulse = 1.0f;

                    if (isCueActive)
                    {
                        float cueProgress = Mathf.Clamp01(discoveryCueTimer / discoveryCueDuration);
                        activeColor = Color.Lerp(glowColor, discoveryGlowColor, cueProgress);
                        pulse = 1.0f + Mathf.Sin(Time.time * 8.0f) * 0.12f;
                    }
                    else if (pulseGlow && isHovered)
                    {
                        pulse = 1.0f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
                    }

                    float alpha = Mathf.Clamp01(Mathf.Max(currentGlowIntensity, isCueActive ? 0.35f : 0f) * pulse * activeColor.a);
                    haloRenderer.color = new Color(activeColor.r, activeColor.g, activeColor.b, alpha);

                    float scaleFactor = (haloBaseScale + ((pulse - 1.0f) * 0.5f)) * (isCueActive ? 1.05f : 1.0f);
                    haloObj.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
                }

                if (spriteRenderer != null)
                {
                    Color targetTint = isCueActive ? Color.Lerp(hoverColor, discoveryGlowColor, 0.4f) : hoverColor;
                    spriteRenderer.color = Color.Lerp(originalColor, targetTint, Mathf.Clamp01(currentGlowIntensity));
                }

                if (highlightGlow != null)
                {
                    highlightGlow.SetActive(isHovered || isCueActive);
                }
            }
            else
            {
                if (haloObj != null && haloObj.activeSelf)
                {
                    haloObj.SetActive(false);
                }

                if (spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }

                if (highlightGlow != null && !isHovered)
                {
                    highlightGlow.SetActive(false);
                }
            }

            if (scaleOnHover && hasCachedBaseScale && baseScale.sqrMagnitude > 0.0001f)
            {
                bool shouldScale = isHovered || isCueActive;
                float scaleMul = isHovered ? hoverScaleMultiplier : 1.025f;
                Vector3 targetScale = shouldScale ? (baseScale * scaleMul) : baseScale;
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * glowFadeSpeed);
            }
        }

        /// <summary>
        /// Auto-sizes BoxCollider2D to match the active sprite's size and center if needed.
        /// </summary>
        public void AdjustColliderToSprite()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            BoxCollider2D boxCol = GetComponent<BoxCollider2D>();

            if (boxCol != null && spriteRenderer != null && spriteRenderer.sprite != null)
            {
                Vector2 spriteSize = spriteRenderer.sprite.rect.size / spriteRenderer.sprite.pixelsPerUnit;
                if (boxCol.size.x < spriteSize.x * 0.7f || boxCol.size.y < spriteSize.y * 0.7f)
                {
                    boxCol.size = spriteSize;
                    boxCol.offset = Vector2.zero;
                }
            }
        }

        /// <summary>
        /// Attempts to resolve the EvidenceSO data from the active case or assigned identifier.
        /// </summary>
        public EvidenceSO ResolveEvidenceData()
        {
            if (evidenceData != null)
            {
                if (string.IsNullOrEmpty(evidenceId)) evidenceId = evidenceData.id;
                return evidenceData;
            }

            if (CaseManager.Instance != null && CaseManager.Instance.activeCase != null)
            {
                var evList = CaseManager.Instance.activeCase.evidenceItems;
                if (evList != null)
                {
                    // 1. Exact ID match
                    if (!string.IsNullOrEmpty(evidenceId))
                    {
                        foreach (var ev in evList)
                        {
                            if (ev != null && (ev.id == evidenceId || ev.id.Equals(evidenceId, System.StringComparison.OrdinalIgnoreCase)))
                            {
                                evidenceData = ev;
                                BindEvidenceData();
                                return evidenceData;
                            }
                        }
                    }

                    // 2. Loose / partial name match fallback
                    string cleanObjName = gameObject.name.ToUpper();
                    foreach (var ev in evList)
                    {
                        if (ev != null)
                        {
                            string cleanEvId = ev.id.ToUpper();
                            if ((!string.IsNullOrEmpty(evidenceId) && cleanEvId.Contains(evidenceId.ToUpper())) ||
                                cleanObjName.Contains(cleanEvId) || cleanEvId.Contains(cleanObjName))
                            {
                                evidenceData = ev;
                                BindEvidenceData();
                                return evidenceData;
                            }
                        }
                    }
                }
            }

            return evidenceData;
        }

        private void HandleCaseLoaded(CaseSO activeCase)
        {
            if (activeCase == null) return;

            ResolveEvidenceData();

            string effectiveId = !string.IsNullOrEmpty(evidenceId) ? evidenceId : evidenceData?.id;

            if (activeCase.evidenceItems != null && !string.IsNullOrEmpty(effectiveId))
            {
                foreach (var ev in activeCase.evidenceItems)
                {
                    if (ev != null && (ev.id == effectiveId || ev.id.Equals(effectiveId, System.StringComparison.OrdinalIgnoreCase)))
                    {
                        evidenceData = ev;
                        evidenceId = ev.id;
                        BindEvidenceData();
                        AdjustColliderToSprite();
                        break;
                    }
                }
            }

            UpdateVisibilityState();
        }

        /// <summary>
        /// Evaluates whether this item has been discovered yet based on initial case data or runtime CaseManager.
        /// </summary>
        public bool CheckIsDiscovered()
        {
            if (openNotebookOnClick) return true;

            ResolveEvidenceData();

            string effectiveId = !string.IsNullOrEmpty(evidenceId) ? evidenceId : evidenceData?.id;
            bool isDiscovered = (evidenceData != null && evidenceData.startsDiscovered) ||
                                (CaseManager.Instance != null && !string.IsNullOrEmpty(effectiveId) && CaseManager.Instance.discoveredEvidenceIds.Contains(effectiveId));
            return isDiscovered;
        }

        /// <summary>
        /// Updates sprite visibility, collider responsiveness, and halo state based on current discovery status.
        /// </summary>
        public void UpdateVisibilityState()
        {
            if (openNotebookOnClick)
            {
                SetItemVisibility(true);
                return;
            }

            bool isDiscovered = CheckIsDiscovered();
            SetItemVisibility(isDiscovered);
        }

        /// <summary>
        /// Enables or disables the visual SpriteRenderer and physical Collider2D components.
        /// </summary>
        public void SetItemVisibility(bool visible)
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = visible;
            }

            if (itemCollider == null) itemCollider = GetComponent<Collider2D>();
            if (itemCollider != null)
            {
                itemCollider.enabled = visible;
            }

            if (!visible)
            {
                SetHoverState(false);
                StopDiscoveryCue();
            }
        }

        /// <summary>
        /// Callback invoked when any evidence is discovered in CaseManager.
        /// If this item matches the discovered evidence, reveals it on desk and triggers the discovery glow cue.
        /// </summary>
        public void HandleEvidenceDiscovered(EvidenceSO ev)
        {
            if (ev == null || openNotebookOnClick) return;

            ResolveEvidenceData();

            string effectiveId = !string.IsNullOrEmpty(evidenceId) ? evidenceId : evidenceData?.id;
            bool isMatch = (evidenceData != null && evidenceData == ev) ||
                           (!string.IsNullOrEmpty(effectiveId) && (ev.id == effectiveId || ev.id.Equals(effectiveId, System.StringComparison.OrdinalIgnoreCase)));

            if (!isMatch && evidenceData == null && !string.IsNullOrEmpty(ev.id))
            {
                string cleanObjName = gameObject.name.ToUpper();
                string cleanEvId = ev.id.ToUpper();
                if (cleanObjName.Contains(cleanEvId) || cleanEvId.Contains(cleanObjName))
                {
                    isMatch = true;
                }
            }

            if (isMatch)
            {
                if (evidenceData == null)
                {
                    evidenceData = ev;
                    if (string.IsNullOrEmpty(evidenceId)) evidenceId = ev.id;
                    BindEvidenceData();
                    AdjustColliderToSprite();
                }

                SetItemVisibility(true);
                TriggerDiscoveryCue();
            }
        }

        /// <summary>
        /// Triggers a luminous discovery flash and pulse glow cue to draw player focus to newly placed clues.
        /// </summary>
        public void TriggerDiscoveryCue(float duration = -1f)
        {
            discoveryCueTimer = duration > 0f ? duration : discoveryCueDuration;
            if (discoveryCueTimer <= 0f) discoveryCueTimer = 2.5f;

            currentGlowIntensity = discoveryGlowIntensity;

            EnsureBaseScaleCached();
            if (haloObj == null) SetupGlowHalo();

            if (haloObj != null)
            {
                haloObj.SetActive(true);
                if (haloRenderer != null)
                {
                    if (spriteRenderer != null && haloRenderer.sprite != spriteRenderer.sprite)
                    {
                        haloRenderer.sprite = spriteRenderer.sprite;
                    }
                    haloRenderer.color = new Color(discoveryGlowColor.r, discoveryGlowColor.g, discoveryGlowColor.b, discoveryGlowColor.a);
                    float initialScale = haloBaseScale * 1.15f;
                    haloObj.transform.localScale = new Vector3(initialScale, initialScale, 1f);
                }
            }

            if (highlightGlow != null)
            {
                highlightGlow.SetActive(true);
            }

            Debug.Log($"[Gameplay:TableEvidence] Triggered discovery glow cue on '{gameObject.name}' (Duration: {discoveryCueTimer:F1}s)");
        }

        /// <summary>
        /// Stops any active discovery cue immediately.
        /// </summary>
        public void StopDiscoveryCue()
        {
            discoveryCueTimer = 0f;
            if (!isHovered)
            {
                currentGlowIntensity = 0f;
                if (haloObj != null) haloObj.SetActive(false);
                if (spriteRenderer != null) spriteRenderer.color = originalColor;
                if (highlightGlow != null) highlightGlow.SetActive(false);
                if (hasCachedBaseScale && baseScale.sqrMagnitude > 0.0001f)
                {
                    transform.localScale = baseScale;
                }
            }
        }

        private void BindEvidenceData()
        {
            if (evidenceData != null && spriteRenderer != null && evidenceData.normalSprite != null)
            {
                spriteRenderer.sprite = evidenceData.normalSprite;
                if (haloRenderer != null) haloRenderer.sprite = evidenceData.normalSprite;
                AdjustColliderToSprite();
            }
        }

        private void OnMouseEnter()
        {
            if (!openNotebookOnClick && spriteRenderer != null && !spriteRenderer.enabled) return;
            SetHoverState(true);
        }

        private void OnMouseExit()
        {
            SetHoverState(false);
        }

        private void OnMouseDown()
        {
            if (!openNotebookOnClick && spriteRenderer != null && !spriteRenderer.enabled) return;
            TriggerClick(false);
        }

        /// <summary>
        /// Programmatically sets the hover visual state (used by ArmPointerController).
        /// Triggers glowing aura, scale lift, and highlighted sprite change.
        /// </summary>
        public void SetHoverState(bool isHovered)
        {
            if (isHovered && !openNotebookOnClick && spriteRenderer != null && !spriteRenderer.enabled)
            {
                isHovered = false;
            }

            this.isHovered = isHovered;

            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (haloObj == null) SetupGlowHalo();

            if (spriteRenderer != null && evidenceData != null)
            {
                if (isHovered && evidenceData.highlightedSprite != null)
                    spriteRenderer.sprite = evidenceData.highlightedSprite;
                else if (!isHovered && evidenceData.normalSprite != null)
                    spriteRenderer.sprite = evidenceData.normalSprite;

                if (haloRenderer != null) haloRenderer.sprite = spriteRenderer.sprite;
            }

            if (highlightGlow != null)
            {
                highlightGlow.SetActive(isHovered || IsDiscoveryCueActive);
            }
        }

        /// <summary>
        /// Programmatically triggers the interaction click on this table item (used by ArmPointerController).
        /// </summary>
        public void TriggerClick(bool isInspectOrRightClick = false)
        {
            // Disallow interaction if item is not visible on desk
            if (!openNotebookOnClick && spriteRenderer != null && !spriteRenderer.enabled) return;

            // OnMouseDown can reach this handler through UI, bypassing the polling guards.
            if (UIManager.Instance != null && UIManager.Instance.currentPanel != UIPanelType.InvestigationTable) return;
            if (EvidenceManager.Instance != null && EvidenceManager.Instance.isInspectingModalOpen) return;

            // 1. Check if configured to open notebook (Open Case Book on desk)
            if (openNotebookOnClick)
            {
                Debug.Log("[Gameplay:TableEvidence] Opening Case File Notebook from table book click");
                UIManager.Instance?.ShowPanel(UIPanelType.CaseFileNotebook);
                return;
            }

            ResolveEvidenceData();
            if (evidenceData == null)
            {
                if (CaseManager.Instance?.activeCase != null && CaseManager.Instance.activeCase.evidenceItems != null && CaseManager.Instance.activeCase.evidenceItems.Count > 0)
                {
                    evidenceData = CaseManager.Instance.activeCase.evidenceItems[0];
                    BindEvidenceData();
                }
            }

            if (evidenceData == null)
            {
                Debug.LogWarning($"[Gameplay:TableEvidence] Cannot trigger click: evidenceData is null for '{gameObject.name}' (evidenceId: '{evidenceId}')");
                return;
            }

            string itemName = evidenceData.evidenceName;
            Debug.Log($"[Gameplay:TableEvidence] TriggerClick on '{itemName}' (InspectMode: {isInspectOrRightClick}, OpenNotebook: {openNotebookOnClick})");

            // 2. Select evidence in EvidenceManager
            EvidenceManager.Instance?.SelectEvidence(evidenceData);

            // 3. If dialogue is currently active with a statement, clicking this table item directly presents it to challenge!
            if (DialogueUI.IsDialogueOpen && InterrogationManager.Instance != null && InterrogationManager.Instance.currentNode != null)
            {
                DialogueUI.Instance?.AlignToWorldTarget(transform);
                Debug.Log($"[Gameplay:TableEvidence] Presenting '{evidenceData.evidenceName}' directly from table to challenge statement '{InterrogationManager.Instance.currentNode.nodeId}'");
                AudioManager.Instance?.PlayButtonClick();
                CaseManager.Instance?.RegisterDiscoveredEvidence(evidenceData);
                InterrogationManager.Instance.PresentEvidenceToChallenge(evidenceData);
                return;
            }

            // 4. Otherwise (exploration mode / dialogue closed), single-click opens close-up inspect modal
            Debug.Log($"[Gameplay:TableEvidence] Opening inspect modal for '{evidenceData.evidenceName}'");
            EvidenceManager.Instance?.OpenInspectModal(evidenceData);

            DialogueUI.Instance?.AlignToWorldTarget(transform);

            string nodeToTrigger = !string.IsNullOrEmpty(dialogueNodeToTriggerOnInspect)
                ? dialogueNodeToTriggerOnInspect
                : evidenceData.dialogueNodeToTriggerOnInspect;
            if (!string.IsNullOrEmpty(nodeToTrigger))
            {
                Debug.Log($"[Gameplay:TableEvidence] Triggering suspect explanation dialogue node '{nodeToTrigger}' for '{evidenceData.evidenceName}'");
                InterrogationManager.Instance?.JumpToNode(nodeToTrigger);
            }
        }
    }
}
