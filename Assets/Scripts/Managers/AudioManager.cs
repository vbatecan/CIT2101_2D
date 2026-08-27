using UnityEngine;

namespace CaseClosed.Managers
{
    /// <summary>
    /// Controller MonoBehaviour managing background music and audio sound effects playback.
    /// Can be dragged directly onto a GameObject in the Unity Inspector.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        /// <summary>Singleton instance of the AudioManager.</summary>
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        public AudioSource bgmSource;
        public AudioSource sfxSource;
        public AudioSource typewriterSource;

        [Header("Audio Clips - Music")]
        public AudioClip investigationBGM;
        public AudioClip interrogationBGM;
        public AudioClip highTensionBGM;

        [Header("Audio Clips - SFX")]
        public AudioClip buttonClickSFX;
        public AudioClip paperFlipSFX;
        public AudioClip examineZoomSFX;
        public AudioClip typewriterKeySFX;
        public AudioClip contradictionFoundSFX;
        public AudioClip clueDiscoveredSFX;
        public AudioClip deductionLinkedSFX;
        public AudioClip caseSolvedSFX;
        public AudioClip caseFailedSFX;

        /// <summary>
        /// Initializes the singleton instance and configures persistent audio sources.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                EnsureAudioSources();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Verifies and attaches required AudioSource components if not assigned in Inspector.
        /// </summary>
        private void EnsureAudioSources()
        {
            if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
            if (typewriterSource == null) typewriterSource = gameObject.AddComponent<AudioSource>();

            bgmSource.loop = true;
            sfxSource.loop = false;
            typewriterSource.loop = false;
        }

        /// <summary>
        /// Plays looping background music clip, ignoring if already playing the same clip.
        /// </summary>
        /// <param name="clip">The music audio clip to play.</param>
        public void PlayBGM(AudioClip clip)
        {
            if (clip == null || (bgmSource != null && bgmSource.clip == clip && bgmSource.isPlaying)) return;
            if (bgmSource != null)
            {
                Debug.Log($"[Audio] Playing BGM track: '{clip.name}'");
                bgmSource.clip = clip;
                bgmSource.Play();
            }
        }

        /// <summary>
        /// Plays a one-shot sound effect.
        /// </summary>
        /// <param name="clip">The sound effect audio clip to play.</param>
        public void PlaySFX(AudioClip clip)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip);
        }

        /// <summary>
        /// Plays a typewriter key click sound effect if not already playing.
        /// </summary>
        public void PlayTypewriterKey()
        {
            if (typewriterKeySFX != null && typewriterSource != null && !typewriterSource.isPlaying)
            {
                typewriterSource.PlayOneShot(typewriterKeySFX, 0.4f);
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
        public void PlayDeductionLinked() => PlaySFX(deductionLinkedSFX);
    }
}
