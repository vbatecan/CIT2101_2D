using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using CaseClosed.Data;
using CaseClosed.Enums;
using CaseClosed.Prototype;
using CaseClosed.Services;

namespace CaseClosed.Tests
{
    [TestFixture]
    public class Case01DialogueAndEvidenceTests
    {
        private GameObject _holder;
        private Case01Initializer _initializer;
        private CaseSO _runtimeCase;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject("Test_Case01Initializer");
            _initializer = _holder.AddComponent<Case01Initializer>();
            _runtimeCase = _initializer.CreateCase01Data();
        }

        [TearDown]
        public void TearDown()
        {
            if (_holder != null)
            {
                Object.DestroyImmediate(_holder);
            }
        }

        #region Runtime Case Data & Dialogue Tree Tests

        [Test]
        public void Case01_RuntimeData_HasExpectedDialogueTree()
        {
            Assert.IsNotNull(_runtimeCase, "Runtime CaseSO must not be null.");
            Assert.IsNotNull(_runtimeCase.dialogueTrees, "DialogueTrees list must not be null.");
            Assert.GreaterOrEqual(_runtimeCase.dialogueTrees.Count, 1, "Must have at least 1 dialogue tree.");

            DialogueTreeSO tree = _runtimeCase.dialogueTrees[0];
            Assert.AreEqual("TREE_VINCE_01", tree.treeId);
            Assert.AreEqual("NODE_01", tree.startNodeId);
            Assert.AreEqual(15, tree.nodes.Count, "Dialogue tree must contain 15 nodes in the screenplay sequence.");

            // Node 1: Opening Alibi (Vince)
            DialogueNode node1 = tree.GetNodeById("NODE_01");
            Assert.IsNotNull(node1, "NODE_01 must exist.");
            Assert.AreEqual("NODE_01B_JANE_INTERJECTION", node1.defaultNextNodeId);
            Assert.IsFalse(node1.isChallengeable);

            // Node 1B: Witness Interjection (Jane)
            DialogueNode node1b = tree.GetNodeById("NODE_01B_JANE_INTERJECTION");
            Assert.IsNotNull(node1b);
            Assert.AreEqual("NODE_01C_VINCE_RETORT", node1b.defaultNextNodeId);

            // Node 1C: Defensive Retort (Vince)
            DialogueNode node1c = tree.GetNodeById("NODE_01C_VINCE_RETORT");
            Assert.IsNotNull(node1c);
            Assert.AreEqual("NODE_01D_DETECTIVE_PRESS", node1c.defaultNextNodeId);

            // Node 1D: Investigator Query (Detective)
            DialogueNode node1d = tree.GetNodeById("NODE_01D_DETECTIVE_PRESS");
            Assert.IsNotNull(node1d);
            Assert.AreEqual("NODE_02_ROOM_LEAD", node1d.defaultNextNodeId);

            // Node 2: Locked Room Lead (Vince)
            DialogueNode node2 = tree.GetNodeById("NODE_02_ROOM_LEAD");
            Assert.IsNotNull(node2);
            Assert.AreEqual("NODE_02B_JANE_HEARD_CRASH", node2.defaultNextNodeId);
            Assert.Contains("EVD_BROKEN_TEACUP", node2.unlockEvidenceOnComplete);

            // Node 2B: Crash Observation (Jane)
            DialogueNode node2b = tree.GetNodeById("NODE_02B_JANE_HEARD_CRASH");
            Assert.IsNotNull(node2b);
            Assert.AreEqual("NODE_02C_DETECTIVE_INSPECT", node2b.defaultNextNodeId);

            // Node 2C: Detective Desk Direction (Break for table inspection)
            DialogueNode node2c = tree.GetNodeById("NODE_02C_DETECTIVE_INSPECT");
            Assert.IsNotNull(node2c);
            Assert.IsTrue(string.IsNullOrEmpty(node2c.defaultNextNodeId), "NODE_02C_DETECTIVE_INSPECT next must be null to close dialogue for table exploration.");

            // Node 3: Teacup Lead (Vince, triggered on inspecting teacup)
            DialogueNode node3 = tree.GetNodeById("NODE_03_TEACUP_LEAD");
            Assert.IsNotNull(node3);
            Assert.AreEqual("NODE_03B_JANE_KITCHEN_LOCK", node3.defaultNextNodeId);
            Assert.Contains("EVD_KITCHEN_LOG", node3.unlockEvidenceOnComplete);

            // Node 3B: Kitchen Lock Observation (Jane)
            DialogueNode node3b = tree.GetNodeById("NODE_03B_JANE_KITCHEN_LOCK");
            Assert.IsNotNull(node3b);
            Assert.AreEqual("NODE_03C_DETECTIVE_CHALLENGE", node3b.defaultNextNodeId);

            // Node 3C: Detective Challenge Warning (Detective)
            DialogueNode node3c = tree.GetNodeById("NODE_03C_DETECTIVE_CHALLENGE");
            Assert.IsNotNull(node3c);
            Assert.AreEqual("NODE_04_FINAL_ALIBI", node3c.defaultNextNodeId);

            // Node 4: Fatal Alibi Lie (Challengeable)
            DialogueNode node4 = tree.GetNodeById("NODE_04_FINAL_ALIBI");
            Assert.IsNotNull(node4);
            Assert.IsTrue(node4.isChallengeable);
            Assert.AreEqual("RULE_VINCE_ALIBI_LIE", node4.targetContradictionRuleId);

            // Node 5: Confession (Vince)
            DialogueNode node5 = tree.GetNodeById("NODE_05_CONFESSION");
            Assert.IsNotNull(node5);
            Assert.AreEqual("NODE_05B_JANE_SHOCKED", node5.defaultNextNodeId);

            // Node 5B: Jane Shocked (Jane)
            DialogueNode node5b = tree.GetNodeById("NODE_05B_JANE_SHOCKED");
            Assert.IsNotNull(node5b);
            Assert.AreEqual("NODE_05C_VINCE_DESPERATE", node5b.defaultNextNodeId);

            // Node 5C: Vince Desperate (Vince)
            DialogueNode node5c = tree.GetNodeById("NODE_05C_VINCE_DESPERATE");
            Assert.IsNotNull(node5c);
            Assert.AreEqual("NODE_05D_DETECTIVE_CLOSE", node5c.defaultNextNodeId);

            // Node 5D: Detective Arrest (Detective)
            DialogueNode node5d = tree.GetNodeById("NODE_05D_DETECTIVE_CLOSE");
            Assert.IsNotNull(node5d);
            Assert.IsTrue(string.IsNullOrEmpty(node5d.defaultNextNodeId));
        }

