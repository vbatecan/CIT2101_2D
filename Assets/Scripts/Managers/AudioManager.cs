using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CaseClosed.Managers
{
    /// <summary>
    /// Controller MonoBehaviour managing background music and audio sound effects playback.
    /// Can be dragged directly onto a GameObject in the Unity Inspector.
    /// Automatically self-heals by assigning audio clips from Assets/Audio in Editor or runtime fallback.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private const string PrefKeyBgmVolume = "CaseClosed_BGM_Volume";
        private const string PrefKeySfxVolume = "CaseClosed_SFX_Volume";
        private const string PrefKeyTypewriterEnabled = "CaseClosed_Typewriter_Enabled";

        /// <summary>Singleton instance of the AudioManager.</summary>
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        public AudioSource bgmSource;
        public AudioSource sfxSource;
        public AudioSource typewriterSource;

        [Header("Audio Clips - Music")]
        [Tooltip("Main menu and table investigation background theme.")]
        public AudioClip investigationBGM;

        [Tooltip("Dialogue and suspect interrogation background theme.")]
        public AudioClip interrogationBGM;

        [Tooltip("Critical contradiction and high-tension confrontation theme.")]
        public AudioClip highTensionBGM;

        [Header("Audio Clips - UI SFX")]
        [Tooltip("Standard UI button click sound.")]
        public AudioClip buttonClickSFX;

        [Tooltip("Notebook page and dossier tab flip sound.")]
        public AudioClip paperFlipSFX;

        [Tooltip("Evidence inspect modal zoom sound.")]
        public AudioClip examineZoomSFX;

        [Tooltip("Dialogue typewriter key click sound.")]
        public AudioClip typewriterKeySFX;

        [Header("Audio Clips - Gameplay SFX")]
        [Tooltip("Contradiction exposed / objection sting.")]
        public AudioClip contradictionFoundSFX;

        [Tooltip("Clue discovered notification chime.")]
        public AudioClip clueDiscoveredSFX;

        [Tooltip("Deduction board connection success sound.")]
        public AudioClip deductionLinkedSFX;

        [Tooltip("Case solved scorecard victory fanfare.")]
        public AudioClip caseSolvedSFX;

        [Tooltip("Case failed / incorrect accusation sound.")]
        public AudioClip caseFailedSFX;

        [Header("Audio Settings")]
        [Range(0f, 1f)] public float bgmVolume = 0.8f;
        [Range(0f, 1f)] public float sfxVolume = 1f;
        public bool isTypewriterEnabled = true;

        private Coroutine crossFadeCoroutine;

        /// <summary>
        /// Initializes the singleton instance, loads saved volume settings, and configures audio sources.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadSavedAudioSettings();
                EnsureAudioSources();
                AutoAssignClipsIfMissing();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Loads user-configured volume and typewriter settings from PlayerPrefs.
        /// </summary>
        private void LoadSavedAudioSettings()
        {
            bgmVolume = PlayerPrefs.GetFloat(PrefKeyBgmVolume, 0.8f);
            sfxVolume = PlayerPrefs.GetFloat(PrefKeySfxVolume, 1.0f);
            isTypewriterEnabled = PlayerPrefs.GetInt(PrefKeyTypewriterEnabled, 1) == 1;
        }

        /// <summary>
        /// Verifies and attaches required AudioSource components if not assigned in Inspector.
        /// </summary>
        public void EnsureAudioSources()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.playOnAwake = false;
                bgmSource.spatialBlend = 0f; // 2D Audio
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.spatialBlend = 0f; // 2D Audio
            }

            if (typewriterSource == null)
            {
                typewriterSource = gameObject.AddComponent<AudioSource>();
                typewriterSource.playOnAwake = false;
                typewriterSource.spatialBlend = 0f; // 2D Audio
            }

            bgmSource.loop = true;
            sfxSource.loop = false;
            typewriterSource.loop = false;

            ApplyVolumes();
        }

        /// <summary>
        /// Applies current volume settings to audio sources.
        /// </summary>
        public void ApplyVolumes()
        {
            if (bgmSource != null) bgmSource.volume = bgmVolume;
            if (sfxSource != null) sfxSource.volume = sfxVolume;
            if (typewriterSource != null) typewriterSource.volume = sfxVolume * 0.4f;
        }

        /// <summary>
        /// Sets BGM volume level (0.0 to 1.0) and persists the value to PlayerPrefs.
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(PrefKeyBgmVolume, bgmVolume);
            PlayerPrefs.Save();
            if (bgmSource != null) bgmSource.volume = bgmVolume;
        }

        /// <summary>
        /// Sets SFX volume level (0.0 to 1.0) and persists the value to PlayerPrefs.
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(PrefKeySfxVolume, sfxVolume);
            PlayerPrefs.Save();
            if (sfxSource != null) sfxSource.volume = sfxVolume;
            if (typewriterSource != null) typewriterSource.volume = sfxVolume * 0.4f;
        }

        /// <summary>
        /// Toggles typewriter sound effect clicks and persists the value to PlayerPrefs.
        /// </summary>
        public void SetTypewriterEnabled(bool enabled)
        {
            isTypewriterEnabled = enabled;
            PlayerPrefs.SetInt(PrefKeyTypewriterEnabled, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Plays looping background music clip, ignoring if already playing the same clip.
        /// </summary>
        /// <param name="clip">The music audio clip to play.</param>
        public void PlayBGM(AudioClip clip)
        {
            if (clip == null) return;
            if (bgmSource != null && bgmSource.clip == clip && bgmSource.isPlaying) return;

            if (crossFadeCoroutine != null)
            {
                StopCoroutine(crossFadeCoroutine);
                crossFadeCoroutine = null;
            }

            if (bgmSource != null)
            {
                Debug.Log($"[AudioManager] Playing BGM track: '{clip.name}'");
                bgmSource.clip = clip;
                bgmSource.volume = bgmVolume;
                bgmSource.Play();
            }
        }

        /// <summary>
        /// Performs a smooth cross-fade transition to a new BGM track over a specified duration.
        /// </summary>
        /// <param name="newClip">The target audio clip to cross-fade into.</param>
        /// <param name="duration">Duration of the fade transition in seconds.</param>
        public void CrossFadeBGM(AudioClip newClip, float duration = 1.0f)
        {
            if (newClip == null) return;
            if (bgmSource != null && bgmSource.clip == newClip && bgmSource.isPlaying) return;

            if (crossFadeCoroutine != null)
            {
                StopCoroutine(crossFadeCoroutine);
            }
            crossFadeCoroutine = StartCoroutine(CrossFadeCoroutine(newClip, Mathf.Max(0.1f, duration)));
        }

        private IEnumerator CrossFadeCoroutine(AudioClip targetClip, float duration)
        {
            if (bgmSource == null) yield break;

            float startVolume = bgmSource.isPlaying ? bgmSource.volume : 0f;
            float halfDuration = duration * 0.5f;

            // Fade out current track
            if (bgmSource.isPlaying && startVolume > 0.01f)
            {
                for (float t = 0f; t < halfDuration; t += Time.unscaledDeltaTime)
                {
                    bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / halfDuration);
                    yield return null;
                }
            }

            // Switch clip & play
            bgmSource.clip = targetClip;
            bgmSource.Play();

            // Fade in new track
            for (float t = 0f; t < halfDuration; t += Time.unscaledDeltaTime)
            {
                bgmSource.volume = Mathf.Lerp(0f, bgmVolume, t / halfDuration);
                yield return null;
            }

            bgmSource.volume = bgmVolume;
            crossFadeCoroutine = null;
        }

        /// <summary>Stops background music with an optional quick fade-out.</summary>
        public void StopBGM(float fadeDuration = 0.5f)
        {
            if (bgmSource == null || !bgmSource.isPlaying) return;

            if (fadeDuration <= 0.01f)
            {
                bgmSource.Stop();
                bgmSource.clip = null;
            }
            else
            {
                StartCoroutine(FadeOutBGMCoroutine(fadeDuration));
            }
        }

        private IEnumerator FadeOutBGMCoroutine(float duration)
        {
            if (bgmSource == null) yield break;
            float initialVol = bgmSource.volume;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                if (bgmSource != null) bgmSource.volume = Mathf.Lerp(initialVol, 0f, t / duration);
                yield return null;
            }
            if (bgmSource != null)
            {
                bgmSource.Stop();
                bgmSource.clip = null;
                bgmSource.volume = bgmVolume;
            }
        }

        /// <summary>Plays the main menu / investigation background music theme.</summary>
        public void PlayMenuBGM() => PlayBGM(investigationBGM);

        /// <summary>Plays the investigation desk background music theme.</summary>
        public void PlayInvestigationBGM() => PlayBGM(investigationBGM);

        /// <summary>Plays the interrogation dialogue background music theme.</summary>
        public void PlayInterrogationBGM() => PlayBGM(interrogationBGM);

        /// <summary>Plays the high-tension confrontation background music theme.</summary>
        public void PlayHighTensionBGM() => PlayBGM(highTensionBGM);

        /// <summary>
        /// Plays a one-shot sound effect.
        /// </summary>
        /// <param name="clip">The sound effect audio clip to play.</param>
        public void PlaySFX(AudioClip clip)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        /// <summary>
        /// Plays a typewriter key click sound effect if not already playing.
        /// </summary>
        public void PlayTypewriterKey()
        {
            if (isTypewriterEnabled && typewriterKeySFX != null && typewriterSource != null && !typewriterSource.isPlaying)
            {
                typewriterSource.PlayOneShot(typewriterKeySFX, sfxVolume * 0.4f);
            }
        }

        /// <summary>Plays standard button click sound effect.</summary>
        public void PlayButtonClick() => PlaySFX(buttonClickSFX);

        /// <summary>Plays notebook paper flip sound effect.</summary>
        public void PlayPaperFlip() => PlaySFX(paperFlipSFX);

        /// <summary>Plays evidence inspect zoom sound effect.</summary>
        public void PlayExamineZoom() => PlaySFX(examineZoomSFX);

        /// <summary>Plays contradiction exposed sound effect.</summary>
        public void PlayContradictionFound() => PlaySFX(contradictionFoundSFX);

        /// <summary>Plays clue discovery sound effect.</summary>
        public void PlayClueDiscovered() => PlaySFX(clueDiscoveredSFX);

        /// <summary>Plays deduction link success sound effect.</summary>
        public void PlayDeductionLinked() => PlaySFX(deductionLinkedSFX != null ? deductionLinkedSFX : clueDiscoveredSFX);

        /// <summary>Plays case solved victory fanfare.</summary>
        public void PlayCaseSolved() => PlaySFX(caseSolvedSFX);

        /// <summary>Plays case failed sound effect.</summary>
        public void PlayCaseFailed() => PlaySFX(caseFailedSFX);

        /// <summary>
        /// Self-healing fallback to auto-assign clips from Assets/Audio if any are missing.
        /// </summary>
        public void AutoAssignClipsIfMissing()
        {
#if UNITY_EDITOR
            AutoAssignClipsFromAssetsAudio();
#endif
        }

