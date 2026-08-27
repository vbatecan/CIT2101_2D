using UnityEngine;
using CaseClosed.Enums;
using CaseClosed.Gameplay;
using CaseClosed.Managers;
using CaseClosed.UI;

namespace CaseClosed.Prototype
{
    /// <summary>
    /// Entrypoint bootstrapping MonoBehaviour that initializes all managers, attaches required camera scripts,
    /// and handles keyboard shortcuts (Keys 1, 2, 3) for quick level switching during prototyping.
    /// Can be dragged directly onto a GameBootstrap GameObject in the Unity Inspector.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        private Case01Initializer level1;
        private Case02Initializer level2;
        private Case03Initializer level3;

        /// <summary>
        /// Ensures all singleton managers exist, sets up fixed camera, and registers level initializers.
        /// </summary>
        private void Awake()
        {
            SetupFixedCamera();
            EnsureManager<AudioManager>();
            EnsureManager<CaseManager>();
            EnsureManager<EvidenceManager>();
            EnsureManager<InterrogationManager>();
            EnsureManager<DeductionBoardController>();
            EnsureManager<CaseConclusionManager>();
            EnsureManager<UIManager>();

            level1 = gameObject.GetComponent<Case01Initializer>();
            if (level1 == null) level1 = gameObject.AddComponent<Case01Initializer>();

            level2 = gameObject.GetComponent<Case02Initializer>();
            if (level2 == null) level2 = gameObject.AddComponent<Case02Initializer>();

            level3 = gameObject.GetComponent<Case03Initializer>();
            if (level3 == null) level3 = gameObject.AddComponent<Case03Initializer>();
        }

        /// <summary>
        /// Listens for number key presses (1, 2, 3) to dynamically switch active cases.
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) LoadLevel(1);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) LoadLevel(2);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) LoadLevel(3);
        }

        /// <summary>
        /// Loads the data corresponding to a specified case index and initializes managers.
        /// </summary>
        /// <param name="levelIndex">The 1-based level index (1, 2, or 3).</param>
        public void LoadLevel(int levelIndex)
        {
            CaseClosed.Data.CaseSO caseData = null;

            switch (levelIndex)
            {
                case 1:
                    if (level1 != null) caseData = level1.CreateCase01Data();
                    break;
                case 2:
                    if (level2 != null) caseData = level2.CreateCase02Data();
                    break;
                case 3:
                    if (level3 != null) caseData = level3.CreateCase03Data();
                    break;
            }

            if (caseData != null)
            {
                CaseManager.Instance?.LoadCase(caseData);
                if (InterrogationManager.Instance != null && caseData.primarySuspect != null && caseData.dialogueTrees.Count > 0)
                {
                    InterrogationManager.Instance.SetInterrogationTarget(caseData.primarySuspect, caseData.dialogueTrees[0]);
                }
                UIManager.Instance?.ShowPanel(UIPanelType.InvestigationTable);
            }
        }

        /// <summary>
        /// Locates or instantiates the Main Camera and ensures the <see cref="FixedInvestigationCamera"/> script is attached.
        /// </summary>
        private void SetupFixedCamera()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                camObj.tag = "MainCamera";
                mainCam = camObj.GetComponent<Camera>();
            }

            if (mainCam.GetComponent<FixedInvestigationCamera>() == null)
            {
                mainCam.gameObject.AddComponent<FixedInvestigationCamera>();
            }
        }

        /// <summary>
        /// Locates an existing manager MonoBehaviour in the scene or creates a new GameObject with the component.
        /// </summary>
        /// <typeparam name="T">The MonoBehaviour manager type to ensure.</typeparam>
        /// <returns>The existing or newly instantiated manager component.</returns>
        private T EnsureManager<T>() where T : MonoBehaviour
        {
            T manager = FindFirstObjectByType<T>();
            if (manager == null)
            {
                GameObject managerObj = new GameObject(typeof(T).Name);
                manager = managerObj.AddComponent<T>();
            }
            return manager;
        }
    }
}
