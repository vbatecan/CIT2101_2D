using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using CaseClosed.Services;
using CaseClosed.UI;

namespace CaseClosed.Tests
{
    [TestFixture]
    public class MainMenuUITests
    {
        private GameObject _rootObj;
        private MainMenuUI _menuUI;
        private InMemoryProgressionStorage _progressionStorage;
        private InMemorySettingsStorage _settingsStorage;

        [SetUp]
        public void SetUp()
        {
            _rootObj = new GameObject("Test_MainMenuUIRoot");

            _menuUI = _rootObj.AddComponent<MainMenuUI>();

            // Containers
            _menuUI.mainButtonsContainer = new GameObject("MainButtons");
            _menuUI.mainButtonsContainer.transform.SetParent(_rootObj.transform);

            _menuUI.caseSelectContainer = new GameObject("CaseSelect");
            _menuUI.caseSelectContainer.transform.SetParent(_rootObj.transform);

            _menuUI.settingsContainer = new GameObject("Settings");
            _menuUI.settingsContainer.transform.SetParent(_rootObj.transform);

            _menuUI.exitConfirmContainer = new GameObject("ExitConfirm");
            _menuUI.exitConfirmContainer.transform.SetParent(_rootObj.transform);

            // Case Select UI elements
            _menuUI.case01Button = CreateButton(_rootObj.transform, "Case01Btn");
            _menuUI.case01StatusText = CreateText(_rootObj.transform, "Case01Status");

            _menuUI.case02Button = CreateButton(_rootObj.transform, "Case02Btn");
            _menuUI.case02StatusText = CreateText(_rootObj.transform, "Case02Status");

            _menuUI.case03Button = CreateButton(_rootObj.transform, "Case03Btn");
            _menuUI.case03StatusText = CreateText(_rootObj.transform, "Case03Status");

            // Settings UI elements
            _menuUI.bgmVolumeSlider = CreateSlider(_rootObj.transform, "BgmSlider");
            _menuUI.bgmPercentText = CreateText(_rootObj.transform, "BgmPercent");
            _menuUI.bgmMuteButton = CreateButton(_rootObj.transform, "BgmMuteBtn");
            _menuUI.bgmMuteText = CreateText(_rootObj.transform, "BgmMuteText");

            _menuUI.sfxVolumeSlider = CreateSlider(_rootObj.transform, "SfxSlider");
            _menuUI.sfxPercentText = CreateText(_rootObj.transform, "SfxPercent");

            _menuUI.dialogVolumeSlider = CreateSlider(_rootObj.transform, "DialogSlider");
            _menuUI.dialogPercentText = CreateText(_rootObj.transform, "DialogPercent");

            _menuUI.textSpeedSlider = CreateSlider(_rootObj.transform, "SpeedSlider", 15f, 100f);
            _menuUI.textSpeedText = CreateText(_rootObj.transform, "SpeedText");

            _menuUI.fullscreenToggle = CreateToggle(_rootObj.transform, "FullscreenToggle");
            _menuUI.fullscreenStatusText = CreateText(_rootObj.transform, "FullscreenStatus");

            _menuUI.confirmExitYesButton = CreateButton(_rootObj.transform, "ExitYesBtn");
            _menuUI.confirmExitNoButton = CreateButton(_rootObj.transform, "ExitNoBtn");

            // Inject pure services
            _progressionStorage = new InMemoryProgressionStorage();
            CaseProgressionService.Instance = new CaseProgressionService(_progressionStorage);

            _settingsStorage = new InMemorySettingsStorage();
            GameSettingsService.Instance = new GameSettingsService(_settingsStorage);
        }

        [TearDown]
        public void TearDown()
        {
            if (_rootObj != null) Object.DestroyImmediate(_rootObj);
        }

        private Button CreateButton(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent);
            return go.GetComponent<Button>();
        }

