using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Managers;

namespace CaseClosed.Gameplay
{
    /// <summary>
    /// Gameplay MonoBehaviour attached to interactive 2D hotspot collider objects,
    /// notifying <see cref="EvidenceManager"/> upon player click.
    /// Can be dragged directly onto a Hotspot GameObject in the Unity Inspector.
    /// </summary>
    public class InteractiveHotspot : MonoBehaviour
    {
        /// <summary>The data definition for this interactive hotspot.</summary>
        public EvidenceHotspot hotspotData;

        /// <summary>
        /// Handles direct collider clicks, triggering discovery in <see cref="EvidenceManager"/>.
        /// </summary>
        private void OnMouseDown()
        {
            if (hotspotData != null)
            {
                Debug.Log($"[Gameplay:Hotspot] Clicked hotspot '{hotspotData.hotspotTitle}' (ID: {hotspotData.hotspotId}, Discovered: {hotspotData.isDiscovered})");
                EvidenceManager.Instance?.DiscoverHotspot(hotspotData);
            }
        }
    }
}
