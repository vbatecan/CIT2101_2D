using UnityEngine;

namespace CaseClosed.Gameplay
{
    /// <summary>
    /// Gameplay MonoBehaviour locking camera orthographic projection and visualizing 3-tier investigation viewport gizmos.
    /// Can be dragged directly onto the Main Camera GameObject in the Unity Inspector.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class FixedInvestigationCamera : MonoBehaviour
    {
        [Header("Fixed Camera Settings")]
        public Vector3 fixedPosition = new Vector3(0f, 0f, -10f);
        public float orthographicSize = 5f;
        public bool lockCameraTransform = true;

        [Header("Viewport Layout Boundary Rects (Normalized 0 to 1)")]
        [Tooltip("Top Half: Suspect & Witness sitting across table")]
        public Rect suspectViewportRect = new Rect(0f, 0.5f, 1f, 0.5f);
        [Tooltip("Center: Investigation Table displaying interactive evidence")]
        public Rect tableViewportRect = new Rect(0.1f, 0.3f, 0.8f, 0.4f);
        [Tooltip("Bottom/Sides: Fixed Overlay UI for dialogue, notebook, controls")]
        public Rect uiOverlayRect = new Rect(0f, 0f, 1f, 0.35f);

        private Camera cam;

        /// <summary>
        /// Retrieves camera component and applies fixed settings on Awake.
        /// </summary>
        private void Awake()
        {
            cam = GetComponent<Camera>();
            ApplyFixedSettings();
        }

        /// <summary>
        /// Reapplies camera settings when modified in the Unity Inspector.
        /// </summary>
        private void OnValidate()
        {
            if (cam == null) cam = GetComponent<Camera>();
            ApplyFixedSettings();
        }

        /// <summary>
        /// Enforces camera position and rotation lock after all standard updates have executed.
        /// </summary>
        private void LateUpdate()
        {
            if (lockCameraTransform)
            {
                transform.position = fixedPosition;
                transform.rotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// Enforces fixed orthographic size, position, and zeroed rotation onto the camera.
        /// </summary>
        public void ApplyFixedSettings()
        {
            if (cam == null) return;
            cam.orthographic = true;
            cam.orthographicSize = orthographicSize;
            transform.position = fixedPosition;
            transform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// Draws editor gizmos in the Scene view visualizing the 3-tier viewport layout regions (Suspect, Table, UI).
        /// </summary>
        private void OnDrawGizmos()
        {
            if (cam == null) cam = GetComponent<Camera>();
            if (cam == null) return;

            // Suspect Top Region
            Gizmos.color = Color.cyan;
            Vector3 topCenter = fixedPosition + new Vector3(0f, orthographicSize * 0.5f, 10f);
            Gizmos.DrawWireCube(topCenter, new Vector3(orthographicSize * cam.aspect * 2f, orthographicSize, 0.1f));

            // Table Center Region
            Gizmos.color = Color.yellow;
            Vector3 centerTable = fixedPosition + new Vector3(0f, 0f, 10f);
            Gizmos.DrawWireCube(centerTable, new Vector3(orthographicSize * cam.aspect * 1.6f, orthographicSize * 0.8f, 0.1f));

            // UI Bottom Region
            Gizmos.color = Color.magenta;
            Vector3 bottomUI = fixedPosition + new Vector3(0f, -orthographicSize * 0.65f, 10f);
            Gizmos.DrawWireCube(bottomUI, new Vector3(orthographicSize * cam.aspect * 2f, orthographicSize * 0.7f, 0.1f));
        }
    }
}
