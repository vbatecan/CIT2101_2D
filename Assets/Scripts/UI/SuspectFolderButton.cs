using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    /// <summary>
    /// Interactive button component for the Suspect Folder Button in the hierarchy.
    /// Supports uGUI pointer clicks, standard Button onClick, 2D physics clicks,
    /// and optional hover animations.
    /// </summary>
    public class SuspectFolderButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Hover Feedback (Optional)")]
        [Tooltip("Scale multiplier applied when cursor hovers over the button.")]
        public float hoverScaleMultiplier = 1.05f;

        private Vector3 _originalScale;
        private bool _isHovered = false;
        private Button _boundButton;

        private void Awake()
        {
            _originalScale = transform.localScale;
            EnsureClickable();
        }

        private void OnEnable()
        {
            transform.localScale = _originalScale;
            _isHovered = false;
            EnsureButtonBinding();
        }

        private void OnDisable()
        {
            transform.localScale = _originalScale;
            _isHovered = false;
            if (_boundButton != null)
            {
                _boundButton.onClick.RemoveListener(OnClick);
            }
        }

        /// <summary>
        /// Uses a Canvas click target or a 2D collider for world-space folders.
        /// </summary>
        public void EnsureClickable()
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            Vector2 clickSize = spriteRenderer != null && spriteRenderer.sprite != null
                ? spriteRenderer.sprite.rect.size / spriteRenderer.sprite.pixelsPerUnit
                : new Vector2(25f, 17f);

            if (GetComponentInParent<Canvas>() == null)
            {
                BoxCollider2D worldCollider = GetComponent<BoxCollider2D>();
                if (worldCollider == null) worldCollider = gameObject.AddComponent<BoxCollider2D>();
                worldCollider.size = clickSize;
                return;
            }

            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            if (collider != null) collider.size = clickSize;

            Button button = GetComponentInChildren<Button>(true);
            if (button == null)
            {
                GameObject clickTarget = new GameObject("FolderClickTarget", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                clickTarget.transform.SetParent(transform, false);

                Image image = clickTarget.GetComponent<Image>();
                image.color = Color.clear;
                image.raycastTarget = true;

                button = clickTarget.GetComponent<Button>();
                button.transition = Selectable.Transition.None;
                button.targetGraphic = image;
            }
            if (spriteRenderer != null && spriteRenderer.sprite != null && button.targetGraphic is Image folderImage)
            {
                // Keep the authored folder art on the Canvas click target so it
                // follows the same rendering and input path as the other UI.
                folderImage.sprite = spriteRenderer.sprite;
                folderImage.color = Color.white;
                folderImage.preserveAspect = true;
                folderImage.raycastTarget = true;
                spriteRenderer.enabled = false;
            }

            RectTransform clickRect = button.GetComponent<RectTransform>();
            if (clickRect != null)
            {
                clickRect.anchorMin = new Vector2(0.5f, 0.5f);
                clickRect.anchorMax = new Vector2(0.5f, 0.5f);
                clickRect.anchoredPosition = Vector2.zero;
                clickRect.sizeDelta = clickSize;
            }

            EnsureButtonBinding();
        }

        /// <summary>
        /// Ensures an underlying Button component is present or hooked.
        /// </summary>
        public void EnsureButtonBinding()
        {
            Button button = GetComponent<Button>();
            if (button == null) button = GetComponentInChildren<Button>(true);
            if (_boundButton != null && _boundButton != button)
            {
                _boundButton.onClick.RemoveListener(OnClick);
            }
            _boundButton = button;
            if (_boundButton != null)
            {
                _boundButton.transition = Selectable.Transition.None;
                _boundButton.onClick.RemoveListener(OnClick);
                _boundButton.onClick.AddListener(OnClick);
            }
        }

        /// <summary>
        /// Handles click via IPointerClickHandler interface.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_boundButton == null && eventData.button == PointerEventData.InputButton.Left)
            {
                OnClick();
            }
        }

        /// <summary>
        /// Invoked when the suspect folder button is clicked.
        /// </summary>
        public void OnClick()
        {
            Debug.Log("[UI:SuspectFolderButton] Clicked — toggling Suspect Folder modal");
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ToggleSuspectFolderPanel();
            }
            else
            {
                // Fallback for standalone scenes without UIManager
                var suspectUI = Object.FindFirstObjectByType<SuspectFolderUI>(FindObjectsInactive.Include);
                if (suspectUI != null)
                {
                    suspectUI.gameObject.SetActive(!suspectUI.gameObject.activeSelf);
                }
            }

            AudioManager audio = AudioManager.Instance;
            if (audio != null) audio.PlayPaperFlip();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_isHovered && hoverScaleMultiplier > 1f)
            {
                _isHovered = true;
                transform.localScale = _originalScale * hoverScaleMultiplier;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isHovered)
            {
                _isHovered = false;
                transform.localScale = _originalScale;
            }
        }

        private void OnMouseDown()
        {
            if (_boundButton == null)
            {
                OnClick();
            }
        }
    }
}
