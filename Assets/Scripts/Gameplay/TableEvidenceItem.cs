using UnityEngine;
using UnityEngine.EventSystems;
using CaseClosed.Data;
using CaseClosed.Managers;

namespace CaseClosed.Gameplay
{
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

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            if (evidenceData != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = evidenceData.normalSprite;
                originalColor = spriteRenderer.color;
            }
            if (highlightGlow != null) highlightGlow.SetActive(false);
        }

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
