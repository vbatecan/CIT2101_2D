using CaseClosed.Data;
using CaseClosed.Managers;
using UnityEngine;

namespace CaseClosed.UI
{
    /// <summary>Displays the authored title artwork for the active case.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class CaseTitleUI : MonoBehaviour
    {
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
            // Other objects may initialize their managers after our OnEnable.
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
            Sprite titleSprite = null;
            if (caseData != null && caseTitleSprites != null)
            {
                int titleIndex = caseData.levelNumber - 1;
                if (titleIndex >= 0 && titleIndex < caseTitleSprites.Length)
                {
                    titleSprite = caseTitleSprites[titleIndex];
                }
            }

            titleRenderer.sprite = titleSprite;
        }
    }
}
