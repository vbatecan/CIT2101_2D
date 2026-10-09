using System;
using UnityEngine;
using TMPro;

namespace CaseClosed.Data
{
    public enum FontRole
    {
        DisplayTitle,       // Bold, chunky, high impact (Logo, Case Closed, Big Victory)
        HeadingPlaque,      // Condensed or classic serif (Screen titles, section headers, case names)
        BodyDialogue,       // Highly readable serif/humanist (Interrogation text, descriptions)
        TypewriterCaseFile, // Monospaced typewriter (Dossiers, evidence observations, notebook)
        UIButton            // Bold condensed sans, uppercase, letter-spaced
    }

    public enum ButtonVariant
    {
        Primary,            // Dark charcoal with gold/tan border
        Danger,             // Blood red (#6E2723) for quit, critical accusations
        Secondary,          // Muted charcoal / paper tone
        IconButton,         // Close 'X', pause, notebook, return arrow
        TabButton,          // Notebook index tabs
        FolderCardButton    // Large dossier folder for Case Select
    }

    [Serializable]
    public class FontRoleStyle
    {
        public FontRole role;
        public TMP_FontAsset fontAsset;
        public Material fontMaterialPreset;
        public float defaultFontSize = 28f;
        public float minFontSize = 20f;
        public float maxFontSize = 36f;
        public bool enableAutoSize = false;
        public float lineSpacing = 1.15f;
        public float characterSpacing = 0f;
        public Color defaultColor = Color.white;
        public bool forceUppercase = false;
    }

    /// <summary>
    /// Central design tokens, color palette, font styles, and button variants for Case Closed.
    /// Acts as the single source of truth for all UI styling.
    /// </summary>
    [CreateAssetMenu(fileName = "UITheme", menuName = "Case Closed/UI Theme")]
    public class UITheme : ScriptableObject
    {
        private static UITheme _instance;
        public static UITheme Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<UITheme>("UITheme");
                }
                return _instance;
            }
        }

        [Header("Color Palette")]
        [Tooltip("Deep warm brown wall / background accent")]
        public Color wallBackground = new Color(0.56f, 0.30f, 0.16f, 1f); // #8E4D2A

        [Tooltip("Dark charcoal for primary buttons and framing panels")]
        public Color darkCharcoal = new Color(0.11f, 0.12f, 0.13f, 1f); // #1D1E22

        [Tooltip("Blood-red accent for Danger buttons, quit, and critical stamps")]
        public Color bloodRed = new Color(0.43f, 0.15f, 0.14f, 1f); // #6E2723

        [Tooltip("Cream aged paper for dossier pages, notebook, and evidence descriptions")]
        public Color creamPaper = new Color(0.96f, 0.94f, 0.92f, 1f); // #F5EFEB

        [Tooltip("Deep ink text on cream paper")]
        public Color deepInk = new Color(0.10f, 0.09f, 0.08f, 1f); // #1A1715

        [Tooltip("Gold / tan border accent")]
        public Color goldBorder = new Color(0.85f, 0.73f, 0.45f, 1f); // #D9BA73

        [Tooltip("Success green for completed stamps and correct connections")]
        public Color successGreen = new Color(0.18f, 0.49f, 0.20f, 1f); // #2E7D32

        [Tooltip("Warning amber for timer low warning and alerts")]
        public Color warningAmber = new Color(1.0f, 0.56f, 0.0f, 1f); // #FF8F00

        [Tooltip("Disabled button and locked state tint")]
        public Color disabledSlate = new Color(0.35f, 0.40f, 0.45f, 0.45f);

        [Header("Typography Styles")]
        public FontRoleStyle[] fontStyles = new FontRoleStyle[]
        {
            new FontRoleStyle { role = FontRole.DisplayTitle, defaultFontSize = 96f, lineSpacing = 1.0f, forceUppercase = true, defaultColor = Color.white },
            new FontRoleStyle { role = FontRole.HeadingPlaque, defaultFontSize = 48f, lineSpacing = 1.1f, forceUppercase = true, defaultColor = new Color(0.96f, 0.94f, 0.92f, 1f) },
            new FontRoleStyle { role = FontRole.BodyDialogue, defaultFontSize = 28f, minFontSize = 24f, maxFontSize = 32f, lineSpacing = 1.2f, defaultColor = Color.white },
            new FontRoleStyle { role = FontRole.TypewriterCaseFile, defaultFontSize = 24f, lineSpacing = 1.25f, defaultColor = new Color(0.10f, 0.09f, 0.08f, 1f) },
            new FontRoleStyle { role = FontRole.UIButton, defaultFontSize = 30f, characterSpacing = 4f, lineSpacing = 1.0f, forceUppercase = true, defaultColor = Color.white }
        };

        [Header("Animation Timings")]
        public float buttonHoverDuration = 0.12f;
        public float buttonPressDuration = 0.08f;
        public float panelFadeDuration = 0.25f;
        public float typewriterSpeedCharsPerSecond = 35f;

        public FontRoleStyle GetStyle(FontRole role)
        {
            if (fontStyles != null)
            {
                foreach (var s in fontStyles)
                {
                    if (s.role == role) return s;
                }
            }
            return null;
        }
    }
}
