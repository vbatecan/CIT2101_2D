using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CaseClosed.Managers
{
    public class AudioManager : MonoBehaviour
    {
        private const string PrefKeyBgmVolume = "CaseClosed_BGM_Volume";
        private const string PrefKeyBgmMuted = "CaseClosed_BGM_Muted";
        private const string PrefKeySfxVolume = "CaseClosed_SFX_Volume";
        private const string PrefKeySfxMuted = "CaseClosed_SFX_Muted";
        private const string PrefKeyDialogVolume = "CaseClosed_Dialog_Volume";
        private const string PrefKeyDialogMuted = "CaseClosed_Dialog_Muted";
        private const string PrefKeyTypewriterEnabled = "CaseClosed_Typewriter_Enabled";

        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        public AudioSource bgmSource;
        public AudioSource sfxSource;
        public AudioSource typewriterSource;

        [SerializeField, Tooltip("Loop played while character dialogue is being revealed.")]
        private AudioClip mumblingSFX;

        private AudioSource mumblingSource;
        private Object mumblingOwner;

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

        [Tooltip("Clock ticking sound effect when investigation timer is low.")]
        public AudioClip clockTickSFX;

        [Header("Audio Settings")]
        [Range(0f, 1f)] public float bgmVolume = 0.8f;
        public bool isBgmMuted = false;
        [Range(0f, 1f)] public float sfxVolume = 1f;
        public bool isSfxMuted = false;
        [Range(0f, 1f)] public float dialogVolume = 1f;
        public bool isDialogMuted = false;
        public bool isTypewriterEnabled = true;

        private Coroutine crossFadeCoroutine;

        private void Awake()
        {
            Instance = this;
            LoadSavedAudioSettings();
            EnsureAudioSources();
            AutoAssignClipsIfMissing();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void LoadSavedAudioSettings()
        {
            bgmVolume = PlayerPrefs.GetFloat(PrefKeyBgmVolume, 0.8f);
            isBgmMuted = PlayerPrefs.GetInt(PrefKeyBgmMuted, 0) == 1;

            sfxVolume = PlayerPrefs.GetFloat(PrefKeySfxVolume, 1.0f);
            isSfxMuted = PlayerPrefs.GetInt(PrefKeySfxMuted, 0) == 1;

            dialogVolume = PlayerPrefs.GetFloat(PrefKeyDialogVolume, 1.0f);
            isDialogMuted = PlayerPrefs.GetInt(PrefKeyDialogMuted, 0) == 1;

            isTypewriterEnabled = PlayerPrefs.GetInt(PrefKeyTypewriterEnabled, 1) == 1;
        }

        public void EnsureAudioSources()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.playOnAwake = false;
                bgmSource.spatialBlend = 0f;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.spatialBlend = 0f;
            }

            if (typewriterSource == null)
            {
                typewriterSource = gameObject.AddComponent<AudioSource>();
                typewriterSource.playOnAwake = false;
                typewriterSource.spatialBlend = 0f;
            }

            bgmSource.loop = true;
            sfxSource.loop = false;
            typewriterSource.loop = false;

            if (mumblingSource == null)
            {
                mumblingSource = gameObject.AddComponent<AudioSource>();
                mumblingSource.playOnAwake = false;
                mumblingSource.spatialBlend = 0f;
                mumblingSource.loop = true;
            }

            ApplyVolumes();
        }

        public void ApplyVolumes()
        {
            if (bgmSource != null) bgmSource.volume = isBgmMuted ? 0f : bgmVolume;
            if (sfxSource != null) sfxSource.volume = isSfxMuted ? 0f : sfxVolume;
            if (typewriterSource != null) typewriterSource.volume = isDialogMuted ? 0f : (dialogVolume * 0.5f);
            if (mumblingSource != null) mumblingSource.volume = isDialogMuted ? 0f : dialogVolume;
        }

        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(PrefKeyBgmVolume, bgmVolume);
            PlayerPrefs.Save();
            ApplyVolumes();
        }

        public void SetBgmMuted(bool muted)
        {
            isBgmMuted = muted;
            PlayerPrefs.SetInt(PrefKeyBgmMuted, muted ? 1 : 0);
            PlayerPrefs.Save();
            ApplyVolumes();
        }

        public void ToggleBgmMute() => SetBgmMuted(!isBgmMuted);

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(PrefKeySfxVolume, sfxVolume);
            PlayerPrefs.Save();
            ApplyVolumes();
        }

        public void SetSfxMuted(bool muted)
        {
            isSfxMuted = muted;
            PlayerPrefs.SetInt(PrefKeySfxMuted, muted ? 1 : 0);
            PlayerPrefs.Save();
            ApplyVolumes();
        }

        public void ToggleSfxMute() => SetSfxMuted(!isSfxMuted);

        public void SetDialogVolume(float volume)
        {
            dialogVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(PrefKeyDialogVolume, dialogVolume);
            PlayerPrefs.Save();
            ApplyVolumes();
        }

        public void SetDialogMuted(bool muted)
        {
            isDialogMuted = muted;
            PlayerPrefs.SetInt(PrefKeyDialogMuted, muted ? 1 : 0);
            PlayerPrefs.Save();
            ApplyVolumes();
        }

        public void ToggleDialogMute() => SetDialogMuted(!isDialogMuted);

        public void ResetAudioSettingsToDefault()
        {
            bgmVolume = 0.8f;
            isBgmMuted = false;
            sfxVolume = 1.0f;
            isSfxMuted = false;
            dialogVolume = 1.0f;
            isDialogMuted = false;
            isTypewriterEnabled = true;

            PlayerPrefs.SetFloat(PrefKeyBgmVolume, bgmVolume);
            PlayerPrefs.SetInt(PrefKeyBgmMuted, 0);
            PlayerPrefs.SetFloat(PrefKeySfxVolume, sfxVolume);
            PlayerPrefs.SetInt(PrefKeySfxMuted, 0);
            PlayerPrefs.SetFloat(PrefKeyDialogVolume, dialogVolume);
            PlayerPrefs.SetInt(PrefKeyDialogMuted, 0);
            PlayerPrefs.SetInt(PrefKeyTypewriterEnabled, 1);
            PlayerPrefs.Save();

            ApplyVolumes();
        }

        public void SetTypewriterEnabled(bool enabled)
        {
            isTypewriterEnabled = enabled;
            PlayerPrefs.SetInt(PrefKeyTypewriterEnabled, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

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

            if (bgmSource.isPlaying && startVolume > 0.01f)
            {
                for (float t = 0f; t < halfDuration; t += Time.unscaledDeltaTime)
                {
                    bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / halfDuration);
                    yield return null;
                }
            }

            bgmSource.clip = targetClip;
            bgmSource.Play();

            for (float t = 0f; t < halfDuration; t += Time.unscaledDeltaTime)
            {
                bgmSource.volume = Mathf.Lerp(0f, bgmVolume, t / halfDuration);
                yield return null;
            }

            bgmSource.volume = bgmVolume;
            crossFadeCoroutine = null;
        }

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

        public void PlayMenuBGM() => PlayBGM(investigationBGM);

        public void PlayInvestigationBGM() => PlayBGM(investigationBGM);

        public void PlayInterrogationBGM() => PlayBGM(interrogationBGM);

        public void PlayHighTensionBGM() => PlayBGM(highTensionBGM);

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null || sfxSource == null || isSfxMuted) return;
            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        public void PlayTypewriterKey()
        {
            if (isTypewriterEnabled && !isDialogMuted && typewriterKeySFX != null && typewriterSource != null && !typewriterSource.isPlaying)
            {
                typewriterSource.PlayOneShot(typewriterKeySFX, dialogVolume * 0.5f);
            }
        }

        public void StartDialogueMumbling(Object owner)
        {
            if (mumblingSource == null || mumblingSFX == null || owner == null) return;

            mumblingSource.Stop();
            mumblingOwner = owner;
            mumblingSource.clip = mumblingSFX;
            mumblingSource.Play();
        }

        public void StopDialogueMumbling(Object owner)
        {
            if (mumblingOwner != owner) return;

            if (mumblingSource != null) mumblingSource.Stop();
            mumblingOwner = null;
        }

        public void PlayButtonClick() => PlaySFX(buttonClickSFX);

        public void PlayPaperFlip() => PlaySFX(paperFlipSFX);

        public void PlayExamineZoom() => PlaySFX(examineZoomSFX);

        public void PlayContradictionFound() => PlaySFX(contradictionFoundSFX);

        public void PlayClueDiscovered() => PlaySFX(clueDiscoveredSFX);

        public void PlayDeductionLinked() => PlaySFX(deductionLinkedSFX != null ? deductionLinkedSFX : clueDiscoveredSFX);

        public void PlayCaseSolved() => PlaySFX(caseSolvedSFX);

        public void PlayCaseFailed() => PlaySFX(caseFailedSFX);

        public void PlayClockTick()
        {
            if (clockTickSFX != null)
            {
                PlaySFX(clockTickSFX);
            }
            else if (buttonClickSFX != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(buttonClickSFX, sfxVolume * 0.45f);
            }
            else if (typewriterKeySFX != null && typewriterSource != null)
            {
                typewriterSource.PlayOneShot(typewriterKeySFX, sfxVolume * 0.45f);
            }
        }

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
            if (mumblingSFX == null) mumblingSFX = LoadClip("Assets/Audio/MUMBLING SFX.mp3");

            if (contradictionFoundSFX == null) contradictionFoundSFX = LoadClip("Assets/Audio/CONTRADICTION SFX.mp3");
            if (clueDiscoveredSFX == null) clueDiscoveredSFX = LoadClip("Assets/Audio/CLUE DISCOVERED SFX.mp3");
            if (deductionLinkedSFX == null) deductionLinkedSFX = LoadClip("Assets/Audio/CLUE DISCOVERED SFX.mp3");
            if (caseSolvedSFX == null) caseSolvedSFX = LoadClip("Assets/Audio/CASE SOLVED SFX.mp3");
            if (caseFailedSFX == null) caseFailedSFX = LoadClip("Assets/Audio/CASE FAILED SFX.mp3");
            if (clockTickSFX == null) clockTickSFX = LoadClip("Assets/Audio/CLOCK TICKING LAST 10 SECONDS SFX.mp3");
        }

        private static AudioClip LoadClip(string path)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
#endif
    }
}