#if UNITY_EDITOR
        private void Reset()
        {
            EnsureAudioSources();
            AutoAssignClipsFromAssetsAudio();
        }

        private void OnValidate()
        {
            if (bgmSource != null) bgmSource.volume = bgmVolume;
            if (sfxSource != null) sfxSource.volume = sfxVolume;
            if (typewriterSource != null) typewriterSource.volume = sfxVolume * 0.4f;
        }

        /// <summary>
        /// Automatically loads and binds all audio clips from Assets/Audio/ into their corresponding fields in the Editor.
        /// </summary>
        [ContextMenu("Auto-Assign Audio Clips from Assets/Audio")]
        public void AutoAssignClipsFromAssetsAudio()
        {
            if (investigationBGM == null) investigationBGM = LoadClip("Assets/Audio/INVESTIGATION BGM.mp3");
            if (interrogationBGM == null) interrogationBGM = LoadClip("Assets/Audio/INTERROGATION BGM.mp3");
            if (highTensionBGM == null) highTensionBGM = LoadClip("Assets/Audio/HIGH TENSION BGM.mp3");

            if (buttonClickSFX == null) buttonClickSFX = LoadClip("Assets/Audio/BUTTON CLICK SFX.mp3");
            if (paperFlipSFX == null) paperFlipSFX = LoadClip("Assets/Audio/PAPER FLIP SFX.mp3");
            if (examineZoomSFX == null) examineZoomSFX = LoadClip("Assets/Audio/EXAMINE ZOOM SFX.mp3");
            if (typewriterKeySFX == null) typewriterKeySFX = LoadClip("Assets/Audio/TYPEWRITER KEY SFX.mp3");

            if (contradictionFoundSFX == null) contradictionFoundSFX = LoadClip("Assets/Audio/CONTRADICTION SFX.mp3");
            if (clueDiscoveredSFX == null) clueDiscoveredSFX = LoadClip("Assets/Audio/CLUE DISCOVERED SFX.mp3");
            if (deductionLinkedSFX == null) deductionLinkedSFX = LoadClip("Assets/Audio/CLUE DISCOVERED SFX.mp3");
            if (caseSolvedSFX == null) caseSolvedSFX = LoadClip("Assets/Audio/CASE SOLVED SFX.mp3");
            if (caseFailedSFX == null) caseFailedSFX = LoadClip("Assets/Audio/CASE FAILED SFX.mp3");
        }

        private static AudioClip LoadClip(string path)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
#endif
    }
}
