using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Gameplay;
using CaseClosed.Managers;
using CaseClosed.Services;
using CaseClosed.UI;

namespace CaseClosed.Tests
{
    [TestFixture]
    public class SuspectFolderUITests
    {
        private GameObject _testRoot;
        private UIManager _uiManager;
        private SuspectFolderUI _suspectFolderUI;
        private SuspectFolderButton _suspectFolderBtn;
        private SuspectFolderService _service;
        private Texture2D _dummyTex;
        private Sprite _dummySprite1;
        private Sprite _dummySprite2;
        private Sprite _dummySprite3;

        [SetUp]
        public void SetUp()
        {
            _testRoot = new GameObject("TestRoot_SuspectFolder");
            _service = new SuspectFolderService();

            _dummyTex = new Texture2D(32, 32);
            _dummySprite1 = Sprite.Create(_dummyTex, new Rect(0, 0, 32, 32), Vector2.zero);
            _dummySprite2 = Sprite.Create(_dummyTex, new Rect(0, 0, 32, 32), Vector2.zero);
            _dummySprite3 = Sprite.Create(_dummyTex, new Rect(0, 0, 32, 32), Vector2.zero);

            // Create Canvas and UIManager
            GameObject canvasGO = new GameObject("Canvas_MainUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(UIManager));
            canvasGO.transform.SetParent(_testRoot.transform);
            _uiManager = canvasGO.GetComponent<UIManager>();

            // Main table panel
            GameObject tableGO = new GameObject("Panel_MainTable");
            tableGO.transform.SetParent(canvasGO.transform);
            _uiManager.mainTablePanel = tableGO;

            // Suspect folder panel
            GameObject folderPanelGO = new GameObject("Panel_SuspectFolder", typeof(RectTransform), typeof(SuspectFolderUI));
            folderPanelGO.transform.SetParent(canvasGO.transform);
            _suspectFolderUI = folderPanelGO.GetComponent<SuspectFolderUI>();
            _uiManager.suspectFolderPanel = folderPanelGO;

            GameObject folderRootGO = new GameObject("Folder_Root", typeof(RectTransform));
            folderRootGO.transform.SetParent(folderPanelGO.transform);
            _suspectFolderUI.folderRoot = folderRootGO.GetComponent<RectTransform>();

            GameObject displayImgGO = new GameObject("Image_FolderDisplay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            displayImgGO.transform.SetParent(folderRootGO.transform);
            _suspectFolderUI.folderDisplayImage = displayImgGO.GetComponent<Image>();

            GameObject closeBtnGO = new GameObject("Button_CloseFolder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            closeBtnGO.transform.SetParent(folderRootGO.transform);
            _suspectFolderUI.closeFolderButton = closeBtnGO.GetComponent<Button>();

            GameObject backdropBtnGO = new GameObject("Button_Backdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            backdropBtnGO.transform.SetParent(folderPanelGO.transform);
            _suspectFolderUI.backdropButton = backdropBtnGO.GetComponent<Button>();

            _suspectFolderUI.case1FolderSprite = _dummySprite1;
            _suspectFolderUI.case2FolderSprite = _dummySprite2;
            _suspectFolderUI.case3FolderSprite = _dummySprite3;

            // Suspect folder button
            GameObject buttonGO = new GameObject("ButtonFOLDER_0", typeof(RectTransform), typeof(SpriteRenderer), typeof(SuspectFolderButton));
            buttonGO.transform.SetParent(canvasGO.transform);
            _suspectFolderBtn = buttonGO.GetComponent<SuspectFolderButton>();
            _uiManager.suspectFolderButton = buttonGO;

            RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(1920f, 1080f);

            RectTransform panelRect = folderPanelGO.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(1920f, 1080f);

            _suspectFolderUI.folderRoot.sizeDelta = new Vector2(1400f, 780f);
            _suspectFolderUI.smoothZoom = false;

            _uiManager.ShowPanel(UIPanelType.InvestigationTable);
            typeof(UIManager).GetProperty("Instance")?.SetValue(null, _uiManager);
        }

        [TearDown]
        public void TearDown()
        {
            typeof(UIManager).GetProperty("Instance")?.SetValue(null, null);

            if (_testRoot != null)
            {
                Object.DestroyImmediate(_testRoot);
            }
            if (_dummyTex != null)
            {
                Object.DestroyImmediate(_dummyTex);
            }
        }

        [Test]
        public void SuspectFolderService_DeterminesCaseIndex_ByLevelNumber()
        {
            CaseSO case1 = ScriptableObject.CreateInstance<CaseSO>();
            case1.levelNumber = 1;
            Assert.AreEqual(1, _service.DetermineCaseIndex(case1));

            CaseSO case2 = ScriptableObject.CreateInstance<CaseSO>();
            case2.levelNumber = 2;
            Assert.AreEqual(2, _service.DetermineCaseIndex(case2));

            CaseSO case3 = ScriptableObject.CreateInstance<CaseSO>();
            case3.levelNumber = 3;
            Assert.AreEqual(3, _service.DetermineCaseIndex(case3));
        }

        [Test]
        public void SuspectFolderService_DeterminesCaseIndex_ByCaseIdAndTitle()
        {
            CaseSO caseWithId = ScriptableObject.CreateInstance<CaseSO>();
            caseWithId.levelNumber = 0;
            caseWithId.caseId = "CASE_02_FRAUD";
            Assert.AreEqual(2, _service.DetermineCaseIndex(caseWithId));

            CaseSO caseWithTitle = ScriptableObject.CreateInstance<CaseSO>();
            caseWithTitle.levelNumber = 0;
            caseWithTitle.caseTitle = "The Last Call - Shania";
            Assert.AreEqual(3, _service.DetermineCaseIndex(caseWithTitle));
        }

        [Test]
        public void SuspectFolderService_DeterminesCaseIndex_BySceneName()
        {
            Assert.AreEqual(1, _service.DetermineCaseIndex(null, "Case001"));
            Assert.AreEqual(2, _service.DetermineCaseIndex(null, "Case002"));
            Assert.AreEqual(3, _service.DetermineCaseIndex(null, "Case003"));
        }

        [Test]
        public void SuspectFolderService_ReturnsExpectedFilenames()
        {
            Assert.AreEqual("Case1. Vince & Jane.png", _service.GetExpectedFilename(1));
            Assert.AreEqual("Case2. Paul & Vonn.png", _service.GetExpectedFilename(2));
            Assert.AreEqual("Case3. Shania and Shan.png", _service.GetExpectedFilename(3));
        }

        [Test]
        public void UIManager_ToggleSuspectFolderPanel_TogglesBetweenTableAndFolder()
        {
            _uiManager.ShowPanel(UIPanelType.InvestigationTable);
            Assert.AreEqual(UIPanelType.InvestigationTable, _uiManager.currentPanel);
            Assert.IsFalse(_uiManager.suspectFolderPanel.activeSelf);

            // Toggle Open
            _uiManager.ToggleSuspectFolderPanel();
            Assert.AreEqual(UIPanelType.SuspectFolder, _uiManager.currentPanel);
            Assert.IsTrue(_uiManager.suspectFolderPanel.activeSelf);
            Assert.IsFalse(_uiManager.mainTablePanel.activeSelf);

            // Toggle Close
            _uiManager.ToggleSuspectFolderPanel();
            Assert.AreEqual(UIPanelType.InvestigationTable, _uiManager.currentPanel);
            Assert.IsFalse(_uiManager.suspectFolderPanel.activeSelf);
            Assert.IsTrue(_uiManager.mainTablePanel.activeSelf);
        }

        [Test]
        public void SuspectFolderUI_UpdatesSpriteForCase()
        {
            _suspectFolderUI.SetCaseFolder(1);
            Assert.AreEqual(1, _suspectFolderUI.CurrentCaseIndex);
            Assert.AreEqual(_dummySprite1, _suspectFolderUI.folderDisplayImage.sprite);

            _suspectFolderUI.SetCaseFolder(2);
            Assert.AreEqual(2, _suspectFolderUI.CurrentCaseIndex);
            Assert.AreEqual(_dummySprite2, _suspectFolderUI.folderDisplayImage.sprite);

            _suspectFolderUI.SetCaseFolder(3);
            Assert.AreEqual(3, _suspectFolderUI.CurrentCaseIndex);
            Assert.AreEqual(_dummySprite3, _suspectFolderUI.folderDisplayImage.sprite);
        }

        [Test]
        public void SuspectFolderButton_OnClick_TogglesPanel()
        {
            _uiManager.ShowPanel(UIPanelType.InvestigationTable);
            Assert.AreEqual(UIPanelType.InvestigationTable, _uiManager.currentPanel);

            _suspectFolderBtn.OnClick();
            Assert.AreEqual(UIPanelType.SuspectFolder, _uiManager.currentPanel);

            _suspectFolderBtn.OnClick();
            Assert.AreEqual(UIPanelType.InvestigationTable, _uiManager.currentPanel);
        }

        [Test]
        public void ArmPointerController_DetermineUIMode_RecognizesSuspectFolder()
        {
            GameObject armGO = new GameObject("ArmPointer", typeof(ArmPointerController));
            armGO.transform.SetParent(_testRoot.transform);
            ArmPointerController arm = armGO.GetComponent<ArmPointerController>();

            _uiManager.ShowPanel(UIPanelType.InvestigationTable);
            Assert.IsFalse(arm.DetermineUIMode());

            _uiManager.ShowPanel(UIPanelType.SuspectFolder);
            Assert.IsTrue(arm.DetermineUIMode());

            _uiManager.ShowPanel(UIPanelType.InvestigationTable);
            Assert.IsFalse(arm.DetermineUIMode());
        }

        [Test]
        public void SuspectFolderService_CalculateNewZoom_ClampsToMinAndMax()
        {
            float zoom1 = _service.CalculateNewZoom(1.0f, -2.0f, 1.0f, 3.5f, 0.15f);
            Assert.AreEqual(1.0f, zoom1, 0.001f, "Zoom below min should clamp to minZoom.");

            float zoom2 = _service.CalculateNewZoom(3.0f, 10.0f, 1.0f, 3.5f, 0.15f);
            Assert.AreEqual(3.5f, zoom2, 0.001f, "Zoom above max should clamp to maxZoom.");

            float zoom3 = _service.CalculateNewZoom(1.5f, 2.0f, 1.0f, 3.5f, 0.15f);
            Assert.AreEqual(1.8f, zoom3, 0.001f, "Zoom should increase by scrollDelta * sensitivity.");
        }

        [Test]
        public void SuspectFolderService_CalculateZoomFocalPosition_ScalesAroundFocalPoint()
        {
            Vector2 currentPos = Vector2.zero;
            Vector2 focalPoint = new Vector2(100f, 50f);
            float oldZoom = 1.0f;
            float newZoom = 2.0f;

            Vector2 newPos = _service.CalculateZoomFocalPosition(currentPos, focalPoint, oldZoom, newZoom);
            // newPos = (0 - (100, 50)) * 2 + (100, 50) = (-200, -100) + (100, 50) = (-100, -50)
            Assert.AreEqual(new Vector2(-100f, -50f), newPos);
        }

        [Test]
        public void SuspectFolderService_ClampFolderPosition_ClampsWithinSafetyMargin()
        {
            Vector2 folderSize = new Vector2(1400f, 780f);
            Vector2 parentSize = new Vector2(1920f, 1080f);
            float zoom = 1.0f;
            float safetyMargin = 100f;

            // Half parent: 960, half folder: 700. Max = 960 + 700 - 100 = 1560
            Vector2 clamped1 = _service.ClampFolderPosition(new Vector2(2000f, 0f), folderSize, zoom, parentSize, safetyMargin);
            Assert.AreEqual(1560f, clamped1.x, 0.01f);

            Vector2 clamped2 = _service.ClampFolderPosition(new Vector2(-2000f, 0f), folderSize, zoom, parentSize, safetyMargin);
            Assert.AreEqual(-1560f, clamped2.x, 0.01f);

            // Within range
            Vector2 inBounds = new Vector2(100f, 50f);
            Vector2 clamped3 = _service.ClampFolderPosition(inBounds, folderSize, zoom, parentSize, safetyMargin);
            Assert.AreEqual(inBounds, clamped3);
        }

        [Test]
        public void SuspectFolderUI_SetTargetZoom_ClampsAndAppliesScale()
        {
            _suspectFolderUI.minZoom = 1.0f;
            _suspectFolderUI.maxZoom = 3.5f;

            _suspectFolderUI.SetTargetZoom(2.5f);
            Assert.AreEqual(2.5f, _suspectFolderUI.TargetZoom, 0.001f);
            Assert.AreEqual(2.5f, _suspectFolderUI.CurrentZoom, 0.001f);
            Assert.AreEqual(2.5f, _suspectFolderUI.folderRoot.localScale.x, 0.001f);

            _suspectFolderUI.SetTargetZoom(5.0f);
            Assert.AreEqual(3.5f, _suspectFolderUI.TargetZoom, 0.001f);
            Assert.AreEqual(3.5f, _suspectFolderUI.folderRoot.localScale.x, 0.001f);

            _suspectFolderUI.SetTargetZoom(0.2f);
            Assert.AreEqual(1.0f, _suspectFolderUI.TargetZoom, 0.001f);
            Assert.AreEqual(1.0f, _suspectFolderUI.folderRoot.localScale.x, 0.001f);
        }

        [Test]
        public void SuspectFolderUI_SetTargetPosition_ClampsAndAppliesPosition()
        {
            _suspectFolderUI.SetTargetPosition(new Vector2(50f, -30f));
            Assert.AreEqual(new Vector2(50f, -30f), _suspectFolderUI.TargetPosition);
            Assert.AreEqual(new Vector2(50f, -30f), _suspectFolderUI.folderRoot.anchoredPosition);
        }

        [Test]
        public void SuspectFolderUI_ResetView_RestoresDefaultZoomAndCenter()
        {
            _suspectFolderUI.SetTargetZoom(3.0f);
            _suspectFolderUI.SetTargetPosition(new Vector2(200f, 150f));

            _suspectFolderUI.ResetView();

            Assert.AreEqual(1.0f, _suspectFolderUI.TargetZoom, 0.001f);
            Assert.AreEqual(1.0f, _suspectFolderUI.CurrentZoom, 0.001f);
            Assert.AreEqual(Vector2.zero, _suspectFolderUI.TargetPosition);
            Assert.AreEqual(Vector2.zero, _suspectFolderUI.folderRoot.anchoredPosition);
            Assert.AreEqual(Vector3.one, _suspectFolderUI.folderRoot.localScale);
        }

        [Test]
        public void SuspectFolderUI_OnPointerClick_RightClickResetsView()
        {
            _suspectFolderUI.SetTargetZoom(2.5f);
            _suspectFolderUI.SetTargetPosition(new Vector2(100f, 80f));

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Right
            };

            _suspectFolderUI.OnPointerClick(eventData);

            Assert.AreEqual(1.0f, _suspectFolderUI.TargetZoom, 0.001f);
            Assert.AreEqual(Vector2.zero, _suspectFolderUI.TargetPosition);
            Assert.AreEqual(Vector3.one, _suspectFolderUI.folderRoot.localScale);
        }

        [Test]
        public void SuspectFolderUI_OnScroll_AdjustsZoom()
        {
            _suspectFolderUI.ResetView();
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                scrollDelta = new Vector2(0f, 2.0f),
                position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)
            };

            _suspectFolderUI.OnScroll(eventData);

            Assert.Greater(_suspectFolderUI.TargetZoom, 1.0f);
        }

        [Test]
        public void SuspectFolderUI_EnsureRaycastSetup_EnablesRaycastTargetOnImage()
        {
            _suspectFolderUI.folderDisplayImage.raycastTarget = false;
            _suspectFolderUI.EnsureRaycastSetup();
            Assert.IsTrue(_suspectFolderUI.folderDisplayImage.raycastTarget);
            Assert.IsNotNull(_suspectFolderUI.folderDisplayImage.GetComponent<SuspectFolderDragTarget>());
            Assert.IsNotNull(_suspectFolderUI.folderRoot.GetComponent<SuspectFolderDragTarget>());
        }

        [Test]
        public void SuspectFolderUI_LeftClickOnFolder_DoesNotClose()
        {
            _suspectFolderUI.EnsureRaycastSetup();

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                pointerCurrentRaycast = new RaycastResult
                {
                    gameObject = _suspectFolderUI.folderDisplayImage.gameObject
                }
            };

            _suspectFolderUI.OnPointerClick(eventData);
            Assert.IsFalse(_suspectFolderUI.IsClosing, "Left-clicking on the suspect folder should do nothing and not close.");

            var dragTarget = _suspectFolderUI.folderDisplayImage.GetComponent<SuspectFolderDragTarget>();
            dragTarget.OnPointerClick(eventData);
            Assert.IsFalse(_suspectFolderUI.IsClosing, "SuspectFolderDragTarget should ignore left clicks.");
        }

        [Test]
        public void SuspectFolderDragTarget_RightClick_ResetsView()
        {
            _suspectFolderUI.EnsureRaycastSetup();
            _suspectFolderUI.SetTargetZoom(2.8f);

            var dragTarget = _suspectFolderUI.folderDisplayImage.GetComponent<SuspectFolderDragTarget>();
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Right
            };

            dragTarget.OnPointerClick(eventData);
            Assert.AreEqual(1.0f, _suspectFolderUI.TargetZoom, 0.001f);
        }
    }
}
