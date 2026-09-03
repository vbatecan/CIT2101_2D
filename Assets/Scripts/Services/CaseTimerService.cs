using UnityEngine;
using CaseClosed.Enums;

namespace CaseClosed.Services
{
    /// <summary>
    /// Pure C# domain service responsible for calculating countdown time, formatting timer displays,
    /// and evaluating urgency thresholds for case investigations.
    /// Fully unit-testable with zero MonoBehaviour dependencies.
    /// </summary>
    public class CaseTimerService
    {
        public const float DefaultWarningThresholdSeconds = 120f; // 2 minutes
        public const float DefaultUrgentThresholdSeconds = 30f;   // 30 seconds

        /// <summary>
        /// Calculates remaining time in seconds given a case time limit and elapsed investigation time.
        /// Clamps output between 0 and timeLimitSeconds.
        /// </summary>
        /// <param name="timeLimitSeconds">Total configured time limit for the case in seconds.</param>
        /// <param name="elapsedTimeSeconds">Elapsed investigation time in seconds.</param>
        /// <returns>Remaining time in seconds.</returns>
        public float CalculateRemainingTime(float timeLimitSeconds, float elapsedTimeSeconds)
        {
            if (timeLimitSeconds <= 0f) return 0f;
            return Mathf.Max(0f, timeLimitSeconds - Mathf.Max(0f, elapsedTimeSeconds));
        }

        /// <summary>
        /// Checks whether the time limit has expired.
        /// </summary>
        /// <param name="timeLimitSeconds">Total configured time limit for the case in seconds.</param>
        /// <param name="elapsedTimeSeconds">Elapsed investigation time in seconds.</param>
        /// <returns>True if time limit is positive and elapsed time meets or exceeds it.</returns>
        public bool IsTimeExpired(float timeLimitSeconds, float elapsedTimeSeconds)
        {
            if (timeLimitSeconds <= 0f) return false;
            return elapsedTimeSeconds >= timeLimitSeconds;
        }

        /// <summary>
        /// Evaluates current urgency level based on remaining seconds.
        /// </summary>
        /// <param name="remainingSeconds">Seconds remaining on the case countdown.</param>
        /// <param name="warningThreshold">Threshold seconds under which status is Warning (default 120s).</param>
        /// <param name="urgentThreshold">Threshold seconds under which status is Urgent (default 30s).</param>
        /// <returns>The corresponding <see cref="TimerUrgencyState"/>.</returns>
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

        /// <summary>
        /// Formats seconds into standard digital MM:SS clock format (e.g. 04:59).
        /// </summary>
        /// <param name="totalSeconds">Total time in seconds.</param>
        /// <returns>Formatted "MM:SS" string.</returns>
        public string FormatTimeMinutesSeconds(float totalSeconds)
        {
            int totalIntSeconds = Mathf.Max(0, Mathf.FloorToInt(totalSeconds));
            int minutes = totalIntSeconds / 60;
            int seconds = totalIntSeconds % 60;
            return $"{minutes:00}:{seconds:00}";
        }

        /// <summary>
        /// Formats seconds into descriptive verbose text (e.g. "5m 23s").
        /// </summary>
        /// <param name="totalSeconds">Total time in seconds.</param>
        /// <returns>Formatted verbose time string.</returns>
        public string FormatTimeVerbose(float totalSeconds)
        {
            int totalIntSeconds = Mathf.Max(0, Mathf.FloorToInt(totalSeconds));
            int minutes = totalIntSeconds / 60;
            int seconds = totalIntSeconds % 60;
            return $"{minutes}m {seconds:00}s";
        }
    }
}
