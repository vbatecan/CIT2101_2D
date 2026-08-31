using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Managers;
using CaseClosed.UI;

namespace CaseClosed.Tests
{
    [TestFixture]
    public class CursorManagerTests
    {
        private GameObject cursorManagerObj;
        private CursorManager cursorManager;

        [SetUp]
        public void SetUp()
        {
            cursorManagerObj = new GameObject("Test_CursorManager");
            cursorManager = cursorManagerObj.AddComponent<CursorManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (cursorManagerObj != null)
            {
                Object.DestroyImmediate(cursorManagerObj);
            }
        }

        [Test]
        public void CursorManager_InitializesSingleton()
        {
            Assert.IsNotNull(CursorManager.Instance);
            Assert.AreEqual(cursorManager, CursorManager.Instance);
        }

        [Test]
        public void CursorManager_LoadsDefaultCursorResource()
        {
            cursorManager.LoadDefaultCursorResources();
            Assert.IsNotNull(cursorManager.armCursorTexture, "Arm cursor texture should be successfully loaded from Resources.");
        }

        [Test]
        public void CursorManager_FingertipHotspot_IsValid()
        {
            Assert.GreaterOrEqual(cursorManager.armHotspot.x, 0f);
            Assert.GreaterOrEqual(cursorManager.armHotspot.y, 0f);
            // Fingertip is at the top row (y = 0)
            Assert.AreEqual(0f, cursorManager.armHotspot.y, 0.001f);
        }

        [Test]
        public void CursorManager_SetArmCursor_UpdatesActiveState()
        {
            bool eventFired = false;
            CursorManager.CursorType notifiedType = CursorManager.CursorType.DefaultSystem;

            cursorManager.OnCursorChanged += (type) =>
            {
                eventFired = true;
                notifiedType = type;
            };

            cursorManager.SetArmCursor();

            Assert.AreEqual(CursorManager.CursorType.ArmPointer, cursorManager.ActiveCursorType);
            Assert.IsTrue(eventFired);
            Assert.AreEqual(CursorManager.CursorType.ArmPointer, notifiedType);
            Assert.IsTrue(Cursor.visible);
        }

        [Test]
        public void CursorManager_SetDefaultCursor_ResetsState()
        {
            cursorManager.SetArmCursor();
            Assert.AreEqual(CursorManager.CursorType.ArmPointer, cursorManager.ActiveCursorType);

            bool eventFired = false;
            cursorManager.OnCursorChanged += (type) =>
            {
                eventFired = true;
            };

            cursorManager.SetDefaultCursor();

            Assert.AreEqual(CursorManager.CursorType.DefaultSystem, cursorManager.ActiveCursorType);
            Assert.IsTrue(eventFired);
            Assert.IsTrue(Cursor.visible);
        }

        [Test]
        public void CursorManager_AttachesPhysics2DRaycaster_ToCamera()
        {
            GameObject camObj = new GameObject("Test_MainCamera", typeof(Camera));
            camObj.tag = "MainCamera";

            Assert.IsNull(camObj.GetComponent<Physics2DRaycaster>());

            cursorManager.EnsurePhysics2DRaycasterOnCamera();

            Assert.IsNotNull(camObj.GetComponent<Physics2DRaycaster>(), "Physics2DRaycaster should be attached to MainCamera for 2D evidence clicks.");

            Object.DestroyImmediate(camObj);
        }
    }
}
