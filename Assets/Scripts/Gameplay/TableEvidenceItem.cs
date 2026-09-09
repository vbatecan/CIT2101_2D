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

        public void SubscribeToCaseManager(CaseManager customManager = null)
        {
            CaseManager targetManager = customManager != null ? customManager : CaseManager.Instance;
            if (targetManager != null && subscribedCaseManager != targetManager)
            {
                UnsubscribeFromCaseManager();

                subscribedCaseManager = targetManager;
                subscribedCaseManager.OnCaseLoaded += HandleCaseLoaded;
                subscribedCaseManager.OnEvidenceDiscovered += HandleEvidenceDiscovered;

                if (subscribedCaseManager.ActiveCase != null)
                {
                    HandleCaseLoaded(subscribedCaseManager.ActiveCase);
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
            Camera cam = Camera.main;
            if (cam == null) return;

            // Disallow desk interaction if UI modal or inspection modal is currently active
            if (UIManager.Instance != null && UIManager.Instance.currentPanel != UIPanelType.InvestigationTable) return;
            if (EvidenceManager.Instance != null && EvidenceManager.Instance.isInspectingModalOpen) return;

            // If dialogue is open but challenge mode is NOT active, ignore direct clicks so dialogue input advances text instead
            if (DialogueUI.IsDialogueOpen && (InterrogationManager.Instance == null || !InterrogationManager.Instance.IsChallengeModeActive)) return;

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
        /// When in Challenge Mode, pulses with a golden selectable aura. Undiscovered items remain black silhouettes.
        /// </summary>
        private void UpdateGlowAnimation()
        {
            EnsureBaseScaleCached();

            if (haloObj == null)
            {
                SetupGlowHalo();
            }

            // If undiscovered, lock to pitch black silhouette and disable glowing aura
            if (!CheckIsDiscovered() && !openNotebookOnClick)
            {
                if (haloObj != null && haloObj.activeSelf) haloObj.SetActive(false);
                if (highlightGlow != null && highlightGlow.activeSelf) highlightGlow.SetActive(false);
                if (spriteRenderer != null) spriteRenderer.color = Color.black;
                if (scaleOnHover && hasCachedBaseScale && baseScale.sqrMagnitude > 0.0001f)
                {
                    transform.localScale = baseScale;
                }
                return;
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

            bool isChallengeActive = InterrogationManager.Instance != null && InterrogationManager.Instance.IsChallengeModeActive;

            float targetIntensity = 0f;
            if (isChallengeActive)
            {
                targetIntensity = 0.95f;
            }
            else if (isHovered)
            {
                targetIntensity = maxGlowIntensity;
            }
            else if (isCueActive)
            {
                float cueProgress = Mathf.Clamp01(discoveryCueTimer / discoveryCueDuration);
                targetIntensity = discoveryGlowIntensity * cueProgress;
            }

            currentGlowIntensity = Mathf.MoveTowards(currentGlowIntensity, targetIntensity, Time.deltaTime * glowFadeSpeed);

            if (currentGlowIntensity > 0.001f || isCueActive || isChallengeActive)
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

                    if (isChallengeActive)
                    {
                        // Pulsating golden challenge aura when waiting for evidence selection
                        activeColor = new Color(1.0f, 0.85f, 0.2f, 0.9f);
                        pulse = 1.0f + Mathf.Sin(Time.time * 5.0f) * 0.08f;
                    }
                    else if (isCueActive)
                    {
                        float cueProgress = Mathf.Clamp01(discoveryCueTimer / discoveryCueDuration);
                        activeColor = Color.Lerp(glowColor, discoveryGlowColor, cueProgress);
                        pulse = 1.0f + Mathf.Sin(Time.time * 8.0f) * 0.12f;
                    }
                    else if (pulseGlow && isHovered)
                    {
                        pulse = 1.0f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
                    }

                    float alpha = Mathf.Clamp01(Mathf.Max(currentGlowIntensity, isCueActive ? 0.35f : (isChallengeActive ? 0.45f : 0f)) * pulse * activeColor.a);
                    haloRenderer.color = new Color(activeColor.r, activeColor.g, activeColor.b, alpha);

                    float scaleFactor = (haloBaseScale + ((pulse - 1.0f) * 0.5f)) * (isCueActive ? 1.05f : 1.0f);
                    haloObj.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
                }

                if (spriteRenderer != null)
                {
                    if (isChallengeActive)
                    {
                        Color challengeTint = Color.Lerp(originalColor, new Color(1f, 0.95f, 0.7f, 1f), (Mathf.Sin(Time.time * 5.0f) + 1f) * 0.15f);
                        spriteRenderer.color = isHovered ? hoverColor : challengeTint;
                    }
                    else
                    {
                        Color targetTint = isCueActive ? Color.Lerp(hoverColor, discoveryGlowColor, 0.4f) : hoverColor;
                        spriteRenderer.color = Color.Lerp(originalColor, targetTint, Mathf.Clamp01(currentGlowIntensity));
                    }
                }

                if (highlightGlow != null)
                {
                    highlightGlow.SetActive(isHovered || isCueActive || isChallengeActive);
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
                bool shouldScale = isHovered || isCueActive || isChallengeActive;
                float scaleMul = isHovered ? hoverScaleMultiplier : (isChallengeActive ? 1.015f : 1.025f);
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
            CaseManager manager = subscribedCaseManager != null ? subscribedCaseManager : CaseManager.Instance;
            if (manager != null && manager.ActiveCase != null && manager.ActiveCase.evidenceItems != null)
            {
                if (evidenceData != null && !manager.ActiveCase.evidenceItems.Contains(evidenceData))
                {
                    evidenceData = null;
                    dialogueNodeToTriggerOnInspect = null;
                }
            }

            if (evidenceData != null)
            {
                if (string.IsNullOrEmpty(evidenceId)) evidenceId = evidenceData.id;
                return evidenceData;
            }
            if (manager != null && manager.ActiveCase != null)
            {
                var evList = manager.ActiveCase.evidenceItems;
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

                    // 3. Fallback by sibling index among TableEvidenceItem components on the investigation table
                    if (transform.parent != null)
                    {
                        var siblings = transform.parent.GetComponentsInChildren<TableEvidenceItem>(true);
                        int myIndex = System.Array.IndexOf(siblings, this);
                        if (myIndex >= 0 && myIndex < evList.Count && evList[myIndex] != null)
                        {
                            evidenceData = evList[myIndex];
                            evidenceId = evidenceData.id;
                            BindEvidenceData();
                            return evidenceData;
                        }
                    }
                }
            }

            return evidenceData;
        }

        private void HandleCaseLoaded(CaseSO activeCase)
        {
            if (activeCase == null) return;

            string effectiveId = !string.IsNullOrEmpty(evidenceId) ? evidenceId : evidenceData?.id;
            bool matched = false;

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
                        matched = true;
                        break;
                    }
                }
            }

            if (!matched && activeCase.evidenceItems != null && transform.parent != null)
            {
                var siblings = transform.parent.GetComponentsInChildren<TableEvidenceItem>(true);
                int myIndex = System.Array.IndexOf(siblings, this);
                if (myIndex >= 0 && myIndex < activeCase.evidenceItems.Count && activeCase.evidenceItems[myIndex] != null)
                {
                    evidenceData = activeCase.evidenceItems[myIndex];
                    evidenceId = evidenceData.id;
                    BindEvidenceData();
                    AdjustColliderToSprite();
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
            CaseManager manager = subscribedCaseManager != null ? subscribedCaseManager : CaseManager.Instance;
            bool isDiscovered = (evidenceData != null && evidenceData.startsDiscovered) ||
                                (manager != null && !string.IsNullOrEmpty(effectiveId) && manager.IsEvidenceDiscovered(effectiveId));
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
        /// Updates the visual rendering and collider responsiveness of this table evidence item.
        /// Discovered items display in full color with interactive glow; undiscovered clues appear as black silhouettes.
        /// </summary>
        public void SetItemVisibility(bool visible)
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                if (!visible && !openNotebookOnClick)
                {
                    spriteRenderer.color = Color.black;
                }
                else
                {
                    spriteRenderer.color = originalColor;
                }
            }

            if (itemCollider == null) itemCollider = GetComponent<Collider2D>();
            if (itemCollider != null)
            {
                itemCollider.enabled = true;
            }

            if (!visible && !openNotebookOnClick)
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
            if (evidenceData != null)
            {
                dialogueNodeToTriggerOnInspect = evidenceData.dialogueNodeToTriggerOnInspect;
                if (spriteRenderer != null && evidenceData.normalSprite != null)
                {
                    spriteRenderer.sprite = evidenceData.normalSprite;
                    if (haloRenderer != null) haloRenderer.sprite = evidenceData.normalSprite;
                    AdjustColliderToSprite();
                }
            }
        }

        private void OnMouseEnter()
        {
            SetHoverState(true);
        }

        private void OnMouseExit()
        {
            SetHoverState(false);
        }

        private void OnMouseDown()
        {
            TriggerClick(false);
        }

        /// <summary>
        /// Programmatically sets the hover visual state (used by ArmPointerController).
        /// Triggers glowing aura, scale lift, and highlighted sprite change.
        /// Undiscovered items do not display hover glowing highlights.
        /// </summary>
        public void SetHoverState(bool isHovered)
        {
            if (isHovered && !CheckIsDiscovered() && !openNotebookOnClick)
            {
                isHovered = false;
            }

            this.isHovered = isHovered;

            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (haloObj == null) SetupGlowHalo();

            if (!CheckIsDiscovered() && !openNotebookOnClick)
            {
                if (spriteRenderer != null) spriteRenderer.color = Color.black;
                if (haloObj != null) haloObj.SetActive(false);
                if (highlightGlow != null) highlightGlow.SetActive(false);
                return;
            }

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
                bool isChallengeActive = InterrogationManager.Instance != null && InterrogationManager.Instance.IsChallengeModeActive;
                highlightGlow.SetActive(isHovered || IsDiscoveryCueActive || isChallengeActive);
            }
        }

        /// <summary>
        /// Programmatically triggers the interaction click on this table item (used by ArmPointerController).
        /// During Challenge Mode, clicking a discovered item presents it to contradiction verification.
        /// Undiscovered items play a subtle locked cue.
        /// </summary>
        public void TriggerClick(bool isInspectOrRightClick = false)
        {
            // OnMouseDown can reach this handler through UI, bypassing the polling guards.
            if (UIManager.Instance != null && UIManager.Instance.currentPanel != UIPanelType.InvestigationTable) return;
            if (EvidenceManager.Instance != null && EvidenceManager.Instance.isInspectingModalOpen) return;

            // 0. If item is undiscovered, clicking gives a locked/dull cue and cannot be inspected or presented
            if (!CheckIsDiscovered() && !openNotebookOnClick)
            {
                Debug.Log($"[Gameplay:TableEvidence] Clicked undiscovered item '{gameObject.name}'. Clue is not yet revealed.");
                AudioManager.Instance?.PlayPaperFlip();
                return;
            }

            // 1. Check if configured to open notebook (Open Case Book on desk)
            if (openNotebookOnClick)
            {
                Debug.Log("[Gameplay:TableEvidence] Opening Case File Notebook from table book click");
                UIManager.Instance?.ShowPanel(UIPanelType.CaseFileNotebook);
                return;
            }

            ResolveEvidenceData();
            CaseManager manager = subscribedCaseManager != null ? subscribedCaseManager : CaseManager.Instance;
            if (evidenceData == null)
            {
                if (manager?.ActiveCase != null && manager.ActiveCase.evidenceItems != null && manager.ActiveCase.evidenceItems.Count > 0)
                {
                    evidenceData = manager.ActiveCase.evidenceItems[0];
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

            // 3. Challenge selection inspects unread evidence before it can be presented.
            if (InterrogationManager.Instance != null && InterrogationManager.Instance.IsChallengeModeActive)
            {
                if (isInspectOrRightClick)
                {
                    EvidenceManager.Instance?.OpenInspectModal(evidenceData, null);
                    return;
                }

                Debug.Log($"[Gameplay:TableEvidence] Challenge Mode: Presenting '{evidenceData.evidenceName}' directly from table to challenge statement '{InterrogationManager.Instance.CurrentNode?.nodeId}'");
                AudioManager.Instance?.PlayButtonClick();
                manager?.RegisterDiscoveredEvidence(evidenceData);
                InterrogationManager.Instance.PresentEvidenceToChallenge(evidenceData);
                return;
            }

            // If dialogue is open but not in challenge mode, do not inspect items
            if (DialogueUI.IsDialogueOpen) return;

            // 4. Otherwise (exploration mode / dialogue closed), single-click opens close-up inspect modal
            Debug.Log($"[Gameplay:TableEvidence] Opening inspect modal for '{evidenceData.evidenceName}'");
            string nodeToTrigger = evidenceData != null && !string.IsNullOrEmpty(evidenceData.dialogueNodeToTriggerOnInspect)
                ? evidenceData.dialogueNodeToTriggerOnInspect
                : dialogueNodeToTriggerOnInspect;
            EvidenceManager.Instance?.OpenInspectModal(evidenceData, nodeToTrigger);
        }
    }
}
