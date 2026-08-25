using UnityEngine;
using CaseClosed.Gameplay;
using CaseClosed.Managers;
using CaseClosed.UI;

namespace CaseClosed.Prototype
{
    public class GameBootstrap : MonoBehaviour
    {
        private Case01Initializer level1;
        private Case02Initializer level2;
        private Case03Initializer level3;

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

        private void Update()
        {
            // Level selection shortcuts for testing / prototype navigation
            if (Input.GetKeyDown(KeyCode.Alpha1)) LoadLevel(1);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) LoadLevel(2);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) LoadLevel(3);
        }

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
