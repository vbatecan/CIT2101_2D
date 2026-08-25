using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaseClosed.Data
{
    public enum EvidenceCategory
    {
        Photograph,
        Document,
        PersonalBelonging,
        PhysicalClue,
        ForensicReport,
        DigitalRecord
    }

    [Serializable]
    public class EvidenceHotspot
    {
        public string hotspotId;
        public string hotspotTitle;
        public Vector2 normalizedPosition; // Normalized (0 to 1) offset on zoomed sprite
        public float radius = 0.1f;
        [TextArea(2, 5)]
        public string observationText;
        public string clueUnlockedId;
        public bool isDiscovered;
    }

    [CreateAssetMenu(fileName = "NewEvidence", menuName = "Case Closed/Evidence Item")]
    public class EvidenceSO : ScriptableObject
    {
        [Header("Basic Information")]
        public string id;
        public string evidenceName;
        public EvidenceCategory category;

        [Header("Visual Sprites")]
        public Sprite normalSprite;
        public Sprite highlightedSprite;
        public Sprite zoomedSprite;

        [Header("Multi-Stage Descriptions")]
        [TextArea(2, 4)]
        public string baseDescription;
        [TextArea(3, 6)]
        public string detailedObservation;
        [TextArea(2, 4)]
        public string unlockedClueText;

        [Header("State Flags")]
        public bool startsDiscovered = true;
        public bool isExamined = false;
        public bool isToggledOnTable = true;

        [Header("Inspectable Hotspots")]
        public List<EvidenceHotspot> hotspots = new List<EvidenceHotspot>();

        public void ResetRuntimeState()
        {
            isExamined = false;
            isToggledOnTable = startsDiscovered;
            if (hotspots != null)
            {
                foreach (var spot in hotspots)
                {
                    spot.isDiscovered = false;
                }
            }
        }
    }
}
