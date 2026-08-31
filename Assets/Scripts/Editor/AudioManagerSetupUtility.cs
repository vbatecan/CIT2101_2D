using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CaseClosed.Managers;

namespace CaseClosed.Editor
{
    /// <summary>
    /// Editor utility for automatically finding, creating, and wiring up the <see cref="AudioManager"/>
    /// and its associated audio clips across all scenes in the project.
    /// </summary>
    public static class AudioManagerSetupUtility
    {
        private static readonly string[] ScenePaths = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Case001.unity",
            "Assets/Scenes/Case002.unity",
            "Assets/Scenes/Main.unity"
        };

        [MenuItem("Case Closed/Audio/Configure Audio Manager in All Scenes", false, 10)]
        public static void ConfigureAllScenes()
        {
            int configuredCount = 0;
            string currentActiveScene = EditorSceneManager.GetActiveScene().path;

            foreach (string scenePath in ScenePaths)
            {
                if (!File.Exists(scenePath))
                {
                    Debug.LogWarning($"[AudioManagerSetup] Scene file not found at: {scenePath}");
                    continue;
                }

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                bool modified = SetupAudioManagerInCurrentScene();

                if (modified)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    configuredCount++;
                    Debug.Log($"[AudioManagerSetup] Successfully configured and saved AudioManager in scene: '{scenePath}'");
                }
            }

            // Return to initial scene if applicable
            if (!string.IsNullOrEmpty(currentActiveScene) && File.Exists(currentActiveScene))
            {
                EditorSceneManager.OpenScene(currentActiveScene, OpenSceneMode.Single);
            }

            Debug.Log($"[AudioManagerSetup] Audio Manager configuration complete! Configured {configuredCount} scene(s).");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Case Closed/Audio/Auto-Assign Clips to Selected Audio Manager", false, 11)]
        public static void ConfigureSelectedAudioManager()
        {
            AudioManager target = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<AudioManager>() : Object.FindFirstObjectByType<AudioManager>();
            if (target == null)
            {
                EditorUtility.DisplayDialog("Audio Manager Setup", "Please select a GameObject with an AudioManager component or open a scene containing one.", "OK");
                return;
            }

            Undo.RecordObject(target, "Auto-Assign Audio Clips");
            AssignClipsAndSources(target);
            EditorUtility.SetDirty(target);
            EditorSceneManager.MarkSceneDirty(target.gameObject.scene);
            Debug.Log($"[AudioManagerSetup] Auto-assigned audio clips to AudioManager on '{target.gameObject.name}'.");
        }

        /// <summary>
        /// Finds or creates an AudioManager GameObject in the active scene and assigns all clips and audio sources.
        /// </summary>
        /// <returns>True if modifications were made; otherwise false.</returns>
        public static bool SetupAudioManagerInCurrentScene()
        {
            AudioManager audioMgr = Object.FindFirstObjectByType<AudioManager>();
            GameObject managersRoot = GameObject.Find("_Managers");

            if (audioMgr == null)
            {
                GameObject audioObj = new GameObject("AudioManager");
                if (managersRoot != null)
                {
                    audioObj.transform.SetParent(managersRoot.transform, false);
                }
                audioMgr = audioObj.AddComponent<AudioManager>();
                Debug.Log("[AudioManagerSetup] Created new AudioManager GameObject in scene.");
            }
            else if (managersRoot != null && audioMgr.transform.parent == null)
            {
                audioMgr.transform.SetParent(managersRoot.transform, false);
            }

            AssignClipsAndSources(audioMgr);
            EditorUtility.SetDirty(audioMgr);
            return true;
        }

        /// <summary>
        /// Assigns all audio clips from Assets/Audio/ and configures 3 AudioSources (BGM, SFX, Typewriter).
        /// </summary>
        /// <param name="audioMgr">The target AudioManager component.</param>
        public static void AssignClipsAndSources(AudioManager audioMgr)
        {
            if (audioMgr == null) return;

            // 1. Configure Audio Sources on GameObject
            AudioSource[] sources = audioMgr.GetComponents<AudioSource>();
            AudioSource bgm = null;
            AudioSource sfx = null;
            AudioSource typewriter = null;

            if (sources.Length >= 1) bgm = sources[0];
            if (sources.Length >= 2) sfx = sources[1];
            if (sources.Length >= 3) typewriter = sources[2];

            if (bgm == null) bgm = audioMgr.gameObject.AddComponent<AudioSource>();
            if (sfx == null) sfx = audioMgr.gameObject.AddComponent<AudioSource>();
            if (typewriter == null) typewriter = audioMgr.gameObject.AddComponent<AudioSource>();

            // Setup 2D properties
            bgm.playOnAwake = false;
            bgm.loop = true;
            bgm.spatialBlend = 0f;
            bgm.volume = audioMgr.bgmVolume;

            sfx.playOnAwake = false;
            sfx.loop = false;
            sfx.spatialBlend = 0f;
            sfx.volume = audioMgr.sfxVolume;

            typewriter.playOnAwake = false;
            typewriter.loop = false;
            typewriter.spatialBlend = 0f;
            typewriter.volume = audioMgr.sfxVolume * 0.4f;

            audioMgr.bgmSource = bgm;
            audioMgr.sfxSource = sfx;
            audioMgr.typewriterSource = typewriter;

            // 2. Load Clips from Assets/Audio
            audioMgr.investigationBGM = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/INVESTIGATION BGM.mp3");
            audioMgr.interrogationBGM = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/INTERROGATION BGM.mp3");
            audioMgr.highTensionBGM = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/HIGH TENSION BGM.mp3");

            audioMgr.buttonClickSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BUTTON CLICK SFX.mp3");
            audioMgr.paperFlipSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/PAPER FLIP SFX.mp3");
            audioMgr.examineZoomSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/EXAMINE ZOOM SFX.mp3");
            audioMgr.typewriterKeySFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/TYPEWRITER KEY SFX.mp3");

            audioMgr.contradictionFoundSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/CONTRADICTION SFX.mp3");
            audioMgr.clueDiscoveredSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/CLUE DISCOVERED SFX.mp3");
            audioMgr.deductionLinkedSFX = audioMgr.clueDiscoveredSFX; // Fallback link
            audioMgr.caseSolvedSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/CASE SOLVED SFX.mp3");
            audioMgr.caseFailedSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/CASE FAILED SFX.mp3");
        }
    }
}
