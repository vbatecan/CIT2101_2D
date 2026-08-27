using UnityEngine;
using UnityEngine.EventSystems;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;
using CaseClosed.UI;

namespace CaseClosed.Gameplay
{
    /// <summary>
    /// Gameplay MonoBehaviour attached to physical evidence and interactive items placed on the investigation desk,
    /// providing hover highlights, single-click selection/explanation, double-click zoom inspection,
    /// and case file notebook opening triggers.
    /// Can be dragged directly onto Table Item GameObjects in the Unity Inspector.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class TableEvidenceItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Evidence Data Link")]
        [Tooltip("The ScriptableObject defining this evidence item. Leave empty if this is a general book/tool prop.")]
        public EvidenceSO evidenceData;

        [Header("Interactive Behavior Mode")]
        [Tooltip("If true, clicking this item on the table directly opens the Case File Notebook (e.g. for the open case book on desk).")]
        public bool openNotebookOnClick = false;

        [Tooltip("Optional dialogue node ID to jump to in InterrogationManager when this item is clicked (suspect explains item).")]
        public string dialogueNodeToTriggerOnInspect;

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
            if (evidenceData != null && spriteRenderer != null && evidenceData.normalSprite != null)
            {
                spriteRenderer.sprite = evidenceData.normalSprite;
            }

            if (spriteRenderer != null)
            {
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
                if (evidenceData != null && evidenceData.normalSprite != null)
                    spriteRenderer.sprite = evidenceData.normalSprite;
            }
            if (highlightGlow != null) highlightGlow.SetActive(false);
        }

        /// <summary>
        /// Handles click events on the table item:
        /// - If openNotebookOnClick is enabled, toggles the Case File Notebook.
        /// - Single click: selects evidence and triggers suspect explanation dialogue if configured.
        /// - Double-click / Right-click: opens close-up inspect modal.
        /// </summary>
        /// <param name="eventData">Pointer event data from EventSystem.</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            string itemName = evidenceData != null ? evidenceData.evidenceName : gameObject.name;
            Debug.Log($"[Gameplay:TableEvidence] Clicked '{itemName}' (Button: {eventData.button}, ClickCount: {eventData.clickCount}, OpenNotebook: {openNotebookOnClick})");

            // 1. Check if configured to open notebook (Open Case Book on desk)
            if (openNotebookOnClick)
            {
                Debug.Log("[Gameplay:TableEvidence] Opening Case File Notebook from table book click");
                UIManager.Instance?.ShowPanel(UIPanelType.CaseFileNotebook);
                return;
            }

            if (evidenceData == null) return;

            // 2. Select evidence in EvidenceManager
            EvidenceManager.Instance?.SelectEvidence(evidenceData);

            // 3. Check for double-click / right-click zoom inspection
            if (eventData.clickCount >= 2 || eventData.button == PointerEventData.InputButton.Right)
            {
                Debug.Log($"[Gameplay:TableEvidence] Opening inspect modal for '{evidenceData.evidenceName}'");
                EvidenceManager.Instance?.OpenInspectModal(evidenceData);
            }
            else
            {
                AudioManager.Instance?.PlayButtonClick();

                // 4. Trigger character explanation dialogue if configured
                if (!string.IsNullOrEmpty(dialogueNodeToTriggerOnInspect))
                {
                    Debug.Log($"[Gameplay:TableEvidence] Triggering suspect explanation dialogue node '{dialogueNodeToTriggerOnInspect}' for '{evidenceData.evidenceName}'");
                    InterrogationManager.Instance?.JumpToNode(dialogueNodeToTriggerOnInspect);
                }
            }
        }
    }
}
