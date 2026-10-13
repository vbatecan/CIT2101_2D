namespace CaseClosed.Services
{
    public sealed class DeductionSelectionState
    {
        public enum Transition
        {
            Ignored,
            FirstClueSelected,
            FirstClueDeselected,
            PairReady
        }

        public string FirstSelectedClueId { get; private set; }

        public string SecondSelectedClueId { get; private set; }

        public void Restore(string firstClueId, string secondClueId)
        {
            FirstSelectedClueId = firstClueId;
            SecondSelectedClueId = secondClueId;
        }

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
                FirstSelectedClueId = null;
                return Transition.FirstClueDeselected;
            }

            SecondSelectedClueId = clueId;
            return Transition.PairReady;
        }

        public void Clear()
        {
            FirstSelectedClueId = null;
            SecondSelectedClueId = null;
        }
    }
}
