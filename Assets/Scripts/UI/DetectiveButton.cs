using UnityEngine;
using UnityEngine.UI;

namespace CaseClosed.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button), typeof(Image))]
    public class DetectiveButton : MonoBehaviour
    {
        [Header("Dossier Button")]
        [SerializeField, Tooltip("Label displayed on the button.")]
        private string caption = "Continue investigation";

        [SerializeField, Tooltip("Use ink for primary actions, muted ink for secondary actions, or red for leaving a case.")]
        private Color backgroundColor = new Color(0.12f, 0.17f, 0.17f, 1f);

        [SerializeField, Tooltip("Keep the scene's graphics, typography, and native Button transitions instead of applying runtime styling.")]
        private bool useScenePresentation;

        public bool UsesScenePresentation => useScenePresentation;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (!useScenePresentation) EnsureDetectiveUI();
        }

        private void OnEnable()
        {
            if (useScenePresentation) return;
            DetectiveUITheme.Action(button, caption, backgroundColor);
            EnsureDetectiveUI();
        }

        private void EnsureDetectiveUI()
        {
            if (GetComponent<DetectiveButtonUI>() == null)
            {
                gameObject.AddComponent<DetectiveButtonUI>();
            }
        }

        // Keep runtime labels and colors when a panel is closed and reopened.
        internal void SetPresentation(string label, Color color)
        {
            caption = label;
            backgroundColor = color;
        }
    }
}
