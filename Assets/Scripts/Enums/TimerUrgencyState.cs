namespace CaseClosed.Enums
{
    /// <summary>
    /// Urgency levels for investigation case timer thresholds.
    /// </summary>
    public enum TimerUrgencyState
    {
        /// <summary>Standard investigation time remaining (e.g. > 120 seconds).</summary>
        Normal,

        /// <summary>Warning state when investigation time is running low (e.g. &lt;= 120 seconds).</summary>
        Warning,

        /// <summary>Critical urgent state requiring immediate attention (e.g. &lt;= 30 seconds).</summary>
        Urgent
    }
}
