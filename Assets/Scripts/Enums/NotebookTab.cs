namespace CaseClosed.Enums
{
    /// <summary>
    /// Represents the navigational tabs available within the detective's case file notebook UI.
    /// </summary>
    public enum NotebookTab
    {
        /// <summary>Displays general case synopsis, location, victim info, and objectives.</summary>
        CaseSummary,

        /// <summary>Displays profiles, alibis, and motives of all persons of interest.</summary>
        Suspects,

        /// <summary>Displays all discovered evidence items and inspection observations.</summary>
        Evidence,

        /// <summary>Displays unlocked deductions and synthesized clues.</summary>
        Clues
    }
}
