using System;
using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    /// <summary>
    /// UI View MonoBehaviour managing the countdown timer display in Panel_HeaderNav.
    /// Updates digital clock display, shifts color across urgency thresholds (Normal -> Amber -> Red),
    /// pulses visually during critical countdown (&lt;= 30s), and triggers audio ticks.
    /// Can be dragged directly onto the Timer GameObject in Panel_HeaderNav in the Unity Inspector.
    /// </summary>
    public class CaseTimerUI : MonoBehaviour
    {
        [Header("UI Text & Visuals")]
        [Tooltip("The TextMesh or UI Text component displaying the MM:SS timer.")]
        [SerializeField] private Text timerText;

        [Tooltip("Optional icon displaying a clock/stopwatch next to the timer text.")]
        [SerializeField] private Image timerIcon;

        [Tooltip("Optional background badge/pill behind the timer.")]
        [SerializeField] private Image backgroundBadge;

        [Header("Urgency Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color warningColor = new Color(1f, 0.78f, 0.15f, 1f); // Amber
        [SerializeField] private Color urgentColor = new Color(1f, 0.25f, 0.25f, 1f);  // Red

        [Header("Behavior & Options")]
        [Tooltip("Hide this GameObject if the active case has no time limit.")]
        [SerializeField] private bool hideWhenUntimed = false;

        [Tooltip("Enable urgent ticking SFX in the final 30 seconds.")]
        [SerializeField] private bool enableTickSFX = true;

        [Tooltip("Scale multiplier applied during urgent pulse animation.")]
        [SerializeField] private float pulseScaleMagnitude = 0.12f;

        [Tooltip("Pulse oscillation frequency in Hz during urgent state.")]
        [SerializeField] private float pulseSpeed = 4f;

        private int _lastDisplayedSecond = -1;
        private TimerUrgencyState _currentUrgency = TimerUrgencyState.Normal;
        private Vector3 _originalScale = Vector3.one;

        private void Awake()
        {
            if (timerText == null)
            {
                timerText = GetComponentInChildren<Text>();
            }
            _originalScale = transform.localScale;
        }

        private void OnEnable()
        {
            if (CaseManager.Instance != null)
            {
                CaseManager.Instance.OnTimerTick += HandleTimerTick;
                CaseManager.Instance.OnTimeExpired += HandleTimeExpired;
                CaseManager.Instance.OnCaseLoaded += HandleCaseLoaded;

                // Sync immediate state
                SyncTimerState(CaseManager.Instance.RemainingTime, CaseManager.Instance.ElapsedTime);
            }
        }

        private void OnDisable()
        {
            if (CaseManager.Instance != null)
            {
                CaseManager.Instance.OnTimerTick -= HandleTimerTick;
                CaseManager.Instance.OnTimeExpired -= HandleTimeExpired;
                CaseManager.Instance.OnCaseLoaded -= HandleCaseLoaded;
            }

            transform.localScale = _originalScale;
        }

        private void Update()
        {
            // Zero-allocation visual pulse animation only active during urgent state
            if (_currentUrgency == TimerUrgencyState.Urgent && CaseManager.Instance != null && CaseManager.Instance.IsTimerRunning)
            {
                float sine = Mathf.Sin(Time.time * pulseSpeed * Mathf.PI);
                float currentScale = 1f + (Mathf.Max(0f, sine) * pulseScaleMagnitude);
                transform.localScale = _originalScale * currentScale;
            }
            else if (transform.localScale != _originalScale)
            {
                transform.localScale = _originalScale;
            }
        }

        private void HandleCaseLoaded(CaseSO caseData)
        {
            _lastDisplayedSecond = -1;
            transform.localScale = _originalScale;

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
                timerText.text = "00:00";
                timerText.color = urgentColor;
            }
            transform.localScale = _originalScale;
        }

        /// <summary>
        /// Synchronizes the UI elements with current remaining seconds, throttling string allocations to 1/sec.
        /// </summary>
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
                    timerText.text = "UNTIMED";
                    timerText.color = normalColor;
                }
                return;
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            int currentIntSecond = Mathf.Max(0, Mathf.CeilToInt(remainingSeconds));

            // Only format string and evaluate urgency when integer seconds roll over (GC-friendly)
            if (currentIntSecond != _lastDisplayedSecond)
            {
                _lastDisplayedSecond = currentIntSecond;

                if (timerText != null)
                {
                    timerText.text = CaseManager.Instance.TimerService.FormatTimeMinutesSeconds(currentIntSecond);
                }

                TimerUrgencyState newUrgency = CaseManager.Instance.TimerService.GetUrgencyState(remainingSeconds);
                ApplyUrgencyVisuals(newUrgency);

                // Urgent ticking audio feedback every second when <= 30s
                if (newUrgency == TimerUrgencyState.Urgent && enableTickSFX && currentIntSecond > 0 && CaseManager.Instance.IsTimerRunning)
                {
                    AudioManager.Instance?.PlayClockTick();
                }
            }
        }

        private void ApplyUrgencyVisuals(TimerUrgencyState urgency)
        {
            _currentUrgency = urgency;

            Color targetColor = normalColor;
            switch (urgency)
            {
                case TimerUrgencyState.Warning:
                    targetColor = warningColor;
                    break;
                case TimerUrgencyState.Urgent:
                    targetColor = urgentColor;
                    break;
                default:
                    targetColor = normalColor;
                    break;
            }

            if (timerText != null)
            {
                timerText.color = targetColor;
            }

            if (timerIcon != null)
            {
                timerIcon.color = targetColor;
            }
        }
    }
}
