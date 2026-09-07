using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;
using CaseClosed.Services;

namespace CaseClosed.UI
{
    /// <summary>
    /// UI View MonoBehaviour managing the detective's case file notebook / clipboard.
    /// Handles tab navigation, smooth slide transitions, right-edge tab pop-outs,
    /// and delegates text compilation to <see cref="NotebookFormattingService"/>.
    /// </summary>
    public class CaseFileNotebookUI : MonoBehaviour, IScrollHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [Header("Hierarchy & Animations")]
        [Tooltip("RectTransform of the centered clipboard to animate sliding in/out.")]
        public RectTransform clipboardRoot;
        [Tooltip("Full-screen transparent or dimmed backdrop button to dismiss when clicking outside.")]
        public Button backdropButton;
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

        [Header("Tab Buttons")]
        public Button summaryTabButton;
        public Button suspectsTabButton;
        public Button evidenceTabButton;
        public Button cluesTabButton;
        public Button closeNotebookButton;

        [Header("Content Container")]
        [Tooltip("ScrollRect wrapping the lined paper text content.")]
        public ScrollRect contentScrollRect;
        public Text notebookTitleText;
        public Text notebookContentBody;

        [Header("Visual Cards (Optional)")]
        [Tooltip("Card container displaying suspect polaroid portrait.")]
        public GameObject suspectCardSection;
        public Image suspectPortraitImage;
        public Text suspectNameLabel;

        [Tooltip("Card container displaying evidence item preview.")]
        public GameObject evidenceCardSection;
        public Image evidencePreviewImage;
        public Text evidenceNameLabel;

        [Header("Evidence Navigation & Locked State")]
        [Tooltip("Button to cycle to previous evidence item.")]
        public Button prevEvidenceButton;
        [Tooltip("Button to cycle to next evidence item.")]
        public Button nextEvidenceButton;
        [Tooltip("Label displaying evidence index/page, e.g. '[ 1 / 3 ]'.")]
        public Text evidenceIndexLabel;
        [Tooltip("Placeholder container shown when current evidence is locked / undiscovered.")]
        public GameObject evidenceLockedPlaceholder;
        [Tooltip("Text label on locked placeholder.")]
        public Text evidenceLockedLabel;

        private int currentEvidenceIndex = 0;
        /// <summary>Index of the currently focused evidence item within activeCase.evidenceItems.</summary>
        public int CurrentEvidenceIndex => currentEvidenceIndex;

        [Tooltip("Card container displaying case overview dossier card.")]
        public GameObject summaryCardSection;
        public Text summaryCaseTitleLabel;
        public Text summaryCaseMetaLabel;

        [Tooltip("Card container displaying clues & deduction status card.")]
        public GameObject cluesCardSection;
        public Text cluesCountLabel;

        [Header("Case Data (Optional Override)")]
        [Tooltip("Direct reference to the active case file, or leave empty to fetch from CaseManager.")]
        public CaseSO activeCaseData;
        /// <summary>Optional override for discovered evidence IDs (useful for isolated tests or static mock previews).</summary>
        public HashSet<string> evidenceOverride;
        /// <summary>Optional override for unlocked clues dictionary (useful for isolated tests or static mock previews).</summary>
        public Dictionary<string, string> cluesOverride;

        private NotebookTab currentTab = NotebookTab.CaseSummary;
        private readonly NotebookFormattingService formattingService = new NotebookFormattingService();
        private readonly NotebookZoomService _zoomService = new NotebookZoomService();
        private Coroutine slideCoroutine;
        private bool isClosing = false;
        private bool isSubscribed = false;

        // Zoom & Drag state
        private float _currentZoom = 1.0f;
        private float _targetZoom = 1.0f;
        private Vector2 _currentPosition = Vector2.zero;
        private Vector2 _targetPosition = Vector2.zero;
        private bool _isDragging = false;
        private Vector2 _lastDragLocalPos;
        private RectTransform _parentCanvasRect;

        public float CurrentZoom => _currentZoom;
        public float TargetZoom => _targetZoom;
        public Vector2 CurrentPosition => _currentPosition;
        public Vector2 TargetPosition => _targetPosition;
        public bool IsDragging => _isDragging;
        public bool IsClosing => isClosing;

        // Base X offsets for tab pop-out animation
        private float summaryBaseX = 0f;
        private float suspectsBaseX = 0f;
        private float evidenceBaseX = 0f;
        private float cluesBaseX = 0f;
        private bool basesCaptured = false;

        private bool buttonsConfigured = false;

        private void Awake()
        {
            SetupTabButtons();
            CaptureTabBasePositions();
            EnsureRaycastSetup();
        }

        private void Start()
        {
            SetupTabButtons();
            EnsureRaycastSetup();
            SubscribeToManagerEvents();
            SwitchTab(currentTab);
        }

