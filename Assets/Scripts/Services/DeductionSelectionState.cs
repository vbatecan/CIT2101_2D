namespace CaseClosed.Services
{
    /// <summary>
    /// Pure runtime state for the two-step clue selection interaction on the deduction board.
    /// It owns no Unity objects and intentionally does not decide whether a pair is valid.
    /// </summary>
    public sealed class DeductionSelectionState
    {
        /// <summary>Describes the state transition caused by a clue-selection request.</summary>
        public enum Transition
        {
            Ignored,
            FirstClueSelected,
            FirstClueDeselected,
            PairReady
        }

        /// <summary>The first clue selected for a connection attempt.</summary>
        public string FirstSelectedClueId { get; private set; }

        /// <summary>The second clue selected for a connection attempt.</summary>
        public string SecondSelectedClueId { get; private set; }

        /// <summary>
        /// Restores legacy inspector defaults before the session begins. Subsequent mutations stay in this state object.
        /// </summary>
        public void Restore(string firstClueId, string secondClueId)
        {
            FirstSelectedClueId = firstClueId;
            SecondSelectedClueId = secondClueId;
        }

        /// <summary>
        /// Selects a clue, deselects the current first clue, or makes a pair ready for validation.
        /// </summary>
        public Transition Select(string clueId)
        {
            if (string.IsNullOrEmpty(clueId))
            {
                return Transition.Ignored;
            }

            if (string.IsNullOrEmpty(FirstSelectedClueId))
            {
                FirstSelectedClueId = clueId;
                return Transition.FirstClueSelected;
            }

            if (FirstSelectedClueId == clueId)
            {
                // Preserve the legacy controller behavior: a pending second clue is only cleared by Clear().
                FirstSelectedClueId = null;
                return Transition.FirstClueDeselected;
            }

            SecondSelectedClueId = clueId;
            return Transition.PairReady;
        }

        /// <summary>Clears both selected clues after a completed or cancelled connection attempt.</summary>
        public void Clear()
        {
            FirstSelectedClueId = null;
            SecondSelectedClueId = null;
        }
    }
}
