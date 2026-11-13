using UnityEngine;
using UnityEditor;

namespace CaseClosed.Editor
{
    public static class PeaceSansFontSetup
    {
        private const string FontTtfPath = "Assets/Fonts/PeaceSans/PeaceSans.ttf";
        private const string FontOtfPath = "Assets/Fonts/PeaceSans/PeaceSans.otf";

        [MenuItem("Tools/CaseClosed/Create Peace Sans Font Asset")]
        public static void CreateFontAsset()
        {
            Font font = AssetDatabase.LoadAssetAtPath<Font>(FontTtfPath);
            if (font == null)
            {
                font = AssetDatabase.LoadAssetAtPath<Font>(FontOtfPath);
            }

            if (font == null)
            {
                Debug.LogError($"[PeaceSansFontSetup] Could not find font at {FontTtfPath} or {FontOtfPath}");
                return;
            }

            string sourceFontPath = AssetDatabase.GetAssetPath(font);
            TrueTypeFontImporter importer = AssetImporter.GetAtPath(sourceFontPath) as TrueTypeFontImporter;
            if (importer != null && !importer.includeFontData)
            {
                importer.includeFontData = true;
                importer.SaveAndReimport();
                font = AssetDatabase.LoadAssetAtPath<Font>(sourceFontPath);
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
    }
}
