using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CaseClosed.UI
{
    /// <summary>Startup-only presentation; later visits retain the existing menu.</summary>
    public sealed class OpeningScreenController : MonoBehaviour
    {
        [SerializeField] private MainMenuUI menu;
        [SerializeField] private CanvasGroup menuGroup;
        [SerializeField] private Image fan;
        [SerializeField] private Sprite[] fanFrames;
        [Header("Suitcase Reveal")]
        [SerializeField] private Image suitcase;
        [SerializeField] private Sprite[] suitcaseFrames;
        [SerializeField] private GameObject startPrompt;
        [SerializeField] private Button firstButton;
        [SerializeField] private RectTransform room;
        [SerializeField] private CanvasGroup suitcaseInterior;
        [SerializeField, Tooltip("Room scale at the end of the opening zoom.")]
        private float openRoomScale = 3f;
        [SerializeField, Tooltip("Room offset at the end of the opening zoom, in canvas units.")]
        private Vector2 openRoomOffset = new Vector2(0f, 430f);
        private Vector3 initialRoomScale;
        private Vector2 initialRoomPosition;
        private static bool continued;
        private bool armed;
        private bool leaving;
        private int firstFrame;
        private float elapsed;
        public bool IsReady { get; private set; }
        public bool IsAwaitingInput => !continued && !leaving;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() => continued = false;

        private void Awake()
        {
            firstFrame = Time.frameCount;
            if (room != null)
            {
                initialRoomScale = room.localScale;
                initialRoomPosition = room.anchoredPosition;
            }
            IsReady = continued;
            SetMenuVisible(continued);
            if (startPrompt != null) startPrompt.SetActive(!continued);
            SamplePresentation(continued ? 1f : 0f);
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(continued && firstButton != null ? firstButton.gameObject : null);
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            if (fan != null)
            {
                if (fanFrames != null && fanFrames.Length > 0)
                    fan.sprite = fanFrames[(int)(elapsed * 10f) % fanFrames.Length];
                // Restrained, local light fluctuation; no full-screen flashing.
                float brightness = .94f + .06f * Mathf.PerlinNoise(elapsed * .25f, 0f);
                fan.color = new Color(brightness, brightness, brightness, 1f);
            }
            if (!IsAwaitingInput) return;
            if (Time.frameCount <= firstFrame + 1) return;
            if (!Input.anyKey) armed = true;
            if (armed && Input.anyKeyDown && !Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2))
                ContinueToCases();
        }

        public void ContinueToCases()
        {
            if (!IsAwaitingInput || !armed || menu == null) return;
            leaving = true;
            if (startPrompt != null) startPrompt.SetActive(false);
            StartCoroutine(RevealMenu());
        }

        private IEnumerator RevealMenu()
        {
            float openingTime = 0f;
            while (openingTime < 1.2f)
            {
                SamplePresentation(openingTime / 1.2f);
                openingTime += Time.unscaledDeltaTime;
                yield return null;
            }
            SamplePresentation(1f);

            float fadeTime = 0f;
            while (fadeTime < .2f)
            {
                fadeTime += Time.unscaledDeltaTime;
                if (menuGroup != null) menuGroup.alpha = Mathf.Clamp01(fadeTime / .2f);
                yield return null;
            }

            // Do not let the start key/click also submit the newly visible menu.
            while (Input.anyKey) yield return null;
            yield return null;
            continued = true;
            IsReady = true;
            SetMenuVisible(true);
            if (firstButton != null && firstButton.interactable && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
        }

        private void SamplePresentation(float progress)
        {
            if (room != null)
            {
                float eased = Mathf.SmoothStep(0f, 1f, progress);
                room.localScale = Vector3.Lerp(initialRoomScale, initialRoomScale * openRoomScale, eased);
                room.anchoredPosition = Vector2.Lerp(initialRoomPosition, initialRoomPosition + openRoomOffset, eased);
            }
            if (suitcaseInterior != null)
                suitcaseInterior.alpha = Mathf.Clamp01((progress - .75f) / .25f);
            if (suitcase == null || suitcaseFrames == null || suitcaseFrames.Length == 0) return;
            int frame = Mathf.Min(suitcaseFrames.Length - 1, Mathf.FloorToInt(progress * 1.2f / .67f * suitcaseFrames.Length));
            suitcase.sprite = suitcaseFrames[frame];
        }

        private void SetMenuVisible(bool visible)
        {
            if (menuGroup == null) return;
            menuGroup.alpha = visible ? 1f : 0f;
            menuGroup.interactable = visible;
            menuGroup.blocksRaycasts = visible;
        }
    }
}
