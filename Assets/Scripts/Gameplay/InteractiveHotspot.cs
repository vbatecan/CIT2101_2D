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
            TriggerDiscovery();
        }

        private void OnMouseDown()
        {
            TriggerDiscovery();
        }

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
