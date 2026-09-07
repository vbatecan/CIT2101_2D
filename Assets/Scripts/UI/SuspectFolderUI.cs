using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;
using CaseClosed.Services;

namespace CaseClosed.UI
{
    /// <summary>
    /// UI View MonoBehaviour managing the detective's suspect dossiers folder modal.
    /// Animates smooth slide transitions, displays the active case's suspect file,
    /// and provides smooth mouse scroll zoom and left-click drag movement.
    /// </summary>
    public class SuspectFolderUI : MonoBehaviour, IScrollHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [Header("Hierarchy & Animations")]
        [Tooltip("RectTransform of the centered suspect folder to animate sliding in/out.")]
        public RectTransform folderRoot;

        [Tooltip("Full-screen transparent or dimmed backdrop button to dismiss when clicking outside.")]
        public Button backdropButton;

        [Tooltip("Prominent button to close the suspect folder.")]
        public Button closeFolderButton;

        [Tooltip("Duration of the slide-in and slide-out animations in seconds.")]
        public float slideDuration = 0.35f;

        [Tooltip("Off-screen vertical Y position when hidden below the table.")]
        public float hiddenPosY = -1100f;

        [Tooltip("On-screen vertical Y position when held up in front of the detective.")]
        public float visiblePosY = 0f;

        [Header("Zoom & Move Settings")]
        [Tooltip("Minimum zoom magnification scale factor.")]
        [Range(0.5f, 1.5f)] public float minZoom = 1.0f;

        [Tooltip("Maximum zoom magnification scale factor.")]
        [Range(2.0f, 5.0f)] public float maxZoom = 3.5f;

        [Tooltip("Scroll wheel zoom sensitivity.")]
        public float scrollSensitivity = 0.15f;

        [Tooltip("Whether zoom and position interpolate smoothly.")]
        public bool smoothZoom = true;

        [Tooltip("Interpolation speed for smooth zoom and position.")]
        public float zoomLerpSpeed = 15f;

        [Tooltip("Safety margin in pixels kept inside screen bounds when moving.")]
        public float safetyMargin = 100f;

        [Header("Visual Display")]
        [Tooltip("UI Image rendering the suspect folder dossier sprite.")]
        public Image folderDisplayImage;

        [Tooltip("Optional text label for displaying the suspect dossier title.")]
        public Text dossierTitleText;

        [Header("Suspect Dossier Sprites")]
        [Tooltip("Folder sprite for Case 1 (Vince & Jane).")]
        public Sprite case1FolderSprite;

        [Tooltip("Folder sprite for Case 2 (Paul & Vonn).")]
        public Sprite case2FolderSprite;

        [Tooltip("Folder sprite for Case 3 (Shania & Shan).")]
        public Sprite case3FolderSprite;

        [Header("Overrides & Testing")]
        [Tooltip("Manual override for testing (0 = Auto-detect from active case or scene, 1-3 = Force case).")]
        public int forcedCaseIndex = 0;

        [Tooltip("Direct reference to a CaseSO for isolated testing.")]
        public CaseSO activeCaseOverride;

        private readonly SuspectFolderService _service = new SuspectFolderService();
        private Coroutine _slideCoroutine;
        private bool _isClosing = false;
        private bool _buttonsConfigured = false;
        private int _currentCaseIndex = 1;

        private float _currentZoom = 1.0f;
        private float _targetZoom = 1.0f;
        private Vector2 _currentPosition = Vector2.zero;
        private Vector2 _targetPosition = Vector2.zero;
        private bool _isDragging = false;
        private Vector2 _lastDragLocalPos;
        private RectTransform _parentCanvasRect;

        public int CurrentCaseIndex => _currentCaseIndex;
        public bool IsClosing => _isClosing;
        public float CurrentZoom => _currentZoom;
        public float TargetZoom => _targetZoom;
        public Vector2 CurrentPosition => _currentPosition;
        public Vector2 TargetPosition => _targetPosition;
        public bool IsDragging => _isDragging;

        private void Awake()
        {
            SetupButtons();
            EnsureRaycastSetup();
            TryAutoLoadSprites();
        }

        private void Start()
        {
            SetupButtons();
            EnsureRaycastSetup();
        }

        private void OnEnable()
        {
            _isClosing = false;
            _isDragging = false;
            SetupButtons();
            EnsureRaycastSetup();
            RefreshDossierView();
            ResetView();

            if (folderRoot != null)
            {
                folderRoot.localScale = Vector3.one;
                Vector2 startPos = new Vector2(0f, hiddenPosY);
                folderRoot.anchoredPosition = startPos;
                _currentPosition = startPos;
                _targetPosition = Vector2.zero;

                if (_slideCoroutine != null) StopCoroutine(_slideCoroutine);
                _slideCoroutine = StartCoroutine(SlideCoroutine(hiddenPosY, visiblePosY, true));
            }

            AudioManager.Instance?.PlayPaperFlip();
        }

