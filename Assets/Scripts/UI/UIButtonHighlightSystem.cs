using UnityEngine;
using UnityEngine.UI;

namespace CaseClosed.UI
{
    /// <summary>
    /// Centralized coordinator and utility for applying a crisp white hover highlight to UGUI Button components.
    /// Configures buttons to use <see cref="Selectable.Transition.ColorTint"/> with a resting dimmed state
    /// (0.85 brightness) and an illuminating pure white (#FFFFFF) highlight on hover with a 0.1s smooth fade.
    /// Can be used as a static utility or attached directly to a Canvas / UI Panel.
    /// </summary>
    [DisallowMultipleComponent]
    public class UIButtonHighlightSystem : MonoBehaviour
    {
        public static readonly Color NormalColor = new Color(0.85f, 0.85f, 0.85f, 1.0f);
        public static readonly Color HighlightedColor = Color.white;
        public static readonly Color PressedColor = new Color(0.70f, 0.70f, 0.70f, 1.0f);
        public static readonly Color SelectedColor = Color.white;
        public static readonly Color DisabledColor = new Color(0.50f, 0.50f, 0.50f, 0.50f);
        public const float ColorMultiplier = 1.0f;
        public const float FadeDuration = 0.10f;

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
            string bName = button.name.ToLowerInvariant();
            if (bName.Contains("backdrop") || bName.Contains("overlay"))
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
