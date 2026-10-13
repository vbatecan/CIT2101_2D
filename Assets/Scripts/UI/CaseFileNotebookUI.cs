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

        [Header("Suspect Navigation")]
        [Tooltip("Button to cycle to previous suspect.")]
        public Button prevSuspectButton;
        [Tooltip("Button to cycle to next suspect.")]
        public Button nextSuspectButton;
        [Tooltip("Label displaying suspect index/page, e.g. '[ 1 / 2 ]'.")]
        public Text suspectIndexLabel;

        private int currentSuspectIndex = 0;
        public int CurrentSuspectIndex => currentSuspectIndex;

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
        public HashSet<string> evidenceOverride;
        public Dictionary<string, string> cluesOverride;

        private NotebookTab currentTab = NotebookTab.CaseSummary;
        private readonly NotebookFormattingService formattingService = new NotebookFormattingService();
        private readonly NotebookZoomService _zoomService = new NotebookZoomService();
        private Coroutine slideCoroutine;
        private bool isClosing = false;
        private bool isSubscribed = false;

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

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnCloseClicked();
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetView();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) SelectSummaryTab();
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) SelectSuspectsTab();
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) SelectEvidenceTab();
            else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) SelectCluesTab();
            else if (Input.GetKeyDown(KeyCode.Tab)) CycleNextTab();

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                if (currentTab == NotebookTab.Evidence) PreviousEvidence();
                else if (currentTab == NotebookTab.Suspects) PreviousSuspect();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                if (currentTab == NotebookTab.Evidence) NextEvidence();
                else if (currentTab == NotebookTab.Suspects) NextSuspect();
            }

            if (slideCoroutine != null) return;
            if (clipboardRoot == null) return;

            if (Mathf.Abs(_currentZoom - _targetZoom) > 0.0005f || (_currentPosition - _targetPosition).sqrMagnitude > 0.001f)
            {
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
            else if (_currentZoom != _targetZoom || _currentPosition != _targetPosition)
            {
                _currentZoom = _targetZoom;
                _currentPosition = _targetPosition;
                clipboardRoot.localScale = new Vector3(_currentZoom, _currentZoom, 1f);
                clipboardRoot.anchoredPosition = _currentPosition;
            }
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
            currentSuspectIndex = 0;
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

                int totalEvidence = activeCase.evidenceItems != null ? activeCase.evidenceItems.Count : 0;
                int discoveredEvidence = 0;
                if (activeCase.evidenceItems != null)
                {
                    foreach (var ev in activeCase.evidenceItems)
                    {
                        if (ev != null && IsEvidenceDiscovered(ev.id)) discoveredEvidence++;
                    }
                }

                var clues = GetUnlockedClues();
                int clueCount = clues != null ? clues.Count : 0;

                summaryCaseMetaLabel.text = $"<b>CASE #{activeCase.levelNumber:00}</b>\n\n" +
                    $"<b>Lead:</b> {lead}\n" +
                    $"<b>Location:</b> {activeCase.dateAndLocation}\n" +
                    $"<b>Victim:</b> {activeCase.victimInfo}\n\n" +
                    $"<b>DOSSIER PROGRESS:</b>\n" +
                    $"• Evidence Logged: <b>{discoveredEvidence} / {totalEvidence}</b>\n" +
                    $"• Clues Synthesized: <b>{clueCount}</b>\n\n" +
                    $"<b>STATUS:</b> <color=#166534><b>ACTIVE INVESTIGATION</b></color>";
            }
        }

        public List<CharacterProfileSO> GetAllSuspects(CaseSO activeCase)
        {
            var list = new List<CharacterProfileSO>();
            if (activeCase == null) return list;

            if (activeCase.primarySuspect != null)
            {
                list.Add(activeCase.primarySuspect);
            }

            if (activeCase.additionalSuspects != null)
            {
                foreach (var suspect in activeCase.additionalSuspects)
                {
                    if (suspect != null && !list.Contains(suspect))
                    {
                        list.Add(suspect);
                    }
                }
            }

            return list;
        }

        private void SetupSuspectCard(CaseSO activeCase)
        {
            if (suspectCardSection == null) return;

            var suspects = GetAllSuspects(activeCase);
            if (suspects.Count == 0)
            {
                suspectCardSection.SetActive(false);
                return;
            }

            suspectCardSection.SetActive(true);
            EnsureSuspectControlsExist();

            currentSuspectIndex = Mathf.Clamp(currentSuspectIndex, 0, suspects.Count - 1);
            CharacterProfileSO suspect = suspects[currentSuspectIndex];

            if (suspectPortraitImage != null)
            {
                Sprite portrait = suspect != null ? (suspect.defaultSittingPose != null ? suspect.defaultSittingPose : suspect.GetSpriteForExpression(CharacterExpression.Neutral)) : null;
                suspectPortraitImage.gameObject.SetActive(portrait != null);
                if (portrait != null)
                {
                    suspectPortraitImage.sprite = portrait;
                    suspectPortraitImage.preserveAspect = true;
                }
            }

            if (suspectNameLabel != null && suspect != null)
            {
                bool isPrimary = (activeCase.primarySuspect != null && suspect == activeCase.primarySuspect);
                string roleText = isPrimary ? "PRIMARY SUSPECT" : "PERSON OF INTEREST";
                suspectNameLabel.text = $"<b>{suspect.fullName.ToUpper()}</b>\n<size=12><color=#5C4033>[{roleText}]</color></size>";
            }

            if (suspectIndexLabel != null)
            {
                suspectIndexLabel.text = $"[ {currentSuspectIndex + 1} / {suspects.Count} ]";
            }

            if (prevSuspectButton != null)
            {
                prevSuspectButton.interactable = suspects.Count > 1;
            }
            if (nextSuspectButton != null)
            {
                nextSuspectButton.interactable = suspects.Count > 1;
            }
        }

        public void NextSuspect()
        {
            CaseSO activeCase = GetActiveCase();
            var suspects = GetAllSuspects(activeCase);
            if (suspects.Count <= 1) return;
            currentSuspectIndex = (currentSuspectIndex + 1) % suspects.Count;
            AudioManager.Instance?.PlayPaperFlip();
            SetupSuspectCard(activeCase);
        }

        public void PreviousSuspect()
        {
            CaseSO activeCase = GetActiveCase();
            var suspects = GetAllSuspects(activeCase);
            if (suspects.Count <= 1) return;
            currentSuspectIndex = (currentSuspectIndex - 1 + suspects.Count) % suspects.Count;
            AudioManager.Instance?.PlayPaperFlip();
            SetupSuspectCard(activeCase);
        }

        public void CycleNextTab()
        {
            switch (currentTab)
            {
                case NotebookTab.CaseSummary: SwitchTab(NotebookTab.Suspects); break;
                case NotebookTab.Suspects: SwitchTab(NotebookTab.Evidence); break;
                case NotebookTab.Evidence: SwitchTab(NotebookTab.Clues); break;
                case NotebookTab.Clues: SwitchTab(NotebookTab.CaseSummary); break;
            }
        }

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

            if (evidenceIndexLabel != null)
            {
                evidenceIndexLabel.text = $"[ {currentEvidenceIndex + 1} / {count} ]";
            }

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

            if (prevEvidenceButton == null || nextEvidenceButton == null || evidenceIndexLabel == null)
            {
                CreateEvidenceNavRow(cardTransform);
            }

            if (prevEvidenceButton != null)
            {
                prevEvidenceButton.onClick.RemoveListener(PreviousEvidence);
                prevEvidenceButton.onClick.AddListener(PreviousEvidence);
                StyleNavButton(prevEvidenceButton, "<");
            }
            if (nextEvidenceButton != null)
            {
                nextEvidenceButton.onClick.RemoveListener(NextEvidence);
                nextEvidenceButton.onClick.AddListener(NextEvidence);
                StyleNavButton(nextEvidenceButton, ">");
            }
        }

        private void EnsureSuspectControlsExist()
        {
            if (suspectCardSection == null) return;

            Transform cardTransform = suspectCardSection.transform;

            if (prevSuspectButton == null)
            {
                var prevTransform = cardTransform.Find("Suspect_Nav_Row/Button_Prev") ?? cardTransform.Find("Button_Prev");
                if (prevTransform != null) prevSuspectButton = prevTransform.GetComponent<Button>();
            }

            if (nextSuspectButton == null)
            {
                var nextTransform = cardTransform.Find("Suspect_Nav_Row/Button_Next") ?? cardTransform.Find("Button_Next");
                if (nextTransform != null) nextSuspectButton = nextTransform.GetComponent<Button>();
            }

            if (suspectIndexLabel == null)
            {
                var indexTransform = cardTransform.Find("Suspect_Nav_Row/Text_Index") ?? cardTransform.Find("Text_Index");
                if (indexTransform != null) suspectIndexLabel = indexTransform.GetComponent<Text>();
            }

            if (prevSuspectButton == null || nextSuspectButton == null || suspectIndexLabel == null)
            {
                CreateSuspectNavRow(cardTransform);
            }

            if (prevSuspectButton != null)
            {
                prevSuspectButton.onClick.RemoveListener(PreviousSuspect);
                prevSuspectButton.onClick.AddListener(PreviousSuspect);
                StyleNavButton(prevSuspectButton, "<");
            }
            if (nextSuspectButton != null)
            {
                nextSuspectButton.onClick.RemoveListener(NextSuspect);
                nextSuspectButton.onClick.AddListener(NextSuspect);
                StyleNavButton(nextSuspectButton, ">");
            }
        }

        private void CreateEvidenceNavRow(Transform parent)
        {
            Transform existingRow = parent.Find("Evidence_Nav_Row");
            GameObject rowObj = existingRow != null ? existingRow.gameObject : new GameObject("Evidence_Nav_Row", typeof(RectTransform));
            rowObj.transform.SetParent(parent, false);

            RectTransform rowRt = rowObj.GetComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0.05f, 0f);
            rowRt.anchorMax = new Vector2(0.95f, 0f);
            rowRt.pivot = new Vector2(0.5f, 1f);
            rowRt.anchoredPosition = new Vector2(0f, -6f);
            rowRt.sizeDelta = new Vector2(0f, 34f);

            if (prevEvidenceButton == null)
            {
                prevEvidenceButton = CreateNavButton(rowRt, "Button_Prev", "<", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(44f, 32f));
            }
            if (nextEvidenceButton == null)
            {
                nextEvidenceButton = CreateNavButton(rowRt, "Button_Next", ">", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(44f, 32f));
            }
            if (evidenceIndexLabel == null)
            {
                evidenceIndexLabel = CreateNavText(rowRt, "Text_Index", "[ 1 / 1 ]", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-96f, 0f));
            }
        }

        private void CreateSuspectNavRow(Transform parent)
        {
            Transform existingRow = parent.Find("Suspect_Nav_Row");
            GameObject rowObj = existingRow != null ? existingRow.gameObject : new GameObject("Suspect_Nav_Row", typeof(RectTransform));
            rowObj.transform.SetParent(parent, false);

            RectTransform rowRt = rowObj.GetComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0.05f, 0f);
            rowRt.anchorMax = new Vector2(0.95f, 0f);
            rowRt.pivot = new Vector2(0.5f, 1f);
            rowRt.anchoredPosition = new Vector2(0f, -6f);
            rowRt.sizeDelta = new Vector2(0f, 34f);

            if (prevSuspectButton == null)
            {
                prevSuspectButton = CreateNavButton(rowRt, "Button_Prev", "<", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(44f, 32f));
            }
            if (nextSuspectButton == null)
            {
                nextSuspectButton = CreateNavButton(rowRt, "Button_Next", ">", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(44f, 32f));
            }
            if (suspectIndexLabel == null)
            {
                suspectIndexLabel = CreateNavText(rowRt, "Text_Index", "[ 1 / 1 ]", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-96f, 0f));
            }
        }

        private static Button CreateNavButton(Transform parent, string name, string labelText, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            Transform existing = parent.Find(name);
            GameObject btnObj = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Button btn = btnObj.GetComponent<Button>();
            Image img = btnObj.GetComponent<Image>();
            img.color = new Color(0.95f, 0.91f, 0.82f, 1f);
            btn.targetGraphic = img;

            Outline outline = btnObj.GetComponent<Outline>() ?? btnObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.40f, 0.30f, 0.20f, 0.85f);
            outline.effectDistance = new Vector2(1.2f, -1.2f);

            Transform labelTransform = btnObj.transform.Find("Label");
            GameObject txtObj = labelTransform != null ? labelTransform.gameObject : new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            txtObj.transform.SetParent(btnObj.transform, false);

            RectTransform txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            Text txt = txtObj.GetComponent<Text>();
            txt.font = DetectiveUITheme.GetStandardFont();
            txt.text = labelText;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(0.20f, 0.16f, 0.12f, 1f);
            txt.fontSize = 18;
            txt.raycastTarget = false;

            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1f, 0.96f, 0.86f, 1f);
            cb.pressedColor = new Color(0.85f, 0.80f, 0.70f, 1f);
            cb.disabledColor = new Color(0.85f, 0.85f, 0.85f, 0.5f);
            btn.colors = cb;

            return btn;
        }

        private static Text CreateNavText(Transform parent, string name, string defaultText, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            Transform existing = parent.Find(name);
            GameObject txtObj = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            txtObj.transform.SetParent(parent, false);

            RectTransform rt = txtObj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Text txt = txtObj.GetComponent<Text>();
            txt.font = DetectiveUITheme.GetStandardFont();
            txt.text = defaultText;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(0.35f, 0.27f, 0.18f, 1f);
            txt.fontSize = 14;
            txt.raycastTarget = false;
            return txt;
        }

        private static void StyleNavButton(Button btn, string labelText)
        {
            if (btn == null) return;
            Image img = btn.GetComponent<Image>();
            if (img == null) img = btn.gameObject.AddComponent<Image>();
            img.color = new Color(0.95f, 0.91f, 0.82f, 1f);
            btn.targetGraphic = img;

            Outline outline = btn.GetComponent<Outline>() ?? btn.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.40f, 0.30f, 0.20f, 0.85f);
            outline.effectDistance = new Vector2(1.2f, -1.2f);

            Text label = btn.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.font = DetectiveUITheme.GetStandardFont();
                label.text = labelText;
                label.fontStyle = FontStyle.Bold;
                label.alignment = TextAnchor.MiddleCenter;
                label.color = new Color(0.20f, 0.16f, 0.12f, 1f);
            }

            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1f, 0.96f, 0.86f, 1f);
            cb.pressedColor = new Color(0.85f, 0.80f, 0.70f, 1f);
            cb.disabledColor = new Color(0.85f, 0.85f, 0.85f, 0.5f);
            btn.colors = cb;
        }

        private void SetupCluesCard(CaseSO activeCase, IReadOnlyDictionary<string, string> cluesDict)
        {
            if (cluesCardSection == null) return;
            cluesCardSection.SetActive(true);
            int count = cluesDict != null ? cluesDict.Count : 0;
            if (cluesCountLabel != null)
            {
                cluesCountLabel.text = $"<b>DEDUCTION JOURNAL</b>\n\n" +
                    $"<b>Synthesized Clues:</b> <size=18><b>{count}</b></size>\n\n" +
                    $"<i>Cross-reference suspect testimonies and gathered physical evidence to uncover critical contradictions.</i>\n\n" +
                    $"<color=#5C4033><b>TIP:</b> Open the Deduction Board to connect clues.</color>";
            }
        }

        private void HideAllCardSections()
        {
            if (suspectCardSection != null) suspectCardSection.SetActive(false);
            if (evidenceCardSection != null) evidenceCardSection.SetActive(false);
            if (summaryCardSection != null) summaryCardSection.SetActive(false);
            if (cluesCardSection != null) cluesCardSection.SetActive(false);
        }

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
            pos.x = isSelected ? baseX + 24f : baseX;
            rt.anchoredPosition = pos;

            Image img = button.GetComponent<Image>();
            if (img != null)
            {
                img.color = isSelected ? Color.white : new Color(0.85f, 0.83f, 0.80f, 0.92f);
            }
        }

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

        public void ApplyTransform()
        {
            if (clipboardRoot != null)
            {
                clipboardRoot.localScale = new Vector3(_currentZoom, _currentZoom, 1f);
                clipboardRoot.anchoredPosition = _currentPosition;
            }
        }

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

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
        }

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
