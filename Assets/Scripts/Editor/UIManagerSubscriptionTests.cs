using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using CaseClosed.Managers;
using CaseClosed.UI;

namespace CaseClosed.Tests
{
    public class UIManagerSubscriptionTests
    {
        private GameObject _testRoot;
        private CaseManager _caseManager;
        private EvidenceManager _evidenceManager;
        private UIManager _uiManager;

        [SetUp]
        public void SetUp()
        {
            _testRoot = new GameObject("Test_UIManagerSubscriptions");
            _caseManager = _testRoot.AddComponent<CaseManager>();
            _evidenceManager = _testRoot.AddComponent<EvidenceManager>();
            _uiManager = _testRoot.AddComponent<UIManager>();

            typeof(CaseManager).GetProperty("Instance")?.SetValue(null, _caseManager);
            typeof(EvidenceManager).GetProperty("Instance")?.SetValue(null, _evidenceManager);
            typeof(UIManager).GetProperty("Instance")?.SetValue(null, _uiManager);
        }

        [TearDown]
        public void TearDown()
        {
            typeof(CaseManager).GetProperty("Instance")?.SetValue(null, null);
            typeof(EvidenceManager).GetProperty("Instance")?.SetValue(null, null);
            typeof(UIManager).GetProperty("Instance")?.SetValue(null, null);

            if (_testRoot != null) UnityEngine.Object.DestroyImmediate(_testRoot);
        }

        [Test]
        public void EnableDisableAndRepeatedRegistration_KeepExactlyOneHandlerPerEvent()
        {
            InvokePrivate(_uiManager, "RegisterEvents");
            InvokePrivate(_uiManager, "RegisterEvents");
            InvokePrivate(_uiManager, "RegisterEvents");

            Assert.AreEqual(1, CountHandlers(_evidenceManager, "OnInspectModalOpened", _uiManager));
            Assert.AreEqual(1, CountHandlers(_evidenceManager, "OnInspectModalClosed", _uiManager));
            Assert.AreEqual(1, CountHandlers(_caseManager, "OnTimeExpired", _uiManager));

            _uiManager.enabled = false;
            InvokePrivate(_uiManager, "OnDisable");

            Assert.AreEqual(0, CountHandlers(_evidenceManager, "OnInspectModalOpened", _uiManager));
            Assert.AreEqual(0, CountHandlers(_evidenceManager, "OnInspectModalClosed", _uiManager));
            Assert.AreEqual(0, CountHandlers(_caseManager, "OnTimeExpired", _uiManager));

            _uiManager.enabled = true;
            InvokePrivate(_uiManager, "OnEnable");

            Assert.AreEqual(1, CountHandlers(_evidenceManager, "OnInspectModalOpened", _uiManager));
            Assert.AreEqual(1, CountHandlers(_evidenceManager, "OnInspectModalClosed", _uiManager));
            Assert.AreEqual(1, CountHandlers(_caseManager, "OnTimeExpired", _uiManager));
        }

        private static void InvokePrivate(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Expected private method '{methodName}'.");
            method.Invoke(target, null);
        }

        private static int CountHandlers(object publisher, string eventFieldName, object target)
        {
            FieldInfo field = publisher.GetType().GetField(eventFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Expected event backing field '{eventFieldName}'.");

            Delegate handlers = field.GetValue(publisher) as Delegate;
            if (handlers == null) return 0;

            int count = 0;
            foreach (Delegate handler in handlers.GetInvocationList())
            {
                if (ReferenceEquals(handler.Target, target)) count++;
            }
            return count;
        }
    }
}
