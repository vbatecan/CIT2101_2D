using UnityEngine;

namespace CaseClosed.Services
{
    /// <summary>
    /// Pure C# domain service resolving zoom scale calculation, focal position shifts,
    /// and boundary clamping for the Case File Notebook without MonoBehaviour dependencies.
    /// </summary>
    public class NotebookZoomService
    {
        /// <summary>
        /// Calculates new clamped zoom factor given current zoom and scroll delta.
        /// </summary>
        /// <param name="currentTargetZoom">Current zoom scale factor.</param>
        /// <param name="scrollDeltaY">Mouse scroll wheel delta or pinch step.</param>
        /// <param name="minZoom">Minimum allowable zoom factor (default 1.0).</param>
        /// <param name="maxZoom">Maximum allowable zoom factor (default 3.5).</param>
        /// <param name="scrollSensitivity">Multiplier applied to scrollDeltaY (default 0.15).</param>
        /// <returns>Clamped target zoom factor between minZoom and maxZoom.</returns>
        public float CalculateNewZoom(float currentTargetZoom, float scrollDeltaY, float minZoom = 1.0f, float maxZoom = 3.5f, float scrollSensitivity = 0.15f)
        {
            float deltaZoom = scrollDeltaY * scrollSensitivity;
            return Mathf.Clamp(currentTargetZoom + deltaZoom, minZoom, maxZoom);
        }

        /// <summary>
        /// Calculates new anchored position when zooming focused towards a cursor local point.
        /// Keeps the content directly under the focal point stationary relative to the cursor.
        /// </summary>
        /// <param name="currentPosition">Current anchored position of the notebook.</param>
        /// <param name="focalPoint">Focal point coordinate in the notebook's parent coordinate space.</param>
        /// <param name="oldZoom">Previous zoom scale factor.</param>
        /// <param name="newZoom">New zoom scale factor.</param>
        /// <returns>Adjusted anchored position keeping focal point stationary.</returns>
        public Vector2 CalculateZoomFocalPosition(Vector2 currentPosition, Vector2 focalPoint, float oldZoom, float newZoom)
        {
            if (oldZoom <= 0.0001f) return currentPosition;
            float ratio = newZoom / oldZoom;
            return (currentPosition - focalPoint) * ratio + focalPoint;
        }

        /// <summary>
        /// Clamps notebook anchored position so that at least safetyMargin pixels
        /// of the scaled notebook remain inside the parent/screen boundaries.
        /// </summary>
        /// <param name="position">Candidate anchored position of the notebook.</param>
        /// <param name="notebookSize">Unscaled dimensions of the notebook rect.</param>
        /// <param name="zoom">Current zoom scale factor.</param>
        /// <param name="parentSize">Dimensions of the parent container or screen viewport.</param>
        /// <param name="safetyMargin">Minimum visible margin in pixels retained inside the viewport (default 100).</param>
        /// <returns>Clamped anchored position within safe viewport boundaries.</returns>
        public Vector2 ClampNotebookPosition(Vector2 position, Vector2 notebookSize, float zoom, Vector2 parentSize, float safetyMargin = 100f)
        {
            if (parentSize.x <= 0.0001f || parentSize.y <= 0.0001f) return position;

            float halfParentW = parentSize.x * 0.5f;
            float halfParentH = parentSize.y * 0.5f;
            float halfNotebookW = notebookSize.x * zoom * 0.5f;
            float halfNotebookH = notebookSize.y * zoom * 0.5f;

            float maxPosX = halfParentW + halfNotebookW - safetyMargin;
            float minPosX = -halfParentW - halfNotebookW + safetyMargin;
            if (minPosX > maxPosX)
            {
                minPosX = 0f;
                maxPosX = 0f;
            }

            float maxPosY = halfParentH + halfNotebookH - safetyMargin;
            float minPosY = -halfParentH - halfNotebookH + safetyMargin;
            if (minPosY > maxPosY)
            {
                minPosY = 0f;
                maxPosY = 0f;
            }

            return new Vector2(
                Mathf.Clamp(position.x, minPosX, maxPosX),
                Mathf.Clamp(position.y, minPosY, maxPosY)
            );
        }
    }
}
