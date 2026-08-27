using UnityEngine;
using UnityEngine.EventSystems;
using CaseClosed.Data;
using CaseClosed.Managers;

namespace CaseClosed.Gameplay
{
    /// <summary>
    /// Gameplay MonoBehaviour attached to physical evidence placed on the investigation desk,
    /// providing hover highlights and single/double-click inspection interactions.
    /// Can be dragged directly onto a Table Item GameObject in the Unity Inspector.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class TableEvidenceItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Evidence Data Link")]
        public EvidenceSO evidenceData;

        [Header("Visual Feedback")]
        public SpriteRenderer spriteRenderer;
        public GameObject highlightGlow;
        public Color hoverColor = new Color(1f, 1f, 0.8f, 1f);

        private Color originalColor = Color.white;

        /// <summary>
        /// Retrieves the attached SpriteRenderer if not set in Inspector.
        /// </summary>
        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Initializes the item sprite and baseline colors on start.
        /// </summary>
        private void Start()
        {
            if (evidenceData != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = evidenceData.normalSprite;
                originalColor = spriteRenderer.color;
            }
            if (highlightGlow != null) highlightGlow.SetActive(false);
        }

        /// <summary>
        /// Applies hover tint and highlighted sprite when the mouse pointer hovers over the item.
        /// </summary>
        /// <param name="eventData">Pointer event data from EventSystem.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = hoverColor;
                if (evidenceData != null && evidenceData.highlightedSprite != null)
                    spriteRenderer.sprite = evidenceData.highlightedSprite;
            }
            if (highlightGlow != null) highlightGlow.SetActive(true);
        }

        /// <summary>
        /// Restores original color and sprite when the mouse pointer exits the item boundary.
        /// </summary>
        /// <param name="eventData">Pointer event data from EventSystem.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
                if (evidenceData != null)
                    spriteRenderer.sprite = evidenceData.normalSprite;
            }
            if (highlightGlow != null) highlightGlow.SetActive(false);
        }

        /// <summary>
        /// Handles click events on the table item: single click selects, double-click/right-click opens zoom inspect modal.
        /// </summary>
        /// <param name="eventData">Pointer event data from EventSystem.</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (evidenceData == null) return;

            EvidenceManager.Instance?.SelectEvidence(evidenceData);

            if (eventData.clickCount >= 2 || eventData.button == PointerEventData.InputButton.Right)
            {
                EvidenceManager.Instance?.OpenInspectModal(evidenceData);
            }
            else
            {
                AudioManager.Instance?.PlayButtonClick();
            }
        }
    }
}
