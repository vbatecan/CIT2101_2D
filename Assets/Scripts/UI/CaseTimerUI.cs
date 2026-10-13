using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    public class CaseTimerUI : MonoBehaviour
    {
        public static readonly Color ThemeParchmentNumeral = new Color(0.96f, 0.94f, 0.88f, 1f);
        public static readonly Color ThemeBrassGold = new Color(0.85f, 0.73f, 0.45f, 1f);
        public static readonly Color ThemeWarningAmber = new Color(0.98f, 0.68f, 0.22f, 1f);
        public static readonly Color ThemeUrgentCrimson = new Color(0.85f, 0.22f, 0.18f, 1f);
        public static readonly Color ThemeWoodDark = new Color(0.36f, 0.22f, 0.13f, 1f);
        public static readonly Color ThemeBrassBorder = new Color(0.68f, 0.52f, 0.28f, 0.85f);

        [Header("UI Text & Visuals")]
        [Tooltip("The TextMesh or UI Text component displaying the MM:SS timer.")]
        [SerializeField] private TMP_Text timerText;

        [Tooltip("Optional icon displaying a clock/stopwatch next to the timer text.")]
        [SerializeField] private Image timerIcon;

        [Tooltip("Optional background badge/pill behind the timer.")]
        [SerializeField] private Image backgroundBadge;

        [SerializeField, Tooltip("Keep the scene-authored panel, font, spacing, and text effects.")]
        private bool useScenePresentation;

        [Header("Detective Theme & Palette")]
        [Tooltip("When enabled, automatically aligns colors with the detective button and settings theme palette.")]
        [SerializeField] private bool matchThemePalette = true;

        [Tooltip("Fallback normal color if theme palette matching is disabled.")]
        [SerializeField] private Color normalColor = new Color(0.96f, 0.94f, 0.88f, 1f);

        [Tooltip("Fallback warning color if theme palette matching is disabled.")]
        [SerializeField] private Color warningColor = new Color(0.98f, 0.68f, 0.22f, 1f);

        [Tooltip("Fallback urgent color if theme palette matching is disabled.")]
        [SerializeField] private Color urgentColor = new Color(0.85f, 0.22f, 0.18f, 1f);

        [Header("Typography & Styling")]
        [Tooltip("When enabled, styles the timer font with the bold uppercase letter-spaced detective typeface matching buttons and settings.")]
        [SerializeField] private bool enforceDetectiveTypography = true;

        [Tooltip("Prefix label displayed before the countdown numbers (e.g. 'TIME  '). Matches button title typography.")]
        [SerializeField] private string timePrefix = "TIME  ";

        [Tooltip("Display the time prefix label alongside countdown digits.")]
        [SerializeField] private bool showTimePrefix = true;

        [Header("Behavior & Options")]
        [Tooltip("Hide this GameObject if the active case has no time limit.")]
        [SerializeField] private bool hideWhenUntimed = false;

        [Tooltip("Enable urgent ticking SFX in the final 30 seconds.")]
        [SerializeField] private bool enableTickSFX = true;

        [Tooltip("Scale multiplier applied during urgent pulse animation.")]
        [SerializeField] private float pulseScaleMagnitude = 0.08f;

        [Tooltip("Pulse oscillation frequency during urgent state.")]
        [SerializeField] private float pulseSpeed = 5f;

        public Color EffectiveNormalColor => matchThemePalette ? ThemeParchmentNumeral : normalColor;
        public Color EffectiveWarningColor => matchThemePalette ? ThemeWarningAmber : warningColor;
        public Color EffectiveUrgentColor => matchThemePalette ? ThemeUrgentCrimson : urgentColor;
        public Color EffectiveBrassColor => matchThemePalette ? ThemeBrassGold : normalColor;

        private int _lastDisplayedSecond = -1;
        private TimerUrgencyState _currentUrgency = TimerUrgencyState.Normal;
        private Vector3 _originalScale = Vector3.one;
        private bool _isSubscribed;
        private SpriteRenderer _parentSpriteRenderer;
        private Color _originalSpriteColor = Color.white;

        private void Awake()
        {
            if (timerText == null)
            {
                timerText = GetComponentInChildren<TMP_Text>();
            }
            _originalScale = transform.localScale;

            ResolveContainerGraphics();
            ApplyTypographyStyling();
        }

        private void Start()
        {
            TrySubscribe();
            SyncImmediate();
        }

        private void OnEnable()
        {
            TrySubscribe();
            ApplyTypographyStyling();
            SyncImmediate();
        }

        private void OnDisable()
        {
            Unsubscribe();
            ResetVisualState();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (timerText == null)
            {
                timerText = GetComponentInChildren<TMP_Text>();
            }
            ApplyTypographyStyling();
        }
#endif

        private void ResolveContainerGraphics()
        {
            if (backgroundBadge == null)
            {
                backgroundBadge = GetComponent<Image>() ?? GetComponentInChildren<Image>();
            }

            if (_parentSpriteRenderer == null)
            {
                _parentSpriteRenderer = GetComponentInParent<SpriteRenderer>();
                if (_parentSpriteRenderer != null)
                {
                    _originalSpriteColor = _parentSpriteRenderer.color;
                }
            }
        }

        private void ApplyTypographyStyling()
        {
            if (useScenePresentation) return;
            if (timerText == null) return;

            if (enforceDetectiveTypography)
            {
                TMP_FontAsset themeFont = DetectiveUITheme.GetStandardTmpFont();
                if (themeFont != null && (timerText.font == null || timerText.font == TMP_Settings.defaultFontAsset))
                {
                    timerText.font = themeFont;
                }

                timerText.fontStyle &= ~FontStyles.LowerCase;
                timerText.fontStyle |= FontStyles.Bold | FontStyles.UpperCase;
                timerText.fontWeight = FontWeight.Bold;

                timerText.characterSpacing = 3.5f;
                timerText.alignment = TextAlignmentOptions.Center;

                timerText.enableAutoSizing = true;
                timerText.fontSizeMin = 20f;
                timerText.fontSizeMax = 36f;
                timerText.fontSize = 32f;

                EnsureDepthEffects();
            }

            if (backgroundBadge != null)
            {
                DetectiveUITheme.Surface(backgroundBadge, DetectiveUITheme.WoodDark, true);
            }
        }

        private void EnsureDepthEffects()
        {
            if (timerText == null) return;

            Shadow shadow = timerText.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = timerText.gameObject.AddComponent<Shadow>();
            }
            if (shadow != null)
            {
                shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
                shadow.effectDistance = new Vector2(0f, -2f);
                shadow.useGraphicAlpha = true;
            }

            Outline outline = timerText.GetComponent<Outline>();
            if (outline == null)
            {
                outline = timerText.gameObject.AddComponent<Outline>();
            }
            if (outline != null)
            {
                outline.effectColor = new Color(0.08f, 0.05f, 0.03f, 0.85f);
                outline.effectDistance = new Vector2(1f, -1f);
                outline.useGraphicAlpha = false;
            }
        }

        private void ResetVisualState()
        {
            transform.localScale = _originalScale;
            if (_parentSpriteRenderer != null)
            {
                _parentSpriteRenderer.color = _originalSpriteColor;
            }
        }

        private void TrySubscribe()
        {
            if (!_isSubscribed && CaseManager.Instance != null)
            {
                CaseManager.Instance.OnTimerTick += HandleTimerTick;
                CaseManager.Instance.OnTimeExpired += HandleTimeExpired;
                CaseManager.Instance.OnCaseLoaded += HandleCaseLoaded;
                _isSubscribed = true;
            }
        }

        private void Unsubscribe()
        {
            if (_isSubscribed && CaseManager.Instance != null)
            {
                CaseManager.Instance.OnTimerTick -= HandleTimerTick;
                CaseManager.Instance.OnTimeExpired -= HandleTimeExpired;
                CaseManager.Instance.OnCaseLoaded -= HandleCaseLoaded;
                _isSubscribed = false;
            }
        }

        private void SyncImmediate()
        {
            if (CaseManager.Instance != null)
            {
                SyncTimerState(CaseManager.Instance.RemainingTime, CaseManager.Instance.ElapsedTime);
            }
        }

        private void Update()
        {
            if (CaseManager.Instance != null && !_isSubscribed)
            {
                TrySubscribe();
            }

            if (_currentUrgency == TimerUrgencyState.Urgent && CaseManager.Instance != null && CaseManager.Instance.IsTimerRunning)
            {
                float sine = Mathf.Sin(Time.time * pulseSpeed);
                float normalizedSine = Mathf.Max(0f, sine);
                float currentScale = 1f + (normalizedSine * pulseScaleMagnitude);
                transform.localScale = _originalScale * currentScale;

                if (_parentSpriteRenderer != null)
                {
                    _parentSpriteRenderer.color = Color.Lerp(_originalSpriteColor, new Color(1f, 0.72f, 0.72f, 1f), normalizedSine * 0.6f);
                }
            }
            else
            {
                if (transform.localScale != _originalScale)
                {
                    transform.localScale = _originalScale;
                }
                if (_parentSpriteRenderer != null && _parentSpriteRenderer.color != _originalSpriteColor)
                {
                    _parentSpriteRenderer.color = _originalSpriteColor;
                }
            }
        }

        private void HandleCaseLoaded(CaseSO caseData)
        {
            _lastDisplayedSecond = -1;
            ResetVisualState();

            if (CaseManager.Instance != null)
            {
                SyncTimerState(CaseManager.Instance.RemainingTime, 0f);
            }
        }

        private void HandleTimerTick(float remainingSeconds, float elapsedSeconds)
        {
            SyncTimerState(remainingSeconds, elapsedSeconds);
        }

        private void HandleTimeExpired()
        {
            _currentUrgency = TimerUrgencyState.Urgent;
            if (timerText != null)
            {
                timerText.text = FormatDisplayTime("00:00", TimerUrgencyState.Urgent);
                timerText.color = EffectiveUrgentColor;
                timerText.ForceMeshUpdate();
            }
            if (timerIcon != null)
            {
                timerIcon.color = EffectiveUrgentColor;
            }
            ResetVisualState();
        }

        public void SyncTimerState(float remainingSeconds, float elapsedSeconds)
        {
            if (CaseManager.Instance == null) return;

            if (!CaseManager.Instance.HasActiveTimeLimit)
            {
                if (hideWhenUntimed)
                {
                    gameObject.SetActive(false);
                }
                else if (timerText != null)
                {
                    timerText.text = FormatDisplayTime("--:--", TimerUrgencyState.Normal);
                    timerText.color = EffectiveNormalColor;
                    timerText.ForceMeshUpdate();
                }
                return;
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            int currentIntSecond = Mathf.Max(0, Mathf.CeilToInt(remainingSeconds));

            if (currentIntSecond != _lastDisplayedSecond)
            {
                _lastDisplayedSecond = currentIntSecond;

                TimerUrgencyState newUrgency = CaseManager.Instance.TimerService.GetUrgencyState(remainingSeconds);
                string formattedDigits = CaseManager.Instance.TimerService.FormatTimeMinutesSeconds(currentIntSecond);

                if (timerText != null)
                {
                    timerText.text = FormatDisplayTime(formattedDigits, newUrgency);
                    timerText.ForceMeshUpdate();
                }

                ApplyUrgencyVisuals(newUrgency);

                if (newUrgency == TimerUrgencyState.Urgent && enableTickSFX && currentIntSecond > 0 && CaseManager.Instance.IsTimerRunning)
                {
                    AudioManager.Instance?.PlayClockTick();
                }
            }
        }

        private void ApplyUrgencyVisuals(TimerUrgencyState urgency)
        {
            _currentUrgency = urgency;

            Color targetColor = EffectiveNormalColor;
            switch (urgency)
            {
                case TimerUrgencyState.Warning:
                    targetColor = EffectiveWarningColor;
                    break;
                case TimerUrgencyState.Urgent:
                    targetColor = EffectiveUrgentColor;
                    break;
                default:
                    targetColor = EffectiveNormalColor;
                    break;
            }

            if (timerText != null)
            {
                timerText.color = targetColor;
                timerText.ForceMeshUpdate();
            }

            if (timerIcon != null)
            {
                timerIcon.color = targetColor;
            }
        }

        private string FormatDisplayTime(string timeDigits, TimerUrgencyState urgency)
        {
            if (!showTimePrefix || string.IsNullOrEmpty(timePrefix))
            {
                return $"<b>{timeDigits}</b>";
            }

            string prefixClean = timePrefix.TrimEnd();
            string prefixColorHex;
            string numeralColorHex;

            switch (urgency)
            {
                case TimerUrgencyState.Warning:
                    prefixColorHex = ColorUtility.ToHtmlStringRGB(EffectiveWarningColor);
                    numeralColorHex = prefixColorHex;
                    break;
                case TimerUrgencyState.Urgent:
                    prefixColorHex = ColorUtility.ToHtmlStringRGB(EffectiveUrgentColor);
                    numeralColorHex = prefixColorHex;
                    break;
                default:
                    prefixColorHex = ColorUtility.ToHtmlStringRGB(EffectiveBrassColor);
                    numeralColorHex = ColorUtility.ToHtmlStringRGB(EffectiveNormalColor);
                    break;
            }

            return $"<color=#{prefixColorHex}><size=80%>{prefixClean}</size></color>  <color=#{numeralColorHex}><b>{timeDigits}</b></color>";
        }
    }
}
