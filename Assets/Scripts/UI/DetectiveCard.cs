using UnityEngine;
using UnityEngine.UI;

namespace CaseClosed.UI
{
    /// <summary>Reusable dossier surface with authored header, content and footer slots.</summary>
    public class DetectiveCard : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Text classificationText;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private RectTransform footer;

        public RectTransform Rect => (RectTransform)transform;
        public RectTransform Footer => footer;

        private void Awake()
        {
            DetectiveUITheme.Surface(background, DetectiveUITheme.Paper, true);
            if (classificationText != null)
                DetectiveUITheme.TextStyle(classificationText, classificationText.fontSize, DetectiveUITheme.Brass, TextAnchor.MiddleLeft);
            if (titleText != null)
                DetectiveUITheme.TextStyle(titleText, titleText.fontSize, DetectiveUITheme.Ink);
            if (bodyText != null)
                DetectiveUITheme.TextStyle(bodyText, bodyText.fontSize, DetectiveUITheme.MutedInk, TextAnchor.MiddleLeft);
        }

        public void SetContent(string classification, string title, string body, Color titleColor)
        {
            if (classificationText != null) classificationText.text = classification;
            if (titleText != null)
            {
                titleText.text = title;
                titleText.color = titleColor;
            }
            if (bodyText != null) bodyText.text = body;
        }

        public void ShowReportText(bool visible)
        {
            if (titleText != null) titleText.gameObject.SetActive(visible);
            if (bodyText != null) bodyText.gameObject.SetActive(visible);
        }
    }
}
