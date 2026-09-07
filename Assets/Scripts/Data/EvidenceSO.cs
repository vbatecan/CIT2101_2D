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
        [Tooltip("Legacy serialized compatibility value. Runtime hotspot discovery is tracked by CaseSessionState.")]
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
        [Tooltip("Top-down visual sprite (from Assets/Assets/EVIDENCES/TopPOV) used in the detective notebook and inspection. If null, falls back to zoomedSprite or normalSprite.")]
        public Sprite topPovSprite;
        [Tooltip("Optional custom background sprite for top-down inspection. If null, defaults to TableTOPVIEW.")]
        public Sprite customInspectBackground;

        /// <summary>
        /// Gets the top-down perspective sprite (TopPOV) for this evidence item, falling back to zoomedSprite or normalSprite if unassigned.
        /// </summary>
        public Sprite GetTopPovSprite()
        {
            if (topPovSprite != null) return topPovSprite;
            if (zoomedSprite != null) return zoomedSprite;
            return normalSprite;
        }

        [Header("Multi-Stage Descriptions")]
        [TextArea(2, 4)]
        public string baseDescription;
        [TextArea(3, 6)]
        public string detailedObservation;
        [TextArea(2, 4)]
        public string unlockedClueText;

        [Header("Authored Availability")]
        public bool startsDiscovered = false;
        [Tooltip("Optional dialogue node that must be completed before this evidence appears on the investigation table.")]
        public string requiredDialogueNodeId;
        [Tooltip("Optional dialogue node to display when this evidence is inspected from the investigation table.")]
        public string dialogueNodeToTriggerOnInspect;
        [Header("Legacy Runtime State (Serialized Compatibility Only)")]
        [Tooltip("Retained to preserve existing serialized assets. Runtime examination state lives in CaseSessionState.")]
        public bool isExamined = false;
        [Tooltip("Retained to preserve existing serialized assets. Runtime table presence lives in CaseSessionState.")]
        public bool isToggledOnTable = true;

        [Header("Inspectable Hotspots")]
        public List<EvidenceHotspot> hotspots = new List<EvidenceHotspot>();

        /// <summary>
        /// Legacy no-op retained for source compatibility. Runtime session state is reset by
        /// <see cref="CaseClosed.Services.CaseSessionState.Begin"/> instead of mutating this asset.
        /// </summary>
        [Obsolete("Runtime evidence state belongs to CaseSessionState. Load a new case session instead.")]
        public void ResetRuntimeState()
        {
        }
    }
}
