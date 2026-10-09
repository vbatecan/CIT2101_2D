using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using CaseClosed.Data;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    /// <summary>
    /// Unified button component driven by UITheme. Manages variant styling (Primary, Danger, Secondary, etc.),
    /// smooth hover/press transitions, accessible keyboard navigation, audio feedback, and disabled state tooltips.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ThemedButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Configuration")]
        [SerializeField] private ButtonVariant variant = ButtonVariant.Primary;
        [SerializeField] private string labelText = "BUTTON";
        [SerializeField] private Sprite iconSprite;
        [SerializeField] private string lockedTooltipReason = "Requirements not met";

        [Header("Internal Component References")]
        [SerializeField] private Image targetImage;
        [SerializeField] private TextMeshProUGUI targetLabel;
        [SerializeField] private Image targetIcon;

        private Button _button;
        private ButtonStateAnimator _animator;
        private bool _isHovered = false;

        public ButtonVariant Variant
        {
            get => variant;
            set
            {
                variant = value;
                ApplyVariantStyle();
            }
        }

        public string Label
        {
            get => labelText;
            set
            {
                labelText = value;
                if (targetLabel != null)
                {
                    targetLabel.text = labelText;
                }
            }
        }

        public string LockedTooltipReason
        {
            get => lockedTooltipReason;
            set => lockedTooltipReason = value;
        }

        private void Awake()
        {
            _button = GetComponent<Button>();
            _animator = GetComponent<ButtonStateAnimator>();
            if (_animator == null)
            {
                _animator = gameObject.AddComponent<ButtonStateAnimator>();
            }

            ResolveReferences();
            ApplyVariantStyle();
        }

        private void OnEnable()
        {
            ApplyVariantStyle();
        }

        public void ResolveReferences()
        {
            if (_button == null) _button = GetComponent<Button>();
            if (targetImage == null) targetImage = GetComponent<Image>();
            if (targetLabel == null) targetLabel = GetComponentInChildren<TextMeshProUGUI>(true);
            if (targetIcon == null && transform.Find("Icon") != null)
            {
                targetIcon = transform.Find("Icon").GetComponent<Image>();
            }
        }

        public void ApplyVariantStyle()
        {
            ResolveReferences();
            UITheme theme = UITheme.Instance;

            Color baseColor = Color.white;
            Color textColor = Color.white;

            if (theme != null)
            {
                switch (variant)
                {
                    case ButtonVariant.Danger:
                        baseColor = theme.bloodRed;
                        textColor = Color.white;
                        break;
                    case ButtonVariant.Secondary:
                        baseColor = theme.disabledSlate;
                        textColor = theme.creamPaper;
                        break;
                    case ButtonVariant.FolderCardButton:
                        baseColor = Color.white; // uses sliced folder sprite
                        textColor = theme.deepInk;
                        break;
                    case ButtonVariant.Primary:
                    default:
                        baseColor = theme.darkCharcoal;
                        textColor = theme.goldBorder;
                        break;
                }
            }

            if (_button != null)
            {
                // Configure clean color tints for standard uGUI button
                _button.transition = Selectable.Transition.ColorTint;
                ColorBlock colors = _button.colors;
                colors.normalColor = baseColor;
                colors.highlightedColor = Color.Lerp(baseColor, Color.white, 0.2f);
                colors.pressedColor = Color.Lerp(baseColor, Color.black, 0.2f);
                colors.disabledColor = theme != null ? theme.disabledSlate : new Color(0.4f, 0.4f, 0.4f, 0.5f);
                colors.fadeDuration = 0.1f;
                _button.colors = colors;
            }

            if (targetLabel != null)
            {
                if (!string.IsNullOrEmpty(labelText))
                {
                    targetLabel.text = labelText.ToUpperInvariant();
                }
                targetLabel.color = textColor;
                targetLabel.alignment = TextAlignmentOptions.Center;
                targetLabel.characterSpacing = 3f;
            }

            if (targetIcon != null && iconSprite != null)
            {
                targetIcon.sprite = iconSprite;
                targetIcon.gameObject.SetActive(true);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            _isHovered = true;
            _animator?.AnimateHover(true);
            AudioManager.Instance?.PlayButtonClick();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            _animator?.AnimateHover(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            _animator?.AnimatePress();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            _animator?.AnimateRelease(_isHovered);
        }
    }
}
