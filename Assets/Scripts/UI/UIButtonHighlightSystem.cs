using UnityEngine;
using UnityEngine.UI;

namespace CaseClosed.UI
{
    /// <summary>
    /// Centralized coordinator for consistent hover, keyboard focus, pressed and disabled feedback.
    /// Can be used as a static utility or attached directly to a Canvas / UI Panel.
    /// </summary>
    [DisallowMultipleComponent]
    public class UIButtonHighlightSystem : MonoBehaviour
    {
        public static readonly Color NormalColor = Color.white;
        public static readonly Color HighlightedColor = new Color(1f, 0.94f, 0.78f, 1f);
        public static readonly Color PressedColor = new Color(0.78f, 0.72f, 0.59f, 1f);
        public static readonly Color SelectedColor = new Color(1f, 0.94f, 0.78f, 1f);
        public static readonly Color DisabledColor = new Color(0.58f, 0.58f, 0.58f, 0.65f);
        public const float ColorMultiplier = 1.0f;
        public const float FadeDuration = 0.12f;

        [Header("Auto-Apply Configuration")]
        [Tooltip("Whether to automatically apply hover colors to all child buttons on Awake/OnEnable.")]
        [SerializeField] private bool autoApplyOnEnable = true;

        [Tooltip("Whether to recursively apply to inactive child GameObjects.")]
        [SerializeField] private bool includeInactiveChildren = true;

        private void Awake()
        {
            if (autoApplyOnEnable)
            {
                ApplyToHierarchy(gameObject, includeInactiveChildren);
            }
        }

        private void OnEnable()
        {
            if (autoApplyOnEnable)
            {
                ApplyToHierarchy(gameObject, includeInactiveChildren);
            }
        }

        /// <summary>
        /// Creates and returns a standardized ColorBlock configured for the white hover highlight.
        /// </summary>
        /// <returns>A configured <see cref="ColorBlock"/>.</returns>
        public static ColorBlock GetHighlightColorBlock()
        {
            ColorBlock cb = ColorBlock.defaultColorBlock;
            cb.normalColor = NormalColor;
            cb.highlightedColor = HighlightedColor;
            cb.pressedColor = PressedColor;
            cb.selectedColor = SelectedColor;
            cb.disabledColor = DisabledColor;
            cb.colorMultiplier = ColorMultiplier;
            cb.fadeDuration = FadeDuration;
            return cb;
        }

        /// <summary>
        /// Applies the standardized white hover highlight to a single Button component.
        /// Ensures a valid targetGraphic and transitions to ColorTint mode.
        /// </summary>
        /// <param name="button">The button to configure.</param>
        public static void ApplyTo(Button button)
        {
            if (button == null) return;

            // Skip full-screen backdrop click dismissers so hovering empty areas does not flash white
            string bName = button.name;
            if (bName.IndexOf("backdrop", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                bName.IndexOf("overlay", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return;
            }

            if (button.GetComponent<CaseFileNotebookUI>() != null || button.GetComponent<SuspectFolderUI>() != null)
            {
                return;
            }

            // Ensure a valid targetGraphic exists so ColorTint has a visual target to manipulate
            if (button.targetGraphic == null)
            {
                Graphic graphic = button.GetComponent<Graphic>() ?? button.GetComponentInChildren<Graphic>(true);
                if (graphic != null)
                {
                    button.targetGraphic = graphic;
                }
            }

            button.transition = Selectable.Transition.ColorTint;
            button.colors = GetHighlightColorBlock();
        }

        /// <summary>
        /// Traverses a GameObject hierarchy and applies the white hover highlight to every Button found.
        /// </summary>
        /// <param name="root">The root GameObject containing buttons.</param>
        /// <param name="includeInactive">Whether to include inactive children.</param>
        public static void ApplyToHierarchy(GameObject root, bool includeInactive = true)
        {
            if (root == null) return;

            Button[] buttons = root.GetComponentsInChildren<Button>(includeInactive);
            if (buttons == null || buttons.Length == 0) return;

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null)
                {
                    ApplyTo(buttons[i]);
                }
            }
        }

        /// <summary>
        /// Finds and applies the white hover highlight to all active and inactive buttons currently loaded in the scene.
        /// </summary>
        public static void ApplyToAllButtonsInScene()
        {
            Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (buttons == null) return;

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null)
                {
                    ApplyTo(buttons[i]);
                }
            }
        }
    }
}