        [Test]
        public void Case01_RuntimeData_EvidenceLocksAndInspectTriggers()
        {
            Assert.IsNotNull(_runtimeCase.evidenceItems);
            Assert.AreEqual(3, _runtimeCase.evidenceItems.Count, "Case 01 must have 3 evidence items.");

            EvidenceSO photo = _runtimeCase.evidenceItems.Find(e => e.id == "EVD_FAMILY_PHOTO");
            Assert.IsNotNull(photo, "EVD_FAMILY_PHOTO must exist.");
            Assert.IsFalse(photo.startsDiscovered);

            EvidenceSO teacup = _runtimeCase.evidenceItems.Find(e => e.id == "EVD_BROKEN_TEACUP");
            Assert.IsNotNull(teacup, "EVD_BROKEN_TEACUP must exist.");
            Assert.IsFalse(teacup.startsDiscovered, "Teacup must not start discovered.");
            Assert.AreEqual("NODE_02_ROOM_LEAD", teacup.requiredDialogueNodeId, "Teacup requires completing NODE_02_ROOM_LEAD.");
            Assert.AreEqual("NODE_03_TEACUP_LEAD", teacup.dialogueNodeToTriggerOnInspect, "Teacup inspect must trigger NODE_03_TEACUP_LEAD.");

            EvidenceSO kitchenLog = _runtimeCase.evidenceItems.Find(e => e.id == "EVD_KITCHEN_LOG");
            Assert.IsNotNull(kitchenLog, "EVD_KITCHEN_LOG must exist.");
            Assert.IsFalse(kitchenLog.startsDiscovered, "Kitchen Log must not start discovered.");
            Assert.AreEqual("NODE_03_TEACUP_LEAD", kitchenLog.requiredDialogueNodeId, "Kitchen Log requires completing NODE_03_TEACUP_LEAD.");
        }

