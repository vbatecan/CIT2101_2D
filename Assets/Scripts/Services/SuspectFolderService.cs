using System;
using UnityEngine;
using CaseClosed.Data;

namespace CaseClosed.Services
{
    /// <summary>
    /// Pure C# domain service resolving suspect folder files, case index mappings,
    /// and dossier metadata without MonoBehaviour dependencies.
    /// </summary>
    public class SuspectFolderService
    {
        public const string Case1Filename = "Case1. Vince & Jane.png";
        public const string Case2Filename = "Case2. Paul & Vonn.png";
        public const string Case3Filename = "Case3. Shania and Shan.png";

        public const string RelativeFolderPath = "Assets/Assets/SUSPECT FOLDERS";

        /// <summary>
        /// Determines the case index (1, 2, or 3) from active case data or scene name.
        /// </summary>
        public int DetermineCaseIndex(CaseSO activeCase, string sceneName = null)
        {
            if (activeCase != null)
            {
                if (activeCase.levelNumber >= 1 && activeCase.levelNumber <= 3)
                {
                    return activeCase.levelNumber;
                }

                if (!string.IsNullOrEmpty(activeCase.caseId))
                {
                    string idUpper = activeCase.caseId.ToUpperInvariant();
                    if (idUpper.Contains("01") || idUpper.Contains("1")) return 1;
                    if (idUpper.Contains("02") || idUpper.Contains("2")) return 2;
                    if (idUpper.Contains("03") || idUpper.Contains("3")) return 3;
                }

                if (!string.IsNullOrEmpty(activeCase.caseTitle))
                {
                    string titleUpper = activeCase.caseTitle.ToUpperInvariant();
                    if (titleUpper.Contains("VINCE") || titleUpper.Contains("HEIRLOOM") || titleUpper.Contains("MISSING")) return 1;
                    if (titleUpper.Contains("PAUL") || titleUpper.Contains("INSURANCE") || titleUpper.Contains("FRAUD")) return 2;
                    if (titleUpper.Contains("SHANIA") || titleUpper.Contains("CALL") || titleUpper.Contains("LAST")) return 3;
                }
            }

            if (!string.IsNullOrEmpty(sceneName))
            {
                string sUpper = sceneName.ToUpperInvariant();
                if (sUpper.Contains("01") || sUpper.Contains("1")) return 1;
                if (sUpper.Contains("02") || sUpper.Contains("2")) return 2;
                if (sUpper.Contains("03") || sUpper.Contains("3")) return 3;
            }

            return 1;
        }

        /// <summary>
        /// Returns the expected PNG filename for the given case index.
        /// </summary>
        public string GetExpectedFilename(int caseIndex)
        {
            switch (caseIndex)
            {
                case 2: return Case2Filename;
                case 3: return Case3Filename;
                case 1:
                default:
                    return Case1Filename;
            }
        }

        /// <summary>
        /// Returns human-readable label identifying the dossier suspects.
        /// </summary>
        public string GetDossierTitle(int caseIndex)
        {
            switch (caseIndex)
            {
                case 2: return "SUSPECT DOSSIER - Paul & Vonn";
                case 3: return "SUSPECT DOSSIER - Shania & Shan";
                case 1:
                default:
                    return "SUSPECT DOSSIER - Vince & Jane";
            }
        }

        /// <summary>
        /// Calculates new clamped zoom factor given current zoom and scroll delta.
        /// </summary>
        public float CalculateNewZoom(float currentTargetZoom, float scrollDeltaY, float minZoom = 1.0f, float maxZoom = 3.5f, float scrollSensitivity = 0.15f)
        {
            float deltaZoom = scrollDeltaY * scrollSensitivity;
            return Mathf.Clamp(currentTargetZoom + deltaZoom, minZoom, maxZoom);
        }

        /// <summary>
        /// Calculates new anchored position when zooming focused towards a cursor local point.
        /// Keeps the content directly under the focal point stationary relative to the cursor.
        /// </summary>
        public Vector2 CalculateZoomFocalPosition(Vector2 currentPosition, Vector2 focalPoint, float oldZoom, float newZoom)
        {
            if (oldZoom <= 0.0001f) return currentPosition;
            float ratio = newZoom / oldZoom;
            return (currentPosition - focalPoint) * ratio + focalPoint;
        }

        /// <summary>
        /// Clamps folder anchored position so that at least safetyMargin pixels
        /// of the scaled folder remain inside the parent/screen boundaries.
        /// </summary>
        public Vector2 ClampFolderPosition(Vector2 position, Vector2 folderSize, float zoom, Vector2 parentSize, float safetyMargin = 100f)
        {
            if (parentSize.x <= 0.0001f || parentSize.y <= 0.0001f) return position;

            float halfParentW = parentSize.x * 0.5f;
            float halfParentH = parentSize.y * 0.5f;
            float halfFolderW = folderSize.x * zoom * 0.5f;
            float halfFolderH = folderSize.y * zoom * 0.5f;

            float maxPosX = halfParentW + halfFolderW - safetyMargin;
            float minPosX = -halfParentW - halfFolderW + safetyMargin;
            if (minPosX > maxPosX)
            {
                minPosX = 0f;
                maxPosX = 0f;
            }

            float maxPosY = halfParentH + halfFolderH - safetyMargin;
            float minPosY = -halfParentH - halfFolderH + safetyMargin;
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
