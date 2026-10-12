using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using CaseClosed.Data;
using CaseClosed.Gameplay;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    /// <summary>
    /// Configuration mapping associating a character identifier to a specific dialogue box GameObject/prefab.
    /// </summary>
    [Serializable]
    public class CharacterDialogueBoxEntry
    {
        [Tooltip("Character identifier or keyword (e.g. 'Vince', 'Jane', 'Detective', 'Paul', 'Vonn', 'Shan', 'Shania').")]
        public string characterKey;
        [Tooltip("The dialogue box GameObject instance or prefab.")]
        public GameObject dialogueBox;
        [Tooltip("Optional direct TextMeshProUGUI component reference on this box.")]
        public TextMeshProUGUI dialogueText;
    }

    /// <summary>
    /// UI View MonoBehaviour managing dialogue presentation, typewriter text rendering,
    /// character-specific speech bubble selection, and evidence challenge presentation.
    /// Supports both legacy UnityEngine.UI.Text and TMPro.TextMeshProUGUI.
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        private static DialogueUI _instance;
        /// <summary>Singleton instance of the DialogueUI.</summary>
        public static DialogueUI Instance
        {
            get
            {
                if (_instance == null) _instance = FindFirstObjectByType<DialogueUI>(FindObjectsInactive.Include);
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("UI Elements (Legacy & TextMeshPro)")]
        public Text speakerNameText;
        public Text dialogueBodyText;
        public TextMeshProUGUI tmpSpeakerNameText;
        public TextMeshProUGUI tmpDialogueText;
        public Button nextButton;
        public Button challengeButton;
        public Button closeDialogueButton;
        public GameObject challengeHighlight;

        [Header("Evidence Selection Overlay (Presenting Evidence)")]
        public GameObject evidencePickerContainer;
        public Transform evidencePickerGrid;
        [Tooltip("Optional prefab for evidence picker items.")]
        [SerializeField] public GameObject evidencePickerItemPrefab;

        [Header("World Bubble Alignment")]
        [Tooltip("Optional RectTransform used as the dialogue bubble. If empty, this component's RectTransform is moved.")]
        public RectTransform bubbleRect;
        public Vector2 bubbleScreenOffset = new Vector2(0f, 90f);

        [Header("Character Dialogue Boxes")]
        [Tooltip("Mappings for character dialogue boxes.")]
        public List<CharacterDialogueBoxEntry> characterDialogBoxes = new List<CharacterDialogueBoxEntry>();

        /// <summary>Alias for characterDialogBoxes to support alternative naming conventions.</summary>
        public List<CharacterDialogueBoxEntry> characterDialogueBoxes
        {
            get => characterDialogBoxes;
            set => characterDialogBoxes = value;
        }

        [Header("Quick Character Dialogue Box References")]
        public GameObject vinceDialogBox;
        public GameObject janeDialogBox;
        public GameObject paulDialogBox;
        public GameObject vonnDialogBox;
        public GameObject shanDialogBox;
        public GameObject shaniaDialogBox;
        public GameObject detectiveDialogBox;
        public GameObject defaultDialogBox;

        [Header("Container & Placement")]
        [Tooltip("Optional parent container for instantiated dialogue boxes. If null, parents to Canvas or this transform.")]
        public Transform dialogBoxContainer;

        [Header("Custom Dialogue Appearance")]
        [SerializeField, Tooltip("Paper surface color for dialogue panels; no baked character banner is needed.")]
        private Color dialoguePanelColor = new Color(0.98f, 0.97f, 0.93f, 0.86f);
        [SerializeField, Tooltip("Color of dialogue text and dynamically displayed speaker names.")]
        private Color dialogueInkColor = new Color(0.06f, 0.04f, 0.02f, 1f);
        [SerializeField, Tooltip("Width and height of the custom world-space dialogue surface.")]
        private Vector2 dialogueWorldSize = new Vector2(35.69f, 5.2f);
        [SerializeField, Tooltip("Lower-left anchor of the custom Canvas dialogue panel. Lifted above bottom to prevent extending to the lower-most screen edge.")]
        private Vector2 dialoguePanelAnchorMin = new Vector2(0.14f, 0.035f);
        [SerializeField, Tooltip("Upper-right anchor of the custom Canvas dialogue panel, limiting panel height.")]
        private Vector2 dialoguePanelAnchorMax = new Vector2(0.86f, 0.185f);

        [Header("Typewriter Settings")]
        public float charactersPerSecond = 35f;

        /// <summary>Global state indicating whether the dialogue window is currently visible and active.</summary>
        public static bool IsDialogueOpen { get; private set; } = false;

        /// <summary>Whether the current dialogue node allows contradiction challenges.</summary>
        public bool isCurrentNodeChallengeable { get; private set; } = false;
        public bool IsCurrentNodeChallengeable => isCurrentNodeChallengeable;

        /// <summary>Whether a failed challenge reaction is currently being displayed.</summary>
        public bool isShowingFailureReaction { get; private set; } = false;
        public bool IsShowingFailureReaction => isShowingFailureReaction;

        /// <summary>Whether the typewriter is currently animating text.</summary>
        public bool IsTyping => isTyping;

        private Coroutine typewriterCoroutine;
        private bool isTyping = false;
        private string currentFullText = "";
        [Header("Scene Dialogue Presentation")]
        [SerializeField] private Image customDialoguePanel;
        [SerializeField] private TextMeshProUGUI customDialogueBody;
        [SerializeField] private TextMeshProUGUI customDialogueSpeaker;

        private bool _isSubscribed = false;
        private bool hiddenForEvidenceSelection;

        // Active dialogue box state
        private GameObject _activeDialogBox;
        private TextMeshProUGUI _activeBoxTmpText;
        private Text _activeBoxLegacyText;
        private Image _panelImage;

        private readonly Dictionary<GameObject, GameObject> _instantiatedBoxes = new Dictionary<GameObject, GameObject>();
        private readonly Dictionary<GameObject, TextMeshProUGUI> _boxTmpTextCache = new Dictionary<GameObject, TextMeshProUGUI>();

        private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();
        private PointerEventData _pointerEventData;

        /// <summary>
        /// Clears placeholder text and ensures dialogue references are initialized.
        /// </summary>
        private void Awake()
        {
            _instance = this;
            IsDialogueOpen = false;

            _panelImage = GetComponent<Image>();

            if (speakerNameText != null) speakerNameText.text = "";
            if (dialogueBodyText != null) dialogueBodyText.text = "";
            if (tmpSpeakerNameText != null) tmpSpeakerNameText.text = "";
            if (tmpDialogueText != null) tmpDialogueText.text = "";

            SetNextButtonInteractable(false);
            if (evidencePickerContainer != null) evidencePickerContainer.SetActive(false);
            if (challengeHighlight != null) challengeHighlight.SetActive(false);
            if (challengeButton != null) challengeButton.gameObject.SetActive(false);

#if UNITY_EDITOR
            if (vinceDialogBox == null || janeDialogBox == null || paulDialogBox == null || vonnDialogBox == null || shanDialogBox == null || shaniaDialogBox == null)
            {
                AutoWireDialogBoxesInEditor();
            }
#endif

            FindSceneChildDialogBoxes();
            SubscribeToInterrogationEvents();
        }

        /// <summary>
        /// Binds UI button click listeners and subscribes to interrogation manager events.
        /// </summary>
        private void Start()
        {
            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(OnNextButtonClicked);
                nextButton.onClick.AddListener(OnNextButtonClicked);
            }
            if (challengeButton != null)
            {
                challengeButton.onClick.AddListener(OnChallengeButtonClicked);
            }
            if (closeDialogueButton != null)
            {
                closeDialogueButton.onClick.AddListener(OnCloseButtonClicked);
            }

            SubscribeToInterrogationEvents();

            if (evidencePickerContainer != null) evidencePickerContainer.SetActive(false);

            if (InterrogationManager.Instance != null && InterrogationManager.Instance.CurrentNode != null)
            {
                DisplayNode(InterrogationManager.Instance.CurrentNode);
            }
            else if (!IsDialogueOpen)
            {
                HideDialoguePanel();
            }

            if (CaseClosed.Services.GameSettingsService.Instance != null)
            {
                charactersPerSecond = CaseClosed.Services.GameSettingsService.Instance.TextSpeed;
            }

        }

        private int lastDialogueInputFrame = -1;

        private void Update()
        {
            if (!IsDialogueOpen || !gameObject.activeInHierarchy) return;

            if (InterrogationManager.Instance != null && InterrogationManager.Instance.IsChallengeModeActive)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Debug.Log("[UI:Dialogue] Escape key pressed: Cancelling challenge mode.");
                    InterrogationManager.Instance.ToggleChallengeMode(false);
                    return;
                }
            }

            if (CheckAdvanceInput())
            {
                HandleDialogueAdvanceInput();
            }
        }

        /// <summary>
        /// Checks keyboard shortcuts (Space, Enter) and mouse clicks to determine whether advance input occurred.
        /// </summary>
        private bool CheckAdvanceInput()
        {
            bool keyPressed = false;
            bool clickPressed = false;

#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame))
            {
                keyPressed = true;
            }
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                clickPressed = true;
            }
