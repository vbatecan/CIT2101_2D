using UnityEngine;

namespace CaseClosed.Data
{
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

        public bool Matches(string clue1, string clue2)
        {
            return (clueA_Id == clue1 && clueB_Id == clue2) ||
                   (clueA_Id == clue2 && clueB_Id == clue1);
        }
    }
}
