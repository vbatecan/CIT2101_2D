using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;
using CaseClosed.UI;

namespace CaseClosed.Gameplay
{
    /// <summary>Presentation only: enter through the door, then hand back to CharacterDisplay.</summary>
    public sealed class DoorEntranceSequence : MonoBehaviour
    {
        [System.Serializable]
        private sealed class Walker
        {
            public SpriteRenderer walkingRenderer;
            public SpriteRenderer seatedRenderer;
            public Sprite[] frames;
            public Transform doorway;
            public Transform destination;
            [Min(0f)] public float delay;
            [Min(0.1f)] public float travelSeconds = 3f;

            [System.NonSerialized] public Color seatedColor;
            [System.NonSerialized] public bool seatedHidden;
            [System.NonSerialized] public Vector3 walkingScale;
        }

        [System.Serializable]
        private sealed class CaseWalkCycle
        {
            [Min(1)] public int levelNumber = 1;
            public Sprite[] primaryFrames;
            public Sprite[] secondaryFrames;
            [Min(0.1f)] public float primaryHeight = 7.2f;
            [Min(0.1f)] public float secondaryHeight = 7f;
        }

        [Header("Scene References")]
        [SerializeField] private CaseManager caseManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private Sprite openBackground;
        [SerializeField] private SpriteRenderer doorLeaf;
        [SerializeField] private SpriteRenderer doorwayInterior;
        [SerializeField] private Walker[] walkers;

        [Header("Case Artwork")]
        [Tooltip("Primary and secondary walk cycles for each case, in the same order as CharacterDisplay slots.")]
        [SerializeField] private CaseWalkCycle[] caseCycles;

        [Header("Playback")]
        [Tooltip("Legacy single-case fallback used when no case cycles are configured.")]
        [SerializeField] private int caseLevel = 1;
        [SerializeField, Min(0.1f)] private float doorSeconds = 0.65f;
        [SerializeField, Min(0.01f)] private float settleSeconds = 0.35f;
        [SerializeField, Min(1f)] private float framesPerSecond = 10f;
        [SerializeField, Min(0f)] private float stepBob = 0.035f;

        private CaseManager publisher;
        private CaseSO pendingCase;
        private Sprite closedBackground;
        private Vector3 closedDoorScale;
        private bool interiorWasEnabled;
        private bool captured;
        private bool playing;
        private float elapsed;
        private float closeAt;

        public bool IsPlaying => playing;
        public bool IsEntrancePending => playing || pendingCase != null;

        public bool CanPlayForCase(CaseSO caseData)
        {
            return isActiveAndEnabled && SupportsCase(caseData) && background != null
                && openBackground != null && doorLeaf != null && walkers != null
                && walkers.Length >= 2 && IsConfigured(walkers[0]) && IsConfigured(walkers[1]);
        }

        private void Awake()
        {
            if (background != null) closedBackground = background.sprite;
            if (doorLeaf != null) closedDoorScale = doorLeaf.transform.localScale;
        }

        private void OnEnable()
        {
            publisher = caseManager;
            if (publisher != null) publisher.OnCaseLoaded += QueueEntrance;
        }

        private void Start()
        {
            if (caseManager != null) QueueEntrance(caseManager.ActiveCase);
        }

        private void OnDisable()
        {
            if (publisher != null) publisher.OnCaseLoaded -= QueueEntrance;
            publisher = null;
            pendingCase = null;
            RestorePresentation();
        }

        private void QueueEntrance(CaseSO caseData)
        {
            RestorePresentation();
            pendingCase = SupportsCase(caseData) ? caseData : null;
        }

        private CaseWalkCycle FindCycle(int levelNumber)
        {
            if (caseCycles == null) return null;
            foreach (CaseWalkCycle cycle in caseCycles)
            {
                if (cycle != null && cycle.levelNumber == levelNumber) return cycle;
            }
            return null;
        }

        private bool SupportsCase(CaseSO caseData)
        {
            if (caseData == null) return false;
            if (caseCycles == null || caseCycles.Length == 0) return caseData.levelNumber == caseLevel;
            CaseWalkCycle cycle = FindCycle(caseData.levelNumber);
            return cycle != null && cycle.primaryFrames != null && cycle.primaryFrames.Length > 0
                && cycle.secondaryFrames != null && cycle.secondaryFrames.Length > 0;
        }

        private bool SelectCaseArtwork()
        {
            CaseSO caseData = caseManager != null ? caseManager.ActiveCase : null;
            if (!SupportsCase(caseData)) return false;
            CaseWalkCycle cycle = FindCycle(caseData.levelNumber);
            if (cycle == null) return true; // Preserve the original single-case configuration.
            if (walkers == null || walkers.Length < 2 || walkers[0] == null || walkers[1] == null) return false;
            walkers[0].frames = cycle.primaryFrames;
            walkers[1].frames = cycle.secondaryFrames;
            SetWalkingHeight(walkers[0], cycle.primaryHeight);
            SetWalkingHeight(walkers[1], cycle.secondaryHeight);
            return true;
        }

