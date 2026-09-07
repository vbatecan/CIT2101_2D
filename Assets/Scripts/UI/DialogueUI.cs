using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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
        private readonly StringBuilder _typewriterBuilder = new StringBuilder(512);

        private bool _isSubscribed = false;

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
            if (nextButton != null) nextButton.onClick.AddListener(OnNextButtonClicked);
            if (challengeButton != null) challengeButton.onClick.AddListener(OnChallengeButtonClicked);
            if (closeDialogueButton != null) closeDialogueButton.onClick.AddListener(OnCloseButtonClicked);

            SubscribeToInterrogationEvents();

            if (evidencePickerContainer != null) evidencePickerContainer.SetActive(false);

            if (InterrogationManager.Instance != null && InterrogationManager.Instance.currentNode != null)
            {
                DisplayNode(InterrogationManager.Instance.currentNode);
            }
            else if (!IsDialogueOpen)
            {
                HideDialoguePanel();
            }

            if (CaseClosed.Services.GameSettingsService.Instance != null)
            {
                charactersPerSecond = CaseClosed.Services.GameSettingsService.Instance.TextSpeed;
            }

            UIButtonHighlightSystem.ApplyToHierarchy(gameObject);
        }

        private void Update()
        {
            if (!IsDialogueOpen || !gameObject.activeInHierarchy) return;

            if (InterrogationManager.Instance != null && InterrogationManager.Instance.isChallengeModeActive)
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
        /// Advances dialogue or accelerates typewriter animation upon user click or keypress.
        /// If typing, completes text immediately.
        /// If typing is done and not challengeable, calls AdvanceDialogue().
        /// </summary>
        public void HandleDialogueAdvanceInput()
        {
            if (isTyping)
            {
                CompleteTypingImmediately();
                return;
            }

            if (InterrogationManager.Instance != null && InterrogationManager.Instance.isChallengeModeActive)
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

            IsDialogueOpen = true;
            gameObject.SetActive(true);
            isShowingFailureReaction = false;
            isCurrentNodeChallengeable = node.isChallengeable;
            SetChallengeButtonText("Challenge");
            ArmPointerController.Instance?.ForceSyncState();

            Debug.Log($"[UI:Dialogue] Displaying node '{node.nodeId}' (Speaker: '{node.speakerName}', Challengeable: {node.isChallengeable})");

            bool isDetective = IsDetectiveSpeaker(node.speakerId, node.speakerName);

            string resolvedSpeakerName = node.speakerName;
            if (isDetective)
            {
                string investigatorName = CaseManager.Instance?.selectedInvestigator?.fullName;
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
                }
                if (dialogueBodyText != null) dialogueBodyText.enabled = true;
                if (tmpDialogueText != null) tmpDialogueText.enabled = true;
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

            if (CaseManager.Instance?.selectedInvestigator != null)
            {
                string invName = CaseManager.Instance.selectedInvestigator.fullName;
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
            if (InterrogationManager.Instance != null && InterrogationManager.Instance.currentSuspect != null)
            {
                string suspectName = InterrogationManager.Instance.currentSuspect.fullName ?? "";
                string suspectId = InterrogationManager.Instance.currentSuspect.characterId ?? "";

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
            if (tmpDialogueText != null) tmpDialogueText.text = text;
            if (_activeBoxTmpText != null && _activeBoxTmpText != tmpDialogueText)
            {
                _activeBoxTmpText.text = text;
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
        /// Zero garbage collection using preallocated StringBuilder.
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
            SetBodyText("");

            _typewriterBuilder.Clear();
            float delay = 1f / Mathf.Max(1f, charactersPerSecond);
            for (int i = 0; i < currentFullText.Length; i++)
            {
                _typewriterBuilder.Append(currentFullText[i]);
                SetBodyText(_typewriterBuilder.ToString());
                if (i % 3 == 0) AudioManager.Instance?.PlayTypewriterKey();
                yield return new WaitForSeconds(delay);
            }

            isTyping = false;
            SetNextButtonInteractable(true);
            if (challengeButton != null)
            {
                bool showChallenge = isCurrentNodeChallengeable && !isShowingFailureReaction;
                challengeButton.gameObject.SetActive(showChallenge);
                challengeButton.interactable = showChallenge;
            }
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
            SetNextButtonInteractable(true);
            if (challengeButton != null)
            {
                bool showChallenge = isCurrentNodeChallengeable && !isShowingFailureReaction;
                challengeButton.gameObject.SetActive(showChallenge);
                challengeButton.interactable = showChallenge;
            }
        }

        private void SetNextButtonInteractable(bool interactable)
        {
            if (nextButton != null) nextButton.interactable = interactable;
        }

        /// <summary>
        /// Handles next button click after the current line has finished printing or to skip typing.
        /// </summary>
        private void OnNextButtonClicked()
        {
            Debug.Log($"[UI:Dialogue] Next button clicked (IsTyping: {isTyping})");

            if (isTyping)
            {
                CompleteTypingImmediately();
                return;
            }

            if (InterrogationManager.Instance != null && InterrogationManager.Instance.isChallengeModeActive)
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
            bool currentState = InterrogationManager.Instance != null && InterrogationManager.Instance.isChallengeModeActive;
            bool newState = !currentState;
            Debug.Log($"[UI:Dialogue] Challenge button clicked (Switching to: {newState})");
            InterrogationManager.Instance?.ToggleChallengeMode(newState);
        }

        /// <summary>
        /// Updates the visual challenge state highlight, toggles challenge button text, and syncs the arm pointer.
        /// </summary>
        /// <param name="isActive">Whether challenge mode is currently enabled.</param>
        private void UpdateChallengeState(bool isActive)
        {
            if (challengeHighlight != null) challengeHighlight.SetActive(isActive);
            if (evidencePickerContainer != null) evidencePickerContainer.SetActive(false);

            SetChallengeButtonText(isActive ? "Cancel" : "Challenge");

            ArmPointerController.Instance?.ForceSyncState();
        }

        private void SetChallengeButtonText(string text)
        {
            if (challengeButton == null) return;
            var tmp = challengeButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
            {
                tmp.text = text;
                return;
            }
            var legacy = challengeButton.GetComponentInChildren<Text>(true);
            if (legacy != null)
            {
                legacy.text = text;
            }
        }

        /// <summary>
        /// Populates the evidence picker grid with clickable buttons representing all discovered evidence items.
        /// Supports prefab instantiation with zero-GC fallbacks.
        /// </summary>
        private void PopulateEvidencePicker()
        {
            if (evidencePickerGrid == null) return;

            foreach (Transform child in evidencePickerGrid)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }

            var discoveredIds = CaseManager.Instance?.discoveredEvidenceIds;
            var activeCase = CaseManager.Instance?.activeCase;

            if (activeCase == null || discoveredIds == null) return;

            foreach (var ev in activeCase.evidenceItems)
            {
                if (ev != null && discoveredIds.Contains(ev.id))
                {
                    EvidenceSO currentEv = ev;
                    bool prefabSuccess = false;

                    if (evidencePickerItemPrefab != null)
                    {
                        GameObject itemObj = Instantiate(evidencePickerItemPrefab, evidencePickerGrid, false);
                        itemObj.name = $"Present_{currentEv.id}";

                        Button btn = itemObj.GetComponent<Button>() ?? itemObj.GetComponentInChildren<Button>();
                        Text titleText = itemObj.GetComponent<Text>() ?? itemObj.GetComponentInChildren<Text>();

                        Image iconImg = null;
                        Image[] images = itemObj.GetComponentsInChildren<Image>(true);
                        foreach (var img in images)
                        {
                            if (img.gameObject.name.IndexOf("icon", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                iconImg = img;
                                break;
                            }
                        }

                        if (iconImg == null && btn != null && btn.targetGraphic is Image targetGraphicImg)
                        {
                            foreach (var img in images)
                            {
                                if (img != targetGraphicImg)
                                {
                                    iconImg = img;
                                    break;
                                }
                            }
                        }

                        if (iconImg == null && images.Length > 0)
                        {
                            iconImg = images[0];
                        }

                        if (btn != null && iconImg != null && titleText != null)
                        {
                            if (currentEv.normalSprite != null)
                            {
                                iconImg.sprite = currentEv.normalSprite;
                                iconImg.enabled = true;
                            }

                            titleText.text = currentEv.evidenceName;

                            btn.onClick.AddListener(() =>
                            {
                                Debug.Log($"[UI:Dialogue] Evidence picker selected item '{currentEv.evidenceName}' (ID: {currentEv.id}) to present");
                                InterrogationManager.Instance?.PresentEvidenceToChallenge(currentEv);
                            });

                            prefabSuccess = true;
                        }
                        else
                        {
                            if (Application.isPlaying)
                                Destroy(itemObj);
                            else
                                DestroyImmediate(itemObj);
                        }
                    }

                    if (!prefabSuccess)
                    {
                        GameObject btnObj = new GameObject($"Present_{currentEv.id}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                        btnObj.transform.SetParent(evidencePickerGrid, false);

                        Image img = btnObj.GetComponent<Image>();
                        if (currentEv.normalSprite != null) img.sprite = currentEv.normalSprite;

                        btnObj.GetComponent<Button>().onClick.AddListener(() =>
                        {
                            Debug.Log($"[UI:Dialogue] Evidence picker selected item '{currentEv.evidenceName}' (ID: {currentEv.id}) to present");
                            InterrogationManager.Instance?.PresentEvidenceToChallenge(currentEv);
                        });
                    }
                }
            }

            if (evidencePickerContainer != null)
            {
                UIButtonHighlightSystem.ApplyToHierarchy(evidencePickerContainer);
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
        /// Hides the dialogue panel and all character speech bubbles when dialogue finishes or is closed.
        /// </summary>
        public void HideDialoguePanel()
        {
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
            IsDialogueOpen = true;
            gameObject.SetActive(true);
            isShowingFailureReaction = !success;
            isCurrentNodeChallengeable = false;
            ArmPointerController.Instance?.ForceSyncState();
            Debug.Log($"[UI:Dialogue] Received challenge result (Success: {success}, FailureReaction: {isShowingFailureReaction}, MessageLength: {reactionMessage?.Length ?? 0})");

            CharacterProfileSO suspect = InterrogationManager.Instance?.currentSuspect;
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
