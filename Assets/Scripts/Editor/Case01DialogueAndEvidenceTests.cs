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
        public void Case01_RuntimeData_HasExpected7NodeDialogueTree()
        {
            Assert.IsNotNull(_runtimeCase, "Runtime CaseSO must not be null.");
            Assert.IsNotNull(_runtimeCase.dialogueTrees, "DialogueTrees list must not be null.");
            Assert.GreaterOrEqual(_runtimeCase.dialogueTrees.Count, 1, "Must have at least 1 dialogue tree.");

            DialogueTreeSO tree = _runtimeCase.dialogueTrees[0];
            Assert.AreEqual("TREE_VINCE_01", tree.treeId);
            Assert.AreEqual("NODE_01", tree.startNodeId);
            Assert.AreEqual(7, tree.nodes.Count, "Dialogue tree must contain exactly 7 nodes.");

            // Node 1: Opening Alibi
            DialogueNode node1 = tree.GetNodeById("NODE_01");
            Assert.IsNotNull(node1, "NODE_01 must exist.");
            Assert.AreEqual("NODE_01B_INTERVIEW", node1.defaultNextNodeId, "NODE_01 must transition to NODE_01B_INTERVIEW.");
            Assert.IsFalse(node1.isChallengeable, "NODE_01 should not be challengeable.");

            // Node 1B: Detective Interview Prompt
            DialogueNode node1b = tree.GetNodeById("NODE_01B_INTERVIEW");
            Assert.IsNotNull(node1b, "NODE_01B_INTERVIEW must exist.");
            Assert.AreEqual("NODE_02_ROOM_LEAD", node1b.defaultNextNodeId, "NODE_01B_INTERVIEW must transition to NODE_02_ROOM_LEAD.");
            Assert.IsFalse(node1b.isChallengeable);

            // Node 2: Vince Locked Room Claim
            DialogueNode node2 = tree.GetNodeById("NODE_02_ROOM_LEAD");
            Assert.IsNotNull(node2, "NODE_02_ROOM_LEAD must exist.");
            Assert.IsTrue(string.IsNullOrEmpty(node2.defaultNextNodeId), "NODE_02_ROOM_LEAD next must be null to close dialogue for table exploration.");
            Assert.Contains("EVD_BROKEN_TEACUP", node2.unlockEvidenceOnComplete, "NODE_02_ROOM_LEAD must unlock EVD_BROKEN_TEACUP.");
            Assert.IsFalse(node2.isChallengeable);

            // Node 3: Vince Teacup Lead
            DialogueNode node3 = tree.GetNodeById("NODE_03_TEACUP_LEAD");
            Assert.IsNotNull(node3, "NODE_03_TEACUP_LEAD must exist.");
            Assert.AreEqual("NODE_03B_CONFIRMATION", node3.defaultNextNodeId, "NODE_03_TEACUP_LEAD must transition to NODE_03B_CONFIRMATION.");
            Assert.Contains("EVD_KITCHEN_LOG", node3.unlockEvidenceOnComplete, "NODE_03_TEACUP_LEAD must unlock EVD_KITCHEN_LOG.");
            Assert.IsFalse(node3.isChallengeable);

            // Node 3B: Detective Confirmation Prompt
            DialogueNode node3b = tree.GetNodeById("NODE_03B_CONFIRMATION");
            Assert.IsNotNull(node3b, "NODE_03B_CONFIRMATION must exist.");
            Assert.AreEqual("NODE_04_FINAL_ALIBI", node3b.defaultNextNodeId, "NODE_03B_CONFIRMATION must transition to NODE_04_FINAL_ALIBI.");
            Assert.IsFalse(node3b.isChallengeable);

            // Node 4: Fatal Alibi Lie (Challengeable)
            DialogueNode node4 = tree.GetNodeById("NODE_04_FINAL_ALIBI");
            Assert.IsNotNull(node4, "NODE_04_FINAL_ALIBI must exist.");
            Assert.IsTrue(node4.isChallengeable, "NODE_04_FINAL_ALIBI must be challengeable.");
            Assert.AreEqual("RULE_VINCE_ALIBI_LIE", node4.targetContradictionRuleId);
            Assert.IsTrue(string.IsNullOrEmpty(node4.defaultNextNodeId), "NODE_04_FINAL_ALIBI next should be null.");

            // Node 5: Confession
            DialogueNode node5 = tree.GetNodeById("NODE_05_CONFESSION");
            Assert.IsNotNull(node5, "NODE_05_CONFESSION must exist.");
            Assert.IsFalse(node5.isChallengeable);
            Assert.IsTrue(string.IsNullOrEmpty(node5.defaultNextNodeId));
        }

        [Test]
        public void Case01_RuntimeData_EvidenceLocksAndInspectTriggers()
        {
            Assert.IsNotNull(_runtimeCase.evidenceItems);
            Assert.AreEqual(3, _runtimeCase.evidenceItems.Count, "Case 01 must have 3 evidence items.");

            EvidenceSO photo = _runtimeCase.evidenceItems.Find(e => e.id == "EVD_FAMILY_PHOTO");
            Assert.IsNotNull(photo, "EVD_FAMILY_PHOTO must exist.");
            Assert.IsTrue(photo.startsDiscovered, "Family Photo must start discovered at case start.");
            Assert.IsTrue(string.IsNullOrEmpty(photo.requiredDialogueNodeId), "Family Photo has no dialogue prerequisite.");

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
        public void Case01_GeneratedDialogueAsset_HasExact7Nodes()
        {
            DialogueTreeSO treeAsset = AssetDatabase.LoadAssetAtPath<DialogueTreeSO>("Assets/Data/Case001/Dialogue_Vince01.asset");
            Assert.IsNotNull(treeAsset, "Assets/Data/Case001/Dialogue_Vince01.asset must exist.");
            Assert.AreEqual(7, treeAsset.nodes.Count, "Dialogue_Vince01 asset must contain exactly 7 nodes.");

            Assert.AreEqual("NODE_01B_INTERVIEW", treeAsset.GetNodeById("NODE_01")?.defaultNextNodeId);
            Assert.AreEqual("NODE_02_ROOM_LEAD", treeAsset.GetNodeById("NODE_01B_INTERVIEW")?.defaultNextNodeId);
            Assert.IsTrue(string.IsNullOrEmpty(treeAsset.GetNodeById("NODE_02_ROOM_LEAD")?.defaultNextNodeId));
            Assert.Contains("EVD_BROKEN_TEACUP", treeAsset.GetNodeById("NODE_02_ROOM_LEAD")?.unlockEvidenceOnComplete);

            Assert.AreEqual("NODE_03B_CONFIRMATION", treeAsset.GetNodeById("NODE_03_TEACUP_LEAD")?.defaultNextNodeId);
            Assert.Contains("EVD_KITCHEN_LOG", treeAsset.GetNodeById("NODE_03_TEACUP_LEAD")?.unlockEvidenceOnComplete);
            Assert.AreEqual("NODE_04_FINAL_ALIBI", treeAsset.GetNodeById("NODE_03B_CONFIRMATION")?.defaultNextNodeId);

            DialogueNode node4 = treeAsset.GetNodeById("NODE_04_FINAL_ALIBI");
            Assert.IsNotNull(node4);
            Assert.IsTrue(node4.isChallengeable);
            Assert.AreEqual("RULE_VINCE_ALIBI_LIE", node4.targetContradictionRuleId);

            DialogueNode node5 = treeAsset.GetNodeById("NODE_05_CONFESSION");
            Assert.IsNotNull(node5);
            Assert.IsFalse(node5.isChallengeable);
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
            Assert.IsTrue(photo.startsDiscovered, "Photo asset must start discovered.");

            EvidenceSO teacup = AssetDatabase.LoadAssetAtPath<EvidenceSO>("Assets/Data/Case001/Evidence_BrokenTeacup.asset");
            Assert.IsNotNull(teacup);
            Assert.IsFalse(teacup.startsDiscovered, "Teacup asset must start hidden.");
            Assert.AreEqual("NODE_02_ROOM_LEAD", teacup.requiredDialogueNodeId);
            Assert.AreEqual("NODE_03_TEACUP_LEAD", teacup.dialogueNodeToTriggerOnInspect);

            EvidenceSO log = AssetDatabase.LoadAssetAtPath<EvidenceSO>("Assets/Data/Case001/Evidence_KitchenLog.asset");
            Assert.IsNotNull(log);
            Assert.IsFalse(log.startsDiscovered, "Kitchen log asset must start hidden.");
            Assert.AreEqual("NODE_03_TEACUP_LEAD", log.requiredDialogueNodeId);
        }

        [Test]
        public void Case01_GeneratedCaseDataAsset_HasFiveConclusionQuestions()
        {
            CaseSO caseData = AssetDatabase.LoadAssetAtPath<CaseSO>("Assets/Data/Case001/Case01_Data.asset");
            Assert.IsNotNull(caseData, "Assets/Data/Case001/Case01_Data.asset must exist.");
            Assert.AreEqual(3, caseData.totalKeyEvidenceCount);
            Assert.AreEqual(1, caseData.totalContradictionsCount);
            Assert.AreEqual(5, caseData.conclusionQuestions.Count, "Case 01 must have 5 conclusion questions.");
            Assert.AreEqual("Q_SUSPECT", caseData.conclusionQuestions[0].questionId);
            Assert.AreEqual("Q_MOTIVE", caseData.conclusionQuestions[1].questionId);
            Assert.AreEqual("Q_ALIBI", caseData.conclusionQuestions[2].questionId);
            Assert.AreEqual("Q_EVIDENCE", caseData.conclusionQuestions[3].questionId);
            Assert.AreEqual("Q_WITNESS", caseData.conclusionQuestions[4].questionId);
        }

        #endregion
    }
}
