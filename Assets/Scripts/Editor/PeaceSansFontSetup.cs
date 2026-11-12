using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;

namespace CaseClosed.Editor
{
    public static class PeaceSansFontSetup
    {
        private const string FontTtfPath = "Assets/Fonts/PeaceSans/PeaceSans.ttf";
        private const string FontOtfPath = "Assets/Fonts/PeaceSans/PeaceSans.otf";
        private const string FontAssetPath = "Assets/Fonts/PeaceSans/PeaceSans SDF.asset";

        [MenuItem("Tools/CaseClosed/Generate Peace Sans SDF")]
        public static void GenerateFontAsset()
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

            // Ensure TrueTypeFontImporter has includeFontData enabled
            string sourceFontPath = AssetDatabase.GetAssetPath(font);
            TrueTypeFontImporter importer = AssetImporter.GetAtPath(sourceFontPath) as TrueTypeFontImporter;
            if (importer != null && !importer.includeFontData)
            {
                importer.includeFontData = true;
                importer.SaveAndReimport();
                font = AssetDatabase.LoadAssetAtPath<Font>(sourceFontPath);
            }

            FontEngine.InitializeFontEngine();
            if (FontEngine.LoadFontFace(font, 90) != FontEngineError.Success)
            {
                Debug.LogError($"[PeaceSansFontSetup] Failed to load font face for {font.name}");
                return;
            }

            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            bool isNew = fontAsset == null;
            if (isNew)
            {
                fontAsset = ScriptableObject.CreateInstance<TMP_FontAsset>();
                AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
            }

            fontAsset.version = "1.1.0";
            fontAsset.faceInfo = FontEngine.GetFaceInfo();

            fontAsset.sourceFontFile = font;
            fontAsset.m_SourceFontFileGUID = AssetDatabase.AssetPathToGUID(sourceFontPath);
            fontAsset.m_SourceFontFile_EditorRef = font;

            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            fontAsset.clearDynamicDataOnBuild = TMP_Settings.clearDynamicDataOnBuild;

            int atlasWidth = fontAsset.atlasWidth = 1024;
            int atlasHeight = fontAsset.atlasHeight = 1024;
            int atlasPadding = fontAsset.atlasPadding = 9;

            Texture2D texture = new Texture2D(1, 1, TextureFormat.Alpha8, false);
            Shader shader = Shader.Find("TextMeshPro/Distance Field");
            int packingModifier = 1;
            Material mat = new Material(shader);

            mat.SetFloat(ShaderUtilities.ID_GradientScale, atlasPadding + packingModifier);
            mat.SetFloat(ShaderUtilities.ID_WeightNormal, fontAsset.normalStyle);
            mat.SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);

            texture.name = "PeaceSans Atlas";
            mat.name = "PeaceSans SDF Material";

            fontAsset.atlasTextures = new Texture2D[] { texture };
            AssetDatabase.AddObjectToAsset(texture, fontAsset);

            fontAsset.freeGlyphRects = new List<GlyphRect> { new GlyphRect(0, 0, atlasWidth - packingModifier, atlasHeight - packingModifier) };
            fontAsset.usedGlyphRects = new List<GlyphRect>();

            mat.SetTexture(ShaderUtilities.ID_MainTex, texture);
            mat.SetFloat(ShaderUtilities.ID_TextureWidth, atlasWidth);
            mat.SetFloat(ShaderUtilities.ID_TextureHeight, atlasHeight);

            fontAsset.material = mat;
            AssetDatabase.AddObjectToAsset(mat, fontAsset);

            fontAsset.creationSettings = new FontAssetCreationSettings(
                fontAsset.m_SourceFontFileGUID,
                (int)fontAsset.faceInfo.pointSize,
                0,
                atlasPadding,
                0,
                1024,
                1024,
                7,
                string.Empty,
                (int)GlyphRenderMode.SDFAA
            );

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[PeaceSansFontSetup] Peace Sans SDF asset generated successfully at {FontAssetPath} (GUID: {AssetDatabase.AssetPathToGUID(FontAssetPath)})");
        }

        public static void GenerateFontAssetBatch()
        {
            GenerateFontAsset();
        }
    }
}

