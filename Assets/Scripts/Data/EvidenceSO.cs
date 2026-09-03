using System;
using System.Collections.Generic;
using UnityEngine;
using CaseClosed.Enums;

namespace CaseClosed.Data
{
    /// <summary>
    /// Represents an inspectable 2D hotspot region located on an evidence item during close-up examination.
    /// </summary>
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

    /// <summary>
    /// ScriptableObject defining an evidence item, including visual sprites, detailed descriptions, and inspectable hotspots.
    /// </summary>
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
        [Tooltip("Optional dialogue node that must be completed before this evidence appears on the investigation table.")]
        public string requiredDialogueNodeId;
        [Tooltip("Optional dialogue node to display when this evidence is inspected from the investigation table.")]
        public string dialogueNodeToTriggerOnInspect;
        public bool isExamined = false;
        public bool isToggledOnTable = true;

        [Header("Inspectable Hotspots")]
        public List<EvidenceHotspot> hotspots = new List<EvidenceHotspot>();

        /// <summary>
        /// Resets the runtime examination flags and hotspot discovery states back to initial defaults.
        /// </summary>
        public void ResetRuntimeState()
        {
            isExamined = false;
            isToggledOnTable = startsDiscovered;
            if (hotspots != null)
            {
                foreach (var spot in hotspots)
                {
                    if (spot != null)
                    {
                        spot.isDiscovered = false;
                    }
                }
            }
        }
    }
}
