using UnityEngine;
using UnityEditor;
using System.IO;

namespace CaseClosed.Editor
{
    /// <summary>
    /// Editor utility under Tools/CaseClosed for applying verified Sprite 2D and UI import settings,
    /// Full Rect mesh, Bilinear filtering, and 9-slice borders across Assets/Assets.
    /// Does not move, delete, or rename any project files.
    /// </summary>
    public static class BulkSpriteImporter
    {
        [MenuItem("Tools/CaseClosed/Apply Bulk Sprite Import Settings")]
        public static void ApplyImportSettings()
        {
            string[] assetGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Assets" });
            int modifiedCount = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string guid in assetGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(path)) continue;

                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) continue;

                    bool changed = false;

                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        changed = true;
                    }

                    if (importer.spritePixelsPerUnit != 100f)
                    {
                        importer.spritePixelsPerUnit = 100f;
                        changed = true;
                    }

                    if (importer.filterMode != FilterMode.Bilinear)
                    {
                        importer.filterMode = FilterMode.Bilinear;
                        changed = true;
                    }

                    // Sliced buttons require Full Rect mesh type
                    if (path.Contains("/BUTTONS/"))
                    {
                        TextureImporterSettings texSettings = new TextureImporterSettings();
                        importer.ReadTextureSettings(texSettings);
                        if (texSettings.spriteMeshType != SpriteMeshType.FullRect)
                        {
                            texSettings.spriteMeshType = SpriteMeshType.FullRect;
                            importer.SetTextureSettings(texSettings);
                            changed = true;
                        }
                    }

                    // Apply verified 9-slice borders for button templates
                    string filename = Path.GetFileNameWithoutExtension(path);
                    Vector4 targetBorder = Vector4.zero;

                    if (filename.Equals("PLAINbox", System.StringComparison.OrdinalIgnoreCase))
                    {
                        targetBorder = new Vector4(48, 48, 48, 48); // L, B, R, T
                    }
                    else if (filename.Equals("QUESTION_BOX", System.StringComparison.OrdinalIgnoreCase))
                    {
                        targetBorder = new Vector4(32, 32, 32, 32);
                    }
                    else if (filename.StartsWith("QUESTION_", System.StringComparison.OrdinalIgnoreCase))
                    {
                        targetBorder = new Vector4(24, 24, 24, 24);
                    }

                    if (targetBorder != Vector4.zero && importer.spriteBorder != targetBorder)
                    {
                        importer.spriteBorder = targetBorder;
                        changed = true;
                    }

                    if (changed)
                    {
                        importer.SaveAndReimport();
                        modifiedCount++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            Debug.Log($"[CaseClosed:Editor] Completed bulk import update. Optimized {modifiedCount} textures.");
            EditorUtility.DisplayDialog("Sprite Import Complete", $"Updated {modifiedCount} textures to Sprite 2D / UI standards.", "OK");
        }
    }
}
