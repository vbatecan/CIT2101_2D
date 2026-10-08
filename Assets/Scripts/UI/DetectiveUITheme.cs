using UnityEngine;
using UnityEngine.UI;

namespace CaseClosed.UI
{
    /// <summary>Shared runtime presentation for dossier panels and their actions.</summary>
    internal static class DetectiveUITheme
    {
        internal static readonly Color Paper = new Color(0.98f, 0.97f, 0.93f, 1f);
        internal static readonly Color Wood = new Color(169f / 255f, 110f / 255f, 73f / 255f, 1f);
        internal static readonly Color Backdrop = new Color(131f / 255f, 82f / 255f, 53f / 255f, 0.72f);
        internal static readonly Color Ink = new Color(0.06f, 0.04f, 0.02f, 1f);
        internal static readonly Color MutedInk = new Color(0.30f, 0.23f, 0.17f, 1f);
        internal static readonly Color Brass = new Color(0.65f, 0.49f, 0.26f, 1f);
        internal static readonly Color StampRed = new Color(0.56f, 0.20f, 0.16f, 1f);
        internal static readonly Color PaperShade = new Color(0.93f, 0.90f, 0.82f, 1f);
        internal static readonly Color WoodDark = new Color(0.36f, 0.22f, 0.13f, 1f);
        internal static readonly Color BrassLight = new Color(0.80f, 0.65f, 0.38f, 1f);
        private static Sprite roundedSprite;
        private static GameObject buttonPrefab;
        private static GameObject cardPrefab;
        private static GameObject dialogPrefab;

        internal static DetectiveCard CreateCard(Transform parent)
        {
            if (cardPrefab == null) cardPrefab = Resources.Load<GameObject>("UI/DetectiveCard");
            if (cardPrefab == null) return null;
            return Object.Instantiate(cardPrefab, parent, false).GetComponent<DetectiveCard>();
        }

        internal static DetectiveDialog CreateDialog(Transform parent)
        {
            if (dialogPrefab == null) dialogPrefab = Resources.Load<GameObject>("UI/DetectiveDialog");
            if (dialogPrefab == null) return null;
            return Object.Instantiate(dialogPrefab, parent, false).GetComponent<DetectiveDialog>();
        }

        internal static GameObject CreateButton(Transform parent, string name)
        {
            if (buttonPrefab == null) buttonPrefab = Resources.Load<GameObject>("UI/DetectiveButton");
            GameObject instance;
            if (buttonPrefab != null)
            {
                instance = Object.Instantiate(buttonPrefab, parent, false);
            }
            else
            {
                instance = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                instance.transform.SetParent(parent, false);
            }
            instance.name = name;
            return instance;
        }

