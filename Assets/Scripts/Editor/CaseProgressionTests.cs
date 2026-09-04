using NUnit.Framework;
using CaseClosed.Services;

namespace CaseClosed.Tests
{
    [TestFixture]
    public class CaseProgressionTests
    {
        private InMemoryProgressionStorage _storage;
        private CaseProgressionService _service;

        [SetUp]
        public void SetUp()
        {
            _storage = new InMemoryProgressionStorage();
            _service = new CaseProgressionService(_storage);
        }

        [Test]
        public void Case01_IsAlwaysUnlocked_ByDefault()
        {
            Assert.IsTrue(_service.IsCaseUnlocked(1), "Case 01 must always be unlocked by default.");
            Assert.IsFalse(_service.IsCaseCompleted(1), "Case 01 must not be marked complete initially.");
        }

        [Test]
        public void Case02_IsLocked_WhenCase01NotCompleted()
        {
            Assert.IsFalse(_service.IsCaseUnlocked(2), "Case 02 must remain locked when Case 01 is not completed.");
            Assert.IsFalse(_service.IsCaseUnlocked(3), "Case 03 must remain locked when Case 01 is not completed.");
        }

        [Test]
        public void Case02_Unlocks_WhenCase01Completed()
        {
            _service.SetCaseCompleted(1, true);

            Assert.IsTrue(_service.IsCaseCompleted(1), "Case 01 should be marked completed.");
            Assert.IsTrue(_service.IsCaseUnlocked(2), "Case 02 should unlock once Case 01 is completed.");
            Assert.IsFalse(_service.IsCaseUnlocked(3), "Case 03 should still be locked when Case 02 is incomplete.");
        }

        [Test]
        public void Case03_Unlocks_WhenCase01AndCase02Completed()
        {
            _service.SetCaseCompleted(1, true);
            _service.SetCaseCompleted(2, true);

            Assert.IsTrue(_service.IsCaseCompleted(1));
            Assert.IsTrue(_service.IsCaseCompleted(2));
            Assert.IsTrue(_service.IsCaseUnlocked(3), "Case 03 should unlock once Case 02 is completed.");
        }

        [Test]
        public void HighestUnlockedLevel_ReflectsProgression()
        {
            Assert.AreEqual(1, _service.GetHighestUnlockedLevel());

            _service.SetCaseCompleted(1, true);
            Assert.AreEqual(2, _service.GetHighestUnlockedLevel());

            _service.SetCaseCompleted(2, true);
            Assert.AreEqual(3, _service.GetHighestUnlockedLevel());
        }

        [Test]
        public void ResetProgression_ClearsAllCompletedCases()
        {
            _service.SetCaseCompleted(1, true);
            _service.SetCaseCompleted(2, true);
            Assert.IsTrue(_service.IsCaseUnlocked(3));

            _service.ResetProgression();

            Assert.IsFalse(_service.IsCaseCompleted(1));
            Assert.IsFalse(_service.IsCaseCompleted(2));
            Assert.IsTrue(_service.IsCaseUnlocked(1), "Case 01 must still be unlocked after reset.");
            Assert.IsFalse(_service.IsCaseUnlocked(2), "Case 02 must be locked after reset.");
            Assert.IsFalse(_service.IsCaseUnlocked(3), "Case 03 must be locked after reset.");
            Assert.AreEqual(1, _service.GetHighestUnlockedLevel());
        }

        [Test]
        public void ProgressionChangedEvent_FiresOnStateChanges()
        {
            int eventCount = 0;
            _service.OnProgressionChanged += () => eventCount++;

            _service.SetCaseCompleted(1, true);
            Assert.AreEqual(1, eventCount);

            _service.ResetProgression();
            Assert.AreEqual(2, eventCount);
        }
    }
}