        private void OnEnable()
        {
            isClosing = false;
            _isDragging = false;
            SetupTabButtons();
            EnsureRaycastSetup();
            CaptureTabBasePositions();
            SubscribeToManagerEvents();
            SwitchTab(currentTab);
            ResetView();

            if (clipboardRoot != null)
            {
                clipboardRoot.localScale = Vector3.one;
                Vector2 startPos = new Vector2(0f, hiddenPosY);
                clipboardRoot.anchoredPosition = startPos;
                _currentPosition = startPos;
                _targetPosition = Vector2.zero;

                if (slideCoroutine != null) StopCoroutine(slideCoroutine);
                slideCoroutine = StartCoroutine(SlideCoroutine(hiddenPosY, visiblePosY, true));
            }

            AudioManager.Instance?.PlayPaperFlip();
        }

        private void OnDisable()
        {
            if (slideCoroutine != null)
            {
                StopCoroutine(slideCoroutine);
                slideCoroutine = null;
            }
            _isDragging = false;
            UnsubscribeFromManagerEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromManagerEvents();
        }

        private void Update()
        {
            if (isClosing) return;

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
            if (slideCoroutine != null) return;
            if (clipboardRoot == null) return;

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

            clipboardRoot.localScale = new Vector3(_currentZoom, _currentZoom, 1f);
            clipboardRoot.anchoredPosition = _currentPosition;
        }

        public void SetupTabButtons()
        {
            if (buttonsConfigured) return;

            if (summaryTabButton != null)
            {
                summaryTabButton.onClick.RemoveListener(SelectSummaryTab);
                summaryTabButton.onClick.AddListener(SelectSummaryTab);
            }
            if (suspectsTabButton != null)
            {
                suspectsTabButton.onClick.RemoveListener(SelectSuspectsTab);
                suspectsTabButton.onClick.AddListener(SelectSuspectsTab);
            }
            if (evidenceTabButton != null)
            {
                evidenceTabButton.onClick.RemoveListener(SelectEvidenceTab);
                evidenceTabButton.onClick.AddListener(SelectEvidenceTab);
            }
            if (cluesTabButton != null)
            {
                cluesTabButton.onClick.RemoveListener(SelectCluesTab);
                cluesTabButton.onClick.AddListener(SelectCluesTab);
            }
            if (closeNotebookButton != null)
            {
                closeNotebookButton.onClick.RemoveListener(OnCloseClicked);
                closeNotebookButton.onClick.AddListener(OnCloseClicked);
            }
            if (backdropButton != null)
            {
                backdropButton.onClick.RemoveListener(OnCloseClicked);
                backdropButton.onClick.RemoveListener(OnBackdropClicked);
                backdropButton.onClick.AddListener(OnBackdropClicked);
            }
            if (prevEvidenceButton != null)
            {
                prevEvidenceButton.onClick.RemoveListener(PreviousEvidence);
                prevEvidenceButton.onClick.AddListener(PreviousEvidence);
            }
            if (nextEvidenceButton != null)
            {
                nextEvidenceButton.onClick.RemoveListener(NextEvidence);
                nextEvidenceButton.onClick.AddListener(NextEvidence);
            }

            UIButtonHighlightSystem.ApplyToHierarchy(gameObject);
            buttonsConfigured = true;
        }

        public void SelectSummaryTab() => SwitchTab(NotebookTab.CaseSummary);
        public void SelectSuspectsTab() => SwitchTab(NotebookTab.Suspects);
        public void SelectEvidenceTab() => SwitchTab(NotebookTab.Evidence);
        public void SelectCluesTab() => SwitchTab(NotebookTab.Clues);

        private void CaptureTabBasePositions()
        {
            if (basesCaptured) return;
            if (summaryTabButton != null) summaryBaseX = ((RectTransform)summaryTabButton.transform).anchoredPosition.x;
            if (suspectsTabButton != null) suspectsBaseX = ((RectTransform)suspectsTabButton.transform).anchoredPosition.x;
            if (evidenceTabButton != null) evidenceBaseX = ((RectTransform)evidenceTabButton.transform).anchoredPosition.x;
            if (cluesTabButton != null) cluesBaseX = ((RectTransform)cluesTabButton.transform).anchoredPosition.x;

            // If summary tab was already popped out at design time, normalize its base position
            if (summaryTabButton != null && suspectsTabButton != null && summaryBaseX > suspectsBaseX + 10f)
            {
                summaryBaseX = suspectsBaseX;
            }

            basesCaptured = true;
        }

        private void SubscribeToManagerEvents()
        {
            if (isSubscribed || CaseManager.Instance == null) return;
            CaseManager.Instance.OnCaseLoaded += HandleCaseStateChanged;
            CaseManager.Instance.OnEvidenceDiscovered += HandleEvidenceDiscovered;
            CaseManager.Instance.OnClueUnlocked += HandleClueUnlocked;
            isSubscribed = true;
        }

        private void UnsubscribeFromManagerEvents()
        {
            if (!isSubscribed || CaseManager.Instance == null) return;
            CaseManager.Instance.OnCaseLoaded -= HandleCaseStateChanged;
            CaseManager.Instance.OnEvidenceDiscovered -= HandleEvidenceDiscovered;
            CaseManager.Instance.OnClueUnlocked -= HandleClueUnlocked;
            isSubscribed = false;
        }

        private void HandleCaseStateChanged(CaseSO c)
        {
            currentEvidenceIndex = 0;
            SwitchTab(currentTab);
        }

