using UnityEngine;
using TMPro;
using CaseClosed.Data;

namespace CaseClosed.UI
{
    /// <summary>
    /// Attaches to any TextMeshProUGUI element to enforce standardized font, sizing, colors,
    /// and spacing based on its designated FontRole in UITheme.
    /// Preserves the existing text string while upgrading typography.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ThemedText : MonoBehaviour
    {
        [SerializeField] private FontRole fontRole = FontRole.BodyDialogue;
        [SerializeField] private UITheme customThemeOverride;
        [SerializeField] private bool overrideColor = false;
        [SerializeField] private Color customColor = Color.white;

        private TextMeshProUGUI _tmp;

        public FontRole Role
        {
            get => fontRole;
            set
            {
                fontRole = value;
                ApplyStyle();
            }
        }

        private void Awake()
        {
            _tmp = GetComponent<TextMeshProUGUI>();
            ApplyStyle();
        }

        private void OnEnable()
        {
            ApplyStyle();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_tmp == null) _tmp = GetComponent<TextMeshProUGUI>();
            ApplyStyle();
        }
#endif

        public void ApplyStyle()
        {
            if (_tmp == null) _tmp = GetComponent<TextMeshProUGUI>();
            if (_tmp == null) return;

            UITheme theme = customThemeOverride != null ? customThemeOverride : UITheme.Instance;
            if (theme == null) return;

            FontRoleStyle style = theme.GetStyle(fontRole);
            if (style == null) return;

            if (style.fontAsset != null)
            {
                _tmp.font = style.fontAsset;
            }

            if (style.fontMaterialPreset != null)
            {
                _tmp.fontSharedMaterial = style.fontMaterialPreset;
            }

            _tmp.fontSize = style.defaultFontSize;
            _tmp.enableAutoSizing = style.enableAutoSize;
            if (style.enableAutoSize)
            {
                _tmp.fontSizeMin = style.minFontSize;
                _tmp.fontSizeMax = style.maxFontSize;
            }

            _tmp.lineSpacing = (style.lineSpacing - 1f) * 100f; // TMP line spacing adjustment
            _tmp.characterSpacing = style.characterSpacing;

            if (!overrideColor)
            {
                _tmp.color = style.defaultColor;
            }
            else
            {
                _tmp.color = customColor;
            }

            if (style.forceUppercase && !string.IsNullOrEmpty(_tmp.text))
            {
                _tmp.text = _tmp.text.ToUpperInvariant();
            }

            _tmp.ForceMeshUpdate();
        }
    }
}
