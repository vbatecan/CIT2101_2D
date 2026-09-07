using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;
using CaseClosed.Services;

namespace CaseClosed.UI
{
    /// <summary>
    /// UI View MonoBehaviour managing the detective's suspect dossiers folder modal.
    /// Animates smooth slide transitions and displays the active case's suspect file
    /// from Assets/Assets/SUSPECT FOLDERS.
    /// </summary>
    public class SuspectFolderUI : MonoBehaviour
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

        public int CurrentCaseIndex => _currentCaseIndex;
        public bool IsClosing => _isClosing;

        private void Awake()
        {
            SetupButtons();
            TryAutoLoadSprites();
        }

        private void Start()
        {
            SetupButtons();
        }

        private void OnEnable()
        {
            _isClosing = false;
            SetupButtons();
            RefreshDossierView();

            if (folderRoot != null)
            {
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
                backdropButton.onClick.AddListener(OnCloseClicked);
            }

            UIButtonHighlightSystem.ApplyToHierarchy(gameObject);
            _buttonsConfigured = true;
        }

        /// <summary>
        /// Updates the folder display image and title according to the active case.
        /// </summary>
        public void RefreshDossierView()
        {
            CaseSO activeCase = activeCaseOverride;
            if (activeCase == null && CaseManager.Instance != null)
            {
                activeCase = CaseManager.Instance.activeCase;
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
        /// Dismisses the suspect folder with a smooth slide-down exit animation.
        /// </summary>
        public void OnCloseClicked()
        {
            if (_isClosing) return;
            _isClosing = true;
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
            Vector2 pos = folderRoot.anchoredPosition;
            pos.y = fromY;
            folderRoot.anchoredPosition = pos;

            while (elapsed < slideDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / slideDuration);
                float ease = 1f - Mathf.Pow(1f - t, 3f);
                pos.y = Mathf.Lerp(fromY, toY, ease);
                folderRoot.anchoredPosition = pos;
                yield return null;
            }

            pos.y = toY;
            folderRoot.anchoredPosition = pos;
            _slideCoroutine = null;

            if (!isOpening)
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
