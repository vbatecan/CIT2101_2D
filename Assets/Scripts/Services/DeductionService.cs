using CaseClosed.Data;

namespace CaseClosed.Services
{
    public class DeductionService
    {
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

        public bool CanConnect(CaseSO activeCase, string clueA, string clueB)
        {
            return FindMatchingConnection(activeCase, clueA, clueB) != null;
        }
    }
}
