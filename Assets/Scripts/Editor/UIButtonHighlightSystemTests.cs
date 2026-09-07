using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using CaseClosed.UI;

namespace CaseClosed.Tests
{
    [TestFixture]
    public class UIButtonHighlightSystemTests
    {
        private GameObject _rootObj;

        [SetUp]
        public void SetUp()
        {
            _rootObj = new GameObject("Test_UIButtonHighlightSystemRoot");
        }

        [TearDown]
        public void TearDown()
        {
            if (_rootObj != null)
            {
                Object.DestroyImmediate(_rootObj);
            }
        }

        [Test]
        public void GetHighlightColorBlock_ReturnsStandardizedHighlightColors()
        {
            ColorBlock cb = UIButtonHighlightSystem.GetHighlightColorBlock();

            Assert.AreEqual(UIButtonHighlightSystem.NormalColor, cb.normalColor, "Normal color should match dimmed resting state.");
            Assert.AreEqual(Color.white, cb.highlightedColor, "Highlighted color must be pure white (#FFFFFF).");
            Assert.AreEqual(UIButtonHighlightSystem.PressedColor, cb.pressedColor, "Pressed color should match pressed state.");
            Assert.AreEqual(Color.white, cb.selectedColor, "Selected color should match white highlight.");
            Assert.AreEqual(UIButtonHighlightSystem.DisabledColor, cb.disabledColor, "Disabled color should match disabled state.");
            Assert.AreEqual(0.10f, cb.fadeDuration, 0.001f, "Fade duration should be 0.1s.");
            Assert.AreEqual(1.0f, cb.colorMultiplier, 0.001f, "Color multiplier should be 1.0.");
        }

        [Test]
        public void ApplyTo_ConfiguresButtonTransitionAndColors()
        {
            GameObject btnObj = new GameObject("TestBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(_rootObj.transform);

            Button btn = btnObj.GetComponent<Button>();
            btn.transition = Selectable.Transition.None;

            UIButtonHighlightSystem.ApplyTo(btn);

            Assert.AreEqual(Selectable.Transition.ColorTint, btn.transition, "Button transition should be set to ColorTint.");
            Assert.AreEqual(Color.white, btn.colors.highlightedColor, "Button highlighted color should be white.");
            Assert.AreEqual(new Color(0.85f, 0.85f, 0.85f, 1.0f), btn.colors.normalColor, "Button normal color should be 0.85 brightness.");
        }

        [Test]
        public void ApplyTo_ResolvesMissingTargetGraphic()
        {
            GameObject btnObj = new GameObject("GraphicResolutionBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(_rootObj.transform);

            Button btn = btnObj.GetComponent<Button>();
            Image img = btnObj.GetComponent<Image>();
            btn.targetGraphic = null;

            UIButtonHighlightSystem.ApplyTo(btn);

            Assert.IsNotNull(btn.targetGraphic, "Target graphic should be automatically resolved.");
            Assert.AreEqual(img, btn.targetGraphic, "Target graphic should point to the Image on the same GameObject.");
        }

        [Test]
        public void ApplyTo_SkipsBackdropButtons()
        {
            GameObject backdropObj = new GameObject("Button_Backdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            backdropObj.transform.SetParent(_rootObj.transform);

            Button btn = backdropObj.GetComponent<Button>();
            btn.transition = Selectable.Transition.None;
            ColorBlock originalColors = btn.colors;

            UIButtonHighlightSystem.ApplyTo(btn);

            Assert.AreEqual(Selectable.Transition.None, btn.transition, "Backdrop button transition should remain untouched.");
            Assert.AreEqual(originalColors.highlightedColor, btn.colors.highlightedColor, "Backdrop button colors should not be modified.");
        }

        [Test]
        public void ApplyToHierarchy_AppliesToAllActiveAndInactiveButtons()
        {
            GameObject activeChild = new GameObject("ActiveChild", typeof(RectTransform));
            activeChild.transform.SetParent(_rootObj.transform);

            GameObject inactiveChild = new GameObject("InactiveChild", typeof(RectTransform));
            inactiveChild.transform.SetParent(_rootObj.transform);
            inactiveChild.SetActive(false);

            GameObject btn1Obj = new GameObject("Btn1", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btn1Obj.transform.SetParent(activeChild.transform);

            GameObject btn2Obj = new GameObject("Btn2", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btn2Obj.transform.SetParent(inactiveChild.transform);

            Button btn1 = btn1Obj.GetComponent<Button>();
            Button btn2 = btn2Obj.GetComponent<Button>();
            btn1.transition = Selectable.Transition.None;
            btn2.transition = Selectable.Transition.None;

            UIButtonHighlightSystem.ApplyToHierarchy(_rootObj, includeInactive: true);

            Assert.AreEqual(Selectable.Transition.ColorTint, btn1.transition, "Active button should have ColorTint transition.");
            Assert.AreEqual(Color.white, btn1.colors.highlightedColor, "Active button should have white highlight.");

            Assert.AreEqual(Selectable.Transition.ColorTint, btn2.transition, "Inactive button should have ColorTint transition.");
            Assert.AreEqual(Color.white, btn2.colors.highlightedColor, "Inactive button should have white highlight.");
        }
    }
}
