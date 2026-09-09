using UnityEngine;
using UnityEngine.EventSystems;
using CaseClosed.Data;
using CaseClosed.Managers;

namespace CaseClosed.Gameplay
{
    /// <summary>
    /// Gameplay MonoBehaviour attached to interactive 2D hotspot collider objects,
    /// notifying <see cref="EvidenceManager"/> upon player click.
    /// Can be dragged directly onto a Hotspot GameObject in the Unity Inspector.
    /// </summary>
    public class InteractiveHotspot : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>The data definition for this interactive hotspot.</summary>
        public EvidenceHotspot hotspotData;

        /// <summary>
        /// Handles EventSystem pointer clicks on this hotspot collider.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            TriggerDiscovery();
        }

        /// <summary>
        /// Handles direct collider clicks, triggering discovery in <see cref="EvidenceManager"/>.
        /// </summary>
        private void OnMouseDown()
        {
            TriggerDiscovery();
        }

        /// <summary>
        /// Triggers discovery for this hotspot via <see cref="EvidenceManager"/>.
        /// </summary>
        public void TriggerDiscovery()
        {
            if (hotspotData != null)
            {
                Debug.Log($"[Gameplay:Hotspot] Clicked hotspot '{hotspotData.hotspotTitle}' (ID: {hotspotData.hotspotId})");
                EvidenceManager.Instance?.DiscoverHotspot(hotspotData);
            }
        }
    }
}
