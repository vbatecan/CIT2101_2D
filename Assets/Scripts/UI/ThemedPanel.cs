using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Data;

namespace CaseClosed.UI
{
    /// <summary>
    /// Standardizes panel background framing and smooth CanvasGroup transitions across screens.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ThemedPanel : MonoBehaviour
    {
        [SerializeField] private bool autoFadeOnEnable = true;
        [SerializeField] private float fadeDuration = 0.2f;

        private CanvasGroup _canvasGroup;
        private Coroutine _fadeRoutine;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            if (autoFadeOnEnable && _canvasGroup != null)
            {
                FadeIn();
            }
        }

        public void FadeIn()
        {
            StartFade(0f, 1f, fadeDuration);
        }

        public void FadeOut(System.Action onComplete = null)
        {
            StartFade(1f, 0f, fadeDuration, onComplete);
        }

        private void StartFade(float startAlpha, float endAlpha, float duration, System.Action onComplete = null)
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) return;

            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeRoutine(startAlpha, endAlpha, duration, onComplete));
        }

        private IEnumerator FadeRoutine(float start, float end, float duration, System.Action onComplete)
        {
            _canvasGroup.alpha = start;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(start, end, elapsed / duration);
                yield return null;
            }

            _canvasGroup.alpha = end;
            _fadeRoutine = null;
            onComplete?.Invoke();
        }
    }
}
