using UnityEngine;
using UnityEngine.EventSystems;
using CaseClosed.Data;
using CaseClosed.Managers;
using CaseClosed.UI;

namespace CaseClosed.Gameplay
{
    /// <summary>
    /// Attaches to suspect GameObjects to receive 2D mouse clicks and pointer events,
    /// triggering interrogation dialogue and providing tactile hover feedback.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SuspectClickTrigger : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Dialogue Target")]
        [SerializeField] private DialogueData customDialogueData;
        [SerializeField] private string targetDialogueTreeId = "TREE_CHARL_01";
        [SerializeField] private CharacterProfileSO suspectProfile;

        [Header("Hover Feedback")]
        [SerializeField] private float hoverScaleMultiplier = 1.025f;
        [SerializeField] private Color hoverTint = new Color(1f, 1f, 0.88f, 1f);

        private Vector3 _originalScale;
        private Color _originalColor = Color.white;
        private SpriteRenderer _spriteRenderer;
        private bool _isHovered = false;

        private void Awake()
        {
            _originalScale = transform.localScale;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null) _originalColor = _spriteRenderer.color;

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = false;
        }

        public void OnPointerEnter(PointerEventData eventData) => SetHover(true);
        public void OnPointerExit(PointerEventData eventData) => SetHover(false);

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                TriggerInterrogation();
            }
        }

        private void OnMouseEnter() => SetHover(true);
        private void OnMouseExit() => SetHover(false);

        private void OnMouseDown()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            TriggerInterrogation();
        }

        private void SetHover(bool hovered)
        {
            _isHovered = hovered;
            transform.localScale = hovered ? _originalScale * hoverScaleMultiplier : _originalScale;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = hovered ? hoverTint : _originalColor;
            }
        }

        public void TriggerInterrogation()
        {
            AudioManager.Instance?.PlayButtonClick();

            // 1. If custom DialogueData is assigned, launch modern DialogueBox
            if (customDialogueData != null && DialogueBox.Instance != null)
            {
                DialogueBox.Instance.StartDialogue(customDialogueData);
                return;
            }

            // 2. Connect to InterrogationManager session
            if (InterrogationManager.Instance != null)
            {
                CaseSO activeCase = CaseManager.Instance?.ActiveCase;
                if (activeCase != null && activeCase.dialogueTrees != null && activeCase.dialogueTrees.Count > 0)
                {
                    DialogueTreeSO targetTree = activeCase.dialogueTrees[0];
                    if (!string.IsNullOrEmpty(targetDialogueTreeId))
                    {
                        foreach (var tree in activeCase.dialogueTrees)
                        {
                            if (tree != null && tree.treeId == targetDialogueTreeId)
                            {
                                targetTree = tree;
                                break;
                            }
                        }
                    }

                    CharacterProfileSO profile = suspectProfile != null ? suspectProfile : activeCase.primarySuspect;
                    InterrogationManager.Instance.SetInterrogationTarget(profile, targetTree);
                }
            }
        }
    }
}