#endif

            if (!keyPressed)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    keyPressed = true;
                }
            }

            if (!clickPressed)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    clickPressed = true;
                }
            }

            if (keyPressed)
            {
                return true;
            }

            if (clickPressed)
            {
                if (IsPointerOverIgnoredUI())
                {
                    return false;
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whether pointer is hovering over specific non-advancing UI elements (Challenge button, Close button, Evidence picker).
        /// Zero allocation implementation.
        /// </summary>
        private bool IsPointerOverIgnoredUI()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null) return false;

            if (_pointerEventData == null)
            {
                _pointerEventData = new PointerEventData(eventSystem);
            }
            else
            {
                _pointerEventData.Reset();
            }

            _pointerEventData.position = GetPointerScreenPosition();
            _raycastResults.Clear();
            eventSystem.RaycastAll(_pointerEventData, _raycastResults);

            for (int i = 0; i < _raycastResults.Count; i++)
            {
                GameObject hit = _raycastResults[i].gameObject;
                if (hit == null) continue;

                if (nextButton != null && (hit == nextButton.gameObject || hit.transform.IsChildOf(nextButton.transform)))
                    return true;
                if (challengeButton != null && (hit == challengeButton.gameObject || hit.transform.IsChildOf(challengeButton.transform)))
                    return true;
                if (closeDialogueButton != null && (hit == closeDialogueButton.gameObject || hit.transform.IsChildOf(closeDialogueButton.transform)))
                    return true;
                if (evidencePickerContainer != null && evidencePickerContainer.activeSelf && (hit == evidencePickerContainer || hit.transform.IsChildOf(evidencePickerContainer.transform)))
                    return true;
            }

            return false;
        }

        private Vector2 GetPointerScreenPosition()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null)
            {
                return mouse.position.ReadValue();
            }
