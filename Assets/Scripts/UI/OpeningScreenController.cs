using UnityEngine;
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
        private static bool continued;
        private bool armed;
        private bool leaving;
        private int firstFrame;
        private float elapsed;
        public bool IsAwaitingInput => !continued && !leaving;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() => continued = false;

        private void Awake()
        {
            firstFrame = Time.frameCount;
            if (continued) { gameObject.SetActive(false); return; }
            SetMenuVisible(false);
        }

        private void Update()
        {
            if (!IsAwaitingInput) return;
            elapsed += Time.unscaledDeltaTime;
            if (fan != null)
            {
                if (fanFrames != null && fanFrames.Length > 0)
                    fan.sprite = fanFrames[(int)(elapsed * 10f) % fanFrames.Length];
                // Restrained, local light fluctuation; no full-screen flashing.
                float brightness = .94f + .06f * Mathf.PerlinNoise(elapsed * .25f, 0f);
                fan.color = new Color(brightness, brightness, brightness, 1f);
            }
            if (Time.frameCount <= firstFrame + 1) return;
            if (!Input.anyKey) armed = true;
            if (armed && Input.anyKeyDown && !Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2))
                ContinueToCases();
        }

        public void ContinueToCases()
        {
            if (!IsAwaitingInput || !armed || menu == null) return;
            leaving = true;
            continued = true;
            SetMenuVisible(true);
            gameObject.SetActive(false);
            menu.OpenCaseSelect();
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
