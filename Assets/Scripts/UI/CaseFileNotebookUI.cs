using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public class CaseFileNotebookUI : MonoBehaviour
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
        private Coroutine slideCoroutine;
        private bool isClosing = false;
        private bool isSubscribed = false;

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
        }

        private void Start()
        {
            SetupTabButtons();
            SubscribeToManagerEvents();
            SwitchTab(currentTab);
        }

        private void OnEnable()
        {
            isClosing = false;
            SetupTabButtons();
            CaptureTabBasePositions();
            SubscribeToManagerEvents();
            SwitchTab(currentTab);

            if (clipboardRoot != null)
            {
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
            UnsubscribeFromManagerEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromManagerEvents();
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
                backdropButton.onClick.AddListener(OnCloseClicked);
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
            if (CaseManager.Instance != null && CaseManager.Instance.activeCase != null)
                return CaseManager.Instance.activeCase;
            var mgr = FindFirstObjectByType<CaseManager>();
            if (mgr != null && mgr.activeCase != null)
                return mgr.activeCase;
            return null;
        }

        private HashSet<string> GetDiscoveredEvidenceIds()
        {
            if (evidenceOverride != null) return evidenceOverride;
            if (CaseManager.Instance != null) return CaseManager.Instance.discoveredEvidenceIds;
            var mgr = FindFirstObjectByType<CaseManager>();
            return mgr != null ? mgr.discoveredEvidenceIds : null;
        }

        private Dictionary<string, string> GetUnlockedClues()
        {
            if (cluesOverride != null) return cluesOverride;
            if (CaseManager.Instance != null) return CaseManager.Instance.unlockedCluesText;
            var mgr = FindFirstObjectByType<CaseManager>();
            return mgr != null ? mgr.unlockedCluesText : null;
        }

        /// <summary>
        /// Switches the active notebook tab, formats case data, and updates tab highlights.
        /// </summary>
        public void SwitchTab(NotebookTab tab)
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
                    contentText = formattingService.FormatCaseSummary(activeCase);
                    SetupSummaryCard(activeCase);
                    break;

                case NotebookTab.Suspects:
                    if (notebookTitleText != null) notebookTitleText.text = "SUSPECT DOSSIER";
                    contentText = formattingService.FormatSuspectProfiles(activeCase);
                    SetupSuspectCard(activeCase);
                    break;

                case NotebookTab.Evidence:
                    if (notebookTitleText != null) notebookTitleText.text = "EVIDENCE REPOSITORY";
                    var discovered = GetDiscoveredEvidenceIds();
                    contentText = SetupEvidenceCard(activeCase, discovered);
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
                string lead = activeCase.leadInvestigator != null ? activeCase.leadInvestigator.fullName : "Detective Bureau";
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
            var discovered = GetDiscoveredEvidenceIds();
            string text = SetupEvidenceCard(activeCase, discovered);
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
            var discovered = GetDiscoveredEvidenceIds();
            string text = SetupEvidenceCard(activeCase, discovered);
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
            SwitchTab(NotebookTab.Evidence);
        }

        private string SetupEvidenceCard(CaseSO activeCase, HashSet<string> discovered)
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

            bool isDiscovered = currentEv != null && discovered != null && discovered.Contains(currentEv.id);

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

            string dossierText = formattingService.FormatEvidenceDossier(currentEv, isDiscovered, currentEvidenceIndex + 1, count);
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

        private void SetupCluesCard(CaseSO activeCase, Dictionary<string, string> cluesDict)
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
            Debug.Log("[UI:Notebook] Close notebook requested, animating slide-out");

            if (clipboardRoot != null && gameObject.activeInHierarchy)
            {
                if (slideCoroutine != null) StopCoroutine(slideCoroutine);
                slideCoroutine = StartCoroutine(SlideCoroutine(clipboardRoot.anchoredPosition.y, hiddenPosY, false));
            }
            else
            {
                UIManager.Instance?.ToggleNotebookPanel();
            }
        }

        private IEnumerator SlideCoroutine(float fromY, float toY, bool isOpening)
        {
            if (clipboardRoot == null) yield break;

            float elapsed = 0f;
            Vector2 pos = clipboardRoot.anchoredPosition;
            pos.y = fromY;
            clipboardRoot.anchoredPosition = pos;

            while (elapsed < slideDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / slideDuration);
                float ease = 1f - Mathf.Pow(1f - t, 3f);
                pos.y = Mathf.Lerp(fromY, toY, ease);
                clipboardRoot.anchoredPosition = pos;
                yield return null;
            }

            pos.y = toY;
            clipboardRoot.anchoredPosition = pos;
            slideCoroutine = null;

            if (!isOpening)
            {
                isClosing = false;
                UIManager.Instance?.ToggleNotebookPanel();
            }
        }
    }
}
