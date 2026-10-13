using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;

namespace CaseClosed.Gameplay
{
    public class CharacterDisplay : MonoBehaviour
    {
        [Header("Character Slot Assignment")]
        [Tooltip("Designates whether this GameObject renders the primary suspect, a secondary suspect/witness, or auto-syncs.")]
        public CharacterSlot characterSlot = CharacterSlot.AutoDetect;

        [Tooltip("Optional explicit character ID filter (e.g. 'CHAR_VINCE_BATECAN' or 'CHAR_PAUL_CAMACHO').")]
        public string explicitCharacterId;

        [Header("Visual Components")]
        public Image characterPortraitImage;
        public SpriteRenderer characterSpriteRenderer;

        private CharacterProfileSO activeSuspect;
        private CharacterExpressionLibrarySO expressionLibrary;

        private void Awake()
        {
            expressionLibrary = Resources.Load<CharacterExpressionLibrarySO>("CharacterExpressions");
        }

        private void Start()
        {
            if (InterrogationManager.Instance != null)
            {
                InterrogationManager.Instance.OnSuspectChanged += HandleSuspectChanged;
                InterrogationManager.Instance.OnExpressionChanged += HandleExpressionChanged;
                InterrogationManager.Instance.OnDialogueNodeDisplayed += HandleDialogueNodeDisplayed;
            }

            if (CaseManager.Instance != null)
            {
                CaseManager.Instance.OnCaseLoaded += HandleCaseLoaded;
                CaseManager.Instance.OnInvestigatorChanged += HandleInvestigatorChanged;
                if (CaseManager.Instance.ActiveCase != null)
                {
                    HandleCaseLoaded(CaseManager.Instance.ActiveCase);
                }
                else if (characterSlot == CharacterSlot.Investigator && CaseManager.Instance.EffectiveInvestigator != null)
                {
                    UpdateSuspectProfile(CaseManager.Instance.EffectiveInvestigator);
                }
            }
            if (InterrogationManager.Instance != null)
                HandleDialogueNodeDisplayed(InterrogationManager.Instance.CurrentNode);
        }

        private void OnDestroy()
        {
            if (InterrogationManager.Instance != null)
            {
                InterrogationManager.Instance.OnSuspectChanged -= HandleSuspectChanged;
                InterrogationManager.Instance.OnExpressionChanged -= HandleExpressionChanged;
                InterrogationManager.Instance.OnDialogueNodeDisplayed -= HandleDialogueNodeDisplayed;
            }

            if (CaseManager.Instance != null)
            {
                CaseManager.Instance.OnCaseLoaded -= HandleCaseLoaded;
                CaseManager.Instance.OnInvestigatorChanged -= HandleInvestigatorChanged;
            }
        }

        private void HandleCaseLoaded(CaseSO activeCase)
        {
            if (activeCase == null) return;

            if (!string.IsNullOrEmpty(explicitCharacterId))
            {
                if (activeCase.primarySuspect != null && activeCase.primarySuspect.characterId == explicitCharacterId)
                {
                    UpdateSuspectProfile(activeCase.primarySuspect);
                    return;
                }

                if (activeCase.additionalSuspects != null)
                {
                    foreach (var s in activeCase.additionalSuspects)
                    {
                        if (s != null && s.characterId == explicitCharacterId)
                        {
                            UpdateSuspectProfile(s);
                            return;
                        }
                    }
                }
            }

            switch (characterSlot)
            {
                case CharacterSlot.PrimarySuspect:
                    UpdateSuspectProfile(activeCase.primarySuspect);
                    break;

                case CharacterSlot.SecondarySuspect:
                    if (activeCase.additionalSuspects != null && activeCase.additionalSuspects.Count > 0)
                    {
                        UpdateSuspectProfile(activeCase.additionalSuspects[0]);
                    }
                    else
                    {
                        SetSprite(null);
                    }
                    break;

                case CharacterSlot.AutoDetect:
                    UpdateSuspectProfile(InterrogationManager.Instance?.CurrentSuspect ?? activeCase.primarySuspect);
                    break;

                case CharacterSlot.Investigator:
                    UpdateSuspectProfile(CaseManager.Instance != null ? CaseManager.Instance.EffectiveInvestigator : activeCase.leadInvestigator);
                    break;
            }
        }