        private void HandleEvidenceDiscovered(EvidenceSO e)
        {
            if (e != null)
            {
                CaseSO activeCase = GetActiveCase();
                if (activeCase != null && activeCase.evidenceItems != null)
                {
                    for (int i = 0; i < activeCase.evidenceItems.Count; i++)
                    {
                        if (activeCase.evidenceItems[i] != null && activeCase.evidenceItems[i].id == e.id)
                        {
                            currentEvidenceIndex = i;
                            break;
                        }
                    }
                }
            }
            SwitchTab(currentTab);
        }
        private void HandleClueUnlocked(string k, string v) => SwitchTab(currentTab);

        private CaseSO GetActiveCase()
        {
            if (activeCaseData != null) return activeCaseData;
            if (CaseManager.Instance != null && CaseManager.Instance.ActiveCase != null)
                return CaseManager.Instance.ActiveCase;
            var mgr = FindFirstObjectByType<CaseManager>();
            if (mgr != null && mgr.ActiveCase != null)
                return mgr.ActiveCase;
            return null;
        }

        private bool IsEvidenceDiscovered(string evidenceId)
        {
            if (evidenceOverride != null) return evidenceOverride.Contains(evidenceId);
            if (CaseManager.Instance != null) return CaseManager.Instance.IsEvidenceDiscovered(evidenceId);
            var mgr = FindFirstObjectByType<CaseManager>();
            return mgr != null && mgr.IsEvidenceDiscovered(evidenceId);
        }

        private IReadOnlyDictionary<string, string> GetUnlockedClues()
        {
            if (cluesOverride != null) return cluesOverride;
            if (CaseManager.Instance != null) return CaseManager.Instance.UnlockedCluesText;
            var mgr = FindFirstObjectByType<CaseManager>();
            return mgr != null ? mgr.UnlockedCluesText : null;
        }

        private CaseManager GetSessionManager(CaseSO caseData)
        {
            CaseManager manager = CaseManager.Instance;
            if (manager != null && manager.ActiveCase == caseData) return manager;

            manager = FindFirstObjectByType<CaseManager>();
            return manager != null && manager.ActiveCase == caseData ? manager : null;
        }

        private CharacterProfileSO GetEffectiveInvestigator(CaseSO caseData)
        {
            CaseManager manager = GetSessionManager(caseData);
            return manager != null ? manager.EffectiveInvestigator : caseData?.leadInvestigator;
        }

        /// <summary>
        /// Switches the active notebook tab, formats case data, and updates tab highlights.
        /// </summary>
        /// <param name="tab">The target notebook tab to display.</param>
        /// <param name="smartFocus">Whether to automatically focus the first discovered evidence if currently focused is undiscovered.</param>
        public void SwitchTab(NotebookTab tab, bool smartFocus = true)
        {
            currentTab = tab;
            UpdateTabVisualStates();

            if (contentScrollRect != null)
            {
                contentScrollRect.verticalNormalizedPosition = 1f;
            }

            CaseSO activeCase = GetActiveCase();
            if (activeCase == null)
            {
                if (notebookTitleText != null) notebookTitleText.text = "CASE FILE NOTEBOOK";
                if (notebookContentBody != null) notebookContentBody.text = "<i>No active case file loaded.\nPlease select an active case from the Case Files menu.</i>";
                HideAllCardSections();
                return;
            }

            Debug.Log($"[UI:Notebook] Switched to tab '{tab}' for case '{activeCase.caseTitle}'");

            string contentText = string.Empty;
            HideAllCardSections();

            switch (tab)
            {
                case NotebookTab.CaseSummary:
                    if (notebookTitleText != null) notebookTitleText.text = activeCase.caseTitle;
                    contentText = formattingService.FormatCaseSummary(activeCase, GetEffectiveInvestigator(activeCase));
                    SetupSummaryCard(activeCase);
                    break;

                case NotebookTab.Suspects:
                    if (notebookTitleText != null) notebookTitleText.text = "SUSPECT DOSSIER";
                    contentText = formattingService.FormatSuspectProfiles(activeCase);
                    SetupSuspectCard(activeCase);
                    break;

                case NotebookTab.Evidence:
                    if (notebookTitleText != null) notebookTitleText.text = "EVIDENCE REPOSITORY";
                    if (smartFocus && activeCase.evidenceItems != null && activeCase.evidenceItems.Count > 0)
                    {
                        bool isCurrentDiscovered = currentEvidenceIndex >= 0 &&
                                                   currentEvidenceIndex < activeCase.evidenceItems.Count &&
                                                   activeCase.evidenceItems[currentEvidenceIndex] != null &&
                                                   IsEvidenceDiscovered(activeCase.evidenceItems[currentEvidenceIndex].id);

                        if (!isCurrentDiscovered)
                        {
                            int firstDiscoveredIndex = -1;
                            for (int i = 0; i < activeCase.evidenceItems.Count; i++)
                            {
                                var item = activeCase.evidenceItems[i];
                                if (item != null && IsEvidenceDiscovered(item.id))
                                {
                                    firstDiscoveredIndex = i;
                                    break;
                                }
                            }

                            if (firstDiscoveredIndex >= 0)
                            {
                                currentEvidenceIndex = firstDiscoveredIndex;
                            }
                            else
                            {
                                currentEvidenceIndex = 0;
                            }
                        }
                    }

                    contentText = SetupEvidenceCard(activeCase);
                    break;

                case NotebookTab.Clues:
                    if (notebookTitleText != null) notebookTitleText.text = "DEDUCTION JOURNAL";
                    var cluesDict = GetUnlockedClues();
                    contentText = formattingService.FormatUnlockedClues(cluesDict);
                    SetupCluesCard(activeCase, cluesDict);
                    break;
            }

            if (notebookContentBody != null)
            {
                notebookContentBody.supportRichText = true;
                notebookContentBody.text = contentText;
            }

            AudioManager.Instance?.PlayPaperFlip();
        }

