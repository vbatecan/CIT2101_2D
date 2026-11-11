using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace CaseClosed.UI
{
    public sealed class SuitcaseSelectionController : MonoBehaviour
    {
        [SerializeField] private RectTransform room;
        [SerializeField] private Image suitcase;
        [SerializeField] private Sprite[] frames;
        [SerializeField] private CanvasGroup closeup;
        [SerializeField] private CanvasGroup dossiers;
        [SerializeField] private CanvasGroup black;
        [SerializeField] private Button firstCase;
        private bool ready;
        private bool returning;
        public bool IsReady => ready;

        private IEnumerator Start()
        {
            dossiers.alpha = 0f;
            dossiers.interactable = dossiers.blocksRaycasts = false;
            closeup.alpha = 0f;
            black.alpha = 1f;
            yield return Blend(.35f, t => black.alpha = 1f-t);
            yield return new WaitForSecondsRealtime(.2f);
            yield return Blend(1.2f, t => Sample(t));
            yield return Blend(.2f, t => dossiers.alpha = t);
            ready = true;
            dossiers.interactable = dossiers.blocksRaycasts = true;
            if (firstCase != null && firstCase.interactable && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(firstCase.gameObject);
        }

        private void Update()
        {
            if (ready && Input.GetKeyDown(KeyCode.Escape)) ReturnToMenu();
        }

        public void ReturnToMenu()
        {
            if (!ready || returning) return;
            returning = true;
            StartCoroutine(Return());
        }

        private IEnumerator Return()
        {
            ready = false;
            dossiers.interactable = dossiers.blocksRaycasts = false;
            yield return Blend(.2f, t => dossiers.alpha = 1f-t);
            yield return Blend(1.2f, t => Sample(1f-t));
            yield return Blend(.35f, t => black.alpha = t);
            if (Application.CanStreamedLevelBeLoaded("MainMenu")) SceneManager.LoadScene("MainMenu");
            else { black.alpha = 0f; returning = false; ready = true; dossiers.alpha = 1f; dossiers.interactable = dossiers.blocksRaycasts = true; Debug.LogError("MainMenu is unavailable."); }
        }

        private void Sample(float t)
        {
            float eased = Mathf.SmoothStep(0f, 1f, t);
            room.localScale = Vector3.one * Mathf.Lerp(1f, 3f, eased);
            room.anchoredPosition = new Vector2(0f, Mathf.Lerp(0f, 430f, eased));
            if (frames != null && frames.Length > 0)
                suitcase.sprite = frames[Mathf.Min(frames.Length-1, Mathf.FloorToInt(t * 1.2f / .67f * frames.Length))];
            closeup.alpha = Mathf.Clamp01((t-.75f)/.25f);
        }

        private IEnumerator Blend(float seconds, System.Action<float> draw)
        {
            float elapsed=0f;
            while(elapsed<seconds) {elapsed+=Time.unscaledDeltaTime;draw(Mathf.Clamp01(elapsed/seconds));yield return null;}
            draw(1f);
        }
    }
}
