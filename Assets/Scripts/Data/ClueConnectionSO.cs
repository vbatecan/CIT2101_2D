using UnityEngine;

namespace CaseClosed.Data
{
    /// <summary>
    /// ScriptableObject defining a synthesis deduction rule that combines two clues to produce a new deduction.
    /// </summary>
    [CreateAssetMenu(fileName = "NewClueConnection", menuName = "Case Closed/Clue Connection Rule")]
    public class ClueConnectionSO : ScriptableObject
    {
        public string connectionId;
        public string connectionTitle;

        [Header("Inputs")]
        public string clueA_Id;
        public string clueB_Id;

        [Header("Synthesized Output Clue")]
        public string resultClueId;
        public string resultClueTitle;
        [TextArea(3, 5)]
        public string deductionText;

        /// <summary>
        /// Checks whether a given pair of clue IDs matches this connection rule in either order.
        /// </summary>
        /// <param name="clue1">The first clue identifier.</param>
        /// <param name="clue2">The second clue identifier.</param>
        /// <returns>True if the clue pair matches this rule; otherwise, false.</returns>
        public bool Matches(string clue1, string clue2)
        {
            return (clueA_Id == clue1 && clueB_Id == clue2) ||
                   (clueA_Id == clue2 && clueB_Id == clue1);
        }
    }
}
