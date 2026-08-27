using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;

namespace CaseClosed.Gameplay
{
    /// <summary>
    /// Gameplay MonoBehaviour managing character portraits/sprites and subtle breathing idle animations across table views.
    /// Can be dragged directly onto the SuspectPortrait or Character GameObject in the Unity Inspector.
    /// </summary>
    public class CharacterDisplay : MonoBehaviour
    {
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
        /// Stores initial visual transform scales and subscribes to interrogation manager suspect and expression events.
        /// </summary>
        private void Start()
        {
            if (characterPortraitImage != null) initialScale = characterPortraitImage.rectTransform.localScale;
            else if (characterSpriteRenderer != null) initialScale = characterSpriteRenderer.transform.localScale;
            else initialScale = transform.localScale;

            if (InterrogationManager.Instance != null)
            {
                InterrogationManager.Instance.OnSuspectChanged += UpdateSuspectProfile;
                InterrogationManager.Instance.OnExpressionChanged += SetExpression;
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
        /// Updates the displayed character to a new suspect profile, setting their default sitting pose sprite.
        /// </summary>
        /// <param name="suspect">The suspect profile to display.</param>
        public void UpdateSuspectProfile(CharacterProfileSO suspect)
        {
            activeSuspect = suspect;
            if (suspect == null) return;

            SetSprite(suspect.defaultSittingPose);
        }

        /// <summary>
        /// Changes the active suspect's facial expression to match the requested emotion.
        /// </summary>
        /// <param name="expression">The <see cref="CharacterExpression"/> to display.</param>
        public void SetExpression(CharacterExpression expression)
        {
            if (activeSuspect == null) return;
            Sprite exprSprite = activeSuspect.GetSpriteForExpression(expression);
            SetSprite(exprSprite);
        }

        /// <summary>
        /// Applies a sprite to either the UI Image or the SpriteRenderer component.
        /// </summary>
        /// <param name="sprite">The sprite to render.</param>
        private void SetSprite(Sprite sprite)
        {
            if (sprite == null) return;

            if (characterPortraitImage != null)
            {
                characterPortraitImage.sprite = sprite;
            }
            if (characterSpriteRenderer != null)
            {
                characterSpriteRenderer.sprite = sprite;
            }
        }
    }
}
