using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CaseClosed.Data;
using CaseClosed.Gameplay;
using CaseClosed.Managers;

namespace CaseClosed.UI
{
    /// <summary>
    /// Transforms the top-right placeholder frame into a live evidence inspection preview.
    /// Displays zoomed artwork, evidence title, observations, and an 'Add to Notebook' button.
    /// </summary>
    public class EvidenceInspectPreviewUI : MonoBehaviour
    {
        public static EvidenceInspectPreviewUI Instance { get; private set; }

        [Header("Preview Frame Components")]
        [SerializeField] private Image previewIconImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI observationText;
        [SerializeField] private Button addToNotebookButton;
        [SerializeField] private TextMeshProUGUI buttonLabelText;

        [Header("Default Placeholder State")]
        [SerializeField] private string defaultPrompt = "Select desk evidence to inspect";

        private EvidenceSO _currentEvidence;

        private void Awake()
        {
            Instance = this;
            if (addToNotebookButton != null)
            {
                addToNotebookButton.onClick.AddListener(OnAddToNotebookClicked);
            }
            ClearPreview();
        }

        public void DisplayEvidence(EvidenceSO evidence)
        {
            _currentEvidence = evidence;
            if (evidence == null)
            {
                ClearPreview();
                return;
            }

            if (previewIconImage != null)
            {
                Sprite displaySprite = evidence.zoomedSprite != null ? evidence.zoomedSprite : evidence.normalSprite;
                previewIconImage.sprite = displaySprite;
                previewIconImage.enabled = (displaySprite != null);
            }

            if (titleText != null) titleText.text = evidence.evidenceName.ToUpperInvariant();
            if (observationText != null) observationText.text = !string.IsNullOrEmpty(evidence.detailedObservation)
                ? evidence.detailedObservation : evidence.baseDescription;

            if (addToNotebookButton != null)
            {
                bool discovered = CaseManager.Instance != null && CaseManager.Instance.IsEvidenceDiscovered(evidence);
                addToNotebookButton.gameObject.SetActive(true);
                addToNotebookButton.interactable = !discovered;
                if (buttonLabelText != null)
                {
                    buttonLabelText.text = discovered ? "RECORDED IN NOTEBOOK" : "+ ADD TO NOTEBOOK";
                }
            }
        }

        private void OnAddToNotebookClicked()
        {
            if (_currentEvidence == null) return;

            AudioManager.Instance?.PlayClueDiscovered();
            CaseManager.Instance?.RegisterDiscoveredEvidence(_currentEvidence);

            if (addToNotebookButton != null)
            {
                addToNotebookButton.interactable = false;
                if (buttonLabelText != null) buttonLabelText.text = "RECORDED IN NOTEBOOK";
            }
        }

        public void ClearPreview()
        {
            _currentEvidence = null;
            if (previewIconImage != null) previewIconImage.enabled = false;
            if (titleText != null) titleText.text = "EVIDENCE INSPECT";
            if (observationText != null) observationText.text = defaultPrompt;
            if (addToNotebookButton != null) addToNotebookButton.gameObject.SetActive(false);
        }
    }
}