        [Test]
        public void Case01_RuntimeData_ContradictionRuleResolvesCorrectly()
        {
            Assert.IsNotNull(_runtimeCase.contradictionRules);
            Assert.AreEqual(1, _runtimeCase.contradictionRules.Count);

            ContradictionRuleSO rule = _runtimeCase.contradictionRules[0];
            Assert.AreEqual("RULE_VINCE_ALIBI_LIE", rule.ruleId);
            Assert.AreEqual("NODE_04_FINAL_ALIBI", rule.targetStatementNodeId, "Contradiction rule must target NODE_04_FINAL_ALIBI.");
            Assert.AreEqual("EVD_KITCHEN_LOG", rule.requiredEvidenceId, "Contradiction rule must require EVD_KITCHEN_LOG.");
            Assert.AreEqual("NODE_05_CONFESSION", rule.unlockedDialogueNodeId, "Exposing contradiction must unlock NODE_05_CONFESSION.");

            InterrogationService service = new InterrogationService();
            ContradictionRuleSO matched = service.FindMatchingContradiction(_runtimeCase, "NODE_04_FINAL_ALIBI", "EVD_KITCHEN_LOG");
            Assert.IsNotNull(matched, "Presenting EVD_KITCHEN_LOG on NODE_04_FINAL_ALIBI must match the rule.");
            Assert.AreEqual("RULE_VINCE_ALIBI_LIE", matched.ruleId);

            ContradictionRuleSO wrongEvidence = service.FindMatchingContradiction(_runtimeCase, "NODE_04_FINAL_ALIBI", "EVD_FAMILY_PHOTO");
            Assert.IsNull(wrongEvidence, "Presenting wrong evidence must not match contradiction.");

            ContradictionRuleSO wrongNode = service.FindMatchingContradiction(_runtimeCase, "NODE_01", "EVD_KITCHEN_LOG");
            Assert.IsNull(wrongNode, "Presenting log on NODE_01 must not match contradiction.");
        }

        #endregion

        #region Asset Database Generated Assets Tests

        [Test]
        public void Case01_GeneratedDialogueAsset_HasExact15Nodes()
        {
            DialogueTreeSO treeAsset = AssetDatabase.LoadAssetAtPath<DialogueTreeSO>("Assets/Data/Case001/Dialogue_Vince01.asset");
            Assert.IsNotNull(treeAsset, "Assets/Data/Case001/Dialogue_Vince01.asset must exist.");
            Assert.AreEqual(15, treeAsset.nodes.Count, "Dialogue_Vince01 asset must contain exactly 15 screenplay nodes.");

            Assert.AreEqual("NODE_01B_JANE_INTERJECTION", treeAsset.GetNodeById("NODE_01")?.defaultNextNodeId);
            Assert.AreEqual("NODE_02_ROOM_LEAD", treeAsset.GetNodeById("NODE_01D_DETECTIVE_PRESS")?.defaultNextNodeId);
            Assert.AreEqual("NODE_02B_JANE_HEARD_CRASH", treeAsset.GetNodeById("NODE_02_ROOM_LEAD")?.defaultNextNodeId);
            Assert.Contains("EVD_BROKEN_TEACUP", treeAsset.GetNodeById("NODE_02_ROOM_LEAD")?.unlockEvidenceOnComplete);
            Assert.IsTrue(string.IsNullOrEmpty(treeAsset.GetNodeById("NODE_02C_DETECTIVE_INSPECT")?.defaultNextNodeId));

            Assert.AreEqual("NODE_03B_JANE_KITCHEN_LOCK", treeAsset.GetNodeById("NODE_03_TEACUP_LEAD")?.defaultNextNodeId);
            Assert.Contains("EVD_KITCHEN_LOG", treeAsset.GetNodeById("NODE_03_TEACUP_LEAD")?.unlockEvidenceOnComplete);
            Assert.AreEqual("NODE_04_FINAL_ALIBI", treeAsset.GetNodeById("NODE_03C_DETECTIVE_CHALLENGE")?.defaultNextNodeId);

            DialogueNode node4 = treeAsset.GetNodeById("NODE_04_FINAL_ALIBI");
            Assert.IsNotNull(node4);
            Assert.IsTrue(node4.isChallengeable);
            Assert.AreEqual("RULE_VINCE_ALIBI_LIE", node4.targetContradictionRuleId);

            DialogueNode node5 = treeAsset.GetNodeById("NODE_05_CONFESSION");
            Assert.IsNotNull(node5);
            Assert.AreEqual("NODE_05B_JANE_SHOCKED", node5.defaultNextNodeId);
            Assert.AreEqual("NODE_05D_DETECTIVE_CLOSE", treeAsset.GetNodeById("NODE_05C_VINCE_DESPERATE")?.defaultNextNodeId);
        }