        private void SetupSummaryCard(CaseSO activeCase)
        {
            if (summaryCardSection == null) return;
            summaryCardSection.SetActive(true);
            if (summaryCaseTitleLabel != null)
            {
                summaryCaseTitleLabel.text = activeCase.caseTitle;
            }
            if (summaryCaseMetaLabel != null)
            {
                CharacterProfileSO investigator = GetEffectiveInvestigator(activeCase);
                string lead = investigator != null ? investigator.fullName : "Detective Bureau";
                summaryCaseMetaLabel.text = $"<b>CASE #{activeCase.levelNumber}</b>\n\n<b>Lead:</b> {lead}\n<b>Location:</b> {activeCase.dateAndLocation}\n<b>Victim:</b> {activeCase.victimInfo}\n\n<b>STATUS:</b> <color=#166534><b>ACTIVE FILE</b></color>";
            }
        }

        private void SetupSuspectCard(CaseSO activeCase)
        {
            if (suspectCardSection == null) return;

            if (activeCase.primarySuspect != null && activeCase.primarySuspect.defaultSittingPose != null)
            {
                suspectCardSection.SetActive(true);
                if (suspectPortraitImage != null)
                {
                    suspectPortraitImage.sprite = activeCase.primarySuspect.defaultSittingPose;
                    suspectPortraitImage.preserveAspect = true;
                }
                if (suspectNameLabel != null)
                {
                    suspectNameLabel.text = activeCase.primarySuspect.fullName;
                }
            }
            else
            {
                suspectCardSection.SetActive(false);
            }
        }

        /// <summary>
        /// Cycles to the next evidence item in the active case file.
        /// </summary>
        public void NextEvidence()
        {
            CaseSO activeCase = GetActiveCase();
            if (activeCase == null || activeCase.evidenceItems == null || activeCase.evidenceItems.Count <= 1) return;
            currentEvidenceIndex = (currentEvidenceIndex + 1) % activeCase.evidenceItems.Count;
            AudioManager.Instance?.PlayPaperFlip();
            string text = SetupEvidenceCard(activeCase);
            if (notebookContentBody != null)
            {
                notebookContentBody.supportRichText = true;
                notebookContentBody.text = text;
            }
        }

        /// <summary>
        /// Cycles to the previous evidence item in the active case file.
        /// </summary>
        public void PreviousEvidence()
        {
            CaseSO activeCase = GetActiveCase();
            if (activeCase == null || activeCase.evidenceItems == null || activeCase.evidenceItems.Count <= 1) return;
            currentEvidenceIndex = (currentEvidenceIndex - 1 + activeCase.evidenceItems.Count) % activeCase.evidenceItems.Count;
            AudioManager.Instance?.PlayPaperFlip();
            string text = SetupEvidenceCard(activeCase);
            if (notebookContentBody != null)
            {
                notebookContentBody.supportRichText = true;
                notebookContentBody.text = text;
            }
        }

        /// <summary>
        /// Focuses a specific evidence item by ID and switches to the Evidence Repository tab.
        /// </summary>
        /// <param name="evidenceId">The unique ID of the evidence item.</param>
        public void FocusEvidence(string evidenceId)
        {
            CaseSO activeCase = GetActiveCase();
            if (activeCase != null && activeCase.evidenceItems != null)
            {
                for (int i = 0; i < activeCase.evidenceItems.Count; i++)
                {
                    if (activeCase.evidenceItems[i] != null && activeCase.evidenceItems[i].id == evidenceId)
                    {
                        currentEvidenceIndex = i;
                        break;
                    }
                }
            }
            SwitchTab(NotebookTab.Evidence, smartFocus: false);
        }

