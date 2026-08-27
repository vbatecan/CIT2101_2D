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
        /// Handles pointer click events, triggering discovery in <see cref="EvidenceManager"/>.
        /// </summary>
        /// <param name="eventData">Pointer event data provided by the Unity EventSystem.</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (hotspotData != null)
            {
                EvidenceManager.Instance?.DiscoverHotspot(hotspotData);
            }
        }
    }
}