        [Test]
        public void Case01_GeneratedRuleAsset_TargetsNode04AndUnlocksNode05()
        {
            ContradictionRuleSO ruleAsset = AssetDatabase.LoadAssetAtPath<ContradictionRuleSO>("Assets/Data/Case001/Rule_VinceAlibiLie.asset");
            Assert.IsNotNull(ruleAsset, "Assets/Data/Case001/Rule_VinceAlibiLie.asset must exist.");
            Assert.AreEqual("RULE_VINCE_ALIBI_LIE", ruleAsset.ruleId);
            Assert.AreEqual("NODE_04_FINAL_ALIBI", ruleAsset.targetStatementNodeId);
            Assert.AreEqual("EVD_KITCHEN_LOG", ruleAsset.requiredEvidenceId);
            Assert.AreEqual("NODE_05_CONFESSION", ruleAsset.unlockedDialogueNodeId);
        }

        [Test]
        public void Case01_GeneratedEvidenceAssets_MatchLocksAndVisibility()
        {
            EvidenceSO photo = AssetDatabase.LoadAssetAtPath<EvidenceSO>("Assets/Data/Case001/Evidence_FamilyPhoto.asset");
            Assert.IsNotNull(photo);
            Assert.IsFalse(photo.startsDiscovered, "Photo asset starts hidden per case configuration.");

            EvidenceSO teacup = AssetDatabase.LoadAssetAtPath<EvidenceSO>("Assets/Data/Case001/Evidence_BrokenTeacup.asset");
            Assert.IsNotNull(teacup);
            Assert.IsFalse(teacup.startsDiscovered, "Teacup asset must start hidden.");

            EvidenceSO log = AssetDatabase.LoadAssetAtPath<EvidenceSO>("Assets/Data/Case001/Evidence_KitchenLog.asset");
            Assert.IsNotNull(log);
            Assert.IsFalse(log.startsDiscovered, "Kitchen log asset must start hidden.");
        }

        [Test]
        public void Case01_GeneratedCaseDataAsset_HasConclusionQuestions()
        {
            CaseSO caseData = AssetDatabase.LoadAssetAtPath<CaseSO>("Assets/Data/Case001/Case01_Data.asset");
            Assert.IsNotNull(caseData, "Assets/Data/Case001/Case01_Data.asset must exist.");
            Assert.AreEqual(3, caseData.evidenceItems.Count, "Case 01 must have 3 evidence items.");
            Assert.AreEqual(3, caseData.conclusionQuestions.Count, "Case 01 generated asset has 3 conclusion questions.");
            Assert.AreEqual("Q_SUSPECT", caseData.conclusionQuestions[0].questionId);
            Assert.AreEqual("Q_MOTIVE", caseData.conclusionQuestions[1].questionId);
            Assert.AreEqual("Q_EVIDENCE", caseData.conclusionQuestions[2].questionId);
        }

        #endregion
    }
}