        private string SetupEvidenceCard(CaseSO activeCase)
        {
            if (evidenceCardSection == null) return string.Empty;

            EnsureEvidenceControlsExist();

            if (activeCase == null || activeCase.evidenceItems == null || activeCase.evidenceItems.Count == 0)
            {
                evidenceCardSection.SetActive(false);
                return "<i>No physical evidence defined for this case file.</i>";
            }

            evidenceCardSection.SetActive(true);
            int count = activeCase.evidenceItems.Count;
            currentEvidenceIndex = Mathf.Clamp(currentEvidenceIndex, 0, count - 1);
            EvidenceSO currentEv = activeCase.evidenceItems[currentEvidenceIndex];

            bool isDiscovered = currentEv != null && IsEvidenceDiscovered(currentEv.id);

            // Update Page Indicator
            if (evidenceIndexLabel != null)
            {
                evidenceIndexLabel.text = $"[ {currentEvidenceIndex + 1} / {count} ]";
            }

            // Update Navigation Buttons
            if (prevEvidenceButton != null)
            {
                prevEvidenceButton.interactable = count > 1;
            }
            if (nextEvidenceButton != null)
            {
                nextEvidenceButton.interactable = count > 1;
            }

            if (isDiscovered && currentEv != null)
            {
                if (evidenceLockedPlaceholder != null)
                {
                    evidenceLockedPlaceholder.SetActive(false);
                }

                if (evidencePreviewImage != null)
                {
                    evidencePreviewImage.gameObject.SetActive(true);
                    evidencePreviewImage.sprite = currentEv.GetTopPovSprite();
                    evidencePreviewImage.preserveAspect = true;
                }

                if (evidenceNameLabel != null)
                {
                    evidenceNameLabel.text = currentEv.evidenceName;
                }
            }
            else
            {
                if (evidencePreviewImage != null)
                {
                    evidencePreviewImage.gameObject.SetActive(false);
                }

                if (evidenceLockedPlaceholder != null)
                {
                    evidenceLockedPlaceholder.SetActive(true);
                    if (evidenceLockedLabel != null)
                    {
                        evidenceLockedLabel.text = "[ EVIDENCE LOCKED ]";
                    }
                }

                if (evidenceNameLabel != null)
                {
                    evidenceNameLabel.text = "[ ??? LOCKED EVIDENCE ]";
                }
            }

            CaseManager manager = GetSessionManager(activeCase);
            bool isExamined = manager != null
                ? manager.IsEvidenceExamined(currentEv)
                : currentEv != null && currentEv.isExamined;
            int discoveredHotspotCount = manager != null ? manager.GetDiscoveredHotspotCount(currentEv) : 0;
            string dossierText = formattingService.FormatEvidenceDossier(
                currentEv,
                isDiscovered,
                isExamined,
                discoveredHotspotCount,
                currentEvidenceIndex + 1,
                count);
            return dossierText;
        }

