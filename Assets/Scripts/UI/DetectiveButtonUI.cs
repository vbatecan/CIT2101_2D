using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    /// <summary>
    /// Button role variants for the detective/mystery UI theme.
    /// </summary>
    public enum DetectiveButtonVariant
    {
        Auto,
        Primary,     // Important actions: START, CONTINUE, CONCLUDE, CONFIRM
        Secondary,   // Navigation: CASE FILES, NOTEBOOK, SETTINGS, HOW TO PLAY
        Danger,      // Destructive actions: QUIT, EXIT, DELETE, LEAVE
        SmallIcon,   // Back, Close, Audio toggles, Reset, compact pills
        DossierCard  // Large dossier folders or cards preserving authored artwork
    }

    /// <summary>
    /// Reusable detective-themed button controller.
    /// Provides vintage mystery aesthetics, smooth scale & depth transitions,
    /// robust interaction states (Normal, Hover, Pressed, Selected, Disabled),
    /// and audio integration across all game buttons while preserving all original
    /// OnClick listeners and UnityEvent callbacks.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public class DetectiveButtonUI : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler,
        ISelectHandler, IDeselectHandler
    {
        [Header("Button Configuration")]
        [SerializeField] private DetectiveButtonVariant variant = DetectiveButtonVariant.Auto;
        [SerializeField] private bool autoStyleOnStart = true;
        [SerializeField] private bool playAudioOnPress = true;
        [SerializeField] private bool enforceReadableTypography = true;

        [Header("Interaction Animation")]
        [Range(1.0f, 1.15f)]
        [SerializeField] private float hoverScale = 1.032f;
        [Range(0.85f, 1.0f)]
        [SerializeField] private float pressedScale = 0.965f;
        [SerializeField] private float transitionSpeed = 16f;

        private Button _button;
        private Image _targetImage;
        private Shadow _shadow;
        private Outline _outline;
        private RectTransform _rectTransform;
        private Vector3 _originalScale = Vector3.one;

        private Text[] _cachedLabels;
        private TMPro.TMP_Text[] _cachedTmpLabels;
        private string[] _lastProcessedTexts;
        private string[] _lastProcessedTmpTexts;

        private bool _isHovered = false;
        private bool _isPressed = false;
        private bool _isSelected = false;
        private bool _wasInteractable = true;

        // Visual Palette - Authentic vintage detective desk & dossier colors
        public static readonly Color PaperColor = new Color(0.95f, 0.91f, 0.82f, 1f);       // #F2E8D1 Parchment
        public static readonly Color PrimaryInkColor = new Color(0.12f, 0.16f, 0.17f, 1f);  // #1F292B Deep antique ink
        public static readonly Color PrimaryHoverColor = new Color(0.19f, 0.25f, 0.27f, 1f);// Richer ink highlight
        public static readonly Color PrimaryPressColor = new Color(0.08f, 0.11f, 0.12f, 1f);// Dark stamp impression

        public static readonly Color SecondarySlateColor = new Color(0.18f, 0.22f, 0.24f, 0.96f); // #2E383D Aged slate
        public static readonly Color SecondaryHoverColor = new Color(0.26f, 0.31f, 0.33f, 1f);     // Slate highlight
        public static readonly Color SecondaryPressColor = new Color(0.12f, 0.15f, 0.16f, 1f);

        public static readonly Color DangerRedColor = new Color(0.44f, 0.15f, 0.13f, 0.98f);  // #702621 Wax stamp red
        public static readonly Color DangerHoverColor = new Color(0.56f, 0.19f, 0.16f, 1f);
        public static readonly Color DangerPressColor = new Color(0.30f, 0.10f, 0.08f, 1f);

        public static readonly Color SmallIconColor = new Color(0.14f, 0.18f, 0.20f, 0.92f);
        public static readonly Color SmallIconHoverColor = new Color(0.22f, 0.27f, 0.30f, 1f);
        public static readonly Color SmallIconPressColor = new Color(0.09f, 0.11f, 0.12f, 1f);

        public static readonly Color BrassAccentColor = new Color(0.68f, 0.52f, 0.28f, 0.85f); // #AE8547 Antique brass
        public static readonly Color BrassHoverGlow = new Color(0.88f, 0.70f, 0.38f, 1f);

        public static readonly Color DisabledTintColor = new Color(0.55f, 0.55f, 0.55f, 0.45f);

        public DetectiveButtonVariant Variant => variant;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _rectTransform = GetComponent<RectTransform>();
            _originalScale = transform.localScale;

            ResolveTargetGraphic();
            EnsureDepthComponents();
            CacheTypographyComponents();
        }

        private void Start()
        {
            if (autoStyleOnStart)
            {
                ApplyStyling();
            }
        }

        private void OnEnable()
        {
            _isHovered = false;
            _isPressed = false;
            _isSelected = false;
            if (_rectTransform != null)
            {
                _rectTransform.localScale = _originalScale;
            }
            CacheTypographyComponents();
        }

        private void Update()
        {
            if (_button == null) return;

            bool isInteractable = _button.interactable;
            if (isInteractable != _wasInteractable)
            {
                _wasInteractable = isInteractable;
                UpdateInteractableState();
            }

            AnimateButtonState();
        }

        /// <summary>
        /// Explicitly sets the button variant and reapplies styling.
        /// </summary>
        public void SetVariant(DetectiveButtonVariant newVariant)
        {
            variant = newVariant;
            ApplyStyling();
        }

        /// <summary>
        /// Resolves the primary Image component to style and manipulate.
        /// </summary>
        private void ResolveTargetGraphic()
        {
            if (_targetImage == null)
            {
                _targetImage = GetComponent<Image>();
                if (_targetImage == null && _button != null && _button.targetGraphic is Image img)
                {
                    _targetImage = img;
                }
                if (_targetImage == null)
                {
                    _targetImage = GetComponentInChildren<Image>(true);
                }
            }

            if (_button != null && _button.targetGraphic == null && _targetImage != null)
            {
                _button.targetGraphic = _targetImage;
            }
        }

        /// <summary>
        /// Ensures subtle shadow and depth components exist on the button.
        /// </summary>
        private void EnsureDepthComponents()
        {
            if (_shadow == null)
            {
                _shadow = GetComponent<Shadow>();
                if (_shadow == null && _targetImage != null)
                {
                    _shadow = _targetImage.gameObject.AddComponent<Shadow>();
                    _shadow.effectColor = new Color(0f, 0f, 0f, 0.38f);
                    _shadow.effectDistance = new Vector2(0f, -3f);
                    _shadow.useGraphicAlpha = true;
                }
            }

            if (_outline == null)
            {
                _outline = GetComponent<Outline>();
                if (_outline == null && _targetImage != null)
                {
                    _outline = _targetImage.gameObject.AddComponent<Outline>();
                }
            }

            if (_outline != null)
            {
                _outline.effectColor = BrassAccentColor;
                _outline.effectDistance = new Vector2(1.5f, -1.5f);
                _outline.useGraphicAlpha = false;
            }
        }

        /// <summary>
        /// Determines the visual variant automatically if set to Auto.
        /// </summary>
        public DetectiveButtonVariant ResolveEffectiveVariant()
        {
            if (variant != DetectiveButtonVariant.Auto)
            {
                return variant;
            }

            string bName = gameObject.name.ToLowerInvariant();

            // Dossier Card / Folder check
            if (bName.Contains("folder") || bName.Contains("dossier") || bName.Contains("case0") || bName.Contains("card"))
            {
                return DetectiveButtonVariant.DossierCard;
            }

            // Text content check
            Text label = GetComponentInChildren<Text>(true);
            string textContent = (label != null && !string.IsNullOrEmpty(label.text)) ? label.text.ToLowerInvariant() : "";

            // Danger actions
            if (bName.Contains("quit") || bName.Contains("exit") || bName.Contains("delete") ||
                textContent.Contains("quit") || textContent.Contains("exit") || textContent.Contains("leave"))
            {
                return DetectiveButtonVariant.Danger;
            }

            // Primary actions
            if (bName.Contains("play") || bName.Contains("start") || bName.Contains("conclude") ||
                bName.Contains("confirm") || bName.Contains("solve") || bName.Contains("primary") ||
                textContent.Contains("start") || textContent.Contains("conclude") || textContent.Contains("confirm") ||
                textContent.Contains("continue") || textContent.Contains("next case") || textContent.Contains("reopen"))
            {
                return DetectiveButtonVariant.Primary;
            }

            // Small / Icon / Utility
            if (bName.Contains("back") || bName.Contains("close") || bName.Contains("return") ||
                bName.Contains("mute") || bName.Contains("reset") || bName.Contains("prev") ||
                bName.Contains("next") || bName.Contains("zoom") || bName.Contains("rotate") ||
                (_rectTransform != null && (_rectTransform.rect.width < 90f || _rectTransform.rect.height < 44f)))
            {
                return DetectiveButtonVariant.SmallIcon;
            }

            return DetectiveButtonVariant.Secondary;
        }

        /// <summary>
        /// Applies the complete detective styling palette, sprite surfaces, and typography.
        /// </summary>
        public void ApplyStyling()
        {
            ResolveTargetGraphic();
            EnsureDepthComponents();

            DetectiveButtonVariant effectiveVariant = ResolveEffectiveVariant();

            // Transition: Set button to ColorTint with standard colors so native selection behaves predictably
            if (_button != null)
            {
                _button.transition = Selectable.Transition.ColorTint;
                ColorBlock cb = _button.colors;
                cb.normalColor = Color.white;
                cb.highlightedColor = new Color(1.05f, 1.05f, 1.02f, 1f);
                cb.pressedColor = new Color(0.88f, 0.88f, 0.88f, 1f);
                cb.selectedColor = new Color(1.02f, 1.02f, 0.98f, 1f);
                cb.disabledColor = DisabledTintColor;
                cb.fadeDuration = 0.08f;
                _button.colors = cb;
            }

            // Style Target Image Surface
            if (_targetImage != null)
            {
                if (effectiveVariant != DetectiveButtonVariant.DossierCard)
                {
                    // Ensure the button uses the 9-sliced rounded sprite
                    _targetImage.sprite = DetectiveUITheme.GetRoundedSprite();
                    _targetImage.overrideSprite = null;
                    _targetImage.type = Image.Type.Sliced;
                    _targetImage.pixelsPerUnitMultiplier = 1.4f;
                    _targetImage.preserveAspect = false;
                    _targetImage.color = GetBaseColor(effectiveVariant);
                }
                else
                {
                    // Dossier folder card: preserve sprite, keep full color
                    _targetImage.color = Color.white;
                }
            }

            EnsureButtonLabel(effectiveVariant);

            // Style Typography
            if (enforceReadableTypography)
            {
                ApplyTypographyStyling(effectiveVariant);
            }
        }

        private Color GetBaseColor(DetectiveButtonVariant v)
        {
            switch (v)
            {
                case DetectiveButtonVariant.Primary: return PrimaryInkColor;
                case DetectiveButtonVariant.Secondary: return SecondarySlateColor;
                case DetectiveButtonVariant.Danger: return DangerRedColor;
                case DetectiveButtonVariant.SmallIcon: return SmallIconColor;
                default: return SecondarySlateColor;
            }
        }

        private Color GetHoverColor(DetectiveButtonVariant v)
        {
            switch (v)
            {
                case DetectiveButtonVariant.Primary: return PrimaryHoverColor;
                case DetectiveButtonVariant.Secondary: return SecondaryHoverColor;
                case DetectiveButtonVariant.Danger: return DangerHoverColor;
                case DetectiveButtonVariant.SmallIcon: return SmallIconHoverColor;
                default: return SecondaryHoverColor;
            }
        }

        private Color GetPressColor(DetectiveButtonVariant v)
        {
            switch (v)
            {
                case DetectiveButtonVariant.Primary: return PrimaryPressColor;
                case DetectiveButtonVariant.Secondary: return SecondaryPressColor;
                case DetectiveButtonVariant.Danger: return DangerPressColor;
                case DetectiveButtonVariant.SmallIcon: return SmallIconPressColor;
                default: return SecondaryPressColor;
            }
        }

        private void CacheTypographyComponents()
        {
            _cachedLabels = GetComponentsInChildren<Text>(true);
            _lastProcessedTexts = (_cachedLabels != null) ? new string[_cachedLabels.Length] : null;

            _cachedTmpLabels = GetComponentsInChildren<TMPro.TMP_Text>(true);
            _lastProcessedTmpTexts = (_cachedTmpLabels != null) ? new string[_cachedTmpLabels.Length] : null;
        }

        private void LateUpdate()
        {
            if (enforceReadableTypography)
            {
                EnforceAllCapsAndBold();
            }
        }

        private void EnforceAllCapsAndBold()
        {
            if (_cachedLabels == null || _cachedTmpLabels == null)
            {
                CacheTypographyComponents();
            }

            if (_cachedLabels != null)
            {
                for (int i = 0; i < _cachedLabels.Length; i++)
                {
                    Text t = _cachedLabels[i];
                    if (t == null) continue;

                    if (t.font == null)
                    {
                        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
                    }

                    if (t.fontStyle != FontStyle.Bold)
                    {
                        t.fontStyle = FontStyle.Bold;
                    }

                    string current = t.text;
                    if (!string.IsNullOrEmpty(current))
                    {
                        if (current != _lastProcessedTexts[i])
                        {
                            string upper = current.ToUpperInvariant();
                            if (current != upper)
                            {
                                t.text = upper;
                                _lastProcessedTexts[i] = upper;
                            }
                            else
                            {
                                _lastProcessedTexts[i] = current;
                            }
                        }
                    }
                }
            }

            if (_cachedTmpLabels != null)
            {
                for (int i = 0; i < _cachedTmpLabels.Length; i++)
                {
                    TMPro.TMP_Text tmp = _cachedTmpLabels[i];
                    if (tmp == null) continue;

                    if ((tmp.fontStyle & TMPro.FontStyles.Bold) == 0 || (tmp.fontStyle & TMPro.FontStyles.LowerCase) != 0 || (tmp.fontStyle & TMPro.FontStyles.UpperCase) == 0)
                    {
                        tmp.fontStyle &= ~TMPro.FontStyles.LowerCase;
                        tmp.fontStyle |= TMPro.FontStyles.Bold | TMPro.FontStyles.UpperCase;
                    }

                    string current = tmp.text;
                    if (!string.IsNullOrEmpty(current))
                    {
                        if (current != _lastProcessedTmpTexts[i])
                        {
                            string upper = current.ToUpperInvariant();
                            if (current != upper)
                            {
                                tmp.text = upper;
                                _lastProcessedTmpTexts[i] = upper;
                            }
                            else
                            {
                                _lastProcessedTmpTexts[i] = current;
                            }
                        }
                    }
                }
            }
        }

        private void ApplyTypographyStyling(DetectiveButtonVariant effectiveVariant)
        {
            CacheTypographyComponents();

            if (_cachedLabels != null && _cachedLabels.Length > 0)
            {
                for (int i = 0; i < _cachedLabels.Length; i++)
                {
                    Text t = _cachedLabels[i];
                    if (t == null) continue;

                    if (t.font == null)
                    {
                        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
                    }

                    t.fontStyle = FontStyle.Bold;
                    if (!string.IsNullOrEmpty(t.text))
                    {
                        string upper = t.text.ToUpperInvariant();
                        if (t.text != upper) t.text = upper;
                        _lastProcessedTexts[i] = upper;
                    }

                    // Dossier cards have specialized title and status badge layouts; do not alter their anchors
                    if (effectiveVariant == DetectiveButtonVariant.DossierCard)
                    {
                        t.resizeTextForBestFit = true;
                        t.resizeTextMinSize = 9;
                        t.resizeTextMaxSize = Mathf.Max(14, t.fontSize);
                        continue;
                    }

                    // Standard action buttons: ensure centered, bold, perfectly legible all-caps text
                    t.alignment = TextAnchor.MiddleCenter;
                    t.color = PaperColor;
                    t.resizeTextForBestFit = true;
                    t.resizeTextMinSize = 10;
                    t.resizeTextMaxSize = Mathf.Max(15, t.fontSize);

                    // Disable muddy default text shadows
                    foreach (Shadow sh in t.GetComponents<Shadow>())
                    {
                        sh.enabled = false;
                    }
                }
            }

            if (_cachedTmpLabels != null && _cachedTmpLabels.Length > 0)
            {
                for (int i = 0; i < _cachedTmpLabels.Length; i++)
                {
                    TMPro.TMP_Text tmp = _cachedTmpLabels[i];
                    if (tmp == null) continue;

                    tmp.fontStyle &= ~TMPro.FontStyles.LowerCase;
                    tmp.fontStyle |= TMPro.FontStyles.Bold | TMPro.FontStyles.UpperCase;
                    tmp.enableAutoSizing = true;
                    tmp.fontSizeMin = 9f;
                    tmp.fontSizeMax = Mathf.Max(14f, tmp.fontSize);

                    if (!string.IsNullOrEmpty(tmp.text))
                    {
                        string upper = tmp.text.ToUpperInvariant();
                        if (tmp.text != upper) tmp.text = upper;
                        _lastProcessedTmpTexts[i] = upper;
                    }

                    if (effectiveVariant != DetectiveButtonVariant.DossierCard)
                    {
                        tmp.alignment = TMPro.TextAlignmentOptions.Center;
                        tmp.color = PaperColor;
                    }
                }
            }
        }

        private void EnsureButtonLabel(DetectiveButtonVariant effectiveVariant)
        {
            if (effectiveVariant == DetectiveButtonVariant.DossierCard) return;

            Text existingText = GetComponentInChildren<Text>(true);
            TMPro.TMP_Text existingTmp = GetComponentInChildren<TMPro.TMP_Text>(true);

            if (existingText == null && existingTmp == null)
            {
                string caption = InferButtonCaption(gameObject.name);
                if (!string.IsNullOrEmpty(caption))
                {
                    GameObject labelObj = new GameObject("ActionLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                    labelObj.transform.SetParent(transform, false);
                    RectTransform rt = labelObj.GetComponent<RectTransform>();
                    DetectiveUITheme.Place(rt, Vector2.zero, Vector2.one);
                    rt.sizeDelta = new Vector2(-20f, -8f);

                    Text t = labelObj.GetComponent<Text>();
                    t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
                    t.text = caption.ToUpperInvariant();
                    t.fontStyle = FontStyle.Bold;
                    t.alignment = TextAnchor.MiddleCenter;
                    t.color = PaperColor;
                    t.resizeTextForBestFit = true;
                    t.resizeTextMinSize = 10;
                    t.resizeTextMaxSize = 18;
                    t.raycastTarget = false;

                    CacheTypographyComponents();
                }
            }
        }

        private static string InferButtonCaption(string objName)
        {
            if (string.IsNullOrEmpty(objName)) return "";
            string lower = objName.ToLowerInvariant();
            if (lower.Contains("mute")) return "MUTE";
            if (lower.Contains("reset")) return "RESET DEFAULTS";
            if (lower.Contains("quit") || lower.Contains("exit")) return "QUIT";
            if (lower.Contains("settings")) return "SETTINGS";
            if (lower.Contains("caseselect") || lower.Contains("cases_select") || lower.Contains("levelselect")) return "CASE FILES";
            if (lower.Contains("howtoplay") || lower.Contains("how_to_play") || lower.Contains("help")) return "HOW TO PLAY";
            if (lower.Contains("credit")) return "CREDITS";
            if (lower.Contains("play") || lower.Contains("start")) return "START";
            if (lower.Contains("resume")) return "RESUME";
            if (lower.Contains("mainmenu") || lower.Contains("main_menu")) return "MAIN MENU";
            if (lower.Contains("returntomenu") || lower.Contains("backtomenu")) return "BACK TO MENU";
            if (lower.Contains("back") || lower.Contains("return")) return "BACK";
            if (lower.Contains("retry") || lower.Contains("reopen")) return "RETRY";
            if (lower.Contains("nextlevel") || lower.Contains("nextgame")) return "NEXT CASE";
            if (lower.Contains("next")) return "NEXT";
            if (lower.Contains("continue")) return "CONTINUE";
            if (lower.Contains("conclude")) return "CONCLUDE CASE";
            if (lower.Contains("notebook")) return "NOTEBOOK";
            if (lower.Contains("suspect")) return "SUSPECTS";
            if (lower.Contains("confirm") || lower.Contains("yes")) return "CONFIRM";
            if (lower.Contains("cancel") || lower.Contains("no")) return "CANCEL";
            if (lower.Contains("challenge")) return "CHALLENGE";
            if (lower.Contains("close")) return "CLOSE";
            return objName.Replace("Button_", "").Replace("Button", "").Trim();
        }

        private void UpdateInteractableState()
        {
            if (_targetImage != null)
            {
                DetectiveButtonVariant v = ResolveEffectiveVariant();
                _targetImage.color = (_button != null && _button.interactable) ? GetBaseColor(v) : DisabledTintColor;
            }
        }

        /// <summary>
        /// Frame-independent animation lerp for scale, depth, and color.
        /// Uses unscaledDeltaTime to function seamlessly even when game is paused.
        /// </summary>
        private void AnimateButtonState()
        {
            if (_rectTransform == null) return;

            bool interactable = (_button != null && _button.interactable);

            // Compute target scale
            Vector3 targetScale = _originalScale;
            if (interactable)
            {
                if (_isPressed) targetScale = _originalScale * pressedScale;
                else if (_isHovered || _isSelected) targetScale = _originalScale * hoverScale;
            }

            float dt = Time.unscaledDeltaTime;
            _rectTransform.localScale = Vector3.Lerp(_rectTransform.localScale, targetScale, dt * transitionSpeed);

            // Animate Shadow depth distance
            if (_shadow != null)
            {
                Vector2 targetDist = new Vector2(0f, -3f);
                if (interactable)
                {
                    if (_isPressed) targetDist = new Vector2(0f, -1.2f);
                    else if (_isHovered) targetDist = new Vector2(0f, -5f);
                }
                _shadow.effectDistance = Vector2.Lerp(_shadow.effectDistance, targetDist, dt * transitionSpeed);
            }

            // Animate Outline border color with warm brass highlight
            if (_outline != null)
            {
                Color targetOutline = BrassAccentColor;
                if (!interactable)
                {
                    targetOutline = new Color(0.40f, 0.35f, 0.25f, 0.35f);
                }
                else if (_isPressed)
                {
                    targetOutline = new Color(0.48f, 0.36f, 0.18f, 1f);
                }
                else if (_isHovered || _isSelected)
                {
                    targetOutline = BrassHoverGlow;
                }

                _outline.effectColor = Color.Lerp(_outline.effectColor, targetOutline, dt * transitionSpeed);
            }

            // Animate Graphic Color (for non-dossier cards)
            DetectiveButtonVariant v = ResolveEffectiveVariant();
            if (v != DetectiveButtonVariant.DossierCard && _targetImage != null && interactable)
            {
                Color targetCol = GetBaseColor(v);
                if (_isPressed) targetCol = GetPressColor(v);
                else if (_isHovered || _isSelected) targetCol = GetHoverColor(v);

                _targetImage.color = Color.Lerp(_targetImage.color, targetCol, dt * transitionSpeed);
            }
        }

        // Pointer & Selection Events
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            _isHovered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            _isPressed = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            _isPressed = true;

            if (playAudioOnPress)
            {
                PlayClickAudio();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            _isSelected = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _isSelected = false;
        }

        private void PlayClickAudio()
        {
            if (AudioManager.Instance != null && AudioManager.Instance.buttonClickSFX != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }
    }
}
