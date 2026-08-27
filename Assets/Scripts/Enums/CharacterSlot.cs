namespace CaseClosed.Enums
{
    /// <summary>
    /// Designates the suspect or witness slot for character display GameObjects in the scene.
    /// </summary>
    public enum CharacterSlot
    {
        /// <summary>Automatically updates based on the currently interrogated suspect.</summary>
        AutoDetect,

        /// <summary>Dedicated slot for the primary suspect (e.g. left character sitting across table).</summary>
        PrimarySuspect,

        /// <summary>Dedicated slot for a secondary suspect, witness, or accomplice (e.g. right character).</summary>
        SecondarySuspect
    }
}
