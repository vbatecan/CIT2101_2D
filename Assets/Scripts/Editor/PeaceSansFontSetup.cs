using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.TextCore.LowLevel;

namespace CaseClosed.Editor
{
    public static class PeaceSansFontSetup
    {
        private const string FontTtfPath = "Assets/Fonts/PeaceSans/PeaceSans.ttf";
        private const string FontOtfPath = "Assets/Fonts/PeaceSans/PeaceSans.otf";
        private const string FontAssetPath = "Assets/Fonts/PeaceSans/PeaceSans SDF.asset";

        [MenuItem("Tools/CaseClosed/Create Peace Sans Font Asset")]
        public static void CreateFontAsset()
        {
            Font font = LoadSourceFont();
            if (font == null)
            {
                return;
            }

            Selection.objects = new Object[] { font };
            Selection.activeObject = font;

            bool success = EditorApplication.ExecuteMenuItem("Assets/Create/TextMeshPro/Font Asset");
            if (success)
            {
                Debug.Log("[PeaceSansFontSetup] Executed TextMeshPro Font Asset creation for Peace Sans.");
            }
            else
            {
                Debug.LogWarning("[PeaceSansFontSetup] Failed to execute 'Assets/Create/TextMeshPro/Font Asset' menu item.");
            }
        }

        [MenuItem("Tools/CaseClosed/Generate Peace Sans SDF")]
        public static void GenerateFontAsset()
        {
            Font font = LoadSourceFont();
            if (font == null)
            {
                return;
            }

            FontEngine.InitializeFontEngine();
            if (FontEngine.LoadFontFace(font, 90) != FontEngineError.Success)
            {
                Debug.LogError($"[PeaceSansFontSetup] Failed to load font face for {font.name}");
                return;
            }

            // Let TMP initialize its internal atlas/source fields through the supported API.
            TMP_FontAsset generated = TMP_FontAsset.CreateFontAsset(
                font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
            if (generated == null)
            {
                Debug.LogError($"[PeaceSansFontSetup] Failed to create font asset for {font.name}");
                return;
            }

            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (fontAsset == null)
            {
                fontAsset = generated;
                AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
            }
            else
            {
                // Retain the existing asset identity and Inspector references.
                EditorUtility.CopySerialized(generated, fontAsset);
                Object.DestroyImmediate(generated);
            }

            fontAsset.atlasTexture.name = "PeaceSans Atlas";
            fontAsset.material.name = "PeaceSans SDF Material";
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[PeaceSansFontSetup] Peace Sans SDF asset generated successfully at {FontAssetPath} (GUID: {AssetDatabase.AssetPathToGUID(FontAssetPath)})");
        }

        public static void GenerateFontAssetBatch()
        {
            GenerateFontAsset();
        }

        private static Font LoadSourceFont()
        {
            Font font = AssetDatabase.LoadAssetAtPath<Font>(FontTtfPath);
            if (font == null)
            {
                font = AssetDatabase.LoadAssetAtPath<Font>(FontOtfPath);
            }

            if (font == null)
            {
                Debug.LogError($"[PeaceSansFontSetup] Could not find font at {FontTtfPath} or {FontOtfPath}");
                return null;
            }

            string sourceFontPath = AssetDatabase.GetAssetPath(font);
            TrueTypeFontImporter importer = AssetImporter.GetAtPath(sourceFontPath) as TrueTypeFontImporter;
            if (importer != null && !importer.includeFontData)
            {
                importer.includeFontData = true;
                importer.SaveAndReimport();
                font = AssetDatabase.LoadAssetAtPath<Font>(sourceFontPath);
            }

            return font;
        }
    }
}
