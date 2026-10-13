using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CaseClosed.UI
{
    public class SuspectFolderDragTarget : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        [Tooltip("Reference to the parent SuspectFolderUI controller.")]
        [SerializeField] private SuspectFolderUI _folderUI;

        public SuspectFolderUI FolderUI
        {
            get => _folderUI;
            set => _folderUI = value;
        }

        private void Awake()
        {
            if (_folderUI == null)
            {
                _folderUI = GetComponentInParent<SuspectFolderUI>();
            }
        }

        public void Init(SuspectFolderUI folderUI)
        {
            _folderUI = folderUI;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (_folderUI != null)
                {
                    _folderUI.ResetView();
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_folderUI != null)
            {
                _folderUI.OnBeginDrag(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_folderUI != null)
            {
                _folderUI.OnDrag(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_folderUI != null)
            {
                _folderUI.OnEndDrag(eventData);
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (_folderUI != null)
            {
                _folderUI.OnScroll(eventData);
            }
        }
    }
}
