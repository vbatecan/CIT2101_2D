using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;
using CaseClosed.Managers;

namespace CaseClosed.Gameplay
{
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

        public void UpdateSuspectProfile(CharacterProfileSO suspect)
        {
            activeSuspect = suspect;
            if (suspect == null) return;

            SetSprite(suspect.defaultSittingPose);
        }

        public void SetExpression(CharacterExpression expression)
        {
            if (activeSuspect == null) return;
            Sprite exprSprite = activeSuspect.GetSpriteForExpression(expression);
            SetSprite(exprSprite);
        }

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