        private void EnsureEvidenceControlsExist()
        {
            if (evidenceCardSection == null) return;

            Transform cardTransform = evidenceCardSection.transform;

            // Try resolving serialized references from existing hierarchy if unassigned
            if (prevEvidenceButton == null)
            {
                var prevTransform = cardTransform.Find("Evidence_Nav_Row/Button_Prev") ?? cardTransform.Find("Button_Prev");
                if (prevTransform != null) prevEvidenceButton = prevTransform.GetComponent<Button>();
            }

            if (nextEvidenceButton == null)
            {
                var nextTransform = cardTransform.Find("Evidence_Nav_Row/Button_Next") ?? cardTransform.Find("Button_Next");
                if (nextTransform != null) nextEvidenceButton = nextTransform.GetComponent<Button>();
            }

            if (evidenceIndexLabel == null)
            {
                var indexTransform = cardTransform.Find("Evidence_Nav_Row/Text_Index") ?? cardTransform.Find("Text_Index");
                if (indexTransform != null) evidenceIndexLabel = indexTransform.GetComponent<Text>();
            }

            if (evidenceLockedPlaceholder == null)
            {
                var lockTransform = cardTransform.Find("Locked_Placeholder") ?? cardTransform.Find("Evidence_Locked_Placeholder");
                if (lockTransform != null)
                {
                    evidenceLockedPlaceholder = lockTransform.gameObject;
                    if (evidenceLockedLabel == null)
                    {
                        evidenceLockedLabel = lockTransform.GetComponentInChildren<Text>();
                    }
                }
            }
            else if (evidenceLockedLabel == null)
            {
                evidenceLockedLabel = evidenceLockedPlaceholder.GetComponentInChildren<Text>();
            }

            // Dynamically construct navigation row if missing from hierarchy
            if (prevEvidenceButton == null || nextEvidenceButton == null || evidenceIndexLabel == null)
            {
                Transform existingNav = cardTransform.Find("Evidence_Nav_Row");
                GameObject navRowGO = existingNav != null ? existingNav.gameObject : new GameObject("Evidence_Nav_Row", typeof(RectTransform));
                if (existingNav == null)
                {
                    navRowGO.transform.SetParent(cardTransform, false);
                    RectTransform navRowRT = navRowGO.GetComponent<RectTransform>();
                    navRowRT.anchorMin = new Vector2(0.08f, 0.15f);
                    navRowRT.anchorMax = new Vector2(0.92f, 0.23f);
                    navRowRT.offsetMin = Vector2.zero;
                    navRowRT.offsetMax = Vector2.zero;
                }

                // Button Prev (<)
                if (prevEvidenceButton == null)
                {
                    GameObject prevBtnGO = new GameObject("Button_Prev", typeof(RectTransform), typeof(Image), typeof(Button));
                    prevBtnGO.transform.SetParent(navRowGO.transform, false);
                    RectTransform prevRT = prevBtnGO.GetComponent<RectTransform>();
                    prevRT.anchorMin = new Vector2(0f, 0f);
                    prevRT.anchorMax = new Vector2(0.25f, 1f);
                    prevRT.offsetMin = Vector2.zero;
                    prevRT.offsetMax = Vector2.zero;

                    Image prevImg = prevBtnGO.GetComponent<Image>();
                    prevImg.color = new Color(0.18f, 0.20f, 0.25f, 0.9f);

                    GameObject prevTextGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
                    prevTextGO.transform.SetParent(prevBtnGO.transform, false);
                    Text prevText = prevTextGO.GetComponent<Text>();
                    prevText.text = "<";
                    prevText.fontSize = 18;
                    prevText.fontStyle = FontStyle.Bold;
                    prevText.alignment = TextAnchor.MiddleCenter;
                    prevText.color = Color.white;
                    RectTransform prevTextRT = prevTextGO.GetComponent<RectTransform>();
                    prevTextRT.anchorMin = Vector2.zero;
                    prevTextRT.anchorMax = Vector2.one;
                    prevTextRT.offsetMin = Vector2.zero;
                    prevTextRT.offsetMax = Vector2.zero;

                    prevEvidenceButton = prevBtnGO.GetComponent<Button>();
                }

                // Text Index ([ 1 / 3 ])
                if (evidenceIndexLabel == null)
                {
                    GameObject indexGO = new GameObject("Text_Index", typeof(RectTransform), typeof(Text));
                    indexGO.transform.SetParent(navRowGO.transform, false);
                    RectTransform indexRT = indexGO.GetComponent<RectTransform>();
                    indexRT.anchorMin = new Vector2(0.28f, 0f);
                    indexRT.anchorMax = new Vector2(0.72f, 1f);
                    indexRT.offsetMin = Vector2.zero;
                    indexRT.offsetMax = Vector2.zero;

                    evidenceIndexLabel = indexGO.GetComponent<Text>();
                    evidenceIndexLabel.text = "[ 1 / 1 ]";
                    evidenceIndexLabel.fontSize = 15;
                    evidenceIndexLabel.fontStyle = FontStyle.Bold;
                    evidenceIndexLabel.alignment = TextAnchor.MiddleCenter;
                    evidenceIndexLabel.color = new Color(0.15f, 0.15f, 0.18f, 1f);
                }

                // Button Next (>)
                if (nextEvidenceButton == null)
                {
                    GameObject nextBtnGO = new GameObject("Button_Next", typeof(RectTransform), typeof(Image), typeof(Button));
                    nextBtnGO.transform.SetParent(navRowGO.transform, false);
                    RectTransform nextRT = nextBtnGO.GetComponent<RectTransform>();
                    nextRT.anchorMin = new Vector2(0.75f, 0f);
                    nextRT.anchorMax = new Vector2(1f, 1f);
                    nextRT.offsetMin = Vector2.zero;
                    nextRT.offsetMax = Vector2.zero;

                    Image nextImg = nextBtnGO.GetComponent<Image>();
                    nextImg.color = new Color(0.18f, 0.20f, 0.25f, 0.9f);

                    GameObject nextTextGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
                    nextTextGO.transform.SetParent(nextBtnGO.transform, false);
                    Text nextText = nextTextGO.GetComponent<Text>();
                    nextText.text = ">";
                    nextText.fontSize = 18;
                    nextText.fontStyle = FontStyle.Bold;
                    nextText.alignment = TextAnchor.MiddleCenter;
                    nextText.color = Color.white;
                    RectTransform nextTextRT = nextTextGO.GetComponent<RectTransform>();
                    nextTextRT.anchorMin = Vector2.zero;
                    nextTextRT.anchorMax = Vector2.one;
                    nextTextRT.offsetMin = Vector2.zero;
                    nextTextRT.offsetMax = Vector2.zero;

                    nextEvidenceButton = nextBtnGO.GetComponent<Button>();
                }
            }

            // Construct locked placeholder if missing
            if (evidenceLockedPlaceholder == null)
            {
                Transform existingLocked = cardTransform.Find("Locked_Placeholder");
                GameObject lockedGO = existingLocked != null ? existingLocked.gameObject : new GameObject("Locked_Placeholder", typeof(RectTransform), typeof(Image));
                if (existingLocked == null)
                {
                    lockedGO.transform.SetParent(cardTransform, false);
                    RectTransform lockedRT = lockedGO.GetComponent<RectTransform>();
                    lockedRT.anchorMin = new Vector2(0.08f, 0.26f);
                    lockedRT.anchorMax = new Vector2(0.92f, 0.98f);
                    lockedRT.offsetMin = Vector2.zero;
                    lockedRT.offsetMax = Vector2.zero;

                    Image lockedImg = lockedGO.GetComponent<Image>();
                    lockedImg.color = new Color(0.12f, 0.13f, 0.16f, 0.95f);

                    GameObject lockedTextGO = new GameObject("Text_LockedMessage", typeof(RectTransform), typeof(Text));
                    lockedTextGO.transform.SetParent(lockedGO.transform, false);
                    RectTransform textRT = lockedTextGO.GetComponent<RectTransform>();
                    textRT.anchorMin = Vector2.zero;
                    textRT.anchorMax = Vector2.one;
                    textRT.offsetMin = new Vector2(10, 10);
                    textRT.offsetMax = new Vector2(-10, -10);

                    evidenceLockedLabel = lockedTextGO.GetComponent<Text>();
                    evidenceLockedLabel.text = "<b>[ EVIDENCE LOCKED ]</b>\n\n<size=13><color=#94A3B8>Keep investigating the crime scene and interrogating suspects to uncover this evidence.</color></size>";
                    evidenceLockedLabel.fontSize = 15;
                    evidenceLockedLabel.fontStyle = FontStyle.Bold;
                    evidenceLockedLabel.alignment = TextAnchor.MiddleCenter;
                    evidenceLockedLabel.color = new Color(0.95f, 0.75f, 0.25f, 1f);
                    evidenceLockedLabel.supportRichText = true;
                }

                evidenceLockedPlaceholder = lockedGO;
                evidenceLockedPlaceholder.SetActive(false);
            }

            // Bind click listeners
            if (prevEvidenceButton != null)
            {
                prevEvidenceButton.onClick.RemoveListener(PreviousEvidence);
                prevEvidenceButton.onClick.AddListener(PreviousEvidence);
            }
            if (nextEvidenceButton != null)
            {
                nextEvidenceButton.onClick.RemoveListener(NextEvidence);
                nextEvidenceButton.onClick.AddListener(NextEvidence);
            }
        }

