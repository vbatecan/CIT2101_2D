using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        [SerializeField] private Image headerRule;

        private TMP_Text _tmpClassification;
        private TMP_Text _tmpTitle;
        private TMP_Text _tmpBody;

        public RectTransform Rect => (RectTransform)transform;
        public RectTransform Footer => footer;

        private void Awake()
        {
            ApplyTheme();
        }

        private void OnEnable()
        {
            ApplyTheme();
        }

        public void ApplyTheme()
        {
            if (background == null) background = GetComponent<Image>();
            DetectiveUITheme.Surface(background, DetectiveUITheme.Paper, true);

            CacheTmpComponents();

            if (classificationText != null)
                DetectiveUITheme.TextStyle(classificationText, Mathf.Max(12, classificationText.fontSize), DetectiveUITheme.WoodDark, TextAnchor.MiddleLeft);
            if (_tmpClassification != null)
            {
                _tmpClassification.color = DetectiveUITheme.WoodDark;
                _tmpClassification.fontStyle = FontStyles.Bold;
            }

            EnsureHeaderRule();

            if (titleText != null)
                DetectiveUITheme.TextStyle(titleText, Mathf.Max(22, titleText.fontSize), DetectiveUITheme.Ink);
            if (_tmpTitle != null)
            {
                _tmpTitle.color = DetectiveUITheme.Ink;
                _tmpTitle.fontStyle = FontStyles.Bold;
            }

            if (bodyText != null)
                DetectiveUITheme.TextStyle(bodyText, Mathf.Max(14, bodyText.fontSize), DetectiveUITheme.MutedInk, TextAnchor.MiddleLeft);
            if (_tmpBody != null)
            {
                _tmpBody.color = DetectiveUITheme.MutedInk;
            }
        }

        private void CacheTmpComponents()
        {
            if (classificationText == null && _tmpClassification == null)
            {
                Transform tr = transform.Find("Classification") ?? transform.Find("Header");
                if (tr != null) _tmpClassification = tr.GetComponent<TMP_Text>();
            }
            if (titleText == null && _tmpTitle == null)
            {
                Transform tr = transform.Find("Title") ?? transform.Find("Text_Title");
                if (tr != null) _tmpTitle = tr.GetComponent<TMP_Text>();
            }
            if (bodyText == null && _tmpBody == null)
            {
                Transform tr = transform.Find("Body") ?? transform.Find("Text_Details") ?? transform.Find("Message");
                if (tr != null) _tmpBody = tr.GetComponent<TMP_Text>();
            }
        }

        private void EnsureHeaderRule()
        {
            if (headerRule == null)
            {
                Transform tr = transform.Find("HeaderRule") ?? transform.Find("DividerRule");
                if (tr != null) headerRule = tr.GetComponent<Image>();
            }

            if (headerRule == null && transform.childCount > 0)
            {
                headerRule = DetectiveUITheme.CreateDividerRule(transform, 0.89f);
            }

            if (headerRule != null)
            {
                headerRule.color = DetectiveUITheme.Brass;
                headerRule.gameObject.SetActive(true);
            }
        }

        public void SetContent(string classification, string title, string body, Color titleColor)
        {
            CacheTmpComponents();
            EnsureHeaderRule();

            string upperClass = !string.IsNullOrEmpty(classification) ? classification.ToUpperInvariant() : "";
            if (classificationText != null) classificationText.text = upperClass;
            if (_tmpClassification != null) _tmpClassification.text = upperClass;

            string upperTitle = !string.IsNullOrEmpty(title) ? title.ToUpperInvariant() : "";
            if (titleText != null)
            {
                titleText.text = upperTitle;
                titleText.color = titleColor;
            }
            if (_tmpTitle != null)
            {
                _tmpTitle.text = upperTitle;
                _tmpTitle.color = titleColor;
            }

            if (bodyText != null) bodyText.text = body;
            if (_tmpBody != null) _tmpBody.text = body;
        }

        public void ShowReportText(bool visible)
        {
            if (titleText != null) titleText.gameObject.SetActive(visible);
            if (_tmpTitle != null) _tmpTitle.gameObject.SetActive(visible);
            if (bodyText != null) bodyText.gameObject.SetActive(visible);
            if (_tmpBody != null) _tmpBody.gameObject.SetActive(visible);
        }
    }
}
