using NUnit.Framework;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Gameplay;

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
    }
}
