using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
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
    }
}
