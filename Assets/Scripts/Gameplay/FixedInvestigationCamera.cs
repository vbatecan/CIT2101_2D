using UnityEngine;

namespace CaseClosed.Gameplay
{
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

        private void Awake()
        {
            cam = GetComponent<Camera>();
            ApplyFixedSettings();
        }

        private void OnValidate()
        {
            if (cam == null) cam = GetComponent<Camera>();
            ApplyFixedSettings();
        }

        private void LateUpdate()
        {
            if (lockCameraTransform)
            {
                transform.position = fixedPosition;
                transform.rotation = Quaternion.identity;
            }
        }

        public void ApplyFixedSettings()
        {
            if (cam == null) return;
            cam.orthographic = true;
            cam.orthographicSize = orthographicSize;
            transform.position = fixedPosition;
            transform.rotation = Quaternion.identity;
        }

        private void OnDrawGizmos()
        {
            // Visualize 3-Tier Fixed Viewport Layout in Unity Editor Scene View
            Gizmos.color = Color.cyan;
            // Suspect Top Region
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
