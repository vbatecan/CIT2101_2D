using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    /// <summary>
    /// UI View MonoBehaviour managing dialogue presentation, typewriter text rendering,
    /// and evidence challenge selection grids.
    /// Can be dragged directly onto the DialoguePanel GameObject in the Unity Inspector.
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        public static DialogueUI Instance { get; private set; }

        [Header("UI Elements")]
        public Text speakerNameText;
        public Text dialogueBodyText;
        public Button nextButton;
        public Button challengeButton;
        public Button closeDialogueButton;
        public GameObject challengeHighlight;

        [Header("Evidence Selection Overlay (Presenting Evidence)")]
        public GameObject evidencePickerContainer;
        public Transform evidencePickerGrid;
        public GameObject evidencePickerItemPrefab;

        [Header("World Bubble Alignment")]
        [Tooltip("Optional RectTransform used as the dialogue bubble. If empty, this component's RectTransform is moved.")]
        public RectTransform bubbleRect;
        public Vector2 bubbleScreenOffset = new Vector2(0f, 90f);

        [Header("Typewriter Settings")]
        public float charactersPerSecond = 35f;

        /// <summary>Global state indicating whether the dialogue window is currently visible and active.</summary>
        public static bool IsDialogueOpen { get; private set; } = false;

        private Coroutine typewriterCoroutine;
        private bool isTyping = false;
        private string currentFullText = "";

        /// <summary>
        /// Clears placeholder text and ensures dialogue starts hidden.
        /// </summary>
        private void Awake()
        {
            Instance = this;

            if (speakerNameText != null) speakerNameText.text = "";
            if (dialogueBodyText != null) dialogueBodyText.text = "";
            if (evidencePickerContainer != null) evidencePickerContainer.SetActive(false);
            if (challengeHighlight != null) challengeHighlight.SetActive(false);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Binds UI button click listeners and subscribes to interrogation manager events.
        /// </summary>
        private void Start()
        {
            if (nextButton != null) nextButton.onClick.AddListener(OnNextButtonClicked);
            if (challengeButton != null) challengeButton.onClick.AddListener(OnChallengeButtonClicked);
            if (closeDialogueButton != null) closeDialogueButton.onClick.AddListener(OnCloseButtonClicked);

            if (InterrogationManager.Instance != null)
            {
                InterrogationManager.Instance.OnDialogueNodeDisplayed += DisplayNode;
                InterrogationManager.Instance.OnChallengeModeToggled += UpdateChallengeState;
                InterrogationManager.Instance.OnChallengeResult += HandleChallengeResult;
                InterrogationManager.Instance.OnDialogueClosed += HideDialoguePanel;
            }

            if (evidencePickerContainer != null) evidencePickerContainer.SetActive(false);
            if (!IsDialogueOpen)
            {
                HideDialoguePanel();
            }

            if (CaseClosed.Services.GameSettingsService.Instance != null)
            {
                charactersPerSecond = CaseClosed.Services.GameSettingsService.Instance.TextSpeed;
            }
        }

        /// <summary>
        /// Updates the typewriter dialogue text speed (characters per second).
        /// </summary>
        public void SetTextSpeed(float speed)
        {
            charactersPerSecond = Mathf.Clamp(speed, 15f, 100f);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;

            if (InterrogationManager.Instance != null)
            {
                InterrogationManager.Instance.OnDialogueNodeDisplayed -= DisplayNode;
                InterrogationManager.Instance.OnChallengeModeToggled -= UpdateChallengeState;
                InterrogationManager.Instance.OnChallengeResult -= HandleChallengeResult;
                InterrogationManager.Instance.OnDialogueClosed -= HideDialoguePanel;
            }
        }

        /// <summary>
        /// Displays a dialogue node statement with typewriter effect.
        /// </summary>
        /// <param name="node">The dialogue node being presented.</param>
        public void DisplayNode(DialogueNode node)
        {
            if (node == null) return;

            IsDialogueOpen = true;
            gameObject.SetActive(true);

            Debug.Log($"[UI:Dialogue] Displaying node '{node.nodeId}' (Speaker: '{node.speakerName}', Challengeable: {node.isChallengeable})");

            if (speakerNameText != null) speakerNameText.text = node.speakerName;

            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = StartCoroutine(TypeText(node.statementText));

            if (challengeButton != null)
            {
                challengeButton.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Places the optional bubble above a world-space evidence item.
        /// </summary>
        public void AlignToWorldTarget(Transform target)
        {
            if (target == null) return;

            RectTransform targetRect = bubbleRect != null ? bubbleRect : transform as RectTransform;
            Canvas canvas = targetRect != null ? targetRect.GetComponentInParent<Canvas>() : null;
            Camera worldCamera = Camera.main;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(worldCamera, target.position) + bubbleScreenOffset;

            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay && canvas.worldCamera != null)
            {
                worldCamera = canvas.worldCamera;
            }

            if (canvas != null && targetRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, screenPoint, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : worldCamera, out Vector2 localPoint))
            {
                targetRect.localPosition = localPoint;
            }
        }

        /// <summary>
        /// Coroutine that animates text character by character with optional typewriter audio clicks.
        /// </summary>
        /// <param name="text">The full statement text to animate.</param>
        /// <returns>IEnumerator for coroutine execution.</returns>
        private IEnumerator TypeText(string text)
        {
            isTyping = true;
            currentFullText = text;
            if (dialogueBodyText != null) dialogueBodyText.text = "";

            float delay = 1f / Mathf.Max(1f, charactersPerSecond);
            for (int i = 0; i < text.Length; i++)
            {
                if (dialogueBodyText != null) dialogueBodyText.text += text[i];
                if (i % 3 == 0) AudioManager.Instance?.PlayTypewriterKey();
                yield return new WaitForSeconds(delay);
            }

            isTyping = false;
        }

        /// <summary>
        /// Instantly finishes the current typewriter animation, displaying the full dialogue string.
        /// </summary>
        public void CompleteTypingInstantly()
        {
            if (isTyping)
            {
                if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
                if (dialogueBodyText != null) dialogueBodyText.text = currentFullText;
                isTyping = false;
            }
        }

        /// <summary>
        /// Handles next button click: completes text immediately if typing, or advances dialogue node if finished.
        /// </summary>
        private void OnNextButtonClicked()
        {
            Debug.Log($"[UI:Dialogue] Next button clicked (IsTyping: {isTyping})");

            if (isTyping)
            {
                CompleteTypingInstantly();
            }
            else
            {
                InterrogationManager.Instance?.AdvanceDialogue();
            }
        }

        /// <summary>
        /// Handles challenge button click, toggling challenge mode in the interrogation controller.
        /// </summary>
        private void OnChallengeButtonClicked()
        {
            bool currentState = InterrogationManager.Instance != null && InterrogationManager.Instance.isChallengeModeActive;
            bool newState = !currentState;
            Debug.Log($"[UI:Dialogue] Challenge button clicked (Switching to: {newState})");
            InterrogationManager.Instance?.ToggleChallengeMode(newState);
        }

        /// <summary>
        /// Updates the visual challenge state highlight and toggles the evidence picker grid.
        /// </summary>
        /// <param name="isActive">Whether challenge mode is currently enabled.</param>
        private void UpdateChallengeState(bool isActive)
        {
            if (challengeHighlight != null) challengeHighlight.SetActive(isActive);
            if (evidencePickerContainer != null) evidencePickerContainer.SetActive(isActive);

            if (isActive)
            {
                PopulateEvidencePicker();
            }
        }

        /// <summary>
        /// Populates the evidence picker grid with clickable buttons representing all discovered evidence items.
        /// </summary>
        private void PopulateEvidencePicker()
        {
            if (evidencePickerGrid == null) return;

            foreach (Transform child in evidencePickerGrid)
            {
                Destroy(child.gameObject);
            }

            var discoveredIds = CaseManager.Instance?.discoveredEvidenceIds;
            var activeCase = CaseManager.Instance?.activeCase;

            if (activeCase == null || discoveredIds == null) return;

            foreach (var ev in activeCase.evidenceItems)
            {
                if (ev != null && discoveredIds.Contains(ev.id))
                {
                    GameObject btnObj = new GameObject($"Present_{ev.id}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                    btnObj.transform.SetParent(evidencePickerGrid, false);

                    Image img = btnObj.GetComponent<Image>();
                    if (ev.normalSprite != null) img.sprite = ev.normalSprite;

                    EvidenceSO currentEv = ev;
                    btnObj.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        Debug.Log($"[UI:Dialogue] Evidence picker selected item '{currentEv.evidenceName}' (ID: {currentEv.id}) to present");
                        InterrogationManager.Instance?.PresentEvidenceToChallenge(currentEv);
                    });
                }
            }
        }

        /// <summary>
        /// Handles dialogue close button click, returning immediately to table exploration mode.
        /// </summary>
        private void OnCloseButtonClicked()
        {
            Debug.Log("[UI:Dialogue] Close dialogue button clicked");
            InterrogationManager.Instance?.CloseDialogue();
        }

        /// <summary>
        /// Hides the dialogue panel when dialogue finishes or is closed.
        /// </summary>
        public void HideDialoguePanel()
        {
            IsDialogueOpen = false;
            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            isTyping = false;
            if (evidencePickerContainer != null) evidencePickerContainer.SetActive(false);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Displays the reaction dialogue and updates the speaker name following a challenge attempt.
        /// </summary>
        /// <param name="success">Whether the challenge exposed a contradiction.</param>
        /// <param name="reactionMessage">The reaction dialogue text returned from the challenge.</param>
        private void HandleChallengeResult(bool success, string reactionMessage)
        {
            IsDialogueOpen = true;
            gameObject.SetActive(true);
            Debug.Log($"[UI:Dialogue] Received challenge result (Success: {success}, MessageLength: {reactionMessage?.Length ?? 0})");

            if (speakerNameText != null)
            {
                CharacterProfileSO suspect = InterrogationManager.Instance?.currentSuspect;
                speakerNameText.text = suspect != null ? suspect.fullName : "Suspect";
            }

            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = StartCoroutine(TypeText(reactionMessage));
        }
    }
}
