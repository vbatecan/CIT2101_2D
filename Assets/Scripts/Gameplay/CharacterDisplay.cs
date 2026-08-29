using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;

namespace CaseClosed.Gameplay
{
    /// <summary>
    /// Gameplay MonoBehaviour managing character portraits/sprites and subtle breathing idle animations across table views.
    /// Supports multi-character setups (Primary and Secondary suspects) sitting side-by-side across the interrogation table.
    /// Can be dragged directly onto character GameObjects in the Unity Inspector.
    /// </summary>
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

        [Header("Idle Animation Settings")]
        public bool enableIdleBreathing = true;
        public float breathingSpeed = 2f;
        public float breathingAmount = 0.03f;

        private CharacterProfileSO activeSuspect;
        private Vector3 initialScale;

        /// <summary>
        /// Stores initial visual transform scales and subscribes to case and interrogation manager events.
        /// </summary>
        private void Start()
        {
            if (characterPortraitImage != null) initialScale = characterPortraitImage.rectTransform.localScale;
            else if (characterSpriteRenderer != null) initialScale = characterSpriteRenderer.transform.localScale;
            else initialScale = transform.localScale;

            if (InterrogationManager.Instance != null)
            {
                InterrogationManager.Instance.OnSuspectChanged += HandleSuspectChanged;
                InterrogationManager.Instance.OnExpressionChanged += HandleExpressionChanged;
            }

            if (CaseManager.Instance != null)
            {
                CaseManager.Instance.OnCaseLoaded += HandleCaseLoaded;
                CaseManager.Instance.OnInvestigatorChanged += HandleInvestigatorChanged;
                if (CaseManager.Instance.activeCase != null)
                {
                    HandleCaseLoaded(CaseManager.Instance.activeCase);
                }
                else if (characterSlot == CharacterSlot.Investigator && CaseManager.Instance.selectedInvestigator != null)
                {
                    UpdateSuspectProfile(CaseManager.Instance.selectedInvestigator);
                }
            }
        }

        /// <summary>
        /// Updates the subtle sinusoidal idle breathing scale on every frame.
        /// </summary>
        private void Update()
        {
            if (enableIdleBreathing)
            {
                float scaleOffset = Mathf.Sin(Time.time * breathingSpeed) * breathingAmount;
                Vector3 newScale = new Vector3(initialScale.x, initialScale.y + scaleOffset, initialScale.z);

                if (characterPortraitImage != null) characterPortraitImage.rectTransform.localScale = newScale;
                else if (characterSpriteRenderer != null) characterSpriteRenderer.transform.localScale = newScale;
            }
        }

        /// <summary>
        /// Handles case loading event, populating character portrait based on slot configuration.
        /// </summary>
        /// <param name="activeCase">The newly loaded case.</param>
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
                    UpdateSuspectProfile(InterrogationManager.Instance?.currentSuspect ?? activeCase.primarySuspect);
                    break;

                case CharacterSlot.Investigator:
                    UpdateSuspectProfile(activeCase.leadInvestigator ?? CaseManager.Instance?.selectedInvestigator);
                    break;
            }
        }

        /// <summary>
        /// Handles investigator change event from case manager.
        /// </summary>
        /// <param name="investigator">The new active investigator profile.</param>
        private void HandleInvestigatorChanged(CharacterProfileSO investigator)
        {
            if (characterSlot == CharacterSlot.Investigator)
            {
                UpdateSuspectProfile(investigator);
            }
        }

        /// <summary>
        /// Handles suspect change event from interrogation manager.
        /// </summary>
        /// <param name="suspect">The new suspect profile.</param>
        private void HandleSuspectChanged(CharacterProfileSO suspect)
        {
            if (characterSlot == CharacterSlot.AutoDetect)
            {
                UpdateSuspectProfile(suspect);
            }
        }

        /// <summary>
        /// Handles character expression change event.
        /// </summary>
        /// <param name="expression">The new expression state.</param>
        private void HandleExpressionChanged(CharacterExpression expression)
        {
            if (activeSuspect != null)
            {
                // Only react if this display corresponds to the active suspect being interrogated
                CharacterProfileSO currentInterrogated = InterrogationManager.Instance?.currentSuspect;
                if (characterSlot == CharacterSlot.AutoDetect || currentInterrogated == null || currentInterrogated.characterId == activeSuspect.characterId)
                {
                    SetExpression(expression);
                }
            }
        }

        /// <summary>
        /// Updates the displayed character to a new suspect profile, setting their default sitting pose sprite.
        /// </summary>
        /// <param name="suspect">The suspect profile to display.</param>
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

        /// <summary>
        /// Changes the active suspect's facial expression to match the requested emotion.
        /// </summary>
        /// <param name="expression">The <see cref="CharacterExpression"/> to display.</param>
        public void SetExpression(CharacterExpression expression)
        {
            if (activeSuspect == null) return;
            Sprite exprSprite = activeSuspect.GetSpriteForExpression(expression);
            if (exprSprite != null)
            {
                SetSprite(exprSprite);
            }
        }

        /// <summary>
        /// Applies a sprite to either the UI Image or the SpriteRenderer component.
        /// </summary>
        /// <param name="sprite">The sprite to render.</param>
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
