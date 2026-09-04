using NUnit.Framework;
using CaseClosed.Services;

namespace CaseClosed.Tests
{
    [TestFixture]
    public class GameSettingsTests
    {
        private InMemorySettingsStorage _storage;
        private GameSettingsService _service;

        [SetUp]
        public void SetUp()
        {
            _storage = new InMemorySettingsStorage();
            _service = new GameSettingsService(_storage);
        }

        [Test]
        public void Defaults_MatchSpecification()
        {
            Assert.AreEqual(0.8f, _service.BgmVolume, 0.001f);
            Assert.IsFalse(_service.IsBgmMuted);

            Assert.AreEqual(1.0f, _service.SfxVolume, 0.001f);
            Assert.IsFalse(_service.IsSfxMuted);

            Assert.AreEqual(1.0f, _service.DialogVolume, 0.001f);
            Assert.IsFalse(_service.IsDialogMuted);

            Assert.IsTrue(_service.IsFullscreen);
            Assert.AreEqual(35f, _service.TextSpeed, 0.001f);
            Assert.IsTrue(_service.IsTypewriterEnabled);
        }

        [Test]
        public void VolumeSetter_ClampsBetweenZeroAndOne()
        {
            _service.SetBgmVolume(1.5f);
            Assert.AreEqual(1.0f, _service.BgmVolume, 0.001f);

            _service.SetBgmVolume(-0.2f);
            Assert.AreEqual(0.0f, _service.BgmVolume, 0.001f);

            _service.SetSfxVolume(2.0f);
            Assert.AreEqual(1.0f, _service.SfxVolume, 0.001f);

            _service.SetDialogVolume(0.65f);
            Assert.AreEqual(0.65f, _service.DialogVolume, 0.001f);
        }

        [Test]
        public void MuteToggle_PreservesVolumeLevel_WhenUnmuted()
        {
            _service.SetBgmVolume(0.45f);
            _service.ToggleBgmMute();

            Assert.IsTrue(_service.IsBgmMuted);
            Assert.AreEqual(0.45f, _service.BgmVolume, 0.001f);
            Assert.AreEqual(0.0f, _service.GetEffectiveBgmVolume(), 0.001f);

            _service.ToggleBgmMute();
            Assert.IsFalse(_service.IsBgmMuted);
            Assert.AreEqual(0.45f, _service.GetEffectiveBgmVolume(), 0.001f);
        }

        [Test]
        public void TextSpeed_ClampsToValidRange()
        {
            _service.SetTextSpeed(5f);
            Assert.AreEqual(15f, _service.TextSpeed, 0.001f);

            _service.SetTextSpeed(150f);
            Assert.AreEqual(100f, _service.TextSpeed, 0.001f);

            _service.SetTextSpeed(50f);
            Assert.AreEqual(50f, _service.TextSpeed, 0.001f);
        }

        [Test]
        public void ResetDefaults_RestoresInitialState()
        {
            _service.SetBgmVolume(0.2f);
            _service.SetBgmMuted(true);
            _service.SetSfxVolume(0.1f);
            _service.SetDialogVolume(0.3f);
            _service.SetFullscreen(false);
            _service.SetTextSpeed(80f);
            _service.SetTypewriterEnabled(false);

            _service.ResetToDefaults();

            Assert.AreEqual(0.8f, _service.BgmVolume, 0.001f);
            Assert.IsFalse(_service.IsBgmMuted);
            Assert.AreEqual(1.0f, _service.SfxVolume, 0.001f);
            Assert.AreEqual(1.0f, _service.DialogVolume, 0.001f);
            Assert.IsTrue(_service.IsFullscreen);
            Assert.AreEqual(35f, _service.TextSpeed, 0.001f);
            Assert.IsTrue(_service.IsTypewriterEnabled);
        }

        [Test]
        public void TextSpeedLabel_ReturnsDescriptiveString()
        {
            Assert.AreEqual("Slow (20 cps)", GameSettingsService.GetTextSpeedLabel(20f));
            Assert.AreEqual("Normal (35 cps)", GameSettingsService.GetTextSpeedLabel(35f));
            Assert.AreEqual("Fast (70 cps)", GameSettingsService.GetTextSpeedLabel(70f));
        }
    }
}
