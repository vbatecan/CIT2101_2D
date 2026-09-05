using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace CaseClosed.Editor
{
    /// <summary>
    /// Editor utility for project-wide UI clarity optimization.
    /// Sets dynamicPixelsPerUnit=3.0f, pixelPerfect=true, adds drop shadows, and scales up
    /// font sizes across all game scenes to eliminate blurriness and ensure crisp, production-grade text.
    /// </summary>
    public static class UIClarityOptimizationUtility
    {
        private static readonly string[] TargetScenes = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Main.unity",
            "Assets/Scenes/Case001.unity",
            "Assets/Scenes/Case002.unity",
            "Assets/Scenes/Case003.unity"
        };

        [MenuItem("Case Closed/UI/Optimize All Game Text & Canvas Clarity", false, 10)]
        public static void OptimizeAllScenes()
        {
            string initialScene = EditorSceneManager.GetActiveScene().path;
            int totalProcessed = 0;

            foreach (string scenePath in TargetScenes)
            {
                if (!File.Exists(scenePath)) continue;

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                OptimizeCurrentScene(scenePath);

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                totalProcessed++;
                Debug.Log($"[UIClarity] Successfully optimized text clarity in: '{scenePath}'");
            }

            if (!string.IsNullOrEmpty(initialScene) && File.Exists(initialScene))
            {
                EditorSceneManager.OpenScene(initialScene, OpenSceneMode.Single);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[UIClarity] Project-wide UI clarity optimization complete across {totalProcessed} scene(s)!");
        }

        public static void OptimizeCurrentScene(string scenePath = "")
        {
            // 1. Optimize all Canvases
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var canvas in canvases)
            {
                canvas.pixelPerfect = true;

                if (scenePath.Contains("MainMenu") || scenePath.EndsWith("Main.unity"))
                {
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                }

                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = 0.5f;
                    scaler.dynamicPixelsPerUnit = 3.0f; // Eliminates dynamic font bitmap blurriness!
                    EditorUtility.SetDirty(scaler);
                }

                EditorUtility.SetDirty(canvas);
            }

            // 2. If it's a menu scene, trigger clean Main Menu rebuild
            if (scenePath.Contains("MainMenu") || scenePath.EndsWith("Main.unity"))
            {
                MainMenuSetupUtility.RebuildMainMenuInCurrentScene();
            }

            // 3. Optimize all Text components: Add crisp drop shadow and enforce minimum readable sizes
            Text[] allTexts = Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var text in allTexts)
            {
                // Ensure drop shadow for pop and legibility
                Shadow shadow = text.GetComponent<Shadow>();
                if (shadow == null) shadow = text.gameObject.AddComponent<Shadow>();
                shadow.effectDistance = new Vector2(1.2f, -1.2f);
                shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
                shadow.useGraphicAlpha = true;

                // Enforce minimum readable font size on 1080p canvas
                if (text.fontSize > 0 && text.fontSize < 14)
                {
                    text.fontSize = 14;
                }

                EditorUtility.SetDirty(text);
                EditorUtility.SetDirty(shadow);
            }
        }
    }
}
