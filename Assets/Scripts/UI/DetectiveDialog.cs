using UnityEngine;
using UnityEngine.UI;

namespace CaseClosed.UI
{
    public class DetectiveDialog : MonoBehaviour
    {
        [SerializeField] private DetectiveCard card;
        [SerializeField] private Button primaryButton;
        [SerializeField] private Button secondaryButton;
        [SerializeField, Tooltip("Keep the scene's authored backdrop styling.")]
        private bool useScenePresentation;

        public DetectiveCard Card => card;
        public Button PrimaryButton => primaryButton;
        public Button SecondaryButton => secondaryButton;

        private void Awake()
        {
            if (useScenePresentation) return;
            Image backdrop = GetComponent<Image>();
            if (backdrop != null) backdrop.color = DetectiveUITheme.Backdrop;
        }

        public void SetContent(string classification, string title, string message, Color titleColor)
        {
            if (card != null) card.SetContent(classification, title, message, titleColor);
        }

        public void SetActions(string primaryCaption, string secondaryCaption, bool leavingCase = false)
        {
            DetectiveUITheme.Action(primaryButton, primaryCaption, DetectiveUITheme.Ink);
            Color secondaryColor = DetectiveUITheme.MutedInk;
            if (leavingCase) secondaryColor = DetectiveUITheme.StampRed;
            DetectiveUITheme.Action(secondaryButton, secondaryCaption, secondaryColor);
        }
    }
}
