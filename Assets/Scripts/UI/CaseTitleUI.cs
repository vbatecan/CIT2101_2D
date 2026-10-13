using CaseClosed.Data;
using CaseClosed.Managers;
using TMPro;
using UnityEngine;

namespace CaseClosed.UI
{
    public class CaseTitleUI : MonoBehaviour
    {
        [SerializeField, Tooltip("Scene-authored label displaying the active case title.")]
        private TMP_Text titleText;

        [SerializeField, Tooltip("Title sprites in level order: case 1, case 2, case 3.")]
        private Sprite[] caseTitleSprites;

        private SpriteRenderer titleRenderer;
        private CaseManager subscribedCaseManager;

        private void Awake()
        {
            titleRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            SubscribeToCaseManager();
        }

        private void Start()
        {
            SubscribeToCaseManager();
        }

        private void OnDisable()
        {
            UnsubscribeFromCaseManager();
        }

        private void SubscribeToCaseManager()
        {
            CaseManager caseManager = CaseManager.Instance;
            if (subscribedCaseManager != caseManager)
            {
                UnsubscribeFromCaseManager();
                subscribedCaseManager = caseManager;
                if (subscribedCaseManager != null)
                {
                    subscribedCaseManager.OnCaseLoaded += HandleCaseLoaded;
                }
            }

            if (subscribedCaseManager != null)
            {
                HandleCaseLoaded(subscribedCaseManager.ActiveCase);
            }
        }

        private void UnsubscribeFromCaseManager()
        {
            if (subscribedCaseManager != null)
            {
                subscribedCaseManager.OnCaseLoaded -= HandleCaseLoaded;
            }
            subscribedCaseManager = null;
        }

        private void HandleCaseLoaded(CaseSO caseData)
        {
            if (titleText != null)
            {
                if (caseData == null)
                {
                    titleText.text = string.Empty;
                    return;
                }

                string title = caseData.caseTitle ?? string.Empty;
                string legacyPrefix = $"LEVEL {caseData.levelNumber}:";
                if (title.StartsWith(legacyPrefix, System.StringComparison.OrdinalIgnoreCase))
                {
                    title = title.Substring(legacyPrefix.Length).Trim();
                }
                titleText.text = $"CASE {caseData.levelNumber:000}: {title}".ToUpperInvariant();
                return;
            }

            Sprite titleSprite = null;
            if (caseData != null && caseTitleSprites != null)
            {
                int titleIndex = caseData.levelNumber - 1;
                if (titleIndex >= 0 && titleIndex < caseTitleSprites.Length)
                {
                    titleSprite = caseTitleSprites[titleIndex];
                }
            }

            if (titleRenderer != null)
            {
                titleRenderer.sprite = titleSprite;
            }
        }
    }
}
