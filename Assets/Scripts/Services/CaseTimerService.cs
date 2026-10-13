using UnityEngine;
using CaseClosed.Enums;

namespace CaseClosed.Services
{
    public class CaseTimerService
    {
        public const float DefaultWarningThresholdSeconds = 120f;
        public const float DefaultUrgentThresholdSeconds = 60f;

        public float CalculateRemainingTime(float timeLimitSeconds, float elapsedTimeSeconds)
        {
            if (timeLimitSeconds <= 0f) return 0f;
            return Mathf.Max(0f, timeLimitSeconds - Mathf.Max(0f, elapsedTimeSeconds));
        }

        public bool IsTimeExpired(float timeLimitSeconds, float elapsedTimeSeconds)
        {
            if (timeLimitSeconds <= 0f) return false;
            return elapsedTimeSeconds >= timeLimitSeconds;
        }

        public TimerUrgencyState GetUrgencyState(
            float remainingSeconds,
            float warningThreshold = DefaultWarningThresholdSeconds,
            float urgentThreshold = DefaultUrgentThresholdSeconds)
        {
            if (remainingSeconds <= urgentThreshold)
            {
                return TimerUrgencyState.Urgent;
            }
            if (remainingSeconds <= warningThreshold)
            {
                return TimerUrgencyState.Warning;
            }
            return TimerUrgencyState.Normal;
        }

        public string FormatTimeMinutesSeconds(float totalSeconds)
        {
            int totalIntSeconds = Mathf.Max(0, Mathf.FloorToInt(totalSeconds));
            int minutes = totalIntSeconds / 60;
            int seconds = totalIntSeconds % 60;
            return $"{minutes:00}:{seconds:00}";
        }

        public string FormatTimeVerbose(float totalSeconds)
        {
            int totalIntSeconds = Mathf.Max(0, Mathf.FloorToInt(totalSeconds));
            int minutes = totalIntSeconds / 60;
            int seconds = totalIntSeconds % 60;
            return $"{minutes}m {seconds:00}s";
        }
    }
}
