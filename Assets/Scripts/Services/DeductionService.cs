using CaseClosed.Data;

namespace CaseClosed.Services
{
    /// <summary>
    /// Service responsible for validating clue connections and evaluating deductions on the deduction board.
    /// </summary>
    public class DeductionService
    {
        /// <summary>
        /// Searches the active case for a clue connection rule matching two given clue IDs.
        /// </summary>
        /// <param name="activeCase">The active case containing clue connection rules.</param>
        /// <param name="clueA">The first selected clue identifier.</param>
        /// <param name="clueB">The second selected clue identifier.</param>
        /// <returns>The matching <see cref="ClueConnectionSO"/> rule if valid; otherwise, null.</returns>
        public ClueConnectionSO FindMatchingConnection(CaseSO activeCase, string clueA, string clueB)
        {
            if (activeCase == null || activeCase.clueConnections == null) return null;
            if (string.IsNullOrEmpty(clueA) || string.IsNullOrEmpty(clueB)) return null;

            foreach (var rule in activeCase.clueConnections)
            {
                if (rule != null && rule.Matches(clueA, clueB))
                {
                    return rule;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks whether two clue IDs form a valid connection in the current case.
        /// </summary>
        /// <param name="activeCase">The active case containing clue connection rules.</param>
        /// <param name="clueA">The first clue identifier.</param>
        /// <param name="clueB">The second clue identifier.</param>
        /// <returns>True if a valid deduction rule exists for the pair; otherwise, false.</returns>
        public bool CanConnect(CaseSO activeCase, string clueA, string clueB)
        {
            return FindMatchingConnection(activeCase, clueA, clueB) != null;
        }
    }
}
