using System.Collections;
using UnityEngine;
using CaseClosed.Enums;
using CaseClosed.Managers;
using CaseClosed.UI;

namespace CaseClosed.Gameplay
{
    /// <summary>
    /// Gameplay controller managing the detective's mouse-controlled physical arm pointer on the investigation desk.
    /// Interacts with physical table items at the index fingertip position when in Table Investigation mode.
    /// Automatically yields control to the OS system cursor when dialogue or UI modals are active.
    /// </summary>
    public class ArmPointerController : MonoBehaviour
    {
        public static ArmPointerController Instance { get; private set; }

        [Header("Camera & Visuals")]
        [Tooltip("The orthographic 2D camera rendering the table scene.")]
        public Camera targetCamera;

        [Tooltip("SpriteRenderer for the arm asset.")]
        public SpriteRenderer armRenderer;

        [Header("Fingertip Interaction Point")]
        [Tooltip("Child transform representing the tip of the index finger where collision occurs.")]
        public Transform fingertipPoint;

        [Tooltip("Radius around the fingertip used to detect interactive desk items.")]
        public float interactionRadius = 0.45f;

        [Tooltip("Layer mask for table evidence items.")]
        public LayerMask interactableLayers = ~0;

        [Header("Movement & Table Clamping")]
        [Tooltip("Horizontal movement range across the desk.")]
        public Vector2 horizontalBounds = new Vector2(-6.5f, 6.5f);

        [Tooltip("Vertical movement range across the desk surface.")]
        public Vector2 verticalBounds = new Vector2(-3.8f, -0.8f);

        [Tooltip("Lerp smoothing speed for following mouse movement.")]
        public float followSpeed = 18f;

        [Tooltip("Slight tilt angle when reaching towards screen edges.")]
        public float maxTiltAngle = 10f;

        [Header("Resting / Inactive State")]
        [Tooltip("Y-position when the arm is lowered to rested position during dialogue/UI.")]
        public float restingY = -5.8f;

        [Tooltip("Speed when retracting or raising the arm.")]
        public float transitionSpeed = 10f;

        [Header("Runtime State")]
        public bool isArmActive = true;
        public bool isDialogueOrUIActive = false;

        private TableEvidenceItem currentHoveredItem;
        private bool isTapping = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (targetCamera == null) targetCamera = Camera.main;
            if (armRenderer == null) armRenderer = GetComponent<SpriteRenderer>();

            if (fingertipPoint == null)
            {
                Transform child = transform.Find("Fingertip_Point");
                if (child != null) fingertipPoint = child;
                else
                {
                    GameObject fpObj = new GameObject("Fingertip_Point");
                    fpObj.transform.SetParent(transform, false);
                    fpObj.transform.localPosition = new Vector3(0f, 2.5f, 0f);
                    fingertipPoint = fpObj.transform;
                }
            }
        }

        private void Start()
        {
            UpdateCursorAndArmState();
        }

        private void OnDisable()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void OnDestroy()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void Update()
        {
            CheckUIModeState();

            if (isArmActive)
            {
                HandleArmMovement();
                HandleFingertipRaycast();
                HandleMouseClicks();
            }
            else
            {
                // Smoothly lower arm to resting position
                Vector3 restPos = new Vector3(transform.position.x, restingY, transform.position.z);
                transform.position = Vector3.Lerp(transform.position, restPos, Time.deltaTime * transitionSpeed);
                ClearHoveredItem();
            }
        }

        private void CheckUIModeState()
        {
            bool shouldBeInUIMode = false;

            // Check if any modal panel is open in UIManager
            if (UIManager.Instance != null && UIManager.Instance.currentPanel != UIPanelType.InvestigationTable)
            {
                shouldBeInUIMode = true;
            }

            // Check if Dialogue is actively displayed
            if (DialogueUI.IsDialogueOpen)
            {
                shouldBeInUIMode = true;
            }

            if (shouldBeInUIMode != isDialogueOrUIActive)
            {
                SetDialogueOrUIMode(shouldBeInUIMode);
            }
        }

        private void HandleArmMovement()
        {
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera == null) return;

            Vector3 mouseScreen = Input.mousePosition;
            Vector3 mouseWorld = targetCamera.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 5f));

            // Clamp within desk boundaries
            float targetX = Mathf.Clamp(mouseWorld.x, horizontalBounds.x, horizontalBounds.y);
            float targetY = Mathf.Clamp(mouseWorld.y, verticalBounds.x, verticalBounds.y);

            // Compute fingertip offset so the fingertip aligns with target
            Vector3 offset = (fingertipPoint != null) ? (fingertipPoint.position - transform.position) : Vector3.zero;
            Vector3 desiredPos = new Vector3(targetX - offset.x, targetY - offset.y, 0f);

            if (!isTapping)
            {
                transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * followSpeed);
            }

            // Subtle rotation tilt based on X position
            float tiltFactor = (targetX / Mathf.Max(1f, horizontalBounds.y));
            float targetTilt = -tiltFactor * maxTiltAngle;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, 0f, targetTilt), Time.deltaTime * followSpeed);
        }

        private void HandleFingertipRaycast()
        {
            Vector2 checkPos = (fingertipPoint != null) ? (Vector2)fingertipPoint.position : (Vector2)transform.position;
            Collider2D hit = Physics2D.OverlapCircle(checkPos, interactionRadius, interactableLayers);

            TableEvidenceItem item = (hit != null) ? hit.GetComponent<TableEvidenceItem>() : null;

            if (item != currentHoveredItem)
            {
                ClearHoveredItem();

                if (item != null)
                {
                    currentHoveredItem = item;
                    currentHoveredItem.SetHoverState(true);
                }
            }
        }

        private void ClearHoveredItem()
        {
            if (currentHoveredItem != null)
            {
                currentHoveredItem.SetHoverState(false);
                currentHoveredItem = null;
            }
        }

        private void HandleMouseClicks()
        {
            if (Input.GetMouseButtonDown(0)) // Left Click
            {
                StartCoroutine(TapAnimation());

                if (currentHoveredItem != null)
                {
                    currentHoveredItem.TriggerClick(false);
                }
            }
            else if (Input.GetMouseButtonDown(1)) // Right Click
            {
                if (currentHoveredItem != null)
                {
                    currentHoveredItem.TriggerClick(true);
                }
            }
        }

        private IEnumerator TapAnimation()
        {
            isTapping = true;
            Vector3 origPos = transform.position;
            Vector3 forwardPos = origPos + transform.up * 0.3f;

            float elapsed = 0f;
            float duration = 0.07f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(origPos, forwardPos, elapsed / duration);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(forwardPos, origPos, elapsed / duration);
                yield return null;
            }

            transform.position = origPos;
            isTapping = false;
        }

        public void SetDialogueOrUIMode(bool active)
        {
            isDialogueOrUIActive = active;
            isArmActive = !active;
            UpdateCursorAndArmState();
        }

        private void UpdateCursorAndArmState()
        {
            if (isDialogueOrUIActive)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Confined;
            }
        }
    }
}
