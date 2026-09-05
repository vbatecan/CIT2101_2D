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

        private void HandleCaseStateChanged(CaseSO c) => SwitchTab(currentTab);
        private void HandleEvidenceDiscovered(EvidenceSO e) => SwitchTab(currentTab);
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
                    contentText = formattingService.FormatDiscoveredEvidence(activeCase, discovered);
                    SetupEvidenceCard(activeCase, discovered);
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

        private void SetupEvidenceCard(CaseSO activeCase, HashSet<string> discovered)
        {
            if (evidenceCardSection == null) return;

            EvidenceSO firstEv = null;
            if (activeCase.evidenceItems != null && discovered != null)
            {
                foreach (var ev in activeCase.evidenceItems)
                {
                    if (ev != null && discovered.Contains(ev.id) && ev.normalSprite != null)
                    {
                        firstEv = ev;
                        break;
                    }
                }
            }

            if (firstEv != null)
            {
                evidenceCardSection.SetActive(true);
                if (evidencePreviewImage != null)
                {
                    evidencePreviewImage.sprite = firstEv.normalSprite;
                    evidencePreviewImage.preserveAspect = true;
                }
                if (evidenceNameLabel != null)
                {
                    evidenceNameLabel.text = firstEv.evidenceName;
                }
            }
            else
            {
                evidenceCardSection.SetActive(false);
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
