namespace CaseClosed.Enums
{
    /// <summary>
    /// Represents the full-screen panels and modals managed by the primary UI coordinator.
    /// </summary>
    public enum UIPanelType
    {
        /// <summary>Main investigation desk with suspect sitting across table and inspectable items.</summary>
        InvestigationTable,

        /// <summary>Detailed zoomed modal for rotating and inspecting individual evidence items.</summary>
        InspectModal,

        /// <summary>Detective's notebook containing case dossier, suspect profiles, and clue logs.</summary>
        CaseFileNotebook,

        /// <summary>Interactive board for connecting pairs of clues to synthesize deductions.</summary>
        DeductionBoard,

        /// <summary>Multiple-choice interrogation conclusion quiz.</summary>
        ConclusionQuiz,

        /// <summary>Final evaluation summary card showing score, letter rank, and star rating.</summary>
        ResultsScreen
    }
}
