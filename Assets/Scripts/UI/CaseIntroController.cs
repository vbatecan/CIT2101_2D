using System;
using System.Collections;
using TMPro;
using UnityEngine;
using CaseClosed.Managers;
using CaseClosed.Services;

namespace CaseClosed.UI
{
    public sealed class CaseIntroController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup overlay;
        [SerializeField] private TMP_Text heading;
        [SerializeField] private TMP_Text location;
        [SerializeField] private TMP_Text narration;
        [SerializeField, Min(0.01f)] private float fadeSeconds = 1f;
        [SerializeField, Min(1f)] private float charactersPerSecond = 30f;
        [SerializeField, Min(0f)] private float minimumHoldSeconds = 4f;
        private CaseManager targetManager;
        private bool ownsGate;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetGate() => CaseStartPresentation.IsActive = false;

        public static void Launch(int level, Action load)
        {
            if (CaseStartPresentation.IsActive) return;
            var asset = Resources.Load<GameObject>("CaseIntro");
            var prefab = asset != null ? asset.GetComponent<CaseIntroController>() : null;
            if (prefab == null)
            {
                Debug.LogError("[CaseIntro] Missing Resources/CaseIntro prefab.");
                return;
            }
            var instance = Instantiate(prefab);
            if (instance.overlay == null || instance.heading == null || instance.location == null || instance.narration == null)
            {
                Debug.LogError("[CaseIntro] Incomplete canvas references.");
                Destroy(instance.gameObject);
                return;
            }
            instance.ownsGate = true;
            CaseStartPresentation.IsActive = true;
            DontDestroyOnLoad(instance.gameObject);
            instance.StartCoroutine(instance.Play(level, load));
        }

        private IEnumerator Play(int level, Action load)
        {
            overlay.alpha = 0f;
            overlay.blocksRaycasts = true;
            heading.text = location.text = narration.text = string.Empty;
            yield return Fade(0f, 1f);
            bool failed = false;
            try { load(); }
            catch (Exception exception) { Debug.LogException(exception); failed = true; }
            // SceneManager.LoadScene completes on the next frame; never bind the outgoing manager.
            yield return null;
            float deadline = Time.realtimeSinceStartup + 15f;
            while (!failed && Time.realtimeSinceStartup < deadline)
            {
                targetManager = CaseManager.Instance;
                if (targetManager != null && targetManager.HasStartedSession && targetManager.ActiveCase != null
                    && targetManager.ActiveCase.levelNumber == level) break;
                yield return null;
            }
            if (!failed && targetManager != null && targetManager.HasStartedSession
                && targetManager.ActiveCase != null && targetManager.ActiveCase.levelNumber == level)
            {
                // Allow scene Start methods to finish while the gate remains closed.
                yield return null;
                var data = targetManager.ActiveCase;
                heading.text = data.caseTitle;
                location.text = data.dateAndLocation;
                string text = string.IsNullOrWhiteSpace(data.introNarration) ? data.incidentDescription : data.introNarration;
                narration.text = text ?? string.Empty;
                narration.maxVisibleCharacters = 0;
                narration.ForceMeshUpdate();
                int count = narration.textInfo.characterCount;
                float elapsed = 0f;
                while (narration.maxVisibleCharacters < count)
                {
                    elapsed += Time.unscaledDeltaTime;
                    narration.maxVisibleCharacters = Mathf.Min(count, Mathf.FloorToInt(elapsed * charactersPerSecond));
                    yield return null;
                }
                if (!string.IsNullOrWhiteSpace(text))
                {
                    int words = text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Length;
                    yield return new WaitForSecondsRealtime(Mathf.Max(minimumHoldSeconds, words / 3f));
                }
            }
            else Debug.LogError("[CaseIntro] Case launch failed or timed out; restoring controls.");
            yield return Fade(1f, 0f);
            ReleaseGate();
            Destroy(gameObject);
        }

        private IEnumerator Fade(float from, float to)
        {
            float elapsed = 0f;
            while (elapsed < fadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                overlay.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / fadeSeconds));
                yield return null;
            }
            overlay.alpha = to;
        }

        private void ReleaseGate()
        {
            if (!ownsGate) return;
            ownsGate = false;
            CaseStartPresentation.IsActive = false;
            if (targetManager != null && UIManager.Instance != null)
            {
                var panel = UIManager.Instance.currentPanel;
                if (panel != CaseClosed.Enums.UIPanelType.MainMenu
                    && panel != CaseClosed.Enums.UIPanelType.ResultsScreen
                    && panel != CaseClosed.Enums.UIPanelType.GameOver
                    && panel != CaseClosed.Enums.UIPanelType.InGameMenu)
                    targetManager.ResumeTimer();
            }
        }

        private void OnDisable() => ReleaseGate();
    }
}
