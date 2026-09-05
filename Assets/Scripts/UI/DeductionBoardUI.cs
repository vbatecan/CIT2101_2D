using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    /// <summary>
    /// UI View coordinator for the Deduction Board (Mind Palace).
    /// Dynamically renders discovered clues as interactive pins/cards, highlights active pair selections,
    /// presents deduction feedback banners, and provides a close button to return to the investigation desk.
    /// Can be attached directly to the Panel_DeductionBoard GameObject in the Unity Inspector.
    /// </summary>
    public class DeductionBoardUI : MonoBehaviour
    {
        [Header("Header & Close Controls")]
        public Text boardTitleText;
        public Button closeBoardButton;

        [Header("Clue Selection Grid")]
        public Transform cluesContainer;
        [Tooltip("Optional prefab for clue cards on the deduction board.")]
        [SerializeField] public GameObject clueCardPrefab;
        public Text selectionStatusText;

        [Header("Deduction Feedback Banner")]
        public GameObject feedbackBanner;
        public Text feedbackText;

        [Header("Completed Deductions List")]
        public Transform deductionsContainer;
        public Text completedDeductionsBody;

        private static readonly Color SelectedCardColor = new Color(0.9f, 0.7f, 0.1f, 0.9f);
        private static readonly Color NormalCardColor = new Color(0.2f, 0.22f, 0.28f, 0.85f);

        private void Awake()
        {
            if (closeBoardButton != null)
            {
                closeBoardButton.onClick.AddListener(OnCloseClicked);
            }

            if (feedbackBanner != null)
            {
                feedbackBanner.SetActive(false);
            }
        }

        private void Start()
        {
            if (closeBoardButton != null)
            {
                closeBoardButton.onClick.RemoveListener(OnCloseClicked);
                closeBoardButton.onClick.AddListener(OnCloseClicked);
            }

            RegisterEvents();
        }

        private void OnEnable()
        {
            RegisterEvents();
            RefreshBoard();
        }

        private void OnDisable()
        {
            UnregisterEvents();
        }

        private void OnDestroy()
        {
            UnregisterEvents();
        }

        private bool eventsRegistered = false;

        private void RegisterEvents()
        {
            if (eventsRegistered || DeductionBoardController.Instance == null) return;

            DeductionBoardController.Instance.OnClueSelectedForConnection += HandleClueSelected;
            DeductionBoardController.Instance.OnConnectionResult += HandleConnectionResult;
            eventsRegistered = true;
        }

        private void UnregisterEvents()
        {
            if (!eventsRegistered || DeductionBoardController.Instance == null) return;

            DeductionBoardController.Instance.OnClueSelectedForConnection -= HandleClueSelected;
            DeductionBoardController.Instance.OnConnectionResult -= HandleConnectionResult;
            eventsRegistered = false;
        }

        /// <summary>
        /// Refreshes the clues list, completed deductions, and current selection status.
        /// </summary>
        public void RefreshBoard()
        {
            CaseSO activeCase = CaseManager.Instance?.activeCase;
            if (activeCase == null) return;

            if (boardTitleText != null)
            {
                boardTitleText.text = $"Deduction Board - {activeCase.caseTitle}";
            }

            UpdateSelectionStatus();
            RenderClueCards(activeCase);
            RenderCompletedDeductions(activeCase);
        }

        /// <summary>
        /// Dynamically renders clickable cards/buttons for all unlocked clues and discovered evidence base clues.
        /// Supports clueCardPrefab instantiation with zero-GC fallbacks.
        /// </summary>
        /// <param name="activeCase">The currently active case.</param>
        private void RenderClueCards(CaseSO activeCase)
        {
            if (cluesContainer == null) return;

            foreach (Transform child in cluesContainer)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }

            var unlockedClues = CaseManager.Instance?.unlockedCluesText;
            var discoveredEvIds = CaseManager.Instance?.discoveredEvidenceIds;

            // Collect unique clue entries: (ClueID, DisplayTitle, BodyText)
            var clueEntries = new List<(string id, string title, string text)>();

            // 1. Evidence base clues
            if (activeCase.evidenceItems != null && discoveredEvIds != null)
            {
                foreach (var ev in activeCase.evidenceItems)
                {
                    if (ev != null && discoveredEvIds.Contains(ev.id))
                    {
                        string baseClueId = $"{ev.id}_BASE_CLUE";
                        string clueText = !string.IsNullOrEmpty(ev.unlockedClueText) ? ev.unlockedClueText : ev.baseDescription;
                        clueEntries.Add((baseClueId, ev.evidenceName, clueText));
                    }
                }
            }

            // 2. Unlocked clues from hotspots / contradictions
            if (unlockedClues != null)
            {
                foreach (var kvp in unlockedClues)
                {
                    string clueId = kvp.Key;
                    string clueText = kvp.Value;

                    // Avoid duplicate if already added via base clue
                    if (clueEntries.Exists(c => c.id == clueId)) continue;

                    string displayTitle = GetClueTitle(activeCase, clueId);
                    clueEntries.Add((clueId, displayTitle, clueText));
                }
            }

            string selectedA = DeductionBoardController.Instance?.selectedClueA;

            // Instantiate buttons for each clue entry
            foreach (var entry in clueEntries)
            {
                string currentId = entry.id;
                bool isSelected = (selectedA == currentId);

                if (clueCardPrefab != null)
                {
                    GameObject cardObj = Instantiate(clueCardPrefab, cluesContainer, false);
                    cardObj.name = $"ClueCard_{currentId}";

                    Button btn = cardObj.GetComponent<Button>() ?? cardObj.GetComponentInChildren<Button>();
                    if (btn == null) btn = cardObj.AddComponent<Button>();

                    Image img = cardObj.GetComponent<Image>() ?? (btn.targetGraphic as Image) ?? cardObj.GetComponentInChildren<Image>();
                    if (img != null)
                    {
                        img.color = isSelected ? SelectedCardColor : NormalCardColor;
                    }

                    Text[] texts = cardObj.GetComponentsInChildren<Text>(true);
                    if (texts.Length == 1)
                    {
                        texts[0].text = $"<b>{entry.title}</b>\n{entry.text}";
                        texts[0].color = isSelected ? Color.black : Color.white;
                    }
                    else if (texts.Length > 1)
                    {
                        Text titleText = null;
                        Text bodyText = null;

                        foreach (var textComp in texts)
                        {
                            string tName = textComp.gameObject.name.ToLowerInvariant();
                            if (titleText == null && (tName.Contains("title") || tName.Contains("header") || tName.Contains("name")))
                            {
                                titleText = textComp;
                            }
                            else if (bodyText == null && (tName.Contains("body") || tName.Contains("desc") || tName.Contains("detail") || tName.Contains("text")))
                            {
                                bodyText = textComp;
                            }
                        }

                        if (titleText == null || bodyText == null)
                        {
                            titleText = texts[0];
                            bodyText = texts[1];
                        }

                        titleText.text = entry.title;
                        bodyText.text = entry.text;
                        titleText.color = isSelected ? Color.black : Color.white;
                        bodyText.color = isSelected ? Color.black : Color.white;
                    }

                    btn.onClick.AddListener(() =>
                    {
                        Debug.Log($"[UI:DeductionBoard] Clue card clicked: '{entry.title}' (ID: {currentId})");
                        AudioManager.Instance?.PlayButtonClick();
                        DeductionBoardController.Instance?.SelectClue(currentId);
                    });
                    continue;
                }

                // Procedural fallback
                GameObject fallbackObj = new GameObject($"ClueCard_{currentId}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                fallbackObj.transform.SetParent(cluesContainer, false);

                RectTransform rt = fallbackObj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(280f, 60f);

                Image fallbackImg = fallbackObj.GetComponent<Image>();
                fallbackImg.color = isSelected ? SelectedCardColor : NormalCardColor;

                // Add text child
                GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                textObj.transform.SetParent(fallbackObj.transform, false);

                RectTransform textRt = textObj.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = new Vector2(10f, 5f);
                textRt.offsetMax = new Vector2(-10f, -5f);

                Text cardText = textObj.GetComponent<Text>();
                cardText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                cardText.fontSize = 12;
                cardText.color = isSelected ? Color.black : Color.white;
                cardText.text = $"<b>{entry.title}</b>\n{entry.text}";
                cardText.alignment = TextAnchor.MiddleLeft;

                fallbackObj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Debug.Log($"[UI:DeductionBoard] Clue card clicked: '{entry.title}' (ID: {currentId})");
                    AudioManager.Instance?.PlayButtonClick();
                    DeductionBoardController.Instance?.SelectClue(currentId);
                });
            }
        }

        /// <summary>
        /// Formats and displays the list of completed deductions.
        /// </summary>
        /// <param name="activeCase">The active case containing clue connections.</param>
        private void RenderCompletedDeductions(CaseSO activeCase)
        {
            if (completedDeductionsBody == null) return;

            var unlockedClues = CaseManager.Instance?.unlockedCluesText;
            if (activeCase.clueConnections == null || unlockedClues == null)
            {
                completedDeductionsBody.text = "No deductions formed yet.";
                return;
            }

            var matchedDeductions = new List<string>();
            foreach (var conn in activeCase.clueConnections)
            {
                if (conn != null && unlockedClues.ContainsKey(conn.resultClueId))
                {
                    matchedDeductions.Add($"✓ <b>{conn.connectionTitle}</b>: {conn.deductionText}");
                }
            }

            if (matchedDeductions.Count > 0)
            {
                completedDeductionsBody.text = string.Join("\n\n", matchedDeductions);
            }
            else
            {
                completedDeductionsBody.text = "Select and link two related clues to reveal connections!";
            }
        }

        private string GetClueTitle(CaseSO activeCase, string clueId)
        {
            if (activeCase.clueConnections != null)
            {
                foreach (var conn in activeCase.clueConnections)
                {
                    if (conn != null && conn.resultClueId == clueId)
                        return conn.resultClueTitle;
                }
            }

            if (activeCase.evidenceItems != null)
            {
                foreach (var ev in activeCase.evidenceItems)
                {
                    if (ev != null && ev.hotspots != null)
                    {
                        foreach (var spot in ev.hotspots)
                        {
                            if (spot != null && spot.clueUnlockedId == clueId)
                                return spot.hotspotTitle;
                        }
                    }
                }
            }

            return clueId;
        }

        private void HandleClueSelected(string clueId)
        {
            UpdateSelectionStatus();
            CaseSO activeCase = CaseManager.Instance?.activeCase;
            if (activeCase != null)
            {
                RenderClueCards(activeCase);
            }
        }

        private void UpdateSelectionStatus()
        {
            if (selectionStatusText == null) return;

            string selectedA = DeductionBoardController.Instance?.selectedClueA;
            if (string.IsNullOrEmpty(selectedA))
            {
                selectionStatusText.text = "Select first clue to connect...";
                selectionStatusText.color = Color.white;
            }
            else
            {
                selectionStatusText.text = $"First clue selected: [{selectedA}]. Select a second clue to connect.";
                selectionStatusText.color = Color.yellow;
            }
        }

        private void HandleConnectionResult(bool success, ClueConnectionSO matchedRule)
        {
            if (feedbackBanner != null) feedbackBanner.SetActive(true);

            if (success && matchedRule != null)
            {
                if (feedbackText != null)
                {
                    feedbackText.text = $"★ DEDUCTION SUCCESSFUL!\n{matchedRule.resultClueTitle}: {matchedRule.deductionText}";
                    feedbackText.color = Color.green;
                }
            }
            else
            {
                if (feedbackText != null)
                {
                    feedbackText.text = "No deduction found connecting those two clues. Try another combination!";
                    feedbackText.color = new Color(1f, 0.4f, 0.4f);
                }
            }

            RefreshBoard();
        }

        /// <summary>
        /// Handles click on close button, returning to the investigation desk view.
        /// </summary>
        private void OnCloseClicked()
        {
            Debug.Log("[UI:DeductionBoard] Close button clicked -> returning to desk");
            UIManager.Instance?.ToggleDeductionBoardPanel();
        }
    }
}