        private void HandleInvestigatorChanged(CharacterProfileSO investigator)
        {
            if (characterSlot == CharacterSlot.Investigator)
            {
                UpdateSuspectProfile(investigator);
            }
        }

        private void HandleSuspectChanged(CharacterProfileSO suspect)
        {
            if (characterSlot == CharacterSlot.AutoDetect)
            {
                UpdateSuspectProfile(suspect);
            }
        }

        private void HandleDialogueNodeDisplayed(DialogueNode node)
        {
            if (node == null || activeSuspect == null) return;

            bool isCurrentSpeaker = (!string.IsNullOrEmpty(node.speakerId) && node.speakerId == activeSuspect.characterId)
                || (!string.IsNullOrEmpty(node.speakerName) && (node.speakerName == activeSuspect.fullName || node.speakerName.Contains(activeSuspect.fullName)));

            if (isCurrentSpeaker)
            {
                SetExpression(node.expression);
            }
        }

        private void HandleExpressionChanged(CharacterExpression expression)
        {
            if (activeSuspect != null)
            {
                DialogueNode currentNode = InterrogationManager.Instance?.CurrentNode;
                if (currentNode != null && !string.IsNullOrEmpty(currentNode.speakerId))
                {
                    if (currentNode.speakerId == activeSuspect.characterId)
                    {
                        SetExpression(expression);
                    }
                    return;
                }

                CharacterProfileSO currentInterrogated = InterrogationManager.Instance?.CurrentSuspect;
                if (characterSlot == CharacterSlot.AutoDetect || currentInterrogated == null || currentInterrogated.characterId == activeSuspect.characterId)
                {
                    SetExpression(expression);
                }
            }
        }

        public void UpdateSuspectProfile(CharacterProfileSO suspect)
        {
            activeSuspect = suspect;
            if (suspect == null) return;

            Debug.Log($"[Gameplay:CharacterDisplay] Updated display for slot '{characterSlot}' to '{suspect.fullName}' (ID: {suspect.characterId})");
            if (suspect.defaultSittingPose != null)
            {
                SetSprite(suspect.defaultSittingPose);
            }
        }

        public void SetExpression(CharacterExpression expression)
        {
            if (activeSuspect == null) return;
            if (activeSuspect.expressions != null)
            {
                foreach (ExpressionSpriteMapping mapping in activeSuspect.expressions)
                {
                    if (mapping != null && mapping.expression == expression && mapping.sprite != null)
                    {
                        SetSprite(mapping.sprite);
                        return;
                    }
                }
            }
            if (expression == CharacterExpression.Neutral && activeSuspect.defaultSittingPose != null)
            {
                SetSprite(activeSuspect.defaultSittingPose);
                return;
            }
            Sprite sprite = expressionLibrary != null
                ? expressionLibrary.GetSprite(activeSuspect.characterId, expression) : null;
            if (sprite == null) sprite = activeSuspect.defaultSittingPose;
            if (sprite == null && expressionLibrary != null)
                sprite = expressionLibrary.GetSprite(activeSuspect.characterId, CharacterExpression.Neutral);
            if (sprite != null) SetSprite(sprite);
        }

        private void SetSprite(Sprite sprite)
        {
            if (characterPortraitImage != null)
            {
                characterPortraitImage.sprite = sprite;
                characterPortraitImage.enabled = (sprite != null);
            }
            if (characterSpriteRenderer != null)
            {
                characterSpriteRenderer.sprite = sprite;
                characterSpriteRenderer.enabled = (sprite != null);
            }
        }
    }
}
