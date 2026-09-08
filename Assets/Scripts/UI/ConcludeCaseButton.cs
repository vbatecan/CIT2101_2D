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

            if (_buttonImage != null)
            {
                _buttonImage.color = _isReady ? unlockedColor : lockedColor;
            }

            if (_boundButton != null)
            {
                _boundButton.interactable = _isReady;
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
                : new Vector2(25f, 10f);

            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<BoxCollider2D>();
            }
            collider.size = clickSize;

            Button button = GetComponentInChildren<Button>(true);
            if (button == null)
            {
                GameObject clickTarget = new GameObject("ConcludeClickTarget", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                clickTarget.transform.SetParent(transform, false);

                Image image = clickTarget.GetComponent<Image>();
                image.color = Color.clear;
                image.raycastTarget = true;
                _buttonImage = image;

                button = clickTarget.GetComponent<Button>();
                button.targetGraphic = image;
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
            if (_boundButton == null)
            {
                OnClick();
            }
        }

        /// <summary>
        /// Invoked when the Conclude Case button is clicked.
        /// </summary>
        public void OnClick()
        {
            bool ready = CaseManager.Instance != null && CaseManager.Instance.IsReadyForConclusion();
            Debug.Log($"[UI:ConcludeCaseButton] Clicked — Readiness: {ready}");

            if (ready)
            {
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
            else
            {
                AudioManager.Instance?.PlaySFX(AudioManager.Instance?.caseFailedSFX);
                bool evDone = CaseManager.Instance != null && CaseManager.Instance.AreAllEvidenceUnlocked();
                bool diagDone = CaseManager.Instance != null && CaseManager.Instance.AreAllDialoguesDone();
                Debug.LogWarning($"[UI:ConcludeCaseButton] Conclude Case is locked! EvidenceUnlocked: {evDone}, DialoguesDone: {diagDone}. Finish all dialogues and uncover all clues first.");
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_isHovered && _isReady && hoverScaleMultiplier > 1f)
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