#endif
            return Input.mousePosition;
        }

        /// <summary>
        /// Advances dialogue upon user click or keypress after the current line finishes typing.
        /// If the completed line is not challengeable, calls AdvanceDialogue().
        /// </summary>
        public void HandleDialogueAdvanceInput()
        {
            if (!CanHandleDialogueInput()) return;
            lastDialogueInputFrame = Time.frameCount;

            if (InterrogationManager.Instance != null && InterrogationManager.Instance.IsChallengeModeActive)
            {
                return;
            }

            if (!isCurrentNodeChallengeable || isShowingFailureReaction)
            {
                InterrogationManager.Instance?.AdvanceDialogue();
            }
        }

        private void SubscribeToInterrogationEvents()
        {
            if (_isSubscribed) return;
            if (InterrogationManager.Instance != null)
            {
                InterrogationManager.Instance.OnDialogueNodeDisplayed += DisplayNode;
                InterrogationManager.Instance.OnChallengeModeToggled += UpdateChallengeState;
                InterrogationManager.Instance.OnChallengeResult += HandleChallengeResult;
                InterrogationManager.Instance.OnDialogueClosed += HideDialoguePanel;
                _isSubscribed = true;
            }
        }

        /// <summary>
        /// Updates the typewriter dialogue text speed (characters per second).
        /// </summary>
        public void SetTextSpeed(float speed)
        {
            charactersPerSecond = Mathf.Clamp(speed, 15f, 100f);
        }

        private void OnDisable()
        {
            IsDialogueOpen = false;
            DeactivateAllCharacterDialogBoxes();
        }

        private void OnDestroy()
        {
            IsDialogueOpen = false;
            DeactivateAllCharacterDialogBoxes();
            if (_instance == this) _instance = null;

            if (_isSubscribed && InterrogationManager.Instance != null)
            {
                InterrogationManager.Instance.OnDialogueNodeDisplayed -= DisplayNode;
                InterrogationManager.Instance.OnChallengeModeToggled -= UpdateChallengeState;
                InterrogationManager.Instance.OnChallengeResult -= HandleChallengeResult;
                InterrogationManager.Instance.OnDialogueClosed -= HideDialoguePanel;
                _isSubscribed = false;
            }
        }

        /// <summary>
        /// Displays a dialogue node statement with character-specific speech bubble and typewriter effect.
        /// </summary>
        /// <param name="node">The dialogue node being presented.</param>
        public void DisplayNode(DialogueNode node)
        {
            if (node == null) return;

            lastDialogueInputFrame = Time.frameCount;
            gameObject.SetActive(true);
            IsDialogueOpen = true;
            isShowingFailureReaction = false;
            isCurrentNodeChallengeable = node.isChallengeable;
            SetChallengeButtonText("Challenge");
            ArmPointerController.Instance?.ForceSyncState();

            Debug.Log($"[UI:Dialogue] Displaying node '{node.nodeId}' (Speaker: '{node.speakerName}', Challengeable: {node.isChallengeable})");

            bool isDetective = IsDetectiveSpeaker(node.speakerId, node.speakerName);

            string resolvedSpeakerName = node.speakerName;
            if (isDetective)
            {
                string investigatorName = CaseManager.Instance?.EffectiveInvestigator?.fullName;
                if (!string.IsNullOrWhiteSpace(investigatorName))
                {
                    resolvedSpeakerName = investigatorName;
                }
                else
                {
                    resolvedSpeakerName = !string.IsNullOrWhiteSpace(node.speakerName) ? node.speakerName : "Detective";
                }
            }

            SetSpeakerName(resolvedSpeakerName);

            // Activate corresponding character dialogue box
            string characterKey = ResolveCharacterKey(node.speakerId, node.speakerName, isDetective);
            ActivateDialogBox(characterKey, resolvedSpeakerName);

            if (challengeButton != null)
            {
                challengeButton.gameObject.SetActive(false);
            }

            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            SetNextButtonInteractable(false);
            typewriterCoroutine = StartCoroutine(TypeText(node.statementText));
        }

        /// <summary>
        /// Resolves which dialogue box should be active based on the character key.
        /// </summary>
        public void ActivateDialogBox(string characterKey, string speakerName = "")
        {
            if (customDialoguePanel != null && customDialogueBody != null && customDialogueSpeaker != null)
            {
                DeactivateAllCharacterDialogBoxes();
                customDialogueSpeaker.text = speakerName;
                customDialoguePanel.gameObject.SetActive(true);
                _activeDialogBox = customDialoguePanel.gameObject;
                _activeBoxTmpText = customDialogueBody;
                _activeBoxLegacyText = null;
                return;
            }

            GameObject targetBox = GetDialogBoxForKey(characterKey);

            DeactivateBoxesExcept(targetBox);

            _activeDialogBox = targetBox;
            _activeBoxTmpText = null;
            _activeBoxLegacyText = null;

            if (targetBox != null)
            {
                targetBox.SetActive(true);
                _activeBoxTmpText = GetTextMeshProFromBox(targetBox);
                _activeBoxLegacyText = targetBox.GetComponentInChildren<Text>(true);

                // Optional speaker name text inside the character box
                var boxSpeakerTmp = targetBox.transform.Find("Text_Speaker")?.GetComponent<TextMeshProUGUI>() ??
                                    targetBox.transform.Find("SpeakerName")?.GetComponent<TextMeshProUGUI>();
                if (boxSpeakerTmp != null && !string.IsNullOrEmpty(speakerName))
                {
                    boxSpeakerTmp.text = speakerName;
                }

                // Hide default panel image so the character's speech bubble sprite is cleanly visible
                if (_panelImage != null && targetBox != gameObject)
                {
                    _panelImage.enabled = false;
                }

                // Hide main panel text if character box provides its own text renderer
                if (_activeBoxTmpText != null || _activeBoxLegacyText != null)
                {
                    if (dialogueBodyText != null) dialogueBodyText.enabled = false;
                    if (tmpDialogueText != null) tmpDialogueText.enabled = false;
                }

            }
            else
            {
                // Fallback to default panel background (DialogBLACK) and main text components
                if (_panelImage != null)
                {
                    _panelImage.enabled = true;
                    DetectiveUITheme.Surface(_panelImage, DetectiveUITheme.Paper, true);
                }
                if (dialogueBodyText != null)
                {
                    dialogueBodyText.enabled = true;
                    dialogueBodyText.color = DetectiveUITheme.Ink;
                }
                if (tmpDialogueText != null)
                {
                    tmpDialogueText.enabled = true;
                    tmpDialogueText.color = DetectiveUITheme.Ink;
                }
                if (speakerNameText != null)
                {
                    speakerNameText.color = DetectiveUITheme.WoodDark;
                    speakerNameText.fontStyle = FontStyle.Bold;
                }
                if (tmpSpeakerNameText != null)
                {
                    tmpSpeakerNameText.color = DetectiveUITheme.WoodDark;
                    tmpSpeakerNameText.fontStyle |= FontStyles.Bold | FontStyles.UpperCase;
                }
            }
        }


        /// <summary>
        /// Resolves the dialogue box GameObject for a character key.
        /// </summary>
        public GameObject GetDialogBoxForKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;

            if (string.Equals(key, "Vince", StringComparison.OrdinalIgnoreCase))
                return EnsureInstance(ref vinceDialogBox, "VinceDialog");
            if (string.Equals(key, "Jane", StringComparison.OrdinalIgnoreCase))
                return EnsureInstance(ref janeDialogBox, "JaneDialog");
            if (string.Equals(key, "Paul", StringComparison.OrdinalIgnoreCase))
                return EnsureInstance(ref paulDialogBox, "PaulDialog");
            if (string.Equals(key, "Vonn", StringComparison.OrdinalIgnoreCase))
                return EnsureInstance(ref vonnDialogBox, "VonnDialog");
            if (string.Equals(key, "Shan", StringComparison.OrdinalIgnoreCase))
                return EnsureInstance(ref shanDialogBox, "ShanDialog");
            if (string.Equals(key, "Shania", StringComparison.OrdinalIgnoreCase))
                return EnsureInstance(ref shaniaDialogBox, "ShaniaDialog");
            if (string.Equals(key, "Detective", StringComparison.OrdinalIgnoreCase))
            {
                if (detectiveDialogBox != null)
                    return EnsureInstance(ref detectiveDialogBox, "DetectiveDialogMessage");
                if (defaultDialogBox != null)
                    return EnsureInstance(ref defaultDialogBox, "DialogBLACK");
                return EnsureInstance(ref detectiveDialogBox, "DetectiveDialogMessage");
            }

            if (characterDialogBoxes != null)
            {
                for (int i = 0; i < characterDialogBoxes.Count; i++)
                {
                    var entry = characterDialogBoxes[i];
                    if (entry != null && string.Equals(entry.characterKey, key, StringComparison.OrdinalIgnoreCase))
                    {
                        var box = EnsureInstance(ref entry.dialogueBox, entry.characterKey + "Dialog");
                        return box;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether a speaker is the Detective / Player.
        /// </summary>
        public bool IsDetectiveSpeaker(string speakerId, string speakerName)
        {
            if (string.Equals(speakerName, "Detective", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(speakerId, "Detective", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(speakerId, "PLAYER", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(speakerId, "Investigator", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (CaseManager.Instance?.EffectiveInvestigator != null)
            {
                string invName = CaseManager.Instance.EffectiveInvestigator.fullName;
                if (!string.IsNullOrEmpty(invName) && !string.IsNullOrEmpty(speakerName) &&
                    (speakerName.IndexOf(invName, StringComparison.OrdinalIgnoreCase) >= 0 ||
                     invName.IndexOf(speakerName, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines character key from node speakerId and speakerName.
        /// </summary>
        public string ResolveCharacterKey(string speakerId, string speakerName, bool isDetective)
        {
            if (isDetective) return "Detective";

            string id = speakerId ?? "";
            string name = speakerName ?? "";

            if (characterDialogBoxes != null)
            {
                for (int i = 0; i < characterDialogBoxes.Count; i++)
                {
                    var entry = characterDialogBoxes[i];
                    if (entry != null && !string.IsNullOrEmpty(entry.characterKey))
                    {
                        if (ContainsIgnoreCase(id, entry.characterKey) || ContainsIgnoreCase(name, entry.characterKey))
                        {
                            return entry.characterKey;
                        }
                    }
                }
            }

            // Check Shania before Shan because Shan is a substring of Shania
            if (ContainsIgnoreCase(id, "shania") || ContainsIgnoreCase(name, "shania"))
                return "Shania";
            if (ContainsIgnoreCase(id, "shan") || ContainsIgnoreCase(name, "shan"))
                return "Shan";
            if (ContainsIgnoreCase(id, "vince") || ContainsIgnoreCase(name, "vince") ||
                ContainsIgnoreCase(id, "batecan") || ContainsIgnoreCase(name, "batecan"))
                return "Vince";
            if (ContainsIgnoreCase(id, "jane") || ContainsIgnoreCase(name, "jane") ||
                ContainsIgnoreCase(id, "reyes") || ContainsIgnoreCase(name, "reyes"))
                return "Jane";
            if (ContainsIgnoreCase(id, "paul") || ContainsIgnoreCase(name, "paul") ||
                ContainsIgnoreCase(id, "camacho") || ContainsIgnoreCase(name, "camacho"))
                return "Paul";
            if (ContainsIgnoreCase(id, "vonn") || ContainsIgnoreCase(name, "vonn") ||
                ContainsIgnoreCase(id, "charl") || ContainsIgnoreCase(name, "charl") ||
                ContainsIgnoreCase(id, "pascual") || ContainsIgnoreCase(name, "pascual"))
                return "Vonn";

            // Fallback to active suspect profile
            if (InterrogationManager.Instance != null && InterrogationManager.Instance.CurrentSuspect != null)
            {
                string suspectName = InterrogationManager.Instance.CurrentSuspect.fullName ?? "";
                string suspectId = InterrogationManager.Instance.CurrentSuspect.characterId ?? "";

                if (ContainsIgnoreCase(suspectId, "shania") || ContainsIgnoreCase(suspectName, "shania"))
                    return "Shania";
                if (ContainsIgnoreCase(suspectId, "shan") || ContainsIgnoreCase(suspectName, "shan"))
                    return "Shan";
                if (ContainsIgnoreCase(suspectId, "vince") || ContainsIgnoreCase(suspectName, "vince") ||
                    ContainsIgnoreCase(suspectId, "batecan") || ContainsIgnoreCase(suspectName, "batecan"))
                    return "Vince";
                if (ContainsIgnoreCase(suspectId, "jane") || ContainsIgnoreCase(suspectName, "jane") ||
                    ContainsIgnoreCase(suspectId, "reyes") || ContainsIgnoreCase(suspectName, "reyes"))
                    return "Jane";
                if (ContainsIgnoreCase(suspectId, "paul") || ContainsIgnoreCase(suspectName, "paul") ||
                    ContainsIgnoreCase(suspectId, "camacho") || ContainsIgnoreCase(suspectName, "camacho"))
                    return "Paul";
                if (ContainsIgnoreCase(suspectId, "vonn") || ContainsIgnoreCase(suspectName, "vonn") ||
                    ContainsIgnoreCase(suspectId, "charl") || ContainsIgnoreCase(suspectName, "charl") ||
                    ContainsIgnoreCase(suspectId, "pascual") || ContainsIgnoreCase(suspectName, "pascual"))
                    return "Vonn";
            }

            return "";
        }

        private static bool ContainsIgnoreCase(string source, string target)
        {
            return source != null && target != null && source.IndexOf(target, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Retrieves or instantiates a dialogue box instance from a prefab or scene reference.
        /// </summary>
        private GameObject EnsureInstance(ref GameObject boxRef, string fallbackName)
        {
            if (boxRef == null)
            {
                TryAssignFromHierarchy(ref boxRef, fallbackName);
#if UNITY_EDITOR
                if (boxRef == null)
                {
                    string path = $"Assets/Prefabs/UI/Dialog Boxes/{fallbackName}.prefab";
                    boxRef = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (boxRef == null && (fallbackName == "DetectiveDialogMessage" || fallbackName == "DetectiveDialog"))
                    {
                        boxRef = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Dialog Boxes/DetectiveDialogMessage.prefab") ??
                                 UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Panels/DialogMessage.prefab");
                    }
                }
#endif
            }

            if (boxRef == null) return null;

            if (boxRef.scene.IsValid())
            {
                ConfigureBoxComponents(boxRef);
                return boxRef;
            }

            if (_instantiatedBoxes.TryGetValue(boxRef, out GameObject existingInst) && existingInst != null)
            {
                return existingInst;
            }

            Transform parentTransform = dialogBoxContainer != null
                ? dialogBoxContainer
                : (transform.parent != null && transform.parent.GetComponent<Canvas>() != null ? transform.parent : transform);

            GameObject newInst = Instantiate(boxRef, parentTransform, false);
            newInst.name = boxRef.name;
            ConfigureBoxComponents(newInst);
            newInst.SetActive(false);
            _instantiatedBoxes[boxRef] = newInst;
            return newInst;
        }

        /// <summary>
        /// Ensures the dialogue box has proper Canvas sorting order so text renders above the speech bubble sprite.
        /// </summary>
        private void ConfigureBoxComponents(GameObject box)
        {
            if (box == null) return;

            Canvas boxCanvas = box.GetComponent<Canvas>();
            if (boxCanvas == null)
            {
                boxCanvas = box.AddComponent<Canvas>();
            }
            boxCanvas.overrideSorting = true;
            boxCanvas.sortingOrder = 30;

            if (box.GetComponent<GraphicRaycaster>() == null)
            {
                box.AddComponent<GraphicRaycaster>();
            }
        }

        /// <summary>
        /// Searches child GameObjects for the TMPro.TextMeshProUGUI text component.
        /// </summary>
        public TextMeshProUGUI GetTextMeshProFromBox(GameObject box)
        {
            if (box == null) return null;

            if (_boxTmpTextCache.TryGetValue(box, out TextMeshProUGUI cached) && cached != null)
            {
                return cached;
            }

            if (characterDialogBoxes != null)
            {
                for (int i = 0; i < characterDialogBoxes.Count; i++)
                {
                    var entry = characterDialogBoxes[i];
                    if (entry != null && entry.dialogueBox == box && entry.dialogueText != null)
                    {
                        _boxTmpTextCache[box] = entry.dialogueText;
                        return entry.dialogueText;
                    }
                }
            }

            Transform msg = box.transform.Find("DialogMessage");
            if (msg != null)
            {
                TextMeshProUGUI tmp = msg.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    _boxTmpTextCache[box] = tmp;
                    return tmp;
                }
            }

            TextMeshProUGUI inChildren = box.GetComponentInChildren<TextMeshProUGUI>(true);
            if (inChildren != null)
            {
                _boxTmpTextCache[box] = inChildren;
                return inChildren;
            }

            return null;
        }

        private void DeactivateBoxesExcept(GameObject keepBox)
        {
            DeactivateIfDifferent(vinceDialogBox, keepBox);
            DeactivateIfDifferent(janeDialogBox, keepBox);
            DeactivateIfDifferent(paulDialogBox, keepBox);
            DeactivateIfDifferent(vonnDialogBox, keepBox);
            DeactivateIfDifferent(shanDialogBox, keepBox);
            DeactivateIfDifferent(shaniaDialogBox, keepBox);
            DeactivateIfDifferent(detectiveDialogBox, keepBox);
            DeactivateIfDifferent(defaultDialogBox, keepBox);

            if (characterDialogBoxes != null)
            {
                for (int i = 0; i < characterDialogBoxes.Count; i++)
                {
                    DeactivateIfDifferent(characterDialogBoxes[i]?.dialogueBox, keepBox);
                }
            }

            foreach (var kvp in _instantiatedBoxes)
            {
                DeactivateIfDifferent(kvp.Value, keepBox);
            }
        }

        private void DeactivateIfDifferent(GameObject box, GameObject keepBox)
        {
            if (box != null && box != keepBox && box.scene.IsValid() && box.activeSelf)
            {
                box.SetActive(false);
            }
        }

        /// <summary>
        /// Hides all active character dialogue boxes.
        /// </summary>
        public void DeactivateAllCharacterDialogBoxes()
        {
            if (customDialoguePanel != null) customDialoguePanel.gameObject.SetActive(false);
            DeactivateBoxIfActive(vinceDialogBox);
            DeactivateBoxIfActive(janeDialogBox);
            DeactivateBoxIfActive(paulDialogBox);
            DeactivateBoxIfActive(vonnDialogBox);
            DeactivateBoxIfActive(shanDialogBox);
            DeactivateBoxIfActive(shaniaDialogBox);
            DeactivateBoxIfActive(detectiveDialogBox);
            DeactivateBoxIfActive(defaultDialogBox);

            if (characterDialogBoxes != null)
            {
                for (int i = 0; i < characterDialogBoxes.Count; i++)
                {
                    DeactivateBoxIfActive(characterDialogBoxes[i]?.dialogueBox);
                }
            }

            foreach (var kvp in _instantiatedBoxes)
            {
                DeactivateBoxIfActive(kvp.Value);
            }

            _activeBoxTmpText = null;
            _activeBoxLegacyText = null;
            _activeDialogBox = null;
        }

        private void DeactivateBoxIfActive(GameObject box)
        {
            if (box != null && box.scene.IsValid() && box.activeSelf)
            {
                box.SetActive(false);
            }
        }

        private void FindSceneChildDialogBoxes()
        {
            TryAssignFromHierarchy(ref vinceDialogBox, "VinceDialog");
            TryAssignFromHierarchy(ref janeDialogBox, "JaneDialog");
            TryAssignFromHierarchy(ref paulDialogBox, "PaulDialog");
            TryAssignFromHierarchy(ref vonnDialogBox, "VonnDialog");
            TryAssignFromHierarchy(ref shanDialogBox, "ShanDialog");
            TryAssignFromHierarchy(ref shaniaDialogBox, "ShaniaDialog");
            TryAssignFromHierarchy(ref detectiveDialogBox, "DetectiveDialogMessage");
            TryAssignFromHierarchy(ref detectiveDialogBox, "DetectiveDialogueMessage");
            TryAssignFromHierarchy(ref detectiveDialogBox, "DialogMessage");
            TryAssignFromHierarchy(ref detectiveDialogBox, "DetectiveDialog");
            TryAssignFromHierarchy(ref detectiveDialogBox, "DialogBLACK");
        }

        private void TryAssignFromHierarchy(ref GameObject boxRef, string name)
        {
            if (boxRef != null && boxRef.scene.IsValid()) return;

            Transform found = transform.Find(name);
            if (found == null && transform.parent != null)
            {
                found = transform.parent.Find(name);
            }
            if (found != null)
            {
                boxRef = found.gameObject;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            AutoWireDialogBoxesInEditor();
        }

        public void AutoWireDialogBoxesInEditor()
        {
            if (vinceDialogBox == null)
                vinceDialogBox = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Dialog Boxes/VinceDialog.prefab");
            if (janeDialogBox == null)
                janeDialogBox = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Dialog Boxes/JaneDialog.prefab");
            if (paulDialogBox == null)
                paulDialogBox = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Dialog Boxes/PaulDialog.prefab");
            if (vonnDialogBox == null)
                vonnDialogBox = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Dialog Boxes/VonnDialog.prefab");
            if (shanDialogBox == null)
                shanDialogBox = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Dialog Boxes/ShanDialog.prefab");
            if (shaniaDialogBox == null)
                shaniaDialogBox = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Dialog Boxes/ShaniaDialog.prefab");
            if (detectiveDialogBox == null)
                detectiveDialogBox = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Dialog Boxes/DetectiveDialogMessage.prefab") ??
                                     UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Panels/DialogMessage.prefab");
        }
#endif

        private void SetSpeakerName(string name)
        {
            if (speakerNameText != null) speakerNameText.text = name;
            if (tmpSpeakerNameText != null) tmpSpeakerNameText.text = name;
        }

        private void SetBodyText(string text)
        {
            if (dialogueBodyText != null) dialogueBodyText.text = text;
            if (tmpDialogueText != null)
            {
                tmpDialogueText.maxVisibleCharacters = int.MaxValue;
                tmpDialogueText.text = text;
            }
            if (_activeBoxTmpText != null && _activeBoxTmpText != tmpDialogueText)
            {
                _activeBoxTmpText.text = text;
                _activeBoxTmpText.maxVisibleCharacters = int.MaxValue;
            }
            if (_activeBoxLegacyText != null && _activeBoxLegacyText != dialogueBodyText)
            {
                _activeBoxLegacyText.text = text;
            }
        }

        /// <summary>
        /// Places the optional bubble above a world-space evidence item.
        /// </summary>
        public void AlignToWorldTarget(Transform target)
        {
            if (target == null || bubbleRect == null) return;

            RectTransform targetRect = bubbleRect;
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
        /// Parses TMP text once and reveals characters using unscaled elapsed time.
        /// </summary>
        private IEnumerator TypeText(string text)
        {
            isTyping = true;
            SetNextButtonInteractable(false);
            if (challengeButton != null)
            {
                challengeButton.gameObject.SetActive(false);
            }
            currentFullText = text ?? "";
            SetBodyText(currentFullText);
            TextMeshProUGUI body = _activeBoxTmpText;
            if (body == null) body = tmpDialogueText;
            int characterCount = currentFullText.Length;
            if (body != null)
            {
                body.ForceMeshUpdate(true);
                characterCount = body.textInfo.characterCount;
            }
            SetVisibleBodyCharacters(0, body != null);
            AudioManager audio = AudioManager.Instance;
            float elapsed = 0f;
            int visibleCharacters = 0;
            while (visibleCharacters < characterCount)
            {
                elapsed += Time.unscaledDeltaTime;
                int nextVisible = Mathf.Min(characterCount, Mathf.FloorToInt(elapsed * Mathf.Max(1f, charactersPerSecond)));
                if (nextVisible > visibleCharacters)
                {
                    SetVisibleBodyCharacters(nextVisible, body != null);
                    if (audio != null && nextVisible / 3 > visibleCharacters / 3) audio.PlayTypewriterKey();
                    visibleCharacters = nextVisible;
                }
                yield return null;
            }
            SetBodyText(currentFullText);
            isTyping = false;
            EnableCompletedLineInteraction();
        }

        private void SetVisibleBodyCharacters(int count, bool hasTmpBody)
        {
            if (tmpDialogueText != null) tmpDialogueText.maxVisibleCharacters = count;
            if (_activeBoxTmpText != null) _activeBoxTmpText.maxVisibleCharacters = count;

            // Retain support for scenes that still use the legacy Text component.
            if (dialogueBodyText == null && _activeBoxLegacyText == null) return;
            if (hasTmpBody && _activeBoxLegacyText == null &&
                (dialogueBodyText == null || !dialogueBodyText.isActiveAndEnabled)) return;
            string visibleText = currentFullText.Substring(0, Mathf.Min(count, currentFullText.Length));
            if (dialogueBodyText != null) dialogueBodyText.text = visibleText;
            if (_activeBoxLegacyText != null) _activeBoxLegacyText.text = visibleText;
        }

        /// <summary>
        /// Instantly finishes typing out the current dialogue text, enabling interaction and challenge buttons.
        /// </summary>
        public void CompleteTypingImmediately()
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }

            SetBodyText(currentFullText);

            isTyping = false;
            EnableCompletedLineInteraction();
        }

        private void EnableCompletedLineInteraction()
        {
            SetNextButtonInteractable(true);
            bool canChallenge = isCurrentNodeChallengeable && !isShowingFailureReaction;

            if (challengeButton != null)
            {
                challengeButton.gameObject.SetActive(canChallenge);
                challengeButton.interactable = canChallenge;
            }
            else if (canChallenge && InterrogationManager.Instance != null && !InterrogationManager.Instance.IsChallengeModeActive)
            {
                // Character-specific dialogue boxes have no challenge button. Once a
                // challengeable line is readable, immediately enable evidence selection.
                InterrogationManager.Instance.ToggleChallengeMode(true);
            }
        }

        private void SetNextButtonInteractable(bool interactable)
        {
            if (nextButton != null) nextButton.interactable = interactable;
        }

        private bool CanHandleDialogueInput()
        {
            return IsDialogueOpen && !isTyping && lastDialogueInputFrame != Time.frameCount &&
                (EvidenceManager.Instance == null || !EvidenceManager.Instance.isInspectingModalOpen);
        }

        /// <summary>
        /// Handles next button clicks after the current line has finished typing.
        /// </summary>
        private void OnNextButtonClicked()
        {
            if (!CanHandleDialogueInput()) return;
            lastDialogueInputFrame = Time.frameCount;
            Debug.Log($"[UI:Dialogue] Next button clicked (IsTyping: {isTyping})");

            if (InterrogationManager.Instance != null && InterrogationManager.Instance.IsChallengeModeActive)
            {
                return;
            }

            InterrogationManager.Instance?.AdvanceDialogue();
        }

        /// <summary>
        /// Handles challenge button click, toggling challenge mode in the interrogation controller.
        /// </summary>
        private void OnChallengeButtonClicked()
        {
            bool currentState = InterrogationManager.Instance != null && InterrogationManager.Instance.IsChallengeModeActive;
            bool newState = !currentState;
            Debug.Log($"[UI:Dialogue] Challenge button clicked (Switching to: {newState})");
            InterrogationManager.Instance?.ToggleChallengeMode(newState);
        }

        /// <summary>
        /// Hides dialogue during evidence selection and restores the statement when selection is cancelled.
        /// </summary>
        /// <param name="isActive">Whether challenge mode is currently enabled.</param>
        private void UpdateChallengeState(bool isActive)
        {
            if (challengeHighlight != null) challengeHighlight.SetActive(isActive);
            if (evidencePickerContainer != null) evidencePickerContainer.SetActive(false);

            SetChallengeButtonText(isActive ? "Cancel" : "Challenge");

            if (isActive)
            {
                if (isTyping) CompleteTypingImmediately();
                hiddenForEvidenceSelection = true;
                DeactivateAllCharacterDialogBoxes();
                gameObject.SetActive(false);
            }
            else if (hiddenForEvidenceSelection)
            {
                hiddenForEvidenceSelection = false;
                InterrogationManager manager = InterrogationManager.Instance;
                if (manager != null && manager.CurrentNode != null)
                {
                    DisplayNode(manager.CurrentNode);
                    if (typewriterCoroutine != null)
                    {
                        StopCoroutine(typewriterCoroutine);
                        typewriterCoroutine = null;
                    }
                    SetBodyText(currentFullText);
                    isTyping = false;
                    SetNextButtonInteractable(true);
                    if (challengeButton != null)
                    {
                        challengeButton.gameObject.SetActive(isCurrentNodeChallengeable);
                        challengeButton.interactable = isCurrentNodeChallengeable;
                    }
                }
            }

            ArmPointerController.Instance?.ForceSyncState();
        }

        private void SetChallengeButtonText(string text)
        {
            if (challengeButton == null) return;
            string upper = !string.IsNullOrEmpty(text) ? text.ToUpperInvariant() : "";
            TextMeshProUGUI tmpLabel = challengeButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmpLabel != null)
            {
                tmpLabel.text = upper;
                return;
            }

            Text label = challengeButton.GetComponentInChildren<Text>(true);
            if (label != null) label.text = upper;
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
        /// Hides the dialogue panel and all character speech bubbles when dialogue finishes or is closed.
        /// </summary>
        public void HideDialoguePanel()
        {
            hiddenForEvidenceSelection = false;
            IsDialogueOpen = false;
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }
            isTyping = false;
            isCurrentNodeChallengeable = false;
            isShowingFailureReaction = false;
            SetNextButtonInteractable(false);
            if (challengeButton != null)
            {
                challengeButton.gameObject.SetActive(false);
                SetChallengeButtonText("Challenge");
            }
            if (evidencePickerContainer != null) evidencePickerContainer.SetActive(false);

            DeactivateAllCharacterDialogBoxes();

            gameObject.SetActive(false);
            ArmPointerController.Instance?.ForceSyncState();
        }

        /// <summary>
        /// Displays the reaction dialogue and updates the speaker name following a challenge attempt.
        /// Activates the current suspect's character speech bubble.
        /// </summary>
        /// <param name="success">Whether the challenge exposed a contradiction.</param>
        /// <param name="reactionMessage">The reaction dialogue text returned from the challenge.</param>
        private void HandleChallengeResult(bool success, string reactionMessage)
        {
            hiddenForEvidenceSelection = false;
            IsDialogueOpen = true;
            gameObject.SetActive(true);
            isShowingFailureReaction = !success;
            isCurrentNodeChallengeable = false;
            ArmPointerController.Instance?.ForceSyncState();
            Debug.Log($"[UI:Dialogue] Received challenge result (Success: {success}, FailureReaction: {isShowingFailureReaction}, MessageLength: {reactionMessage?.Length ?? 0})");

            CharacterProfileSO suspect = InterrogationManager.Instance?.CurrentSuspect;
            string speakerName = suspect != null && !string.IsNullOrEmpty(suspect.fullName) ? suspect.fullName : "Suspect";
            SetSpeakerName(speakerName);

            string characterKey = ResolveCharacterKey(suspect?.characterId, speakerName, false);
            ActivateDialogBox(characterKey, speakerName);

            if (challengeButton != null)
            {
                challengeButton.gameObject.SetActive(false);
                SetChallengeButtonText("Challenge");
            }

            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            SetNextButtonInteractable(false);
            typewriterCoroutine = StartCoroutine(TypeText(reactionMessage));
        }
    }
}
