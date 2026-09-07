using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CaseClosed.UI
{
    /// <summary>
    /// Event target attached to the case file notebook / clipboard hierarchy to intercept clicks,
    /// prevent left-clicks from bubbling to the backdrop button (which would close the notebook),
    /// handle right-clicks for resetting the view, and forward drag and scroll events to CaseFileNotebookUI.
    /// </summary>
    public class CaseFileNotebookDragTarget : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        [Tooltip("Reference to the parent CaseFileNotebookUI controller.")]
        [SerializeField] private CaseFileNotebookUI _notebookUI;

        public CaseFileNotebookUI NotebookUI
        {
            get => _notebookUI;
            set => _notebookUI = value;
        }

        private void Awake()
        {
            if (_notebookUI == null)
            {
                _notebookUI = GetComponentInParent<CaseFileNotebookUI>();
            }
        }

        public void Init(CaseFileNotebookUI notebookUI)
        {
            _notebookUI = notebookUI;
        }

        /// <summary>
        /// Handles pointer clicks on the notebook:
        /// - Right-Click: Resets notebook zoom (1.0x) and position (0, 0).
        /// - Left-Click: Consumed and ignored (does nothing, preventing backdrop button dismissal).
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (_notebookUI != null)
                {
                    _notebookUI.ResetView();
                }
            }
            // Left click intentionally does nothing on the notebook.
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_notebookUI != null)
            {
                _notebookUI.OnBeginDrag(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_notebookUI != null)
            {
                _notebookUI.OnDrag(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_notebookUI != null)
            {
                _notebookUI.OnEndDrag(eventData);
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (_notebookUI != null)
            {
                _notebookUI.OnScroll(eventData);
            }
        }
    }
}
