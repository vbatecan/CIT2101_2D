using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaseClosed.Data
{
    public enum CaseStatus
    {
        Locked,
        Available,
        Completed
    }

    /// <summary>
    /// Single source of truth ScriptableObject for a Case Dossier.
    /// Resolves canonical titles across Archive, Gameplay HUD, and Notebook.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCaseData", menuName = "Case Closed/Case Data")]
    public class CaseData : ScriptableObject
    {
        [Header("Case Identification")]
        public int caseNumber = 1;
        public string title = "CASE 01: THE MISSING NECKLACE";
        public string subtitle = "A high-society gala turns into a crime scene.";
        public CaseStatus status = CaseStatus.Available;

        [Header("Artwork & Backgrounds")]
        [Tooltip("The dossier folder sprite from Assets/Assets/BUTTONS or SUSPECT FOLDERS")]
        public Sprite folderSprite;

        [Tooltip("Main investigation background from Assets/Assets/BACKGROUNDS")]
        public Sprite background;

        [Header("Investigation Roster")]
        public List<CharacterData> suspects = new List<CharacterData>();
        public List<EvidenceData> evidence = new List<EvidenceData>();

        [Header("Rules & Limits")]
        [Tooltip("Case time limit in seconds (0 = untimed)")]
        public float timeLimitSeconds = 300f;
    }
}
