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
        public Color lockedColor = new Color(0.35f, 0.40f, 0.45f, 0.45f); // Semi-transparent slate

        [Tooltip("Color tint when all dialogues are done and all evidence is unlocked (ready).")]
        public Color unlockedColor = Color.white;

        [Header("Locked Tooltip Settings")]
        [SerializeField] private GameObject tooltipObject;
        [SerializeField] private TMPro.TextMeshProUGUI tooltipText;

        private Vector3 _originalScale;
        private bool _isHovered = false;
        private Button _boundButton;
        private SpriteRenderer _spriteRenderer;
        private Image _buttonImage;
        private bool _isReady = false;
        private CaseManager _subscribedCaseManager;

        private void Awake()
        {
            _originalScale = transform.localScale;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            EnsureClickable();
        }

        private void Start()
        {
            EnsureClickable();
            SubscribeEvents();
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
            HideTooltip();
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            CaseManager publisher = CaseManager.Instance;
            if (_subscribedCaseManager == publisher) return;
            UnsubscribeEvents();
            _subscribedCaseManager = publisher;
            if (_subscribedCaseManager != null)
            {
                _subscribedCaseManager.OnConclusionReadinessChanged += HandleReadinessChanged;
                _subscribedCaseManager.OnDialogueTreeCompleted += HandleDialogueChanged;
                _subscribedCaseManager.OnEvidenceDiscovered += HandleEvidenceChanged;
                _subscribedCaseManager.OnCaseLoaded += HandleCaseLoaded;
            }
        }

        private void UnsubscribeEvents()
        {
            if (_subscribedCaseManager != null)
            {
                _subscribedCaseManager.OnConclusionReadinessChanged -= HandleReadinessChanged;
                _subscribedCaseManager.OnDialogueTreeCompleted -= HandleDialogueChanged;
                _subscribedCaseManager.OnEvidenceDiscovered -= HandleEvidenceChanged;
                _subscribedCaseManager.OnCaseLoaded -= HandleCaseLoaded;
            }
            _subscribedCaseManager = null;
        }

        private void HandleReadinessChanged(bool ready) => UpdateReadinessState();
        private void HandleDialogueChanged(string treeId) => UpdateReadinessState();
        private void HandleEvidenceChanged(Data.EvidenceSO ev) => UpdateReadinessState();
        private void HandleCaseLoaded(Data.CaseSO caseData) => UpdateReadinessState();

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

            if (_boundButton != null)
            {
                _boundButton.interactable = _isReady;
            }

            if (!_isReady && _isHovered)
            {
                _isHovered = false;
                transform.localScale = _originalScale;
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
                button.transition = Selectable.Transition.None;
                button.targetGraphic = image;
            }
            else
            {
                button = clickTarget.GetComponent<Button>();
                if (button != null) button.transition = Selectable.Transition.None;
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
                _boundButton.interactable = _isReady;
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
            UpdateReadinessState();
            if (!_isReady)
            {
                Debug.Log($"[UI:ConcludeCaseButton] Locked — {GetReadinessHint()}");
                AudioManager.Instance?.PlayPaperFlip();
                ShowTooltip();
                StopAllCoroutines();
                StartCoroutine(ShakeButton());
                return;
            }

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
            if (_isReady && hoverScaleMultiplier > 1f)
            {
                _isHovered = true;
                transform.localScale = _originalScale * hoverScaleMultiplier;
            }
            else if (!_isReady)
            {
                ShowTooltip();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isHovered)
            {
                _isHovered = false;
                transform.localScale = _originalScale;
            }
            HideTooltip();
        }

        private void ShowTooltip()
        {
            if (tooltipObject == null)
            {
                CreateTooltip();
            }
            if (tooltipObject != null)
            {
                if (tooltipText != null)
                {
                    tooltipText.text = GetReadinessHint();
                }
                tooltipObject.SetActive(true);
            }
        }

        private void HideTooltip()
        {
            if (tooltipObject != null)
            {
                tooltipObject.SetActive(false);
            }
        }

        private string GetReadinessHint()
        {
            if (CaseManager.Instance == null || CaseManager.Instance.ActiveCase == null)
                return "Investigation in progress...";

            bool evDone = CaseManager.Instance.AreAllEvidenceUnlocked();
            bool dlgDone = CaseManager.Instance.AreAllDialoguesDone();

            if (!evDone && !dlgDone)
                return "Discover all evidence & interrogate suspects to conclude";
            if (!evDone)
                return "Discover remaining evidence to conclude";
            if (!dlgDone)
                return "Complete suspect interrogations to conclude";

            return "Ready to conclude case!";
        }

        private void CreateTooltip()
        {
            Transform existing = transform.Find("LockedTooltip");
            if (existing != null)
            {
                tooltipObject = existing.gameObject;
                tooltipText = tooltipObject.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
                return;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            Transform parentTransform = canvas != null ? canvas.transform : transform;

            tooltipObject = new GameObject("LockedTooltip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            tooltipObject.transform.SetParent(parentTransform, false);

            Image bg = tooltipObject.GetComponent<Image>();
            bg.color = new Color(0.10f, 0.11f, 0.14f, 0.95f);
            bg.raycastTarget = false;

            RectTransform rt = tooltipObject.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(320f, 44f);
            rt.position = transform.position + new Vector3(0f, 0.8f, 0f);

            GameObject textGo = new GameObject("TooltipText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
            textGo.transform.SetParent(tooltipObject.transform, false);

            tooltipText = textGo.GetComponent<TMPro.TextMeshProUGUI>();
            tooltipText.fontSize = 12f;
            tooltipText.alignment = TMPro.TextAlignmentOptions.Center;
            tooltipText.color = new Color(0.95f, 0.90f, 0.82f, 1f);
            tooltipText.raycastTarget = false;

            RectTransform textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = new Vector2(-12f, -8f);
            textRt.anchoredPosition = Vector2.zero;

            tooltipObject.SetActive(false);
        }

        private System.Collections.IEnumerator ShakeButton()
        {
            float elapsed = 0f;
            float duration = 0.3f;
            Vector3 originalPos = transform.localPosition;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float xOffset = Mathf.Sin(elapsed * 45f) * 0.05f;
                transform.localPosition = originalPos + new Vector3(xOffset, 0f, 0f);
                yield return null;
            }
            transform.localPosition = originalPos;
        }
    }
}
