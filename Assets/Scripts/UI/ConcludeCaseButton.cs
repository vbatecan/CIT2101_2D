using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    /// <summary>
    /// Interactive button component for the Conclude Case Button in the investigation view.
    /// Manages locked/unlocked visual feedback (dimmed vs active), uGUI pointer clicks,
    /// standard Button onClick, and 2D physics clicks to transition to the Conclusion Quiz.
    /// </summary>
    public class ConcludeCaseButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Visual Feedback Settings")]
        [Tooltip("Scale multiplier applied when cursor hovers over an active button.")]
        public float hoverScaleMultiplier = 1.05f;

        [Tooltip("Color tint when conclusion requirements are not yet satisfied (locked).")]
        public Color lockedColor = new Color(0.45f, 0.45f, 0.45f, 0.65f);

        [Tooltip("Color tint when all dialogues are done and all evidence is unlocked (ready).")]
        public Color unlockedColor = Color.white;

        private Vector3 _originalScale;
        private bool _isHovered = false;
        private Button _boundButton;
        private SpriteRenderer _spriteRenderer;
        private Image _buttonImage;
        private bool _isReady = false;

        private void Awake()
        {
            _originalScale = transform.localScale;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            EnsureClickable();
        }

        private void Start()
        {
            EnsureClickable();
            UpdateReadinessState();
        }

        private void OnEnable()
        {
            transform.localScale = _originalScale;
            _isHovered = false;
            EnsureButtonBinding();
            SubscribeEvents();
            UpdateReadinessState();
        }

        private void OnDisable()
        {
            transform.localScale = _originalScale;
            _isHovered = false;
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (CaseManager.Instance != null)
            {
                CaseManager.Instance.OnConclusionReadinessChanged -= HandleReadinessChanged;
                CaseManager.Instance.OnConclusionReadinessChanged += HandleReadinessChanged;
                CaseManager.Instance.OnDialogueTreeCompleted -= HandleDialogueChanged;
                CaseManager.Instance.OnDialogueTreeCompleted += HandleDialogueChanged;
                CaseManager.Instance.OnEvidenceDiscovered -= HandleEvidenceChanged;
                CaseManager.Instance.OnEvidenceDiscovered += HandleEvidenceChanged;
            }
        }

        private void UnsubscribeEvents()
        {
            if (CaseManager.Instance != null)
            {
                CaseManager.Instance.OnConclusionReadinessChanged -= HandleReadinessChanged;
                CaseManager.Instance.OnDialogueTreeCompleted -= HandleDialogueChanged;
                CaseManager.Instance.OnEvidenceDiscovered -= HandleEvidenceChanged;
            }
        }

        private void HandleReadinessChanged(bool ready) => UpdateReadinessState();
        private void HandleDialogueChanged(string treeId) => UpdateReadinessState();
        private void HandleEvidenceChanged(Data.EvidenceSO ev) => UpdateReadinessState();

        /// <summary>
        /// Updates the visual tint and interactability based on CaseManager readiness.
        /// </summary>
        public void UpdateReadinessState()
        {
            _isReady = CaseManager.Instance != null && CaseManager.Instance.IsReadyForConclusion();

            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _isReady ? unlockedColor : lockedColor;
            }
            else
            {
                Image mainImg = GetComponent<Image>();
                if (mainImg != null)
                {
                    mainImg.color = _isReady ? unlockedColor : lockedColor;
                }
            }

            // Keep the invisible hitbox transparent
            if (_buttonImage != null)
            {
                _buttonImage.color = Color.clear;
                _buttonImage.raycastTarget = true;
            }

            // Always interactable so clicking concludes the case
            if (_boundButton != null)
            {
                _boundButton.interactable = true;
            }
        }

        /// <summary>
        /// Ensures both 2D physics collider and uGUI click target are present and ready.
        /// </summary>
        public void EnsureClickable()
        {
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();

            Vector2 clickSize = _spriteRenderer != null && _spriteRenderer.sprite != null
                ? _spriteRenderer.sprite.rect.size / _spriteRenderer.sprite.pixelsPerUnit
                : new Vector2(37.63f, 9.81f);

            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<BoxCollider2D>();
            }
            collider.size = clickSize;

            Transform clickTarget = transform.Find("ConcludeClickTarget");
            Button button = null;
            if (clickTarget == null)
            {
                GameObject go = new GameObject("ConcludeClickTarget", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                go.transform.SetParent(transform, false);
                go.layer = 5; // UI layer
                clickTarget = go.transform;

                Image image = go.GetComponent<Image>();
                image.color = Color.clear;
                image.raycastTarget = true;
                _buttonImage = image;

                button = go.GetComponent<Button>();
                button.targetGraphic = image;
            }
            else
            {
                button = clickTarget.GetComponent<Button>();
                _buttonImage = clickTarget.GetComponent<Image>();
                if (_buttonImage != null)
                {
                    _buttonImage.color = Color.clear;
                    _buttonImage.raycastTarget = true;
                }
            }

            RectTransform clickRect = clickTarget.GetComponent<RectTransform>();
            if (clickRect != null)
            {
                clickRect.anchorMin = new Vector2(0.5f, 0.5f);
                clickRect.anchorMax = new Vector2(0.5f, 0.5f);
                clickRect.pivot = new Vector2(0.5f, 0.5f);
                clickRect.anchoredPosition = Vector2.zero;
                clickRect.sizeDelta = clickSize;
            }

            EnsureButtonBinding();
        }

        /// <summary>
        /// Ensures the underlying Button component is hooked to OnClick.
        /// </summary>
        public void EnsureButtonBinding()
        {
            _boundButton = GetComponent<Button>();
            if (_boundButton == null)
            {
                _boundButton = GetComponentInChildren<Button>(true);
            }

            if (_boundButton != null)
            {
                _boundButton.interactable = true;
                _boundButton.onClick.RemoveListener(OnClick);
                _boundButton.onClick.AddListener(OnClick);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnClick();
            }
        }

        private void OnMouseDown()
        {
            OnClick();
        }

        /// <summary>
        /// Invoked when the Conclude Case button is clicked.
        /// </summary>
        public void OnClick()
        {
            Debug.Log("[UI:ConcludeCaseButton] Clicked — opening conclusion quiz");
            AudioManager.Instance?.PlayButtonClick();

            if (UIManager.Instance != null)
            {
                UIManager.Instance.OpenConclusionQuiz();
            }
            else
            {
                var conclusionUI = Object.FindFirstObjectByType<ConclusionUI>(FindObjectsInactive.Include);
                if (conclusionUI != null)
                {
                    conclusionUI.gameObject.SetActive(true);
                }
            }
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
    }
}
