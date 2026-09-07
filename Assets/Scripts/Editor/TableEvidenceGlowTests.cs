using NUnit.Framework;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Gameplay;
using CaseClosed.Managers;

namespace CaseClosed.Tests
{
    [TestFixture]
    public class TableEvidenceGlowTests
    {
        private GameObject testItemObj;
        private TableEvidenceItem tableItem;
        private SpriteRenderer spriteRenderer;
        private BoxCollider2D boxCollider;
        private Sprite testSprite;
        private GameObject caseManagerObj;

        [SetUp]
        public void SetUp()
        {
            testItemObj = new GameObject("Test_TableEvidenceItem");
            spriteRenderer = testItemObj.AddComponent<SpriteRenderer>();
            boxCollider = testItemObj.AddComponent<BoxCollider2D>();
            tableItem = testItemObj.AddComponent<TableEvidenceItem>();
            tableItem.spriteRenderer = spriteRenderer;

            // Create 32x32 test sprite
            Texture2D texture = new Texture2D(32, 32);
            testSprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 100f);
            spriteRenderer.sprite = testSprite;
        }

        [TearDown]
        public void TearDown()
        {
            if (testItemObj != null)
            {
                Object.DestroyImmediate(testItemObj);
            }
            if (caseManagerObj != null)
            {
                Object.DestroyImmediate(caseManagerObj);
            }
            if (testSprite != null && testSprite.texture != null)
            {
                Object.DestroyImmediate(testSprite.texture);
                Object.DestroyImmediate(testSprite);
            }
        }

        [Test]
        public void SetHoverState_True_UpdatesHoverFlag()
        {
            Assert.IsFalse(tableItem.IsHovered);
            tableItem.SetHoverState(true);
            Assert.IsTrue(tableItem.IsHovered);
        }

        [Test]
        public void SetHoverState_False_ClearsHoverFlag()
        {
            tableItem.SetHoverState(true);
            Assert.IsTrue(tableItem.IsHovered);
            tableItem.SetHoverState(false);
            Assert.IsFalse(tableItem.IsHovered);
        }

        [Test]
        public void AdjustColliderToSprite_AutoSizesColliderToMatchSprite()
        {
            // Initial collider size is (0, 0) or default (1, 1)
            boxCollider.size = new Vector2(0.1f, 0.1f);

            tableItem.AdjustColliderToSprite();

            Vector2 expectedSize = new Vector2(32f / 100f, 32f / 100f); // 0.32 x 0.32
            Assert.AreEqual(expectedSize.x, boxCollider.size.x, 0.001f);
            Assert.AreEqual(expectedSize.y, boxCollider.size.y, 0.001f);
            Assert.AreEqual(Vector2.zero, boxCollider.offset);
        }

        [Test]
        public void SetHoverState_SwapsHighlightedSprite_WhenProvided()
        {
            Texture2D highlightTex = new Texture2D(32, 32);
            Sprite highlightSprite = Sprite.Create(highlightTex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 100f);

            EvidenceSO evidenceSO = ScriptableObject.CreateInstance<EvidenceSO>();
            evidenceSO.id = "EVD_TEST";
            evidenceSO.normalSprite = testSprite;
            evidenceSO.highlightedSprite = highlightSprite;

            tableItem.evidenceData = evidenceSO;

            tableItem.SetHoverState(true);
            Assert.AreEqual(highlightSprite, spriteRenderer.sprite);

            tableItem.SetHoverState(false);
            Assert.AreEqual(testSprite, spriteRenderer.sprite);

            Object.DestroyImmediate(evidenceSO);
            Object.DestroyImmediate(highlightTex);
            Object.DestroyImmediate(highlightSprite);
        }

        [Test]
        public void GlowHalo_CreatedWithProperSortingOrder()
        {
            spriteRenderer.sortingOrder = 10;
            tableItem.SetHoverState(true);

            Transform haloChild = testItemObj.transform.Find("Glow_Halo");
            Assert.IsNotNull(haloChild, "Glow_Halo child should be generated when hovered");

            SpriteRenderer haloRenderer = haloChild.GetComponent<SpriteRenderer>();
            Assert.IsNotNull(haloRenderer, "Glow_Halo child must have a SpriteRenderer");
            Assert.AreEqual(9, haloRenderer.sortingOrder, "Halo sorting order should be behind the main item (order 9 vs 10)");
        }

        [Test]
        public void OpenNotebookProp_AlwaysVisibleAndInteractive()
        {
            tableItem.openNotebookOnClick = true;
            tableItem.UpdateVisibilityState();

            Assert.IsTrue(tableItem.IsItemVisible);
            Assert.IsTrue(spriteRenderer.enabled);
            Assert.IsTrue(boxCollider.enabled);
        }

        [Test]
        public void EvidenceItem_StartsHidden_WhenNotDiscovered()
        {
            EvidenceSO evidenceSO = ScriptableObject.CreateInstance<EvidenceSO>();
            evidenceSO.id = "EVD_BROKEN_TEACUP";
            evidenceSO.startsDiscovered = false;
            tableItem.evidenceData = evidenceSO;
            tableItem.evidenceId = evidenceSO.id;
            tableItem.openNotebookOnClick = false;

            tableItem.UpdateVisibilityState();

            Assert.IsFalse(tableItem.IsDiscovered);
            Assert.IsFalse(tableItem.IsItemVisible);
            Assert.IsFalse(spriteRenderer.enabled);
            Assert.IsFalse(boxCollider.enabled);

            Object.DestroyImmediate(evidenceSO);
        }