        internal static Sprite GetRoundedSprite()
        {
            if (roundedSprite != null) return roundedSprite;

            // One small antialiased sprite, shared and nine-sliced instead of stretching corners.
            const int size = 64;
            const float radius = 16f;
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Max(Mathf.Abs(x + 0.5f - size * 0.5f) - (size * 0.5f - radius), 0f);
                    float dy = Mathf.Max(Mathf.Abs(y + 0.5f - size * 0.5f) - (size * 0.5f - radius), 0f);
                    float alpha = Mathf.Clamp01(radius - Mathf.Sqrt(dx * dx + dy * dy));
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "Dossier rounded surface";
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            roundedSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f),
                100f, 0, SpriteMeshType.FullRect, new Vector4(18f, 18f, 18f, 18f));
            roundedSprite.name = texture.name;
            roundedSprite.hideFlags = HideFlags.HideAndDontSave;
            return roundedSprite;
        }

        internal static void Surface(Image image, Color color, bool panel = false)
        {
            if (image == null) return;
            image.sprite = GetRoundedSprite();
            image.overrideSprite = null;
            image.type = Image.Type.Sliced;
            image.preserveAspect = false;
            image.pixelsPerUnitMultiplier = panel ? 0.8f : 1.6f;
            image.color = color;
            if (panel)
            {
                Shadow[] shadows = image.GetComponents<Shadow>();
                Shadow soft = null;
                Shadow contact = null;
                Outline frame = null;
                for (int i = 0; i < shadows.Length; i++)
                {
                    if (shadows[i] is Outline outline) frame = outline;
                    else if (soft == null) soft = shadows[i];
                    else if (contact == null) contact = shadows[i];
                }
                if (soft == null) soft = image.gameObject.AddComponent<Shadow>();
                if (contact == null) contact = image.gameObject.AddComponent<Shadow>();
                if (frame == null) frame = image.gameObject.AddComponent<Outline>();
                soft.effectColor = new Color(0f, 0f, 0f, 0.18f);
                soft.effectDistance = new Vector2(0f, -8f);
                soft.useGraphicAlpha = true;
                contact.effectColor = new Color(0f, 0f, 0f, 0.12f);
                contact.effectDistance = new Vector2(0f, -2f);
                contact.useGraphicAlpha = true;
                frame.effectColor = WoodDark;
                frame.effectDistance = new Vector2(2f, -2f);
                frame.useGraphicAlpha = false;
            }
        }

        internal static void Place(RectTransform rect, Vector2 min, Vector2 max)
        {
            if (rect == null) return;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        internal static void TextStyle(Text text, int size, Color color, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            if (text == null) return;
            text.color = color;
            text.fontSize = size;
            text.fontStyle = FontStyle.Bold;
            text.alignment = alignment;
            text.raycastTarget = false;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(14, size - 4);
            text.resizeTextMaxSize = size;
            foreach (Shadow shadow in text.GetComponents<Shadow>()) shadow.enabled = false;
        }

        internal static void Action(Button button, string caption, Color color)
        {
            if (button == null) return;
            DetectiveButton presentation = button.GetComponent<DetectiveButton>();
            if (presentation != null) presentation.SetPresentation(caption, color);
            Image image = button.GetComponent<Image>();
            bool isAlert = color == StampRed;
            Color surface = isAlert ? StampRed : Ink;
            Surface(image, surface);
            if (image != null)
            {
                button.targetGraphic = image;
                Outline edge = image.GetComponent<Outline>();
                if (edge == null) edge = image.gameObject.AddComponent<Outline>();
                edge.effectColor = Brass;
                edge.effectDistance = new Vector2(1.5f, -1.5f);
                edge.useGraphicAlpha = false;
            }

            string upperText = caption != null ? caption.ToUpperInvariant() : "";
            TMPro.TextMeshProUGUI tmp = button.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
            if (tmp != null)
            {
                tmp.gameObject.SetActive(true);
                tmp.text = upperText;
                tmp.color = Paper;
                tmp.fontStyle |= TMPro.FontStyles.Bold | TMPro.FontStyles.UpperCase;
                tmp.alignment = TMPro.TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
            }
            else
            {
                Text label = button.GetComponentInChildren<Text>(true);
                if (label == null)
                {
                    GameObject labelObject = new GameObject("ActionLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                    labelObject.transform.SetParent(button.transform, false);
                    label = labelObject.GetComponent<Text>();
                    label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
                label.gameObject.SetActive(true);
                Place(label.rectTransform, Vector2.zero, Vector2.one);
                label.rectTransform.sizeDelta = new Vector2(-28f, -8f);
                TextStyle(label, 18, Paper);
                label.fontStyle = FontStyle.Bold;
                label.text = upperText;
            }
            UIButtonHighlightSystem.ApplyTo(button);
        }

        internal static Image CreateDividerRule(Transform parent, float anchorY = 0.88f)
        {
            if (parent == null) return null;
            Transform existing = parent.Find("HeaderRule") ?? parent.Find("DividerRule");
            Image ruleImage = existing != null ? existing.GetComponent<Image>() : null;
            if (ruleImage == null)
            {
                GameObject ruleObj = new GameObject("HeaderRule", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                ruleObj.transform.SetParent(parent, false);
                ruleImage = ruleObj.GetComponent<Image>();
            }

            RectTransform rt = ruleImage.rectTransform;
            rt.anchorMin = new Vector2(0.06f, anchorY);
            rt.anchorMax = new Vector2(0.94f, anchorY);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0f, 1.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            ruleImage.sprite = null;
            ruleImage.color = Brass;
            ruleImage.raycastTarget = false;
            return ruleImage;
        }
    }
}