        private void SetupCluesCard(CaseSO activeCase, IReadOnlyDictionary<string, string> cluesDict)
        {
            if (cluesCardSection == null) return;
            cluesCardSection.SetActive(true);
            int count = cluesDict != null ? cluesDict.Count : 0;
            if (cluesCountLabel != null)
            {
                cluesCountLabel.text = $"<b>DEDUCTION JOURNAL</b>\n\n<b>Unlocked Clues:</b> {count}\n\n<i>Cross-reference suspect testimonies and gathered physical evidence to uncover critical contradictions.</i>";
            }
        }

        private void HideAllCardSections()
        {
            if (suspectCardSection != null) suspectCardSection.SetActive(false);
            if (evidenceCardSection != null) evidenceCardSection.SetActive(false);
            if (summaryCardSection != null) summaryCardSection.SetActive(false);
            if (cluesCardSection != null) cluesCardSection.SetActive(false);
        }

        /// <summary>
        /// Updates tab button visuals (offset pop-out and brightness tint) to reflect current selection.
        /// </summary>
        private void UpdateTabVisualStates()
        {
            ApplyTabState(summaryTabButton, summaryBaseX, currentTab == NotebookTab.CaseSummary);
            ApplyTabState(suspectsTabButton, suspectsBaseX, currentTab == NotebookTab.Suspects);
            ApplyTabState(evidenceTabButton, evidenceBaseX, currentTab == NotebookTab.Evidence);
            ApplyTabState(cluesTabButton, cluesBaseX, currentTab == NotebookTab.Clues);
        }

        private void ApplyTabState(Button button, float baseX, bool isSelected)
        {
            if (button == null) return;
            RectTransform rt = (RectTransform)button.transform;
            Vector2 pos = rt.anchoredPosition;
            pos.x = isSelected ? baseX + 20f : baseX;
            rt.anchoredPosition = pos;

            Image img = button.GetComponent<Image>();
            if (img != null)
            {
                img.color = isSelected ? Color.white : new Color(0.82f, 0.82f, 0.82f, 0.92f);
            }
        }

        /// <summary>
        /// Handles close action with a smooth slide-down exit animation.
        /// </summary>
        public void OnCloseClicked()
        {
            if (isClosing) return;
            isClosing = true;
            _isDragging = false;
            Debug.Log("[UI:Notebook] Close notebook requested, animating slide-out");

            if (clipboardRoot != null && gameObject.activeInHierarchy)
            {
                if (slideCoroutine != null) StopCoroutine(slideCoroutine);
                slideCoroutine = StartCoroutine(SlideCoroutine(clipboardRoot.anchoredPosition.y, hiddenPosY, false));
            }
            else
            {
                isClosing = false;
                UIManager.Instance?.ToggleNotebookPanel();
            }
        }

        private IEnumerator SlideCoroutine(float fromY, float toY, bool isOpening)
        {
            if (clipboardRoot == null) yield break;

            float elapsed = 0f;
            Vector2 startPos = clipboardRoot.anchoredPosition;
            startPos.y = fromY;
            clipboardRoot.anchoredPosition = startPos;

            float startX = startPos.x;
            float startScale = clipboardRoot.localScale.x;

            while (elapsed < slideDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / slideDuration);
                float ease = 1f - Mathf.Pow(1f - t, 3f);

                Vector2 pos = clipboardRoot.anchoredPosition;
                pos.x = Mathf.Lerp(startX, 0f, ease);
                pos.y = Mathf.Lerp(fromY, toY, ease);
                clipboardRoot.anchoredPosition = pos;

                if (!isOpening)
                {
                    float s = Mathf.Lerp(startScale, 1f, ease);
                    clipboardRoot.localScale = new Vector3(s, s, 1f);
                }

                yield return null;
            }