        private void OnDisable()
        {
            if (_slideCoroutine != null)
            {
                StopCoroutine(_slideCoroutine);
                _slideCoroutine = null;
            }
            _isDragging = false;
        }

        public void EnsureRaycastSetup()
        {
            if (folderDisplayImage != null)
            {
                folderDisplayImage.raycastTarget = true;
                var imgTarget = folderDisplayImage.GetComponent<SuspectFolderDragTarget>();
                if (imgTarget == null)
                {
                    imgTarget = folderDisplayImage.gameObject.AddComponent<SuspectFolderDragTarget>();
                }
                imgTarget.Init(this);
            }

            if (folderRoot != null)
            {
                var rootTarget = folderRoot.GetComponent<SuspectFolderDragTarget>();
                if (rootTarget == null)
                {
                    rootTarget = folderRoot.gameObject.AddComponent<SuspectFolderDragTarget>();
                }
                rootTarget.Init(this);
            }
        }

        private void Update()
        {
            if (_isClosing) return;

            // Keyboard shortcut: Escape to close
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnCloseClicked();
                return;
            }

            // Keyboard shortcut: 'R' to reset view
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetView();
            }

            // While animating slide in/out, do not apply regular zoom/pan lerp
            if (_slideCoroutine != null) return;
            if (folderRoot == null) return;

            if (smoothZoom)
            {
                _currentZoom = Mathf.Lerp(_currentZoom, _targetZoom, Time.unscaledDeltaTime * zoomLerpSpeed);
                _currentPosition = Vector2.Lerp(_currentPosition, _targetPosition, Time.unscaledDeltaTime * zoomLerpSpeed);
            }
            else
            {
                _currentZoom = _targetZoom;
                _currentPosition = _targetPosition;
            }

            folderRoot.localScale = new Vector3(_currentZoom, _currentZoom, 1f);
            folderRoot.anchoredPosition = _currentPosition;
        }

        public void SetupButtons()
        {
            if (_buttonsConfigured) return;

            if (closeFolderButton != null)
            {
                closeFolderButton.onClick.RemoveListener(OnCloseClicked);
                closeFolderButton.onClick.AddListener(OnCloseClicked);
            }

            if (backdropButton != null)
            {
                backdropButton.onClick.RemoveListener(OnCloseClicked);
                backdropButton.onClick.RemoveListener(OnBackdropClicked);
                backdropButton.onClick.AddListener(OnBackdropClicked);
            }

            UIButtonHighlightSystem.ApplyToHierarchy(gameObject);
            _buttonsConfigured = true;
        }

        /// <summary>
        /// Handles clicks on the backdrop button. If the pointer was over the folder, the click is ignored.
        /// </summary>
        public void OnBackdropClicked()
        {
            if (IsPointerOverFolder())
            {
                return;
            }
            OnCloseClicked();
        }

        /// <summary>
        /// Checks whether the mouse cursor is currently positioned over folderRoot or any of its children.
        /// </summary>
        public bool IsPointerOverFolder()
        {
            if (folderRoot == null) return false;

            if (EventSystem.current != null)
            {
                var pointerData = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };
                var results = new System.Collections.Generic.List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);
                foreach (var result in results)
                {
                    if (result.gameObject == null) continue;
                    if (result.gameObject.transform.IsChildOf(folderRoot) || result.gameObject == folderRoot.gameObject)
                    {
                        return true;
                    }
                }
            }

            Camera cam = GetComponentInParent<Canvas>()?.worldCamera;
            return RectTransformUtility.RectangleContainsScreenPoint(folderRoot, Input.mousePosition, cam);
        }

        /// <summary>
        /// Updates the folder display image and title according to the active case.
        /// </summary>
        public void RefreshDossierView()
        {
            CaseSO activeCase = activeCaseOverride;
            if (activeCase == null && CaseManager.Instance != null)
            {
                activeCase = CaseManager.Instance.ActiveCase;
            }

            string sceneName = SceneManager.GetActiveScene().name;

            if (forcedCaseIndex >= 1 && forcedCaseIndex <= 3)
            {
                _currentCaseIndex = forcedCaseIndex;
            }
            else
            {
                _currentCaseIndex = _service.DetermineCaseIndex(activeCase, sceneName);
            }

            Sprite selectedSprite = GetSpriteForCase(_currentCaseIndex);

            if (folderDisplayImage != null)
            {
                folderDisplayImage.preserveAspect = true;
                if (selectedSprite != null)
                {
                    folderDisplayImage.sprite = selectedSprite;
                    folderDisplayImage.enabled = true;
                }
            }

            if (dossierTitleText != null)
            {
                dossierTitleText.text = _service.GetDossierTitle(_currentCaseIndex);
            }
        }

        /// <summary>
        /// Retrieves the configured or loaded sprite for the given case index.
        /// </summary>
        public Sprite GetSpriteForCase(int caseIndex)
        {
            switch (caseIndex)
            {
                case 2:
                    return case2FolderSprite != null ? case2FolderSprite : case1FolderSprite;
                case 3:
                    return case3FolderSprite != null ? case3FolderSprite : case1FolderSprite;
                case 1:
                default:
                    return case1FolderSprite;
            }
        }

        /// <summary>
        /// Manually switches the active case folder view.
        /// </summary>
        public void SetCaseFolder(int caseIndex)
        {
            forcedCaseIndex = Mathf.Clamp(caseIndex, 1, 3);
            RefreshDossierView();
        }

        /// <summary>
        /// Sets target zoom scale factor clamped between minZoom and maxZoom.
        /// </summary>
        public void SetTargetZoom(float zoom)
        {
            _targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
            ClampTargetPosition();

            if (!smoothZoom)
            {
                _currentZoom = _targetZoom;
                _currentPosition = _targetPosition;
                ApplyTransform();
            }
        }

        /// <summary>
        /// Sets target position clamped within allowable screen bounds.
        /// </summary>
        public void SetTargetPosition(Vector2 position)
        {
            _targetPosition = position;
            ClampTargetPosition();

            if (!smoothZoom)
            {
                _currentPosition = _targetPosition;
                ApplyTransform();
            }
        }

        /// <summary>
        /// Resets target zoom to minZoom (1.0x) and centers position at (0, 0).
        /// </summary>
        public void ResetView()
        {
            _targetZoom = minZoom;
            _targetPosition = Vector2.zero;

            if (!smoothZoom)
            {
                _currentZoom = minZoom;
                _currentPosition = Vector2.zero;
                ApplyTransform();
            }
        }

        /// <summary>
        /// Directly applies current scale and position to folderRoot.
        /// </summary>
        public void ApplyTransform()
        {
            if (folderRoot != null)
            {
                folderRoot.localScale = new Vector3(_currentZoom, _currentZoom, 1f);
                folderRoot.anchoredPosition = _currentPosition;
            }
        }

        /// <summary>
        /// Clamps target position so the folder stays partially on screen by safetyMargin pixels.
        /// </summary>
        public void ClampTargetPosition()
        {
            _targetPosition = _service.ClampFolderPosition(_targetPosition, GetFolderSize(), _targetZoom, GetParentSize(), safetyMargin);
        }

        public Vector2 GetFolderSize()
        {
            if (folderRoot != null)
            {
                return folderRoot.rect.size;
            }
            return new Vector2(1400f, 780f);
        }

        public Vector2 GetParentSize()
        {
            RectTransform parent = GetParentRect();
            if (parent != null)
            {
                return parent.rect.size;
            }
            return new Vector2(Screen.width, Screen.height);
        }

        public RectTransform GetParentRect()
        {
            if (_parentCanvasRect != null) return _parentCanvasRect;

            if (folderRoot != null && folderRoot.parent is RectTransform p)
            {
                _parentCanvasRect = p;
                return _parentCanvasRect;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                _parentCanvasRect = canvas.GetComponent<RectTransform>();
                return _parentCanvasRect;
            }

            return transform as RectTransform;
        }

        /// <summary>
        /// Handles mouse scroll wheel events over the folder, smoothly zooming towards cursor position.
        /// </summary>
        public void OnScroll(PointerEventData eventData)
        {
            if (_isClosing || _slideCoroutine != null) return;
            if (Mathf.Abs(eventData.scrollDelta.y) < 0.001f) return;

            float newZoom = _service.CalculateNewZoom(_targetZoom, eventData.scrollDelta.y, minZoom, maxZoom, scrollSensitivity);
            if (Mathf.Abs(newZoom - _targetZoom) < 0.001f) return;

            RectTransform parentRect = GetParentRect();
            if (parentRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out Vector2 cursorLocalPoint))
            {
                _targetPosition = _service.CalculateZoomFocalPosition(_targetPosition, cursorLocalPoint, _targetZoom, newZoom);
            }

            _targetZoom = newZoom;
            ClampTargetPosition();

            if (!smoothZoom)
            {
                _currentZoom = _targetZoom;
                _currentPosition = _targetPosition;
                ApplyTransform();
            }
        }

        /// <summary>
        /// Handles beginning of drag operation on the suspect folder.
        /// </summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_isClosing || _slideCoroutine != null) return;
            if (eventData.button != PointerEventData.InputButton.Left) return;

            RectTransform parentRect = GetParentRect();
            if (parentRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out _lastDragLocalPos))
            {
                _isDragging = true;
            }
        }

        /// <summary>
        /// Handles continuous mouse drag, moving Folder_Root around on screen.
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;

            RectTransform parentRect = GetParentRect();
            if (parentRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out Vector2 currentLocalPos))
            {
                Vector2 delta = currentLocalPos - _lastDragLocalPos;
                _targetPosition += delta;
                ClampTargetPosition();
                _lastDragLocalPos = currentLocalPos;

                if (!smoothZoom)
                {
                    _currentPosition = _targetPosition;
                    ApplyTransform();
                }
            }
        }

        /// <summary>
        /// Handles end of drag operation.
        /// </summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
        }

        /// <summary>
        /// Handles pointer clicks:
        /// - Right-Click anywhere resets zoom (1.0x) and position (0, 0).
        /// - Left-Click directly on background outside folder dismisses the folder.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                ResetView();
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Left && !eventData.dragging)
            {
                GameObject clicked = eventData.pointerCurrentRaycast.gameObject;
                // Left-click on suspect folder or any child of folderRoot does NOTHING!
                if (clicked != null && folderRoot != null && (clicked == folderRoot.gameObject || clicked.transform.IsChildOf(folderRoot)))
                {
                    return;
                }

                if (clicked == gameObject || (backdropButton != null && clicked == backdropButton.gameObject))
                {
                    OnCloseClicked();
                }
            }
        }

        /// <summary>
        /// Dismisses the suspect folder with a smooth slide-down exit animation.
        /// </summary>
        public void OnCloseClicked()
        {
            if (_isClosing) return;
            _isClosing = true;
            _isDragging = false;
            Debug.Log("[UI:SuspectFolder] Close suspect folder requested, animating slide-out");

            if (folderRoot != null && gameObject.activeInHierarchy)
            {
                if (_slideCoroutine != null) StopCoroutine(_slideCoroutine);
                _slideCoroutine = StartCoroutine(SlideCoroutine(folderRoot.anchoredPosition.y, hiddenPosY, false));
            }
            else
            {
                _isClosing = false;
                UIManager.Instance?.ToggleSuspectFolderPanel();
            }
        }

        private IEnumerator SlideCoroutine(float fromY, float toY, bool isOpening)
        {
            if (folderRoot == null) yield break;

            float elapsed = 0f;
            Vector2 startPos = folderRoot.anchoredPosition;
            startPos.y = fromY;
            folderRoot.anchoredPosition = startPos;

            float startX = startPos.x;
            float startScale = folderRoot.localScale.x;

            while (elapsed < slideDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / slideDuration);
                float ease = 1f - Mathf.Pow(1f - t, 3f);

                Vector2 pos = folderRoot.anchoredPosition;
                pos.x = Mathf.Lerp(startX, 0f, ease);
                pos.y = Mathf.Lerp(fromY, toY, ease);
                folderRoot.anchoredPosition = pos;

                if (!isOpening)
                {
                    float s = Mathf.Lerp(startScale, 1f, ease);
                    folderRoot.localScale = new Vector3(s, s, 1f);
                }

                yield return null;
            }

            Vector2 finalPos = new Vector2(0f, toY);
            folderRoot.anchoredPosition = finalPos;
            _slideCoroutine = null;

            if (isOpening)
            {
                _currentPosition = finalPos;
                _targetPosition = finalPos;
                _currentZoom = 1.0f;
                _targetZoom = 1.0f;
                folderRoot.localScale = Vector3.one;
            }
            else
            {
                _isClosing = false;
                UIManager.Instance?.ToggleSuspectFolderPanel();
            }
        }

        private void TryAutoLoadSprites()
        {
#if UNITY_EDITOR
            if (case1FolderSprite == null)
            {
                case1FolderSprite = LoadFirstSpriteAtPath("Assets/Assets/SUSPECT FOLDERS/Case1. Vince & Jane.png");
            }
            if (case2FolderSprite == null)
            {
                case2FolderSprite = LoadFirstSpriteAtPath("Assets/Assets/SUSPECT FOLDERS/Case2. Paul & Vonn.png");
            }
            if (case3FolderSprite == null)
            {
                case3FolderSprite = LoadFirstSpriteAtPath("Assets/Assets/SUSPECT FOLDERS/Case3. Shania and Shan.png");
            }
#endif
        }

#if UNITY_EDITOR
        private static Sprite LoadFirstSpriteAtPath(string path)
        {
            var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
            if (assets != null)
            {
                foreach (var a in assets)
                {
                    if (a is Sprite s) return s;
                }
            }
            return null;
        }
#endif
    }
}
