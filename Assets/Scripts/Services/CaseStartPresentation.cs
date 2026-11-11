namespace CaseClosed.Services
{
    /// <summary>Transient launch gate; retries and direct scene entry leave this unset.</summary>
    public static class CaseStartPresentation
    {
        public static bool IsActive { get; internal set; }
    }
}