            Vector2 finalPos = new Vector2(0f, toY);
            clipboardRoot.anchoredPosition = finalPos;
            slideCoroutine = null;

            if (isOpening)
            {
                _currentPosition = finalPos;
                _targetPosition = finalPos;
                _currentZoom = 1.0f;
                _targetZoom = 1.0f;
                clipboardRoot.localScale = Vector3.one;
            }
            else
            {
                isClosing = false;
                UIManager.Instance?.ToggleNotebookPanel();
            }
        }

        #region Zoom, Pan & Drag Event Handlers

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
        /// Directly applies current scale and position to clipboardRoot.
        /// </summary>
        public void ApplyTransform()
        {
            if (clipboardRoot != null)
            {
                clipboardRoot.localScale = new Vector3(_currentZoom, _currentZoom, 1f);
                clipboardRoot.anchoredPosition = _currentPosition;
            }
        }

        /// <summary>
        /// Clamps target position so the notebook stays partially on screen by safetyMargin pixels.
        /// </summary>
        public void ClampTargetPosition()
        {
            _targetPosition = _zoomService.ClampNotebookPosition(_targetPosition, GetNotebookSize(), _targetZoom, GetParentSize(), safetyMargin);
        }

        public Vector2 GetNotebookSize()
        {
            if (clipboardRoot != null)
            {
                return clipboardRoot.rect.size;
            }
            return new Vector2(1540f, 860f);
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

            if (clipboardRoot != null && clipboardRoot.parent is RectTransform p)
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
        /// Checks whether the mouse cursor is currently positioned over clipboardRoot or any of its children.
        /// </summary>
        public bool IsPointerOverClipboard()
        {
            if (clipboardRoot == null) return false;

            if (EventSystem.current != null)
            {
                var pointerData = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };
                var results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);
                foreach (var result in results)
                {
                    if (result.gameObject == null) continue;
                    if (result.gameObject.transform.IsChildOf(clipboardRoot) || result.gameObject == clipboardRoot.gameObject)
                    {
                        return true;
                    }
                }
            }

            Camera cam = GetComponentInParent<Canvas>()?.worldCamera;
            return RectTransformUtility.RectangleContainsScreenPoint(clipboardRoot, Input.mousePosition, cam);
        }

        /// <summary>
        /// Handles clicks on the backdrop button. If the pointer was over the clipboard, the click is ignored.
        /// </summary>
        public void OnBackdropClicked()
        {
            if (IsPointerOverClipboard())
            {
                return;
            }
            OnCloseClicked();
        }

        public void EnsureRaycastSetup()
        {
            if (clipboardRoot != null)
            {
                var img = clipboardRoot.GetComponent<Image>();
                if (img != null)
                {
                    img.raycastTarget = true;
                }

                var rootTarget = clipboardRoot.GetComponent<CaseFileNotebookDragTarget>();
                if (rootTarget == null)
                {
                    rootTarget = clipboardRoot.gameObject.AddComponent<CaseFileNotebookDragTarget>();
                }
                rootTarget.Init(this);
            }

            if (contentScrollRect != null)
            {
                contentScrollRect.scrollSensitivity = 0f;
            }
        }

        /// <summary>
        /// Handles mouse scroll wheel events over the notebook, smoothly zooming towards cursor position.
        /// </summary>
        public void OnScroll(PointerEventData eventData)
        {
            if (isClosing || slideCoroutine != null) return;
            if (Mathf.Abs(eventData.scrollDelta.y) < 0.001f) return;

            float newZoom = _zoomService.CalculateNewZoom(_targetZoom, eventData.scrollDelta.y, minZoom, maxZoom, scrollSensitivity);
            if (Mathf.Abs(newZoom - _targetZoom) < 0.001f) return;

            RectTransform parentRect = GetParentRect();
            if (parentRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out Vector2 cursorLocalPoint))
            {
                _targetPosition = _zoomService.CalculateZoomFocalPosition(_targetPosition, cursorLocalPoint, _targetZoom, newZoom);
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
        /// Handles beginning of drag operation on the case file notebook.
        /// </summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (isClosing || slideCoroutine != null) return;
            if (eventData.button != PointerEventData.InputButton.Left) return;

            RectTransform parentRect = GetParentRect();
            if (parentRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out _lastDragLocalPos))
            {
                _isDragging = true;
            }
        }

        /// <summary>
        /// Handles continuous mouse drag, moving clipboardRoot around on screen.
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
        /// - Left-Click directly on background outside clipboard dismisses the notebook.
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
                // Left-click on clipboard or any child of clipboardRoot does NOTHING!
                if (clicked != null && clipboardRoot != null && (clicked == clipboardRoot.gameObject || clicked.transform.IsChildOf(clipboardRoot)))
                {
                    return;
                }

                if (clicked == gameObject || (backdropButton != null && clicked == backdropButton.gameObject))
                {
                    OnCloseClicked();
                }
            }
        }

        #endregion
    }
}