        [Test]
        public void EvidenceItem_StartsVisible_WhenStartsDiscovered()
        {
            EvidenceSO evidenceSO = ScriptableObject.CreateInstance<EvidenceSO>();
            evidenceSO.id = "EVD_FAMILY_PHOTO";
            evidenceSO.startsDiscovered = true;
            tableItem.evidenceData = evidenceSO;
            tableItem.evidenceId = evidenceSO.id;

            tableItem.UpdateVisibilityState();

            Assert.IsTrue(tableItem.IsDiscovered);
            Assert.IsTrue(tableItem.IsItemVisible);
            Assert.IsTrue(spriteRenderer.enabled);
            Assert.IsTrue(boxCollider.enabled);

            Object.DestroyImmediate(evidenceSO);
        }

        [Test]
        public void HandleEvidenceDiscovered_RevealsItemAndTriggersDiscoveryGlow()
        {
            EvidenceSO evidenceSO = ScriptableObject.CreateInstance<EvidenceSO>();
            evidenceSO.id = "EVD_BROKEN_TEACUP";
            evidenceSO.startsDiscovered = false;
            tableItem.evidenceData = evidenceSO;
            tableItem.evidenceId = evidenceSO.id;

            tableItem.UpdateVisibilityState();
            Assert.IsFalse(spriteRenderer.enabled);
            Assert.IsFalse(boxCollider.enabled);

            tableItem.HandleEvidenceDiscovered(evidenceSO);

            Assert.IsTrue(spriteRenderer.enabled);
            Assert.IsTrue(boxCollider.enabled);
            Assert.IsTrue(tableItem.IsDiscoveryCueActive);
            Assert.AreEqual(tableItem.discoveryGlowIntensity, tableItem.CurrentGlowIntensity, 0.001f);

            Transform haloChild = testItemObj.transform.Find("Glow_Halo");
            Assert.IsNotNull(haloChild);
            Assert.IsTrue(haloChild.gameObject.activeSelf);

            Object.DestroyImmediate(evidenceSO);
        }

        [Test]
        public void TriggerDiscoveryCue_And_StopDiscoveryCue_StateTransitions()
        {
            tableItem.TriggerDiscoveryCue(3.0f);
            Assert.IsTrue(tableItem.IsDiscoveryCueActive);
            Assert.Greater(tableItem.CurrentGlowIntensity, 0f);

            Transform haloChild = testItemObj.transform.Find("Glow_Halo");
            Assert.IsNotNull(haloChild);
            Assert.IsTrue(haloChild.gameObject.activeSelf);

            tableItem.StopDiscoveryCue();
            Assert.IsFalse(tableItem.IsDiscoveryCueActive);
            Assert.AreEqual(0f, tableItem.CurrentGlowIntensity, 0.001f);
            Assert.IsFalse(haloChild.gameObject.activeSelf);
        }

        [Test]
        public void CaseManager_EvidenceDiscovery_EventTriggersTableItemReveal()
        {
            caseManagerObj = new GameObject("Test_CaseManager");
            CaseManager caseManager = caseManagerObj.AddComponent<CaseManager>();

            CaseSO caseSO = ScriptableObject.CreateInstance<CaseSO>();
            EvidenceSO teacupSO = ScriptableObject.CreateInstance<EvidenceSO>();
            teacupSO.id = "EVD_BROKEN_TEACUP";
            teacupSO.evidenceName = "Broken Teacup";
            teacupSO.startsDiscovered = false;
            caseSO.evidenceItems.Add(teacupSO);

            caseManager.LoadCase(caseSO);

            tableItem.evidenceId = "EVD_BROKEN_TEACUP";
            tableItem.SubscribeToCaseManager();
            tableItem.UpdateVisibilityState();

            Assert.IsFalse(tableItem.IsDiscovered);
            Assert.IsFalse(spriteRenderer.enabled);
            Assert.IsFalse(boxCollider.enabled);

            caseManager.RegisterDiscoveredEvidence(teacupSO);

            Assert.IsTrue(tableItem.IsDiscovered);
            Assert.IsTrue(spriteRenderer.enabled);
            Assert.IsTrue(boxCollider.enabled);
            Assert.IsTrue(tableItem.IsDiscoveryCueActive);

            Object.DestroyImmediate(teacupSO);
            Object.DestroyImmediate(caseSO);
        }

        [Test]
        public void HiddenItem_IgnoresHoverAndClick()
        {
            EvidenceSO evidenceSO = ScriptableObject.CreateInstance<EvidenceSO>();
            evidenceSO.id = "EVD_SECRET";
            evidenceSO.startsDiscovered = false;
            tableItem.evidenceData = evidenceSO;
            tableItem.evidenceId = evidenceSO.id;

            tableItem.UpdateVisibilityState();
            Assert.IsFalse(spriteRenderer.enabled);

            // Attempt hover
            tableItem.SetHoverState(true);
            Assert.IsFalse(tableItem.IsHovered);

            // Attempt click
            tableItem.TriggerClick(false);
            Assert.IsFalse(tableItem.IsHovered);

            Object.DestroyImmediate(evidenceSO);
        }
    }
}