        private Text CreateText(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent);
            return go.GetComponent<Text>();
        }

        private Slider CreateSlider(Transform parent, string name, float min = 0f, float max = 1f)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent);
            var sld = go.GetComponent<Slider>();
            sld.minValue = min;
            sld.maxValue = max;
            return sld;
        }

        private Toggle CreateToggle(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            go.transform.SetParent(parent);
            return go.GetComponent<Toggle>();
        }

        [Test]
        public void ReturnToMainView_ActivatesMainButtons_AndHidesOthers()
        {
            _menuUI.caseSelectContainer.SetActive(true);
            _menuUI.settingsContainer.SetActive(true);
            _menuUI.exitConfirmContainer.SetActive(true);

            _menuUI.ReturnToMainView();

            Assert.IsTrue(_menuUI.mainButtonsContainer.activeSelf);
            Assert.IsFalse(_menuUI.caseSelectContainer.activeSelf);
            Assert.IsFalse(_menuUI.settingsContainer.activeSelf);
            Assert.IsFalse(_menuUI.exitConfirmContainer.activeSelf);
        }

        [Test]
        public void OpenSubView_SwitchesPanels_Correctly()
        {
            _menuUI.OpenSubView(_menuUI.settingsContainer);

            Assert.IsFalse(_menuUI.mainButtonsContainer.activeSelf);
            Assert.IsTrue(_menuUI.settingsContainer.activeSelf);
            Assert.IsFalse(_menuUI.caseSelectContainer.activeSelf);
        }

        [Test]
        public void ExitConfirmation_OpensAndClosesModal()
        {
            _menuUI.ReturnToMainView();
            Assert.IsFalse(_menuUI.exitConfirmContainer.activeSelf);

            _menuUI.OpenExitConfirmation();
            Assert.IsTrue(_menuUI.exitConfirmContainer.activeSelf);

            _menuUI.CloseExitConfirmation();
            Assert.IsFalse(_menuUI.exitConfirmContainer.activeSelf);
        }

        [Test]
        public void RefreshCaseSelectUI_EnablesUnlockedAndDisablesLockedCases()
        {
            // Initially Case 1 is unlocked, Cases 2 and 3 are locked
            _menuUI.RefreshCaseSelectUI();

            Assert.IsTrue(_menuUI.case01Button.interactable, "Case 01 must be interactable initially.");
            Assert.IsFalse(_menuUI.case02Button.interactable, "Case 02 must be non-interactable initially.");
            Assert.IsFalse(_menuUI.case03Button.interactable, "Case 03 must be non-interactable initially.");

            StringAssert.Contains("AVAILABLE", _menuUI.case01StatusText.text);
            StringAssert.Contains("LOCKED", _menuUI.case02StatusText.text);

            // Complete Case 01
            CaseProgressionService.Instance.SetCaseCompleted(1, true);
            _menuUI.RefreshCaseSelectUI();

            Assert.IsTrue(_menuUI.case01Button.interactable);
            StringAssert.Contains("SOLVED", _menuUI.case01StatusText.text);

            Assert.IsTrue(_menuUI.case02Button.interactable, "Case 02 must become interactable after Case 01 is completed.");
            StringAssert.Contains("AVAILABLE", _menuUI.case02StatusText.text);

            Assert.IsFalse(_menuUI.case03Button.interactable, "Case 03 must still be locked.");
        }

        [Test]
        public void RefreshSettingsUI_PopulatesValuesFromService()
        {
            GameSettingsService.Instance.SetBgmVolume(0.6f);
            GameSettingsService.Instance.SetSfxVolume(0.9f);
            GameSettingsService.Instance.SetDialogVolume(0.75f);
            GameSettingsService.Instance.SetTextSpeed(60f);

            _menuUI.RefreshSettingsUI();

            Assert.AreEqual(0.6f, _menuUI.bgmVolumeSlider.value, 0.01f);
            Assert.AreEqual("60%", _menuUI.bgmPercentText.text);

            Assert.AreEqual(0.9f, _menuUI.sfxVolumeSlider.value, 0.01f);
            Assert.AreEqual("90%", _menuUI.sfxPercentText.text);

            Assert.AreEqual(0.75f, _menuUI.dialogVolumeSlider.value, 0.01f);
            Assert.AreEqual("75%", _menuUI.dialogPercentText.text);

            Assert.AreEqual(60f, _menuUI.textSpeedSlider.value, 0.01f);
            StringAssert.Contains("Fast", _menuUI.textSpeedText.text);
        }

        [Test]
        public void ResetSettingsClicked_RestoresDefaultsInUIAndService()
        {
            GameSettingsService.Instance.SetBgmVolume(0.1f);
            GameSettingsService.Instance.SetTextSpeed(20f);

            _menuUI.OnResetSettingsClicked();

            Assert.AreEqual(0.8f, GameSettingsService.Instance.BgmVolume, 0.01f);
            Assert.AreEqual(35f, GameSettingsService.Instance.TextSpeed, 0.01f);

            Assert.AreEqual(0.8f, _menuUI.bgmVolumeSlider.value, 0.01f);
            Assert.AreEqual("80%", _menuUI.bgmPercentText.text);
        }
    }
}