        private static void SetWalkingHeight(Walker walker, float height)
        {
            if (walker.walkingRenderer == null || walker.frames[0] == null) return;
            float spriteHeight = walker.frames[0].bounds.size.y;
            if (spriteHeight > 0f) walker.walkingRenderer.transform.localScale = Vector3.one * (height / spriteHeight);
        }

        private void Update()
        {
            // Follow existing panel/time behavior without changing the timer or UI workflow.
            if (uiManager != null && uiManager.currentPanel != UIPanelType.InvestigationTable) return;
            if (!playing && pendingCase != null)
            {
                pendingCase = null;
                PlayEntrance();
            }
            if (!playing) return;
            elapsed += Time.deltaTime;
            Sample(elapsed);
            if (elapsed >= closeAt + doorSeconds) RestorePresentation();
        }

        [ContextMenu("Replay Entrance (Play Mode)")]
        public void PlayEntrance()
        {
            if (!Application.isPlaying || !isActiveAndEnabled) return;
            pendingCase = null;
            RestorePresentation();
            if (background == null || openBackground == null || doorLeaf == null || walkers == null) return;
            if (!SelectCaseArtwork()) return;

            closedBackground = background.sprite;
            closedDoorScale = doorLeaf.transform.localScale;
            interiorWasEnabled = doorwayInterior != null && doorwayInterior.enabled;
            closeAt = doorSeconds;
            foreach (Walker walker in walkers)
            {
                if (!IsConfigured(walker)) continue;
                walker.seatedColor = walker.seatedRenderer.color;
                walker.seatedHidden = walker.seatedRenderer.forceRenderingOff;
                walker.walkingScale = walker.walkingRenderer.transform.localScale;
                walker.seatedRenderer.forceRenderingOff = true;
                closeAt = Mathf.Max(closeAt, doorSeconds + walker.delay + walker.travelSeconds + settleSeconds);
            }
            captured = true;
            background.sprite = openBackground;
            if (doorwayInterior != null) doorwayInterior.enabled = true;
            elapsed = 0f;
            playing = true;
            Sample(0f);
        }

        private static bool IsConfigured(Walker walker)
        {
            return walker != null && walker.walkingRenderer != null && walker.seatedRenderer != null
                && walker.doorway != null && walker.destination != null
                && walker.frames != null && walker.frames.Length > 0;
        }

        private void Sample(float seconds)
        {
            float opening = Mathf.SmoothStep(0f, 1f, seconds / doorSeconds);
            if (seconds >= closeAt) opening = 1f - Mathf.SmoothStep(0f, 1f, (seconds - closeAt) / doorSeconds);
            Vector3 leafScale = closedDoorScale;
            // Orthographic projection of a leaf swinging away around the left hinge.
            leafScale.x *= Mathf.Cos(opening * Mathf.PI * 0.5f);
            doorLeaf.transform.localScale = leafScale;
            doorLeaf.enabled = opening < 0.999f;

            foreach (Walker walker in walkers)
            {
                if (!IsConfigured(walker)) continue;
                float localTime = seconds - doorSeconds - walker.delay;
                float travel = Mathf.Clamp01(localTime / walker.travelSeconds);
                float settling = Mathf.Clamp01((localTime - walker.travelSeconds) / settleSeconds);
                bool visible = localTime >= 0f && settling < 1f;
                walker.walkingRenderer.enabled = visible;
                if (visible)
                {
                    int frame = (int)(Mathf.Max(0f, localTime) * framesPerSecond) % walker.frames.Length;
                    walker.walkingRenderer.sprite = walker.frames[frame];
                    Vector3 position = Vector3.Lerp(walker.doorway.position, walker.destination.position, travel);
                    position.y += Mathf.Sin(localTime * framesPerSecond * Mathf.PI / 2f) * stepBob * (1f - settling);
                    position.y -= settling * 0.2f;
                    walker.walkingRenderer.transform.position = position;
                    Color tint = Color.white;
                    tint.a = Mathf.Clamp01(localTime / 0.12f) * (1f - settling);
                    walker.walkingRenderer.color = tint;
                }
                walker.seatedRenderer.forceRenderingOff = settling <= 0f || walker.seatedHidden;
                Color seatedTint = walker.seatedColor;
                seatedTint.a *= settling;
                walker.seatedRenderer.color = seatedTint;
            }
        }

        private void RestorePresentation()
        {
            playing = false;
            if (!captured) return;
            if (background != null) background.sprite = closedBackground;
            if (doorLeaf != null)
            {
                doorLeaf.enabled = false;
                doorLeaf.transform.localScale = closedDoorScale;
            }
            if (doorwayInterior != null) doorwayInterior.enabled = interiorWasEnabled;
            foreach (Walker walker in walkers)
            {
                if (!IsConfigured(walker)) continue;
                walker.walkingRenderer.enabled = false;
                walker.walkingRenderer.transform.localScale = walker.walkingScale;
                walker.seatedRenderer.color = walker.seatedColor;
                walker.seatedRenderer.forceRenderingOff = walker.seatedHidden;
            }
            captured = false;
        }
    }
}
