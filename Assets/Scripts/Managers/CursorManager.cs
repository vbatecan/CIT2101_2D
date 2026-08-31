using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.UI;

namespace CaseClosed.Managers
{
    /// <summary>
    /// Master cursor manager coordinating custom hardware/software cursors across Case 1-3 gameplay and menus.
    /// Uses the custom pointing arm cursor during investigation and dialogue in Cases 1-3,
    /// with an accurately calibrated hotspot at the fingertip for precise clicking on UI and table evidence.
    /// Resets back to the system default cursor in Main Menu and non-case views.
    /// </summary>
    public class CursorManager : MonoBehaviour
    {
        /// <summary>Singleton instance of CursorManager.</summary>
        public static CursorManager Instance { get; private set; }

        public enum CursorType
        {
            DefaultSystem,
            ArmPointer
        }

        [Header("Cursor Assets")]
        [Tooltip("The pointing arm cursor texture used during Case 1-3 gameplay.")]
        public Texture2D armCursorTexture;

        [Header("Hotspot & Mode Settings")]
        [Tooltip("Hotspot offset (in pixels from top-left) matching the tip of the pointing index finger.")]
        public Vector2 armHotspot = new Vector2(25f, 0f);

        [Tooltip("Cursor rendering mode: Auto (hardware cursor where supported) or ForceSoftware.")]
        public CursorMode cursorMode = CursorMode.Auto;

        [Header("State Tracking")]
        [SerializeField]
        private CursorType currentCursorType = CursorType.DefaultSystem;

        /// <summary>The currently active cursor type.</summary>
        public CursorType ActiveCursorType => currentCursorType;

        /// <summary>Event raised whenever the cursor type changes.</summary>
        public event Action<CursorType> OnCursorChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                LoadDefaultCursorResources();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void Start()
        {
            EnsurePhysics2DRaycasterOnCamera();
            HookCaseManagerEvents();
            EvaluateAndApplyCursorForState();
        }

        /// <summary>
        /// Attempts to load the custom arm cursor texture from Resources if not assigned via Inspector.
        /// </summary>
        public void LoadDefaultCursorResources()
        {
            if (armCursorTexture == null)
            {
                armCursorTexture = Resources.Load<Texture2D>("Cursors/ArmPointerCursor");
            }

            if (armCursorTexture == null)
            {
                armCursorTexture = Resources.Load<Texture2D>("Cursors/ArmPointerCursor_128");
            }
        }

        /// <summary>
        /// Subscribes to CaseManager events if present.
        /// </summary>
        public void HookCaseManagerEvents()
        {
            if (CaseManager.Instance != null)
            {
                CaseManager.Instance.OnCaseLoaded -= HandleCaseLoaded;
                CaseManager.Instance.OnCaseLoaded += HandleCaseLoaded;
            }
        }

        private void HandleCaseLoaded(CaseSO caseData)
        {
            if (caseData != null)
            {
                SetArmCursor();
            }
            else
            {
                SetDefaultCursor();
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsurePhysics2DRaycasterOnCamera();
            EvaluateAndApplyCursorForState();
        }

        /// <summary>
        /// Evaluates current active scene and UI state to determine whether to show Arm cursor or Default cursor.
        /// </summary>
        public void EvaluateAndApplyCursorForState()
        {
            string sceneName = SceneManager.GetActiveScene().name;

            // Check if active scene is dedicated Case scene (Case001, Case002, Case003)
            bool isCaseScene = sceneName.StartsWith("Case00", StringComparison.OrdinalIgnoreCase) ||
                               sceneName.Equals("Case001", StringComparison.OrdinalIgnoreCase) ||
                               sceneName.Equals("Case002", StringComparison.OrdinalIgnoreCase) ||
                               sceneName.Equals("Case003", StringComparison.OrdinalIgnoreCase);

            // Check if playing case inside Main scene
            bool isPlayingCaseInMain = (sceneName.Equals("Main", StringComparison.OrdinalIgnoreCase) || sceneName.Equals("Game", StringComparison.OrdinalIgnoreCase)) &&
                                       CaseManager.Instance != null && CaseManager.Instance.activeCase != null;

            // Check if explicitly in MainMenu
            bool isMainMenu = sceneName.Equals("MainMenu", StringComparison.OrdinalIgnoreCase) ||
                              (UIManager.Instance != null && UIManager.Instance.currentPanel == UIPanelType.MainMenu);

            if (isMainMenu)
            {
                SetDefaultCursor();
            }
            else if (isCaseScene || isPlayingCaseInMain)
            {
                SetArmCursor();
            }
            else
            {
                SetDefaultCursor();
            }
        }

        /// <summary>
        /// Switches the active OS mouse cursor to the custom pointing arm cursor.
        /// </summary>
        public void SetArmCursor()
        {
            if (armCursorTexture == null)
            {
                LoadDefaultCursorResources();
            }

            if (armCursorTexture != null)
            {
                Cursor.SetCursor(armCursorTexture, armHotspot, cursorMode);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                currentCursorType = CursorType.ArmPointer;
                Debug.Log($"[Managers:CursorManager] Switched cursor to Arm Pointer (Hotspot: {armHotspot}, Mode: {cursorMode})");
                OnCursorChanged?.Invoke(CursorType.ArmPointer);
            }
            else
            {
                Debug.LogWarning("[Managers:CursorManager] Arm cursor texture not found. Retaining system default cursor.");
                SetDefaultCursor();
            }
        }

        /// <summary>
        /// Resets the mouse cursor back to the standard OS system cursor.
        /// </summary>
        public void SetDefaultCursor()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            currentCursorType = CursorType.DefaultSystem;
            Debug.Log("[Managers:CursorManager] Reset cursor to System Default");
            OnCursorChanged?.Invoke(CursorType.DefaultSystem);
        }

        /// <summary>
        /// Ensures the active Main Camera has a Physics2DRaycaster component so 2D colliders
        /// (such as TableEvidenceItem) receive EventSystem clicks directly from the custom cursor.
        /// </summary>
        public void EnsurePhysics2DRaycasterOnCamera()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                if (cam.GetComponent<Physics2DRaycaster>() == null)
                {
                    cam.gameObject.AddComponent<Physics2DRaycaster>();
                    Debug.Log("[Managers:CursorManager] Attached Physics2DRaycaster to Main Camera for 2D evidence interaction.");
                }
            }
        }
    }
}
