using UnityEngine;

namespace CaseClosed.Services
{
    public class NotebookZoomService
    {
        public float CalculateNewZoom(float currentTargetZoom, float scrollDeltaY, float minZoom = 1.0f, float maxZoom = 3.5f, float scrollSensitivity = 0.15f)
        {
            float deltaZoom = scrollDeltaY * scrollSensitivity;
            return Mathf.Clamp(currentTargetZoom + deltaZoom, minZoom, maxZoom);
        }

        public Vector2 CalculateZoomFocalPosition(Vector2 currentPosition, Vector2 focalPoint, float oldZoom, float newZoom)
        {
            if (oldZoom <= 0.0001f) return currentPosition;
            float ratio = newZoom / oldZoom;
            return (currentPosition - focalPoint) * ratio + focalPoint;
        }

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
