using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Gameplay;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    /// <summary>
    /// Reusable bottom-anchored detective dialogue box with speaker badges,
    /// active speaker dimming, typewriter text, bouncing arrow, choices, and fade animations.
    /// Supports both standalone DialogueData and existing InterrogationManager sessions.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class DialogueBox : MonoBehaviour
    {
        private static DialogueBox _instance;
        public static DialogueBox Instance
        {
            get
            {
                if (_instance == null) _instance = FindFirstObjectByType<DialogueBox>(FindObjectsInactive.Include);
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("UI Rects & Canvas Group")]
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Typography (TextMeshPro)")]
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueBodyText;
        [SerializeField] private GameObject speakerBadgeObject;

        [Header("Active Speaker Portraits & Suspect Dimming")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private CharacterDisplay leftSuspectDisplay;
        [SerializeField] private CharacterDisplay rightSuspectDisplay;
        [SerializeField] private Color activeSuspectColor = Color.white;
        [SerializeField] private Color dimmedSuspectColor = new Color(0.42f, 0.42f, 0.48f, 1f);

        [Header("Typewriter Settings")]
        [Range(15f, 120f)]
        [SerializeField] private float charactersPerSecond = 42f;

        [Header("Next Indicator")]
        [SerializeField] private RectTransform nextIndicatorArrow;
        [SerializeField] private float bounceHeight = 6f;
        [SerializeField] private float bounceSpeed = 6f;

        [Header("Branching Choice Buttons")]
        [SerializeField] private Transform choiceContainer;
        [SerializeField] private GameObject choiceButtonPrefab;

        // Events
        public event Action OnDialogueStarted;
        public event Action<DialogueLine> OnLineStarted;
        public event Action<DialogueLine> OnLineCompleted;
        public event Action OnDialogueEnded;
        public event Action<int> OnChoiceSelected;

        // State
        private DialogueData _currentData;
        private int _currentLineIndex = -1;
        private bool _isTyping = false;
        private string _fullLineText = "";
        private readonly StringBuilder _textBuffer = new StringBuilder(512);

        private Coroutine _typewriterCoroutine;
        private Coroutine _fadeCoroutine;
        private Coroutine _bounceCoroutine;
        private Vector2 _indicatorInitialPos;
        private int _lastInputFrame = -1;
        private bool _isInterrogationMode = false;

        public bool IsOpen => canvasGroup != null && canvasGroup.alpha > 0.01f;
        public bool IsTyping => _isTyping;

        private void Awake()
        {
            _instance = this;
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (panelRect == null) panelRect = GetComponent<RectTransform>();

            if (nextIndicatorArrow != null)
            {
                _indicatorInitialPos = nextIndicatorArrow.anchoredPosition;
                nextIndicatorArrow.gameObject.SetActive(false);
            }

            if (choiceContainer != null) choiceContainer.gameObject.SetActive(false);

            // Initial state: hidden
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        private void Start()
        {
            // Auto-detect suspects in scene if unassigned
            if (leftSuspectDisplay == null || rightSuspectDisplay == null)
            {
                CharacterDisplay[] displays = FindObjectsByType<CharacterDisplay>(FindObjectsSortMode.None);
                for (int i = 0; i < displays.Length; i++)
                {
                    var d = displays[i];
                    if (d.characterSlot == CharacterSlot.PrimarySuspect || d.gameObject.name.IndexOf("Left", StringComparison.OrdinalIgnoreCase) >= 0)
                        leftSuspectDisplay = d;
                    else if (d.characterSlot == CharacterSlot.SecondarySuspect || d.gameObject.name.IndexOf("Right", StringComparison.OrdinalIgnoreCase) >= 0)
                        rightSuspectDisplay = d;
                }
            }

            // Sync text speed from persistent game settings if available
            if (CaseClosed.Services.GameSettingsService.Instance != null)
            {
                charactersPerSecond = CaseClosed.Services.GameSettingsService.Instance.TextSpeed;
            }
        }

        private void Update()
        {
            if (!IsOpen) return;

            if (CheckAdvanceInput())
            {
                Advance();
            }
        }

        private bool CheckAdvanceInput()
        {
            if (_lastInputFrame == Time.frameCount) return false;

            bool pressed = Input.GetKeyDown(KeyCode.Space) ||
                           Input.GetKeyDown(KeyCode.Return) ||
                           Input.GetKeyDown(KeyCode.KeypadEnter) ||
                           Input.GetMouseButtonDown(0);

            if (pressed)
            {
                _lastInputFrame = Time.frameCount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Begins presenting a sequential or branching DialogueData ScriptableObject.
        /// </summary>
        public void StartDialogue(DialogueData data)
        {
            if (data == null || data.LineCount == 0)
            {
                Debug.LogWarning("[UI:DialogueBox] Cannot start dialogue: DialogueData is null or empty.");
                return;
            }

            _isInterrogationMode = false;
            _currentData = data;
            _currentLineIndex = -1;

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeCanvasGroup(1f, 0.2f, true));

            OnDialogueStarted?.Invoke();
            Advance();
        }

        /// <summary>
        /// Displays an authored DialogueLine statement with typewriter text.
        /// </summary>
        public void ShowLine(DialogueLine line)
        {
            if (line == null) return;

            if (speakerNameText != null)
            {
                speakerNameText.text = line.speakerName.ToUpperInvariant();
                if (speakerBadgeObject != null)
                    speakerBadgeObject.SetActive(!string.IsNullOrEmpty(line.speakerName));
            }

            // Update portraits & suspect stage dimming
            UpdateSpeakerVisuals(line);

            // Clear choices
            ClearChoices();

            // Start typewriter
            if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = StartCoroutine(TypeText(line.text));

            OnLineStarted?.Invoke(line);
        }

        /// <summary>
        /// Bridge to display a DialogueNode from InterrogationManager.
        /// </summary>
        public void DisplayNode(DialogueNode node)
        {
            if (node == null) return;

            _isInterrogationMode = true;
            _currentData = null;
            _currentLineIndex = -1;

            if (!IsOpen)
            {
                if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = StartCoroutine(FadeCanvasGroup(1f, 0.2f, true));
            }

            DialogueLine line = new DialogueLine
            {
                speakerName = !string.IsNullOrEmpty(node.speakerName) ? node.speakerName : "Suspect",
                speakerId = node.speakerId,
                text = node.statementText,
                expression = node.expression
            };

            ShowLine(line);
        }

        public void Advance()
        {
            if (_isTyping)
            {
                CompleteTypingImmediately();
                return;
            }

            if (_isInterrogationMode)
            {
                if (InterrogationManager.Instance != null && !InterrogationManager.Instance.IsChallengeModeActive)
                {
                    InterrogationManager.Instance.AdvanceDialogue();
                }
                return;
            }

            if (_currentData != null && _currentLineIndex >= 0)
            {
                DialogueLine currentLine = _currentData.GetLine(_currentLineIndex);
                if (currentLine != null && currentLine.HasChoices)
                {
                    // Wait for player to choose an option
                    return;
                }
            }

            _currentLineIndex++;
            if (_currentData != null && _currentLineIndex < _currentData.LineCount)
            {
                ShowLine(_currentData.GetLine(_currentLineIndex));
            }
            else
            {
                EndDialogue();
            }
        }

        public void CompleteTypingImmediately()
        {
            if (_typewriterCoroutine != null)
            {
                StopCoroutine(_typewriterCoroutine);
                _typewriterCoroutine = null;
            }

            _isTyping = false;
            if (dialogueBodyText != null) dialogueBodyText.text = _fullLineText;

            OnTypingFinished();
        }

        private IEnumerator TypeText(string text)
        {
            _isTyping = true;
            _fullLineText = text ?? "";
            _textBuffer.Clear();

            if (dialogueBodyText != null) dialogueBodyText.text = "";
            if (nextIndicatorArrow != null) nextIndicatorArrow.gameObject.SetActive(false);

            float charDelay = 1f / Mathf.Max(10f, charactersPerSecond);
            for (int i = 0; i < _fullLineText.Length; i++)
            {
                _textBuffer.Append(_fullLineText[i]);
                if (dialogueBodyText != null) dialogueBodyText.text = _textBuffer.ToString();

                if (i % 3 == 0)
                {
                    AudioManager.Instance?.PlayTypewriterKey();
                }
                yield return new WaitForSeconds(charDelay);
            }

            _isTyping = false;
            OnTypingFinished();
        }

        private void OnTypingFinished()
        {
            DialogueLine currentLine = (_currentData != null && _currentLineIndex >= 0)
                ? _currentData.GetLine(_currentLineIndex) : null;

            OnLineCompleted?.Invoke(currentLine);

            if (currentLine != null && currentLine.HasChoices)
            {
                DisplayChoices(currentLine.choices);
            }
            else
            {
                ShowNextArrow(true);
            }
        }

        private void UpdateSpeakerVisuals(DialogueLine line)
        {
            if (portraitImage != null)
            {
                portraitImage.sprite = line.portraitSprite;
                portraitImage.enabled = (line.portraitSprite != null);
            }

            bool isLeftSpeaker = (line.speakerSlot == CharacterSlot.PrimarySuspect) ||
                                 (line.speakerName.IndexOf("Charl", StringComparison.OrdinalIgnoreCase) >= 0) ||
                                 (line.speakerName.IndexOf("Vince", StringComparison.OrdinalIgnoreCase) >= 0);

            bool isDetective = line.speakerName.IndexOf("Detective", StringComparison.OrdinalIgnoreCase) >= 0;

            if (leftSuspectDisplay != null)
            {
                SpriteRenderer sr = leftSuspectDisplay.characterSpriteRenderer;
                if (sr != null) sr.color = (isLeftSpeaker || isDetective) ? activeSuspectColor : dimmedSuspectColor;
                if (isLeftSpeaker) leftSuspectDisplay.SetExpression(line.expression);
            }

            if (rightSuspectDisplay != null)
            {
                SpriteRenderer sr = rightSuspectDisplay.characterSpriteRenderer;
                if (sr != null) sr.color = (!isLeftSpeaker || isDetective) ? activeSuspectColor : dimmedSuspectColor;
                if (!isLeftSpeaker) rightSuspectDisplay.SetExpression(line.expression);
            }
        }

        private void ShowNextArrow(bool show)
        {
            if (nextIndicatorArrow == null) return;
            nextIndicatorArrow.gameObject.SetActive(show);

            if (show)
            {
                if (_bounceCoroutine != null) StopCoroutine(_bounceCoroutine);
                _bounceCoroutine = StartCoroutine(BounceArrow());
            }
            else if (_bounceCoroutine != null)
            {
                StopCoroutine(_bounceCoroutine);
                _bounceCoroutine = null;
            }
        }

        private IEnumerator BounceArrow()
        {
            float timer = 0f;
            while (true)
            {
                timer += Time.deltaTime * bounceSpeed;
                float offset = Mathf.Sin(timer) * bounceHeight;
                nextIndicatorArrow.anchoredPosition = _indicatorInitialPos + new Vector2(0f, offset);
                yield return null;
            }
        }

        private void DisplayChoices(List<DialogueChoice> choices)
        {
            ShowNextArrow(false);
            if (choiceContainer == null || choiceButtonPrefab == null) return;

            choiceContainer.gameObject.SetActive(true);

            for (int i = 0; i < choices.Count; i++)
            {
                int choiceIndex = i;
                DialogueChoice choice = choices[i];

                GameObject btnObj = Instantiate(choiceButtonPrefab, choiceContainer, false);
                btnObj.name = $"Choice_{choiceIndex}";

                TextMeshProUGUI label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = choice.choiceText;

                Button btn = btnObj.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => SelectChoice(choiceIndex, choice));
                }
            }
        }

        private void SelectChoice(int index, DialogueChoice choice)
        {
            AudioManager.Instance?.PlayButtonClick();
            ClearChoices();
            OnChoiceSelected?.Invoke(index);

            if (!string.IsNullOrEmpty(choice.targetNodeId))
            {
                InterrogationManager.Instance?.JumpToNode(choice.targetNodeId);
            }
            else if (choice.targetLineIndex >= 0 && _currentData != null)
            {
                _currentLineIndex = choice.targetLineIndex - 1;
                Advance();
            }
            else
            {
                Advance();
            }
        }

        private void ClearChoices()
        {
            if (choiceContainer == null) return;
            for (int i = choiceContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(choiceContainer.GetChild(i).gameObject);
            }
            choiceContainer.gameObject.SetActive(false);
        }

        public void EndDialogue()
        {
            if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
            ShowNextArrow(false);
            ClearChoices();

            // Restore suspect lighting
            if (leftSuspectDisplay?.characterSpriteRenderer != null) leftSuspectDisplay.characterSpriteRenderer.color = activeSuspectColor;
            if (rightSuspectDisplay?.characterSpriteRenderer != null) rightSuspectDisplay.characterSpriteRenderer.color = activeSuspectColor;

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeCanvasGroup(0f, 0.2f, false));

            OnDialogueEnded?.Invoke();
        }

        private IEnumerator FadeCanvasGroup(float targetAlpha, float duration, bool interactable)
        {
            float startAlpha = canvasGroup != null ? canvasGroup.alpha : 0f;
            float elapsed = 0f;

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = interactable;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                    yield return null;
                }

                canvasGroup.alpha = targetAlpha;
                canvasGroup.interactable = interactable;
                canvasGroup.blocksRaycasts = interactable;
            }
        }

        public void SetTextSpeed(float cps)
        {
            charactersPerSecond = Mathf.Clamp(cps, 15f, 120f);
        }
    }
}
