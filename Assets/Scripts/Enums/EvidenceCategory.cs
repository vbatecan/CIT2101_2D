namespace CaseClosed.Enums
{
    /// <summary>
    /// Categorizes evidence items discovered during the investigation to aid in classification and filtering.
    /// </summary>
    public enum EvidenceCategory
    {
        /// <summary>Visual captures, crime scene photos, or portraits.</summary>
        Photograph,

        /// <summary>Written logs, letters, contracts, or receipts.</summary>
        Document,

        /// <summary>Personal items owned by suspects or victims.</summary>
        PersonalBelonging,

        /// <summary>Tangible crime scene objects like broken glass or cups.</summary>
        PhysicalClue,

        /// <summary>Official analytical findings or medical autopsy records.</summary>
        ForensicReport,

        /// <summary>Electronic records, phone call logs, or camera feeds.</summary>
        DigitalRecord
    }
}
