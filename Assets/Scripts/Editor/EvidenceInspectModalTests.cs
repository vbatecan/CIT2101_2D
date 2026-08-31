using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CaseClosed.Data;
using CaseClosed.UI;

namespace CaseClosed.Tests
{
    [TestFixture]
    public class EvidenceInspectModalTests
    {
        private GameObject modalObj;
        private EvidenceInspectModal inspectModal;
        private Image zoomImage;
        private RectTransform viewportRect;

        [SetUp]
        public void SetUp()
        {
            modalObj = new GameObject("Test_InspectModal");
            inspectModal = modalObj.AddComponent<EvidenceInspectModal>();

            // Setup viewport
            GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform));
            viewportObj.transform.SetParent(modalObj.transform);
            viewportRect = viewportObj.GetComponent<RectTransform>();
            viewportRect.sizeDelta = new Vector2(400f, 400f);
            inspectModal.viewportRectTransform = viewportRect;

            // Setup Zoom Image
            GameObject imageObj = new GameObject("Image_Zoomed", typeof(RectTransform), typeof(Image));
            imageObj.transform.SetParent(viewportObj.transform);
            zoomImage = imageObj.GetComponent<Image>();
            zoomImage.rectTransform.sizeDelta = new Vector2(400f, 400f);
            inspectModal.evidenceZoomImage = zoomImage;

            inspectModal.minZoom = 1.0f;
            inspectModal.maxZoom = 3.5f;
            inspectModal.zoomStep = 0.25f;
            inspectModal.smoothZoom = false; // Instant updates for test assertions
            inspectModal.smoothRotation = false; // Instant rotation for test assertions
        }

        [TearDown]
        public void TearDown()
        {
            if (modalObj != null)
            {
                Object.DestroyImmediate(modalObj);
            }
        }

        [Test]
        public void RotateWithCursor_RotatesEvidenceAndTransform()
        {
            inspectModal.ResetView();
            Assert.AreEqual(0f, inspectModal.TargetRotationAngle, 0.001f);
            Assert.AreEqual(Quaternion.identity, zoomImage.rectTransform.localRotation);

            inspectModal.RotateWithCursor(45f);
            Assert.AreEqual(45f, inspectModal.TargetRotationAngle, 0.001f);
            Assert.AreEqual(45f, zoomImage.rectTransform.localEulerAngles.z, 0.01f);

            inspectModal.RotateWithCursor(-90f);
            Assert.AreEqual(-45f, inspectModal.TargetRotationAngle, 0.001f);
        }

        [Test]
        public void Zoom_ClampsToMinAndMax()
        {
            inspectModal.SetTargetZoom(0.5f);
            Assert.AreEqual(1.0f, inspectModal.TargetZoom, 0.001f, "Zoom below minZoom should clamp to 1.0f");

            inspectModal.SetTargetZoom(10.0f);
            Assert.AreEqual(3.5f, inspectModal.TargetZoom, 0.001f, "Zoom above maxZoom should clamp to 3.5f");
        }

        [Test]
        public void ResetZoom_ResetsZoomAndPanToDefaults()
        {
            inspectModal.SetTargetZoom(2.5f);
            inspectModal.ResetZoom();

            Assert.AreEqual(1.0f, inspectModal.TargetZoom, 0.001f);
            Assert.AreEqual(Vector2.zero, inspectModal.TargetPanPosition);
        }

        [Test]
        public void ResetView_ResetsZoomPanAndRotation()
        {
            inspectModal.SetTargetZoom(2.0f);
            inspectModal.RotateWithCursor(90f);
            Assert.AreNotEqual(Quaternion.identity, zoomImage.rectTransform.localRotation);

            inspectModal.ResetView();

            Assert.AreEqual(1.0f, inspectModal.TargetZoom, 0.001f);
            Assert.AreEqual(Vector2.zero, inspectModal.TargetPanPosition);
            Assert.AreEqual(0f, inspectModal.TargetRotationAngle, 0.001f);
            Assert.AreEqual(Quaternion.identity, zoomImage.rectTransform.localRotation);
        }

        [Test]
        public void ClampPan_WhenZoomIsMin_LocksPanToZero()
        {
            inspectModal.SetTargetZoom(1.0f);
            inspectModal.ClampPan();

            Assert.AreEqual(Vector2.zero, inspectModal.TargetPanPosition);
        }

        [Test]
        public void ClampPan_WhenZoomIsMagnified_BoundsPanWithinAllowableRange()
        {
            // Viewport: 400x400, Image: 400x400 at 2.0x zoom => Scaled size: 800x800.
            // Max allowable pan offset = (800 - 400) / 2 = 200 on X and Y.
            inspectModal.SetTargetZoom(2.0f);

            inspectModal.GetType().GetField("targetPanPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(inspectModal, new Vector2(150f, -150f));
            inspectModal.ClampPan();
            Assert.AreEqual(new Vector2(150f, -150f), inspectModal.TargetPanPosition);

            inspectModal.GetType().GetField("targetPanPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(inspectModal, new Vector2(500f, -500f));
            inspectModal.ClampPan();
            Assert.AreEqual(200f, inspectModal.TargetPanPosition.x, 0.01f);
            Assert.AreEqual(-200f, inspectModal.TargetPanPosition.y, 0.01f);
        }

        [Test]
        public void DisplayEvidence_ResetsStateAndIsolatesScene()
        {
            // Create dummy scene objects to test isolation
            GameObject envObj = new GameObject("Environments");
            GameObject charsObj = new GameObject("Characters");
            GameObject itemsObj = new GameObject("Items");

            inspectModal.sceneObjectsToHide = new string[] { "Environments", "Characters", "Items" };

            EvidenceSO testEvidence = ScriptableObject.CreateInstance<EvidenceSO>();
            testEvidence.id = "EVD_TEST";
            testEvidence.evidenceName = "Antique Pocket Watch";
            testEvidence.baseDescription = "A vintage pocket watch with a cracked glass face.";

            inspectModal.SetTargetZoom(2.5f);
            inspectModal.RotateWithCursor(90f);

            inspectModal.DisplayEvidence(testEvidence);

            Assert.IsTrue(inspectModal.IsInspecting);
            Assert.AreEqual(1.0f, inspectModal.TargetZoom, 0.001f);
            Assert.AreEqual(Quaternion.identity, zoomImage.rectTransform.localRotation);

            // Verify scene objects were hidden
            Assert.IsFalse(envObj.activeSelf);
            Assert.IsFalse(charsObj.activeSelf);
            Assert.IsFalse(itemsObj.activeSelf);
            Assert.AreEqual(3, inspectModal.HiddenSceneObjects.Count);

            // Close inspection and verify scene objects were restored
            inspectModal.CloseInspect();

            Assert.IsFalse(inspectModal.IsInspecting);
            Assert.IsTrue(envObj.activeSelf);
            Assert.IsTrue(charsObj.activeSelf);
            Assert.IsTrue(itemsObj.activeSelf);

            Object.DestroyImmediate(envObj);
            Object.DestroyImmediate(charsObj);
            Object.DestroyImmediate(itemsObj);
            Object.DestroyImmediate(testEvidence);
        }
    }
}
