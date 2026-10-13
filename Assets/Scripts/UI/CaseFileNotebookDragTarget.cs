using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CaseClosed.UI
{
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

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (_notebookUI != null)
                {
                    _notebookUI.ResetView();
                }
            }
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
