using System.Collections;
using UnityEngine;

namespace CaseClosed.UI
{
    /// <summary>
    /// Handles smooth tactile transitions (hover lift, click punch, reset) for buttons
    /// using coroutines without per-frame allocations.
    /// </summary>
    public class ButtonStateAnimator : MonoBehaviour
    {
        [Header("Animation Properties")]
        [SerializeField] private float hoverScale = 1.04f;
        [SerializeField] private float pressScale = 0.96f;
        [SerializeField] private float transitionDuration = 0.12f;

        private Vector3 _originalScale = Vector3.one;
        private Coroutine _activeAnimation;

        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        private void OnEnable()
        {
            transform.localScale = _originalScale;
        }

        private void OnDisable()
        {
            if (_activeAnimation != null)
            {
                StopCoroutine(_activeAnimation);
                _activeAnimation = null;
            }
            transform.localScale = _originalScale;
        }

        public void AnimateHover(bool isHovered)
        {
            Vector3 target = isHovered ? (_originalScale * hoverScale) : _originalScale;
            StartScaleTransition(target, transitionDuration);
        }

        public void AnimatePress()
        {
            Vector3 target = _originalScale * pressScale;
            StartScaleTransition(target, transitionDuration * 0.6f);
        }

        public void AnimateRelease(bool stillHovered)
        {
            Vector3 target = stillHovered ? (_originalScale * hoverScale) : _originalScale;
            StartScaleTransition(target, transitionDuration);
        }

        private void StartScaleTransition(Vector3 targetScale, float duration)
        {
            if (!gameObject.activeInHierarchy)
            {
                transform.localScale = targetScale;
                return;
            }

            if (_activeAnimation != null)
            {
                StopCoroutine(_activeAnimation);
            }
            _activeAnimation = StartCoroutine(ScaleRoutine(targetScale, duration));
        }

        private IEnumerator ScaleRoutine(Vector3 targetScale, float duration)
        {
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, t);
                yield return null;
            }

            transform.localScale = targetScale;
            _activeAnimation = null;
        }
    }
}
