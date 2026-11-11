using UnityEngine;

namespace CaseClosed.Gameplay
{
    /// <summary>Moves the window silhouettes independently of their hand-drawn pose changes.</summary>
    public sealed class BackgroundPeopleMotion : MonoBehaviour
    {
        [Header("Window artwork")]
        [SerializeField] private SpriteRenderer originalRenderer;
        [SerializeField] private SpriteRenderer firstPerson;
        [SerializeField] private SpriteRenderer secondPerson;
        [SerializeField] private Sprite standingFrame;
        [SerializeField] private Sprite walkingFrame;
        [Tooltip("Pixel rectangles in the original transparent textures, measured from bottom left.")]
        [SerializeField] private Rect firstStandingRect = new Rect(159, 190, 47, 96);
        [SerializeField] private Rect firstWalkingRect = new Rect(267, 190, 55, 96);
        [SerializeField] private Rect secondStandingRect = new Rect(528, 190, 41, 87);
        [SerializeField] private Rect secondWalkingRect = new Rect(439, 190, 53, 89);

        [Header("Path in this object's local space")]
        [SerializeField] private Vector2 leftEndpoint = new Vector2(-3f, -1.111111f);
        [SerializeField] private Vector2 rightEndpoint = new Vector2(3f, -1.111111f);
        [SerializeField, Min(0.1f)] private float travelSeconds = 6f;
        [SerializeField, Min(0f)] private float poseRate = 6f;
        [SerializeField, Min(0.01f)] private float fadeSeconds = 0.3f;
        [SerializeField, Range(0f, 1f)] private float opacity = 0.8f;

        private readonly Sprite[] poses = new Sprite[4];
        private float elapsed;
        private bool initialized;

        private void Awake()
        {
            if (originalRenderer == null || firstPerson == null || secondPerson == null
                || standingFrame == null || walkingFrame == null)
            {
                Debug.LogError("Background people need both frames and all renderer references.", this);
                enabled = false;
                return;
            }

            poses[0] = Crop(standingFrame, firstStandingRect);
            poses[1] = Crop(walkingFrame, firstWalkingRect);
            poses[2] = Crop(standingFrame, secondStandingRect);
            poses[3] = Crop(walkingFrame, secondWalkingRect);
            originalRenderer.enabled = false;
            initialized = true;
        }

        private static Sprite Crop(Sprite source, Rect rect)
        {
            // Bottom-centred pivots keep the baseline stable as arm poses change.
            return Sprite.Create(source.texture, rect, new Vector2(0.5f, 0f),
                source.pixelsPerUnit, 0, SpriteMeshType.FullRect, Vector4.zero, false);
        }

        private void OnEnable()
        {
            if (!initialized) return;
            elapsed = 0f;
            DrawPeople();
        }

        private void Update()
        {
            if (!initialized) return;
            elapsed = Mathf.Repeat(elapsed + Time.deltaTime, Mathf.Max(0.1f, travelSeconds));
            DrawPeople();
        }

        private void DrawPeople()
        {
            float duration = Mathf.Max(0.1f, travelSeconds);
            Draw(firstPerson, 0, elapsed, leftEndpoint, rightEndpoint, duration);
            Draw(secondPerson, 2, Mathf.Repeat(elapsed + duration * 0.62f, duration),
                rightEndpoint, leftEndpoint, duration);
        }

        private void Draw(SpriteRenderer person, int poseOffset, float time,
            Vector2 start, Vector2 end, float duration)
        {
            if (person == null) return;
            person.enabled = true;
            person.sprite = poses[poseOffset + (Mathf.FloorToInt(time * poseRate) & 1)];
            Vector2 position = Vector2.Lerp(start, end, time / duration);
            person.transform.localPosition = new Vector3(position.x, position.y, 0f);
            float fade = Mathf.Min(Mathf.Max(0.01f, fadeSeconds), duration * 0.5f);
            float visibility = Mathf.SmoothStep(0f, 1f, Mathf.Min(time, duration - time) / fade);
            Color tint = person.color;
            tint.a = opacity * visibility;
            person.color = tint;
        }

        private void OnDisable()
        {
            if (firstPerson != null) firstPerson.enabled = false;
            if (secondPerson != null) secondPerson.enabled = false;
        }

        private void OnDestroy()
        {
            foreach (Sprite pose in poses)
                if (pose != null) Destroy(pose);
        }
    }
}
