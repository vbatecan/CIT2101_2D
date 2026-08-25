using System.Collections.Generic;
using UnityEngine;

namespace CaseClosed.Managers
{
    public class AudioManager : MonoBehaviour
    {
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

        private void EnsureAudioSources()
        {
            if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
            if (typewriterSource == null) typewriterSource = gameObject.AddComponent<AudioSource>();

            bgmSource.loop = true;
            sfxSource.loop = false;
            typewriterSource.loop = false;
        }

        public void PlayBGM(AudioClip clip)
        {
            if (clip == null || bgmSource.clip == clip) return;
            bgmSource.clip = clip;
            bgmSource.Play();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip);
        }

        public void PlayTypewriterKey()
        {
            if (typewriterKeySFX != null && !typewriterSource.isPlaying)
            {
                typewriterSource.PlayOneShot(typewriterKeySFX, 0.4f);
            }
        }

        public void PlayButtonClick() => PlaySFX(buttonClickSFX);
        public void PlayPaperFlip() => PlaySFX(paperFlipSFX);
        public void PlayExamineZoom() => PlaySFX(examineZoomSFX);
        public void PlayContradictionFound() => PlaySFX(contradictionFoundSFX);
        public void PlayClueDiscovered() => PlaySFX(clueDiscoveredSFX);
        public void PlayDeductionLinked() => PlaySFX(deductionLinkedSFX);
    }
}
