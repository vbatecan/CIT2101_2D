using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    public class DialogueUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public Text speakerNameText;
        public Text dialogueBodyText;
        public Button nextButton;
        public Button challengeButton;
        public GameObject challengeHighlight;

        [Header("Evidence Selection Overlay (Presenting Evidence)")]
        public GameObject evidencePickerContainer;
        public Transform evidencePickerGrid;
        public GameObject evidencePickerItemPrefab;

        [Header("Typewriter Settings")]
        public float charactersPerSecond = 35f;

        private Coroutine typewriterCoroutine;
        private bool isTyping = false;
        private string currentFullText = "";

        private void Start()
        {
            if (nextButton != null) nextButton.onClick.AddListener(OnNextButtonClicked);
            if (challengeButton != null) challengeButton.onClick.AddListener(OnChallengeButtonClicked);

            if (InterrogationManager.Instance != null)
            {
                InterrogationManager.Instance.OnDialogueNodeDisplayed += DisplayNode;
                InterrogationManager.Instance.OnChallengeModeToggled += UpdateChallengeState;
                InterrogationManager.Instance.OnChallengeResult += HandleChallengeResult;
            }

            if (evidencePickerContainer != null) evidencePickerContainer.SetActive(false);
        }

        public void DisplayNode(DialogueNode node)
        {
            if (node == null) return;

            if (speakerNameText != null) speakerNameText.text = node.speakerName;

            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = StartCoroutine(TypeText(node.statementText));

            if (challengeButton != null)
            {
                challengeButton.gameObject.SetActive(node.isChallengeable);
            }
        }

        private IEnumerator TypeText(string text)
        {
            isTyping = true;
            currentFullText = text;
            dialogueBodyText.text = "";

            float delay = 1f / charactersPerSecond;
            for (int i = 0; i < text.Length; i++)
            {
                dialogueBodyText.text += text[i];
                if (i % 3 == 0) AudioManager.Instance?.PlayTypewriterKey();
                yield return new WaitForSeconds(delay);
            }

            isTyping = false;
        }

        public void CompleteTypingInstantly()
        {
            if (isTyping)
            {
                if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
                dialogueBodyText.text = currentFullText;
                isTyping = false;
            }
        }

        private void OnNextButtonClicked()
        {
            if (isTyping)
            {
                CompleteTypingInstantly();
            }
            else
            {
                InterrogationManager.Instance?.AdvanceDialogue();
            }
        }

        private void OnChallengeButtonClicked()
        {
            bool currentState = InterrogationManager.Instance != null && InterrogationManager.Instance.isChallengeModeActive;
            InterrogationManager.Instance?.ToggleChallengeMode(!currentState);
        }

        private void UpdateChallengeState(bool isActive)
        {
            if (challengeHighlight != null) challengeHighlight.SetActive(isActive);
            if (evidencePickerContainer != null) evidencePickerContainer.SetActive(isActive);

            if (isActive)
            {
                PopulateEvidencePicker();
            }
        }

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
                if (discoveredIds.Contains(ev.id))
                {
                    GameObject btnObj = new GameObject($"Present_{ev.id}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                    btnObj.transform.SetParent(evidencePickerGrid, false);

                    Image img = btnObj.GetComponent<Image>();
                    if (ev.normalSprite != null) img.sprite = ev.normalSprite;

                    EvidenceSO currentEv = ev;
                    btnObj.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        InterrogationManager.Instance?.PresentEvidenceToChallenge(currentEv);
                    });
                }
            }
        }

        private void HandleChallengeResult(bool success, string reactionMessage)
        {
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
