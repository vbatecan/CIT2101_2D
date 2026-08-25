using UnityEngine;
using UnityEngine.EventSystems;
using CaseClosed.Data;
using CaseClosed.Managers;

namespace CaseClosed.Gameplay
{
    public class InteractiveHotspot : MonoBehaviour, IPointerClickHandler
    {
        public EvidenceHotspot hotspotData;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (hotspotData != null)
            {
                EvidenceManager.Instance?.DiscoverHotspot(hotspotData);
            }
        }
    }
}
