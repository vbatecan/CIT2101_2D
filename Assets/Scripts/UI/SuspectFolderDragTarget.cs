using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CaseClosed.UI
{
    /// <summary>
    /// Event target attached to the suspect folder hierarchy to intercept clicks,
    /// prevent left-clicks from bubbling to the backdrop button (which would close the folder),
    /// handle right-clicks for resetting the view, and forward drag and scroll events to SuspectFolderUI.
    /// </summary>
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

        /// <summary>
        /// Handles pointer clicks on the suspect folder:
        /// - Right-Click: Resets folder zoom (1.0x) and position (0, 0).
        /// - Left-Click: Consumed and ignored (does nothing, preventing backdrop button dismissal).
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (_folderUI != null)
                {
                    _folderUI.ResetView();
                }
            }
            // Left click intentionally does nothing on the folder.
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
