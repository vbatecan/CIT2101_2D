using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaseClosed.Data
{
    [CreateAssetMenu(fileName = "NewEvidenceData", menuName = "Case Closed/Evidence Data")]
    public class EvidenceData : ScriptableObject
    {
        [Header("Evidence Identity")]
        public string id;
        public string evidenceName;

        [Header("Perspective Sprites")]
        [Tooltip("Physical prop perspective sitting on investigation desk (TablePOV)")]
        public Sprite tableSprite;

        [Tooltip("Enlarged top-down inspection perspective (TopPOV)")]
        public Sprite inspectSprite;

        [Header("Forensic Details")]
        [TextArea(2, 4)]
        public string baseDescription;
        [TextArea(3, 6)]
        public string detailedObservation;

        [Header("Associations")]
        public List<CharacterData> relatedSuspects = new List<CharacterData>();
    }
}
